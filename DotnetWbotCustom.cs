using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.IO;
using System.Diagnostics;
using System.Net;
using HtmlAgilityPack;
using System.Web;
using botSolution;

namespace DotNetWikiBot
{
    public partial class PageList
    {
        public void FillFromFile(string filePathName, bool noErrorOnNoFile)
        {
            if (File.Exists(filePathName))
                FillFromFile(filePathName);
        }

        public void FillFromCategoryTree(string categoryName, StringCollection stopList )
        {
            FillAllFromCategoryTree(categoryName);
            RemoveNamespaces(new int[] { 14 });
            if (pages.Count != 0)
                Console.WriteLine(
                    Bot.Msg("PageList filled with {0} page titles, found in \"{1}\" category."),
                    Count().ToString(), categoryName);
            else
                Console.Error.WriteLine(
                    Bot.Msg("Nothing was found in \"{0}\" category."), categoryName);
        }

        public void FillAllFromCategoryTree(string categoryName, StringCollection stopList)
		{
			
			Clear();
			//categoryName = site.CorrectNSPrefix(categoryName);
			StringCollection doneCats = new StringCollection();
            FillAllFromCategory(categoryName);
			doneCats.Add(categoryName);
			for (int i = 0; i < Count(); i++)
				if (pages[i].GetNamespace() == 14 
                    && !doneCats.Contains(pages[i].title)
                    && !stopList.Contains(pages[i].title)
                    ) 
                {
					FillAllFromCategory(pages[i].title);
					doneCats.Add(pages[i].title);
				}
			RemoveRecurring();

			
		}

        public static bool StrСontains(string str, string [] words)
        {
            if (words == null)
                return false;

            foreach (string word in words)
            {
                if (str.Contains(word) || str.Contains(word.ToLower()))                
                    return true;
            }
            return false;
        }

        public void FillAllFromCategoryTreeUkrainian(string categoryName, bool excludeNotExistingVillages)
        {
            List<string> include = new List<string>();
            include.Add("област");
            include.Add("Крым");
            include.Add("Украин");

            string[] excl = null;
            if (excludeNotExistingVillages)
                excl = new string[] { "Исчезнувшие населённые пункты" };

            FillAllFromCategoryTree(categoryName, include.ToArray(), excl);

            this.FilterNamespaces(new int[] { 0 }); // only pages will remain
            
        }

        /// <summary>Gets subcategories titles for this PageList from specified wiki category page,
        /// excluding other pages. Use FillFromCategory function to get other pages.</summary>
        /// <param name="categoryName">Category name, with or without prefix.</param>
        public void FillSubsFromCategory(string categoryName)
        {
            int count = pages.Count;
            PageList pl = new PageList(site);
            pl.FillAllFromCategory(categoryName);
            pl.FilterNamespaces(new int[] { 14 });
            pages.AddRange(pl.pages);
            if (pages.Count != count)
                Console.WriteLine(Bot.Msg("PageList filled with {0} subcategory page titles, " +
                    "found in \"{1}\" category."), (pages.Count - count).ToString(), categoryName);
            else
                Console.Error.WriteLine(
                    Bot.Msg("Nothing was found in \"{0}\" category."), categoryName);
        }

        public void FillSubsFromCategoryTree(string categoryName, int depth, string[] includeCategoryWords,
            string[] excludeCategoryWords = null)
        {
            if (depth == 0)
                return;

            PageList list = new PageList(this.site);
            
            
            list.FillSubsFromCategory(categoryName);

            var newIncludeList = from page in list.pages
                                 where StrСontains(page.title,
                        includeCategoryWords) && !StrСontains(page.title,
                        excludeCategoryWords)
                select page;

            foreach (Page p in newIncludeList)
            {
                PageList l2 = new PageList(this.site);
                l2.FillSubsFromCategoryTree(p.title, depth-1,includeCategoryWords,excludeCategoryWords);
                this.pages.AddRange(l2.pages);
            }
            this.pages.AddRange(list.pages);
            
        }
    

