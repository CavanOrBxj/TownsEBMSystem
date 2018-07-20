using System.Windows.Forms;
using CCWin;

namespace TownsEBMSystem
{
    public partial class MessageShowForm : Form
    {
        public bool IsSure;
        public MessageShowForm()
        {
            InitializeComponent();
            this.Load += MessageShowForm_Load;

        }

        void MessageShowForm_Load(object sender, System.EventArgs e)
        {
            int Size_x = (this.Width - label1.Size.Width) / 2;
            int Size_y = label1.Location.Y;
            label1.Location = new System.Drawing.Point(Size_x, Size_y);
            IsSure = false;
        }

        private void btn_OK_Click(object sender, System.EventArgs e)
        {
            IsSure = true;
            Close();
        }

        private void btn_cancle_Click(object sender, System.EventArgs e)
        {
            IsSure = false;
            Close();
        }

        private void picClose_Click(object sender, System.EventArgs e)
        {
            IsSure = false;
            Close();
        }

     
    }
}
