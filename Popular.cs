using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWikiBot;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;
using WikiClientLibrary.Sites;
using WikiClientLibrary.Client;
using WikiFunctions.API;

namespace botSolution
{
    public class Popular
    {
        public static string popFolder = NoUkInterwikis.PathToFolder + "pop\\";
        public static string popRuFile = popFolder + "ru-top.txt";
        public static string popIncrementalFile = popFolder + "ru-incremental.txt";
        public static string popHaveInterwikisFile = popFolder + "haveInterwikis.txt";
        public static string popFolderFull = popFolder + "Full\\";
        public static string popFolderAbsent = popFolder + "Absent\\";

        

        private int noViewsDefault = 1000;

        Dictionary<string, string> requestCache = new Dictionary<string, string>();

        public static async void GetRatioList() {

            PageList list = new PageList(MyBot.ukSiteL.Value);
            list.FillFromWatchList();

        }

        
        public static void GetPopularInRu()
        {
            //ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //hack https://stackoverflow.com/questions/2859790/the-request-was-aborted-could-not-create-ssl-tls-secure-channel/48930280#48930280

            var pop = new Popular();
            Directory.CreateDirectory(popFolderFull);
            Directory.CreateDirectory(popFolderAbsent);

            //pop.GetAllPopularInRuIncremental();
            //pop.GetPopularInRuInternal("ru",DateTime.Now.AddDays(-87));

            for (int i = -6; i <= -2; i++ )
                pop.GetPopularInRuInternal("ru", DateTime.Now.AddDays(i));

            //pop.GetPopInRuForMonth("ru", 2019, 11);
        }
        private void GetAllPopularInRuIncremental()
        {
            while (true)
            {
                GetAllPopularInRuIncremental("");
            }
        }

        private void GetAllPopularInRuIncremental(string lang)
        {
            PageList list = new PageList(MyBot.ruSiteL.Value);            

            string starter = "(:";
            
            //get start position
            starter = File.ReadAllText(popIncrementalFile,Encoding.UTF8);
            //list.FillFromFile(popIncrementalFile);
            //starter = list.pages.Last().title;
            list.Clear();

            int nOfArticles = 14000;
            list.FillFromAllPages(starter, 0, false, nOfArticles );
            
            PageList noUkInterwiki = new PageList(MyBot.ruSiteL.Value);

            noUkInterwiki = GetAbsentArticles(list, "uk");
            //SortInterwikis(list, noUkInterwiki, hasUkInterwiki);

            

            DateTime date = DateTime.Parse("11.11.2019");

            this.noViewsDefault = 0;
            GetViewsAndPublish(date, noUkInterwiki, "ab2", starter + " ++. " + nOfArticles , false);

            File.WriteAllText(popIncrementalFile, list.pages.Last().title, Encoding.UTF8); // only if everything was good
            //list.SaveTitlesToFile(popIncrementalFile); 
            
        }

        private void GetPopularInRuInternal(string lang, DateTime date)
        {
            Console.WriteLine("Get popular in date:" + date.ToShortDateString());
            
            //ServicePointManager.Expect100Continue = true;
            

            
            PageList list = new PageList();

            list = GetListOfPopular("ru", date);

            //list.FillFromFile(popFolderFull+ "01.02.2020.txt");

            var cleanList = GetAbsentArticles(list, "uk");
            list = cleanList;

            string file = GetFileNameByDate(popFolderAbsent, date);
            PageList noUkInterwiki = new PageList(MyBot.ruSiteL.Value);
            noUkInterwiki = cleanList;

            /*
            if (true)
                //!File.Exists(file))
            {
                list.SaveTitlesToFile(popRuFile);
                SavetoFileByDate(list, popFolderFull, date);

                PageList hasUkInterwiki = new PageList(MyBot.ruSiteL.Value);

                hasUkInterwiki.FillFromFile(popHaveInterwikisFile, true);

                list.pages.RemoveAll((p) => hasUkInterwiki.pages.Contains(p, new PageComparer())); //remove existing pages            

                SortInterwikis(list, noUkInterwiki, hasUkInterwiki);

                hasUkInterwiki.SaveTitlesToFile(popHaveInterwikisFile);
                SavetoFileByDate(noUkInterwiki, popFolderAbsent, date);
            }
            else
            {
                noUkInterwiki.FillFromFile(file);
            }
            */

            this.noViewsDefault = 1000;
            GetViewsAndPublish(date, noUkInterwiki, "ab", date.Day.ToString());
        }

