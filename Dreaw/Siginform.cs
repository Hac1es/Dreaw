using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;

namespace Dreaw
{
    public partial class Siginform : Form
    {
        private const string ConnectionString = @"Server=LAPTOP-02EOBG92;Database=TEST;Trusted_Connection=True;";
        public Siginform()
        {
            InitializeComponent();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            // Khởi tạo và mở Form2
            Loginform newForm = new Loginform();
            newForm.Show();
            this.Hide();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill all fields!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Invalid email format!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    if (IsEmailExists(email, connection))
                    {
                        MessageBox.Show("Email already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Chuyển sang form OTP và truyền thông tin người dùng
                    otp newForm = new otp(name, email, password);
                    newForm.Show();
                    this.Hide();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            ClearFields();
        }
        
        
        private bool IsValidEmail(string email)
        {
            // Sử dụng Regex để kiểm tra định dạng email
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }
        private void ClearFields()
        {
            txtName.Clear();
            txtEmail.Clear();
            txtPassword.Clear();
        }
        
        private bool IsEmailExists(string email, SqlConnection connection)
        {
            string query = "SELECT COUNT(1) FROM Users WHERE email = @Email";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }
    }
}
