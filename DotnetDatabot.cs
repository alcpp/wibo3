using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetDataBot;

namespace botSolution
{
    class DotnetDatabotExt
    {
        public static DotNetDataBot.Site WD = new DotNetDataBot.Site
                ("https://www.wikidata.org", "Alex_Blokha", "predator32");

        

        public static bool HasInterwikiInWikiData(DotNetWikiBot.Page page, string lang)
        {            
            Dictionary<string, string> site_link = new Dictionary<string, string>();
            Item emptyItem = new Item(WD);            

            Item item = null;

            /*if (emptyItem.itemExists("it", "George Lucas")) // Check if exist on Wikidata
            {
                Console.WriteLine("exists");
            }*/

            try
            {
                item = new Item(WD, "Q" +
                    emptyItem.GetIdBySitelink(page.site.language, page.title));

                Retry.Do(() =>
                    {
                        item = new Item(WD, "Q" +
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
            bool hasIwiki = site_link.ContainsKey(lang+"wiki");
            if (hasIwiki)
            {
                Console.WriteLine("Page has uk iwiki:" + page.title);
            }
            return hasIwiki;
        }
    }
}
