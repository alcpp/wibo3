// This supplementary script is used to test and debug DotNetWikiBot Framework
// This script requires "Defaults.dat" file, see "Site(string address)" constructor documentation 
// Distributed under the terms of the GNU GPLv2 license: http://www.gnu.org/licenses/gpl-2.0.html
// Copyright (c) Iaroslav Vassiliev <codedriller@gmail.com>

using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using DotNetWikiBot;
using System.Linq;
using System.Net;
using System.Xml;
using System.Web;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

class DebugBot : Bot
{
	public static Site testwp;
	public static Site ruwp;
	public static Site wikiaSite;
	public static Site wikiaTools;

	/// <summary>The entry point function. Debug version.</summary>
	public static void Main()
	{

		// Uncomment to enable Fiddler web-debugger
		// http://www.fiddler2.com/Fiddler/Help/hookup.asp#Q-DOTNET
		//GlobalProxySelection.Select = new WebProxy("127.0.0.1", 8888);

        // Clear cache
        if (Directory.Exists("Cache"))
		    foreach (FileInfo f in new DirectoryInfo("Cache").GetFiles("http*.xml"))
			    f.Delete();

		//System.Diagnostics.Debugger.Launch();	// DEBUG
		//System.Diagnostics.Debugger.Break();	// DEBUG

		//Console.WriteLine("here");	// DEBUG
		//File.WriteAllText("debug.html", src, Encoding.UTF8);	// DEBUG
		//askConfirm = true;	// DEBUG
		//EnableLogging(@"Cache\BotLog.txt");

		// View cookies
		//foreach(Cookie c in site.cookies.GetCookies(new Uri("https://ru.wikipedia.org/")))
			//Console.WriteLine(c.Name + "=" + c.Value);

		// COMPLEX UNIT TESTING
		// Press "Retry" on failed assertion window and then F11 ("Step Into")
		TestLogins();
		TestTestWikipedia(true);
		//TestRuWikipedia(true);
		//TestWikiaWikibotSite(true);    // tests site-modifying functions only
		//TestWikiaAdminTools(true);    // tests most functions not requiring admin rights

		//CustomTest();

		Console.WriteLine("Press any key to continue...");
		Console.ReadKey();
	}

	/// <summary>
	/// Tests logging into different sites.
	/// </summary>
	public static void TestLogins()
	{
		testwp = new Site("https://test.wikipedia.org");
			Debug.Assert(testwp.GetType() == typeof(Site));
		ruwp = new Site("https://ru.wikipedia.org");
			Debug.Assert(ruwp.GetType() == typeof(Site));
		wikiaSite = new Site("http://wikibot.wikia.com");
			Debug.Assert(wikiaSite.GetType() == typeof(Site));
		wikiaTools = new Site("http://admintools.wikia.com");
			Debug.Assert(wikiaTools.GetType() == typeof(Site));
		Console.WriteLine("\n    END OF LOGINS TEST");
	}

