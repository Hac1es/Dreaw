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

namespace Dreaw.WorldForm
{
    public partial class createRoom : Form
    {
        HttpClient client;
        bool isSending = false;
        const string serverAdd = "https://localhost:7183"; //Địa chỉ server
        public createRoom()
        {
            InitializeComponent();
            #region FixBugUI
            this.WindowState = FormWindowState.Maximized;
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            Resolution objFormResizer = new Resolution();
            objFormResizer.ResizeForm(this, screenHeight, screenWidth);
            #endregion
            client = new HttpClient();
            button1.Visible = false;
        }

        public string RoomName { get; private set; }

        public string RoomID { get; private set; }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            var username = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(username))
            {
                Status.ForeColor = Color.Black;
                Status.Text = "Type something!";
                button1.Visible = false;
                return;
            }

            if (username.Length > 20)
            {
                Status.ForeColor = Color.Red;
                Status.Text = "Room name need to be less than 20 characters!";
                button1.Visible = false;
                return;
            }
            Status.ForeColor = Color.Green;
            Status.Text = "OK!";
            button1.Visible = true;
        }

        private async Task<bool> CheckRoomID(string ID)
        {
            isSending = true;
            var content = new StringContent(ID, Encoding.UTF8, "text/plain");
            var result = await client.PostAsync($"{serverAdd}/api/rooms/validID", content);
            if (result.IsSuccessStatusCode)
            {
                isSending = false;
                await Task.Delay(100);
                return true;
            }
            else
            {
                isSending = false;
                await Task.Delay(100);
                return false;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            RoomName = textBox1.Text.Trim();
            if (isSending) return;
            Random random = new Random();
            var result = false;
            RoomID = "-1";
            string generated;
            do
            {
                generated = random.Next(1000, 10000).ToString(); // Sinh số từ 1000 đến 9999
                result = await CheckRoomID(generated);
            } while (!result);
            RoomID = generated;
            this.Close();
        }
    }
}
