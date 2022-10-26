// DotNetWikiBot Framework v1.4 - bot framework based on Microsoft® .NET Framework 2.0 for wiki projects
// Distributed under the terms of the MIT (X11) license: http://www.opensource.org/licenses/mit-license.php
// Copyright © Iaroslav Vassiliev (2006-2007) codedriller@gmail.com

using System;
using System.IO;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using System.Xml.XPath;
using System.Web;

namespace DotNetWikiBot
{	
	/// <summary>Class defines wiki site object.</summary>
	public class Site
	{
		/// <summary>Site URL.</summary>
		public readonly string site = "http://en.wikipedia.org";
		/// <summary>Default user name to login with.</summary>
		private string userName = "YourBotLogin";
		/// <summary>Default password to login with.</summary>
		private string userPass = "YourBotPassword";
		/// <summary>Site title.</summary>
		public string name;
		/// <summary>MediaWiki version.</summary>
		public string generator;
		/// <summary>Rule of page title capitalization.</summary>
		public string capitalization;
		/// <summary>Short relative path to wiki pages (if such an alias is set on server ).</summary>
		public string wikiPath = "/wiki/";
		/// <summary>Relative path to "index.php" file on server.</summary>
		public string indexPath = "/w/";
		/// <summary>PageList, containing all MediaWiki messages of the site.</summary>
		public PageList messages;
		/// <summary>Regular expression to find redirect target.</summary>
		public Regex redirectRE;
		/// <summary>Regular expression to find links to pages in list in HTML source.</summary>
		public Regex linkToPageRE1 = new Regex("<li><a href=\"[^\"]*?\" title=\"([^\"]+?)\">");
		/// <summary>Alternative regular expression to find links to pages in HTML source.</summary>
		public Regex linkToPageRE2 = new Regex("<a href=\"[^\"]*?\" title=\"([^\"]+?)\">\\1</a>");
		/// <summary>Alternative regular expression to find links to pages in HTML source.</summary>
		public Regex linkToPageRE3;		
		/// <summary>Regular expression to find titles in markup.</summary>
		public Regex pageTitleTagRE = new Regex("<title>(.+?)</title>");
		/// <summary>Regular expression to find internal wiki links in markup.</summary>
		public Regex wikiLinkRE = new Regex(@"\[\[(.+?)(\|.+?)?]]");
		/// <summary>Regular expression to find wiki category links.</summary>
		public Regex wikiCategoryRE;
		/// <summary>Regular expression to find wiki templates in markup.</summary>
		public Regex wikiTemplateRE = new Regex(@"\{\{(.+?)(\|.*?)*?}}");
		/// <summary>Regular expression to find embedded images in wiki markup.</summary>
		public Regex wikiImageRE;
		/// <summary>Regular expression to find links to sister wiki projects in markup.</summary>
		public Regex sisterWikiLinkRE;
		/// <summary>Regular expression to find interwiki links in wiki markup.</summary>
		public Regex iwikiLinkRE;
		/// <summary>Regular expression to find displayed interwiki links in wiki markup,
		/// like "[[:de:...]]".</summary>
		public Regex iwikiDispLinkRE;
		/// <summary>Regular expression to find external web links in wiki markup.</summary>
		public Regex webLinkRE = new Regex("(https?|t?ftp|news|nntp|telnet|irc|gopher)" +
			"://([^\\s'\"<>]+)");
		/// <summary>A template for disambigation page.</summary>
		public string disambigStr = "{{disambig}}";
		/// <summary>Regular expression to extract language code from site URL.</summary>
		public Regex siteLangRE = new Regex(@"http://(.*?)\.(.+?\..+)");
		/// <summary>Regular expression to extract edit session time attribute.</summary>
		public Regex editSessionTimeRE = new Regex("value=\"([^\"]*?)\" name=\"wpEdittime\"");
		/// <summary>Regular expression to extract edit session token attribute.</summary>
		public Regex editSessionTokenRE1 = new Regex("value=\"([^\"]*?)\" name=\"wpEditToken\"");
		/// <summary>Alternative regular expression to extract edit session token attribute.</summary>
		public Regex editSessionTokenRE2 = new Regex("name='wpEditToken' value=\"([^\"]*?)\"");
		/// <summary>Site cookies.</summary>
		public CookieContainer cookies = new CookieContainer();			
		/// <summary>Local namespaces.</summary>
		public Hashtable namespaces = new Hashtable();
		/// <summary>Default namespaces.</summary>
		public static Hashtable wikiNSpaces = new Hashtable();
		/// <summary>List of Wikimedia Foundation sites and prefixes.</summary>
		public static Hashtable WMSites = new Hashtable();
		/// <summary>Built-in variables of MediaWiki software
		/// (see http://meta.wikimedia.org/wiki/Help:Magic_words).</summary>
		public static string[] mediaWikiVars;
		/// <summary>Built-in parser functions of MediaWiki software
		/// (see http://meta.wikimedia.org/wiki/Help:Magic_words).</summary>
		public static string[] parserFunctions;			
		/// <summary>Built-in template Modifiers of MediaWiki software
		/// (see http://meta.wikimedia.org/wiki/Help:Magic_words).</summary>
		public static string[] templateModifiers;			
		/// <summary>Wikimedia Foundation sites and prefixes in one string.</summary>
		public static string WMSitesStr;
		/// <summary>ISO 639-1:2002 language codes, used as prefixes to identify Wikimedia
		/// Foundation sites, gathered in one string with "|" as separator.</summary>		
		public static string WMLangsStr;		
		/// <summary>Availability of "query.php" MediaWiki extension (bot interface).</summary>
		public bool botQuery = false;
		/// <summary>Site language.</summary>
		public string language;
		/// <summary>Site encoding.</summary>
		public Encoding encoding = Encoding.UTF8;
 
		/// <summary>This constructor is used to generate most Site objects.</summary>
		/// <param name="site">Site URI like "http://en.wikipedia.org".</param>
		/// <param name="userName">User name to log in.</param>
		/// <param name="userPass">Password.</param>
		/// <returns>Returns Site object.</returns>
    	public Site(string site, string userName, string userPass)
    	{
	    	this.site = site;
	    	this.userName = userName;
	    	this.userPass = userPass;
	    	GetPaths();
	    	GetMediaWikiMessages(false);
	    	LoadDefaults();
	    	LogIn();
	    	GetInfo();				    	
    	}

    	/// <summary>This constructor uses default site, userName and password.</summary>		
    	public Site()
    	{
	    	GetPaths();	    	
	    	GetMediaWikiMessages(false);
	    	LoadDefaults();
	    	LogIn();
	    	GetInfo();	    	
    	}
    	
    	/// <summary>Gets path to "index.php" and path to pages and saves it to file.</summary>
    	public void GetPaths()
    	{
	    	string filePathName = "Cache\\" + HttpUtility.UrlEncode(site) + ".dat";
	    	if (File.Exists(filePathName) == true) {
		    	string[] lines = File.ReadAllLines(filePathName, Encoding.UTF8);
		    	if (lines.GetUpperBound(0) + 1 == 3) {
			    	wikiPath = lines[0];
			    	indexPath = lines[1];
			    	language = lines[2];  		    	
	    			return;
    			}
    		}
	    	Console.WriteLine("Logging in...");
			Regex wikiPathRE = new Regex(site + @"(/.+?/).+");
			Regex indexPathRE = new Regex("(?i)href=\"(/[^\"\\s<>?]*?)index\\.php\\?");
			Regex languageRE = new Regex("(?i)xml:lang=\"(.+?)\" lang=\"(.+?)\"");
			HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(site);
            webReq.Proxy.Credentials = CredentialCache.DefaultCredentials;
	        webReq.ContentType = Bot.webContentType;
            webReq.UserAgent = Bot.botVer;
			HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse();
			wikiPath = wikiPathRE.Match(webResp.ResponseUri.ToString()).Groups[1].ToString();
			webResp.Close();
			Uri testPage = new Uri(site + indexPath + "index.php?title=" +
				DateTime.Now.Ticks.ToString("x"));
		    Bot.InitWebClient();
		    string src = Bot.wc.DownloadString(testPage);
			indexPath = indexPathRE.Match(src).Groups[1].ToString();
			language = languageRE.Match(src).Groups[1].ToString();
			Directory.CreateDirectory("Cache");
			File.WriteAllText(filePathName, wikiPath + "\r\n" + indexPath + "\r\n" + language,
				Encoding.UTF8);
    	}
    
    	/// <summary>Gets all MediaWiki messages and dumps it to XML file.</summary>
    	/// <param name="forceLoad">If true, the messages are forced to be updated.</param>
    	public void GetMediaWikiMessages(bool forceLoad)
    	{
	    	if (forceLoad == false)
	    		messages = new PageList(this);
	    	string filePathName = "Cache\\" + HttpUtility.UrlEncode(site) + ".xml";
	    	if (forceLoad == false && File.Exists(filePathName) &&
	    		(DateTime.Now - File.GetLastWriteTime(filePathName)).Days  <= 90) {
	    		messages.FillAndLoadFromXMLDump(filePathName);
		    	return;
	    	}
	    	Console.WriteLine("Updating MediaWiki messages dump. Please, wait...");
	    	PageList pl = new PageList(this);
	    	pl.FillFromAllPages("!", 8, false, 100000);
	    	File.Delete(filePathName);
	    	pl.SaveXMLDumpToFile(filePathName);
	    	Console.WriteLine("MediaWiki messages dump updated successfully.");	    	
	    	messages.FillAndLoadFromXMLDump(filePathName);
    	}
    	
    	/// <summary>Retrieves metadata and local namespace names from site.</summary>
    	public void GetInfo()
    	{
	    	Uri res = new Uri(site + indexPath + "index.php?title=Special:Export/" +
	    		DateTime.Now.Ticks.ToString("x"));
		    Bot.InitWebClient();
		    XmlTextReader reader = new XmlTextReader(Bot.wc.OpenRead(res));
		    reader.WhitespaceHandling = WhitespaceHandling.None;
		    reader.ReadToFollowing("sitename");
		    name = reader.ReadString();
		    reader.ReadToFollowing("generator");
		    generator = reader.ReadString();
		    reader.ReadToFollowing("case");
		    capitalization = reader.ReadString();
		    namespaces.Clear();		   		    
	  		while (reader.ReadToFollowing("namespace"))
		  		namespaces.Add(reader.GetAttribute("key"), reader.ReadString());
		  	namespaces.Remove("0");
		  	reader.Close();
		  	wikiCategoryRE = new Regex(@"\[\[(?i)(((" + wikiNSpaces["14"] + "|" +
		  		namespaces["14"] + @"):(.+?))(\|(.+?))?)]]");
		  	wikiImageRE = new Regex(@"\[\[(?i)((" + wikiNSpaces["6"] + "|" +
		  		namespaces["6"] + @"):(.+?))(\|(.+?))*?]]");
			string namespacesStr = "";		  				  	 
	    	foreach (DictionaryEntry ns in namespaces)    		
	    		namespacesStr += ns.Value + "|";
	    	namespacesStr = namespacesStr.Replace("||", "|").Trim("|".ToCharArray());    				  	
			linkToPageRE3 = new Regex("<a href=\"[^\"]*?\" title=\"((" + namespacesStr +
				")?:?([^\"]+?))\">([^<]*?)</a>\\) ?.?<a ([^>]*?)>\\3</a>");
		  	string redirectTag = "REDIRECT";
			switch(language) {
				case "ar": redirectTag += "|?????"; break;
				case "be": redirectTag += "|перанакіраваньне"; break;
				case "bg": redirectTag += "|виж"; break;
				case "bs": redirectTag += "|preusmjeri"; break;
				case "cy": redirectTag += "|ail-cyfeirio"; break;
				case "et": redirectTag += "|suuna"; break;
				case "eu": redirectTag += "|bidali"; break;
				case "ga": redirectTag += "|athsheoladh"; break;
				case "he": redirectTag += "|?????"; break;
				case "id": redirectTag += "|redirected"; break;
				case "is": redirectTag += "|tilv?sun"; break;
				case "nn": redirectTag += "|omdiriger"; break;
				case "ru": redirectTag += "|перенапр|перенаправление"; break;
				case "sk": redirectTag += "|presmeruj"; break;
				case "sr": redirectTag += "|преусмери"; break;
				case "tt": redirectTag += "|y?n?lt?"; break;
				default: redirectTag = "REDIRECT"; break;				  		
	  		}
			redirectRE = new Regex(@"(?i)^#(?:" + redirectTag + @") \[\[(.+?)]]",
				RegexOptions.Compiled);
      		Console.WriteLine("Site: " + name + " (" + generator + ")");
      		if (Bot.useBotQuery == false)
      			return;      			
	    	Uri queryPHP = new Uri(site + indexPath + "query.php");
		    Bot.InitWebClient();
		    try {
			    string respStr = Bot.wc.DownloadString(queryPHP);
			    if (respStr.IndexOf("<title>MediaWiki Query Interface</title>") != -1)
		    		botQuery = true;
			}
			catch (System.Net.WebException) {
				return;
			}
    	}

