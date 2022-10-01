using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace FilmOrganizer
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Form_Visible(object sender, EventArgs e)
        {
            if (!this.Visible) return;
            this.SuspendLayout();
            Properties.Settings set = new Properties.Settings();

            rtb_Extensions.Text = "";

            foreach (string str in set.ExtensionsList)
                if (str != "@")
                    rtb_Extensions.AppendText(str+ "\n");

            this.ResumeLayout();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Properties.Settings set = new Properties.Settings();

            foreach (string str in rtb_Extensions.Text.Split(new string[] { "\n" },StringSplitOptions.None))
                if (!set.ExtensionsList.Contains(str) && str!="")
                    set.ExtensionsList.Add(str);

            set.Save();
            this.Hide();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            this.VisibleChanged += Form_Visible;
        }
    }
}
