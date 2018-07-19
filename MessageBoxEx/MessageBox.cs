using CCWin;
using MessageBoxEx.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MessageBoxEx
{
    public partial class MessageBox :SkinMain
    {
        private string context_text="无内容";
        public MessageBox()
        {
            InitializeComponent();
        }

        public string text
        {
            get { return context_text; }
            set { context_text = value; label_context.Text = context_text; }
        }
       
        private void button_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (this.DialogResult != DialogResult.Cancel && this.DialogResult != DialogResult.OK)
            //    e.Cancel = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
