using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dreaw
{
    public partial class code : Form
    {
        public code()
        {
            InitializeComponent();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            // Khởi tạo và mở Form2
            newpw newForm = new newpw();
            newForm.Show();
            this.Hide();
        }
    }
}
