using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWikiBot;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Diagnostics;
using System.Net;

using System.Web;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


namespace botSolution
{
    public class NoUkInterwikis
    {
        public Site theySite = null;
        
        public static Site ukSite = MyBot.ukSiteL.Value;

        protected  bool updateFromPrevious;

        public static string PathToFolder = @"c:\ukraine-iwi\";

        //uk-uk
        public static string allUkWikiAboutUkraine = PathToFolder + "allUkWikiAboutUkraine.txt";
        public static string allUkWikiAboutUkrMusic = PathToFolder + "allUkWikiAboutUkrMusic.txt";


        public virtual string GetSrcLangFolder()
        {
            return PathToFolder + @"ru-source\";
        }

        protected string allTheyWikiAboutUkraine;
        public string absentTheyWikiAboutUkraine;
        public string allPagesListVsi = PathToFolder + "vsi.txt";

        private static string hasUkInterwikiFile = PathToFolder + "hasUkInterwiki.txt";
        public  static string finalBezPerson = PathToFolder + "finalBezPerson.txt";
        public static string finalBezPersonTranslated = PathToFolder + "finalBezPersonTranslated.txt";
        public static string finalZPersonamy = PathToFolder + "finalZPersonamy.txt";
        
        //stats
        private static string absentStats = PathToFolder + "absentStats.txt";
        private static string absentStatsSorted = PathToFolder + "absentStatsSorted.txt";
        private static string absentStatsSortedBySize = PathToFolder + "absentStatsSortedBySize.txt";

        

        public static int GetStats(string pageName, string lang)
        {
            //post is not allowed

            string _requestUrl =
                //"http://stats.grok.se/json/uk/201307/%D0%A3%D0%BA%D1%80%D0%B0%D1%97%D0%BD%D0%B0";
                //string.Format("http://stats.grok.se/json/{0}/latest/",lang)
                string.Format("http://stats.grok.se/json/{0}/201504/", lang)
                + HttpUtility.UrlPathEncode(pageName);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_requestUrl);
            request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; ru; rv:1.9.1.7) Gecko/20091221 Firefox/3.5.7"; // the point!

            WebResponse response =null ;

            response = request.GetResponse();

            /*Retry.Do(() =>
                        response = request.GetResponse(),
                        TimeSpan.FromSeconds(1), 4);*/
            
            StreamReader reader = new StreamReader(response.GetResponseStream()
            );
            string json = reader.ReadToEnd();

