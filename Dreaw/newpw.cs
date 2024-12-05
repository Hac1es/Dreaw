using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace Dreaw
{
    public partial class newpw : Form
    {
        private string _userEmail; // Email người dùng từ OTP form.
        private const string ConnectionString = @"Server=LAPTOP-02EOBG92;Database=TEST;Trusted_Connection=True;";

        public newpw(string userEmail)
        {
            InitializeComponent();
            _userEmail = userEmail; // Lưu email của người dùng.
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            string newPassword = txtNewPassword.Text.Trim();
            if (string.IsNullOrEmpty(newPassword) )
            {
                MessageBox.Show("Please fill in both fields!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "UPDATE Users SET password_hash = @PasswordHash WHERE email = @Email";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PasswordHash", HashPassword(newPassword));
                        command.Parameters.AddWithValue("@Email", _userEmail);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Password updated successfully! Please log in with your new password.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Chuyển hướng về form đăng nhập
                            Loginform loginForm = new Loginform();
                            loginForm.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Failed to update password. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
            private string HashPassword(string password)
            {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
