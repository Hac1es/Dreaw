using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Dreaw
{
    public partial class code : Form
    {
        string Case; //Cái này(form code) tạo ra để làm gì?
        string otp;
        bool isSending;
        string name;
        string email;
        string password;
        bool _isTicking;
        int timer = 0;
        const string serverAdd = "https://localhost:7183";
        public code(string otp, string name, string email, string password, string @case)
        {
            InitializeComponent();
            this.otp = otp;
            this.name = name;
            this.email = email;
            this.password = password;
            waittoResend.Start();
            _isTicking = true;
            Case = @case;
        }
        public code(string otp, string email, string @case)
        {
            InitializeComponent();
            #region FixBugUI
            this.WindowState = FormWindowState.Maximized;
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            Resolution objFormResizer = new Resolution();
            objFormResizer.ResizeForm(this, screenHeight, screenWidth);
            #endregion
            this.otp = otp;
            this.name = "";
            this.email = email;
            this.password = ""; //Truyền thông tin vào
            waittoResend.Start(); //Đếm ngược 3 phút, hết 3 phút thì mới cho gửi tiếp
            _isTicking = true;
            Case = @case;
        }

        private async void pictureBox3_Click(object sender, EventArgs e)
        {
            if (isSending)
            {
                MessageBox.Show("Complete the last request first!");
                return;
            }
            if (textBox1.Text.Trim() != otp)
            {
                MessageBox.Show("OTP is not correct!");
                return;
            }
            if (Case == "signup") //Nếu được tạo ra để hoàn tất Sign up
            {
                var requestData = new
                {
                    Username = name,
                    Email = email,
                    Password = password
                };
                using (var client = new HttpClient())
                {
                    isSending = true;
                    var jsonRequest = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                    Cursor = Cursors.WaitCursor;
                    var response = await client.PostAsync($"{serverAdd}/api/finishsignup", content);
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Sign Up Successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Chuyển sang form Login
                        Loginform loginForm = new Loginform();
                        isSending = false;
                        Cursor = Cursors.Default;
                        loginForm.Show();
                        this.Close();
                    }
                    else
                    {
                        isSending = false;
                        Cursor = Cursors.Default;
                        MessageBox.Show("Sign Up Failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            } 
            else //Nếu được tạo ra để hoàn tất Forgot PW
            {
                MessageBox.Show("OTP Verification Completed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                newpw newpw = new newpw(email);
                newpw.Show();
                this.Close();
            }
        }

        private async void pictureBox2_Click(object sender, EventArgs e) //Resend Code
        {
            if (!_isTicking && !isSending) 
            {
                var requestData = new
                {
                    Email = email
                };
                using (var client = new HttpClient())
                {
                    isSending = true;
                    var jsonRequest = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                    Cursor = Cursors.WaitCursor;
                    var response = await client.PostAsync($"{serverAdd}/api/resendotp", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        otp = responseObject?.otp; //Thay OTP bằng OTP server mới gửi
                        isSending = false;
                        MessageBox.Show("A new OTP has been sent to your email.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Cursor = Cursors.Default;
                        timer = 0;
                        _isTicking = true;
                        waittoResend.Start();
                    }
                    else
                    {
                        isSending = false;
                        Cursor = Cursors.Default;
                        MessageBox.Show("An error has occured!");
                    }
                }            
            }
            else if (_isTicking) //Nếu đang đếm 3 phút
            {
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(180000 - timer);
                MessageBox.Show($"Wait 3 minutes to resend. {timeSpan.Minutes}:{timeSpan.Seconds:D2} remaining.");
            }
            else
            {
                MessageBox.Show("Complete the last request first!");
                return;
            }
        }

        private void waittoResend_Tick(object sender, EventArgs e) //Đồng hồ
        {
            if (timer >= 180000)
            {
                waittoResend.Stop();
                _isTicking = false;
            }    
            else
            {
                timer += waittoResend.Interval;
            }
        }
    }
}
