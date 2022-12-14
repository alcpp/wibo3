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

namespace botSolution
{
    public class NoUkInterwikisInFr : NoUkInterwikis
    {
        public NoUkInterwikisInFr()
        {
            Directory.CreateDirectory(GetSrcLangFolder());
            allTheyWikiAboutUkraine =  GetSrcLangFolder() +  "all_FrWikiAboutUkraine.txt";
            absentTheyWikiAboutUkraine = GetSrcLangFolder() + "absent_FrWikiAboutUkraine.txt";
            allPagesListVsi = GetSrcLangFolder() + "vsi.txt";
            theySite = MyBot.frSiteL.Value;
        }


        public override string GetSrcLangFolder()
        {
            return PathToFolder + @"fr-source\";
        }

        protected override PageList GetUkraineSubCategoriesInTheyWiki()
        {
            Page.CacheEnabled = true;
            Directory.CreateDirectory(PathToFolder);

            PageList plUkr = new PageList(theySite);


            StringCollection coll = new StringCollection();


            List<string> include = new List<string>();
            include.Add("Oblast");
            include.Add("Crimée");
            include.Add("Ukrain");


            plUkr.FillAllFromCategoryTree("Ukraine", include.ToArray());
            plUkr.RemoveNamespaces(new int[] { 14 }); //category           


            //MyBot.ClearAllNamespaces(plUkr, MyBot.ruSite); //why?

            plUkr.SaveTitlesToFile(allTheyWikiAboutUkraine);

            return plUkr;
        }
    }
}
