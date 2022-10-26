using System;
using System.Collections.Generic;
using System.Text;
using DotNetWikiBot;

using System.IO;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace botSolution
{

	class DatesSearchResult
	{
		

		public Page page;
		public string found;

		public string[] imagesOnPage;

		public bool areImagesFound;



		public DatesSearchResult(Page p, string found)
		{
			page = p;
			this.found = found;
            imagesOnPage = GetClearedImages(p);

			areImagesFound = AreImagesInFound();
		}

        public string[] GetClearedImages(Page p)
        {
            List<string> images = new List<string>();
            foreach (string img in p.GetImages()) // bool withNameSpacePrefix
            {
                if(!IsNotEssentialImage(img) && !IsNonFreeImage(img.ToLower()))
                    images.Add(img);
            }

            return images.ToArray();
        }

		bool AreImagesInFound()
		{
            if (!string.IsNullOrEmpty(found))
            {
                Page p = new Page(DatesProcessor.site);
                p.text = found;

                var images = p.GetImages(); //bool withNameSpacePrefix

                return (images.Count > 0) ? true : false;
            }
            else
                return false;

			//return false;
		}

		public virtual string ToString(bool withImages)
		{
            if (found.Trim().Length < 10)
                return "";

			if (imagesOnPage.Length > 0)
			{
				found += "\n<br>Зобр:" + imagesOnPage.Length + "шт.";
			}

            /*
			if (withImages && imagesOnPage.Length>0 && !areImagesFound)
			{
                string image = GetImageFromFound();

				return found + "\n" + image;

			}
			else
             */ 
				return found;
		}

        public string GetImageFromFound()
        {
            if (imagesOnPage.Length > 0 && !areImagesFound)
            {
                string image = imagesOnPage[imagesOnPage.Length - 1];
                if (!IsNotEssentialImage(image) && !IsNonFreeImage(image.ToLower()))
                {
                //|thumb|150px]] gives wrong result
                //[[Зображення:Depeche_Mode_early_years.JPG|200px|left|thumb|Depeche Mode у 1984&nbsp;р.]]                
                image = "[[" + image + "|thumb |150px|right]]";
                }
                return image;
            }
            return "";
        }
        private static bool IsNonFreeImage(string image)
        {
            image = image.Replace("_", " ");
            bool contains = !DatesProcessor.nonFreeImages.pages.All(p => p.title.ToLower() != image);
            var poster = DatesProcessor.nonFreeImages.pages.FindAll(p => p.title.Contains("pianoboy"));
            if (contains)
            {
                var list = DatesProcessor.nonFreeImages.pages.FindAll(p => p.title.ToLower() == image);
                return contains;
            }
            return contains;
        }


        private static bool IsNotEssentialImage(string image)
        {
            if (image.Contains("Flag_of") || image.Contains("Christian_cross.") 
                || image.Contains("Kerze.png"))
                return true;
                
            return false;
        }
	}

	class DatesProcessor
	{

		protected string linkToPage;

		public static Site site;

		private readonly bool isTest = false;

		/// <summary>
		/// if true, then daysare analyzed, false - years;
		/// </summary>
		public bool _AreDaysAnalyzed = true;
        PageList analyzedlist = null;

        public static PageList nonFreeImages = null;

        int pagesCounter=0;

        string analyzedTopic = null; // null for all

		public string ResultDir
		{
			get { return "DatesResult\\" + linkToPage + "\\"; }

		}

		public DatesProcessor()
		{

		}

		public DatesProcessor(string dateStr)
		{
			Init(dateStr);
			//botSolution.com.amazonaws.ahp.
		}

		public string GetCalculatedDateString(DateTime srcDate, int step)
		{
			//DateTime date = AddDate(srcDate, step, _areDaysAnalyzed);

			CultureInfo culture = CultureInfo.GetCultureInfo("uk-UA");
			DateTimeFormatInfo dtformat = culture.DateTimeFormat;

			string datePattern ;

			if(_AreDaysAnalyzed)
				datePattern = "d MMMM";
			else
				datePattern = "yyyy";

			string resultDate = AddDate(srcDate, step, _AreDaysAnalyzed).ToString(datePattern, dtformat);

			return resultDate;
		}

		protected DateTime AddDate(DateTime srcDate, int step)
		{
			return AddDate(srcDate, step, _AreDaysAnalyzed);
		}

		protected DateTime AddDate(DateTime srcDate, int step, bool areDaysAnalyzed)
		{
			if (areDaysAnalyzed)
			 	return srcDate.AddDays(step);
			else
				return srcDate.AddYears(step);
		}

		

		private void Init(string dateStr)
		{
            if (nonFreeImages == null)
            {
                nonFreeImages = new PageList(MyBot.ukSiteL.Value);
                if(File.Exists("NonFreeFiles.txt"))
                    nonFreeImages.FillFromFile("NonFreeFiles.txt");
                else
                {
                    nonFreeImages.FillAllFromCategoryTree("Невільні файли");
                    nonFreeImages.SaveTitlesToFile("NonFreeFiles.txt");
                }
                nonFreeImages.Sort();
            }
            
			linkToPage = dateStr;
			Directory.CreateDirectory(ResultDir);
		}
        

		public  void ProcessDatesRange(PageList analyzedlist)
		{
            
            this.analyzedlist = analyzedlist;

			site =
				//new Site();// loads from file
				new Site("https://uk.wikipedia.org", "Alex_Blokha", "predator32");

            _AreDaysAnalyzed = true;

            analyzedTopic = "Україна";

			if (isTest)
			{
				ProcessDate(new DateTime(1800, 06, 20));
				return;
			}

            if (_AreDaysAnalyzed)
            {
                DateTime day = new DateTime(1577, 06, 23); // till 18.08 are old pages
                CultureInfo culture = CultureInfo.GetCultureInfo("uk-UA");
                //for (int i = 0; i < 5; i++)
                for (int i = 0; i < 345; i++)
                {
                    ProcessDate(day);
                    day = AddDate(day, 1);
                }
            }
            else //years
            {
                DateTime day = new DateTime(1577, 09, 29); // till 18.08 are old pages
                CultureInfo culture = CultureInfo.GetCultureInfo("uk-UA");
                for (int i = 0; i < 1000; i++)
                {
                    ProcessDate(day);
                    day = AddDate(day, 1);
                }
            }
		}

		DateTime processedDate;

		private void  ProcessDate(DateTime day)
		{
			processedDate = day;			

			string link = GetCalculatedDateString(day, 0);
			//string prev = AddDate(day, -1).ToString(datePattern, dtformat);
			//string next = AddDate(day, 1).ToString(datePattern, dtformat);

			string prev = GetCalculatedDateString(day,-1);
			string next = GetCalculatedDateString(day, 1);
			
			Process(prev , link, next);
			
		}

		public string GetSingleMatchSentence(string str, Match ma)
		{
			int startIndex = ma.Groups[0].Index;
			startIndex = str.LastIndexOf(".", startIndex);
			return "";

		}

		protected bool IsYearSearch()
		{
			//if (linkToPage.Length != 4)
			//	return false;


			return IsYearSearch(linkToPage);
		}

		protected bool IsYearSearch(string pageTitle)
		{
			//if (linkToPage.Length != 4)
			//	return false;

			int ret;
			return Int32.TryParse(pageTitle, out ret);
		}

		public bool IsYearInYear(string pageTitle)
		{
			if (IsYearSearch())
			{
				//if (IsYearSearch(pageTitle))
				//	return true;
			}
			return false;
		}

		public bool IsBeginOfYearPage(string str, string pageTitle, Match ma)
		{
			if (IsYearSearch())
			{
				if (IsYearSearch(pageTitle))
				{
					//Regex wikiNoInterwiki = new Regex(@"\[\[(([^:].+)?)(\|.+?)?]]");
                    Regex wikiLinkRE = new Regex(@"\[\[ *:*(.+?)(]]|\|)");

					MatchCollection wikis = wikiLinkRE.Matches(str);
					//MatchCollection interwikis = site.sisterWikiLinkRE.Matches(str);

					if (wikis.Count > 4)
						return true;
					else
						return false;
				}
			}
			return false;

			/*if (ma.Value.Contains(linkToPage)) // can be commented out for speed
			{
				if (IsYearSearch())
				{

					int ind = str.IndexOf("події", 0, ma.Index, StringComparison.OrdinalIgnoreCase);

					return ind == -1 ? true : false;
				}
			}
			return false;
			 */


		}

		public DatesSearchResult GetMatches(Page page)
		{
			string str = page.text;
			string newStr = "";
			string paragraph = "";


			//will count 20 words from the found
			Regex rx20words = new Regex(@"((\w+)\W+){0,20}(" + linkToPage + @")(\W+(\w+)){0,30}",RegexOptions.Compiled);

			//if (IsYearInYear(page.title))
			//	return "";

			if( string.IsNullOrEmpty(str) )
				return null;

			if (true)
			{
				//Regex rx = new Regex(@"([^\.\?\!\r\n]*)[\.\?\!\r\n]");
				Regex rxParagraph = new Regex(@"([^\?\!\r\n]*)[\?\!\r\n]", RegexOptions.Compiled); // catches the right sentence.
				//Dots are not considered

				MatchCollection mcParagraphs = rxParagraph.Matches(str);

				//Regex podiiRX = new Regex(@"==\s{0,5}\w*\s{0,5}=="); //found  podii
				Regex podiiRX = new Regex(@"=={0,3}\s{0,5}\w*\s{0,5}=={0,3}"); //found  podii

				for (int i = 0; i < mcParagraphs.Count; i++)
				{
					paragraph = mcParagraphs[i].Value;



					if (!_AreDaysAnalyzed && IsBeginOfYearPage(paragraph, page.title, mcParagraphs[i]))
						return null;

                    Page removeCateg = new Page(MyBot.ukSiteL.Value);
                    removeCateg.text = paragraph;
                    removeCateg.RemoveCategories();

                    paragraph = removeCateg.text;
                    
                    
					if (paragraph.Contains(linkToPage)// && !paragraph.Contains("Категорія:")

						)
					{

						//paragraph = rx20words.Match(paragraph).Value;

						MatchCollection podiiMC = podiiRX.Matches(str.Substring(0, mcParagraphs[i].Index));
						string podii = "-";

                        if (podiiMC.Count > 0)
                        {
                            string pidzaholovok = podiiMC[podiiMC.Count - 1].Groups[0].Value;
                            if (!string.IsNullOrEmpty(pidzaholovok.Trim(new char[] { '=', ' ' })))
                            {

                                podii = "Підзаголовок:" + pidzaholovok
                                    + "<br>\n";
                            }
                        }



						if (paragraph.Trim() == "")
						{
							throw new WikiBotException("The sentence with dates is empty ");
						}

                        if (paragraph.Contains("<ref>") && !paragraph.Contains("</ref>"))
                        {
                            paragraph += "</ref>";
                        }

						newStr += podii + "Знайдено: " + "+ " +paragraph +  "<br>\n";
					}
				}
			}
			else
			{
				//linkToPage
				Regex rx = new Regex(linkToPage);

				MatchCollection mc = rx.Matches(str);

				//foreach (Match ma in rx.Matches(str))
				for (int i = 0; i < mc.Count; i++)
				{
					paragraph = GetSingleMatchSentence(str, mc[i]);
					newStr += paragraph + "\n+";
				}

			}
			DatesSearchResult result = new DatesSearchResult(page,newStr);

			return result;
		}

        public PageList  RemoveNonCommon(PageList plToChange, PageList pl2)
        {
            var intersect = plToChange.pages.Intersect(pl2.pages, new PageComparer());

            var intersect2 = pl2.pages.Intersect(plToChange.pages, new PageComparer() );

            PageList pl = new PageList(MyBot.ukSiteL.Value);
            pl.pages = new List<Page>(intersect.ToList());

            return pl;
        }

        public void Process(string prevPage, string linkToPage, string nextPage)
		{
			Init(linkToPage);
			Console.WriteLine("GetDatesStrings22===");


            string proProect = "Користувач:Alex Blokha/Події, згадані у Вікіпедії/";
			string rootDirectory;
            if (string.IsNullOrEmpty(analyzedTopic))
            {
                if (_AreDaysAnalyzed)
                    rootDirectory = @"Користувач:Alex Blokha/Події в Вікіпедії/";
                else
                    rootDirectory = @"Користувач:Alex Blokha/Події згадані в Вікіпедії/";
            }
            else
            {
                if (_AreDaysAnalyzed)
                    rootDirectory
                        = @"Користувач:Alex Blokha/Дні згадані в Вікіпедії/";
                else
                    rootDirectory 
                        = @"Користувач:Alex Blokha/Роки згадані в Вікіпедії/";

                rootDirectory += analyzedTopic + "/";
            }

            Page.CacheEnabled = false;

			Page save = new Page(site, rootDirectory  + linkToPage);
			save.Load();

            

            if (save.Exists()
            //    && DateTime.Now < save.timestamp.AddDays(200) )
                && save.timestamp > new DateTime(2013, 08, 21, 8,0,0))
                return;

            if (pagesCounter >= 365)
                return;
            
            pagesCounter++;
            

            /*
			if (!isTest && save.Exists() )
			{
				save.Save();
				//Console.Beep();
				Console.WriteLine("this page already exists");
				return;
			}
            */

			PageList plAllLinks = new PageList(site);

            plAllLinks.FillFromLinksToPage(linkToPage);

            PageList pl = RemoveNonCommon(plAllLinks, analyzedlist);

			Console.WriteLine("Pages loaded:" + pl.pages.Count);
			MyBot.RemoveNonInterestingPages(pl);
			if (pl.Count() < 1)
				return;
			else
				pl.Load();

			

			string menuBar = string.Format
("[[{5} | 100 назад]] - [[{4} | 50 назад]] - [[{3} | 10 назад]] -  [[{0} | Попередня]] - [[{1} | Про проект]] - [[{2} | Наступна]] - [[{6} | 10 вперед]] - [[{7} | 50 вперед]] - [[{8} | 100 вперед]]   \n----- \n",
                rootDirectory + prevPage, proProect, rootDirectory + nextPage,
				//назад
				rootDirectory + GetCalculatedDateString(processedDate,-10), 
				rootDirectory + GetCalculatedDateString(processedDate,-50), 
				rootDirectory + GetCalculatedDateString(processedDate,-100), 
				//vpered
				rootDirectory + GetCalculatedDateString(processedDate,+10),
				rootDirectory + GetCalculatedDateString(processedDate,+50),
				rootDirectory + GetCalculatedDateString(processedDate,+100)

				);

			string wholeResult=""  , matches;
			

			int imageCount = 0;

			int previmageIndex = 0;

			for (int i = 0; i < pl.pages.Count; i++)
			{
				Page p = pl.pages[i];
				DatesSearchResult searchResult = GetMatches(p);
                

				if (searchResult == null)
					continue;


				if (imageCount < 5 && 
                    searchResult.imagesOnPage.Length > 0
                    && previmageIndex < (i + 3) // what the heck?
                    ) 
				{
					matches = searchResult.ToString(true);
					imageCount++;

					previmageIndex = i;
				}
				else
				{
					matches = searchResult.ToString(false); 
				}

                string image = searchResult.GetImageFromFound();

				if (searchResult.areImagesFound)
				{
					previmageIndex = i;
				}

				File.WriteAllText(ResultDir + p.CompatibleFileTitle, matches, Encoding.Default);

				//\n\n  or <br>- perenos stroki v vikipedii
                if (matches.Trim().Length > 10)
                    wholeResult += string.Format("Назва:[[{0}]] \n{1}\n{2}\n----\n", p.title, image, matches);
                else
                    Console.WriteLine("Empty match for page"+p.title ); 
			}

			if (wholeResult == "")
			{
				Console.WriteLine("Empty ResultPage");
				Console.Beep(11000, 1000);
				Console.Beep(11000, 1000);
				return;
			}

			wholeResult = menuBar + wholeResult + menuBar;

			File.WriteAllText(ResultDir + "whole.txt", wholeResult, Encoding.Default);

            
			save.text = wholeResult;

            
			

            save.text += "\n==Примітки==";
            save.text += "\n{{Примітки}}";

			if (_AreDaysAnalyzed)
			{
                //string category = "Дні загадні в Вікіпедії/Україна";
                //category += "про Україну";

				save.text += string.Format("\n [[Категорія: Дні згадані в Вікіпедії/{0}]]", analyzedTopic);
                
			}
			else
            {

                save.text += string.Format("\n [[Категорія: Роки згадані в Вікіпедії/{0}]]", analyzedTopic);
            }

			//save.AddToCategory("Дні в Вікіпедії"); // does not work in offline mode

			if (!isTest)
			{
                try
                {
                    save.Save(pl.pages.Count + " результат(ів)", false);
                    save.Watch();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

				
			}
			else
				save.SaveToCache();
		}


	}

    public class PageComparer : IEqualityComparer<Page>
    {
        public bool Equals(Page p1, Page p2)
        {
            return (p1.title == p2.title);
        }

        public int GetHashCode(Page p)
        {
            return p.title.ToLower().GetHashCode();
        }
    }
}
