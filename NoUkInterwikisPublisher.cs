using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWikiBot;
using System.IO;
using System.Net;
using System.Threading;

namespace botSolution
{
    public class NoUkInterwikisPublisher
    {
        public static Site theySite;

        public static Site ukSite = MyBot.ukSiteL.Value;

        public NoUkInterwikis NoUkInterw;

        public string mainPageNamePart = @"Вікіпедія:Статті рувікі про Україну без українських інтервік/";
        public string templateName = @"Шаблон:Статті рувікі про Україну без українських інтервік|{0}";

        static bool writeToWeb = true;

        string bezSilradPath ;
        string bezSilradTranslatedPath ;
        string vsiPath;

        public  NoUkInterwikisPublisher(NoUkInterwikis nui)
        {
            this.NoUkInterw = nui;
            theySite = nui.theySite;
            DebugFolder = NoUkInterw.GetSrcLangFolder() + "Debug\\";
            Directory.CreateDirectory(DebugFolder);          
            

            bezSilradPath = DebugFolder + "bezSilrad.txt";
            bezSilradTranslatedPath = DebugFolder + "bezSilradTranslated.txt";
            vsiPath = DebugFolder + "bezSilradTranslated.txt";

            mainPageNamePart = @"Вікіпедія:Статті рувікі про Україну без українських інтервік/";
            templateName = @"Шаблон:Статті рувікі про Україну без українських інтервік|{0}";
        }

        public static NoUkInterwikisPublisher CreateFromRuPublisher(NoUkInterwikis nui)
        {
            var publisher = new NoUkInterwikisPublisher(nui);

            publisher.mainPageNamePart = @"Вікіпедія:Статті рувікі про Україну без українських інтервік/";
            publisher.templateName = @"Шаблон:Статті рувікі про Україну без українських інтервік|{0}";

            return publisher;
        }

        public static  NoUkInterwikisPublisher CreateFromPlPublisher(NoUkInterwikis nui)
        {
            var publisher = new NoUkInterwikisPublisher(nui);

            publisher.mainPageNamePart = @"Вікіпедія:Статті полвікі про Україну без українських інтервік/";
            publisher.templateName = @"Шаблон:Статті полвікі про Україну без українських інтервік|{0}";

            return publisher;
        }

        private string DebugFolder;

        public static string GetCommentForPage(int pageCount)
        {
            return string.Format("/*Не вистачає сторінок: {0}*/", pageCount);
        }

        public static PageList FilterSilRady(PageList pl)
        {
            Console.WriteLine("Filtering Silrady, miskrady & else");
            //pl.pages.RemoveAll((p) => { return p.title.ToLower().Contains("Лобачевский сельский совет")? true: false; });

            string[] excludedList = { "сельский совет", "поселковый совет", "городской совет" };

            /*var list = from page in pl.pages
                        //where !page.title.ToLower().Contains("сельский совет")
                        //&& 
                        
                       where !excludedList.Contains(page.title.ToLower())
                        select page.title;

             */

            var list = from page in pl.pages
                       where !excludedList.Any(val => page.title.Contains(val))
                       select page.title;

            PageList filtered = new PageList(pl.site, list.ToArray());

            return filtered;

        }
        

        public virtual void PrepareAndPublish(int splitNumber)
        {
            //writeToWeb = false;

            PageList plRu = new PageList(theySite);
            plRu.FillFromFile(NoUkInterw.absentTheyWikiAboutUkraine);
            plRu = FilterAndTranslate(plRu);
            //

            PageList plUkrTranslated = new PageList(ukSite);
            plUkrTranslated.FillFromFile(bezSilradTranslatedPath);

            if (plRu.Count() != plUkrTranslated.Count())
                throw new Exception("wrong translation!");

            StopwatchExtensions.BenchMark(() => SplitAndPublish(plRu, null, splitNumber), "split");

            AddDynamics(plRu);
            
            AddVsi(plRu);

            PublishCategories(plRu);            
            
        }

        public virtual void PublishCategories(PageList plRu)
        {
            Console.WriteLine("PublishCategories");
            Console.Beep();
            Console.Beep();
            //throw new Exception("abstract method PublishCategories");
        }

        private  PageList FilterAndTranslate(PageList plRu)
        {


            if (plRu.Count() < 3)
            {
                throw new Exception("It seems error happened. Too less of pages.");
            }

            PageList filteredList = null;
            StopwatchExtensions.BenchMark(() => filteredList = FilterSilRady(plRu), "FilterSilRady");

            filteredList.SaveTitlesToFile(bezSilradPath);
            plRu = filteredList;

            string toTranslate = File.ReadAllText(bezSilradPath);

            /*
              StopwatchExtensions.BenchMark(() =>
                  {
                      string translated = AlexTranslator.TranslationTools.WikiGoogleTranslate(toTranslate, "ru", "uk", false);
                      Console.WriteLine("List was translated");
                      File.WriteAllText(bezSilradTranslatedPath, translated);
                  }, "translation");
              */
            File.WriteAllText(bezSilradTranslatedPath, toTranslate);

            return plRu;
        }