        private void GetPopInRuForMonth(string lang, int year, int month)
        {
            PageList list = new PageList();
            

            list = GetPopularForMonth(lang, year, month);

            var noUkInterwiki = GetAbsentArticles(list, "uk");
            

            
            this.noViewsDefault = 1000;
            DateTime date = new DateTime(year, month, 28);
            GetViewsAndPublish(date , noUkInterwiki, "abm", date.Day.ToString());
            
        }

        private PageList GetPopularForMonth(string lang, int year, int month)
        {
            PageList list = new PageList();

            for (var dateofMonth = new DateTime(year, month, 1); dateofMonth.Month == month; dateofMonth = dateofMonth.AddDays(1))
            {
                var pages = GetListOfPopular(lang, dateofMonth);
                list.pages.AddRange(pages.pages);
            }
            list.RemoveRecurring();
            return list;
        }

        private PageList GetListOfPopular(string lang, DateTime date)
        {
            string url = String.Format("https://wikimedia.org/api/rest_v1/metrics/pageviews/top/{0}.wikipedia/all-access/{1}/{2, 0:D2}/{3, 0:D2}", lang, date.Year, date.Month, date.Day);
            //url = $"https://wikimedia.org/api/rest_v1/metrics/pageviews/top/ru.wikipedia/all-access/2020/03/29"; // _03/29__

            PostSubmitter ps = new PostSubmitter(url);
            ps.Type = PostSubmitter.PostTypeEnum.Get;
            string result = ps.Post();

            string json = result;
            var list = JSONParseDynamic(result);
            return list;
        }

        private void GetViewsAndPublish(DateTime date, PageList noUkInterwiki, string pageName, string titleString, bool publishHuge=true)
        {
            Page pResult = new Page(MyBot.ruSiteL.Value, "Участник:Alex Blokha/" + pageName);
            pResult.text += titleString + "\n\r";

            PageList popular = new PageList(MyBot.ruSiteL.Value);

            if (publishHuge)
            {
                pResult.text += FormatList("==\n\r H", popular);
                GetHugePopular(noUkInterwiki, popular, date);
                pResult.text += FormatList("==\n\r", popular);
                noUkInterwiki.pages.RemoveAll((p) => popular.pages.Contains(p, new PageComparer())); //remove existing pages
            }

            GetConstantPopular(noUkInterwiki, popular, date, 1000);
            pResult.text += FormatList("==1 \n\r", popular);
            noUkInterwiki.pages.RemoveAll((p) => popular.pages.Contains(p, new PageComparer())); //remove existing pages


            GetConstantPopular(noUkInterwiki, popular, date, 600);
            pResult.text += FormatList("==\n\r", popular);
            noUkInterwiki.pages.RemoveAll((p) => popular.pages.Contains(p, new PageComparer())); //remove existing pages

            GetConstantPopular(noUkInterwiki, popular, date, 400);
            pResult.text += FormatList("==4 " + "\n\r", popular);
            noUkInterwiki.pages.RemoveAll((p) => popular.pages.Contains(p, new PageComparer())); //remove existing pages

            GetConstantPopular(noUkInterwiki, popular, date, 200);
            pResult.text+= FormatList("== \n\r", popular);

            pResult.Save();
        }

        private string FormatList(string startText, PageList popular)
        {
            
            popular.pages.ForEach(page =>
            {
                startText += string.Format("* [[:ru:{0}]] \r", page.title);
            });

            return startText;
        }
        public void SavetoFileByDate(PageList pl, string path, DateTime date)
        {
            string file = GetFileNameByDate(path, date);
            pl.SaveTitlesToFile(file);
        }

        public string GetFileNameByDate(string path, DateTime date)
        {
            string file = Path.Combine(path, date.ToShortDateString() + ".txt");
            return file;

        }

        public  PageList JSONParseDynamic(string jsonText)
        {
            Site s = MyBot.ruSiteL.Value;

            dynamic o = JsonConvert.DeserializeObject(jsonText);

            var items = o["items"];
            var item = o["items"][0];

            var project = items[0]["project"];

            var articles = items[0]["articles"];

            PageList pages = new PageList();
            foreach (var a in articles)
            {
                var p = new Page((string)a.article);
                p.nOfViews = a.views;
                p.rank = a.rank;
                pages.Add(p);
                if (p.title.Contains(":")) 
                    Console.WriteLine(a.article);
            }
            pages.RemoveNamespaces(new int[] { -1, 6, 4 }); //Special, File, 
            
            return pages;
            
        }

