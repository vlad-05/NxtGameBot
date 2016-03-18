using CefSharp;
using CefSharp.WinForms;
using System;
using System.Windows.Forms;

namespace NxtGameBot
{
    public partial class frmBrowser : Form
    {
        public ChromiumWebBrowser browser;

        bool InitBrowser()
        {
            CefSettings cefSettings = new CefSettings
            {
                CachePath = Environment.CurrentDirectory + "/cache",
            };
            if (!Cef.IsInitialized)
                if (!Cef.Initialize(cefSettings))
                    return false;
            browser = new ChromiumWebBrowser("about:blank");
            browser.Dock = DockStyle.Fill;
            this.Controls.Add(browser);
            return true;
        }

        public frmBrowser()
        {
            InitializeComponent();
            InitBrowser();
        }

        private void Browser_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}