            JObject o = JObject.Parse(json);
            var viewes = o["daily_views"];
            var viewsArray = o["daily_views"].Select(x => x.Last());
            int[] intViewsArray = viewsArray.Select(x => int.Parse(x.ToString())).ToArray();
            int sum = intViewsArray.Sum();
            Console.WriteLine(sum);
            return sum;
            
        }
        
        public static void NoUkInterwikiFullCycle()
        {
            Page.CacheEnabled = false;


            var noukInterw = new NoUkInterwikisInRu();
            noukInterw.updateFromPrevious = true;

          
          //Page.CacheEnabled = false;
            if (!noukInterw. updateFromPrevious)
          {
              noukInterw. GetUkraineSubCategoriesInTheyWiki();
          }

          if (noukInterw.updateFromPrevious)
          {
              noukInterw.UpdateFromPreviousRun();
          }
          

          PostSubmitter ps = new PostSubmitter();
            

          //NoUkInterwikisPublisher.PrepareAndPublish();
          //return;

          if (!noukInterw.updateFromPrevious)
          {
              //FilterPersonalii(); //obsolete, now we can do it using wikidata
          }

          //GetRussianArticlesWithoutUkrainian();

          NoUkInterwikisPublisher publisher = new NoUkInterwikisPublisherFromRu(noukInterw);

          publisher.PrepareAndPublish(600);
        }

        public static void NoUkInterwikiFullCyclePl()
        {
            Page.CacheEnabled = false;

            var noukInterw = new NoUkInterwikisInPl();
            noukInterw.updateFromPrevious = false;

            if (!noukInterw.updateFromPrevious)
            {
                //noukInterw.GetUkraineSubCategoriesInTheyWiki();
            }

            NoUkInterwikisPublisher publisher = new NoUkInterwikisPublisherFromPl(noukInterw);

            publisher.PrepareAndPublish(600);
        }

        public static void NoUkInterwikiFullCycleFr()
        {
            Page.CacheEnabled = false;

            var noukInterw = new NoUkInterwikisInFr();
            noukInterw.updateFromPrevious = false;

            if (!noukInterw.updateFromPrevious)
            {
                noukInterw.GetUkraineSubCategoriesInTheyWiki();
            }

            if (noukInterw.updateFromPrevious)
            {
                //noukInterw.UpdateFromPreviousRun();
            }

            NoUkInterwikisPublisher publisher = new NoUkInterwikisPublisherFromFr(noukInterw);

            publisher.PrepareAndPublish(600);
        }

        public static void NoUkInterwikiFullCyclePt()
        {
            Page.CacheEnabled = false;

            var noukInterw = new NoUkInterwikisInPt();
            noukInterw.updateFromPrevious = true;
            NoUkInterwikisPublisher publisher = new NoUkInterwikisPublisherFromPt(noukInterw);

            if (!noukInterw.updateFromPrevious)
            {
                var list = noukInterw.GetUkraineSubCategoriesInTheyWiki();
                publisher.AddDynamics(list, false);
            }

            if (noukInterw.updateFromPrevious)
            {
                //noukInterw.UpdateFromPreviousRun();
            }
            publisher.PrepareAndPublish(600);
        }

        public static void NoUkInterwikiFullCycleEs()
        {
            Page.CacheEnabled = false;

            var noukInterw = new NoUkInterwikisInEs();
            noukInterw.updateFromPrevious = false;
            NoUkInterwikisPublisher publisher = new NoUkInterwikisPublisherFromEs(noukInterw);

            if (!noukInterw.updateFromPrevious)
            {
                var list = noukInterw.GetUkraineSubCategoriesInTheyWiki();
                publisher.AddDynamics(list, false);
            }

            if (noukInterw.updateFromPrevious)
            {
                //noukInterw.UpdateFromPreviousRun();
            }
            publisher.PrepareAndPublish(600);
        }


        public static void NoUkInterwikiFullCycleDe()
        {
            Page.CacheEnabled = false;

            var noukInterw = new NoUkInterwikisInDe();
            noukInterw.updateFromPrevious = true;
            NoUkInterwikisPublisher publisher = new NoUkInterwikisPublisherFromDe(noukInterw);

            if (!noukInterw.updateFromPrevious)
            {
                var list = noukInterw.GetUkraineSubCategoriesInTheyWiki();
                publisher.AddDynamics(list, false);
            }

            if (noukInterw.updateFromPrevious)
            {
                //noukInterw.UpdateFromPreviousRun();
            }
            publisher.PrepareAndPublish(900);
        }

        public static void NoUkInterwikiFullCycleEn()
        {
            Page.CacheEnabled = false;

            var noukInterw = new NoUkInterwikisInEn();
            noukInterw.updateFromPrevious = false;


            //Page.CacheEnabled = false;
            if (!noukInterw.updateFromPrevious)
            {
                //noukInterw.GetUkraineSubCategoriesInTheyWiki();
            }

            if (noukInterw.updateFromPrevious)
            {
                //noukInterw.UpdateFromPreviousRun();
            }

            NoUkInterwikisPublisher publisher = new NoUkInterwikisPublisherFromEn(noukInterw);

            publisher.PrepareAndPublish(1000);
        }

        public static string PostFile()
        {
            WebRequest wr = WebRequest.Create(@"https://tools.wmflabs.org/paste/api/create");
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] bData = encoding.GetBytes("My big STR");
            wr.Method = "POST";
            wr.ContentType = "application/x-www-form-urlencoded";
            wr.ContentLength = bData.Length;
            Stream sMyStream = wr.GetRequestStream();
            sMyStream.Write(bData, 0, bData.Length);
            sMyStream.Close();
            return "";
        }
        
        public static string GetTranslationByPost(string text, string langFrom, string LangTo)
        {
            PostSubmitter post = new PostSubmitter();
            //post.Url = "https://translate.googleusercontent.com/translate_f/t";
            post.Url = "https://translate.googleusercontent.com/translate_f";
            post.PostItems.Add("client", "v2");
            post.PostItems.Add("text", HttpUtility.HtmlEncode(text));            
            post.PostItems.Add("sl", langFrom);
            post.PostItems.Add("tl", LangTo);
            post.Type = PostSubmitter.PostTypeEnum.Post;


            string json = post.Post();

            //string result = GetTranslationFromJson(json); //no json comes fo far

            //string result = FixDoubleConversion(json);

            return "";
        }
        

        protected virtual PageList GetUkraineSubCategoriesInTheyWiki()
        {
            Page.CacheEnabled = true;
            Directory.CreateDirectory(PathToFolder);

            PageList plUkr = new PageList(theySite);
            

            StringCollection coll = new StringCollection();
            

            List<string> include = new List<string>();
            include.Add("област");
            include.Add("Крым");
            include.Add("Украин");


            plUkr.FillAllFromCategoryTree("Украина", include.ToArray());
            plUkr.RemoveNamespaces(new int[] { 14 }); //category           
            

            //MyBot.ClearAllNamespaces(plUkr, MyBot.ruSite); //why?

            plUkr.SaveTitlesToFile(allTheyWikiAboutUkraine);
            plUkr.SaveTitlesToFile(allTheyWikiAboutUkraine+".bk");

            return plUkr;
        }

      protected virtual void UpdateFromPreviousRun()
      {
          var vsi = File.ReadAllLines(this.allPagesListVsi, Encoding.UTF8);
          //string vsiPageStr = NoUkInterwikisPublisher.mainPageNamePart + "Всі";
          //Page vsiPage = new Page(ukSite,vsiPageStr);
          //var vsi = vsiPage.text;
    

        var vsiList = vsi.ToList();
        vsiList.RemoveAll((x) => x == "");

        //vsiList.RemoveRange(2,3290);

        PageList pl = new PageList(ukSite, vsiList.ToArray());

        pl.pages.RemoveAt(0); // template

        foreach (var page in pl.pages)
        {
          if (page.title.StartsWith("*")) //removing *
            page.title = page.title.Remove(0,1);
        }

        

        pl.SaveTitlesToFile(allTheyWikiAboutUkraine);
      }
      
      public static bool HasInterwikiInWikiData(DotNetWikiBot.Page page, string lang)
      {

          var languages = page.GetInterLanguageLinks();

          var result = languages.Where( (item) => item.Contains(lang + ":"));

          return result.Count() > 0 ? true : false;
      }

        protected virtual void GetRussianArticlesWithoutUkrainian()
        {
            Page.CacheEnabled = true; // for test

            PageList plRu = new PageList(theySite);
            plRu.FillFromFile(allTheyWikiAboutUkraine);

            //plRu.LoadEx();

            PageList noUkInterwiki = new PageList(theySite);
            PageList hasUkInterwiki = new PageList(theySite);

            PageList hasUkInterwikiOld = new PageList(theySite);
            PageList noUkInterwikiOld = new PageList(theySite);

            
            hasUkInterwikiOld.FillFromFile(hasUkInterwikiFile,true);
            noUkInterwikiOld.FillFromFile(absentTheyWikiAboutUkraine, true);

            MoveFromAbsentToNewList(plRu, hasUkInterwiki, hasUkInterwikiOld);
            //MoveFromAbsentToNewList(plRu, noUkInterwiki, noUkInterwikiOld);

            plRu = NoUkInterwikisPublisher.FilterSilRady(plRu);

            int counter = 1;
            File.Delete(absentStats);
            File.Delete(absentStatsSorted);
            File.Delete(absentStatsSortedBySize);

            //Parallel.ForEach(plRu.pages, page =>
            plRu.pages.ForEach(page =>
            //foreach (Page page in plRu)
            {               
                Console.Write("Page N" + counter + ". ");

                bool hasUkLink = false;

                hasUkLink = HasInterwikiInWikiData(page, "uk");

                

                object syncObj = new object();

                if (!hasUkLink && string.IsNullOrWhiteSpace(page.text)) // make redirects as hauklink
                    {
                    page.Load();
                    //hasUkLink = page.IsRedirect(); //not skip and not insert it in the result list

                    if(page.IsRedirect())
                        {
                            page.ResolveRedirect();
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

            noUkInterwiki.SaveTitlesToFile(absentTheyWikiAboutUkraine);
            hasUkInterwiki.SaveTitlesToFile(hasUkInterwikiFile);
        }

        

        private static bool HasInterwikiInline(Page page, string lang)
        {
            page.Load();
            var links = page.GetInterLanguageLinks();

            string ukLink = links.FirstOrDefault<string>(l => l.ToLower().Contains(lang+":"));
            bool hasUkLink = !string.IsNullOrEmpty(ukLink);
            return hasUkLink;
        }

        protected virtual void FilterPersonalii()
        {
            
            /*
            PageList plAbsent = new PageList(ruSite);
            plAbsent.FillFromFile(absentTheyWikiAboutUkraine);
            

            PageList bezPersonaliy = new PageList(ruSite);
            PageList personalii = new PageList(ruSite);

            PageList bezPersonaliyOld = new PageList(ruSite);
            PageList personaliiOld = new PageList(ruSite);

            
            bezPersonaliyOld.FillFromFile(finalBezPerson,true);

            personaliiOld.FillFromFile(finalZPersonamy, true);

            StopwatchExtensions.BenchMark(() => {
            MoveFromAbsentToNewList(plAbsent, bezPersonaliy, bezPersonaliyOld);
            MoveFromAbsentToNewList(plAbsent, personalii, personaliiOld);}
            , "moving");

            

            foreach (Page page in plAbsent)
            {                

                page.Load();
                if (!IsPerson(page))
                {
                    bezPersonaliy.Add(page);
                }
                else
                {
                    personalii.Add(page);
                }


            }
            bezPersonaliy.SaveTitlesToFile(finalBezPerson);
            personalii.SaveTitlesToFile(finalZPersonamy);
             */ 
        }

        private static void MoveFromAbsentToNewList(PageList plStartCommonList, PageList newList, PageList oldList)
        {
            newList.pages.AddRange(plStartCommonList.pages.Intersect(oldList.pages.AsParallel(), new PageComparer()));

            plStartCommonList.pages.RemoveAll((p) => newList.pages.Contains(p, new PageComparer()));
        }        

        private static bool IsPerson(Page page)
        {
            bool isPerson = false;

            var categories = page.GetAllCategories();


            string personalii = categories.FirstOrDefault<string>
                (l => l.Contains("персоналии") || l.Contains("Персоналии")
                    || l.Contains("Архитекторы")
                    || l.Contains("Родившиеся")
                    || l.Contains("Ныне живущие")
                    || l.Contains("Выпускники")
                    || l.Contains("Члены и члены-корреспонденты")
                    || l.Contains("Ботаники")
                    || l.Contains("Фамилии")
                    || l.Contains("Тренеры")
                    || l.Contains("Яхтсмены")
                    );

            if (!string.IsNullOrEmpty(personalii))
                isPerson = true;


            if (!isPerson)
            {
                var titlearr = page.title.Split(new string[] { " " },
                    StringSplitOptions.RemoveEmptyEntries);

                //Шалом, Украина! (фестиваль)
                //Мероприятия, посвящённые 1020-летию крещения Киевской Руси
                //Сёла, включённые в состав других населённых пунктов (Симферопольский район)
                //Сёла, включённые в состав Симферополя
                //bad idea...
                if (titlearr.Count() >= 1 && titlearr[0].Contains(",")
                    && titlearr.Count() <= 3
                    && !page.title.Contains("!"))
                    isPerson = true;
            }

            return isPerson;
        }
    }
}