    	/// <summary>Loads default english namespace names for site.</summary>    	
    	public void LoadDefaults()
    	{
	    	if (wikiNSpaces.Count != 0 && WMSites.Count != 0)
	    		return;
    		string[] wikiNSNames = { "Media", "Special", "", "Talk", "User", "User talk", name,
    			name + " talk", "Image", "Image talk", "MediaWiki", "MediaWiki talk", "Template",
    			"Template talk", "Help", "Help talk", "Category", "Category talk" };
    		for (int i=-2, j=0; i < 16; i++, j++)
	    		wikiNSpaces.Add(i.ToString(), wikiNSNames[j]);
	    	wikiNSpaces.Remove("0");
	    	WMSites.Add("w", "wikipedia");
	    	WMSites.Add("wikt", "wiktionary");
	    	WMSites.Add("b", "wikibooks");
	    	WMSites.Add("n", "wikinews");
	    	WMSites.Add("q", "wikiquote");
	    	WMSites.Add("s", "wikisource");
	    	foreach (DictionaryEntry s in WMSites)    		
	    		WMSitesStr += s.Key + "|" + s.Value + "|";
	    	mediaWikiVars = new string[] { "currentmonth", "currentmonthname", "currentmonthnamegen", 
	    		"currentmonthabbrev", "currentday2", "currentdayname", "currentyear", "currenttime",
				"currenthour", "localmonth", "localmonthname", "localmonthnamegen", "localmonthabbrev",
				"localday", "localday2", "localdayname", "localyear", "localtime", "localhour",
				"numberofarticles", "numberoffiles", "sitename", "server", "servername", "scriptpath",
				"pagename", "pagenamee", "fullpagename", "fullpagenamee", "namespace", "namespacee",
				"currentweek", "currentdow", "localweek", "localdow", "revisionid", "revisionday",
				"revisionday2", "revisionmonth", "revisionyear", "revisiontimestamp", "subpagename",
				"subpagenamee", "talkspace", "talkspacee", "subjectspace", "dirmark", "directionmark",
				"subjectspacee", "talkpagename", "talkpagenamee", "subjectpagename", "subjectpagenamee",
				"numberofusers", "rawsuffix", "newsectionlink", "numberofpages", "currentversion",
				"basepagename", "basepagenamee", "urlencode", "currenttimestamp", "localtimestamp",
				"directionmark", "language", "contentlanguage", "pagesinnamespace", "numberofadmins",				  
				"currentday", "pagesinns:ns", "pagesinns:ns:r", "numberofarticles:r", "numberofpages:r",
				"numberoffiles:r", "numberofusers:r", "numberofadmins:r" };
			parserFunctions = new string[] { "ns:", "localurl:", "localurle:", "urlencode:",
				"anchorencode:", "fullurl:", "fullurle:",  "grammar:", "plural:", "lc:", "lcfirst:",
				"uc:", "ucfirst:", "formatnum:", "padleft:", "padright:", "#language:",
				"displaytitle:", "defaultsort:", "#if:", "#if:", "#switch:", "#ifexpr:" };
			templateModifiers = new string[] { ":", "int:", "msg:", "msgnw:", "raw:", "subst:" };
	    }

	    /// <summary>Logs in and retrieves cookies.</summary>    	  	
	    private void LogIn()
	    {
	        HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(site + indexPath +
	        	"index.php?title=Special:Userlogin&action=submitlogin&type=login");
	        string postData = string.Format("wpName=+{0}&wpPassword={1}&wpRemember=1" +
	        	"&wpLoginattempt=Log+in", new string[] {userName, userPass});	
	        webReq.Method = "POST";
	        webReq.ContentType = Bot.webContentType;
	        webReq.UserAgent = Bot.botVer;
	        webReq.Proxy.Credentials = CredentialCache.DefaultCredentials;
	        webReq.CookieContainer = new CookieContainer();
	        webReq.AllowAutoRedirect = false;
	        byte[] postBytes = Encoding.UTF8.GetBytes(postData);
	        webReq.ContentLength = postBytes.Length;
	        Stream reqStrm = webReq.GetRequestStream();
	        reqStrm.Write(postBytes, 0, postBytes.Length);
	        reqStrm.Close();
	        HttpWebResponse webResp;
	        try {
	        	webResp = (HttpWebResponse)webReq.GetResponse();	        	
        	}
        	catch (WebException) {
	        	throw new WikiBotException("\n\nLogin failed. Check your username and password.\n");
        	}
        	foreach (Cookie cookie in webResp.Cookies)
                cookies.Add(cookie);        	
        	webResp.Close();
	        Page testLogin = new Page(this, DateTime.Now.Ticks.ToString("x"));
	        testLogin.GetEditSessionData();
			if (string.IsNullOrEmpty(testLogin.editSessionToken)) {
				throw new WikiBotException("\n\nLogin failed. Check your username and password.\n");
			}
	        Console.WriteLine("Logged in as " + userName);
	    }

	    /// <summary>Gets the list of Wikimedia Foundation wiki sites and ISO 639-1:2002
	    /// language codes, used as prefixes.</summary>    	  	
	    public void GetWikimediaWikisList()
	    {
		    Uri wikimediaMeta = new Uri("http://meta.wikimedia.org/wiki/Special:SiteMatrix");
		    Bot.InitWebClient();
		    string respStr = Bot.wc.DownloadString(wikimediaMeta);
		    Regex langCodeRE = new Regex("<a id=\"([^\"]+?)\"");
			Regex siteCodeRE = new Regex("<li><a href=\"[^\"]+?\">([^\\s]+?)<");
			MatchCollection langMatches = langCodeRE.Matches(respStr);
			MatchCollection siteMatches = siteCodeRE.Matches(respStr);
		 	foreach(Match m in langMatches)
		 		WMLangsStr += m.Groups[1].ToString() + "|";
		 	WMLangsStr = WMLangsStr.Remove(WMLangsStr.Length - 1);
		 	foreach(Match m in siteMatches)
		 		WMSitesStr += m.Groups[1].ToString() + "|";
		 	WMSitesStr += "m";
		 	iwikiLinkRE = new Regex(@"(?i)\[\[((" + WMLangsStr + "):(.+?))]]\r?\n?");
		 	iwikiDispLinkRE = new Regex(@"(?i)\[\[:((" + WMLangsStr + "):(.+?))]]");
		 	sisterWikiLinkRE = new Regex(@"(?i)\[\[((" + WMSitesStr + "):(.+?))]]");
		}

		/// <summary>This internal function gets the hypertext markup (HTM) of wiki-page.</summary>
	    /// <param name="pageURL">Absolute or relative URL of а page to get.</param>		
		/// <returns>Returns HTM source code.</returns>		
		public string GetPageHTM(string pageURL)		
		{
			return PostDataAndGetResultHTM(pageURL, "");
		}
		
		/// <summary>This internal function posts specified string to request and gets
		/// the result hypertext markup (HTM).</summary>
	    /// <param name="pageURL">Absolute or relative URL of а page to get.</param>
	    /// <param name="postData">String to post to site with web request.</param>	    		
		/// <returns>Returns HTM source code.</returns>		
		public string PostDataAndGetResultHTM(string pageURL, string postData)
		{   
			if (!pageURL.StartsWith(site))
				pageURL = site + pageURL;
	        HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(pageURL);
            webReq.Proxy.Credentials = CredentialCache.DefaultCredentials;
	        webReq.ContentType = Bot.webContentType;
            webReq.UserAgent = Bot.botVer;
            webReq.CookieContainer = cookies;
            if (!string.IsNullOrEmpty(postData)) {
	            webReq.Method = "POST";
            	byte[] postBytes = Encoding.UTF8.GetBytes(postData);
	        	webReq.ContentLength = postBytes.Length;
	        	Stream reqStrm = webReq.GetRequestStream();
	        	reqStrm.Write(postBytes, 0, postBytes.Length);
	        	reqStrm.Close();
        	}
        	HttpWebResponse webResp;
        	try {
            	webResp = (HttpWebResponse)webReq.GetResponse();
        	}
	    	catch (WebException e) {
		    	if (e.Message.Contains("error: (502) Bad Gateway") ||
		    		e.Message.Contains("error: (500) Internal Server Error")) {
			    	Console.WriteLine(e.Message + " Retrying in 60 seconds.");
			    	Thread.Sleep(60000);
					webResp = (HttpWebResponse)webReq.GetResponse();
				}
				else
					throw e;
		    }
            StreamReader strmReader = new StreamReader(webResp.GetResponseStream());
            string respStr = strmReader.ReadToEnd();
            strmReader.Close();
            webResp.Close();		    
            return respStr;
		}

		/// <summary>This internal function removes the namespace prefix from page title.</summary>
	    /// <param name="pageTitle">Page title to remove prefix from.</param>
	    /// <param name="nsIndex">Index of namespace to remove. If this parameter is 0,
	    /// any found namespace prefix will be removed.</param>	    		
		/// <returns>Page title without prefix.</returns>
		public string RemoveNSPrefix(string pageTitle, int nsIndex)
		{
		  	if (nsIndex != 0) {
		    	pageTitle = Regex.Replace(pageTitle, "(?i)^(" +
		    		Regex.Escape(wikiNSpaces[nsIndex.ToString()].ToString()) + "|" +
			  		Regex.Escape(namespaces[nsIndex.ToString()].ToString()) + "):", "");	  	
		  		return pageTitle;
	  		}
			foreach (DictionaryEntry ns in wikiNSpaces) {
				if (ns.Value == null)
					continue;				
				pageTitle = Regex.Replace(pageTitle, "(?i)^" +
					Regex.Escape(ns.Value.ToString()) + ":", "");
			}
			foreach (DictionaryEntry ns in namespaces) {
				if (ns.Value == null)
					continue;				
			    pageTitle = Regex.Replace(pageTitle, "(?i)^" +
			    	Regex.Escape(ns.Value.ToString()) + ":", "");
			}
			return pageTitle;
		}

		/// <summary>Function changes default english namespace prefixes to correct local prefixes		
		/// (e.g. for german wiki-sites it changes "Category:..." to "Kategorie:...").</summary>
	    /// <param name="pageTitle">Page title to correct prefix in.</param>
		/// <returns>Page title with corrected prefix.</returns>	    			
	    public string CorrectNSPrefix(string pageTitle)
	    {
			foreach (DictionaryEntry ns in wikiNSpaces) {
				if (ns.Value == null)
					continue;
			    if (Regex.IsMatch(pageTitle, "(?i)" + Regex.Escape(ns.Value.ToString()) + ":"))
			    	pageTitle = namespaces[ns.Key] + pageTitle.Substring(pageTitle.IndexOf(":"));
		    }
			return pageTitle;
		}
															    
	    /// <summary>Shows names and integer keys of local and default namespaces.</summary>    	  	
	    public void ShowNamespaces()
	    {
			foreach (DictionaryEntry ns in namespaces) {
				Console.WriteLine(ns.Key.ToString() + "\t" + ns.Value.ToString().PadRight(20) +
					"\t" + wikiNSpaces[ns.Key.ToString()]);
			}
		}
	}
	
	/// <summary>Class defines wiki page object.</summary>    
	public class Page
	{
		/// <summary>Page title.</summary>
		public string title;
		/// <summary>Page text.</summary>
		public string text;
		/// <summary>Page ID in internal MediaWiki database.</summary>
		public string pageID;
		/// <summary>Username or IP-address of last page contributor.</summary>
		public string lastUser;
		/// <summary>Last contributer ID in internal MediaWiki database.</summary>
		public string lastUserID;
		/// <summary>Page revision ID in internal MediaWiki database.</summary>
		public string lastRevisionID;
		/// <summary>True if last edit was minor edit.</summary>
		public bool lastMinorEdit;
		/// <summary>Last edit comment.</summary>
		public string comment;
		/// <summary>Date and time of last edit.</summary>
		public DateTime timestamp;
		/// <summary>This edit session time attribute is used to edit pages.</summary>
		public string editSessionTime;
		/// <summary>This edit session token attribute is used to edit pages.</summary>
		public string editSessionToken;
		/// <summary>Site, on which the page is.</summary>
		public Site site;

	    /// <summary>Constructs Page object with specified title and specified Site object.
	    /// When constructed, new Page object doesn't contain text. Use page.Load() to get text from live
	    /// wiki. Or use page.LoadEx() to get both text and metadata via XML export interface.</summary>
		/// <param name="site">Site object, it must be constructed beforehand.</param>
		/// <param name="title">Page title as string.</param>		
		public Page(Site site, string title)
		{
			this.title = title;
			this.site = site;
		}

		/// <summary>This constructor creates Site object with default properties internally and logs in.
		/// It is too slow, don't use it too often.</summary>
		/// <param name="title">Page title as string.</param>
		public Page(string title)
		{
			site = new Site();
			this.title = title;
		}
		
		/// <summary>Loads actual page text for live wiki site via raw web interface.</summary>				
		public void Load()
		{
			Uri res = new Uri(site.site + site.indexPath + "index.php?title=" + title +
				"&action=raw&ctype=text/plain&dontcountme=s");
		    Bot.InitWebClient();
		    try {
			    text = Bot.wc.DownloadString(res);
			}
	    	catch (System.Net.WebException) {
		    	Console.WriteLine("Page \"" + title + "\" doesn't exist.");
		    	text = "";
		    	return;
		    }
		    Console.WriteLine("Page \"" + title + "\" loaded successfully.");
		}

		/// <summary>Loads page text and metadata via XML export interface. It is slower,
		/// than Load(), don't use it if you don't need page metadata (page id, timestamp,
		/// comment, last contributor, minor edit mark).</summary>			
		public void LoadEx()
	    {
		    Uri res = new Uri(site.site + site.indexPath + "index.php?title=Special:Export/" + title);
		    Bot.InitWebClient();
		    ParsePageXML(Bot.wc.DownloadString(res));
	    }

