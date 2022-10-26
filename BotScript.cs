// Write your own bot scripts and functions in this file.
// Run "Compile & Run.bat" file - it will compile this file as executable and run it.
// XML autodocumentation for DotNetWikiBot namespace is available as "DotNetWikiBotDoc.xml"

using System;
using System.IO;
using System.Text;

using DotNetWikiBot;
using System.Diagnostics;
using System.Threading;
using System.Linq;

using botSolution;
using System.Globalization;

using System.Collections.Generic;
using System.Net;
using System.Collections.Specialized;

class MyBot : Bot
{
    //public static Site ukSite2.Value =     new Site("https://uk.wikipedia.org", "Alex_Blokha", "predator32");


    public static readonly Lazy<Site> ukSiteL = new Lazy<Site>(() =>
        new Site("https://uk.wikipedia.org", "Alex_Blokha", "predator32"));


    public static readonly Lazy<Site> ruSiteL = new Lazy<Site>(() =>
        new Site("https://ru.wikipedia.org", "Alex_Blokha", "predator32"));


    public static readonly Lazy<Site> plSiteL = new Lazy<Site>(() =>
        new Site("https://pl.wikipedia.org", "Alex_Blokha", "predator32"));
    
    public static readonly Lazy<Site> frSiteL = new Lazy<Site>(() =>
        new Site("https://fr.wikipedia.org", "Alex_Blokha", "predator32"));

    public static readonly Lazy<Site> esSiteL = new Lazy<Site>(() =>
        new Site("https://es.wikipedia.org", "Alex_Blokha", "predator32"));

    public static readonly Lazy<Site> ptSiteL = new Lazy<Site>(() =>
        new Site("https://pt.wikipedia.org", "Alex_Blokha", "predator32"));

    public static readonly Lazy<Site> deSiteL = new Lazy<Site>(() =>
        new Site("https://de.wikipedia.org", "Alex_Blokha", "predator32"));

    public static readonly Lazy<Site> enSiteL = new Lazy<Site>(() =>
        new Site("https://en.wikipedia.org", "Alex_Blokha", "predator32"));


    public static readonly Lazy<Site> beSiteL = new Lazy<Site>(() =>
        new Site("https://be.wikipedia.org", "Alex_Blokha", "predator32"));
	

	public static void Main()
	{

		ServicePointManager.Expect100Continue = true;
		ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //hack https://stackoverflow.com/questions/2859790/the-request-was-aborted-could-not-create-ssl-tls-secure-channel/48930280#48930280

		Stopwatch sw = new Stopwatch();
		sw.Start();


		//NeVystachaye.NeVystachayeFunc();
		//NoUkInterwikis.GetUkraineSubCategoriesInUk();

		//ProcessDates();

		//RenameDni();
		//InserLinkToDates();

		//NoUkInterwikis.GetStatsNoUkGeneratedList();
		//NoUkInterwikis.NoUkInterwikiFullCycle();

		//Popular.GetPopularInRu();
		//Ratio.GetRatioList();

		//NoUkInterwikis.NoUkInterwikiFullCyclePt();

		//DniproArticleCounter();

		sw.Stop();
		Console.WriteLine("Whole Operation took " + sw.Elapsed.ToString());

        //CreatePagePile();
        Search();

		//NadPopular();
	}

