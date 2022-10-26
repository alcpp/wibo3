using DotNetWikiBot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace botSolution
{
    public class NoInterwikiInternational
    {
        
        public static Site beSite = MyBot.beSiteL.Value;

        public static Site ukSite = MyBot.ukSiteL.Value;
        public static string PathToFolder = @"c:\ukraine-iwi\";
        private string lang;

        public string PathToLangFolder
        {
            get
            {
                var path = PathToFolder + lang + "\\";

                Directory.CreateDirectory(path);
                return path;
            }
        }

        

        
        public  NoInterwikiInternational(string lang)
        {
            this.lang = lang;
        }
        
        public void Run()
        {
            List<string> include = GetIncludeList();

            PageList pl = new PageList(beSite);
            pl.FillSubsFromCategoryTree("Украіна", 5, include.ToArray(), GetExcludeList().ToArray());

            pl.SaveTitlesToFile(PathToLangFolder+"categList.txt");
        }

        protected List<string> GetIncludeList()
        {
            List<string> include = new List<string>();
            include.Add("Воблас");
            include.Add("Крым");
            include.Add("Украін");
            return include;
        }

        protected List<string> GetExcludeList()
        {
            PageList pl = new PageList(beSite);
            {
                pl.FillFromFile(PathToLangFolder + "people.txt");
            }

            var list = (from page in pl.pages
                        select page.title).ToList();
            
            list.Add("Постаці");
            list.Add("Памерлі");
            list.Add("Пахаваныя");
            list.Add("Нарадзіліся");

            return list;
        }

    }
}