	    /// <summary>This internal function parses XML export source
	    /// to get page text and metadata.</summary>
	    /// <param name="xmlSrc">XML export source code.</param>		
		public void ParsePageXML(string xmlSrc)
	    {
		    XmlDocument doc = new XmlDocument();
		    doc.LoadXml(xmlSrc);
		    if (doc.GetElementsByTagName("page").Count == 0) {
			    Console.WriteLine("Page \"" + title + "\" doesn't exist.");
		    	return;
	    	}
		    text = doc.GetElementsByTagName("text")[0].InnerText;
			pageID = doc.GetElementsByTagName("id")[0].InnerText;
			if (doc.GetElementsByTagName("username").Count != 0) {
				lastUser = doc.GetElementsByTagName("username")[0].InnerText;
				lastUserID = doc.GetElementsByTagName("id")[2].InnerText;
			}
			else
				lastUser = doc.GetElementsByTagName("ip")[0].InnerText;
			lastRevisionID = doc.GetElementsByTagName("id")[1].InnerText;
			if (doc.GetElementsByTagName("comment").Count != 0)
				comment = doc.GetElementsByTagName("comment")[0].InnerText;
			timestamp = DateTime.Parse(doc.GetElementsByTagName("timestamp")[0].InnerText);
			timestamp = timestamp.ToUniversalTime();
			lastMinorEdit = (doc.GetElementsByTagName("minor").Count != 0) ? true : false;
	    	if (title == "")
	    		title = doc.GetElementsByTagName("title")[0].InnerText;
	    	else			
				Console.WriteLine("Page \"" + title + "\" loaded successfully.");
	    }
	    
	    /// <summary>Loads page text from the specified UTF8-encoded file.</summary>
	    /// <param name="filePathName">Path and name of the file.</param>	    	    
	    public void LoadFromFile(string filePathName)
	    {
			StreamReader strmReader = new StreamReader(filePathName);
			text = strmReader.ReadToEnd();
			strmReader.Close();
			Console.WriteLine("Text for page \"" + title + "\" successfully loaded from \"" +
				filePathName + "\" file.");
		}
	    
	    /// <summary>This function is used internally to gain rights to edit page on a live wiki
	    /// site, using retrieved login cookies.</summary>
	    public void GetEditSessionData()
	    {
		    string src = site.GetPageHTM(site.indexPath + "index.php?title=" +
		    	title + "&action=edit");
			editSessionTime = site.editSessionTimeRE.Match(src).Groups[1].ToString();
		    editSessionToken = site.editSessionTokenRE1.Match(src).Groups[1].ToString();
		    if (string.IsNullOrEmpty(editSessionToken))
		    	editSessionToken = site.editSessionTokenRE2.Match(src).Groups[1].ToString();
    	}
    	
		/// <summary>Saves current contents of page.text on live wiki site. Uses default bot
		/// edit comment and default minor edit mark setting ("true" in most cases)/</summary>
	    public void Save()
	    {
		    Save(text, Bot.editComment, Bot.isMinorEdit);		
		}

		/// <summary>Saves specified text in page on live wiki. Uses default bot
		/// edit comment and default minor edit mark setting ("true" in most cases).</summary>
		/// <param name="newText">New text for this page.</param>
		public void Save(string newText)
	    {
		    Save(newText, Bot.editComment, Bot.isMinorEdit);		
		}

		/// <summary>Saves current page.text contents on live wiki site.</summary>
		/// <param name="comment">Your edit comment.</param>
		/// <param name="isMinorEdit">Minor edit mark (true = minor edit).</param>		
		public void Save(string comment, bool isMinorEdit)
	    {
		    Save(text, comment, isMinorEdit);		
		}

		/// <summary>Saves specified text in page on live wiki.</summary>
		/// <param name="newText">New text for this page.</param>
		/// <param name="comment">Your edit comment.</param>
		/// <param name="isMinorEdit">Minor edit mark (true = minor edit).</param>						
	    public void Save(string newText, string comment, bool isMinorEdit)
	    {
		    GetEditSessionData();
		    if (string.IsNullOrEmpty(editSessionTime) || string.IsNullOrEmpty(editSessionToken)) {
			    Console.WriteLine("Insufficient rights to edit page \"" + title + "\".");
			    return;
			}
            string postData = string.Format("wpSection=&wpStarttime={0}&wpEdittime={1}&wpScrolltop=" +
            	"&wpTextbox1={2}&wpWatchThis=off&wpSummary={3}&wpSave=Save%20Page&wpEditToken={4}",
                new string[] { DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmss"),
                editSessionTime, HttpUtility.UrlEncode(newText), HttpUtility.UrlEncode(comment),
                editSessionToken });
            if (isMinorEdit)
            	postData = postData.Insert(postData.IndexOf("wpSummary"), "wpMinoredit=1&");
			if (Bot.askConfirm) {
				Console.WriteLine("\n\nThe following text is going to be saved on page \"" +
					title + "\":\n\n" + text + "\n\n");
				if(!Bot.UserConfirms())
					return;
				}
            string respStr = site.PostDataAndGetResultHTM(site.indexPath + "index.php?title=" + 
            	title + "&action=submit", postData);
			if (site.editSessionTokenRE1.IsMatch(respStr))
				Console.WriteLine("Edit conflict occured when saving page \"" + title + "\".");   		
   			else {
		    	Console.WriteLine("Page \"" + title + "\" saved successfully.");
		    	text = newText;
	    	}
		}
		
		/// <summary>Uploads local image to wiki site. Uploaded image title will be the
		/// the same as the title of this page, not the title of source file.</summary>
		/// <param name="filePathName">Path and name of image file.</param>
		/// <param name="description">Image description.</param>
		/// <param name="license">Image license type (may be template title). Used only in
		/// some wiki sites. Pass empty string, if the wiki site doesn't require it.</param>	
		/// <param name="copyStatus">Image copy status. Used only in some wiki sites. Pass
		/// empty string, if the wiki site doesn't require it.</param>
		/// <param name="source">Image source. Used only in some wiki sites. Pass
		/// empty string, if the wiki site doesn't require it.</param>									
	    public void UploadImage(string filePathName, string description,
	    	string license, string copyStatus, string source)
	    {
		    if (File.Exists(filePathName) == false) {
			    Console.WriteLine("Image file \"" + filePathName + "\" doesn't exist.");
		    	return;
	    	}
	    	Console.WriteLine("Uploading image \"" + title + "\"...");
	    	string fileName = Path.GetFileName(filePathName).Substring(0, 1).ToUpper() +
	    		Path.GetFileName(filePathName).Substring(1);
	    	string targetName = site.RemoveNSPrefix(title, 6);
	    	targetName = targetName.Substring(0, 1).ToUpper() + targetName.Substring(1);		  		   		
			HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(site.site + 
	        	site.indexPath + "index.php?title=" + site.namespaces["-1"].ToString() + ":Upload");
            webReq.Proxy.Credentials = CredentialCache.DefaultCredentials;
	        webReq.Method = "POST";
	        string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
	        webReq.ContentType = "multipart/form-data; boundary=" + boundary;
            webReq.UserAgent = Bot.botVer;
            webReq.CookieContainer = site.cookies;   
    		StringBuilder sb = new StringBuilder();
    		string ph = "--" + boundary + "\r\nContent-Disposition: form-data; name=\"";
    		sb.Append(ph + "wpIgnoreWarning\"\r\n\r\n1\r\n"); 
    		sb.Append(ph + "wpDestFile\"\r\n\r\n" + targetName + "\r\n");    		
    		sb.Append(ph + "wpUploadAffirm\"\r\n\r\n1\r\n");
    		sb.Append(ph + "wpWatchthis\"\r\n\r\n0\r\n");    		    		    		    		
    		sb.Append(ph + "wpUploadCopyStatus\"\r\n\r\n" + copyStatus + "\r\n");
    		sb.Append(ph + "wpUploadSource\"\r\n\r\n" + source + "\r\n");    		    		    		    		
    		sb.Append(ph + "wpUpload\"\r\n\r\n" + "upload bestand" + "\r\n");
    		sb.Append(ph + "wpLicense\"\r\n\r\n" + license + "\r\n");
    		sb.Append(ph + "wpUploadDescription\"\r\n\r\n" + description + "\r\n");
    		sb.Append(ph + "wpUploadFile\"; filename=\"" +
    			HttpUtility.UrlEncode(Path.GetFileName(filePathName)) + "\"\r\n" +    			
    			"Content-Type: application/octet-stream\r\n\r\n");			
			byte[] postHeaderBytes = Encoding.UTF8.GetBytes(sb.ToString());
			byte[] fileBytes = File.ReadAllBytes(filePathName);
			byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");			
			webReq.ContentLength = postHeaderBytes.Length + fileBytes.Length + boundaryBytes.Length;			
			Stream reqStream = webReq.GetRequestStream();		
			reqStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
			reqStream.Write(fileBytes, 0, fileBytes.Length);
			reqStream.Write(boundaryBytes, 0, boundaryBytes.Length);		
			WebResponse webResp = webReq.GetResponse();
            StreamReader strmReader = new StreamReader(webResp.GetResponseStream());
            string respStr = strmReader.ReadToEnd();
            strmReader.Close();
            webResp.Close();
            //File.WriteAllText("debug.htm", respStr);
			if (!respStr.Contains(targetName) || (site.messages.Contains("MediaWiki:Uploadcorrupt") && 
				respStr.Contains(site.messages["MediaWiki:Uploadcorrupt"].text)))
					Console.WriteLine("Error occured when uploading image \"" + title + "\".");   		
   			else {
		    	title = site.namespaces["6"] + ":" + targetName;
		    	text = description;
		    	Console.WriteLine("Image \"" + title + "\" uploaded successfully.");		    	
	    	}
		}
		
		/// <summary>Uploads web image to wiki site.</summary>
		/// <param name="imageFileUrl">Full URL of image file on the web.</param>
		/// <param name="description">Image description.</param>
		/// <param name="license">Image license type. Used only in some wiki sites. Pass
		/// empty string, if the wiki site doesn't require it.</param>
		/// <param name="copyStatus">Image copy status. Used only in some wiki sites. Pass
		/// empty string, if the wiki site doesn't require it.</param>							
	    public void UploadImageFromWeb(string imageFileUrl, string description,
	    	string license, string copyStatus)
	    {		    
		    Uri res = new Uri(imageFileUrl);
		    Bot.InitWebClient();
		    string imageFileName = imageFileUrl.Substring(imageFileUrl.LastIndexOf("/")+1);
		    try {
		    	Bot.wc.DownloadFile(res, "Cache\\" + imageFileName);
	    	}
	    	catch (System.Net.WebException) {
		    	Console.WriteLine("Can't download image \"" + imageFileUrl + "\".");
		    	return;
		    }
		    if (File.Exists("Cache\\" + imageFileName) == false) {
		    	Console.WriteLine("Error occured when downloading image \"" + imageFileUrl + "\".");
		    	return;
	    	}	    	
		    UploadImage("Cache\\" + imageFileName, description, license, copyStatus, imageFileUrl);
		    File.Delete("Cache\\" + imageFileName);
	    }

	    /// <summary>Downloads image, pointed by this page title, from wiki site.</summary>
		/// <param name="filePathName">Path and name of local file to save image to.</param>
	    public void DownloadImage(string filePathName)
	    {		    
		    Uri res = new Uri(site.site + site.indexPath + "index.php?title=" + title);
		    Bot.InitWebClient();
		    string src = "";
		    try {
			    src = Bot.wc.DownloadString(res);
			}
	    	catch (System.Net.WebException) {
		    	Console.WriteLine("Page \"" + title + "\" doesn't exist.");
		    	return;
		    }
		    Regex imageLinkRE = new Regex("(?:<a href=\"(?'1'[^\"]+?)\" class=\"internal\"|" +
		    	"<div class=\"fullImageLink\" id=\"file\"><a href=\"(?'1'[^\"]+?)\")");
		    if (imageLinkRE.IsMatch(src) == false) {
		    	Console.WriteLine("Image \"" + title + "\" doesn't exist.");
		    	return;
		    }
		    Bot.InitWebClient();
		    Console.WriteLine("Downloading image \"" + title + "\"...");		    
		    Bot.wc.DownloadFile(imageLinkRE.Match(src).Groups[1].ToString(), filePathName);
			Console.WriteLine("Image \"" + title + "\" downloaded successfully.");
	    }
		/*
		/// <summary>This is the interface for XML import. Not implemented yet.</summary>		
	    public void SaveEx()
	    {
		    SaveEx(text, Bot.editComment, Bot.isMinorEdit);		
		}

		/// <summary>This is the interface for XML import. Not implemented yet.</summary>		
		public void SaveEx(string newText)
	    {
		    SaveEx(newText, Bot.editComment, Bot.isMinorEdit);		
		}

		/// <summary>This is the interface for XML import. Not implemented yet.</summary>				
		public void SaveEx(string comment, bool isMinorEdit)
	    {
		    SaveEx(text, comment, isMinorEdit);		
		}
		
		/// <summary>This is the interface for XML import. Not implemented yet.</summary>
	    public void SaveEx(string newText, string comment, bool isMinorEdit) {}
	    */
	    
	    /// <summary>Saves page text to the specified file. If the target file already exists,
	    /// it is overwritten.</summary>
	    /// <param name="filePathName">Path and name of the file.</param>
	    public void SaveToFile(string filePathName)
	    {
		    if (IsEmpty()) {
		    	Console.WriteLine("Page \"" + title + "\" contains no text to save.");
		    	return;
	    	}
            File.WriteAllText(filePathName, text, Encoding.UTF8);
			Console.WriteLine("Text of \"" + title + "\" page successfully saved in \"" +
				filePathName + "\" file.");
		}

