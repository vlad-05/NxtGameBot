using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;

namespace NxtGameBot
{
	public partial class frmBrowser : Form
    {
        private ChromiumWebBrowser browser;

		public event EventHandler<string> PageLoaded;

        public frmBrowser(string Url = "about:blank")
        {
            InitializeComponent();
			CefSettings cefSettings = new CefSettings
			{
				UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36",
				CachePath = Environment.CurrentDirectory + "/cache",
			};

			if (!Cef.IsInitialized)
			{
				if (!Cef.Initialize(cefSettings))
				{
					throw new Exception("Ошибка инициализации браузера.");
				}
			}

			browser = new ChromiumWebBrowser(Url);
			browser.Dock = DockStyle.Fill;

			browser.LoadingStateChanged += new EventHandler<LoadingStateChangedEventArgs>((object lSender, LoadingStateChangedEventArgs lE) =>
			{
				if (!lE.IsLoading && PageLoaded != null)
				{
					PageLoaded(this, browser.Address);
				}
			});

			this.Controls.Add(browser);
		}

		public void GetCookies(Action<List<System.Net.Cookie>> callback)
		{
			try
			{
				CookieVisitor visitor = new CookieVisitor(callback);
				Cef.GetGlobalCookieManager().VisitUrlCookies(browser.Address, true, visitor);
			}
			catch
			{
				callback(null);
			}
		}

        private void Browser_FormClosing(object sender, FormClosingEventArgs e)
        {
			if (!Cef.IsInitialized)
			{
				Cef.Shutdown();
			}
		}
	}

	class CookieVisitor : ICookieVisitor
	{
		public readonly List<System.Net.Cookie> cookies = new List<System.Net.Cookie>();
		readonly Action<List<System.Net.Cookie>> useAllCookies;

		public CookieVisitor(Action<List<System.Net.Cookie>> callback)
		{
			useAllCookies = callback;
		}

		public bool Visit(CefSharp.Cookie cookie, int count, int total, ref bool deleteCookie)
		{
			cookies.Add(new System.Net.Cookie
			{
				Name = cookie.Name,
				Domain = cookie.Domain,
				Value = cookie.Value,
				Path = cookie.Path,
				Expires = (cookie.Expires.HasValue) ? cookie.Expires.Value : DateTime.MaxValue
			});

			if (count == total - 1)
				useAllCookies(cookies);

			return true;
		}
	}
}