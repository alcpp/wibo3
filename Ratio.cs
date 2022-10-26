using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWikiBot;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;
using WikiClientLibrary.Sites;
using WikiClientLibrary.Client;
using WikiFunctions.API;

namespace botSolution {
	public class RatioHolder {
		public string UkWikiName { get; set; }
		public int UkWikiVisits { get; set; }
		public string OtherWikiName { get; set; }
		public int OtherWikiVisits { get; set; }
		public decimal RatioVisits { get 
				{
				if (UkWikiVisits == 0)
					return 0;
				return (decimal) OtherWikiVisits / UkWikiVisits; 
			} 
		}

	}
	public class Ratio {

		public static async void GetRatioList2() {
			ApiEdit editor = new ApiEdit("http://uk.wikipedia.org/w/");
			editor.Login("Alex Blokha", "predator32");

			//editor.Open()
			var prov = new WikiFunctions.Lists.Providers.UserContribsListProvider();
			var list = prov.MakeList();
		}

		public static async void GetRatioList() {

			PageList list = new PageList(MyBot.ukSiteL.Value);
			//list.FillFromWatchList();
			//list.FillFromFile("Watchlist");
			string category = "Релігії та релігійні течії";
			list.FillFromCategory(category);
			list.FilterNamespaces(new int[] { 0,14 });

			Ratio r = new Ratio();
			r.PublishRatioForList(list, category);
			
		}

		public static string GetInterwiki(Page page, string lang) {
			//page.Load();
			var links = page.GetInterLanguageLinks();

			string link = links.FirstOrDefault<string>(l => l.ToLower().Contains(lang + ":"));
			if (!string.IsNullOrWhiteSpace(link)) {
				var result = link.Split(':')[1];
				return result;
			}			
			
			return "";
		}

		public void PublishRatioForList(PageList list, string name) {
			Ratio r = new Ratio();
			SortedDictionary<string, RatioHolder> set = new SortedDictionary<string, RatioHolder>();
			int count=0;
			string result = "";
			foreach (Page p in list) {
				//if(count > 20) break;
				var iwiki = GetInterwiki(p, "ru");
				if(!string.IsNullOrEmpty(iwiki)) {
					count++;
					Console.WriteLine(count);
					var ukViews = Popular.GetViews("uk", p.title, new DateTime(2020,10,1), 30);
					if(Popular.Median(ukViews)<=6)
						continue;
					var ruViews = Popular.GetViews("ru", iwiki, new DateTime(2020, 10, 1), 30);
					var ratioHolder = new RatioHolder() { UkWikiName = p.title, OtherWikiName = iwiki,
						UkWikiVisits = Popular.Median(ukViews), OtherWikiVisits = Popular.Median(ruViews)
					};					
					set.Add(p.title, ratioHolder);
					
				}
			}

			var sorted = set.OrderBy(x => x.Value.RatioVisits);
			foreach(KeyValuePair<string, RatioHolder> pair in sorted) {
				result += string.Format("[[{0}]] - {1:0.00} | {2}\n\r", pair.Value.UkWikiName, pair.Value.RatioVisits, pair.Value.UkWikiVisits);
			}
			result += "Avg:" + string.Format("{0:0.00}", set.Average(x => x.Value.UkWikiVisits));

			Page pResult = new Page(MyBot.ukSiteL.Value, "Користувач:Alex Blokha/" + name);
			pResult.text += result + "\n\r";
			pResult.Save();
		}


	}
}
