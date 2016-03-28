using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NxtGameBot
{
    public partial class frmSetting : Form
    {
        public frmSetting()
        {
            InitializeComponent();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoStart = checkBox1.Checked;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoStartInterval = (int)numericUpDown1.Value;
        }

        private void frmSetting_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void frmSetting_Shown(object sender, EventArgs e)
        {
            numericUpDown1.Value = Properties.Settings.Default.AutoStartInterval;
            checkBox1.Checked = Properties.Settings.Default.AutoStart;
        }
    }
}