		/// <summary>Saves page text to the ".txt" file in current directory.
		/// Use Directory.SetCurrentDirectory function to change the current directory (but don't
		/// forget to change it back after saving file). The name of the file is constructed
		/// from the title of the article. Forbidden characters in filenames are replaced
		/// with their Unicode numeric codes (also known as numeric character references
		/// or NCRs).</summary>
	    public void SaveToFile()
	    {
	    	string fileTitle = title;
	    	//Path.GetInvalidFileNameChars();
	    	fileTitle = fileTitle.Replace("\"", "&#x22;");
	    	fileTitle = fileTitle.Replace("<", "&#x3c;");
	    	fileTitle = fileTitle.Replace(">", "&#x3e;");
	    	fileTitle = fileTitle.Replace("?", "&#x3f;");
	    	fileTitle = fileTitle.Replace(":", "&#x3a;");
	    	fileTitle = fileTitle.Replace("\\", "&#x5c;");
	    	fileTitle = fileTitle.Replace("/", "&#x2f;");
	    	fileTitle = fileTitle.Replace("*", "&#x2a;");
	    	fileTitle = fileTitle.Replace("|", "&#x7c;");
		    SaveToFile(fileTitle + ".txt");
		}
			    
	    /// <summary>Returns true, if page.text field is empty. Don't forget to call
	    /// page.Load() before using this function.</summary>
	    public bool IsEmpty()
	    {
			return string.IsNullOrEmpty(text);    
		}

		/// <summary>Returns true, if page.text field is not empty. Don't forget to call
	    /// Load before using this function.</summary>
		public bool Exists()
	    {
			return (string.IsNullOrEmpty(text) == true) ? false : true;    
		}
		
		/// <summary>Returns true, if page redirects to another page. Don't forget to load
		/// actual page contents from live wiki "page.Load()" before using this function.</summary>		
	    public bool IsRedirect()
	    {
		    if (!Exists())
		    	return false;    
		    return site.redirectRE.IsMatch(text);
		}
		
		/// <summary>Returns true, if page redirects to another page. Don't forget to load
		/// actual page contents from live wiki "page.Load()" before using this function.</summary>
	    public string RedirectsTo()
	    {	
		    if (IsRedirect())
		    	return site.redirectRE.Match(text).Groups[1].ToString();
		    else
		    	return "";
		}
	    
		/// <summary>If this page is a redirect, this function loads the title and text
		/// of redirected-to page into this Page object.</summary>		
		public void ResolveRedirect()
	    {	
		    if (IsRedirect()) {
		    	title = RedirectsTo();
		    	LoadEx();
	    	}
		}
		
		/// <summary>Returns true, if this page is a disambigation page. Don't forget to load
		/// actual page contents from live wiki "page.Load()" before using this function.</summary>	
	    public bool IsDisambig()
	    {
		    return text.Contains(site.disambigStr);
		}

		/// <summary>This internal function removes the namespace prefix from page title.</summary>
		public void RemoveNSPrefix()
		{
			title = site.RemoveNSPrefix(title, 0);
		}
		
		/// <summary>Function changes default english namespace prefixes to correct local prefixes
		/// (e.g. for german wiki-sites it changes "Category:..." to "Kategorie:...").</summary>	
	    public void CorrectNSPrefix()
	    {
		    title = site.CorrectNSPrefix(title);
		}
				
		/// <summary>Returns the array of strings, containing all wikilinks ([[...]])
		/// found in page text, excluding links in image descriptions, but including
		/// interwiki links, links to sister projects, categories, images, etc.</summary>
		/// <returns>Returns raw links in strings array.</returns>
	    public string[] GetAllLinks()
	    {		    	
		    MatchCollection matches = site.wikiLinkRE.Matches(text);
		    string[] matchStrings = new string[matches.Count];
		    for(int i = 0; i < matches.Count; i++)
			    matchStrings[i] = matches[i].Groups[1].Value;
		    return matchStrings;
		}
		
		/// <summary>Finds all internal wikilinks in page text, excluding interwiki
		/// links, links to sister projects, categories, embedded images and links in
		/// image descriptions.</summary>	
		/// <returns>Returns the PageList object, where page titles are the wikilinks,
		/// found in text.</returns>
	    public PageList GetLinks()
	    {		    	
		    MatchCollection matches = site.wikiLinkRE.Matches(text);		     
			StringCollection exclLinks = new StringCollection();
			exclLinks.AddRange(GetInterWikiLinks());
			exclLinks.AddRange(GetSisterWikiLinks(true));
			StringCollection inclLinks = new StringCollection();
			string str;
			for(int i = 0; i < matches.Count; i++)
				if (exclLinks.Contains(matches[i].Groups[1].Value) == false &&
					exclLinks.Contains(matches[i].Groups[1].Value.TrimStart(':')) == false) {
					str = matches[i].Groups[1].Value;
					if (str.IndexOf("#") != -1)
						str = str.Substring(0, str.IndexOf("#"));
					inclLinks.Add(str); }
		    PageList pl = new PageList(site, inclLinks);
		    pl.RemoveNamespaces(new int[] {6,14});
		    foreach (Page p in pl.pages)
		    	p.title = p.title.TrimStart(':');
		    return pl;
		}
				
		/// <summary>Returns the array of strings, containing external links,
		/// found in page text.</summary>	
		/// <returns>Returns the string[] array.</returns>
	    public string[] GetExternalLinks()
	    {		    	
		    MatchCollection matches = site.webLinkRE.Matches(text);
		    string[] matchStrings = new string[matches.Count];
		    for(int i = 0; i < matches.Count; i++)
			    matchStrings[i] = matches[i].Value;
		    return matchStrings;
		}
		
		/// <summary>Returns the array of strings, containing interwiki links,
		/// found in page text. But no displayed links, like [[:de:Stern]] - these
		/// are returned by GetSisterWikiLinks(true) function.</summary>	
		/// <returns>Returns the string[] array.</returns>
	    public string[] GetInterWikiLinks()
	    {
		    if (string.IsNullOrEmpty(Site.WMLangsStr))
		    	site.GetWikimediaWikisList();	    	    	
		    MatchCollection matches = site.iwikiLinkRE.Matches(text);
		    string[] matchStrings = new string[matches.Count];
		    for(int i = 0; i < matches.Count; i++)
			    matchStrings[i] = matches[i].Groups[1].Value;
		    return matchStrings;
		}		

		/// <summary>Adds interwiki links to the page. It doesn't remove or replace old
		/// interwiki links, this must be done manually, if necessary.</summary>
		/// <param name="iwikiLinks">Interwiki links as an array of strings, with or
		/// without square brackets, for example: "de:Stern" or "[[de:Stern]]".</param>		
		public void AddInterWikiLinks(string[] iwikiLinks)
		{
			List<string> iwList = new List<string>(iwikiLinks);			
			for(int i = 0; i < iwList.Count; i++)
				iwList[i] = iwList[i].Trim("[]\f\n\r\t\v ".ToCharArray());
			iwList.AddRange(GetInterWikiLinks());
			iwList.Sort();
			RemoveInterWikiLinks();
			text += "\r\n";
			foreach(string str in iwList)
				text += "\r\n[[" + str + "]]";
		}

		/// <summary>Removes all interwiki links from text of page.</summary>		
		public void RemoveInterWikiLinks()
		{
			if (string.IsNullOrEmpty(Site.WMLangsStr))
		    	site.GetWikimediaWikisList();
			text = site.iwikiLinkRE.Replace(text, "");
			text = text.TrimEnd("\r\n".ToCharArray());
		}
				
		/// <summary>Returns the array of strings, containing links to sister Wikimedia
		/// Foundation Projects, found in page text.</summary>
		/// <param name="includeDisplayedInterWikiLinks">Include displayed interwiki
		/// links like "[[:de:Stern]]".</param>	
		/// <returns>Returns the string[] array.</returns>
	    public string[] GetSisterWikiLinks(bool includeDisplayedInterWikiLinks)
	    {
		    if (string.IsNullOrEmpty(Site.WMLangsStr))
		    	site.GetWikimediaWikisList();	    	    	
		    MatchCollection sisterMatches = site.sisterWikiLinkRE.Matches(text);
		    MatchCollection iwikiMatches = site.iwikiDispLinkRE.Matches(text);
		    int size = (includeDisplayedInterWikiLinks == true) ?
		    	sisterMatches.Count + iwikiMatches.Count : sisterMatches.Count;
		    string[] matchStrings = new string[size];
		    int i = 0;
		    for(; i < sisterMatches.Count; i++)
			    matchStrings[i] = sisterMatches[i].Groups[1].Value;
			if (includeDisplayedInterWikiLinks == true)
		    	for(int j = 0; j < iwikiMatches.Count; i++, j++)
			    	matchStrings[i] = iwikiMatches[j].Groups[1].Value;
		    return matchStrings;
		}

		/// <summary>Returns the array of strings, containing category names found in 
		/// page text with namespace prefix, but without sorting keys. Use the result
		/// strings to call FillFromCategory(string) function.</summary>	
		/// <returns>Returns the string[] array.</returns>
	    public string[] GetCategories()
	    {	    	    	
		    return GetCategories(true, false);
		}

		/// <summary>Returns the array of strings, containing category names found in 
		/// page text.</summary>
		/// <param name="withNameSpacePrefix">If true, function will return strings with
		/// namespace prefix like "Category:Stars", not just "Stars".</param>
		/// <param name="withSortKey">If true, function will return strings with sort keys,
		/// if found. Like "Stars|D3" (in [[Category:Stars|D3]]), not just "Stars".</param>
		/// <returns>Returns the string[] array.</returns>
	    public string[] GetCategories(bool withNameSpacePrefix, bool withSortKey)
	    {	    	    	
		    MatchCollection matches = site.wikiCategoryRE.Matches(text);
		    string[] matchStrings = new string[matches.Count];
		    for(int i = 0; i < matches.Count; i++) {    	
		    	matchStrings[i] = matches[i].Groups[4].Value;
		    	if (withSortKey == true)
			    	matchStrings[i] += matches[i].Groups[5].Value;
			    if (withNameSpacePrefix == true)
			    	matchStrings[i] = site.namespaces["14"] + ":" + matchStrings[i];
		    }
		    return matchStrings;
		}
		
		/// <summary>Adds the page to the specified category by adding
		/// link to that category in page text. If the link to the specified category
		/// already exists, the function does nothing.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>
		public void AddToCategory(string categoryName)
		{
			categoryName = categoryName.Trim("[]\f\n\r\t\v ".ToCharArray());
			categoryName = site.RemoveNSPrefix(categoryName, 14);			
			if (Regex.IsMatch(text, @"(?i)\[\[(" + site.namespaces["14"] + 
				"|" + Site.wikiNSpaces["14"] + ") *: *" + categoryName + @" *(]]|\|)"))
				return;
			if (site.wikiCategoryRE.IsMatch(text))
				text = site.wikiCategoryRE.Replace(text, "$&\r\n[[" + 
					site.namespaces["14"] + ":" + categoryName + "]]", 1, 0);
			else {
				string[] iw = GetInterWikiLinks();
				RemoveInterWikiLinks();
				text += "\r\n\r\n[[" + site.namespaces["14"] + ":" + categoryName + "]]\r\n";
				AddInterWikiLinks(iw);
				text = text.TrimEnd("\r\n".ToCharArray());
			}
		}

		/// <summary>Removes the page from category by deleting link to that category in
		/// page text.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>		
		public void RemoveFromCategory(string categoryName)
		{
			categoryName = site.RemoveNSPrefix(categoryName, 14);
			text = Regex.Replace(text, @"\[\[(" + site.namespaces["14"] + "|" +
				Site.wikiNSpaces["14"] + "):" + categoryName + @"(\|.*?)?]]\r?\n?", "");
			text = text.TrimEnd("\r\n".ToCharArray());
		}
						
		/// <summary>Returns the array of strings, containing names of templates, used in page.
		/// Templates are returned with template modifiers (like "subst:"), if present.
		/// Links to templates (like [[:Template:...]]) are not included. The "magic words"
		/// (see http://meta.wikimedia.org/wiki/Help:Magic_words) are recognized and not returned
		/// by this function as templates.</summary>
		/// <param name="withNameSpacePrefix">If true, function will return strings with
		/// namespace prefix like "Template:SomeTemplate", not just "SomeTemplate".</param>
		/// <returns>Returns the string[] array.</returns>
	    public string[] GetTemplates(bool withNameSpacePrefix)
	    {   	    	
		    MatchCollection matches = site.wikiTemplateRE.Matches(text);
		    string[] matchStrings = new string[matches.Count];
		    string match = "";
		    for(int i = 0, j = 0; i < matches.Count; i++) {
			    match = matches[i].Groups[1].Value;
			    foreach (string mediaWikiVar in Site.mediaWikiVars)
		    		if (match.ToLower() == mediaWikiVar) {
			    		match = "";
		    			break;
	    			}
	    		if (match == "")
	    			continue;	    							    
		    	foreach (string parserFunction in Site.parserFunctions)
		    		if (match.ToLower().StartsWith(parserFunction)) {
			    		match = "";
		    			break;
	    			}
	    		if (match == "")
	    			continue;
		    	match = site.RemoveNSPrefix(match, 10);
		    	if (withNameSpacePrefix)
		    		matchStrings[j++] = site.namespaces["10"] + ":" + match;
		    	else
			    	matchStrings[j++] = match;
		    }
		    return matchStrings;
		}