	private static void CreatePagePile()
	{
		string allArticles = File.ReadAllText(@"C:\ukraine-iwi\en-source\allENWikiAboutUkraine.txt");

		allArticles = "aaa\n\rbbb";

		NameValueCollection coll = new NameValueCollection();
		coll.Add("action", "create_pile_with_data");
		coll.Add("wiki", "en");
		coll.Add("data", allArticles);




		PostSubmitter ps = new PostSubmitter("http://tools.wmflabs.org/pagepile/api.php", coll);
		ps.Type = PostSubmitter.PostTypeEnum.Post;
		string result = ps.Post();
	}
    private static void Search()
    {
        string allArticles = File.ReadAllText(@"C:\ukraine-iwi\en-source\allENWikiAboutUkraine.txt");

        //allArticles = "aaa\r\nbbb";

        NameValueCollection coll = System.Web.HttpUtility.ParseQueryString("https://petscan.wmflabs.org/?show%5Fsoft%5Fredirects=both&labels%5Fno=&subpage%5Ffilter=either&cb%5Flabels%5Fyes%5Fl=1&referrer%5Furl=&ns%5B0%5D=1&before=&output%5Flimit=&labels%5Fany=&project=wikipedia&outlinks%5Fany=&ores%5Fprob%5Ffrom=&smaller=&wikidata%5Fitem=no&min%5Fsitelink%5Fcount=&langs%5Flabels%5Fno=&wikidata%5Fsource%5Fsites=&cb%5Flabels%5Fany%5Fl=1&negcats=&ores%5Ftype=any&edits%5Bbots%5D=both&links%5Fto%5Fall=&templates%5Fany=&sitelinks%5Fno=ukwiki&larger=&maxlinks=&max%5Fage=&categories=&edits%5Bflagged%5D=both&page%5Fimage=any&templates%5Fyes=&outlinks%5Fyes=&outlinks%5Fno=&max%5Fsitelink%5Fcount=&langs%5Flabels%5Fyes=&sortby=none&sortorder=ascending&doit=Do%20it%21&active%5Ftab=tab%5Foutput&sitelinks%5Fany=&common%5Fwiki=auto&source%5Fcombination=&templates%5Fno=&since%5Frev0=&wpiu=any&labels%5Fyes=&manual%5Flist=Ukraine%0D%0ADnipro&cb%5Flabels%5Fno%5Fl=1&links%5Fto%5Fany=&ores%5Fprediction=any&output%5Fcompatability=catscan&pagepile=&language=en&minlinks=&sitelinks%5Fyes=&wikidata%5Fprop%5Fitem%5Fuse=&min%5Fredlink%5Fcount=1&wikidata%5Flabel%5Flanguage=&show%5Fredirects=both&search%5Fquery=&combination=subset&search%5Fwiki=&ores%5Fprob%5Fto=&interface%5Flanguage=en&sparql=&show%5Fdisambiguation%5Fpages=both&manual%5Flist%5Fwiki=enwiki&common%5Fwiki%5Fother=&search%5Ffilter=&links%5Fto%5Fno=&referrer%5Fname=&namespace%5Fconversion=keep&depth=0&search%5Fmax%5Fresults=500&format=wiki&regexp%5Ffilter=&langs%5Flabels%5Fany=&edits%5Banons%5D=both&after=");

        //NameValueCollection coll = new NameValueCollection();
        coll["sitelinks_no"] = "ukwiki";
        coll["manual_list_wiki"]= "enwiki";
        coll["manual_list"] = allArticles;
        coll["format"] = "wiki";

        PostSubmitter ps = new PostSubmitter("https://petscan.wmflabs.org/", coll);
        ps.Type = PostSubmitter.PostTypeEnum.Post;
        string result = ps.Post();
		Console.WriteLine(result);
    }

    private static void GetAndSavePagesFromCategory(string name)
    {
        PageList category = new PageList(ukSiteL.Value);
        category.FillAllFromCategoryTree(name);
        category.FilterNamespaces(new int[] { 0 }); // only pages will remain
        category.SaveTitlesToFile(NoUkInterwikis.PathToFolder + name);
    }    

