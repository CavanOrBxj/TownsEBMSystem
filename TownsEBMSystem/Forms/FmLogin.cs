using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TownsEBMSystem
{
    public partial class FmLogin : Form
    {
        public FmLogin()
        {
            InitializeComponent();
        }
        public FmLogin(bool bl) //超时登录走这个
        {
            InitializeComponent();
            isTimer = bl;
        }
        public static bool isTimer = false;//判断是否是超时了
        private void Login_Load(object sender, EventArgs e)
        {
        
        }
        private void btn_num_1_Click(object sender, EventArgs e)
        {
            Txt_inputdata.Focus();
            SendKeys.Send("1");
        }

        private void btn_num_2_Click(object sender, EventArgs e)
        {
            Txt_inputdata.Focus();
            SendKeys.Send("2");
        }

        private void btn_num_3_Click(object sender, EventArgs e)
        {
            Txt_inputdata.Focus();
            SendKeys.Send("3");
        }

        private void btn_num_4_Click(object sender, EventArgs e)
        {
            Txt_inputdata.Focus();
            SendKeys.Send("4");
        }

        private void btn_num_5_Click(object sender, EventArgs e)
        {
            Txt_inputdata.Focus();
            SendKeys.Send("5");
        }

        private void btn_num_6_Click(object sender, EventArgs e)
        {
            Txt_inputdata.Focus();
            SendKeys.Send("6");
        }

        private void btn_num_7_Click(object sender, EventArgs e)
        {
            Txt_inputdata.Focus();
            SendKeys.Send("7");
        }

        private void btn_num_8_Click(object sender, EventArgs e)
        {
            Txt_inputdata.Focus();
            SendKeys.Send("8");
        }

        private void btn_num_9_Click(object sender, EventArgs e)
        {
            Txt_inputdata.Focus();
            SendKeys.Send("9");
        }

        private void btn_num_0_Click(object sender, EventArgs e)
        {
            Txt_inputdata.Focus();
            SendKeys.Send("0");
        }

        private void btn_Reset_Click(object sender, EventArgs e)
        {
            Txt_inputdata.Focus();
            Txt_inputdata.Text = "";
        }

        private void btn_num_OK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Txt_inputdata.Text.Trim()) || string.IsNullOrEmpty(Txt_inputdata.Text.Trim()))
            {
                MessageBox.Show("用户名或密码不能为空！");
                Txt_inputdata.Focus();
            }
            else
            {
                if (Txt_inputdata.Text.Trim() == SingletonInfo.GetInstance().logincode)
                {
                    SingletonInfo.GetInstance().lockstatus = false;
                    this.Close();
                 
                }
                else
                {
                    MessageBox.Show("密码错误！");
                    Txt_inputdata.Text = "";
                    Txt_inputdata.Focus();//获得焦点
                }
            }
        }
    }
}
