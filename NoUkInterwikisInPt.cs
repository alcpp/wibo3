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
    public class NoUkInterwikisInPt : NoUkInterwikis
    {
        public NoUkInterwikisInPt()
        {
            Directory.CreateDirectory(GetSrcLangFolder());
            allTheyWikiAboutUkraine =  GetSrcLangFolder() +  "all_PtWikiAboutUkraine.txt";
            absentTheyWikiAboutUkraine = GetSrcLangFolder() + "absent_PtWikiAboutUkraine.txt";
            allPagesListVsi = GetSrcLangFolder() + "vsi.txt";
            theySite = MyBot.ptSiteL.Value;
        }


        public override string GetSrcLangFolder()
        {
            return PathToFolder + @"pt-source\";
        }

        protected override PageList GetUkraineSubCategoriesInTheyWiki()
        {
            Page.CacheEnabled = true;
            Directory.CreateDirectory(PathToFolder);

            PageList plUkr = new PageList(theySite);


            StringCollection coll = new StringCollection();


            List<string> include = new List<string>();
            include.Add("Oblast");
            include.Add("Crimeia");
            include.Add("Ucrânia");


            plUkr.FillAllFromCategoryTree("Ucrânia", include.ToArray());
            plUkr.RemoveNamespaces(new int[] { 14 }); //category


            //MyBot.ClearAllNamespaces(plUkr, MyBot.ruSite); //why?

            plUkr.SaveTitlesToFile(allTheyWikiAboutUkraine);

            return plUkr;
        }
    }
}