		/// <summary>Adds a specified template to the end of the page text
		/// (right before categories).</summary>
		/// <param name="templateText">Template text, like "{{template_name|...|...}}".</param>		
		public void AddTemplate(string templateText)
		{			
			Regex templateInsertion = new Regex("([^}]\n|}})\n*\\[\\[(" + site.namespaces["14"] +
				"|" + Site.wikiNSpaces["14"] + "):");
			if (templateInsertion.IsMatch(text))
				text = templateInsertion.Replace(text, "$1\n" + templateText + "\n\n[[" + 
					site.namespaces["14"] + ":", 1);
			else {
				string[] iw = GetInterWikiLinks();
				RemoveInterWikiLinks();
				text += "\n\n" + templateText;
				AddInterWikiLinks(iw);
				text = text.TrimEnd("\r\n".ToCharArray());
			}
		}
				
		/// <summary>Returns the array of strings, containing names of images (image files), 
		/// embedded in page, including images in galleries (inside "gallery" tag).
		/// But no links to images, like [[:Image:...]].</summary>
		/// <param name="withNameSpacePrefix">If true, function will return strings with
		/// namespace prefix like "Image:Example.jpg", not just "Example.jpg".</param>	
		/// <returns>Returns the string[] array.</returns>
	    public string[] GetImages(bool withNameSpacePrefix)	    
	    {   
		    MatchCollection matches; 	    	
		    if (Regex.IsMatch(text, "(?is)<gallery>.*</gallery>"))
		    	matches = Regex.Matches(text, "(?i)(?<!:)(" + Site.wikiNSpaces["6"] + "|" +
		    		site.namespaces["6"] + ")(:)(.*?)(\\||\r|\n|]])");
		    else
		    	matches = site.wikiImageRE.Matches(text);
		    string[] matchStrings = new string[matches.Count];
		    for(int i = 0; i < matches.Count; i++)
		    	if (withNameSpacePrefix == true)
		    		matchStrings[i] = site.namespaces["6"] + ":" + matches[i].Groups[3].Value;
		    	else
			    	matchStrings[i] = matches[i].Groups[3].Value;
		    return matchStrings;
		}
								
		/// <summary>Identifies the namespace of the page.</summary>
		/// <returns>Returns the integer key of the namespace.</returns>	
	    public int GetNamespace()
	    {
		    foreach (DictionaryEntry ns in site.namespaces) {
			    if (title.StartsWith(ns.Value + ":"))
			    	return int.Parse(ns.Key.ToString());
			}
			foreach (DictionaryEntry ns in Site.wikiNSpaces) {
			    if (title.StartsWith(ns.Value + ":"))
			    	return int.Parse(ns.Key.ToString());
			}    
		    return 0;
		}

		/// <summary>Sends page title to console.</summary>
	    public void ShowTitle()
	    {
			Console.Write("\nThe title of this page is \"" + title + "\".\n");
		}
					
		/// <summary>Sends page text to console.</summary>	
		public void ShowText()
		{
			Console.Write("\nThe text of \"" + title + "\" page:\n\n" + text + "\n\n");
		}
				
		/// <summary>Renames the page.</summary>
	    /// <param name="newTitle">New title of that page.</param>
	    /// <param name="reason">Reason for renaming.</param>	
		public void RenameTo(string newTitle, string reason)
		{
			Page mp = new Page(site, "Special:Movepage/" + HttpUtility.UrlEncode(title));
			mp.GetEditSessionData();
			if (string.IsNullOrEmpty(mp.editSessionToken)) {
				Console.WriteLine("Unable to rename page \"" + title + "\" to \"" + 
					newTitle + "\".");
				return;
			}
			string postData = string.Format("wpNewTitle={0}&wpOldTitle={1}&wpEditToken={2}" +
				"&wpReason={3}", HttpUtility.UrlEncode(newTitle), HttpUtility.UrlEncode(title),
				mp.editSessionToken, HttpUtility.UrlEncode(reason));
			string respStr = site.PostDataAndGetResultHTM(site.indexPath +
				"index.php?title=Special:Movepage&action=submit", postData);
			if (site.editSessionTokenRE2.IsMatch(respStr)) {
				Console.WriteLine("Failed to rename page \"" + title + "\" to \"" + 
					newTitle + "\".");
				return;
			}
			Console.WriteLine("Page \"" + title + "\" was successfully renamed to \"" +
				newTitle + "\".");
			title = newTitle;
		}
		
		/// <summary>Deletes the page. Sysop rights are needed to delete page.</summary>
	    /// <param name="reason">Reason for deleting.</param>	
		public void Delete(string reason)
		{
            string respStr1 = site.GetPageHTM(site.indexPath + "index.php?title=" +
            	title + "&action=delete"); 
		    editSessionToken = site.editSessionTokenRE1.Match(respStr1).Groups[1].ToString();
		    if (string.IsNullOrEmpty(editSessionToken))
		    	editSessionToken = site.editSessionTokenRE2.Match(respStr1).Groups[1].ToString();						
			if (string.IsNullOrEmpty(editSessionToken)) {
				Console.WriteLine("Unable to delete page \"" + title + "\".");
				return;
			}
			string postData = string.Format("wpReason={0}&wpEditToken={1}",
				HttpUtility.UrlEncode(reason), editSessionToken);
			string respStr2 = site.PostDataAndGetResultHTM(site.indexPath + "index.php?title=" +
				title + "&action=delete", postData);
			if (site.editSessionTokenRE2.IsMatch(respStr2)) {
				Console.WriteLine("Failed to delete page \"" + title + "\".");
				return;
			}
			Console.WriteLine("Page \"" + title + "\" was successfully deleted.");
			title = "";
		}
	}
	
	/// <summary>Class defines a set of wiki pages (constructed inside as List object).</summary>
	public class PageList
	{
		/// <summary>Internal generic List, that contains collection of pages.</summary>
		public List<Page> pages = new List<Page>();
		/// <summary>Site, on which the pages are.</summary>
		public Site site;
		
		/// <returns>Returns the PageList object.</returns>
		public PageList()
		{
			site = new Site();
		}
		
		/// <returns>Returns the PageList object.</returns>
		public PageList(Site site)
		{
			this.site = site;
		}
		
		/// <returns>Returns the PageList object.</returns>
		public PageList(Site site, string[] pageNames)
		{
			this.site = site;
			foreach (string pageName in pageNames)
				pages.Add(new Page(site, pageName));
			CorrectNSPrefixes();				
		}
		
		/// <returns>Returns the PageList object.</returns>
		public PageList(Site site, StringCollection pageNames)
		{
			this.site = site;
			foreach (string pageName in pageNames)
				pages.Add(new Page(site, pageName));
			CorrectNSPrefixes();
		}
		
		/// <summary>This index allows to call pageList[i] instead of pageList.pages[i].</summary>
		public Page this[int index]
		{
        	get { return pages[index]; }
        	set { pages[index] = value; }
		}

		/// <summary>This index allows to call pageList["title"]. Don't forget to use correct
		/// local namespace prefixes. Call CorrectNSPrefixes function to correct namespace
		/// prefixes in all PageList.</summary>
		public Page this[string index]
		{
        	get {
	        	foreach (Page p in pages)
	        		if (p.title == index)
	        			return p;
	        	return null;
	        }
        	set {
	        	for (int i=0; i < pages.Count; i++)
	        		if (pages[i].title == index)
	        			pages[i] = value;
	        }
		}

		/// <summary>This standard internal function allows to directly use pageList objects
		/// in "foreach" statements.</summary>
		public IEnumerator GetEnumerator()
		{
			return pages.GetEnumerator();
		}

		/// <summary>This function returns true, if in this PageList there exists a page with
		/// the same title, as a page specified as a parmeter.</summary>
		/// <param name="page">.</param>		
		public bool Contains(Page page)
		{
			page.CorrectNSPrefix();
			CorrectNSPrefixes();
			foreach (Page p in pages)
	        	if (p.title == page.title)
	        		return true;
	        return false;
		}
				
		/// <summary>This function returns true, if a page with specified title exists
		/// in this PageList.</summary>
		/// <param name="title">Title of page to check.</param>		
		public bool Contains(string title)
		{
			Page page = new Page(site, title);
			page.CorrectNSPrefix();
			CorrectNSPrefixes();
			foreach (Page p in pages)
	        	if (p.title == page.title)
	        		return true;
	        return false;
		}
			
		/// <summary>This function returns the number of pages in PageList.</summary>
		public int Count()
		{
			return pages.Count;
		}
		
		/// <summary>Removes page at specified index from PageList.</summary>
		/// <param name="index">Zero-based index.</param>		
		public void RemoveAt(int index)
		{
			pages.RemoveAt(index);
		}

		/// <summary>Removes a page with specified title from this PageList.</summary>
		/// <param name="title">Title of page to remove.</param>		
		public void Remove(string title)
		{
			for(int i = 0; i < Count(); i++)
	        	if (pages[i].title == title)
	        		pages.RemoveAt(i);	        
		}
						
		/// <summary>Gets page titles for this PageList from "Special:Allpages" MediaWiki page.
		/// That means a list of site pages in alphabetical order.</summary>
		/// <param name="firstPageTitle">Title of page to start filling from. The title must
		/// not have namespace prefix like "Talk:", just the page title itself. Here you can
		/// also specify just a letter or two instead of full real title. Pass the empty string
		/// to start from the very beginning.</param>
		/// <param name="neededNSpace">Integer, presenting the key of namespace to get pages
		/// from. Only one key of one namespace can be specified (zero for default).</param>
		/// <param name="acceptRedirects">Set this to "false" to exclude redirects.</param>
		/// <param name="quantity">Maximum allowed quantity of pages in this PageList.</param>
		public void FillFromAllPages(string firstPageTitle, int neededNSpace, bool acceptRedirects,
			int quantity)
		{
			Console.WriteLine("Getting " + quantity + " page titles from " +
				"\"Special:Allpages\" MediaWiki page...");
			//RemoveAll();
			string src = "";
			Regex linkToPageRE;
			if (acceptRedirects)
				linkToPageRE = new Regex("<td>(?:<div class=\"allpagesredirect\">)?" +
					"<a href=\"[^\"]*?\" title=\"([^\"]*?)\">");
			else 
				linkToPageRE = new Regex("<td><a href=\"[^\"]*?\" title=\"([^\"]*?)\">");
			MatchCollection matches;
			if (firstPageTitle == "!")
				firstPageTitle = "";
			Regex nextPortionRE = new Regex("&amp;from=(.*?)\" title=\"");
			do {
				Uri res = new Uri(site.site + site.indexPath +
					"index.php?title=Special:Allpages&from=" + firstPageTitle + "&namespace=" +
					neededNSpace.ToString());
		    	Bot.InitWebClient();
		    	src = Bot.wc.DownloadString(res);
		    	matches = linkToPageRE.Matches(src);
		    	foreach (Match match in matches)
			    	pages.Add(new Page(site, match.Groups[1].Value));
			    firstPageTitle = nextPortionRE.Match(src).Groups[1].Value;
			}
			while(nextPortionRE.IsMatch(src) && pages.Count < quantity);			
			if (pages.Count > quantity)
				pages.RemoveRange(quantity, pages.Count - quantity);
			Console.WriteLine("PageList filled with " + pages.Count + " page titles from " +
				"\"Special:Allpages\" MediaWiki page.");
		}

		/// <summary>Gets page titles for this PageList from specified special page,
		/// e.g. "Deadendpages". The function does not filter namespaces. And the function
		/// does not clear the existing PageList, so new titles will be added.</summary>
		/// <param name="pageTitle">Title of special page, e.g. "Deadendpages".</param>
		/// <param name="quantity">Maximum number of page titles to get. Usually
		/// MediaWiki provides not more than 1000 titles.</param>
		public void FillFromCustomSpecialPage(string pageTitle, int quantity)
		{
			Console.WriteLine("Getting " + quantity + " page titles from " +
				"\"Special:" + pageTitle + "\" page...");
			Uri res = new Uri(site.site + site.indexPath +
				"index.php?title=Special:" + pageTitle + "&limit=" + quantity.ToString());
		    Bot.InitWebClient();
		    string src = Bot.wc.DownloadString(res);
		    MatchCollection matches;
		    if (pageTitle == "Unusedimages")
		    	matches = site.linkToPageRE3.Matches(src);
		    else
		    	matches = site.linkToPageRE2.Matches(src);
		    if (matches.Count == 0) {
			    Console.WriteLine("Page \"Special:" + pageTitle + "\" does not contain page titles.");
				return;
			}
		    foreach (Match match in matches)
			    pages.Add(new Page(site, match.Groups[1].Value));
			Console.WriteLine("PageList filled with " + matches.Count + " page titles from " +
				"\"Special:" + pageTitle + "\" page...");
		}
		