	/// <summary>
	/// Tests DotNetWikiBot functions on ru.wikipedia.org site.
	/// </summary>
	public static void TestRuWikipedia(bool testSaving)
	{
		DateTime testingTime = DateTime.Now;

		if (ruwp == null)
			ruwp = new Site("https://ru.wikipedia.org");
		ruwp.retryTimes = 10;

		Page p;
		PageList pl;
		/**/
		Console.WriteLine("Checking Site variables...");
		Debug.Assert(ruwp.address.Equals("https://ru.wikipedia.org"));
		Debug.Assert(ruwp.indexPath.Equals("https://ru.wikipedia.org/w/index.php"));
		Debug.Assert(ruwp.shortPath.Equals("/wiki/"));
		Debug.Assert(ruwp.language.Equals("ru"));
		Debug.Assert(ruwp.name.Equals("Википедия"));
		Debug.Assert(ruwp.capitalization.Equals("first-letter"));
		Debug.Assert(ruwp.timeOffset.Equals("0"));
		Debug.Assert(ruwp.software.Contains("MediaWiki"));
		Debug.Assert(ruwp.version > new Version("1.20"));
		Debug.Assert(ruwp.langCulture.Name.Equals("ru"));
		Debug.Assert(ruwp.regCulture.Name.Equals("ru-RU"));
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking namespaces...");
		Debug.Assert(ruwp.namespaces[4].Contains("|Википедия|Project|"));
		Debug.Assert(ruwp.namespaces[7].Contains("|Обсуждение изображения|"));
		//ruwp.ShowNamespaces();
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking generalData...");
		Debug.Assert(ruwp.generalData["interwiki"].Contains("|de|"));
		Debug.Assert(ruwp.generalData["variables"].Contains("|localyear|"));
		Debug.Assert(ruwp.generalData["magicWords"].Contains("|ПАДЕЖ:|"));
		Debug.Assert(ruwp.generalData["redirectTags"].Contains("перенаправление"));
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking GetWikimediaProjects()...");
		Debug.Assert(ruwp.GetWikimediaProjects(true).Contains("ru.wikipedia.org"));
		Debug.Assert(!ruwp.GetWikimediaProjects(true).Contains("www.mediawiki.org"));
		Debug.Assert(ruwp.GetWikimediaProjects(false).Contains("www.mediawiki.org"));
		Console.WriteLine("    PASSED");
		
		Console.WriteLine("Checking LoadMediawikiMessages()...");
		var mediawikiMessages = ruwp.LoadMediawikiMessages(false);
			Debug.Assert(mediawikiMessages["abusefilter"] == "Настройка фильтра злоупотреблений");
		ruwp.LoadMediawikiMessages(true);
			Debug.Assert(ruwp.messages["abusefilter"] == "Фильтр правок");
			ruwp.useApi = false;
			ruwp.LoadMediawikiMessages(true);
			Debug.Assert(ruwp.messages["abusefilter"] == "Фильтр правок");
			ruwp.useApi = true;
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking Page loading...");
			p = new Page(ruwp, "Физика");		
		p.Load();
			Debug.Assert(p.text.Contains("Физика"));
			p.text = "";
		p.LoadWithMetadata();
			Debug.Assert(p.text.Contains("Физика"));
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking template parsing...");
			p = new Page(ruwp, "Физика");		
			p.Load();
		var templates = p.GetTemplates(false, false);
			Debug.Assert(templates.Contains("Разделы физики"));
			templates = p.GetTemplates(true, false);
			Debug.Assert(templates.Contains("Разделы физики|state=expanded"));
			templates = p.GetTemplateParameter("Разделы физики", "state");
			Debug.Assert(templates.Contains("expanded"));
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking disambiguation detection...");
			p = new Page(ruwp, "Физика");		
			p.Load();
		Debug.Assert(!p.IsDisambig());
		Debug.Assert(ruwp.disambig.Contains("Неоднозначность|"));
		Debug.Assert(ruwp.disambig.Contains("|Disambig|"));
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking GetInterLanguageLinks()...");
			p = new Page(ruwp, "Физика");		
			p.Load();
		var langLinks = p.GetInterLanguageLinks();
			Debug.Assert(langLinks.Contains("en:Physics"));
		Console.WriteLine("    PASSED");
		
		Console.WriteLine("Checking Page constructor...");
		p = new Page(ruwp, 1000);
			p.Load();
			p.GetTitle();
			p.ShowTitle();
			Debug.Assert(p.title == "Август");
		Console.WriteLine("    PASSED");
		
		Console.WriteLine("Checking Page's text parsing and manipulation functions...");
			p = new Page(ruwp, "Участник:CodeMonkBot/Тест-площадка");
			p.Load();
			string src = p.text;
		p.AddTemplate("{{Тест|парам}}");
			Debug.Assert(p.text.Contains("{{Тест|парам}}\n\n[[Категория:Википедия:Тест1|aaa]]"));
			p.text = src;
		p.AddToCategory("NewTest");
			Debug.Assert(p.text.Contains(
				"[[Категория:Википедия:Тест2|bbb]]\n[[Категория:NewTest]]"));
			p.text = src;
		pl = p.GetLinks();
			Debug.Assert(pl.Contains("ааа:ббб"));
			Debug.Assert(pl.Contains("Talk:WWW"));
			Debug.Assert(pl.Contains("1926"));
			Debug.Assert(pl.Contains(":Category:ААААААА ССССССССССС"));
			Debug.Assert(!pl.Contains("а - а"));
			Debug.Assert(!pl.Contains("en:Physics"));
			Debug.Assert(!pl.Contains("Файл:Example -- 234.jpeg"));
			Debug.Assert(!pl.Contains("Category:Википедия:Тест1"));
			pl.Clear();
			var l = p.GetAllLinks();
			Debug.Assert(l.Contains(":ааа:ббб"));
			Debug.Assert(l.Contains("Talk:WWW"));
			Debug.Assert(l.Contains("1926"));
			Debug.Assert(l.Contains(":Category:ААААААА ССССССССССС"));
			Debug.Assert(!l.Contains("а - а"));
			Debug.Assert(l.Contains("en:Physics"));
			Debug.Assert(l.Contains("Файл:Example -- 234.jpeg"));
			Debug.Assert(l.Contains("Category:Википедия:Тест1"));
			l.Clear();
			p = new Page(ruwp, "Марк Твен");
			p.Load();
		l = p.GetAllCategories();
			Debug.Assert(l.Contains("Категория:Детские писатели США"));
			Debug.Assert(l.Contains("Категория:Персоналии по алфавиту"));
			l.Clear();
			ruwp.useApi = false;
			l = p.GetAllCategories();
			Debug.Assert(l.Contains("Категория:Детские писатели США"));
			Debug.Assert(l.Contains("Категория:Персоналии по алфавиту"));
			l.Clear();
			ruwp.useApi = true;
			p = new Page(ruwp, "Участник:CodeMonkBot/Тест-площадка");
			p.Load();
		l = p.GetCategories(true, true);
			Debug.Assert(l.Contains("Категория:Википедия:Тест1|aaa"));
			Debug.Assert(l.Contains("Категория:Википедия:Тест2|bbb"));
			l.Clear();
		l = p.GetExternalLinks();
			Debug.Assert(l.Contains("http://example.com/somefile.html"));
			l.Clear();
		l = p.GetImages();
			Debug.Assert(l.Contains("Файл:Example - 123.JPG"));
			Debug.Assert(l.Contains("Файл:Example -- 234.jpeg"));
			l.Clear();
		Console.WriteLine("    PASSED");
		
		Console.WriteLine("Checking filling from categories...");
			pl = new PageList(ruwp);
		pl.FillFromCategory("История физики");
			Debug.Assert(pl.Contains("Пузырьковая камера"));
			Debug.Assert(!pl.Contains("Категория:Историки физики"));
			pl.Clear();
		pl.FillAllFromCategory("История физики");
			Debug.Assert(pl.Contains("Пузырьковая камера"));
			Debug.Assert(pl.Contains("Категория:Историки физики"));
			pl.Clear();
			ruwp.useApi = false;
			pl.FillAllFromCategory("История физики");
			Debug.Assert(pl.Contains("Пузырьковая камера"));
			Debug.Assert(pl.Contains("Категория:Историки физики"));
			pl.Clear();
			ruwp.useApi = true;
		pl.FillFromCategoryTree("История физики");
			Debug.Assert(pl.Contains("Мах, Эрнст"));
			Debug.Assert(!pl.Contains("Категория:Историки механики"));
			pl.Clear();
		pl.FillAllFromCategoryTree("История физики");
			Debug.Assert(pl.Contains("Мах, Эрнст"));
			Debug.Assert(pl.Contains("Категория:Историки механики"));
			pl.Clear();
		Console.WriteLine("    PASSED");
		
		Console.WriteLine("Checking PageList loading and dumping...");
			pl = new PageList(ruwp);
		pl.FillFromCategory("Категория:Историки механики");
			pl.Load();
			Debug.Assert(pl.Contains("Мах, Эрнст"));
			Debug.Assert(pl["Мах, Эрнст"].text.Contains("Эрнст"));
			pl.Clear();
		pl.FillFromCategory("Категория:Историки механики");
			pl.LoadWithMetadata();
			Debug.Assert(pl.Contains("Мах, Эрнст"));
			Debug.Assert(pl["Мах, Эрнст"].text.Contains("Эрнст"));
			File.Delete("Dumps" + Path.DirectorySeparatorChar + "test.xml");
			foreach (FileInfo f in new DirectoryInfo("Dumps").GetFiles("*.txt"))
				f.Delete();
		pl.SaveToFiles("Dumps");
        pl.SaveTitlesToFile("Dumps" + Path.DirectorySeparatorChar + "list.dat");
        pl.SaveXmlDumpToFile("Dumps" + Path.DirectorySeparatorChar + "test.xml");
			pl.Clear();
		pl.FillAndLoadFromFiles("Dumps");
			Debug.Assert(pl.Contains("Мах, Эрнст"));
			Debug.Assert(pl["Мах, Эрнст"].text.Contains("Эрнст"));
			pl.Clear();
		pl.FillAndLoadFromXmlDump("Dumps" + Path.DirectorySeparatorChar + "test.xml");
			Debug.Assert(pl.Contains("Мах, Эрнст"));
			Debug.Assert(pl["Мах, Эрнст"].text.Contains("Эрнст"));
			pl.Clear();
		pl.FillFromFile("Dumps" + Path.DirectorySeparatorChar + "list.dat");
			Debug.Assert(pl.Contains("Мах, Эрнст"));
			pl.Clear();
		Console.WriteLine("    PASSED");
		
		Console.WriteLine("Checking PageList filling functions...");
			pl = new PageList(ruwp);
		pl.FillFromPageLinks("Физика");
			Debug.Assert(pl.Contains("Файл:CollageFisica.jpg"));
			Debug.Assert(pl.Contains("гравитация"));
			Debug.Assert(!pl.Contains("b:Физика в конспектах"));
			pl.Clear();
		pl.FillFromAllPages("", 0, true, 100);
			Debug.Assert(pl.Contains("$ (значения)"));
			Debug.Assert(pl["$ (значения)"].IsDisambig());
			Debug.Assert(pl.Contains("'39"));
			pl.Clear();
		pl.FillFromAllPages("", 0, false, 100);
			Debug.Assert(pl.Contains("$ (значения)"));
			Debug.Assert(!pl.Contains("'39"));
			pl.Clear();
		pl.FillFromCustomLog("protect", "", "Заглавная страница", 100);
			Debug.Assert(pl.Contains("Заглавная страница"));
			pl.Clear();
			ruwp.useApi = false;
			pl.FillFromCustomLog("protect", "", "Заглавная страница", 100);
			Debug.Assert(pl.Contains("Заглавная страница"));
			pl.Clear();
			ruwp.useApi = true;
		pl.FillFromCustomSpecialPage("Listredirects", 100);
			Debug.Assert(pl.Contains("% (знак)"));
			pl.Clear();
			ruwp.useApi = false;
			pl.FillFromCustomSpecialPage("Listredirects", 100);
			Debug.Assert(pl.Contains("Факториал"));
			pl.Clear();
			ruwp.useApi = true;
		pl.FillFromSearchResults("Физика", 100);
			Debug.Assert(pl.Contains("Физика"));
			pl.Clear();
		pl.FillFromGoogleSearchResults("Физика", 100);
			Debug.Assert(pl.Contains("Физика"));
			pl.Clear();
		pl.FillFromLinksToPage("Физика");
			Debug.Assert(pl.Contains("Классическая механика"));
			pl.Clear();
		pl.FillFromPageHistory("Физика",100);
			Debug.Assert(pl.Contains("Физика"));
			pl.Clear();
		pl.FillFromPagesUsingImage("CollageFisica.jpg");
			Debug.Assert(pl.Contains("Физика"));
			pl.Clear();
			ruwp.useApi = false;
			pl.FillFromPagesUsingImage("CollageFisica.jpg");
			Debug.Assert(pl.Contains("Физика"));
			pl.Clear();
			ruwp.useApi = true;
		pl.FillFromTransclusionsOfPage("Портал:Физика/Избранная статья");
			Debug.Assert(pl.Contains("Портал:Физика"));
			pl.Clear();
			ruwp.useApi = false;
			pl.FillFromTransclusionsOfPage("Портал:Физика/Избранная статья");
			Debug.Assert(pl.Contains("Портал:Физика"));
			pl.Clear();
			ruwp.useApi = true;
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking watchlist manipulation...");
			pl = new PageList(ruwp);
		pl.FillFromWatchList();
			Debug.Assert(pl.Contains("Участник:CodeMonkBot/Тест-площадка"));
			pl.Clear();
			p = new Page(ruwp, "Участник:CodeMonkBot/Тест-площадка");
			p.Unwatch();
			pl.FillFromWatchList();
			Debug.Assert(!pl.Contains("Участник:CodeMonkBot/Тест-площадка"));
			pl.Clear();
			p.Watch();
			pl.FillFromWatchList();
			Debug.Assert(pl.Contains("Участник:CodeMonkBot/Тест-площадка"));
			pl.Clear();
		Console.WriteLine("    PASSED");

		if (testSaving) {
			Console.WriteLine("Checking Page saving...");
			p = new Page(ruwp, "Участник:CodeMonkBot/Тест-площадка");
			p.Load();
			Debug.Assert(p.text.Contains("тест"));
			string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
			p.text = Regex.Replace(p.text, "тест[\\d]+", "тест" + timestamp);
			p.Save("тест", true);
			ruwp.useApi = false;
			p.Load();    // purging doesn't work here, requires load via XML to get actual text
			ruwp.useApi = true;
			Debug.Assert(p.text.Contains("тест" + timestamp));
			ruwp.useApi = false;
			timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
			p.text = Regex.Replace(p.text, "тест[\\d]+", "тест" + timestamp);
			p.Save("test", true);
			p.Load();
			Debug.Assert(p.text.Contains("тест" + timestamp));
			ruwp.useApi = true;
			Console.WriteLine("    PASSED");
		}

		Console.WriteLine("Checking recent changes, watchlist changes and user contributions...");
			pl = new PageList(ruwp);
		pl.FillFromRecentChanges(false, false, false, false, false, false, 100, 3);
			if (testSaving)
				Debug.Assert(pl.Contains("Участник:CodeMonkBot/Тест-площадка"));
			else
				Debug.Assert(pl.Count() > 0);
			pl.Clear();
		pl.FillFromChangedWatchedPages();
			if (testSaving)
				Debug.Assert(pl.Contains("Участник:CodeMonkBot/Тест-площадка"));
			else
				Debug.Assert(pl.Count() > 0);
			pl.Clear();
		pl.FillFromUserContributions(ruwp.userName, 100);
			if (testSaving)
				Debug.Assert(pl.Contains("Участник:CodeMonkBot/Тест-площадка"));
			else
				Debug.Assert(pl.Count() > 0);
			pl.Clear();
		Console.WriteLine("    PASSED");

		Console.WriteLine("\nTesing time: " + (DateTime.Now - testingTime).ToString());
		Console.WriteLine("\n    END OF TEST SESSION");
	}


