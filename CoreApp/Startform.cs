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
    /*Flow khi chạy app:
     * Login -> Server xác thực -> Đúng -> world
     * Sign up -> Server gửi OTP xác thực email -> Gửi OTP về app -> App tạo Form code với OTP kẹp vào 
     -> Xác thực OTP trong form code -> form Code gửi lệnh lên Server -> Server lưu thông tin lên DB -> Login
     * Forget PW -> Nhập email -> Server kiểm tra rồi gửi OTP xác thực email -> Gửi OTP về app 
    -> App tạo Form code với OTP kẹp vào -> Xác thực OTP trong form code -> form newpw -> newpw gửi thông tin lên Server 
    -> Server lưu pass mới lên DB -> Login
    */
    public partial class Startform: Form
    {
        public Startform()
        {
            InitializeComponent();
            #region FixBugUI
            this.WindowState = FormWindowState.Maximized;
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            Resolution objFormResizer = new Resolution();
            objFormResizer.ResizeForm(this, screenHeight, screenWidth);
            #endregion
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            // Khởi tạo và mở Form2
            Loginform newForm = new Loginform();

            // Hiển thị Form2
            newForm.Show();
        }

        private void Startform_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Form này đóng == Đóng app
            Application.Exit();
        }
    }
}
