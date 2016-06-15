using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace NxtGameBot
{
    public partial class frmMain : Form
    {
		NxtGame nxtGame;

        frmBrowser browser;
        frmSetting setting = new frmSetting();
		BinaryFormatter bf = new BinaryFormatter();

		private FormWindowState SaveFormState;
        string traytitle = "NxtGameBot v." + Assembly.GetExecutingAssembly().GetName().Version.Major + "." + Assembly.GetExecutingAssembly().GetName().Version.Minor + "." + Assembly.GetExecutingAssembly().GetName().Version.Build;

        public frmMain()
        {
			InitializeComponent();

			try
			{
				MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.Settings.Default.Cookies));
				List<Cookie> data = (List<Cookie>)bf.Deserialize(ms);
				
				nxtGame = new NxtGame(data);
			}
			catch(Exception)
			{
				nxtGame = new NxtGame(null);
			}

			nxtGame.Log += new EventHandler<string>((object lSender, string lE) =>
			{
				if (InvokeRequired)
				{
					BeginInvoke(new Action<string>(textBox1.AppendText), lE);
				}
				else
				{
					textBox1.AppendText(lE + Environment.NewLine);
				}
			});

			var version = Assembly.GetExecutingAssembly().GetName().Version;
            label2.Text = string.Format("v." + version.Major + "." + version.Minor + "." + version.Build);
            notifyIcon1.Text = traytitle;
		}

		private async void frmMain_Shown(object sender, EventArgs e)
		{
			UserProfile profile = await nxtGame.GetProfile();

			if (profile == null)
			{
				button2.Enabled = false;
			}
			else
			{
				label1.Text = profile.Name;
				pictureBox1.Image = profile.Avatar;
				button2.Enabled = true;
			
				int i = await nxtGame.CheckInventory();
				
				if (i > 0)
				{
					notifyIcon1.BalloonTipTitle = traytitle + " (" + label1.Text + ")";
					notifyIcon1.BalloonTipText = "Доступно вещей для вывода: " + i;
					notifyIcon1.ShowBalloonTip(1000);
				}

				if (Properties.Settings.Default.AutoStart && sender != timer1)
				{
					button2.PerformClick();
				}
			}
		}

		private void CookiesCallback(List<Cookie> cookies)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<List<Cookie>>(CookiesCallback), cookies);
				return;
			}

			browser.Close();

			MemoryStream ms = new MemoryStream();
			bf.Serialize(ms, cookies);
			Properties.Settings.Default.Cookies = Convert.ToBase64String(ms.ToArray());
			Properties.Settings.Default.Save();

			nxtGame.Cookies = cookies;

			frmMain_Shown(null, null);
		}

        private void button1_Click(object sender, EventArgs e)
        {
			browser = new frmBrowser("https://steamcommunity.com/openid/login?openid.ns=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0&openid.mode=checkid_setup&openid.return_to=http%3A%2F%2Fwww.nxtgame.com%2Fauth&openid.realm=http%3A%2F%2Fwww.nxtgame.com&openid.ns.sreg=http%3A%2F%2Fopenid.net%2Fextensions%2Fsreg%2F1.1&openid.claimed_id=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select&openid.identity=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select");

			browser.PageLoaded += new EventHandler<string>((object lSender, string lE) =>
			{
				if (lE.StartsWith("http://www.nxtgame.com"))
				{
					browser.GetCookies(CookiesCallback);
				}
			});

			browser.ShowDialog();
		}

		private async void button2_Click(object sender, EventArgs e)
		{
			if (pictureBox1.Image != null)
			{
				button2.Enabled = false;

				timer1.Stop();
				timer1.Interval = 60000 * Properties.Settings.Default.AutoStartInterval;
				timer1.Start();

				await nxtGame.GetMatches();
				await nxtGame.PredictMatches();

				button2.Enabled = true;
			}
		}

		private void button4_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Process.GetCurrentProcess().Kill();
        }

        private void button3_Click(object sender, EventArgs e)
        {
			//string remoteUri = "https://ci.appveyor.com/api/buildjobs/4akcm55elc60cg0t/artifacts/";
			//string fileName = "NGB.zip", myStringWebResource = null;
			//WebClient myWebClient = new WebClient();
			//myStringWebResource = remoteUri + fileName;
			//textBox1.AppendText( "Загрузка новой версии... " + Environment.NewLine );
			//myWebClient.DownloadFile(myStringWebResource, fileName);
			//textBox1.AppendText( "Загрузка завершена." + Environment.NewLine );
		}

		private void button5_Click(object sender, EventArgs e)
        {
            setting.ShowDialog();
        }

        private void timer1_Tick(object sender, EventArgs e)
		{
			timer1.Stop();
			frmMain_Shown(timer1, null);
			button2.PerformClick();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (WindowState == FormWindowState.Normal || WindowState == FormWindowState.Maximized)//если оно развернуто
                {
                    SaveFormState = WindowState;
                    WindowState = FormWindowState.Minimized;
                    ShowInTaskbar = false;
                }
                else
                {
                    ShowInTaskbar = true;
                    Show();
                    WindowState = SaveFormState;
                }
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon1.Visible = false;
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
            else
            {
                ShowInTaskbar = true;
            }
        }
	}
}