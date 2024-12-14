using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DoAnPaint;
using SkiaSharp;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;
using Dreaw.WorldForm;

namespace Dreaw
{
    public partial class world : Form
    {
        HubConnection connection;
        SKBitmap btmap;
        bool is_join_a_room;
        const string serverAdd = "https://localhost:7183/api/hub"; //Địa chỉ Server
        const string serverIP = "127.0.0.1";
        List<userRoom> userRooms = new List<userRoom>();
        public world()
        {
            InitializeComponent();
            userRooms.Add(new userRoom("MU -0.5", "15/12/2024"));
            userRooms.Add(new userRoom("Chelsea -1", "15/12/2024"));
            userRooms.Add(new userRoom("Black Goku ngầu vê lờ", "15/12/2024"));
            userRooms.Add(new userRoom("Tot gà -2.5", "15/12/2024"));
            userRooms.Add(new userRoom("Ông Từ -2", "15/12/2024"));
            userRooms.Add(new userRoom("Thái Lan -1.5", "15/12/2024"));
            userRooms.Add(new userRoom("Nhà vua ĐNÁ -1", "15/12/2024"));
            userRooms.Add(new userRoom("Vạn vật thua Goku", "15/12/2024"));
            if (userRoomList.Controls.Count > 0)
            {
                userRoomList.Controls.Clear();
            }
            foreach (userRoom room in userRooms)
            {
                userRoomList.Controls.Add(room);
                room.Show();
            }
        }

        private async void pictureBox4_Click(object sender, EventArgs e)
        {
            if (!is_join_a_room)
            {
                is_join_a_room = true;
                Cursor = Cursors.WaitCursor;
                var completed = await ConnectServer();
                if (completed)
                {
                    DoAnPaint.Form1 drawingpanel = new DoAnPaint.Form1(serverIP, connection, 7777);
                    drawingpanel.Show();
                }
                else
                {
                    MessageBox.Show("Cannot connect to server!");
                }
                Cursor = Cursors.Default;
                is_join_a_room = false;
            }
            else
            {
                MessageBox.Show("Wait for last request to be completed!");
                return;
            }
        }

        /// <summary>
        /// Kết nối tới server
        /// </summary>
        private async Task<bool> ConnectServer()
        {
            connection = new HubConnectionBuilder()
                .WithUrl(serverAdd, options =>
                {
                    options.HttpMessageHandlerFactory = handler =>
                    {
                        if (handler is HttpClientHandler clientHandler)
                            clientHandler.ServerCertificateCustomValidationCallback =
                                (message, cert, chain, sslPolicyErrors) => true;
                        return handler;
                    };
                })
                .WithAutomaticReconnect(new[]
                {
            TimeSpan.Zero,   // Try reconnecting immediately
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(30)
                })
                .Build();

            try
            {
                // Start the connection
                await connection.StartAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async void pictureBox2_Click(object sender, EventArgs e)
        {
            if (!is_join_a_room)
            {
                is_join_a_room = true;
                Cursor = Cursors.WaitCursor;
                var completed = await ConnectServer();
                if (completed)
                {
                    Random random = new Random();
                    int randomNumber = random.Next(1000, 10000); // Sinh số từ 1000 đến 9999
                    DoAnPaint.Form1 drawingpanel = new DoAnPaint.Form1(serverIP, connection, randomNumber);
                    drawingpanel.Show();
                }
                else
                {
                    MessageBox.Show("Cannot connect to server!");
                }
                Cursor = Cursors.Default;
                is_join_a_room = false;
            }
            else
            {
                MessageBox.Show("Wait for last request to be completed!");
                return;
            }
        }

        private async void pictureBox5_Click(object sender, EventArgs e)
        {
            if (!is_join_a_room)
            {
                is_join_a_room = true;
                Cursor = Cursors.WaitCursor;
                var completed = await ConnectServer();
                if (completed)
                {
                    DoAnPaint.Form1 drawingpanel = new DoAnPaint.Form1(serverIP, connection, 7777);
                    drawingpanel.Show();
                }
                else
                {
                    MessageBox.Show("Cannot connect to server!");
                }
                Cursor = Cursors.Default;
                is_join_a_room = false;
            }
            else
            {
                MessageBox.Show("Wait for last request to be completed!");
                return;
            }
        }
    }
}