    private static void InserLinkToDates()
    {

        PageList plUkr = new PageList(ukSiteL.Value);
        //plUkr.FillAllFromCategory("Категорія:Дні загадні в Вікіпедії/Україна");

        int counter = 0;

        string baseAddress = "Користувач:Alex Blokha/Дні згадані в Вікіпедії/Україна/";

        DateTime day = new DateTime(2000, 09, 1);
        for(; ;)
        {
            if (counter > 7)
                break;

            DatesProcessor datesProcessor = new DatesProcessor();
            datesProcessor._AreDaysAnalyzed = true;
            string dateStr = datesProcessor.GetCalculatedDateString(day, 0);


            Page p = new Page(ukSiteL.Value, "Вікіпедія:Проект:Цей день в історії/" + dateStr);
            p.Load();


            if (!p.text.Contains(baseAddress))
            {
                //string[] iw = p.GetInterWikiLinks(); // we don't need slowdown, when collecting interwikis
                //p.RemoveInterWikiLinks();

                Page ukrPodii = new Page(ukSiteL.Value, "Користувач:Alex Blokha/Дні згадані в Вікіпедії/Україна/" + dateStr);
                bool doRenameAlso = false;
                if (doRenameAlso)
                {
                    ukrPodii.Load();

                    if (string.IsNullOrEmpty(ukrPodii.text))
                    {
                        Page ukrPodiiRoky = new Page(ukSiteL.Value, "Користувач:Alex Blokha/Роки згадані в Вікіпедії/Україна/" + dateStr);
                        try
                        {
                            ukrPodiiRoky.RenameTo(ukrPodii.title, "wrong name");
                        }
                        catch
                        {
                        }
                    }
                }
                

                p.text += "\n\n" + "*\n\n[[" + ukrPodii.title + "|Українські події]]";
                //if (iw.Length != 0)
                //    p.AddInterWikiLinks(iw);

                p.Save("Українські події", false);

                counter++;
                
            }
            day=day.AddDays(1);

            
        }


    }

    private static void RenameDni()
    {
        
        PageList plUkr = new PageList(ukSiteL.Value);
        plUkr.FillAllFromCategory("Категорія:Дні загадні в Вікіпедії/Україна");


        foreach (Page p in plUkr.pages)
        {
            if (p.title.Contains("Користувач:Alex Blokha/Роки згадані"))
            {
                try
                {
                    p.RenameTo(p.title.Replace("Роки згадані", "Дні згадані"), "wrong name");
                }
                catch
                { }
                //Thread.Sleep(1000);
            }
        }

        
    }       

    

    

    private static void LinksCountOfArticle()
    {
        var site =
                ukSiteL.Value;
        PageList plUkr = new PageList(site);
        plUkr.FillFromFile(@"C:\1000list.txt");

        string stat = "title;nOfLinks";
        StringBuilder b = new StringBuilder();
        b.AppendLine(stat);
        string popCategory = "Категорія:Надпопулярні статті";
        Semaphore sem = new Semaphore(0, 5);

        //Page.CacheEnabled = true;

        int count = 0;
        foreach (Page p in plUkr)
        {
            

            count++;
            p.Load();
            var links = p.GetLinks();
            stat = string.Format("{0};{1}", p.title, links.Count());
            b.AppendLine(stat);

            Page ps = p;

            ThreadPool.QueueUserWorkItem(delegate(object o)
            {
                sem.WaitOne();
                try
                {
                    //ps.Save("/*Додано категорію *Надпопулярні статті*/", true);
                }
                catch (Exception)
                {
                }
                finally
                {
                    sem.Release();
                    Console.WriteLine("=====page {0}, #{1} saved", ps.title, count);
                }
            }
            );
        }

        File.WriteAllText(@"c:\1000popStats.txt", b.ToString(), Encoding.UTF8);
    }