        public void FillAbsentFromCategoryTree(string categoryName, string absentLanguage)
        {
            //http://tools.wmflabs.org/joanjoc/sugart.php?l1=ru&pr=wiki&l2=uk&cat=%D0%90%D0%B2%D0%B8%D0%B0%D1%86%D0%B8%D1%8F+%D0%A3%D0%BA%D1%80%D0%B0%D0%B8%D0%BD%D1%8B&dpt=3&tpl=&uselang=en&go=Proceed
            //l1=ru&pr=wiki&l2=uk&cat=%D0%90%D0%B2%D0%B8%D0%B0%D1%86%D0%B8%D1%8F+%D0%A3%D0%BA%D1%80%D0%B0%D0%B8%D0%BD%D1%8B&dpt=3&tpl=&uselang=en&go=Proceed
            string url = "http://tools.wmflabs.org/joanjoc/sugart.php?";
 
            NameValueCollection coll = new NameValueCollection();
            coll.Add("l1", site.language);
            coll.Add("l2", absentLanguage);
            coll.Add("pr","wiki");
            coll.Add("cat",categoryName);
            coll.Add("dpt","3");
            coll.Add("go","Proceed");
            coll.Add("uselang","en");            

            PostSubmitter ps = new PostSubmitter("http://tools.wmflabs.org/joanjoc/sugart.php", coll);
            ps.Type = PostSubmitter.PostTypeEnum.Get;
            string result = ps.Post();

            HtmlAgilityPack.HtmlNode.ElementsFlags.Remove("form"); 
            HtmlDocument doc = new HtmlDocument();
            doc.OptionOutputAsXml = true;
            doc.LoadHtml(result);

            var linkTags = doc.DocumentNode.Descendants("link");
            var linkedPages = doc.DocumentNode.Descendants("a")
                                              .Select(a => a.GetAttributeValue("href", null))
                                              .Where(u => !String.IsNullOrEmpty(u));


            //var ru = (from c in plRu.pages
            //          select c.title).Skip(j * splitNumber).Take(splitNumber);

            

            var listOfLang = from lp in linkedPages
                       where lp.Contains(site.language+".wikipedia.org") 
                             select new Uri(lp);

            
            

            var list = from lol in listOfLang
                       select new Page(this.site, HttpUtility.UrlDecode(lol.Segments.Last()) );
            
            this.pages.AddRange(list); 
            
        }


        public void FillAllFromCategoryTree(string categoryName,
            string[] includeCategoryWords, 
            string[] excludeCategoryWords = null)
            
        {            
            Clear();
            categoryName = site.CorrectNsPrefix(categoryName);
            StringCollection doneCats = new StringCollection();
            FillAllFromCategory(categoryName);
            doneCats.Add(categoryName);
            for (int i = 0; i < Count(); i++)
            {
                if (pages[i].GetNamespace() == 14
                    && !doneCats.Contains(pages[i].title)
                    )
                {
                    if (StrСontains(pages[i].title,
                        includeCategoryWords) && !StrСontains(pages[i].title,
                        excludeCategoryWords)
                        )
                    {
                        FillAllFromCategory(pages[i].title);
                    }

                    doneCats.Add(pages[i].title);

                    if (pages[i].title.Contains(":"))
                    {
                        if (pages[i].GetNamespace() != 14)
                            Console.WriteLine("something found"); ;
                    }

                    if (pages[i].GetNamespace() != 14)
                        Console.WriteLine("is not category");

                }
            }
            RemoveRecurring();
        }
    }

    public partial class Page: IComparable<Page>
    {
        public static bool CacheEnabled = false;

        public static readonly string CacheDirectory =
    AppDomain.CurrentDomain.BaseDirectory + "PageCache\\";

        public int? nOfViews;
        public int rank;

        string CurrentPageCacheDirectory
        {
            get
            {

                string firstLetter = CompatibleFileTitle[0].ToString();

                if (firstLetter == ".")
                    firstLetter = ".dot";

                string dir = CacheDirectory + "\\" + site.name + "\\" + firstLetter + "\\";

                Directory.CreateDirectory(dir);

                return dir;
            }
        }
        /*
        /// <summary>Returns the array of strings, containing interwiki links,
		/// found in page text. But no displayed links are returned,
		/// like [[:de:Stern]] - these are returned by GetSisterWikiLinks(true)
		/// function. Interwiki links are returned without square brackets.</summary>
		/// <returns>Returns the string[] array.</returns>
		public string[] GetInterWikiLinks()
		{
			return GetInterWikiLinks(false);
		}
        */
		