        private void SplitAndPublish(PageList plRu, PageList plUkrTranslated, int splitNumber)
        {
            

            for (int j = 0; j <= plRu.Count() / splitNumber; j++)
            {
                var ru = (from c in plRu.pages
                          select c.title).Skip(j * splitNumber).Take(splitNumber);

                List<string> mergedList;
                string lang = theySite.language;

                if (plUkrTranslated != null)
                {
                    var uk = (from c in plUkrTranslated.pages
                              select c.title).Skip(j * splitNumber).Take(splitNumber);

                    

                    var merged = ru.Zip(uk, (rus, ukr) => string.Format("* [[:{2}:{0}]]  -  [[{1}]]", rus, ukr,lang));


                    mergedList = merged.ToList();
                }
                else
                {
                    var merged = ru.Zip(ru, (rus, ukr) => string.Format("* [[:{2}:{0}]]", rus, ukr, lang));
                    

                    mergedList = merged.ToList();
                }


                int step = 5;
                /*for (int j = mergedList.Count - step; j >= 0; j -= step)
                {
                    mergedList.Insert(j, "==" + ((j / step) + 1) + "==");
                }*/

                mergedList = mergedList.SelectMany((s, i) => ((i + 1) % step == 0 && i != mergedList.Count - 1) ? new[] { s, "==" + ((i / step) + 1) + "==" } : new[] { s }).ToList();

                string result = string.Join("\r\n", mergedList);
                result = "\r\n" + result;


                result = AddHeader(plRu, result, true);

                string fileName = DebugFolder + (j) + ".txt";
                File.WriteAllText(fileName, result);

                Page p = new Page(ukSite, mainPageNamePart + j);
                if (writeToWeb)
                    p.Save(result, GetCommentForPage(plRu.Count()), false);
            }
        }

        private void AddVsi(PageList plRu)
        {
            Page.CacheEnabled = false;

            var vsi = (from v in plRu.pages select v.title);
            string concat = string.Join("\n*", vsi);
            Page pVsi = new Page(ukSite, mainPageNamePart + "Всі");
            concat = AddHeader(plRu, concat, false);
            if (writeToWeb)
                pVsi.Save(concat, GetCommentForPage(plRu.Count()), false);
            string fileName = NoUkInterw.allPagesListVsi;
            File.WriteAllText(fileName, concat, Encoding.UTF8);

        }

        protected void AddFromCategory(PageList plRu, string ruCategoryName, string ukDisplayName, bool excludeNotExistingVillages = false)
        {

            PageList category = new PageList(plRu.site);

            category.FillAllFromCategoryTreeUkrainian(ruCategoryName, excludeNotExistingVillages);

            try
            {
                category.FillAbsentFromCategoryTree(ruCategoryName, "uk"); //works bad // THE second option
            }
            catch (WebException ex)
            {
                if (!ex.Message.Contains("403"))
                    throw;

            }

            string lang = theySite.language;
            var intersect = from ru in plRu.pages
                            join selCat in category.pages on ru.title equals selCat.title //into res
                            select string.Format("\n* [[:{1}:{0}]]", selCat.title, lang ); //we don't need wikilinks in format, because its russian links




            string concat = "Загалом:" + intersect.Count() + "\n";
            if (intersect.Count() > 0)
            {
                concat += intersect.Aggregate((a, b) => a + b);
            }


            Page pCategory = new Page(ukSite, mainPageNamePart + ukDisplayName);
            concat = AddHeader(plRu, concat, false);


            if (writeToWeb)
                pCategory.Save(concat, GetCommentForPage(intersect.Count()), false);
            string fileName = DebugFolder + ukDisplayName + ".txt";
            File.WriteAllText(fileName, concat);
        }

        public void AddDynamics(PageList plRu, bool isMissedPagesDynamics = true)
        {
            Page.CacheEnabled = false;

            Page pDynamics = new Page(ukSite, mainPageNamePart + "Динаміка");
            pDynamics.Load();
            if (string.IsNullOrEmpty(pDynamics.text))
            {
                pDynamics.text = AddHeader(plRu, "", false);
            }

            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            if (isMissedPagesDynamics)
            {
                pDynamics.text += "\n\nКількість відсутніх сторінок:" + plRu.Count() + ".  -- " + DateTime.Now.ToShortDateString();
            }
            else
			{
                pDynamics.text += "\n\nWhole Index: " + plRu.Count() + " articles. -- " + DateTime.Now.ToShortDateString();
            }

            if (writeToWeb)
            {
                string comment = GetCommentForPage(plRu.Count());
                if (!isMissedPagesDynamics)
				{
                    comment = "Whole index:" + plRu.Count();
                }

                pDynamics.Save(comment, false);
            }

            string fileName = DebugFolder + ("pDynamics") + ".txt";
            File.WriteAllText(fileName, pDynamics.text);
        }

        private string AddHeader(PageList plRu, string result, bool addTranslationRow)
        {
            if (addTranslationRow)
            {
                result = "*Стаття в рувікі  -   Приблизний переклад\r\n\n" + result; // no translation, so we will not change it
            }
            string template = string.Format(templateName, plRu.pages.Count);
            template = "{{" + template + "}}" + "\n\n";
            result = template + result;
            return result;
        }
    }

}