	/// <summary>
	/// Tests DotNetWikiBot functions on test.wikipedia.org site.
	/// </summary>
	public static void TestTestWikipedia(bool testSaving)
	{
		DateTime testingTime = DateTime.Now;

		if (testwp == null)
			testwp = new Site("https://test.wikipedia.org");
		testwp.retryTimes = 10;

		Page p;
		PageList pl;

		/**/

		Console.WriteLine("Checking Site variables...");
		Debug.Assert(testwp.address.Equals("https://test.wikipedia.org"));
		Debug.Assert(testwp.indexPath.Equals("https://test.wikipedia.org/w/index.php"));
		Debug.Assert(testwp.shortPath.Equals("/wiki/"));
		Debug.Assert(testwp.language.Equals("en"));
		Debug.Assert(testwp.name.Equals("Wikipedia"));
		Debug.Assert(testwp.capitalization.Equals("first-letter"));
		Debug.Assert(testwp.timeOffset.Equals("0"));
		Debug.Assert(testwp.software.Contains("MediaWiki"));
		Debug.Assert(testwp.version > new Version("1.20"));
		Debug.Assert(testwp.langCulture.Name.Equals("en"));
		Debug.Assert(testwp.regCulture.Name.Equals("en-US"));
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking namespaces...");
		Debug.Assert(testwp.namespaces[4].Contains("|Wikipedia|"));
		//testwp.ShowNamespaces();
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking generalData...");
		Debug.Assert(testwp.generalData["interwiki"].Contains("|de|"));
		Debug.Assert(testwp.generalData["variables"].Contains("|localyear|"));
		Debug.Assert(testwp.generalData["magicWords"].Contains("|GENDER:|"));
		Debug.Assert(testwp.generalData["redirectTags"].Contains("REDIRECT"));
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking GetWikimediaProjects()...");
		Debug.Assert(testwp.GetWikimediaProjects(true).Contains("ru.wikipedia.org"));
		Debug.Assert(!testwp.GetWikimediaProjects(true).Contains("www.mediawiki.org"));
		Debug.Assert(testwp.GetWikimediaProjects(false).Contains("www.mediawiki.org"));
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking LoadMediawikiMessages()...");
		var mediawikiMessages = testwp.LoadMediawikiMessages(false);
		Debug.Assert(mediawikiMessages["about"] == "About");
		testwp.LoadMediawikiMessages(true);
		Debug.Assert(testwp.messages["about"] == "About");
		testwp.messages.Clear();
		testwp.useApi = false;
		testwp.LoadMediawikiMessages(true);
		Debug.Assert(testwp.messages["about"] == "About");
		testwp.useApi = true;
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking Page loading...");
		p = new Page(testwp, "Main Page");
		p.Load();
		Debug.Assert(p.text.Contains("MediaWiki"));
		p.text = "";
		p.LoadWithMetadata();
		Debug.Assert(p.text.Contains("MediaWiki"));
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking disambiguation detection...");
		p = new Page(testwp, "Main Page");
		p.Load();
		try {
			Debug.Assert(!p.IsDisambig());
		}
		catch (ArgumentNullException) { }
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking Page constructor...");
		p = new Page(testwp, 1);
		p.Load();
		p.GetTitle();
		p.ShowTitle();
		Debug.Assert(p.title == "Main Page");
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking filling from categories...");
		pl = new PageList(testwp);
		pl.FillFromCategory("Category:!Requests");
		Debug.Assert(pl.Contains("Wikipedia:Requests/Permissions"));
		Debug.Assert(!pl.Contains("Category:Successful requests for permissions"));
		pl.Clear();
		pl.FillAllFromCategory("Category:!Requests");
		Debug.Assert(pl.Contains("Wikipedia:Requests/Permissions"));
		Debug.Assert(pl.Contains("Category:Successful requests for permissions"));
		pl.Clear();
		testwp.useApi = false;
		pl.FillAllFromCategory("Category:!Requests");
		Debug.Assert(pl.Contains("Wikipedia:Requests/Permissions"));
		Debug.Assert(pl.Contains("Category:Successful requests for permissions"));
		pl.Clear();
		testwp.useApi = true;
		pl.FillFromCategoryTree("Category:!Requests");
		Debug.Assert(pl.Contains("Wikipedia:Requests/Permissions/!Silent"));
		Debug.Assert(!pl.Contains("Category:Successful requests for permissions"));
		pl.Clear();
		pl.FillAllFromCategoryTree("Category:!Requests");
		Debug.Assert(pl.Contains("Wikipedia:Requests/Permissions/!Silent"));
		Debug.Assert(pl.Contains("Category:Successful requests for permissions"));
		pl.Clear();
		Console.WriteLine("    PASSED");
		
		Console.WriteLine("Checking PageList filling functions...");
		pl = new PageList(testwp);
		pl.FillFromPageLinks("Main Page");
		Debug.Assert(pl.Contains("Wikipedia:Configuration"));
		pl.Clear();
		pl.FillFromAllPages("M", 0, true, 100);
		Debug.Assert(pl.Contains("Main Page"));
		pl.Clear();
		pl.FillFromSearchResults("Main Page", 100);
		Debug.Assert(pl.Contains("Main Page"));
		pl.Clear();
		//pl.FillFromGoogleSearchResults("Main Page", 100);    // TestWiki is excluded from search
		//Debug.Assert(pl[0].title.Contains("Main Page"));
		//pl.Clear();
		pl.FillFromLinksToPage("Wikipedia:Requests/Tools");
		Debug.Assert(pl.Contains("Wikipedia:Requests"));
		pl.Clear();
		pl.FillFromPageHistory("Main Page", 100);
		Debug.Assert(pl.Contains("Main Page"));
		pl.Clear();
		Console.WriteLine("    PASSED");
		
		Console.WriteLine("Checking watchlist manipulation...");
		pl = new PageList(testwp);
		pl.FillFromWatchList();
		Debug.Assert(pl.Contains("User:CodeMonk/TestPage1"));
		pl.Clear();
		p = new Page(testwp, "User:CodeMonk/TestPage1");
		p.Unwatch();
		pl.FillFromWatchList();
		Debug.Assert(!pl.Contains("User:CodeMonk/TestPage1"));
		pl.Clear();
		p.Watch();
		pl.FillFromWatchList();
		Debug.Assert(pl.Contains("User:CodeMonk/TestPage1"));
		pl.Clear();
		testwp.useApi = false;
		p.Unwatch();
		pl.FillFromWatchList();
		Debug.Assert(!pl.Contains("User:CodeMonk/TestPage1"));
		pl.Clear();
		p.Watch();
		pl.FillFromWatchList();
		Debug.Assert(pl.Contains("User:CodeMonk/TestPage1"));
		pl.Clear();
		testwp.useApi = true;
		Console.WriteLine("    PASSED");
		
		if (testSaving) {
			Console.WriteLine("Checking Page saving...");
			p = new Page(testwp, "User:CodeMonk/TestPage1");
			p.Load();
			Debug.Assert(p.text.Contains("тест"));
			string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
			p.text = Regex.Replace(p.text, "тест[\\d]+", "тест" + timestamp);
			p.Save("тест", true);
			testwp.useApi = false;
			p.Load();    // purging doesn't work here, requires load via XML to get actual text
			testwp.useApi = true;
			Debug.Assert(p.text.Contains("тест" + timestamp));
			testwp.useApi = false;
			timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
			p.text = Regex.Replace(p.text, "тест[\\d]+", "тест" + timestamp);
			p.Save("test", true);
			p.Load();
			Debug.Assert(p.text.Contains("тест" + timestamp));
			testwp.useApi = true;
			Console.WriteLine("    PASSED");
		}
	
		Console.WriteLine("Checking recent changes, watchlist changes and user contributions...");
		pl = new PageList(testwp);
		pl.FillFromRecentChanges(false, false, false, false, false, false, 100, 3);
		if (testSaving)
			Debug.Assert(pl.Contains("User:CodeMonk/TestPage1"));
		else
			Debug.Assert(pl.Count() > 0);
		pl.Clear();
		pl.FillFromChangedWatchedPages();
		if (testSaving)
			Debug.Assert(pl.Contains("User:CodeMonk/TestPage1"));
		else
			Debug.Assert(pl.Count() > 0);
		pl.Clear();
		pl.FillFromUserContributions(testwp.userName, 100);
		if (testSaving)
			Debug.Assert(pl.Contains("User:CodeMonk/TestPage1"));
		else
			Debug.Assert(pl.Count() > 0);
		pl.Clear();
		Console.WriteLine("    PASSED");

        Console.WriteLine("Checking renaming and deleting...");
        p = new Page(testwp, "User:CodeMonk/TestPage3");
        p.Save("test", "testing", true);
        p.Load();
        Debug.Assert(p.text.Contains("test"));
        p.RenameTo("User:CodeMonk/TestPage4", "testing");
        p = new Page(testwp, "User:CodeMonk/TestPage4");
        p.Load();
        Debug.Assert(p.text.Contains("test"));
        p.Delete("testing");
        p = new Page(testwp, "User:CodeMonk/TestPage3");
        try
        {
            p.Load();
        }
        catch { }
        Debug.Assert(string.IsNullOrEmpty(p.text) || p.IsRedirect());
        Console.WriteLine("    PASSED");
		
        Console.WriteLine("Testing image uploading...");
		//testwp.messages.Clear();    // unmodified messages could be loaded at this point
        testwp.messages = null;    // reloading is required
        p = new Page(testwp, "File:TestImage.png");
        p.UploadImage("Dumps" + Path.DirectorySeparatorChar + "TestImage.png",
            "test", "test", "test", "test");
        p = new Page(testwp, "File:TestImage.png");
        p.Load();
        Debug.Assert(p.text.Contains("test"));
        testwp.useApi = false;
        p.Delete("test");
        testwp.useApi = true;
        p = new Page(testwp, "File:TestImage.png");
		p.UploadImageFromWeb("http://upload.wikimedia.org/wikipedia/test/b/bc/Wiki.png",
			"test", "test", "test");
        p = new Page(testwp, "File:TestImage.png");
        p.Load();
        Debug.Assert(p.text.Contains("test"));
        p.DownloadImage("Dumps" + Path.DirectorySeparatorChar + "TestImage2.png");
        Debug.Assert(File.Exists("Dumps" + Path.DirectorySeparatorChar + "TestImage2.png"));
        File.Delete("Dumps" + Path.DirectorySeparatorChar + "TestImage2.png");
        p.Delete("test");
        Console.WriteLine("    PASSED");
		
        Console.WriteLine("Testing page revertion...");
        p = new Page(testwp, "User:CodeMonk/TestPage5");
        p.Save("test1", "test", true);
        p.Save("test2", "test", true);
        p.Revert("test", true);
        p.Load();
        Debug.Assert(p.text == "test1");
        // Methods Revert() and UndoLastEdits() use Save() internally, no hard testing is required
        Console.WriteLine("    PASSED");

        Console.WriteLine("Testing page protection...");
        p = new Page(testwp, "User:CodeMonk/TestPage5");
        p.Load();
        p.Protect(2, 2, false, DateTime.Now.AddHours(3), "testing");
        string src = testwp.GetWebPage(testwp.indexPath + "?title=" +
			Uri.EscapeDataString(p.title));
        Debug.Assert(src.Contains("action=unprotect"));
        Debug.Assert(!src.Contains("action=protect"));
        p.Protect(0, 0, false, DateTime.Now.AddHours(3), "testing");
		src = testwp.GetWebPage(testwp.indexPath + "?title=" + Uri.EscapeDataString(p.title));
        Debug.Assert(!src.Contains("action=unprotect"));
        Debug.Assert(src.Contains("action=protect"));
        Console.WriteLine("    PASSED");

        Console.WriteLine("\nTesing time: " + (DateTime.Now - testingTime).ToString());
        Console.WriteLine("\n    END OF TEST SESSION");
	}

