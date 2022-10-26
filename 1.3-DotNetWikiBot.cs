// DotNetWikiBot Framework v1.1 - bot framework based on Microsoft® .NET Framework 2.0 for wiki projects
// Distributed under the terms of the MIT (X11) license: http://www.opensource.org/licenses/mit-license.php
// Copyright © Iaroslav Vassiliev (2006) codedriller@gmail.com

using System;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
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
		/// <summary>Relative path to wiki pages.</summary>
		public string wikiPath = "/wiki/";
		/// <summary>Relative path to "index.php".</summary>
		public string indexPath = "/w/";
		/// <summary>Redirect directive.</summary>
		public string redirectStr = "#REDIRECT ";
		/// <summary>Regular expression to find redirect target.</summary>
		public Regex redirectRE = new Regex(@"#REDIRECT \[\[(.*?)]]");
		/// <summary>Regular expression to find links to pages in HTML source.</summary>
		public Regex linkToPageRE = new Regex("<li><a href=\"[^\"]*?\" title=\"([^\"]*?)\">");
		/// <summary>Regular expression to find links to pages in HTML source.</summary>
		public Regex pageTitleTagRE = new Regex("<title>([^<]*?)</title>");
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
		public Regex siteLangRE = new Regex(@"http://(.+?)\..+?\..+?/");
		/// <summary>Regular expression to extract edit session time attribute.</summary>
		public Regex editSessionTimeRE = new Regex("value=\"([^\"]*?)\" name=\"wpEdittime\"");
		/// <summary>Regular expression to extract edit session token attribute.</summary>
		public Regex editSessionTokenRE1 = new Regex("value=\"([^\"]*?)\" name=\"wpEditToken\"");
		/// <summary>Alternative regular expression to extract edit session token attribute.</summary>
		public Regex editSessionTokenRE2 = new Regex("name='wpEditToken' value=\"([^\"]*?)\"");
		/// <summary>Login cookies.</summary>
		public CookieCollection cookies;
		/// <summary>Local namespaces.</summary>
		public Hashtable namespaces = new Hashtable();
		/// <summary>Default namespaces.</summary>
		public static Hashtable wikiNSpaces = new Hashtable();
		/// <summary>List of Wikimedia Foundation sites and prefixes.</summary>
		public static Hashtable WMSites = new Hashtable();
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
	    	LoadDefaults();
	    	LogIn();
	    	GetInfo();			
	    	//ShowNamespaces();	    	
    	}

    	/// <summary>This constructor uses default site, userName and password.</summary>		
    	public Site()
    	{
	    	LoadDefaults();
	    	LogIn();
	    	GetInfo();	    	
			//ShowNamespaces();
    	}
    	
    	/// <summary>Retrieves metadata and local namespace names from site.</summary>
    	public void GetInfo()
    	{
	    	Uri res = new Uri(site + wikiPath + "Special:Export/Non-existing-page");
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
		  	language = siteLangRE.Match(site).Groups[1].ToString();
		  	wikiCategoryRE = new Regex(@"\[\[(?i)(((" + wikiNSpaces["14"] + "|" +
		  		namespaces["14"] + @"):(.+?))(\|(.+?))?)]]");
		  	wikiImageRE = new Regex(@"\[\[(?i)((" + wikiNSpaces["6"] + "|" +
		  		namespaces["6"] + @"):(.+?))(\|(.+?))*?]]");
      		Console.WriteLine("Site: " + name + " (" + generator + ")");
      		if (Bot.useBotQuery == false)
      			return;      			
	    	Uri queryPHP = new Uri(site + indexPath + "query.php");
		    Bot.InitWebClient();
		    string respStr = Bot.wc.DownloadString(queryPHP);
		    if (respStr.IndexOf("<title>MediaWiki Query Interface</title>") != -1)
		    	botQuery = true;			
    	}

    	/// <summary>Loads default english namespace names for site.</summary>    	
    	public void LoadDefaults()
    	{
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
	    }

	    /// <summary>Logs in and retrieves cookies.</summary>    	  	
	    private void LogIn()
	    {
	        HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(site + indexPath +
	        	"index.php?title=Special:Userlogin&action=submitlogin&type=login");
	        String postData = String.Format("wpName=+{0}&wpPassword={1}&wpRemember=1&wpLoginattempt=Log+in",
	            new string[] {userName, userPass});	
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
	        try {
	        	HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse();
	        	cookies = webResp.Cookies;
	        	webResp.Close();
        	}
        	catch (WebException) {
	        	if (indexPath == "/wiki/")
	        		throw new WikiBotException("\n\nLogin failed. Check your username and password.\n");
				indexPath = "/wiki/";
				wikiPath = "/wiki/index.php?title=";
				LogIn();
				return;	        	
        	}
	        Page testLogin = new Page(this, "Non-existing-page");
	        testLogin.GetEditSessionData();
			if (String.IsNullOrEmpty(testLogin.editSessionToken)) {
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
		Site site;

	    /// <summary>Constructs Page object with specified title and specified Site object.
	    /// When constructed, new Page object doesn't contain text. Use page.Load() to get text from live
	    /// wiki. Or use page.LoadEx() to get both text and metadata via XML export interface.</summary>
		/// <param name="site">Site object, it must be constructed beforehand.</param>
		/// <param name="title">Page title as string.</param>
		/// <returns>Returns Page object.</returns>		
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
		    	return;
		    }
		    Console.WriteLine("Page \"" + title + "\" loaded successfully.");
		}

		/// <summary>Loads page text and metadata via XML export interface. It is slower,
		/// than Load(), don't use it if you don't need page metadata (page id, timestamp,
		/// comment, last contributor, minor edit mark).</summary>			
		public void LoadEx()
	    {
		    Uri res = new Uri(site.site + site.wikiPath + "Special:Export/" + title);
		    Bot.InitWebClient();
		    XmlDocument doc = new XmlDocument();
		    doc.LoadXml(Bot.wc.DownloadString(res));
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
			lastMinorEdit = doc.GetElementsByTagName("minor").Count != 0 ? true : false;
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
	        HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(site.site + 
	        	site.indexPath + "index.php?title=" + title + "&action=edit");
            webReq.Proxy.Credentials = CredentialCache.DefaultCredentials;
	        webReq.ContentType = Bot.webContentType;
            webReq.UserAgent = Bot.botVer;
            webReq.CookieContainer = new CookieContainer();
            foreach (Cookie cookie in site.cookies)
                webReq.CookieContainer.Add(cookie);
            HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse();
            StreamReader strmReader = new StreamReader(webResp.GetResponseStream());
            string src = strmReader.ReadToEnd();
			editSessionTime = site.editSessionTimeRE.Match(src).Groups[1].ToString();
		    editSessionToken = site.editSessionTokenRE1.Match(src).Groups[1].ToString();
		    if (String.IsNullOrEmpty(editSessionToken))
		    	editSessionToken = site.editSessionTokenRE2.Match(src).Groups[1].ToString();
            strmReader.Close();
            webResp.Close();
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
		    if (String.IsNullOrEmpty(editSessionTime) || String.IsNullOrEmpty(editSessionToken)) {
			    Console.WriteLine("Insufficient rights to edit page \"" + title + "\".");
			    return;
			}
			HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(site.site + 
	        	site.indexPath + "index.php?title=" + title + "&action=submit");
            webReq.Proxy.Credentials = CredentialCache.DefaultCredentials;
	        webReq.Method = "POST";
	        webReq.ContentType = Bot.webContentType;
            webReq.UserAgent = Bot.botVer;
            webReq.CookieContainer = new CookieContainer();
            foreach (Cookie cookie in site.cookies)
                webReq.CookieContainer.Add(cookie);
            string postData = string.Format("wpSection=&wpStarttime={0}&wpEdittime={1}&wpScrolltop=" +
            	"&wpTextbox1={2}&wpWatchThis=off&wpSummary={3}&wpSave=Save%20Page&wpEditToken={4}",
                new string[] { DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmss"), editSessionTime,
                HttpUtility.UrlEncode(newText), HttpUtility.UrlEncode(comment), editSessionToken });
            if (isMinorEdit)
            	postData = postData.Insert(postData.IndexOf("wpSummary"), "wpMinoredit=1&");
            byte[] postBytes = Encoding.UTF8.GetBytes(postData);
            webReq.ContentLength = postBytes.Length;
            Stream reqStrm = webReq.GetRequestStream();
            reqStrm.Write(postBytes, 0, postBytes.Length);
            reqStrm.Close();            	
            HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse();
            StreamReader strmReader = new StreamReader(webResp.GetResponseStream());
            string respStr = strmReader.ReadToEnd();
            strmReader.Close();
            webResp.Close();
			if (site.editSessionTokenRE1.IsMatch(respStr))
				Console.WriteLine("Edit conflict occured when saving page \"" + title + "\".");   		
   			else {
		    	Console.WriteLine("Page \"" + title + "\" saved successfully.");
		    	text = newText;
	    	}
		}
		/*
	    public void SaveEx()
	    {
		    SaveEx(text, Bot.editComment, Bot.isMinorEdit);		
		}

		public void SaveEx(string newText)
	    {
		    SaveEx(newText, Bot.editComment, Bot.isMinorEdit);		
		}
		
		public void SaveEx(string comment, bool isMinorEdit)
	    {
		    SaveEx(text, comment, isMinorEdit);		
		}
		
		/// <summary>This is the interface for XML import. Not implemented yet.</summary>
	    public void SaveEx(string newText, string comment, bool isMinorEdit) {}
	    */
	    
	    /// <summary>Saves page text to the specified file.</summary>
	    /// <param name="filePathName">Path and name of the file.</param>
	    public void SaveToFile(string filePathName)
	    {
		    if(IsEmpty()) {
		    	Console.WriteLine("Page \"" + title + "\" contains no text to save.");
		    	return;
	    	}
            File.WriteAllText(filePathName, text, Encoding.UTF8);
			Console.WriteLine("Text of \"" + title + "\" page successfully saved in \"" +
				filePathName + "\" file.");
		}

		/// <summary>Saves page text to the file. The name of the file is constructed
		/// from the title of the article.</summary>
	    public void SaveToFile()
	    {
		    SaveToFile(Regex.Replace(title, "[\\\\/:?*<>\"|]", "") + ".txt");
		}
			    
	    /// <summary>Returns true, if page.text field is empty. Don't forget to call
	    /// page.Load() before using this function.</summary>
	    public bool IsEmpty()
	    {
			return String.IsNullOrEmpty(text);    
		}

		/// <summary>Returns true, if page.text field is not empty. Don't forget to call
	    /// page.Load() before using this function.</summary>
		public bool Exists()
	    {
			return (String.IsNullOrEmpty(text) == true) ? false : true;    
		}
		
		/// <summary>Returns true, if page redirects to another page. Don't forget to load
		/// actual page contents from live wiki "page.Load()" before using this function.</summary>		
	    public bool IsRedirect()
	    {		    
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
		    if (String.IsNullOrEmpty(Site.WMLangsStr))
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
			if (String.IsNullOrEmpty(Site.WMLangsStr))
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
		    if (String.IsNullOrEmpty(Site.WMLangsStr))
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
		/// if found. Like "Stars|D3" (if [[Category:Stars|D3]]), not just "Stars".</param>
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
		/// link to that category in page text.</summary>
		/// <param name="categoryName">Category name, with or without prefix.</param>		
		public void AddToCategory(string categoryName)
		{
			categoryName = categoryName.Trim("[]\f\n\r\t\v ".ToCharArray());			
			categoryName = Regex.Replace(categoryName, "(?i)^:?(" + site.namespaces["14"] + 
				"|" + Site.wikiNSpaces["14"] + "):", "");
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
			categoryName = Regex.Replace(categoryName, "(" + site.namespaces["14"] + "|" +
				Site.wikiNSpaces["14"] + "):", "");
			text = Regex.Replace(text, @"\[\[(" + site.namespaces["14"] + "|" +
				Site.wikiNSpaces["14"] + "):" + categoryName + @"(\|.*?)?]]\r?\n?", "");
			text = text.TrimEnd("\r\n".ToCharArray());
		}
						
		/// <summary>Returns the array of strings, containing names of template, used in page.
		/// But links to templates, like [[:Template:...]], are not included.</summary>
		/// <param name="withNameSpacePrefix">If true, function will return strings with
		/// namespace prefix like "Template:SomeTemplate", not just "SomeTemplate".</param>
		/// <returns>Returns the string[] array.</returns>
	    public string[] GetTemplates(bool withNameSpacePrefix)
	    {	    	    	
		    MatchCollection matches = site.wikiTemplateRE.Matches(text);
		    string[] matchStrings = new string[matches.Count];
		    for(int i = 0; i < matches.Count; i++)
		    	if (withNameSpacePrefix == true)
		    		matchStrings[i] = site.namespaces["10"] + ":" + matches[i].Groups[1].Value;
		    	else
			    	matchStrings[i] = matches[i].Groups[3].Value;
		    return matchStrings;
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
		
		/// <summary>Renames the page.</summary>
	    /// <param name="newTitle">New title of that page.</param>
	    /// <param name="reason">Reason for renaming.</param>	
		public void RenameTo(string newTitle, string reason)
		{
			Page mp = new Page(site, "Special:Movepage/" + HttpUtility.UrlEncode(title));
			mp.GetEditSessionData();
			if (String.IsNullOrEmpty(mp.editSessionToken)) {
				Console.WriteLine("Unable to rename page \"" + title + "\" to \"" + 
					newTitle + "\".");
				return;
			}
       		HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(site.site +
				site.indexPath + "index.php?title=Special:Movepage&action=submit");       
			webReq.Proxy.Credentials = CredentialCache.DefaultCredentials;
			webReq.Method = "POST";
			webReq.ContentType = Bot.webContentType;
			webReq.UserAgent = Bot.botVer;
			webReq.CookieContainer = new CookieContainer();
			string postData = string.Format("wpNewTitle={0}&wpOldTitle={1}&wpEditToken={2}" +
				"&wpReason={3}", HttpUtility.UrlEncode(newTitle), HttpUtility.UrlEncode(title),
				mp.editSessionToken, HttpUtility.UrlEncode(reason));
			foreach (Cookie cookie in site.cookies)
				webReq.CookieContainer.Add(cookie);
			byte[] postBytes = Encoding.UTF8.GetBytes(postData);
			webReq.ContentLength = postBytes.Length;
			Stream reqStrm = webReq.GetRequestStream();
			reqStrm.Write(postBytes, 0, postBytes.Length);
			reqStrm.Close();
			HttpWebResponse webResp = (HttpWebResponse) webReq.GetResponse();
			StreamReader strmReader = new StreamReader(webResp.GetResponseStream());
			string respStr = strmReader.ReadToEnd();
			strmReader.Close();
			webResp.Close();
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
	        HttpWebRequest webReq1 = (HttpWebRequest)WebRequest.Create(site.site + 
	        	site.indexPath + "index.php?title=" + title + "&action=delete");
            webReq1.Proxy.Credentials = CredentialCache.DefaultCredentials;
	        webReq1.ContentType = Bot.webContentType;
            webReq1.UserAgent = Bot.botVer;
            webReq1.CookieContainer = new CookieContainer();
            foreach (Cookie cookie in site.cookies)
                webReq1.CookieContainer.Add(cookie);
            HttpWebResponse webResp1 = (HttpWebResponse)webReq1.GetResponse();
            StreamReader strmReader1 = new StreamReader(webResp1.GetResponseStream());
            string respStr1 = strmReader1.ReadToEnd();
		    editSessionToken = site.editSessionTokenRE1.Match(respStr1).Groups[1].ToString();
		    if (String.IsNullOrEmpty(editSessionToken))
		    	editSessionToken = site.editSessionTokenRE2.Match(respStr1).Groups[1].ToString();
            strmReader1.Close();
            webResp1.Close();						
			if (String.IsNullOrEmpty(editSessionToken)) {
				Console.WriteLine("Unable to delete page \"" + title + "\".");
				return;
			}
       		HttpWebRequest webReq2 = (HttpWebRequest)WebRequest.Create(site.site +
				site.indexPath + "index.php?title=" + title + "&action=delete");       
			webReq2.Proxy.Credentials = CredentialCache.DefaultCredentials;
			webReq2.Method = "POST";
			webReq2.ContentType = Bot.webContentType;
			webReq2.UserAgent = Bot.botVer;
			webReq2.CookieContainer = new CookieContainer();
			string postData = string.Format("wpReason={0}&wpEditToken={1}",
				HttpUtility.UrlEncode(reason), editSessionToken);
			foreach (Cookie cookie in site.cookies)
				webReq2.CookieContainer.Add(cookie);
			byte[] postBytes = Encoding.UTF8.GetBytes(postData);
			webReq2.ContentLength = postBytes.Length;
			Stream reqStrm = webReq2.GetRequestStream();
			reqStrm.Write(postBytes, 0, postBytes.Length);
			reqStrm.Close();
			HttpWebResponse webResp2 = (HttpWebResponse) webReq2.GetResponse();
			StreamReader strmReader2 = new StreamReader(webResp2.GetResponseStream());
			string respStr2 = strmReader2.ReadToEnd();
			strmReader2.Close();
			webResp2.Close();
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
		Site site;
		
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
		}
		
		/// <returns>Returns the PageList object.</returns>
		public PageList(Site site, StringCollection pageNames)
		{
			this.site = site;
			foreach (string pageName in pageNames)
				pages.Add(new Page(site, pageName));
		}
		
		/// <summary>This index allows to call pageList[i] instead of pageList.pages[i].</summary>
		public Page this[int index]
		{
        	get { return pages[index]; }
        	set { pages[index] = value; }
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
		/// <param name="quantity">Quantity of page titles to get.</param>
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

		/// <summary>Gets subcategories titles for this PageList from specified wiki category page,
		/// excluding other pages. Use FillFromCategory function to get other pages.</summary>
		/// <param name="categoryName">Name of category with prefix, like "Category:...".</param>		
		public void FillSubsFromCategory(string categoryName)
		{
			int count = pages.Count;
			FillAllFromCategory(categoryName);
			FilterNamespaces(new int[] {14});
			if (pages.Count != count)
				Console.WriteLine("PageList filled with subcategory page titles, found in \"" +
					categoryName + "\" category.");
			else
				Console.WriteLine("Nothing was found in that category.");
		}
		
		/// <summary>Gets page titles for this PageList from specified wiki category page, excluding
		/// subcategories. Use FillSubsFromCategory function to get subcategories.</summary>
		/// <param name="categoryName">Name of category with prefix, like "Category:...".</param>		
		public void FillFromCategory(string categoryName)
		{
			int count = pages.Count;
			FillAllFromCategory(categoryName);
			RemoveNamespaces(new int[] {14});
			if (pages.Count != count)
				Console.WriteLine("PageList filled with page titles, found in \"" +
					categoryName + "\" category.");
			else
				Console.WriteLine("Nothing was found in that category.");	
		}
		
		/// <summary>This internal function gets all page titles for this PageList from specified
		/// category page, including subcategories.</summary>
		/// <param name="categoryName">Name of category with prefix, like "Category:...".</param>
		public void FillAllFromCategory(string categoryName)
		{
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
		    	matches = site.linkToPageRE.Matches(src);
		    	foreach (Match match in matches)
			    	pages.Add(new Page(site, match.Groups[1].Value));
			}
			while(nextPortionRE.IsMatch(src));
		}

		/// <summary>This internal function gets all page titles for this PageList from specified
		/// category page using "query.php" interface. It gets titles portion (usually 200) by portion.
		/// It gets subcategories too.</summary>
		/// <param name="categoryName">Name of category with prefix, like "Category:...".</param>
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
			Sort();
		}

		/// <summary>This internal function gets all page titles for this PageList from specified
		/// category page using "query.php" interface. It gets subcategories too.</summary>
		/// <param name="categoryName">Name of category with prefix, like "Category:...".</param>
		public void FillAllFromCategoryEx2(string categoryName)
		{
			string src = "";
			MatchCollection matches;
			Uri res = new Uri(site.site + site.indexPath + "query.php?what=category&cptitle=" +
				categoryName + "&format=xml");
		    Bot.InitWebClient();
		    src = Bot.wc.DownloadString(res);		    	
		    matches = site.pageTitleTagRE.Matches(src);
		    foreach (Match match in matches)
			    pages.Add(new Page(site, match.Groups[1].Value));
		}
						
		/// <summary>Gets page titles for this PageList from links in some wiki page. But only
		/// links to articles and pages from Wikipedia, Template and Help namespaces will be
		/// retrieved. And no interwiki links. Use FillFromAllPageLinks function instead
		/// to filter namespaces manually.</summary>
		/// <param name="pageTitle">Page title as string.</param>
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
			string nsRE = String.Join("|", ns).Trim("|".ToCharArray());
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
		    XmlDocument doc = new XmlDocument();
		    string src = Bot.wc.DownloadString(res);
		    MatchCollection matches = site.linkToPageRE.Matches(src);
		    foreach (Match match in matches)
			    pages.Add(new Page(site, match.Groups[1].Value));
			//RemoveRedirects();
			Console.WriteLine("PageList filled with titles of pages, referring to \"" +
				pageTitle + "\".");
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
				if (String.IsNullOrEmpty(input) != true)
					pages.Add(new Page(site, input));
			}
			strmReader.Close();
			Console.WriteLine("PageList filled with titles, founs in \"" + filePathName +
				"\" file.");
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
			int r = String.CompareOrdinal(x.title, y.title);
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
				
		/// <summary>Loads text for pages in PageList from site via common web interface.</summary>
		public void Load()
		{
			foreach (Page page in pages)
				page.Load();	
		}
		
		/// <summary>Loads text and metadata for pages in PageList via XML export interface.</summary>		
		public void LoadEx()
		{
			Console.WriteLine("Loading " + this.pages.Count + " pages...");
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
				page.lastRevisionID = nav.SelectSingleNode(query +
					"ns:revision/ns:id", nsMngr).InnerXml;
				page.lastMinorEdit = (nav.SelectSingleNode(query +
					"ns:revision/ns:minor", nsMngr) == null) ? false : true;
				try {
					page.comment = nav.SelectSingleNode(query + 
						"ns:revision/ns:comment", nsMngr).InnerXml;
				}
				catch (System.NullReferenceException) {;}
				page.timestamp = nav.SelectSingleNode(query + 
					"ns:revision/ns:timestamp", nsMngr).ValueAsDateTime;
			}
			Console.WriteLine("Pages download completed.");
		}

		// Gets page titles and text from local XML dump. Not implemented yet.		
		//public void LoadFromXMLDump() {}
		
		/// <summary>Saves all pages in PageList to live wiki. Uses default bot
		/// edit comment and default minor edit mark setting ("true" in most cases).</summary>
	    public void Save()
	    {
		    Save(Bot.editComment, Bot.isMinorEdit);		
		}

		/// <summary>Saves all pages in PageList to live wiki site.</summary>
		/// <param name="comment">Your edit comment.</param>
		/// <param name="isMinorEdit">Minor edit mark (true = minor edit).</param>		
	    public void Save(string comment, bool isMinorEdit)
	    {
			foreach (Page page in pages)
				page.Save(page.text, comment, isMinorEdit);
		}
		/*
		public void SaveEx()
		{
			SaveEx(Bot.editComment, Bot.isMinorEdit);
		}
		
		/// <summary>This is the interface for XML import. Not implemented yet.</summary>		
	    public void SaveEx(string comment, bool isMinorEdit) {}
	    */	    
	    
		/// <summary>Saves titles of all pages in PageList to the specified file.
		/// Each title on a separate line.</summary>
		/// <param name="filePathName">The path to and name of the target file as string.</param>
	    public void SaveTitlesToFile(string filePathName)
	    {
            string titles = "";
            foreach (Page page in pages)
            	titles += page.title + "\r\n";
			File.WriteAllText(filePathName, titles.Trim(), Encoding.UTF8);
            Console.WriteLine("Titles in PageList saved to \"" + filePathName + "\" file.");
		}

		/// <summary>Loads the contents of all pages in pageList from live site via XML export
		/// and saves the retrieved XML content to the specified file. The functions just dumps
		/// data, it does not load pages in PageList itself, call LoadEx() to to that.</summary>
		/// <param name="filePathName">The path to and name of the target file as string.</param>		
		public void SaveXMLDumpToFile(string filePathName)
		{
			Console.WriteLine("Loading " + this.pages.Count + " pages for XML dump...");
			Uri res = new Uri(site.site + site.indexPath +
				"index.php?title=Special:Export&action=submit");
		    string postData = "curonly=True&pages=";
		    foreach (Page page in pages)
		    	postData += page.title + "\r\n";
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
						
		/// <summary>Shows if there are any Page objects in this PageList.</summary>		
		public bool IsEmpty()
		{
			return (pages.Count == 0) ? true : false;
		}					    
		
		/// <summary>Sends all the titles of contained pages to console.</summary>
	    public void ShowTitles()
	    {
		    Console.WriteLine("\nPages in this PageList:");
			foreach (Page p in pages)
				Console.WriteLine(p.title);
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
		public static string botVer = "DotNetWikiBot/1.0";
		/// <summary>Content type for header of web client.</summary>
		public static string webContentType = "application/x-www-form-urlencoded";
		/// <summary>Default edit comment.</summary>
		public static string editComment = "Automatic page editing";
		/// <summary>If true, the bot edits will be marked as minor by default.</summary>
		public static bool isMinorEdit = true;
		/// <summary>If true, the bot will use "MediaWiki Query Interface" extension
		/// (special MediaWiki bot interface, "query.php"), if it is available.</summary>
		public static bool useBotQuery = true;
		/// <summary>Internal web client, that is used to access the site.</summary>
		public static WebClient wc = new WebClient();
		
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