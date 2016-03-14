using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace NxtGameBot
{
    public partial class frmBrowser : Form
    {
        public ChromiumWebBrowser browser;

        bool InitBrowser()
        {
            CefSettings cefSettings = new CefSettings
            {
                CachePath = Environment.CurrentDirectory + "/Cache",
            };
            if (!Cef.IsInitialized)
                if (!Cef.Initialize(cefSettings))
                    return false;
            browser = new ChromiumWebBrowser("http://www.nxtgame.com/profile");
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
