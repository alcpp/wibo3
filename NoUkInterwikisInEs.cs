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
    public class NoUkInterwikisInEs : NoUkInterwikis
    {
        public NoUkInterwikisInEs()
        {
            Directory.CreateDirectory(GetSrcLangFolder());
            allTheyWikiAboutUkraine =  GetSrcLangFolder() +  "all_EsWikiAboutUkraine.txt";
            absentTheyWikiAboutUkraine = GetSrcLangFolder() + "absent_EsWikiAboutUkraine.txt";
            allPagesListVsi = GetSrcLangFolder() + "vsi.txt";
            theySite = MyBot.esSiteL.Value;
        }


        public override string GetSrcLangFolder()
        {
            return PathToFolder + @"es-source\";
        }

        protected override PageList GetUkraineSubCategoriesInTheyWiki()
        {
            Page.CacheEnabled = true;
            Directory.CreateDirectory(PathToFolder);

            PageList plUkr = new PageList(theySite);


            StringCollection coll = new StringCollection();


            List<string> include = new List<string>();
            include.Add("Óblast");
            include.Add("Crimea");
            include.Add("Ucrania");


            plUkr.FillAllFromCategoryTree("Ucrania", include.ToArray());
            plUkr.RemoveNamespaces(new int[] { 14 }); //category           


            //MyBot.ClearAllNamespaces(plUkr, MyBot.ruSite); //why?

            plUkr.SaveTitlesToFile(allTheyWikiAboutUkraine);

            return plUkr;
        }
    }
}
