using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dreaw
{
    public partial class newpw : Form
    {
        string email;
        const string serverAdd = "https://localhost:7183";
        bool isSending;
        public newpw(string email)
        {
            InitializeComponent();
            this.email = email;
        }
        private async void pictureBox3_Click(object sender, EventArgs e)
        {
            //Kiểm tra đầu vào
            if (isSending)
            {
                MessageBox.Show("Completed the last request first!");
                return;
            }
            string pass = txtPass.Text.Trim();
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please fill email box!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Invalid email format!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Tạo dữ liệu cần gửi
            var requestData = new
            {
                Email = email,
                Password = pass
            };
            using (var client = new HttpClient())
            {
                isSending = true;
                var jsonRequest = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                Cursor.Current = Cursors.WaitCursor;
                var response = await client.PostAsync($"{serverAdd}/api/updatepw", content); //Y chang, chỉ khác endpoint
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Password updated successfully! Please log in with your new password.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Cursor.Current = Cursors.Default;
                    isSending = false;
                    Loginform loginForm = new Loginform();
                    loginForm.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("An error has occured!");
                    Cursor.Current = Cursors.Default;
                    isSending = false;
                    return;
                }
            }
        }

        private bool IsValidEmail(string email)
        {
            // Sử dụng Regex để kiểm tra định dạng email
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }
    }
}
