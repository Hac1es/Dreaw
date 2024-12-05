using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Dreaw
{
    public partial class forgetpw : Form
    {
        public forgetpw()
        {
            InitializeComponent();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please enter your email!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Invalid email format!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Gửi mã xác thực qua email
                string verificationCode = GenerateVerificationCode();
                SendVerificationEmail(email, verificationCode);
                MessageBox.Show("Verification code sent to your email.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Điều hướng sang form xác thực
                // Khởi tạo và mở Form2
                code newForm = new code(verificationCode, email);
                newForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending email: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool IsValidEmail(string email)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }
        private string GenerateVerificationCode()
        {
            byte[] randomNumber = new byte[4];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomNumber);
            }
            int code = BitConverter.ToInt32(randomNumber, 0) % 1000000;
            return Math.Abs(code).ToString("D6");
        }
        private void SendVerificationEmail(string email, string verificationCode)
        {
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("thienlt.h.2023@gmail.com", "iyrsitfgmbbphfho"),
                EnableSsl = true
            };

            MailMessage mail = new MailMessage
            {
                From = new MailAddress("thienlt.h.2023@gmail.com"),
                Subject = "Password Reset Code",
                Body = $"Your password reset code is: {verificationCode}",
                IsBodyHtml = true
            };
            mail.To.Add(email);
            smtp.Send(mail);
        }
    }
}