        public static int[] GetViews(string lang, string article, DateTime date, int daysBack)
        {
            Console.WriteLine("Get Views for page:"+ article);
            string url = String.Format("https://wikimedia.org/api/rest_v1/metrics/pageviews/per-article/{0}.wikipedia/all-access/user/{1}/daily/{2}00/{3}00", lang, article, date.AddDays(-1 * daysBack).ToString("yyyyMMdd"), date.ToString("yyyyMMdd"));
            //string url = String.Format("https://wikimedia.org/api/rest_v1/metrics/pageviews/top/{0}.wikipedia/all-access/{1}/{2, 0:D2}/{3, 0:D2}", lang, date.Year, date.Month, date.Day);
            //url = $"GET https://wikimedia.org/api/rest_v1/metrics/pageviews/per-article/de.wikipedia/all-access/user/Johann_Wolfgang_von_Goethe/daily/2015101300/2015102700"; 

            PostSubmitter ps = new PostSubmitter(url);

            ps.Type = PostSubmitter.PostTypeEnum.Get;
            try
            {
                string result;
                
                result = ps.Post();

                /*
                if (!requestCache.ContainsKey(url))
                {
                    Console.WriteLine("Put in Cache");
                    result = ps.Post();
                    requestCache.Add(url, result);
                }
                else
                {
                    Console.WriteLine("Get From Cache");
                    result = requestCache[url];
                }
                */
                string jsonText = result;

                dynamic o = JsonConvert.DeserializeObject(jsonText);

                var items = (IEnumerable<dynamic>)o["items"];

                var views = from item in items
                            select (int)item.views;

                return views.ToArray();
            }
            catch (WebException wex)
            {
                if (wex.Message.Contains("404"))
                {
                    int noViewsDefault = 0;
                    Console.WriteLine("No Views for this date!");
                    var views = new int[] { noViewsDefault, noViewsDefault, noViewsDefault };
                    return views;
                }
                throw;
            }
        }

        public static int Median(int[] xs)
        {
            if (0 == xs.Length)
                return 0;
            
            var ys = xs.OrderBy(x => x).ToList();
            double mid = (ys.Count - 1) / 2.0;
            return (ys[(int)(mid)] + ys[(int)(mid + 0.5)]) / 2;
        }

        private int GetConstantPopular(PageList plRu, PageList constantPopular, DateTime date, int popCeiling, int removeCeiling=200)
        {
            Console.WriteLine("Get ConstPopular Views:" + popCeiling);
            constantPopular.Clear();
            PageList remove = new PageList();
            //foreach (Page page in plRu) 
            Parallel.ForEach(plRu.pages, (page) =>
                       {
                           var views = GetViews("ru", page.title, date.AddDays(-3), 7);
                           var median = Median(views);
                           if (median > popCeiling)
                           {
                               constantPopular.Add(page);
                           }
                           if (median < removeCeiling)
                           {
                               remove.Add(page);
                           }
                       });

            plRu.pages.RemoveAll((p) => remove.pages.Contains(p, new PageComparer())); //remove existing pages
            

            return 0;
        }

        private int GetHugePopular(PageList plRu, PageList constantPopular, DateTime date)
        {
            Console.WriteLine("GetHugePopular Views");
            foreach (Page page in plRu)
            {
                var views = GetViews("ru", page.title, date, 1);

                var hugeCount = views.Count((v) => v > 6000);

                if (hugeCount > 1)
                {
                    constantPopular.Add(page);
                }
            }

            return 0;
        }

        private static int SortInterwikis(PageList plRu, PageList noUkInterwiki, PageList hasUkInterwiki)
        {
            int counter = 0;
            
            plRu.pages.ForEach(page =>            
            {
                Console.Write("Page N" + counter + ". ");

                bool hasUkLink = false;                

                hasUkLink = HasInterwikiInWikiData(page, "uk");

                object syncObj = new object();

                if (!hasUkLink && string.IsNullOrWhiteSpace(page.text)) // make redirects as hasuklink
                {
                    page.Load();
                    //hasUkLink = page.IsRedirect(); //not skip and not insert it in the result list

                    if (page.IsRedirect())
                    {
                        page.ResolveRedirect();
                        hasUkLink = HasInterwikiInWikiData(page, "uk");
                    }

                }



                if (!hasUkLink)
                {

                    lock (syncObj)
                    {
                        noUkInterwiki.Add(page);
                    }

                }
                else
                {
                    lock (syncObj)
                    {
                        hasUkInterwiki.Add(page);
                    }
                }

                counter++;
                Console.WriteLine(counter);
            }
            ); // end of parallel.for
            return counter;
        }

