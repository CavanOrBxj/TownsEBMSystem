using System.Diagnostics;
using System.Windows.Forms;
using CCWin;

namespace TownsEBMSystem
{
    public partial class UpgradeForm : Form
    {
       // public bool IsSure;
        public UpgradeForm()
        {
            InitializeComponent();
            this.Load += UpgradeForm_Load;

        }

        void UpgradeForm_Load(object sender, System.EventArgs e)
        {
            int Size_x = (this.Width - label1.Size.Width) / 2;
            int Size_y = label1.Location.Y;
            label1.Location = new System.Drawing.Point(Size_x, Size_y);
           // IsSure = false;
        }

        private void btn_OK_Click(object sender, System.EventArgs e)
        {

            SingletonInfo.GetInstance().UpgradeFlag = "0";
            MainForm.ini.WriteValue("SystemConfig", "UpgradeFlag", "0");

            Process m_Process = null;
            m_Process = new Process();
            m_Process.StartInfo.FileName = Application.StartupPath.ToString() + "\\CopyFile.exe";
            m_Process.Start();
            Application.Exit();
        }

        private void btn_cancle_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void picClose_Click(object sender, System.EventArgs e)
        {
            Close();
        }

     
    }
}
