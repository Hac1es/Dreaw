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
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Dreaw
{
    public partial class Siginform : Form
    {
        const string serverAdd = "https://localhost:7183"; //Địa chỉ server
        bool isSending = false; //Cái isSending này giống nhau ở nhiều form, nên tui viết
        //1 lần thôi:D Nó tượng trưng cho việc đã gửi request lên server chưa(vì request cần thời 
        //gian để xử lý). Nếu nó là true, sẽ chặn việc gửi request đi tiếp
        public Siginform()
        {
            InitializeComponent();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            // Khởi tạo và mở Form2
            Loginform newForm = new Loginform();
            newForm.Show();
            this.Close();
        }

        private async void pictureBox3_Click(object sender, EventArgs e)
        {
            if (isSending)
            {
                MessageBox.Show("Completed the last request first!"); //Đây, chặn
                return;
            }
            string name = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();
            //Kiểm tra dử liệu đầu vào
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
            // Tạo dữ liệu cần gửi
            var requestData = new
            {
                Email = email
            };
            //Gửi request bằng HttpClient
            using (var client = new HttpClient())
            {
                isSending = true;
                var jsonRequest = JsonConvert.SerializeObject(requestData); //Đóng gói dạng JSON
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json"); 
                Cursor.Current = Cursors.WaitCursor;
                var response = await client.PostAsync($"{serverAdd}/api/signup", content); //HttpPost
                if (response.IsSuccessStatusCode) //Nếu là 200 OK
                {
                    var responseContent = 
                        await response.Content.ReadAsStringAsync(); //Đọc thành string JSON
                    var responseObject = 
                        JsonConvert.DeserializeObject<dynamic>(responseContent); //Convert thành một object
                    string otp = responseObject?.otp; //lấy ra otp
                    code newForm = 
                        new code(otp, name, email, password, "signup"); //Gán OTP vào form code
                    isSending = false; //Đặt cờ Send về False
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show("A new OTP has been sent to your email.", 
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    newForm.Show();
                    this.Close();
                } 
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict) //Mail đã tồn tại
                {
                    MessageBox.Show("Email exists! Please sign in!");
                    Cursor.Current = Cursors.Default;
                    isSending = false;
                    return;
                }
                else
                {
                    MessageBox.Show("An error has occured!"); //Lỗi Server
                    Cursor.Current = Cursors.Default;
                    isSending = false;
                    return;
                }
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
    }
}
