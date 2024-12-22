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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dreaw
{
    public partial class world : Form
    {
        HubConnection connection;
        SKBitmap btmap;
        bool is_join_a_room;
        const string serverAdd = "https://localhost:7183/api/hub"; //Địa chỉ Server
        const string serverAPIAdd = "https://localhost:7183";
        const string serverIP = "127.0.0.1";
        List<userRoom> userRooms = new List<userRoom>();
        int selectedRoom = -1;
        string roomname;
        string ownerID;
        readonly string usrrname;
        readonly string avtPic;
        readonly string userID;
        public world(string usrrname, string userID, string avtPic = null)
        {
            InitializeComponent();
            userRooms.Add(new userRoom("MU -0.5", "15/12/2024", 3234));
            userRooms.Add(new userRoom("Chelsea -1", "15/12/2024", 4536));
            userRooms.Add(new userRoom("Black Goku ngầu vê lờ", "15/12/2024", 1234));
            userRooms.Add(new userRoom("Tot gà -2.5", "15/12/2024", 5645));
            userRooms.Add(new userRoom("Ông Từ -2", "15/12/2024", 8967));
            userRooms.Add(new userRoom("Thái Lan -1.5", "15/12/2024", 3245));
            userRooms.Add(new userRoom("Nhà vua ĐNÁ -1", "15/12/2024", 5234));
            userRooms.Add(new userRoom("Vạn vật thua Goku", "15/12/2024", 7654));
            if (userRoomList.Controls.Count > 0)
            {
                userRoomList.Controls.Clear();
            }
            foreach (userRoom room in userRooms)
            {
                userRoomList.Controls.Add(room);
                room.Click += (s, e) =>
                {
                    foreach (userRoom roomm in userRoomList.Controls)
                    {
                        roomm.BackColor = Color.FromArgb(239, 241, 230);
                    }
                    room.BackColor = Color.LightYellow;
                    selectedRoom = room.ID;
                    roomname = room.room_Name;
                };
                room.Show();
            }
            var roommm = userRooms.FirstOrDefault();
            if (roommm != null)
            {
                roommm.BackColor = Color.LightYellow;
                selectedRoom = roommm.ID;
                roomname = roommm.room_Name;
            }
            this.usrrname = usrrname;
            this.avtPic = avtPic;
            this.userID = userID;
        }

        //Kết nối với phòng trong danh sách
        private async void pictureBox4_Click(object sender, EventArgs e)
        {
            if (!is_join_a_room)
            {
                is_join_a_room = true;
                Cursor = Cursors.WaitCursor;
                if (selectedRoom == -1)
                {
                    Cursor = Cursors.Default;
                    return;
                }
                var currentbmp = await GetBitmap(selectedRoom);
                await ConnectServer(userID, selectedRoom, roomname, userID, usrrname);
                if (currentbmp != null)
                {
                    var imageData = Convert.FromBase64String(currentbmp);
                    var crtbmp = SKBitmap.Decode(imageData);
                    DoAnPaint.Form1 drawingpanel = new DoAnPaint.Form1(serverIP, connection, selectedRoom, usrrname, crtbmp);
                    is_join_a_room = false;
                    drawingpanel.Show();
                }
                else
                {
                    DoAnPaint.Form1 drawingpanel = new DoAnPaint.Form1(serverIP, connection, selectedRoom, usrrname);
                    is_join_a_room = false;
                    drawingpanel.Show();
                }
            }
            else
            {
                MessageBox.Show("Wait for the last request to be completed!");
                return;
            }
        }

        /// <summary>
        /// Kết nối tới server
        /// </summary>
        private async Task ConnectServer(string ownerID, int roomID, string roomName, string userID, string usrname)
        {
            connection = new HubConnectionBuilder()
                .WithUrl($"{serverAdd}?ownerID={ownerID}&roomname={roomName}&roomID={roomID}&userID={userID}&name={usrname}", options =>
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
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }

        //Tạo phòng
        private async void pictureBox2_Click(object sender, EventArgs e)
        {
            if (!is_join_a_room)
            {
                is_join_a_room = true;
                Cursor = Cursors.WaitCursor;
                var enterName = new CreateRoom();
                enterName.ShowDialog();
                selectedRoom = Convert.ToInt32(enterName.ID);
                roomname = enterName.name;
                if (selectedRoom == -1)
                {
                    Cursor = Cursors.Default;
                    return;
                }
                await ConnectServer(userID, selectedRoom, roomname, userID, usrrname);
                DoAnPaint.Form1 drawingpanel = new DoAnPaint.Form1(serverIP, connection, selectedRoom, usrrname);
                drawingpanel.Show();
                Cursor = Cursors.Default;
                is_join_a_room = false;
            }
            else
            {
                MessageBox.Show("Wait for last request to be completed!");
                return;
            }
        }

        //Join phòng
        private async void pictureBox5_Click(object sender, EventArgs e)
        {
            if (!is_join_a_room)
            {
                is_join_a_room = true;
                Cursor = Cursors.WaitCursor;
                var codeForm = new enterCode();
                codeForm.ShowDialog();
                selectedRoom = codeForm.GetCode();
                if (selectedRoom != -1)
                {
                    using (var client = new HttpClient())
                    {
                        var isInList = userRooms.Select(room => room.ID).ToList().Contains(selectedRoom);
                        if (isInList)
                        {
                            var currentbmp = await GetBitmap(selectedRoom);
                            await ConnectServer("", selectedRoom, roomname, userID, usrrname);
                            if (currentbmp != null)
                            {
                                var imageData = Convert.FromBase64String(currentbmp);
                                var crtbmp = SKBitmap.Decode(imageData);
                                DoAnPaint.Form1 drawingpanel = new DoAnPaint.Form1(serverIP, connection, selectedRoom, usrrname, crtbmp);
                                is_join_a_room = false;
                                drawingpanel.Show();
                            }
                            else
                            {
                                DoAnPaint.Form1 drawingpanel = new DoAnPaint.Form1(serverIP, connection, selectedRoom, usrrname);
                                is_join_a_room = false;
                                drawingpanel.Show();
                            }
                            await ConnectServer("", selectedRoom, roomname, userID, usrrname);
                        }  
                        else
                        {
                            var content = new StringContent(selectedRoom.ToString(), Encoding.UTF8, "text/plain");
                            var response = await client.PostAsync($"{serverAPIAdd}/api/room/getname", content);
                            roomname = await response.Content.ReadAsStringAsync();
                            var currentbmp = await GetBitmap(selectedRoom);
                            await ConnectServer("", selectedRoom, roomname, userID, usrrname);
                            if (currentbmp != null)
                            {
                                var imageData = Convert.FromBase64String(currentbmp);
                                var crtbmp = SKBitmap.Decode(imageData);
                                DoAnPaint.Form1 drawingpanel = new DoAnPaint.Form1(serverIP, connection, selectedRoom, usrrname, crtbmp);
                                is_join_a_room = false;
                                drawingpanel.Show();
                            }
                            else
                            {
                                DoAnPaint.Form1 drawingpanel = new DoAnPaint.Form1(serverIP, connection, selectedRoom, usrrname);
                                is_join_a_room = false;
                                drawingpanel.Show();
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Code not valid!");
                    Cursor = Cursors.Default;
                    is_join_a_room = false;
                    return;
                } 
                    
            }
            else
            {
                MessageBox.Show("Wait for the last request to be completed!");
                return;
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void world_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(avtPic))
                Avatarr.Visible = false;
            AdjustFontSize(usrname, usrrname);
            usrname.Text = usrrname;
            if (IsTextTruncated(usrname))
            {
                // Thiết lập ToolTip một lần duy nhất
                toolTip1.SetToolTip(usrname, usrname.Text);
            }    
        }

        /// <summary>
        /// Tính toán cỡ chữ
        /// </summary>
        private void AdjustFontSize(Label label, string text)
        {
            float fontSize = 20; // Cỡ chữ ban đầu
            SizeF textSize;
            var graphics = label.CreateGraphics();
            var font = new Font(label.Font.FontFamily, fontSize);

            do
            {
                font = new Font(label.Font.FontFamily, fontSize);
                textSize = graphics.MeasureString(text, font);
                fontSize--;
            }
            while ((textSize.Width > label.Width || textSize.Height > label.Height) && fontSize > 8);
            label.Font = font;
        }

        // Hàm kiểm tra văn bản có bị cắt không
        private bool IsTextTruncated(Label label)
        {
            Size textSize = TextRenderer.MeasureText(label.Text, label.Font);
            return textSize.Width > label.Width || textSize.Height > label.Height;
        }

        private async Task<string> GetBitmap(int roomCode)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(roomCode.ToString(), Encoding.UTF8, "text/plain");
                var response = await client.PostAsync($"{serverAPIAdd}/api/room/getcurrentbmp", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;
                }
                else
                    return null;
            }
        }
    }
}
