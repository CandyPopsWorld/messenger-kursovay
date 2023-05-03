using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace messenger
{
    public partial class Form2 : Form
    {
        string userId = ConfigurationManager.AppSettings["UserId"];
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["IsRegistered"].Value = "false";
            config.AppSettings.Settings["UserId"].Value = "";
            config.Save(ConfigurationSaveMode.Modified);
            Application.Restart();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show(userId);
        }
    }
}
