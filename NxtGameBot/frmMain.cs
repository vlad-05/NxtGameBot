using CefSharp;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
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

        public frmMain()
        {
            browser = new frmBrowser();
            browser.Show();
            browser.Hide();
            InitializeComponent();
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            label2.Text = string.Format("ver.: {0}.{1} (build {2})", version.Major,version.Minor,version.Build);
        }

        public delegate void XD();
        public delegate void XDD(string text);

        private void button1_Click(object sender, EventArgs e)
        {
            browser.browser.LoadingStateChanged += new EventHandler<LoadingStateChangedEventArgs>((x, y) =>
            {
                if (!(y as LoadingStateChangedEventArgs).IsLoading)
                    if (browser.browser.Address.Contains("http://www.nxtgame.com/")) Invoke(new XD(browser.Hide));
            });
            browser.Show();
            browser.browser.Load("http://www.nxtgame.com/auth");
        }

        public async Task Parse()
        {
            Invoke(new XDD(textBox1.AppendText), new string[] { Environment.NewLine });
            List<int> matchid = new List<int>();
            double mA = 0;
            double mB = 0;
            string outputTextA;
            string outputTextB;
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
            Invoke(new XDD(textBox1.AppendText), new string[] { "Всего матчей: " + matchid.Count });
            Invoke(new XDD(textBox1.AppendText), new string[] { Environment.NewLine });
            //c = c - 1;
            HtmlDocument HD1 = new HtmlDocument();
            var web1 = new HtmlWeb
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8,
            };
            for (int i = 0; i <= matchid.Count-1; i++)
            {
                HD1 = web.Load("http://www.nxtgame.com/match/details/" + matchid[i]);
                HtmlNodeCollection bodyNodeA = HD1.DocumentNode.SelectNodes("//div[@class='col-xs-6 col-md-3 text-center odds-panel-teamA']/span");
                foreach (var hnA in bodyNodeA)
                {
                    Invoke(new XDD(textBox1.AppendText), new string[] { "Матч " + matchid[i] + ": " });
                    outputTextA = hnA.InnerText.Trim();
                    mA = Convert.ToDouble(outputTextA.Replace(".", ","));
                    Invoke(new XDD(textBox1.AppendText), new string[] { mA.ToString() });
                }
                HtmlNodeCollection bodyNodeB = HD1.DocumentNode.SelectNodes("//div[@class='col-xs-6 col-md-3 text-center odds-panel-teamB']/span");
                foreach (var hnB in bodyNodeB)
                {
                    Invoke(new XDD(textBox1.AppendText), new string[] { " vs " });
                    outputTextB = hnB.InnerText.Trim();
                    mB = Convert.ToDouble(outputTextB.Replace(".", ","));
                    Invoke(new XDD(textBox1.AppendText), new string[] { mB.ToString() });
                }
                if (mA > mB)
                {
                    Invoke(new XDD(textBox1.AppendText), new string[] { " -> Команда Б" });
                    browser.browser.Load("http://www.nxtgame.com/prediction/action?action=add&matchid=" + matchid[i] + "&value=3");
                    HD.LoadHtml(await browser.browser.GetSourceAsync());
                }
                if (mA < mB)
                {
                    Invoke(new XDD(textBox1.AppendText), new string[] { " -> Команда А" });
                    browser.browser.Load("http://www.nxtgame.com/prediction/action?action=add&matchid=" + matchid[i] + "&value=1");
                    HD.LoadHtml(await browser.browser.GetSourceAsync());
                }
                if (mA == mB)
                {
                    Invoke(new XDD(textBox1.AppendText), new string[] { " -> Ничья" });
                    browser.browser.Load("http://www.nxtgame.com/prediction/action?action=add&matchid=" + matchid[i] + "&value=2");
                    HD.LoadHtml(await browser.browser.GetSourceAsync());
                }
                Invoke(new XDD(textBox1.AppendText), new string[] { Environment.NewLine });
            }
            Invoke(new XDD(textBox1.AppendText), new string[] { "Готово" });
            started = false;
        }

        EventHandler<LoadingStateChangedEventArgs> loaded;
        static bool started = false;

        private void button2_Click(object sender, EventArgs e)
        {
            browser.browser.Load("http://www.nxtgame.com/profile");
            browser.browser.LoadingStateChanged += new EventHandler<LoadingStateChangedEventArgs>(async (x, y) =>
            {
                if (!(y as LoadingStateChangedEventArgs).IsLoading)
                    if (browser.browser.Address.Contains("http://www.nxtgame.com/profile"))
                    {
                        browser.browser.LoadingStateChanged -= loaded;
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
                            Invoke(new XDD((xy) => {
                                label1.Text = xy;
                            }), new string[] { nickname });
                        }
                    }
            });

            browser.browser.LoadingStateChanged += loaded;

            textBox1.AppendText("Получение списка матчей...");
            browser.browser.Load("http://www.nxtgame.com/?sports=0");
            browser.browser.LoadingStateChanged += new EventHandler<LoadingStateChangedEventArgs>(async (x, y) =>
            {
                if (!(y as LoadingStateChangedEventArgs).IsLoading)
                    if (browser.browser.Address.Contains("http://www.nxtgame.com/?sports=0"))
                    {
                        browser.browser.LoadingStateChanged -= loaded;
                        if (!started)
                        {
                            started = true;
                            await Parse();
                        }
                    }
            });
            browser.browser.LoadingStateChanged += loaded;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string LatestGithubReleaseURL = "https://api.github.com/repos/vlad-05/NxtGameBot/releases/latest";
            string Version = "0.1";

            browser.browser.Load(LatestGithubReleaseURL);
            browser.browser.LoadingStateChanged += new EventHandler<LoadingStateChangedEventArgs>(async (x, y) =>
            {
                if (!(y as LoadingStateChangedEventArgs).IsLoading)
                    if (browser.browser.Address.Contains("https://api.github.com/repos/vlad-05/NxtGameBot/releases/latest"))
                    {
                        browser.browser.LoadingStateChanged -= loaded;
                        if (!started)
                        {
                            started = true;
                            string json = await browser.browser.GetTextAsync();
                            JObject o = JObject.Parse(json);
                            string LatestVersion = (string)o["tag_name"];
                            if (Version != LatestVersion)
                            {
                                Invoke(new XDD(textBox1.AppendText), new string[] { "Загрузка новой версии...\r\n" });
                                WebClient webClient = new WebClient();
                                webClient.DownloadProgressChanged += delegate (object q, DownloadProgressChangedEventArgs args)
                                {
                                    progressBar1.Value = args.ProgressPercentage;
                                    int a = progressBar1.Value;
                                    Invoke(new XDD((str) =>
                                    {
                                        textBox1.Text = "Загрузка новой версии...\r\nЗавершено: " + str + "%";
                                    }), new string[] { a.ToString() });
                                };
                                webClient.DownloadFileCompleted += delegate (object q, AsyncCompletedEventArgs args)
                                {
                                    progressBar1.Value = 100;
                                    if (progressBar1.Value == 100)
                                    {
                                        string zipFilePath = "NGB.zip";
                                        FileStream zipFileStream = File.OpenRead(zipFilePath);
                                        ZipArchive zipFileArchive = new ZipArchive(zipFileStream);
                                        IEnumerator<ZipArchiveEntry> zipFileEnum = zipFileArchive.Entries.GetEnumerator();
                                        while (zipFileEnum.MoveNext())
                                        {
                                            using (FileStream file = File.Create("update/"+zipFileEnum.Current.Name))
                                            using (Stream zipFile = zipFileEnum.Current.Open())
                                            {
                                                zipFile.CopyTo(file);
                                            }
                                        }
                                        Process process = new Process();
                                        //process.StartInfo.FileName = exeExtractedPath;
                                        if (process.Start())
                                            Application.Exit();
                                        else
                                            MessageBox.Show("Error");
                                    }
                                };
                                webClient.DownloadFileAsync(new Uri("https://github.com/vlad-05/NxtGameBot/releases/download/" + LatestVersion + "/NGB.zip"), "NGB.zip");
                            }
                            else
                            {
                                MessageBox.Show("Нет обновлений.");
                            }
                        }
                    }
            });
        }
    }
}
