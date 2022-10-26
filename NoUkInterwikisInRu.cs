using System.Collections.Generic;
using DotNetWikiBot;
using System.Collections.Specialized;
using System.IO;

namespace botSolution
{
    public class NoUkInterwikisInRu : NoUkInterwikis
    {
        public NoUkInterwikisInRu()
        {
            Directory.CreateDirectory(GetSrcLangFolder());
            allTheyWikiAboutUkraine =  GetSrcLangFolder() +  "allRuWikiAboutUkraine.txt";
            absentTheyWikiAboutUkraine = GetSrcLangFolder() + "absentRuWikiAboutUkraine.txt";
            allPagesListVsi = GetSrcLangFolder() + "vsi.txt";
            theySite = MyBot.ruSiteL.Value;
        }


        public override string GetSrcLangFolder()
        {
            return PathToFolder + @"ru-source\";
        }

        protected override PageList GetUkraineSubCategoriesInTheyWiki()
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

            return plUkr;
        }
    }
}
