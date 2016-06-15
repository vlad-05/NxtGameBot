using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace NxtGameBot
{
	public class WebClient
	{
		private Uri baseUrl;
		private string userAgent;
		private CookieContainer cookies;

		public List<Cookie> Cookies
		{
			get
			{
				List<Cookie> result = new List<Cookie>();
				foreach (Cookie cookie in cookies.GetCookies(baseUrl))
				{
					result.Add(cookie);
				}
				return result;
			}
			set
			{
				cookies = new CookieContainer();
				if (value != null)
				{
					foreach (Cookie cookie in value)
					{
						cookies.Add(cookie);
					}
				}
			}
		}

		public WebClient(string baseUrl, string userAgent, List<Cookie> cookies = null)
		{
			this.baseUrl = new Uri(baseUrl);
			this.userAgent = userAgent;
			if (cookies != null)
			{
				this.cookies = new CookieContainer();
				foreach (Cookie cookie in cookies)
				{
					this.cookies.Add(cookie);
				}
			}
			else
			{
				this.cookies = new CookieContainer();
			}
		}

		private async Task<Stream> GetStream(string url, byte[] postdata)
		{
			if (!url.StartsWith("http://") && !url.StartsWith("https://"))
				url = baseUrl + url.TrimStart('/');

			HttpWebRequest request = HttpWebRequest.CreateHttp(url);
			request.Proxy = null;
			request.UserAgent = userAgent;
			request.CookieContainer = cookies;

			if (postdata != null)
			{
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = postdata.Length;
				using (Stream requestStream = request.GetRequestStream())
					requestStream.Write(postdata, 0, postdata.Length);
			}

			WebResponse response = await request.GetResponseAsync();

			return response.GetResponseStream();
		}

		public async Task<byte[]> GetBytes(string url, byte[] postdata = null)
		{
			using (Stream responseStream = await GetStream(url, postdata))
			using (MemoryStream result = new MemoryStream())
			{
				responseStream.CopyTo(result);
				return result.ToArray();
			}
		}

		public async Task<string> GetString(string url, byte[] postdata = null)
		{
			using (StreamReader responseStream = new StreamReader(await GetStream(url, postdata)))
			{
				return responseStream.ReadToEnd();
			}
		}

		public async Task LoadUrlToStream(string url, Stream stream, byte[] postdata = null)
		{
			using (Stream responseStream = await GetStream(url, postdata))
			{
				responseStream.CopyTo(stream);
			}
		}
	}
}
