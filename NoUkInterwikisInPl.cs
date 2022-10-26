using System.Collections.Generic;
using DotNetWikiBot;
using System.Collections.Specialized;
using System.IO;

namespace botSolution
{
    public class NoUkInterwikisInPl : NoUkInterwikis
    {
        public NoUkInterwikisInPl()
        {
            Directory.CreateDirectory(GetSrcLangFolder());
            allTheyWikiAboutUkraine =  GetSrcLangFolder() +  "allPLWikiAboutUkraine.txt";
            absentTheyWikiAboutUkraine = GetSrcLangFolder() + "absentPLWikiAboutUkraine.txt";
            allPagesListVsi = GetSrcLangFolder() + "vsi.txt";
            theySite = MyBot.plSiteL.Value;
        }


        public override string GetSrcLangFolder()
        {
            return PathToFolder + @"pl-source\";
        }

        protected override PageList GetUkraineSubCategoriesInTheyWiki()
        {
            Page.CacheEnabled = true;
            Directory.CreateDirectory(PathToFolder);

            PageList plUkr = new PageList(theySite);


            StringCollection coll = new StringCollection();


            List<string> include = new List<string>();
            include.Add("Obwód");
            include.Add("Krym");
            include.Add("Ukrain");


            plUkr.FillAllFromCategoryTree("Ukraina", include.ToArray());
            plUkr.RemoveNamespaces(new int[] { 14 }); //category           


            //MyBot.ClearAllNamespaces(plUkr, MyBot.ruSite); //why?

            plUkr.SaveTitlesToFile(allTheyWikiAboutUkraine);

            return plUkr;
        }
    }
}