        /*
        /// <summary>Adds interwiki links to the page. It doesn't remove or replace old
        /// interwiki links, this can be done by calling RemoveInterWikiLinks function
        /// or manually, if necessary.</summary>
        /// <param name="iwikiLinks">Interwiki links as an array of strings, with or
        /// without square brackets, for example: "de:Stern" or "[[de:Stern]]".</param>
        public void AddInterWikiLinks(string[] iwikiLinks)
        {
            if (iwikiLinks.Length == 0)
                throw new ArgumentNullException("iwikiLinks");
            List<string> iwList = new List<string>(iwikiLinks);
            AddInterWikiLinks(iwList);
        }

        /// <summary>Adds interwiki links to the page. It doesn't remove or replace old
        /// interwiki links, this can be done by calling RemoveInterWikiLinks function
        /// or manually, if necessary.</summary>
        /// <param name="iwikiLinks">Interwiki links as List of strings, with or
        /// without square brackets, for example: "de:Stern" or "[[de:Stern]]".</param>
        public void AddInterWikiLinks(List<string> iwikiLinks)
        {
            if (iwikiLinks.Count == 0)
                throw new ArgumentNullException("iwikiLinks");
            if (iwikiLinks.Count == 1 && iwikiLinks[0] == null)
                iwikiLinks.Clear();
            for (int i = 0; i < iwikiLinks.Count; i++)
                iwikiLinks[i] = iwikiLinks[i].Trim("[]\f\n\r\t\v ".ToCharArray());
            iwikiLinks.AddRange(GetInterWikiLinks());
            SortInterWikiLinks(ref iwikiLinks);
            RemoveInterWikiLinks();
            text += "\r\n";
            foreach (string str in iwikiLinks)
                text += "\r\n[[" + str + "]]";
        }
        */
        public void SaveToCache()
        {
            SaveToFile(CurrentPageCacheDirectory + CompatibleFileTitle);
        }

        public bool TryToLoadFromCache()
        {
            if (!CacheEnabled) return false;

            string path = CacheDirectory + CompatibleFileTitle;

            if (File.Exists(path) || File.Exists(CurrentPageCacheDirectory + CompatibleFileTitle))
            {
                LoadFromCache();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// loads content of the page from Cache
        /// </summary>
        public void LoadFromCache()
        {
            try
            {
                string oldFilename = CacheDirectory + CompatibleFileTitle;

                if (File.Exists(oldFilename))
                    File.Move(oldFilename, CurrentPageCacheDirectory + CompatibleFileTitle);

                LoadFromFile(CurrentPageCacheDirectory + CompatibleFileTitle);

                if (string.IsNullOrEmpty(text))
                {
                    throw new WikiBotException("nothing in cache for page:" + title);
                }


            }
            catch (IOException ex)
            {
                throw;
            }
        }

        ///<summary>
		/// gets the title of the page, which can be used during saving the file, without errors
		/// </summary>
		public string CompatibleFileTitle
		{
			get
			{
				string myTitle = title;
				myTitle = myTitle.Replace(":", "-");
				myTitle = myTitle.Replace("/", "-");
				myTitle = myTitle.Replace("\"", "'");

				//fileTitle = fileTitle.Replace("\"", "&#x22;");
				myTitle = myTitle.Replace("<", "&#x3c;"); 
				myTitle = myTitle.Replace(">", "&#x3e;");
				myTitle = myTitle.Replace("?", "&#x3f;");
				//fileTitle = fileTitle.Replace(":", "&#x3a;");
				myTitle = myTitle.Replace("\\", "&#x5c;");
				//fileTitle = fileTitle.Replace("/", "&#x2f;");
				myTitle = myTitle.Replace("*", "&#x2a;");
				myTitle = myTitle.Replace("|", "&#x7c;");
				return myTitle;
			}

		}

        internal void RemoveCategories()
        {
            var categs = GetCategories();

            foreach (string category in categs)
            {
                RemoveFromCategory(categoryName: category);
            }
        }

        public int CompareTo(Page other)
        {
            return this.title.CompareTo(other.title);
        }
    }

    public static class StopwatchExtensions
    {
        public static long BenchMark(this Stopwatch sw, Action action, string message)
        {
            sw.Reset();
            sw.Start();
            
                action();
            
            sw.Stop();
            Console.WriteLine(message + " occured in sec:" + sw.Elapsed.TotalSeconds);
            
            return sw.ElapsedMilliseconds;
        }

        public static long BenchMark(Action action, string message)
        {
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();

            action();

            sw.Stop();
            Console.WriteLine(message + " occured in sec:" + sw.Elapsed.TotalSeconds);

            return sw.ElapsedMilliseconds;
        }
    }

}
