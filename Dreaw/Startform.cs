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
    public partial class Startform: Form
    {
        public Startform()
        {
            InitializeComponent();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            // Khởi tạo và mở Form2
            Loginform newForm = new Loginform();

            // Đăng ký sự kiện FormClosed để thoát ứng dụng khi Form2 đóng
            newForm.FormClosed += (s, args) => Application.Exit();

            // Hiển thị Form2 và đóng Form1
            newForm.Show();
            this.Close();
        }
    }
}
