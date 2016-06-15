using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using System.IO;
using System.Net;
using System.Drawing;

namespace NxtGameBot
{
	internal class UserProfile
	{
		public string Name;
		public Image Avatar;
	}

	internal class Match
	{
		public int Id = -1, A, B;
		public string TeamA, TeamB;
		public bool IsLive = false;
	}

	internal class NxtGame
	{
		private WebClient webClient;
		private List<Match> matches;

		public event EventHandler<string> Log;

		public List<Cookie> Cookies
		{
			get
			{
				return webClient.Cookies;
			}
			set
			{
				webClient.Cookies = value;
			}
		}

		public NxtGame(List<Cookie> cookies)
		{
			webClient = new WebClient("http://www.nxtgame.com", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36", cookies);
		}

		public async Task<UserProfile> GetProfile()
		{
			try
			{
				UserProfile result = new UserProfile();
				WriteLog("N [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + "Загрузка данных профиля...");
				string html = await webClient.GetString("/profile");
				HtmlDocument htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(html);

				if (htmlDocument.DocumentNode.SelectSingleNode("//img[@alt='Steam Login']") != null)
				{
					WriteLog("E [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + "Требуется авторизация. Старт невозможен.");
					return null;
				}

				HtmlNode htmlNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='profile-name']/a");

				if (htmlNode != null)
				{
					result.Name = htmlNode.InnerText;
				}

				htmlNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='avatar']/img");

				if (htmlNode != null)
				{
					try
					{
						MemoryStream ms = new MemoryStream();
						await webClient.LoadUrlToStream(htmlNode.Attributes["src"].Value, ms);
						result.Avatar = Image.FromStream(ms);
					}
					catch (Exception e)
					{
						WriteLog("E [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + e.Message);
						result.Avatar = null;
					}
				}

				WriteLog("N [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + "Данные загружены.");

				return result;
			}
			catch (Exception e)
			{
				WriteLog("E [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + e.Message);
				return null;
			}
		}

		public async Task<int> CheckInventory()
		{
			try
			{
				int result = 0;
				WriteLog("N [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + "Получение списка вещей...");
				string html = await webClient.GetString("/my-inventory");
				HtmlDocument htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(html);
				HtmlNodeCollection htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes("//div[@class='trade-items ']");
				result = (htmlNodeCollection == null)? 0 : htmlNodeCollection.Count;
				WriteLog("N [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + "Доступно вещей для вывода: " + result.ToString());
				return result;
			}
			catch (Exception e)
			{
				WriteLog("E [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + e.Message);
				return -1;
			}
		}

		public async Task<int> GetMatches()
		{
			try
			{
				matches = new List<Match>();
				WriteLog("N [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + "Получение списка матчей...");
				string html = await webClient.GetString("/?sports=0");
				HtmlDocument htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(html);
				HtmlNodeCollection htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes("//div[@class='col-xs-12 item match-thumbnail']");
				
				foreach (HtmlNode htmlNode in htmlNodeCollection)
				{
					Match match = new Match();

					if (int.TryParse(htmlNode.SelectSingleNode("div/div/a").Attributes["id"].Value, out match.Id))
					{
						if (htmlNode.SelectSingleNode("div/div/a/div[1]/div/div[2]/p[1]/span[1]") != null)
						{
							match.IsLive = true;
						}

						match.TeamA = htmlNode.SelectSingleNode("div/div/a/div[2]/div/div[1]/div[2]/p[1]").InnerText;
						match.TeamB = htmlNode.SelectSingleNode("div/div/a/div[2]/div/div[2]/div[2]/p[1]").InnerText;
						
						int.TryParse(htmlNode.SelectSingleNode("div/div/a/div[2]/div/div[1]/div[2]/p[2]").InnerText.TrimEnd(new char[] { '%', ' ' }), out match.A);
						int.TryParse(htmlNode.SelectSingleNode("div/div/a/div[2]/div/div[2]/div[2]/p[2]").InnerText.TrimEnd(new char[] { '%', ' ' }), out match.B);

						matches.Add(match);
					}
				}
				
				WriteLog("N [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + "Матчи успешно получены. Всего матчей: " + matches.Count);
				return matches.Count;
			}
			catch (Exception e)
			{
				WriteLog("E [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + e.Message);
				return -1;
			}
		}

		public async Task PredictMatches()
		{
			foreach (Match match in matches)
			{
				if (match.IsLive)
				{
					string message = "Матч " + match.Id.ToString();
					
					WriteLog("N [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + message + " -> Матч уже начался.");
				}
				else
				{
					if (!(await Predict(match)))
					{
						return;
					}
				}
			}
			WriteLog("N [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] Предсказания сделаны.");
		}

		private void WriteLog(string message)
		{
			if (Log != null)
			{
				Log(this, message);
			}
		}

		private async Task<bool> Predict(Match match)
		{
			try
			{
				string message = "Матч " + match.Id.ToString() + " -> ";

				string predictLink = "http://www.nxtgame.com/prediction/action?action=add&matchid=" + match.Id + "&value=";

				if (match.A > match.B)
				{
					message += match.TeamA;
					predictLink += "1";
				}
				else if (match.A < match.B)
				{
					message += match.TeamB;
					predictLink += "3";
				}
				else
				{
					message += "Ничья";
					predictLink += "2";
				}

				string json = await webClient.GetString(predictLink);

				JObject o = JObject.Parse(json);
				string response = (string)o["message"];
				int status = (int)o["status"];
				if (status == 2 && response.StartsWith("<span>Retry:"))
				{
					message += " -> Неудалось сделать предсказание, повторите через минуту.";
				}
				else if (response == "This match has already started.")
				{
					message += " -> Матч уже начался.";
				}
				else if (response == "1")
				{
					message += " -> Прогноз сделан.";
				}
				else if (response == "Prediction is offline.")
				{
					message += " -> Прогнозы отключены.";
					WriteLog("N [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + message);
					return false;
				}
				else if (response == "This match is already done.")
				{
					message += " -> Матч уже прошел.";
				}
				else if (response == "This match has been cancelled.")
				{
					message += " -> Матч отменен.";
				}

				WriteLog("N [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + message);
				return true;
			}
			catch (Exception e)
			{
				WriteLog("E [" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + e.Message);
				return false;
			}
		}
	}
}