        public static bool HasInterwikiInWikiData(DotNetWikiBot.Page page, string lang)
        {
            var languages = page.GetInterLanguageLinks();
            var result = languages.Where((item) => item.Contains(lang + ":"));

            bool res = result.Count() > 0 ? true : false;

            /*if (res == false)
            {
                res = WDHasInterwikiInWikiData(page, lang);
            }*/

            return res;
        }
        public PageList GetAbsentArticles(PageList pl, string lang)
        {
            //lang.IsNullOrEmpty();
            Console.WriteLine("AbsentArticles bulk requests");
            List<string> list = new List<string>();
            int nOfPagesInRequest = 49; // no more than 49

            for (int i = 0; i < pl.Count(); i += nOfPagesInRequest)
            {
                Console.WriteLine("AbsentArticles bulk request. Index: " + i);
                int pagesToRequest = (i + nOfPagesInRequest < pl.Count()) ? nOfPagesInRequest : pl.Count()-i;

                list.AddRange(GetAbsentArticlesByPortions(pl.pages.GetRange(i, pagesToRequest), MyBot.ruSiteL.Value, lang));
            }            
            list.RemoveAll((s) => string.IsNullOrWhiteSpace(s));
            PageList plRet = new PageList(pl.site, list.ToArray());

            return plRet;
        }

        public List<string> GetAbsentArticlesByPortions(List<Page> pl, Site site, string lang)
        {
            //var site = pl.site;                        
            //pl.RemoveRange(stayCount, pl.Count - stayCount);

            //string title = "1977 год|1988 год|1999 год";
            string title = "";
            foreach(var page in pl)
            {
                
                string addTitle = string.Format("{0}|", page.title);
                if (Bot.UrlEncode(title+addTitle ).Length > 7880) // 7800 - be sure our title will not overrun the max lenth of query
                    break;
                title += addTitle;
            }
            
            string url = site.apiPath +
                "?format=xml&action=query&prop=langlinks&lllang="+lang+"&lllimit=500&titles=" +
                Bot.UrlEncode(title); //max lenght - 7990 // 104 length - without title

            string src = "";

            try
            {
                src = site.GetWebPage(url);
            }
            catch (Exception ex)
            {
                throw;
            }

            if(src.Contains("<continue"))
            {
                throw new Exception("<continue");
            } // if we see continue we need to know, because the response was not full

            var xdoc = XDocument.Parse(src);
            //xdoc.Descendants("page").Descendants("langlinks");
            var langLinks = (
                from page in xdoc.Descendants("page")
                where page.Descendants("langlinks").Count()==0
                select page.Attribute("title").Value
                //select link.Parent.Parent.Attribute("title").Value
            ).ToList();
            return langLinks;
        }
        /*
        public static bool WDHasInterwikiInWikiData(DotNetWikiBot.Page page, string lang) // does not work!!!
        {
            // does not work!!!

            var localSite = new DotNetDataBot.Site("https://www.wikidata.org", "Alex_Blokha", "predator32");

            DotNetDataBot.Site WD = WDS.Value;

            Dictionary<string, string> site_link = new Dictionary<string, string>();
            DotNetDataBot.Item emptyItem = new DotNetDataBot.Item(WD);

            DotNetDataBot.Item item = null;

            try
            {
                item = new DotNetDataBot.Item(WD, "Q" +
                    emptyItem.GetIdBySitelink(page.site.language, page.title));

                Retry.Do(() =>
                {
                    item = new DotNetDataBot.Item(WD, "Q" +
            emptyItem.GetIdBySitelink(page.site.language, page.title));
                    item.Load();
                }
                        , new TimeSpan(0));

            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine("page has errors on wikidata:" + page.title + ex.ToString());
                return false;
            }
            site_link = item.links;
            bool hasIwiki = site_link.ContainsKey(lang + "wiki");
            if (hasIwiki)
            {
                Console.WriteLine("Page has uk iwiki:" + page.title);
            }
            return hasIwiki;
        }
        */
    }
}