	/// <summary>
	/// Tests DotNetWikiBot functions on specially created Wikia site.
	/// </summary>
	public static void TestWikiaWikibotSite(bool testSaving)
	{
		DateTime testingTime = DateTime.Now;

		if (wikiaSite == null)
			wikiaSite = new Site("http://wikibot.wikia.com");
		wikiaSite.retryTimes = 10;

		Page p;
		PageList pl;
		/**/
		Console.WriteLine("Checking Site variables...");
		Debug.Assert(wikiaSite.address.Equals("http://wikibot.wikia.com"));
		Debug.Assert(wikiaSite.indexPath.Equals("http://wikibot.wikia.com/index.php"));
		Debug.Assert(wikiaSite.shortPath == "/wiki/");
		Debug.Assert(wikiaSite.language.Equals("en"));
		Debug.Assert(wikiaSite.name.Equals("Bot Wiki"));
		Debug.Assert(wikiaSite.capitalization.Equals("first-letter"));
		Debug.Assert(wikiaSite.timeOffset.Equals("0"));
		Debug.Assert(wikiaSite.software.Contains("MediaWiki"));
		Debug.Assert(wikiaSite.version >= new Version("1.19"));
		Debug.Assert(wikiaSite.langCulture.Name.Equals("en"));
		Debug.Assert(wikiaSite.regCulture.Name.Equals("en-US"));
		Console.WriteLine("    PASSED");

		if (!testSaving)
			return;

		Console.WriteLine("Checking Page saving...");
		p = new Page(wikiaSite, "User:CodeMonk/Test");
		p.Load();
		Debug.Assert(p.text.Contains("test"));
		string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
		p.text = Regex.Replace(p.text, "test[\\d]+", "test" + timestamp);
		p.Save("test", true);
		wikiaSite.useApi = false;
		p.Load();    // purging doesn't work here, requires load via XML to get actual text
		wikiaSite.useApi = true;
		Debug.Assert(p.text.Contains("test" + timestamp));
		wikiaSite.useApi = false;
		timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
		p.text = Regex.Replace(p.text, "test[\\d]+", "test" + timestamp);
		p.Save("test", true);
		p.Load();
		Debug.Assert(p.text.Contains("test" + timestamp));
		wikiaSite.useApi = true;
		Console.WriteLine("    PASSED");

		// Deleting requires admin rights
		Console.WriteLine("Checking renaming and deleting...");
		p = new Page(wikiaSite, "User:CodeMonk/Test2");
		p.Save("test", "testing", true);
		p.Load();
		Debug.Assert(p.text.Contains("test"));
		p.RenameTo("User:CodeMonk/Test4", "testing");
		p = new Page(wikiaSite, "User:CodeMonk/Test4");
		p.Load();
		Debug.Assert(p.text.Contains("test"));
		p.Delete("testing");
		p = new Page(wikiaSite, "User:CodeMonk/Test4");
		try {
		    p.Load();
		}
		catch { }
		Debug.Assert(string.IsNullOrEmpty(p.text));
		Console.WriteLine("    PASSED");
		
		Console.WriteLine("Testing image uploading...");
		p = new Page(wikiaSite, "File:TestImage1.png");
		p.UploadImage("Dumps" + Path.DirectorySeparatorChar + "TestImage.png",
			"test", "test", "test", "test");
		p = new Page(wikiaSite, "File:TestImage1.png");
		p.Load();
		Debug.Assert(p.text.Contains("test"));
		p.Delete("test");
		p = new Page(wikiaSite, "File:TestImage2.png");
		p.UploadImageFromWeb("http://img1.wikia.nocookie.net/__cb64/admintools/images/8/89/" +
			"Wiki-wordmark.png", "test", "test", "test");
		p = new Page(wikiaSite, "File:TestImage2.png");
		p.Load();
		Debug.Assert(p.text.Contains("test"));
		p.DownloadImage("Dumps" + Path.DirectorySeparatorChar + "TestImage2.png");
		Debug.Assert(File.Exists("Dumps" + Path.DirectorySeparatorChar + "TestImage2.png"));
		File.Delete("Dumps" + Path.DirectorySeparatorChar + "TestImage2.png");
		p.Delete("test");
		Console.WriteLine("    PASSED");

		// Page revertion testing
		// TO DO

		// Protection requires admin rights
		Console.WriteLine("Testing page protection...");
		p = new Page(wikiaSite, "User:CodeMonk/Test3");
		p.Load();
		p.Protect(2, 2, false, DateTime.Now.AddHours(3), "testing");
		string src = wikiaSite.GetWebPage(wikiaSite.indexPath + "?title=" +
			Uri.EscapeDataString(p.title));
		Debug.Assert(src.Contains("action=unprotect"));
		Debug.Assert(!src.Contains("action=protect"));
		p.Protect(0, 0, false, DateTime.Now.AddHours(3), "testing");
		src = wikiaSite.GetWebPage(wikiaSite.indexPath + "?title=" +
			Uri.EscapeDataString(p.title));
		Debug.Assert(!src.Contains("action=unprotect"));
		Debug.Assert(src.Contains("action=protect"));
		Console.WriteLine("    PASSED");

		Console.WriteLine("\nTesing time: " + (DateTime.Now - testingTime).ToString());
		Console.WriteLine("\n    END OF TEST SESSION");
	}