		/// <summary>Gets page titles for this PageList from specified special page,
		/// e.g. "Deadendpages". The function does not filter namespaces. And the function
		/// does not clear the existing PageList, so new titles will be added.
		/// The function uses XML (XHTML) parsing instead of regular expressions matching.
		/// This function is slower, than FillFromCustomSpecialPage.</summary>
		/// <param name="pageTitle">Title of special page, e.g. "Deadendpages".</param>
		/// <param name="quantity">Maximum number of page titles to get. Usually
		/// MediaWiki provides not more than 1000 titles.</param>
		public void FillFromCustomSpecialPageEx(string pageTitle, int quantity)
		{
			Console.WriteLine("Getting " + quantity + " page titles from " +
				"\"Special:" + pageTitle + "\" page...");
			Uri res = new Uri(site.site + site.indexPath +
				"index.php?title=Special:" + pageTitle + "&limit=" + quantity.ToString());
			Bot.InitWebClient();			
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(Bot.wc.DownloadString(res));
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
			string nsURI = doc.DocumentElement.SelectSingleNode("/*").Attributes["xmlns"].InnerXml;
			nsmgr.AddNamespace("ns", nsURI); 							
			XmlNodeList nl = doc.DocumentElement.SelectNodes("//ns:ol/ns:li/ns:a[@title != '']",
				nsmgr);		
			if (nl.Count == 0) {
				Console.WriteLine("Nothing was found on \"Special:" + pageTitle + "\" page...");
				return;
			}
		    foreach (XmlNode node in nl)
			    pages.Add(new Page(site, node.Attributes["title"].InnerXml));
			Console.WriteLine("PageList filled with " + nl.Count + " page titles from " +
				"\"Special:" + pageTitle + "\" page...");
		}		

		/// <summary>Gets page titles for this PageList from specified MediaWiki events log.
		/// The function does not filter namespaces. And the function does not clear the
		/// existing PageList, so new titles will be added.</summary>
		/// <param name="logType">Type of log, it could be: "block" for blocked users log;
		/// "protect" for protected pages log; "rights" for users rights log; "delete" for
		/// deleted pages log; "upload" for uploaded files log; "move" for renamed pages log;
		/// "import" for transwiki import log; "renameuser" for renamed accounts log;
		/// "newusers" for new users log; "makebot" for bot status assignment log.</param>
		/// <param name="userName">Select log entries only for specified account. Pass empty
		/// string, if that restriction is not needed.</param>
		/// <param name="pageTitle">Select log entries only for specified page. Pass empty
		/// string, if that restriction is not needed.</param>
		/// <param name="quantity">Maximum number of page titles to get.</param>
		public void FillFromCustomLog(string logType, string userName, string pageTitle, int quantity)
		{
			Console.WriteLine("Getting " + quantity.ToString() + " page titles from \"" +
				logType + "\" log...");
			Uri res = new Uri(site.site + site.indexPath + "index.php?title=Special:Log&type=" +
				 logType + "&user=" + HttpUtility.UrlEncode(userName) + "&page=" +
				 HttpUtility.UrlEncode(pageTitle) + "&limit=" + quantity.ToString());
		    Bot.InitWebClient();
		    string src = Bot.wc.DownloadString(res);
		    MatchCollection matches = site.linkToPageRE2.Matches(src);
		    if (matches.Count == 0) {
			    Console.WriteLine("Log \"" + logType + "\" does not contain page titles.");
				return;
			}
		    foreach (Match match in matches)
			    pages.Add(new Page(site, match.Groups[1].Value));
			Console.WriteLine("PageList filled with " + matches.Count + " page titles from \"" +
				logType + "\"log.");
		}
		
		/// <summary>Gets page titles for this PageList from recent changes page,
		/// "Special:Recentchanges". Image uploads, page deletions and page renamings are
		/// not included, use FillFromCustomLog function instead to fill from respective logs.
		/// The function does not clear the existing PageList, so new titles will be added.
		/// Use FilterNamespaces() or RemoveNamespaces() functions to remove
		/// pages from unwanted namespaces.</summary>
		/// <param name="hideMinor">Ignore minor edits.</param>
		/// <param name="hideBots">Ignore bot edits.</param>
		/// <param name="hideAnons">Ignore anonymous users edits.</param>
		/// <param name="hideLogged">Ignore logged-in users edits.</param>
		/// <param name="hideSelf">Ignore edits of this bot account.</param>
		/// <param name="limit">Maximum number of changes to get.</param>
		/// <param name="days">Get changes in this number of last days.</param>				
		public void FillFromRecentChanges(bool hideMinor, bool hideBots, bool hideAnons,
			bool hideLogged, bool hideSelf, int limit, int days)
		{
			Console.WriteLine("Getting " + limit + " page titles from " +
				"\"Special:Recentchanges\" page...");
			string uri = string.Format("{0}{1}index.php?title=Special:Recentchanges&" +
				"hideminor={2}&hideBots={3}&hideAnons={4}&hideliu={5}&hidemyself={6}&" +
				"limit={7}&days={8}", new string[] { site.site, site.indexPath,
				hideMinor ? "1" : "0", hideBots ? "1" : "0", hideAnons ? "1" : "0",
				hideLogged ? "1" : "0", hideSelf ? "1" : "0",
				limit.ToString(), days.ToString() }	);
            string respStr = site.GetPageHTM(uri);
		    MatchCollection matches = site.linkToPageRE2.Matches(respStr);
		    foreach (Match match in matches)
			    pages.Add(new Page(site, match.Groups[1].Value));
			Console.WriteLine("PageList filled with " + matches.Count + " page titles from " +
				"\"Special:Recentchanges\" page...");
		}

		/// <summary>Gets page titles for this PageList from specified wiki category page, excluding
		/// subcategories. Use FillSubsFromCategory function to get subcategories.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>					
		public void FillFromCategory(string categoryName)
		{
			int count = pages.Count;
			PageList pl = new PageList(site);
			pl.FillAllFromCategory(categoryName);
			pl.RemoveNamespaces(new int[] {14});
			pages.AddRange(pl.pages);
			if (pages.Count != count)
				Console.WriteLine("PageList filled with " + (pages.Count - count).ToString() + 
					" page titles, found in \"" + categoryName + "\" category.");
			else
				Console.WriteLine("Nothing was found in \"" + categoryName + "\" category.");	
		}
						
		/// <summary>Gets subcategories titles for this PageList from specified wiki category page,
		/// excluding other pages. Use FillFromCategory function to get other pages.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>		
		public void FillSubsFromCategory(string categoryName)
		{
			int count = pages.Count;
			PageList pl = new PageList(site);
			pl.FillAllFromCategory(categoryName);
			pl.FilterNamespaces(new int[] {14});
			pages.AddRange(pl.pages);
			if (pages.Count != count)
				Console.WriteLine("PageList filled with " + (pages.Count - count).ToString() + 
					" subcategory page titles, found in \"" + categoryName + "\" category.");
			else
				Console.WriteLine("Nothing was found in \"" + categoryName + "\" category.");
		}
		
		/// <summary>This internal function gets all page titles for this PageList from specified
		/// category page, including subcategories.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>
		public void FillAllFromCategory(string categoryName)
		{
			categoryName = categoryName.Trim("[]\f\n\r\t\v ".ToCharArray());			
			categoryName = site.RemoveNSPrefix(categoryName, 14);
			categoryName = site.namespaces["14"] + ":" + categoryName;
			Console.WriteLine("Getting category \"" + categoryName + "\" contents...");
			//RemoveAll();
			if (site.botQuery == true) {
				//string categoryNameStrip = categoryName.Replace(site.namespaces["14"] + ":", "");
				//categoryNameStrip = categoryNameStrip.Replace(Site.wikiNSpaces["14"] + ":", "");
				FillAllFromCategoryEx(categoryName);
				return;
			}
			string src = "";
			MatchCollection matches;
			Regex nextPortionRE = new Regex("&amp;from=(.*?)\" title=\"");
			do {
				Uri res = new Uri(site.site + site.indexPath + "index.php?title=" + categoryName +
					"&from=" + nextPortionRE.Match(src).Groups[1].Value);
		    	Bot.InitWebClient();
		    	src = Bot.wc.DownloadString(res);		    	
		    	matches = site.linkToPageRE1.Matches(src);
		    	foreach (Match match in matches)
			    	pages.Add(new Page(site, match.Groups[1].Value));
			}
			while(nextPortionRE.IsMatch(src));
		}

		/// <summary>This internal function gets all page titles for this PageList from specified
		/// category page using "query.php" interface. It gets titles portion (usually 200) by portion.
		/// It gets subcategories too.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>
		public void FillAllFromCategoryEx(string categoryName)
		{
			string src = "";
			MatchCollection matches;
			Regex nextPortionRE = new Regex("<category next=\"(.+?)\" />");
			do {
				Uri res = new Uri(site.site + site.indexPath + "query.php?what=category&cptitle=" +
					categoryName + "&cpfrom=" + nextPortionRE.Match(src).Groups[1].Value + "&format=xml");
		    	Bot.InitWebClient();
		    	src = Bot.wc.DownloadString(res);
		    	matches = site.pageTitleTagRE.Matches(src);
		    	foreach (Match match in matches)
			    	pages.Add(new Page(site, match.Groups[1].Value));
		    }
			while(nextPortionRE.IsMatch(src));
		}

		/// <summary>Gets all levels of subcategories of some wiki category (that means subcategories,
		/// sub-subcategories, and so on) and fills this PageList with titles of all pages, found in
		/// all levels of subcategories. The multiplicates of recurring pages are removed.
		/// Use FillSubsFromCategoryTree function instead to get titles of subcategories.
		/// The operation may be very time-consuming and traffic-consuming.
		/// The function clears the PageList before filling.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>		
		public void FillFromCategoryTree(string categoryName)
		{
			FillAllFromCategoryTree(categoryName);
			RemoveNamespaces(new int[] {14});
			if (pages.Count != 0)
				Console.WriteLine("PageList filled with " + Count().ToString() + 
					" page titles, found in \"" + categoryName + "\" category.");
			else
				Console.WriteLine("Nothing was found in \"" + categoryName + "\" category.");	
		}
						
		/// <summary>Gets all levels of subcategories of some wiki category (that means subcategories,
		/// sub-subcategories, and so on) and fills this PageList with found subcategory titles.
		/// Use FillFromCategoryTree function instead to get pages of other namespaces.
		/// The multiplicates of recurring categories are removed. The operation may be very
		/// time-consuming and traffic-consuming. The function clears the PageList
		/// before filling.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>		
		public void FillSubsFromCategoryTree(string categoryName)
		{
			FillAllFromCategoryTree(categoryName);
			FilterNamespaces(new int[] {14});
			if (pages.Count != 0)
				Console.WriteLine("PageList filled with " + Count().ToString() + 
					" subcategory page titles, found in \"" + categoryName + "\" category.");
			else
				Console.WriteLine("Nothing was found in \"" + categoryName + "\" category.");
		}
		
		/// <summary>Gets all levels of subcategories of some wiki category (that means subcategories,
		/// sub-subcategories, and so on) and fills this PageList with titles of all pages, found in
		/// all levels of subcategories, including the titles of subcategories. The multiplicates of
		/// recurring pages and subcategories are removed. The operation may be very time-consuming
		/// and traffic-consuming. The function clears the PageList before filling.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>
		public void FillAllFromCategoryTree(string categoryName)
		{
			Clear();
			categoryName = site.CorrectNSPrefix(categoryName);					
			StringCollection doneCats = new StringCollection();
			FillAllFromCategory(categoryName);
			doneCats.Add(categoryName);
			for (int i = 0; i < Count(); i++)
				if (pages[i].GetNamespace() == 14 && !doneCats.Contains(pages[i].title)) {					
					FillAllFromCategory(pages[i].title);
					doneCats.Add(pages[i].title);
				}
			RemoveRecurring();
		}
										
		/// <summary>Gets page titles for this PageList from links in some wiki page. But only
		/// links to articles and pages from Wikipedia, Template and Help namespaces will be
		/// retrieved. And no interwiki links. Use FillFromAllPageLinks function instead
		/// to filter namespaces manually.</summary>
		/// <param name="pageTitle">Page name to get links from.</param>
		public void FillFromPageLinks(string pageTitle)
		{
			FillFromAllPageLinks(pageTitle);
			FilterNamespaces(new int[] {0,4,10,12});
		}
		
		/// <summary>Gets page titles for this PageList from all links in some wiki page. All links
		/// will be retrieved, from all standard namespaces, except interwiki links to other
		/// sites. Use FillFromPageLinks function instead to filter namespaces automatically.</summary>
		/// <param name="pageTitle">Page title as string.</param>
		/// <example><code>pageList.FillFromAllPages("Art", 0, true, 100);</code></example>
		public void FillFromAllPageLinks(string pageTitle)
		{
			string[] ns = new string[50];
			site.namespaces.Values.CopyTo(ns, 0);
			Site.wikiNSpaces.Values.CopyTo(ns, site.namespaces.Count);
			string nsRE = string.Join("|", ns).Trim("|".ToCharArray());
			Regex wikiLinkRE = new Regex(@"\[\[(((" + nsRE + @"):)?([^\:\|\]]+?))(]]|\|)");
			Page page = new Page(site, pageTitle);
			page.LoadEx();
			MatchCollection matches = wikiLinkRE.Matches(page.text);
		    foreach (Match match in matches)
			    pages.Add(new Page(site, match.Groups[1].Value));
			Console.WriteLine("PageList filled with links, found in \"" +
				pageTitle + "\".");
		}
				
		/// <summary>Gets page titles for this PageList from "Special:Whatlinkshere" Mediawiki page
		/// of specified page. That means the titles of pages, referring to the specified page.
		/// But not more than 5000 titles. The function does not internally remove redirecting
		///	pages from the results. Call RemoveRedirects() manually, if you need it. And the
		/// function does not clears the existing PageList, so new titles will be added.</summary>
		/// <param name="pageTitle">Page title as string.</param>		
		public void FillFromLinksToPage(string pageTitle)
		{
			//RemoveAll();
			Uri res = new Uri(site.site + site.indexPath +
				"index.php?title=Special:Whatlinkshere/" + pageTitle + "&limit=5000");
		    Bot.InitWebClient();
		    string src = Bot.wc.DownloadString(res);
		    MatchCollection matches = site.linkToPageRE1.Matches(src);
		    foreach (Match match in matches)
			    pages.Add(new Page(site, match.Groups[1].Value));
			//RemoveRedirects();
			Console.WriteLine("PageList filled with titles of pages, referring to \"" +
				pageTitle + "\".");
		}

