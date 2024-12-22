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
    public partial class CreateRoom : Form
    {
        public string name { get; set; }
        public string ID { get; set; }
        const string serverAdd = "https://localhost:7183";
        public CreateRoom()
        {
            InitializeComponent();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(richTextBox1.Text))
            {
                Noti.Visible = true;
                Noti.ForeColor = Color.Black;
                Noti.Text = "Type something!";
                button1.Visible = false;
                return;
            }
            if (richTextBox1.Text.Length > 20)
            {
                Noti.Visible = true;
                Noti.ForeColor = Color.Red;
                Noti.Text = "Room name need to be less than 20 character!";
                button1.Visible = false;
                return;
            }
            button1.Visible = true;
            Noti.Visible = false;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            ID = await GenerateRoom();
            this.Close();
            name = richTextBox1.Text;
        }

        private async Task<string> GenerateRoom()
        {
            bool isEnd = false;
            string returned = "";
            Random random = new Random();
            var client = new HttpClient();
            do
            {
                var candidateID = random.Next(1000, 10000).ToString(); // Sinh số từ 1000 đến 9999
                var content = new StringContent(candidateID, Encoding.UTF8, "text/plain");
                var response = await client.PostAsync($"{serverAdd}/api/room/exists", content);
                if (response.IsSuccessStatusCode)
                    isEnd = false;
                else
                {
                    isEnd = true;
                    returned = candidateID;
                }
            } while (!isEnd);
            return returned;
        }
    }
}
