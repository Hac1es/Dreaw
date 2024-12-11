using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Dreaw
{
    public partial class code : Form
    {
        private string _verificationCode;
        private string _userEmail;
        public code(string verificationCode, string userEmail)
        {
            InitializeComponent();
            _verificationCode = verificationCode;
            _userEmail = userEmail;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            string enteredCode = txtVerificationCode.Text.Trim();

            if (string.IsNullOrEmpty(enteredCode))
            {
                MessageBox.Show("Please enter the verification code!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (enteredCode != _verificationCode)
            {
                MessageBox.Show("Invalid verification code!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Khởi tạo và mở Form2.
            newpw newForm = new newpw(_userEmail);
            newForm.Show();
            this.Hide();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("A new OTP has been sent to your email.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _verificationCode = GenerateOTP(); // Tạo mã OTP mới
            SendOTPEmail(_userEmail, _verificationCode); // Gửi mã OTP mới
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        private async void SendOTPEmail(string email, string otp)
        {
            await Task.Delay(100); // Giới thiệu một độ trễ nhỏ nếu cần

            try
            {
                // Cấu hình SMTP
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("23521458@gm.uit.edu.vn", "mnlwwrbfbojbmcza"), // Chuyển thông tin này ra cấu hình
                    EnableSsl = true
                };

                // Cấu hình email
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("23521458@gm.uit.edu.vn"),
                    Subject = "OTP Verification",
                    Body = $"Your OTP code is: {otp}",
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                // Gửi email
                await smtp.SendMailAsync(mail);

                MessageBox.Show("OTP has been sent to your email.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send OTP email: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // Tạo mã OTP 6 chữ số
        }
    }
}