		/// <summary>Gets page titles of pages, in which the specified image is included.</summary>
		/// <param name="imageFileTitle">Image file title. With or without "Image:" prefix.</param>	
		public void FillFromPagesUsingImage(string imageFileTitle)
		{			
			imageFileTitle = site.RemoveNSPrefix(imageFileTitle, 6);			
			Uri res = new Uri(site.site + site.indexPath + "index.php?title=Image:" + imageFileTitle);
		    Bot.InitWebClient();
		    string src = Bot.wc.DownloadString(res);
			int mi = src.IndexOf(site.messages["MediaWiki:Linkstoimage"].text);
			if (string.IsNullOrEmpty(src) == false && mi <= 0) {
				site.GetMediaWikiMessages(true);
				mi = src.IndexOf(site.messages["MediaWiki:Linkstoimage"].text);
			}
		    src = src.Substring(mi, src.IndexOf("<div class=\"printfooter\">") - mi);
		    MatchCollection matches = site.linkToPageRE1.Matches(src);
		    foreach (Match match in matches)
			    pages.Add(new Page(site, match.Groups[1].Value));
			Console.WriteLine("PageList filled with titles of pages, containing \"" +
				imageFileTitle + "\" image.");
		}
				
		/// <summary>Gets page titles for this PageList from user contributions
		/// of specified user. The function does not internally remove redirecting
		///	pages from the results. Call RemoveRedirects() manually, if you need it. And the
		/// function does not clears the existing PageList, so new titles will be added.</summary>
		/// <param name="userName">User's name.</param>
		/// <param name="limit">Maximum number of page titles to get.</param>
		public void FillFromUserContributions(string userName, int limit)
		{
			Uri res = new Uri(site.site + site.indexPath +
				"index.php?title=Special:Contributions&target=" + userName +
				"&limit=" + limit.ToString());			
		    Bot.InitWebClient();
		    string src = Bot.wc.DownloadString(res);
		    MatchCollection matches = site.linkToPageRE2.Matches(src);
		    foreach (Match match in matches)
			    pages.Add(new Page(site, match.Groups[1].Value));
			Console.WriteLine("PageList filled with user " + userName + " contributions.");
		}

		/// <summary>Gets page titles for this PageList from watchlist
		/// of bot account. The function does not internally remove redirecting
		///	pages from the results. Call RemoveRedirects() manually, if you need it. And the
		/// function neither filters namespaces, nor clears the existing PageList,
		/// so new titles will be added to the existing in PageList.</summary>
		public void FillFromWatchList()
		{
            string src = site.GetPageHTM(site.indexPath + "index.php?title=Special:Watchlist/edit");           
		    MatchCollection matches = site.linkToPageRE2.Matches(src);
		    Console.WriteLine(src);
		    foreach (Match match in matches)
			    pages.Add(new Page(site, match.Groups[1].Value));
			Console.WriteLine("PageList filled with bot account watchlist.");
		}

		/// <summary>Gets page titles for this PageList from list of recently changed 
		/// watched articles (watched by bot account). The function does not internally
		/// remove redirecting pages from the results. Call RemoveRedirects() manually,
		///	if you need it. And the function neither filters namespaces, nor clears
		/// the existing PageList, so new titles will be added to the existing
		/// in PageList.</summary>
		public void FillFromChangedWatchedPages()
		{
            string src = site.GetPageHTM(site.indexPath + "index.php?title=Special:Watchlist/edit");           
		    MatchCollection matches = site.linkToPageRE2.Matches(src);
		    Console.WriteLine(src);
		    foreach (Match match in matches)
			    pages.Add(new Page(site, match.Groups[1].Value));
			Console.WriteLine("PageList filled with bot account watchlist.");
		}		
		/// <summary>Gets page titles for this PageList from wiki site internal search results.
		/// The function does not filter namespaces. And the function does not clear
		/// the existing PageList, so new titles will be added.</summary>
		/// <param name="searchStr">String to search.</param>
		/// <param name="limit">Maximum number of page titles to get.</param>
		public void FillFromSearchResults(string searchStr, int limit)
		{
			Uri res = new Uri(site.site + site.indexPath +
				"index.php?title=Special:Search&fulltext=Search&search=" +
				HttpUtility.UrlEncode(searchStr) + "&limit=" + limit.ToString());			
		    Bot.InitWebClient();
		    string src = Bot.wc.DownloadString(res);
		    src = src.Substring(src.IndexOf("<div id='results'>"));
		    MatchCollection matches = site.linkToPageRE2.Matches(src);
		    foreach (Match match in matches)
			    pages.Add(new Page(site, match.Groups[1].Value));
			Console.WriteLine("PageList filled with search results.");
		}

		/// <summary>Gets page titles for this PageList from www.google.com search results.
		/// The function does not filter namespaces. And the function does not clear
		/// the existing PageList, so new titles will be added.</summary>
		/// <param name="searchStr">String to search.</param>
		/// <param name="limit">Maximum number of page titles to get.</param>
		public void FillFromGoogleSearchResults(string searchStr, int limit)
		{
			Uri res = new Uri("http://www.google.com/search?q=" +
				HttpUtility.UrlEncode(searchStr) + "+site:" +
				site.site.Substring(site.site.IndexOf("://") + 3) +
				"&num=" + limit.ToString());
		    Bot.InitWebClient();
		    string src = Bot.wc.DownloadString(res);
		    Regex GoogleLinkToPageRE = new Regex("<a class=l href=\"" + site.site + "(" +
		    	site.wikiPath + "|" + site.indexPath + @"index\.php\?title=)" + "([^\"]+?)\">");
		    MatchCollection matches = GoogleLinkToPageRE.Matches(src);
		    foreach (Match match in matches)
			    pages.Add(new Page(site,
			    	HttpUtility.UrlDecode(match.Groups[2].Value).Replace("_", " ")));
			Console.WriteLine("PageList filled with www.google.com search results.");
		}
										
		/// <summary>Gets page titles from UTF8-encoded file. Each title must be on separate line.
		/// The function does not clears the existing PageList, so new pages will be added.</summary>
		public void FillFromFile(string filePathName)
		{
			//RemoveAll();
			StreamReader strmReader = new StreamReader(filePathName);
			string input;
			while ((input = strmReader.ReadLine()) != null) {
				input = input.Trim(" []".ToCharArray());
				if (string.IsNullOrEmpty(input) != true)
					pages.Add(new Page(site, input));
			}
			strmReader.Close();
			Console.WriteLine("PageList filled with titles, found in \"" + filePathName + "\" file.");
		}
		
		/// <summary>Removes the pages, that are not in given namespaces.</summary>
		/// <param name="neededNSs">Array of integers, presenting keys of namespaces to retain.</param>
		/// <example><code>pageList.FilterNamespaces(new int[] {0,3});</code></example>
		public void FilterNamespaces(int[] neededNSs)
		{
			for (int i=pages.Count-1; i >= 0; i--) {
		    	if (Array.IndexOf(neededNSs, pages[i].GetNamespace()) == -1)
		    		pages.RemoveAt(i); }
		}

		/// <summary>Removes the pages, that are in given namespaces.</summary>
		/// <param name="needlessNSs">Array of integers, presenting keys of namespaces to remove.</param>
		/// <example><code>pageList.RemoveNamespaces(new int[] {2,4});</code></example>
		public void RemoveNamespaces(int[] needlessNSs)
		{
			for (int i=pages.Count-1; i >= 0; i--) {
		    	if (Array.IndexOf(needlessNSs, pages[i].GetNamespace()) != -1)
		    		pages.RemoveAt(i); }
		}

		/// <summary>This internal function sorts all pages in page list by titles.</summary>
		public void Sort()
		{
			pages.Sort(ComparePagesByTitles);
		}

		/// <summary>This internal function compares pages by titles (alphabetically).</summary>
		/// <returns>Returns 1 if x is greater, -1 if y is greater, 0 if equal.</returns>		
		public static int ComparePagesByTitles(Page x, Page y)
		{
			int r = string.CompareOrdinal(x.title, y.title);
			return (r != 0) ? r/Math.Abs(r) : 0;
		}
				
		/// <summary>Removes all pages in PageList from specified category by deleting
		/// links to that category in pages texts.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>		
		public void RemoveFromCategory(string categoryName)
		{
			foreach (Page p in pages)
				p.RemoveFromCategory(categoryName);
		}

		/// <summary>Adds all pages in PageList to the specified category by adding
		/// links to that category in pages texts.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>		
		public void AddToCategory(string categoryName)
		{
			foreach (Page p in pages)
				p.AddToCategory(categoryName);
		}
	
		/// <summary>Adds a specified template to the end of all pages in PageList.</summary>
		/// <param name="templateText">Template text, like "{{template_name|...|...}}".</param>		
		public void AddTemplate(string templateText)
		{
			foreach (Page p in pages)
				p.AddTemplate(templateText);
		}
							
		/// <summary>Loads text for pages in PageList from site via common web interface.</summary>
		public void Load()
		{
			foreach (Page page in pages)
				page.Load();	
		}

		/// <summary>Loads text and metadata for pages in PageList via XML export interface.
		/// Non-existent pages will be automatically removed from the PageList.</summary>		
		public void LoadEx()
		{
			if (IsEmpty()) {
				Console.WriteLine("The PageList is empty. Nothing to load.");
				return;
			}
			Console.WriteLine("Loading " + pages.Count + " pages...");
		    Uri res = new Uri(site.site + site.indexPath +
		    	"index.php?title=Special:Export&action=submit");
		    string postData = "curonly=True&pages=";
		    foreach (Page page in pages)
				postData += HttpUtility.UrlEncode(page.title) + "\r\n";
		    Bot.InitWebClient();
		    XmlReader reader = XmlReader.Create(new StringReader(Bot.wc.UploadString(res, postData)));		    
		    Clear();
		    while (reader.ReadToFollowing("page")) {
			    Page p = new Page(site, "");
			    p.ParsePageXML(reader.ReadOuterXml());
			    pages.Add(p);
		    }		    			
		}
						
		/// <summary>Loads text and metadata for pages in PageList via XML export interface.
		/// The function uses XPathNavigator and is less efficient than LoadEx().</summary>
		public void LoadEx2()
		{
			if (IsEmpty()) {
				Console.WriteLine("The PageList is empty. Nothing to load.");
				return;
			}			
			Console.WriteLine("Loading " + pages.Count + " pages...");
		    Uri res = new Uri(site.site + site.indexPath +
		    	"index.php?title=Special:Export&action=submit");
		    string postData = "curonly=True&pages=";
		    foreach (Page page in pages)
		    	postData += page.title + "\r\n";
		    Bot.InitWebClient();
		    string rawXML = Bot.wc.UploadString(res, postData);
		    string nsURI = rawXML.Substring(rawXML.IndexOf("xmlns=\"") + 7, 100);
		    nsURI = nsURI.Substring(0, nsURI.IndexOf("\""));
			StringReader strReader = new StringReader(rawXML);			
		    XPathDocument doc = new XPathDocument(strReader);
		    strReader.Close();
			XPathNavigator nav = doc.CreateNavigator();
			XmlNamespaceManager nsMngr = new XmlNamespaceManager(nav.NameTable);
			nsMngr.AddNamespace("ns", nsURI);
			foreach (Page page in pages)
			{	
				if (page.title.Contains("'")) {
					page.LoadEx();
					continue;
				}
				string query = "//ns:page[ns:title='" + page.title + "']/";
				try {
					page.text = nav.SelectSingleNode(query + "ns:revision/ns:text", nsMngr).InnerXml;					
				}
				catch (System.NullReferenceException) {
					continue;
				}
				page.text = HttpUtility.HtmlDecode(page.text);
				page.pageID = nav.SelectSingleNode(query + "ns:id", nsMngr).InnerXml;
				try {
					page.lastUser = nav.SelectSingleNode(query +
						"ns:revision/ns:contributor/ns:username", nsMngr).InnerXml;
					page.lastUserID = nav.SelectSingleNode(query +
						"ns:revision/ns:contributor/ns:id", nsMngr).InnerXml;
				}
				catch (System.NullReferenceException) {
					page.lastUser = nav.SelectSingleNode(query +
						"ns:revision/ns:contributor/ns:ip", nsMngr).InnerXml;
				}
				page.lastUser = HttpUtility.HtmlDecode(page.lastUser);
				page.lastRevisionID = nav.SelectSingleNode(query +
					"ns:revision/ns:id", nsMngr).InnerXml;
				page.lastMinorEdit = (nav.SelectSingleNode(query +
					"ns:revision/ns:minor", nsMngr) == null) ? false : true;
				try {
					page.comment = nav.SelectSingleNode(query + 
						"ns:revision/ns:comment", nsMngr).InnerXml;
					page.comment = HttpUtility.HtmlDecode(page.comment);
				}
				catch (System.NullReferenceException) {;}
				page.timestamp = nav.SelectSingleNode(query + 
					"ns:revision/ns:timestamp", nsMngr).ValueAsDateTime;
			}
			Console.WriteLine("Pages download completed.");
		}

		/// <summary>Loads text and metadata for pages in PageList via XML export interface.
		/// The function loads pages one by one and is slightly less efficient than LoadEx().</summary>
		public void LoadEx3()
		{
			if (IsEmpty()) {
				Console.WriteLine("The PageList is empty. Nothing to load.");
				return;
			}			
			foreach (Page p in pages)
				p.LoadEx();			
		}
		
