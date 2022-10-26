using System;
using System.Collections.Generic;
using System.Linq;
using DotNetWikiBot;
using System.IO;
using System.Diagnostics;


namespace botSolution
{
    public class NoUkInterwikisOther : NoUkInterwikis
    {
        public static void GetUkrMusicSubCategoriesInUk()
        {
            PageList plUkr = new PageList(ukSite);

            List<string> include = new List<string>();
            include.Add("област");
            include.Add("Крим");
            include.Add("Україн");
            include.Add("муз");
            include.Add("Муз");


            plUkr.FillAllFromCategoryTree("Українська музика", include.ToArray());
            plUkr.RemoveNamespaces(new int[] { 14 });

            MyBot.ClearAllNamespaces(plUkr, MyBot.ukSiteL.Value);


            plUkr.SaveTitlesToFile(allUkWikiAboutUkrMusic);
        }

        public static void GetUkraineSubCategoriesInUk()
        {

            PageList plUkr = new PageList(ukSite);            

            List<string> include = new List<string>();
            include.Add("област");
            include.Add("Крим");
            include.Add("Україн");


            plUkr.FillAllFromCategoryTree("Україна", include.ToArray());
            plUkr.RemoveNamespaces(new int[] { 14 });

            MyBot.ClearAllNamespaces(plUkr, MyBot.ukSiteL.Value);


            plUkr.SaveTitlesToFile(allUkWikiAboutUkraine);
        }

        private static void GetStatsForList(List<string> links, string fileNamePart)
        {
            var watch = Stopwatch.StartNew();
            PageList noUkInterwikiOld = new PageList(MyBot.ruSiteL.Value, links);

            //noUkInterwikiOld.FillFromFile(PathToFolder + "absentStatsTop20.txt.txt", true);

            int counter = 0;

            var statsArr = new[] 
            { 
            new { PageName = "Empty", stats = 0, size = 0 } 
            }.ToList();

            File.Delete(fileNamePart + ".txt");

            foreach (Page page in noUkInterwikiOld)
            {
                Console.Write("Page N" + counter + ". ");

                object syncObj = new object();

                int pageStats = 0;
                Retry.Do(() =>
                    pageStats = GetStats(page.title.Replace("ru:", ""), "ru"),
                    TimeSpan.FromSeconds(1), 6);



                if (string.IsNullOrWhiteSpace(page.text))
                {
                    Retry.Do(() =>
                    page.LoadTextOnly(),
                    TimeSpan.FromSeconds(1), 3);
                }
                lock (syncObj)
                {


                    statsArr.Add(new { PageName = page.title, stats = pageStats, size = page.text.Length });

                    File.AppendAllText(fileNamePart + ".txt", string.Format("{0} - {1} - за день - {2} - оцінка в укрвікі {3} \n\r",
                                page.title, pageStats, pageStats / 30, pageStats / 5));
                }



                counter++;
            }
            //); // end of parallel.for

            statsArr = statsArr.OrderByDescending(x => x.stats).ToList();

            var result = from statsArrSel
                    in statsArr
                         orderby statsArrSel.stats descending
                         select string.Concat(
                         string.Format("[[:ru:{0}]] ; {1}; - за день - {2}; розмір - {3}  \n\r",
                     statsArrSel.PageName, statsArrSel.stats,
                     statsArrSel.stats / 30, statsArrSel.size))
                         ;

            File.Delete(fileNamePart + "Sorted" + ".txt");
            File.AppendAllLines(fileNamePart + "Sorted" + ".txt", result);

            result = from statsArrSel
                    in statsArr
                     orderby statsArrSel.size ascending, statsArrSel.stats descending
                     select string.Concat(
                     string.Format("{0} ; {1}; - за день - {2}; розмір - {3}  \n\r",
                 statsArrSel.PageName, statsArrSel.stats,
                 statsArrSel.stats / 30, statsArrSel.size))
                         ;
            File.Delete(fileNamePart + "SortedBySize" + ".txt");

            File.AppendAllLines(fileNamePart + "SortedBySize" + ".txt", result);

            File.AppendAllText(fileNamePart + "SortedBySize" + ".txt", "Time Taken:" + watch.Elapsed.ToString());
        }

        private static void GetStatsForList()
        {
            Page absent = new Page();
            absent.text = File.ReadAllText(PathToFolder + "absentStatsTop20.txt");

            var links = absent.GetAllLinks();

            GetStatsForList(links, PathToFolder + "MyAbsentStats");
        }

        public static void GetStatsNoUkGeneratedList()
        {
            Page absent = new Page();
            absent.text = File.ReadAllText(PathToFolder + "NoUkGeneratedSource.txt");

            var links = absent.GetAllLinks();

            links = links.Where((pageName) => pageName.Contains("ru:")).ToList();

            GetStatsForList(links, PathToFolder + "NoUkGeneratedStats");
        }
    
    }
}
