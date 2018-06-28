using System;
using System.Drawing;
using System.Windows.Forms;
using CCWin;
using System.IO;

namespace TownsEBMSystem
{
    public partial class MainForm : CCSkinMain
    {

        AutoSizeFormClass asc = new AutoSizeFormClass();

        public MainForm()
        {
           
            InitializeComponent();
            //记录初始尺寸
            asc.controllInitializeSize(this);
            WindowState= FormWindowState.Maximized;
            //任务栏不显示
            this.ShowInTaskbar = false;
            Load += MainForm_Load;


        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            //   this.skinLabel1.Font = new System.Drawing.Font("微软雅黑", 40F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            #region 使标题居中
            int ylocation = skinLabel1.Location.Y;
            int with = skinLabel1.Size.Width;
            skinLabel1.Location = new System.Drawing.Point((this.Width - with) /2, ylocation);
            #endregion

            switchImage1.Font = new Font("微软雅黑", 11);
            //  switchImage1.ForeColor = Color.Transparent;
            string[] jpgFiles = Directory.GetFiles(System.Windows.Forms.Application.StartupPath + "\\image", "*.jpg");

            foreach (var item in jpgFiles)
            {
                switchImage1.AddImageItems(item, "", Color.Transparent);
            }


            WebKit.WebKitBrowser browser = new WebKit.WebKitBrowser();
            browser.Dock = DockStyle.Fill;
            panel1.Controls.Add(browser);
            browser.Navigate("http://www.baidu.com");

        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            //自适应
            asc.controlAutoSize(this);

        }

        private void skinButton2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