	/// <summary>
	/// Tests DotNetWikiBot functions on AdminTools Wikia site.
	/// </summary>
	public static void TestWikiaAdminTools(bool testSaving)
	{
		DateTime testingTime = DateTime.Now;

		if (wikiaTools == null)
			wikiaTools = new Site("http://admintools.wikia.com");
		wikiaTools.retryTimes = 10;

		Page p;
		PageList pl;
		/**/
		Console.WriteLine("Checking Site variables...");
		Debug.Assert(wikiaTools.address.Equals("http://admintools.wikia.com"));
		Debug.Assert(wikiaTools.indexPath.Equals("http://admintools.wikia.com/index.php"));
		Debug.Assert(wikiaTools.shortPath == "/wiki/");
		Debug.Assert(wikiaTools.language.Equals("en"));
		Debug.Assert(wikiaTools.name.Equals("Admin Tools Wiki"));
		Debug.Assert(wikiaTools.capitalization.Equals("first-letter"));
		Debug.Assert(wikiaTools.timeOffset.Equals("0"));
		Debug.Assert(wikiaTools.software.Contains("MediaWiki"));
		Debug.Assert(wikiaTools.version >= new Version("1.19"));
		Debug.Assert(wikiaTools.langCulture.Name.Equals("en"));
		Debug.Assert(wikiaTools.regCulture.Name.Equals("en-US"));
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking namespaces...");
		Debug.Assert(wikiaTools.namespaces[4].Contains("|Project|ATW|"));
		Debug.Assert(wikiaTools.namespaces[7].Contains("|File talk|"));
		//wikiaTools.ShowNamespaces();
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking generalData...");
		Debug.Assert(wikiaTools.generalData["interwiki"].Contains("|commons|"));
		//Debug.Assert(wikiaTools.generalData["variables"].Contains("|localyear|"));
			// generalData["variables"] is not available in MW 1.19
		Debug.Assert(wikiaTools.generalData["magicWords"].Contains("|CURRENTDAY|"));
		Debug.Assert(wikiaTools.generalData["redirectTags"].Contains("REDIRECT"));
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking LoadMediawikiMessages()...");
		wikiaTools.LoadMediawikiMessages(true);
		Debug.Assert(wikiaTools.messages["about"] == "About");
		// TO DO: "Special:AllMessages" testing
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking Page loading...");
		p = new Page(wikiaTools, "Admin_Tools_Wiki");
		p.Load();
		Debug.Assert(p.text.Contains("Admin Tools Wiki"));
		p.text = "";
		p.LoadWithMetadata();
		Debug.Assert(p.text.Contains("Admin Tools Wiki"));
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking Page constructor...");
		p = new Page(wikiaTools, 42241);
		p.Load();
		p.ShowTitle();
		Debug.Assert(p.title == "User:CodeMonk");
		Console.WriteLine("    PASSED");

		Console.WriteLine("Checking filling from categories...");
		pl = new PageList(wikiaTools);
		pl.FillFromCategory("Pages not to be tested on");
		Debug.Assert(pl.Contains("Admin Tools Wiki:About"));
		Debug.Assert(!pl.Contains("Category:Admin Tools Wiki adminship"));
		pl.Clear();
		pl.FillAllFromCategory("Pages not to be tested on");
		Debug.Assert(pl.Contains("Admin Tools Wiki:About"));
        Debug.Assert(pl.Contains("Category:Admin Tools Wiki adminship"));
		pl.Clear();
		// Non-API way in FillAllFromCategory() is not required to be tested on MW 1.19
		pl.FillFromCategoryTree("Admin Tools Wiki project pages");
        Debug.Assert(pl.Contains("Admin Tools Wiki:Administrators' Guide"));
        Debug.Assert(!pl.Contains("Category:Admin Tools Wiki adminship"));
		pl.Clear();
		pl.FillAllFromCategoryTree("Admin Tools Wiki project pages");
		Debug.Assert(pl.Contains("Admin Tools Wiki:Administrators' Guide"));
        Debug.Assert(pl.Contains("Category:Admin Tools Wiki adminship"));
		pl.Clear();
		Console.WriteLine("    PASSED");

		// PageList loading and dumping check skipped

		Console.WriteLine("Checking PageList filling functions...");
		pl = new PageList(wikiaTools);
		pl.FillFromPageLinks("Admin Tools Wiki");
		Debug.Assert(pl.Contains("Admin Tools Wiki:About"));
		pl.Clear();
		pl.FillFromAllPages("Admin", 0, true, 100);
		Debug.Assert(pl.Contains("Admin Tools Wiki"));
		Debug.Assert(pl.Contains("Main Page"));
		pl.Clear();
		pl.FillFromAllPages("", 0, false, 100);
		Debug.Assert(pl.Contains("Admin Tools Wiki"));
		Debug.Assert(!pl.Contains("Main Page"));
		pl.Clear();
		pl.FillFromCustomLog("protect", "", "Admin Tools Wiki", 100);
		Debug.Assert(pl.Contains("Admin Tools Wiki"));
		pl.Clear();
		pl.FillFromCustomSpecialPage("Listredirects", 100);
		Debug.Assert(pl.Contains("ATW:A"));
		pl.Clear();
        //pl.FillFromSearchResults("Admin Tools Wiki", 100);    // text search is disabled on Wikia
		//Debug.Assert(pl.Contains("Admin Tools Wiki"));
		//pl.Clear();
		pl.FillFromGoogleSearchResults("Admin Tools Wiki", 100);
        Debug.Assert(pl.Contains("Admin Tools Wiki"));
		pl.Clear();
		pl.FillFromLinksToPage("Admin Tools Wiki");
		Debug.Assert(pl.Contains("Main Page"));
		pl.Clear();
		pl.FillFromPageHistory("Admin Tools Wiki", 100);
		Debug.Assert(pl.Contains("Admin Tools Wiki"));
		pl.Clear();
		// FillFromTransclusionsOfPage() is not required to be tested on MW 1.19
		Console.WriteLine("    PASSED");
		
		Console.WriteLine("Checking watchlist manipulation...");
		pl = new PageList(wikiaTools);
		pl.FillFromWatchList();
		Debug.Assert(pl.Contains("User:CodeMonk/Test1"));
		pl.Clear();
		p = new Page(wikiaTools, "User:CodeMonk/Test1");
		p.Unwatch();
		pl.FillFromWatchList();
		Debug.Assert(!pl.Contains("User:CodeMonk/Test1"));
		pl.Clear();
		p.Watch();
		pl.FillFromWatchList();
		Debug.Assert(pl.Contains("User:CodeMonk/Test1"));
		pl.Clear();
		Console.WriteLine("    PASSED");
		
		if (testSaving) {
			Console.WriteLine("Checking Page saving...");
			p = new Page(wikiaTools, "User:CodeMonk/Test");
			p.Load();
			Debug.Assert(p.text.Contains("test"));
			string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
			p.text = Regex.Replace(p.text, "test[\\d]+", "test" + timestamp);
			p.Save("test", true);
			wikiaTools.useApi = false;
			Thread.Sleep(5 * 1000);
			p.Load();    // purging doesn't work here, requires load via XML to get actual text
			wikiaTools.useApi = true;
			Debug.Assert(p.text.Contains("test" + timestamp));
			wikiaTools.useApi = false;
			timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
			p.text = Regex.Replace(p.text, "test[\\d]+", "test" + timestamp);
			p.Save("test", true);
			Thread.Sleep(5 * 1000);
			p.Load();
			Debug.Assert(p.text.Contains("test" + timestamp));
			wikiaTools.useApi = true;
			Console.WriteLine("    PASSED");
		}

		Console.WriteLine("Checking recent changes, watchlist changes and user contributions...");
		pl = new PageList(wikiaTools);
		pl.FillFromRecentChanges(false, false, false, false, false, false, 100, 3);
		if (testSaving)
			Debug.Assert(pl.Contains("User:CodeMonk/Test"));
		else
			Debug.Assert(pl.Count() > 0);
		pl.Clear();
		pl.FillFromChangedWatchedPages();
		if (testSaving)
			Debug.Assert(pl.Contains("User:CodeMonk/Test"));
		else
			Debug.Assert(pl.Count() > 0);
		pl.Clear();
		pl.FillFromUserContributions(wikiaTools.userName, 100);
		if (testSaving)
			Debug.Assert(pl.Contains("User:CodeMonk/Test"));
		else
			Debug.Assert(pl.Count() > 0);
		pl.Clear();
		Console.WriteLine("    PASSED");

		// Deletion requires admin rights
		//Console.WriteLine("Checking renaming and deleting...");
		//p = new Page(wikiaTools, "User:CodeMonk/Test2");
		//p.Save("test", "testing", true);
		//p.Load();
		//Debug.Assert(p.text.Contains("test"));
		//p.RenameTo("Test4", "testing");
		//p = new Page(wikiaTools, "User:CodeMonk/Test4");
		//p.Load();
		//Debug.Assert(p.text.Contains("test"));
		//p.Delete("testing");
		//p = new Page(wikiaTools, "Test4");
		//try {
		//    p.Load();
		//}
		//catch { }
		//Debug.Assert(string.IsNullOrEmpty(p.text));
		//Console.WriteLine("    PASSED");

		Console.WriteLine("Testing image uploading...");
		p = new Page(wikiaTools, "File:TestImage1.png");
		p.UploadImage("Dumps" + Path.DirectorySeparatorChar + "TestImage.png",
			"test", "test", "test", "test");
		p = new Page(wikiaTools, "File:TestImage1.png");
		p.Load();
		Debug.Assert(p.text.Contains("test"));
		//p.Delete("test");
		p = new Page(wikiaTools, "File:TestImage2.png");
		p.UploadImageFromWeb("http://img1.wikia.nocookie.net/__cb64/admintools/images/8/89/" +
			"Wiki-wordmark.png", "test", "test", "test");
		p = new Page(wikiaTools, "File:TestImage2.png");
		p.Load();
		Debug.Assert(p.text.Contains("test"));
		p.DownloadImage("Dumps" + Path.DirectorySeparatorChar + "TestImage2.png");
		Debug.Assert(File.Exists("Dumps" + Path.DirectorySeparatorChar + "TestImage2.png"));
		File.Delete("Dumps" + Path.DirectorySeparatorChar + "TestImage2.png");
		//p.Delete("test");
		Console.WriteLine("    PASSED");

		// Page revertion testing omitted

        // Protection requires admin rights
		//Console.WriteLine("Testing page protection...");
		//p = new Page(wikiaTools, "User:CodeMonk/Test3");
		//p.Load();
		//p.Protect(2, 2, false, DateTime.Now.AddHours(3), "testing");
		//string src = wikiaTools.GetWebPage(wikiaTools.indexPath + "?title=" +
			//Bot.UrlEncode(p.title));
		//Debug.Assert(src.Contains("action=unprotect"));
		//Debug.Assert(!src.Contains("action=protect"));
		//p.Protect(0, 0, false, DateTime.Now.AddHours(3), "testing");
		//src = wikiaTools.GetWebPage(wikiaTools.indexPath + "?title=" +
			//Bot.UrlEncode(p.title));
		//Debug.Assert(!src.Contains("action=unprotect"));
		//Debug.Assert(src.Contains("action=protect"));
		//Console.WriteLine("    PASSED");

		Console.WriteLine("\nTesing time: " + (DateTime.Now - testingTime).ToString());
		Console.WriteLine("\n    END OF TEST SESSION");
	}

