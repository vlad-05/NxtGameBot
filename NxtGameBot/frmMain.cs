using CefSharp;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace NxtGameBot
{
    public partial class frmMain : Form
    {
        frmBrowser browser;

        void GetProfile()
        {
            try
            {
                browser.browser.Load("http://www.nxtgame.com/profile");
                textBox1.AppendText( "Загрузка данных профиля..." + Environment.NewLine );
                EventHandler<LoadingStateChangedEventArgs> avatar = null;
                avatar = new EventHandler<LoadingStateChangedEventArgs>(async (x, y) =>
                {
                    if (!(y as LoadingStateChangedEventArgs).IsLoading)
                        if (browser.browser.Address.Contains("nxtgame.com/profile"))
                        {
                            browser.browser.LoadingStateChanged -= avatar;
                            if (!started)
                            {
                                string avatarurl = "";
                                string nickname = "";
                                HtmlDocument HD = new HtmlDocument();
                                var web = new HtmlWeb
                                {
                                    AutoDetectEncoding = false,
                                    OverrideEncoding = Encoding.UTF8,
                                };
                                HD = new HtmlDocument();
                                HD.LoadHtml(await browser.browser.GetSourceAsync());
                                HtmlNodeCollection bodyNode = HD.DocumentNode.SelectNodes("//div[@class='avatar']/img");
                                foreach (var hn in bodyNode)
                                {
                                    avatarurl = hn.Attributes["src"].Value;
                                }
                                HtmlNodeCollection bodyNodeA = HD.DocumentNode.SelectNodes("//div[@class='profile-name']/a");
                                foreach (var hnA in bodyNodeA)
                                {
                                    nickname = hnA.InnerText.Trim();
                                }
                                pictureBox1.Load(avatarurl);
                                Invoke(new XDD((xy) =>
                                {
                                    label1.Text = xy;
                                }), new string[] { nickname });
                                Invoke(new XDD(textBox1.AppendText), new string[] { "Данные профиля успешно получены." + Environment.NewLine });
                                Invoke(new XD(() =>
                                {
                                    button2.Enabled = true;
                                }));
                            }
                        }
                        else if (browser.browser.Address == "about:blank")
                            browser.browser.Load("http://www.nxtgame.com/profile");
                        else
                        {
                            browser.browser.LoadingStateChanged -= avatar;
                            Invoke(new XDD(textBox1.AppendText), new string[] { "Старт невозможен. Вы не авторизованы." + Environment.NewLine });
                        }
                });

                browser.browser.LoadingStateChanged += avatar;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public frmMain()
        {
            browser = new frmBrowser();
            browser.Show();
            browser.Hide();
            InitializeComponent();
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            label2.Text = string.Format("v.: {0}.{1} (build {2})", version.Major, version.Minor, version.Build);
            GetProfile();
        }

        public delegate void XD();
        public delegate void XDD(string text);

        private void button1_Click(object sender, EventArgs e)
        {
            EventHandler<LoadingStateChangedEventArgs> login = null;
            login = new EventHandler<LoadingStateChangedEventArgs>((x, y) =>
            {
                if (!(y as LoadingStateChangedEventArgs).IsLoading)
                {
                    if (browser.browser.Address.Contains("http://www.nxtgame.com/"))
                    {
                        browser.browser.LoadingStateChanged -= login;
                        Invoke(new XD(GetProfile));
                        Invoke(new XD(browser.Hide));
                    }
                }
            });
            browser.browser.LoadingStateChanged += login;
            browser.Show();
            browser.browser.Load("http://www.nxtgame.com/auth");
        }

        public async void next(int i)
        {
            double mA = 0;
            double mB = 0;
            string outputTextA;
            string outputTextB;
            string teamwin = "";
            string value = "";
            HtmlDocument HD = new HtmlDocument();
            var web = new HtmlWeb
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8,
            };
            HD = web.Load("http://www.nxtgame.com/match/details/" + matchid[i]);
            HtmlNodeCollection bodyNodeA = HD.DocumentNode.SelectNodes("//div[@class='col-xs-6 col-md-3 text-center odds-panel-teamA']/span");
            foreach (var hnA in bodyNodeA)
            {
                Invoke(new XDD(textBox1.AppendText), new string[] { "Матч " + matchid[i] + ": " });
                outputTextA = hnA.InnerText.Trim();
                mA = Convert.ToDouble(outputTextA.Replace(".", ","));
                Invoke(new XDD(textBox1.AppendText), new string[] { mA.ToString() });
            }
            HtmlNodeCollection bodyNodeB = HD.DocumentNode.SelectNodes("//div[@class='col-xs-6 col-md-3 text-center odds-panel-teamB']/span");
            foreach (var hnB in bodyNodeB)
            {
                Invoke(new XDD(textBox1.AppendText), new string[] { " vs " });
                outputTextB = hnB.InnerText.Trim();
                mB = Convert.ToDouble(outputTextB.Replace(".", ","));
                Invoke(new XDD(textBox1.AppendText), new string[] { mB.ToString() });
            }
            if (mA > mB)
            {
                teamwin = " -> Команда Б ->";
                value = "3";
            }
            if (mA < mB)
            {
                teamwin = " -> Команда А ->";
                value = "1";
            }
            if (mA == mB)
            {
                teamwin = " -> Ничья ->";
                value = "1";
            }
            Invoke(new XDD(textBox1.AppendText), new string[] { teamwin });
            string urlmatch = "http://www.nxtgame.com/prediction/action?action=add&matchid=" + matchid[i] + "&value=" + value;
            browser.browser.Load(urlmatch);
            EventHandler<LoadingStateChangedEventArgs> l = null;
            l = new EventHandler<LoadingStateChangedEventArgs>(async (x, y) =>
            {
                if (!(y as LoadingStateChangedEventArgs).IsLoading)
                    if (browser.browser.Address.Contains(urlmatch))
                    {
                        browser.browser.LoadingStateChanged -= l;
                        string json = await browser.browser.GetTextAsync();
                        JObject o = JObject.Parse(json);
                        string message = (string)o["message"];
                        if (message == "This match has already started.")
                        {
                            message = "Этот матч уже начался.";
                        }
                        if (message == "1")
                        {
                            message = "Прогноз сделан.";
                        }
                        if (message == "Prediction is offline.")
                        {
                            message = "Прогнозы отключены.";
                        }
                        Invoke(new XDD(textBox1.AppendText), new string[] { " " + message + Environment.NewLine });
                        if (i < matchid.Count - 1)
                        {
                            next(++i);
                        }
                        else if (i >= matchid.Count - 1)
                        {
                            Invoke(new XDD(textBox1.AppendText), new string[] { "Готово. Все прогнозы сделаны." + Environment.NewLine });
                            started = false;
                        }
                    }
            });
            browser.browser.LoadingStateChanged += l;
            HD.LoadHtml(await browser.browser.GetSourceAsync());
        }

        List<int> matchid;

        public async Task Parse()
        {
            matchid = new List<int>();
            HtmlDocument HD = new HtmlDocument();
            var web = new HtmlWeb
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8,
            };
            HD = new HtmlDocument();
            HD.LoadHtml(await browser.browser.GetSourceAsync());
            HtmlNodeCollection bodyNode = HD.DocumentNode.SelectNodes("//div[@class='panel-body']/a");
            foreach (var hn in bodyNode)
                matchid.Add(Convert.ToInt32(hn.Attributes["id"].Value));
            Invoke(new XDD(textBox1.AppendText), new string[] { "Матчи успешно получены. Всего матчей: " + matchid.Count + Environment.NewLine });
            next(0);
        }

        static bool started = false;

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.AppendText( "Получение списка матчей..." + Environment.NewLine );
            browser.browser.Load("http://www.nxtgame.com/?sports=0");
            EventHandler<LoadingStateChangedEventArgs> loading = null;
            loading = new EventHandler<LoadingStateChangedEventArgs>(async (x, y) =>
            {
                if (!(y as LoadingStateChangedEventArgs).IsLoading)
                    if (browser.browser.Address.Contains("http://www.nxtgame.com/?sports=0"))
                    {
                        browser.browser.LoadingStateChanged -= loading;
                        if (!started)
                        {
                            started = true;
                            await Parse();
                        }
                    }
            });
            browser.browser.LoadingStateChanged += loading;
        }

        private void button4_Click(object sender, EventArgs e)
        {
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
    }
}