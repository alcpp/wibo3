using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWikiBot;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;

namespace botSolution
{
    class NeVystachaye
    {
        private static int maxPagesToModify = 12;

        public static void NeVystachayeFunc()
        {
            //Page.CacheEnabled = false;

            var site = MyBot.ukSiteL.Value;


            //Вікіпедія:Статті, які повинні бути в усіх вікіпедіях/stat
            Page p = new Page(site, "Вікіпедія:Статті, які повинні бути в усіх вікіпедіях/stat");

            p.Load();

            string[] arr = new string[] { "|-" };
            var rows = p.text.Split(new string[] { "|-" },
                StringSplitOptions.RemoveEmptyEntries);

            int modifiedCounter = 0;

            bool processNewOnly = false;

            for (int i = 0; i < rows.Count(); i++)
            {
                if (i <= 1) //first row
                    continue;

                if (modifiedCounter >= maxPagesToModify)
                    break;

                var columns = rows[i].Split(new string[] { "||" },
                    StringSplitOptions.RemoveEmptyEntries);

                string pageName = columns[1];
                
                pageName = GetPageNameWithoutBrackets(pageName);

                string nOfSymbols = columns[2];

                string nOfSymbolsNormal = columns[3];

                nOfSymbols = ClearCell(nOfSymbols);
                nOfSymbolsNormal = ClearCell(nOfSymbolsNormal);

                double intNOfSymbolsNormal = double.Parse(nOfSymbolsNormal, CultureInfo.InvariantCulture.NumberFormat);
                intNOfSymbolsNormal *= 1000;

                int lowBound = 24000;

                if (intNOfSymbolsNormal >= lowBound && intNOfSymbolsNormal < 30000)
                {

                    int neVystachayeInt = GetInsufficientCharactersCount(site, pageName);

                    if (neVystachayeInt >= 30000 - lowBound && neVystachayeInt <= 0)
                        continue;                    

                    Page articleDisc = new Page(site, "Обговорення:" + pageName);
                    articleDisc.Load();

                    if (string.IsNullOrEmpty(articleDisc.text))
                        throw new Exception("IsNullOrEmpty");

                    string templText = string.Format
                       (@"Не_вистачає|{0}|{1}|{2}|великою|{3} ",
                       pageName, neVystachayeInt, (neVystachayeInt * 1.7),
                       DateTime.Now.ToShortDateString());
                    templText = "{{" + templText + "}}";
                    templText += "\n\n";

                    if (articleDisc.text.Contains("{{Не_вистачає")
                        || articleDisc.text.Contains("{{Не вистачає")
                        )
                    {
                        if (processNewOnly)
                            continue;

                        var prevSizeList = articleDisc.GetTemplateParameter("Не_вистачає", "2");
                        if (prevSizeList.Count>0 && prevSizeList[0] == neVystachayeInt.ToString())
                            continue;

                        string prevtext = articleDisc.text;
                        articleDisc.SetTemplateParameter("Не_вистачає", "1", pageName, true);
                        articleDisc.SetTemplateParameter("Не_вистачає", "2", neVystachayeInt.ToString(),true);
                        articleDisc.SetTemplateParameter("Не_вистачає", "3", (neVystachayeInt * 1.7).ToString(), true);
                        articleDisc.SetTemplateParameter("Не_вистачає", "4", "великою", true);
                        articleDisc.SetTemplateParameter("Не_вистачає", "5", DateTime.Now.ToShortDateString(), true);

                        if(prevtext== articleDisc.text)
                            continue;
                    }
                    else
                    {
                        articleDisc.text = templText + articleDisc.text;                        
                    }

                    string descr = GetPageComment(neVystachayeInt);

                    try
                    {
                        articleDisc.Save(descr, false);
                        articleDisc.Watch();
                        //articleDisc.Save()
                    }
                    catch (WikiBotException ex)
                    {
                        if (!ex.Message.Contains("Ne Vystachaye exception"))
                            throw;
                    }

                    if (modifiedCounter > 0)
                    {
                        Console.WriteLine("...Waiting some time..."); // why? // because we don't want to look like bots :)
                        Thread.Sleep(1000 * 60);
                    }
                    modifiedCounter++;
                    
                }

            }


        }

        private static string GetPageComment(int neVystachayeInt)
        {
            string descr = string.Format
                ("/*Не вистачає {0} символів, до великої, в списку 1000 найнеобхідніших*/",
                neVystachayeInt);
            return descr;
        }

        private static int GetInsufficientCharactersCount(Site site, string pageName)
        {
            Page article = new Page(site, pageName);
            article.Load();

            string clearedText = ClearArticle(article);

            double nOfSymbolsNormArticle = clearedText.Length;
            nOfSymbolsNormArticle *= 1.3;





            double neVystachayeNormal = (int)(30000 - nOfSymbolsNormArticle);

            double neVystachaye = neVystachayeNormal + 100; //cyrillic
            //neVystachaye /= 1.3; // Normalyzing

            int neVystachayeInt = (int)neVystachaye;
            return neVystachayeInt;
        }

        private static string ClearArticle(Page article)
        {
            
            string commentTest = @"<!-- 
                al
                    lo--><!--
comment
--><!--comment22 -->";
            //article.text = comment + article.text + comment;

            Regex r = new Regex("<!(--.*?--)?>", RegexOptions.Singleline); // RegexOptions.Singleline means it will not stop on \n
            string clearedText = r.Replace(article.text, "");
            return clearedText;
        }

        private static string GetPageNameWithoutBrackets(string pageName)
        {
            pageName = pageName.Replace("[", "");
            pageName = pageName.Replace("]", "");
            pageName = pageName.Trim();
            return pageName;
        }

        private static string ClearCell(string cell)
        {
            var arr = cell.Split(new string[] { "|" },
                StringSplitOptions.RemoveEmptyEntries);

            string cellRet = "";

            if (arr.Count() > 1)
                cellRet = arr[1];
            else
                cellRet = arr[0];

            return cellRet;
        }
    }
}