	/// <summary>
	/// Custom tests for experimenting and bugs investigation.
	/// </summary>
	public static void CustomTest()
	{
		testwp = new Site("https://he.wikipedia.org");
		Page p = new Page(testwp, "עמוד_ראשי");
		p.ResolveRedirect();
		p.Load();
		p.ShowTitle();
		return;
		/*
		//testwp.useApi = false;
		//var x = testwp.GetApiQueryResult("list=users", "usprop=groups&ususers=CodeMonk", int.MaxValue);
		string src = testwp.GetWebPage(testwp.apiPath +
			"?format=xml&action=query&list=users&usprop=groups&ususers=CodeMonk");
		List<string> groups = (from g in XElement.Parse(src).Descendants("g")
							   where g.Value != "*"
							   select g.Value).ToList();
		return;
		Page p = new Page(testwp, "Douglas Adams");
		p.Load();
		XElement wikidataItem = p.GetWikidataItem();
		string description = (from desc in wikidataItem.Descendants("description")
							  where desc.Attribute("language").Value == "en"
							  select desc.Attribute("value").Value).FirstOrDefault();

		//PageList pl = new PageList();
		//pl.FillFromPageHistory("Main Page", 10);
		*/
		/*
		Console.WriteLine("Checking Page saving...");
		Page p = new Page(testwp, "User:CodeMonk/TestPage1");
		p.Load();
		Debug.Assert(p.text.Contains("тест"));
		string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
		p.text = Regex.Replace(p.text, "тест[\\d]+", "тест" + timestamp);
		p.Save("тест", true);
		testwp.useApi = false;
		p.Load();    // purging doesn't work here, requires load via XML to get actual text
		testwp.useApi = true;
		Debug.Assert(p.text.Contains("тест" + timestamp));
		Console.WriteLine("    PASSED");
		*/
	}
}