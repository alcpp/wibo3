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
    public class NoUkInterwikisInEn : NoUkInterwikis
    {
        public NoUkInterwikisInEn()
        {
            Directory.CreateDirectory(GetSrcLangFolder());
            allTheyWikiAboutUkraine =  GetSrcLangFolder() +  "allENWikiAboutUkraine.txt";
            absentTheyWikiAboutUkraine = GetSrcLangFolder() + "absentENWikiAboutUkraine.txt";
            allPagesListVsi = GetSrcLangFolder() + "vsi.txt";
            theySite = MyBot.enSiteL.Value;
            
        }


        public override string GetSrcLangFolder()
        {
            return PathToFolder + @"en-source\";
        }

        protected override PageList GetUkraineSubCategoriesInTheyWiki()
        {
            Page.CacheEnabled = true;
            Directory.CreateDirectory(PathToFolder);

            PageList plUkr = new PageList(theySite);

            StringCollection coll = new StringCollection();


            List<string> include = new List<string>();
            include.Add("Oblast");
            include.Add("Crimea");
            include.Add("Ukrain");


            plUkr.FillAllFromCategoryTree("Ukraine", include.ToArray());
            plUkr.RemoveNamespaces(new int[] { 14 }); //category           
            

            plUkr.SaveTitlesToFile(allTheyWikiAboutUkraine);

            return plUkr;
        }
    }
}
