using CefSharp;
using CefSharp.WinForms;
using System;
using System.Windows.Forms;

namespace NxtGameBot
{
    public partial class frmBrowser : Form
    {
        public ChromiumWebBrowser browser;

        public frmBrowser()
        {
            InitializeComponent();
            InitBrowser();
        }

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

        private void Browser_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}