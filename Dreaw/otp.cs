using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Data.SqlClient;

namespace Dreaw
{
    public partial class otp : Form
    {
        private const string ConnectionString = @"Server=LAPTOP-02EOBG92;Database=TEST;Trusted_Connection=True;";
        private string _otpCode;  // Mã OTP đã gửi
        private string _name;     // Tên người dùng
        private string _email;    // Email người dùng
        private string _password; // Mật khẩu đã nhập
        public otp(string name, string email, string password)
        {
            InitializeComponent();
            _name = name;
            _email = email;
            _password = password;
            _otpCode = GenerateOTP(); // Tạo mã OTP
            MessageBox.Show("OTP has been sent to your email.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SendOTPEmail(_email, _otpCode);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void otp_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("A new OTP has been sent to your email.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _otpCode = GenerateOTP(); // Tạo mã OTP mới
            SendOTPEmail(_email, _otpCode); // Gửi mã OTP mới
            
        }
        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // Tạo mã OTP 6 chữ số
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

                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send OTP email: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            string enteredOTP = txtOTP.Text.Trim();

            if (string.IsNullOrEmpty(enteredOTP))
            {
                MessageBox.Show("Please enter the OTP!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (enteredOTP == _otpCode)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // Lưu thông tin vào cơ sở dữ liệu sau khi OTP khớp
                        string query = @"
                            INSERT INTO Users (userID, name, password_hash, email, isDrawing, avatar)
                            VALUES (NEWID(), @Name, @PasswordHash, @Email, 0, NULL)";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Name", _name);
                            command.Parameters.AddWithValue("@PasswordHash", HashPassword(_password));
                            command.Parameters.AddWithValue("@Email", _email);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Sign Up Successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Chuyển sang form Login
                                Loginform loginForm = new Loginform();
                                loginForm.Show();
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Sign Up Failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Invalid OTP! Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