		/// <summary>Gets page titles and page text from local XML dump.
		/// This function consumes much resources.</summary>
		/// <param name="filePathName">The path to and name of the XML dump file as string.</param>		
		public void FillAndLoadFromXMLDump(string filePathName)
		{
			Console.WriteLine("Loading pages from XML dump...");
		    XmlReader reader = XmlReader.Create(filePathName);
		    while (reader.ReadToFollowing("page")) {
			    Page p = new Page(site, "");
			    p.ParsePageXML(reader.ReadOuterXml());
			    pages.Add(p);
		    }
			Console.WriteLine("XML dump loaded successfully.");			
		}
		
		/// <summary>Gets page titles and page texts from all ".txt" files in the specified
		/// directory (folder). Each file becomes a page. Page titles are constructed from file names.
		/// Page text is read from file contents. If any Unicode numeric codes (also known as numeric
		/// character references or NCRs) of the forbidden characters (forbidden in filenames)
		/// are recognized in filenames, those codes are converted to characters
		/// (e.g. "&#x7c;" is converted to "|").</summary>
		/// <param name="dirPath">The path and name of a directory (folder) to load files from.</param>		
		public void FillAndLoadFromFiles(string dirPath)
		{
			foreach (string fileName in Directory.GetFiles(dirPath, "*.txt")) {
				Page p = new Page(site, Path.GetFileNameWithoutExtension(fileName));
				p.title = p.title.Replace("&#x22;", "\"");
		    	p.title = p.title.Replace("&#x3c;", "<");
		    	p.title = p.title.Replace("&#x3e;", ">");
		    	p.title = p.title.Replace("&#x3f;", "?");
		    	p.title = p.title.Replace("&#x3a;", ":");
		    	p.title = p.title.Replace("&#x5c;", "\\");
		    	p.title = p.title.Replace("&#x2f;", "/");
		    	p.title = p.title.Replace("&#x2a;", "*");
		    	p.title = p.title.Replace("&#x7c;", "|");
		    	p.LoadFromFile(fileName);
		    	pages.Add(p);
			}
		}		
		
		/// <summary>Saves all pages in PageList to live wiki site. Uses default bot
		/// edit comment and default minor edit mark setting ("true" by default). This function
		/// doesn't limit the saving speed, so in case of working on public wiki, it's better
		/// to use SaveSmoothly function in order not to overload public server (HTTP errors or
		/// framework errors may arise in case of overloading).</summary>
	    public void Save()
	    {
		    Save(Bot.editComment, Bot.isMinorEdit);		
		}

		/// <summary>Saves all pages in PageList to live wiki site. This function
		/// doesn't limit the saving speed, so in case of working on public wiki, it's better
		/// to use SaveSmoothly function in order not to overload public server (HTTP errors or
		/// framework errors may arise in case of overloading).</summary>
		/// <param name="comment">Your edit comment.</param>
		/// <param name="isMinorEdit">Minor edit mark (true = minor edit).</param>		
	    public void Save(string comment, bool isMinorEdit)
	    {
			foreach (Page page in pages)
				page.Save(page.text, comment, isMinorEdit);
		}

		/// <summary>Saves all pages in PageList to live wiki site. The function waits for 5 seconds
		/// between each page save operation in order not to overload server. Uses default bot
		/// edit comment and default minor edit mark setting ("true" by default). This function
		/// doesn't limit the saving speed, so in case of working on public wiki, it's better
		/// to use SaveSmoothly function in order not to overload public server (HTTP errors or
		/// framework errors may arise in case of overloading).</summary>
	    public void SaveSmoothly()
	    {
		    SaveSmoothly(5, Bot.editComment, Bot.isMinorEdit);		
		}

		/// <summary>Saves all pages in PageList to live wiki site. The function waits for specified
		/// number of seconds between each page save operation in order not to overload server.
		/// Uses default bot edit comment and default minor edit mark setting
		/// ("true" by default).</summary>
		/// <param name="intervalSeconds">Number of seconds to wait between each save operation.</param>		
	    public void SaveSmoothly(int intervalSeconds)
	    {
		    SaveSmoothly(intervalSeconds, Bot.editComment, Bot.isMinorEdit);		
		}
				
		/// <summary>Saves all pages in PageList to live wiki site. The function waits for specified
		/// number of seconds between each page save operation in order not to overload server.</summary>
		/// <param name="intervalSeconds">Number of seconds to wait between each save operation.</param>		
		/// <param name="comment">Your edit comment.</param>
		/// <param name="isMinorEdit">Minor edit mark (true = minor edit).</param>
	    public void SaveSmoothly(int intervalSeconds, string comment, bool isMinorEdit)
	    {
		    if (intervalSeconds == 0)
		    	intervalSeconds = 1;
			foreach (Page page in pages) {
				Thread.Sleep(intervalSeconds * 1000);
				page.Save(page.text, comment, isMinorEdit);				
			}
		}

		/*
		/// <summary>This is the interface for XML import. Not implemented yet.</summary>		
		public void SaveEx()
		{
			SaveEx(Bot.editComment, Bot.isMinorEdit);
		}
		
		/// <summary>This is the interface for XML import. Not implemented yet.</summary>		
	    public void SaveEx(string comment, bool isMinorEdit) {}
	    */	    
	    
		/// <summary>Saves titles of all pages in PageList to the specified file. Each title
		/// on a separate line. If the target file already exists, it is overwritten.</summary>
		/// <param name="filePathName">The path to and name of the target file as string.</param>
	    public void SaveTitlesToFile(string filePathName)
	    {
            string titles = "";
            foreach (Page page in pages)
            	titles += page.title + "\r\n";
			File.WriteAllText(filePathName, titles.Trim(), Encoding.UTF8);
            Console.WriteLine("Titles in PageList saved to \"" + filePathName + "\" file.");
		}

		/// <summary>Saves the contents of all pages in pageList to ".txt" files in specified directory.
		/// Each page is saved to separate file, the name of that file is constructed from page title.
		/// Forbidden characters in filenames are replaced with their Unicode numeric codes
		/// (also known as numeric character references or NCRs). If the target file already exists,
		/// it is overwritten.</summary>
		/// <param name="dirPath">The path and name of a directory (folder) to save files to.</param>		
		public void SaveToFiles(string dirPath)
		{
			string curDirPath = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(dirPath);
			foreach (Page page in pages)
            	page.SaveToFile();
            Directory.SetCurrentDirectory(curDirPath);
		}
		
		/// <summary>Loads the contents of all pages in pageList from live site via XML export
		/// and saves the retrieved XML content to the specified file. The functions just dumps
		/// data, it does not load pages in PageList itself, call LoadEx() or FillAndLoadFromXMLDump()
		/// to do that.</summary>
		/// <param name="filePathName">The path to and name of the target file as string.</param>		
		public void SaveXMLDumpToFile(string filePathName)
		{
			Console.WriteLine("Loading " + this.pages.Count + " pages for XML dump...");
			Uri res = new Uri(site.site + site.indexPath +
				"index.php?title=Special:Export&action=submit");
		    string postData = "curonly=True&pages=";
		    foreach (Page page in pages)
		    	postData += HttpUtility.UrlEncode(page.title) + "\r\n";
		    Bot.InitWebClient();
		    string rawXML = Bot.wc.UploadString(res, postData);
		    rawXML = rawXML.Replace("\n", "\r\n");
		    if (File.Exists(filePathName))
            	File.Delete(filePathName);
			FileStream fs = File.Create(filePathName);
            byte[] XMLBytes = new System.Text.UTF8Encoding(true).GetBytes(rawXML);
            fs.Write(XMLBytes, 0, XMLBytes.Length);
            fs.Close();
			Console.WriteLine("XML dump successfully saved in \"" + filePathName + "\" file.");
		}
						
		/// <summary>Removes all empty pages from PageList. But firstly don't forget to load
		/// the pages from site using pageList.LoadEx().</summary>		
	    public void RemoveEmpty()
	    {
		    for (int i=pages.Count-1; i >= 0; i--)
		   		if (pages[i].IsEmpty())
		    		pages.RemoveAt(i);
		}

		/// <summary>Removes all recurring pages from PageList. Only one page with some title will
		/// remain in PageList. This makes all page elements in PageList unique.</summary>		
	    public void RemoveRecurring()
	    {
		    for (int i=pages.Count-1; i >= 0; i--)
		    	for (int j=i-1; j >= 0; j--)
		   			if (pages[i].title == pages[j].title) {
		    			pages.RemoveAt(i);
		    			break;
	    			}
		}
				
		/// <summary>Removes all redirecting pages from PageList. But firstly don't forget to load
		/// the pages from site using pageList.LoadEx().</summary>		
	    public void RemoveRedirects()
	    {
		    for (int i=pages.Count-1; i >= 0; i--)
		    	if (pages[i].IsRedirect())
		    		pages.RemoveAt(i);
		}
		
		/// <summary>For all redirecting pages in this PageList, this function loads the titles and 
		/// texts of redirected-to pages.</summary>	
	    public void ResolveRedirects()
	    {
		    foreach(Page page in pages) {
		    	if (page.IsRedirect() == false)
		    		continue;
		    	page.title = page.RedirectsTo();
		    	page.LoadEx();
			}
		}
		
		/// <summary>Removes all disambigation pages from PageList. But firstly don't forget to load
		/// the pages from site using pageList.LoadEx().</summary>
	    public void RemoveDisambigs()
	    {
		    for (int i=pages.Count-1; i >= 0; i--)
		    	if (pages[i].IsDisambig())
		    		pages.RemoveAt(i);
		}

		
		/// <summary>Removes all pages from PageList.</summary>
	    public void RemoveAll()
	    {
		    pages.Clear();
		}

		/// <summary>Removes all pages from PageList.</summary>
	    public void Clear()
	    {
		    pages.Clear();
		}

		/// <summary>Function changes default english namespace prefixes to correct local prefixes
		/// (e.g. for german wiki-sites it changes "Category:..." to "Kategorie:...").</summary>	
	    public void CorrectNSPrefixes()
	    {
			foreach (Page p in pages)
			    p.CorrectNSPrefix();
		}
								
		/// <summary>Shows if there are any Page objects in this PageList.</summary>		
		public bool IsEmpty()
		{
			return (pages.Count == 0) ? true : false;
		}					    

		/// <summary>Sends titles of all contained pages to console.</summary>
	    public void ShowTitles()
	    {
		    Console.WriteLine("\nPages in this PageList:");
			foreach (Page p in pages)
				Console.WriteLine(p.title);
			Console.WriteLine("\n");
		}
				
		/// <summary>Sends texts of all contained pages to console.</summary>	
		public void ShowTexts()
		{
		    Console.WriteLine("\nPages in this PageList:");
		    Console.WriteLine("--------------------------------------------------");
			foreach (Page p in pages) {
				p.ShowText();
				Console.WriteLine("--------------------------------------------------");
			}
			Console.WriteLine("\n");
		}						
	}
	
	/// <summary>Class establishes custom application exceptions.</summary>
	public class WikiBotException : System.ApplicationException
	{
		/// <summary>Just overriding default constructor.</summary>
	    public WikiBotException() {}
	    /// <summary>Just overriding default constructor.</summary>
	    public WikiBotException(string message) : base (message) {}
	    /// <summary>Just overriding default constructor.</summary>
	    public WikiBotException(string message, System.Exception inner)
	    	: base (message, inner) {}
	    /// <summary>Just overriding default constructor.</summary>
	    protected WikiBotException(System.Runtime.Serialization.SerializationInfo info,
	        System.Runtime.Serialization.StreamingContext context)
	        : base (info, context) {}
	}
	
	/// <summary>Class defines a Bot instance and Main() function.</summary>
	public class Bot
	{
		/// <summary>Short description of this web agent.</summary>
		public static string botVer = "DotNetWikiBot/1.4";
		/// <summary>Content type for header of web client.</summary>
		public static string webContentType = "application/x-www-form-urlencoded";
		/// <summary>Default edit comment.</summary>
		public static string editComment = "Automatic page editing";
		/// <summary>If true, the bot edits will be marked as minor by default.</summary>
		public static bool isMinorEdit = true;
		/// <summary>If true, the bot will use "MediaWiki Query Interface" extension
		/// (special MediaWiki bot interface, "query.php"), if it is available.</summary>
		public static bool useBotQuery = true;
		/// <summary>If true, the bot will ask user to confirm next Save operation.</summary>
		public static bool askConfirm = false;
		/// <summary>Internal web client, that is used to access the site.</summary>
		public static WebClient wc = new WebClient();		

		/// <summary>This function asks user to confirm next operation. Make sure 
		/// to set "askConfirm" variable to "true" before calling this function.</summary>		
		public static bool UserConfirms()
		{
			if (!askConfirm)
				return true;
			ConsoleKeyInfo k;
			Console.Write("Would you like to proceed (y/n/a)? ");
			k = Console.ReadKey();
			Console.Write("\n");
			if (k.KeyChar == 'y')			
				return true;
			else if (k.KeyChar == 'a') {
				askConfirm = false;
				return true;
			}
			else
				return false;
		}
						
		/// <summary>This internal function initializes web client to get resources from web.</summary>		
		public static void InitWebClient()
		{   
			wc.Credentials = CredentialCache.DefaultCredentials;
			wc.Encoding = System.Text.Encoding.UTF8;
		    wc.Headers.Add("Content-Type", webContentType);
		    wc.Headers.Add("User-agent", botVer);
		}
	}
}