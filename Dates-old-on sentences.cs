using System;
using System.Collections.Generic;
using System.Text;
using DotNetWikiBot;
using System.Text.RegularExpressions;
using System.IO;

namespace botSolution
{
	class GetDates
	{
		public string ResultDir
		{
			get { return "DatesResult\\" + linkToPage + "\\"; }

		}
		protected string linkToPage;

		Site site;
		
		public GetDates(string dateStr)
		{
			linkToPage = dateStr;
			Directory.CreateDirectory(ResultDir);

			//botSolution.com.amazonaws.ahp.
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

					MatchCollection wikis = site.wikiLinkRE.Matches(str);
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

		public string GetMatches(Page page)
		{
			string str = page.text;
			string newStr = "";
			string sentense = "";


			//will count 20 words from the found 
			Regex rx10words = new Regex( @"((\w+)\W+){0,10}("+linkToPage +@"\s*?(]])*?)(\W+(\w+)){0,10}" ); 

			//if (IsYearInYear(page.title))
			//	return "";

			if (true)
			{
				//Regex rx = new Regex(@"([^\.\?\!\r\n]*)[\.\?\!\r\n]");
				Regex rx = new Regex(@"([^\?\!\r\n]*)[\?\!\r\n]"); // catches the right sentence. 
				//Dots are not considered

				MatchCollection mcSentenses = rx.Matches(str);				

				for (int i = 0; i < mcSentenses.Count; i++)
				{
					sentense = "+ " + mcSentenses[i].Groups[0].Value;

					if (IsBeginOfYearPage(sentense, page.title, mcSentenses[i]))
						continue;

					if (sentense.Contains(linkToPage) && !sentense.Contains("Категорія:")

						)
					{
						Regex podiiRX = new Regex(@"==\s{0,5}\w*\s{0,5}=="); //found  podii
						MatchCollection podiiMC = podiiRX.Matches(str.Substring(0, mcSentenses[i].Index));
						string podii = "-";
						if (podiiMC.Count > 0)
						{
							podii = "Підзаголовок:" + podiiMC[podiiMC.Count - 1].Groups[0].Value + "<br />\n";
						}

						string begin = "";
						string end = "";
						
						newStr += podii + "Знайдено: " + begin + sentense + end + "<br />\n";
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
					sentense = GetSingleMatchSentence(str, mc[i]);
					newStr += sentense + "\n+";
				}

			}
			return newStr;
		}

		public void Process()
		{

			site =
				//new Site("http://uk.wikipedia.org", "Alex_Blokha", "predator32");
				new Site();// loads from file
			Console.WriteLine("GetDatesStrings22===");



			PageList pl = new PageList(site);

			pl.FillFromLinksToPage(linkToPage);


			/*
			PageList plUkr = new PageList(site);
			//pl.FillAllFromCategoryTree("Україна");
			plUkr.FillAllFromCategory("Україна");

			//pl = MyBot.GetCommonPages(pl, plUkr);
			*/

			Console.WriteLine("Pages loaded:" + pl.pages.Count);
			MyBot.RemoveNonInterestingPages(pl);
			pl.LoadEx();

			string wholeResult = "", matches;
			int i = 0;
			foreach (Page p in pl.pages)
			{
				matches = GetMatches(p);

				File.WriteAllText(ResultDir + p.CompatibleFileTitle, matches, Encoding.Default);

				//\n\n - perenos stroki v vikipedii
				wholeResult += string.Format("Назва сторінки:[[{0}]] <br />\n{1}\n-------\n", p.title, matches);
			}

			wholeResult += "\n [[Категорія: Дні в Вікіпедії]]";

			File.WriteAllText(ResultDir + "whole.txt", wholeResult, Encoding.Default);

			Page save = new Page(site, @"Користувач:Alex Blokha/Події в Вікіпедії/" + linkToPage);
			save.Save(wholeResult);
		}
	}
}
