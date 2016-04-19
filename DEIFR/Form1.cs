using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DEIFR
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            numericUpDown1.Value = Program.MaxImages;
            checkBox1.Checked = Program.KeepImages;
            Program.Progress = progressBar1;
            Program.AllProgress = progressBar2;
            this.FormClosing += Form1_FormClosing;
            ImageDownloader.Update();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ImageDownloader.Stop)
            {
                ImageDownloader.Stop = true;
                e.Cancel = true;
                this.ControlBox = false;
                this.Enabled = false;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Program.KeepImages = checkBox1.Checked;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Program.MaxImages = (int)numericUpDown1.Value;
            numericUpDown1.Value = Program.MaxImages; //A way to disallow non-integer inputs
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
