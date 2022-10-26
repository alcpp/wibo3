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
    public class NoUkInterwikisInDe : NoUkInterwikis
    {
        public NoUkInterwikisInDe()
        {
            Directory.CreateDirectory(GetSrcLangFolder());
            allTheyWikiAboutUkraine =  GetSrcLangFolder() +  "all_DeWikiAboutUkraine.txt";
            absentTheyWikiAboutUkraine = GetSrcLangFolder() + "absent_DeWikiAboutUkraine.txt";
            allPagesListVsi = GetSrcLangFolder() + "vsi.txt";
            theySite = MyBot.deSiteL.Value;
        }


        public override string GetSrcLangFolder()
        {
            return PathToFolder + @"de-source\";
        }

        protected override PageList GetUkraineSubCategoriesInTheyWiki()
        {
            Page.CacheEnabled = true;
            Directory.CreateDirectory(PathToFolder);

            PageList plUkr = new PageList(theySite);


            StringCollection coll = new StringCollection();


            List<string> include = new List<string>();
            include.Add("Oblast");
            include.Add("Krim");
            include.Add("Ukrain");


            plUkr.FillAllFromCategoryTree("Ukraine", include.ToArray());
            plUkr.RemoveNamespaces(new int[] { 14 }); //category           


            //MyBot.ClearAllNamespaces(plUkr, MyBot.ruSite); //why?

            plUkr.SaveTitlesToFile(allTheyWikiAboutUkraine);

            return plUkr;
        }
    }
}