    private static void NadPopular()
    {
        object SyncObj = new object();
        Stopwatch sw = new Stopwatch();
        sw.Start();

        var site = ukSiteL.Value;
        PageList plUkr = new PageList(site);
        plUkr.FillFromFile(@"C:\200-worklist.txt");

        //var deleg = new Action (delegate() { lock (SyncObj) { plUkr.Load(20); } });
        //deleg.BeginInvoke(null, null);
        //ThreadPool.QueueUserWorkItem();


        plUkr.Load();

        string stat = "title;nOfLinks";
        StringBuilder b = new StringBuilder();
        b.Append(stat);
        string popCategory = "Категорія:Надпопулярні статті";
        Semaphore sem = new Semaphore(0, 5);

        int count = 0;
        foreach (Page p in plUkr)
        {
            count++;
            var links = p.GetLinks();
            stat = string.Format("{0};{1}", p.title, links.Count());
            b.Append(stat);



            if (!p.GetAllCategories().Contains(popCategory))
            {
                p.AddToCategory(popCategory);

                Page ps = p;

                ThreadPool.QueueUserWorkItem(delegate(object o)
                {
                    sem.WaitOne();
                    try
                    {
                        ps.Save("/*Додано категорію *Надпопулярні статті*/", true);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        sem.Release();
                        Console.WriteLine("=====page {0}, #{1} saved", ps.title, count);
                    }
                }
                );
                //p.Save("/*Додано категорію *Надпопулярні статті*/", true);


                //var deleg = new Action(delegate() { lock (SyncObj) { p.Save("/*Додано категорію *Надпопулярні статті*/", true); } });
                //deleg.BeginInvoke(null, null);


            }
            else
                Console.WriteLine("Page {0}, #{1} was already processed", p.title, count);

        }

        File.WriteAllText(@"c:\1000popStats.txt", b.ToString());

        sw.Stop();
        Console.WriteLine("Whole Operation took " + sw.Elapsed.ToString());

        Thread.Sleep(3000);
        sem.Release(5);
        Console.WriteLine("You should wait untill other threads stop");

        for (int i = 0; i < 1000; i++)
        {
            Console.WriteLine("Waiting");
            Thread.Sleep(3000);
        }

        Console.ReadKey();
    }

    

    private static void ProcessDates()
    {
        
        PageList pl = new PageList(ukSiteL.Value);
        pl.FillFromFile(NoUkInterwikis.allUkWikiAboutUkraine);

        DatesProcessor dt = new DatesProcessor();
        dt.ProcessDatesRange(pl);

        
    }

	public static void RemoveNonInterestingPages(PageList pl)
	{
        ClearAllNamespaces(pl, ukSiteL.Value);
		for(int i=0; i<pl.pages.Count;i++)
		{
			Page p = pl[i];
			if (p.title.Contains("NGC ") || p.title.Contains("Користувач:")
				|| p.title.Contains("Чемпіонат України з футболу")
				|| p.title.Contains("Чемпіонат Європи з футболу")
				|| p.title.Contains("Чемпіонат світу з футболу")
				|| p.title.Contains("S/")
                //|| p.title.Contains(":") // всі техн. статті // ClearAllNamespaces
				|| p.title.Contains("Січень")
				|| p.title.Contains("Лютий")
				|| p.title.Contains("Березень")
				|| p.title.Contains("Квітень")
				|| p.title.Contains("Травень")
				|| p.title.Contains("Червень")
				|| p.title.Contains("Липень")
				|| p.title.Contains("Серпень")
				|| p.title.Contains("Вересень")
				|| p.title.Contains("Жовтень")
				|| p.title.Contains("Листопад")
				|| p.title.Contains("Грудень")
				
				|| p.title.Contains("Вікіпедія")
				|| p.title.Contains("Список усiх днiв")
				|| p.title.Contains("з футболу")

				|| p.title.Contains(" район")

				|| (p.title.Contains(".") && p.title.Length <= 3) // removes domains like ".ad"

				//Цей день в iсторiї
				//|| !p.title.Contains("Драгоманов")
				)
			{
				pl.pages.Remove(p);
				i--;
				//p.RemoveFromCategory()
				
			}
			//Console.WriteLine(p.title);
		}
	}

    public static void ClearAllNamespaces(PageList pl, Site siteForNamespaces)
    {
        //pl.FillFromFile(@"c:\ukraine-ru-obl.txt");
        Console.WriteLine("Clearing namespaces...");
        foreach (var ns in siteForNamespaces.namespaces)
        {
            pl.RemoveNamespaces(new int[] { int.Parse(ns.Key.ToString()) });
        }
    }

	public static PageList GetCommonPages(PageList one, PageList two)
	{
		PageList result = new PageList();
		foreach (Page page1 in one.pages)
		{

			foreach (Page page2 in two.pages)
			{
				if (page1.title == page2.title)
					result.Add(page1);
			}

		}

		return result;

	}
	
	
	
}