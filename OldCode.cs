using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWikiBot;

namespace botSolution
{
    class OldCode
    {
        public static void AddRikAndSib()
        {


            string[] arr = new string[50];
            int start = 1;
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = (start + i).ToString();
            }

            Site site = new Site();
            PageList pl = new PageList(site, arr);

            //pl.a

            pl.LoadEx();
            //pl.FillFromLinksToPage
            Console.WriteLine("all pages in the list are loaded");
            foreach (Page p in pl.pages)
            {

                //if (p.lastUser == "PBato")
                {


                    p.text = System.Web.HttpUtility.HtmlDecode(p.text);
                    Console.WriteLine(p.text.Substring(0, 200));
                    Console.WriteLine(" this page was edited by Pbato: " + p.title);

                    if (AddRikAndSibToPage(p) == true)
                        p.Save();
                }
            }
        }


        private static bool AddRikAndSibToPage(Page p)
        {
            if (p.text.IndexOf("{{Рік}}") == -1)
            {
                p.text = "{{Рік}}" + p.text;
                Bot.editComment = "Додано {{Рік}}";
                Bot.isMinorEdit = true;
                Console.WriteLine("-Page " + p.title + " had no RIK template");
                string[] sisters = p.GetSisterWikiLinks(true);
                Console.WriteLine(sisters.Length + " sisters======");
                foreach (string sis in sisters)
                {
                    Console.WriteLine("sister " + sis.ToString());
                }
                //p.comment = "Додано {{Рік}} та ru-sib:";
                if (p.text.IndexOf("ru-sib:") == -1)
                {
                    string[] newSisters = { "ru-sib:" + p.title };
                    p.AddInterWikiLinks(newSisters);
                    Console.WriteLine("Added interwiki " + newSisters[0]);
                }
                return true;

            }
            else
            {
                Console.WriteLine("+Page " + p.title + " HAS RIK template");
                return false;
            }
        }

        //{"daily_views": {"2013-02-24": 27323, "2013-02-25": 29696, "2013-02-13": 39588, "2013-02-12": 42897, "2013-02-11": 44469, "2013-02-10": 40538, "2013-02-17": 31899, "2013-02-16": 26110, "2013-02-15": 32376, "2013-02-14": 33764, "2013-02-31": 0, "2013-02-30": 0, "2013-02-19": 31295, "2013-02-18": 30755, "2013-02-04": 40473, "2013-02-23": 23394, "2013-02-06": 39369, "2013-02-07": 38502, "2013-02-27": 28032, "2013-02-28": 30617, "2013-02-29": 0, "2013-02-08": 33441, "2013-02-09": 32148, "2013-02-22": 23375, "2013-02-05": 40040, "2013-02-20": 30446, "2013-02-21": 26753, "2013-02-26": 31837, "2013-02-01": 43782, "2013-02-02": 34826, "2013-02-03": 42392}, "project": "uk", "month": "201302", "rank": 2, "title": "\u0413\u043e\u043b\u043e\u0432\u043d\u0430_\u0441\u0442\u043e\u0440\u0456\u043d\u043a\u0430"}
    }
}
