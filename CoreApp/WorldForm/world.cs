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
        int selectedRoom = -1;
        string roomname = "";
        readonly string usrrname;
        readonly string avtPic;
        readonly string userID;
        public world(string usrrname, string userID, string avtPic = null)
        {
            InitializeComponent();
            #region FixBugUI
            this.WindowState = FormWindowState.Maximized;
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            Resolution objFormResizer = new Resolution();
            objFormResizer.ResizeForm(this, screenHeight, screenWidth);
            #endregion
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
                    roomname = room.RoomnAme;
                };
                room.Show();
            }
            var roommm = userRooms.FirstOrDefault();
            if (roommm != null)
            {
                roommm.BackColor = Color.LightYellow;
                selectedRoom = roommm.ID;
                roomname = roommm.RoomnAme;
            }
            this.usrrname = usrrname;
            this.avtPic = avtPic;
            this.userID = userID;
        }

        //Chọn phòng từ danh sách
        private async void pictureBox4_Click(object sender, EventArgs e)
        {
            if (!is_join_a_room)
            {
                is_join_a_room = true;
                Cursor = Cursors.WaitCursor;
                if (selectedRoom == -1)
                {
                    Cursor = Cursors.Default;
                    is_join_a_room = false;
                    return;
                }
                var completed = await ConnectServer(selectedRoom, userID, usrrname, roomname);
                if (completed)
                {
                    DoAnPaint.Form1 drawingpanel = new DoAnPaint.Form1(serverIP, connection, selectedRoom, usrrname);
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
                MessageBox.Show("Wait for the last request to be completed!");
                return;
            }
        }

        /// <summary>
        /// Kết nối tới server
        /// </summary>
        private async Task<bool> ConnectServer(int room, string userID, string usrname, string roomName)
        {
            connection = new HubConnectionBuilder()
                .WithUrl($"{serverAdd}?room={room}&userID={userID}&name={usrname}&roomName={roomName}", options =>
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

        //Tạo phòng mới
        private async void pictureBox2_Click(object sender, EventArgs e)
        {
            if (!is_join_a_room)
            {
                var codeForm = new createRoom();
                codeForm.ShowDialog();
                codeForm.StartPosition = FormStartPosition.CenterScreen;
                roomname = codeForm.RoomName;
                is_join_a_room = true;
                Cursor = Cursors.WaitCursor;
                selectedRoom = Convert.ToInt32(codeForm.RoomID);
                if (selectedRoom == -1)
                {
                    is_join_a_room = false;
                    Cursor = Cursors.Default;
                    return;
                }
                var completed = await ConnectServer(selectedRoom, userID, usrrname, roomname);
                if (completed)
                {
                    DoAnPaint.Form1 drawingpanel = new DoAnPaint.Form1(serverIP, connection, selectedRoom, usrrname);
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

        //Join phòng
        private async void pictureBox5_Click(object sender, EventArgs e)
        {
            if (!is_join_a_room)
            {
                is_join_a_room = true;
                Cursor = Cursors.WaitCursor;
                var codeForm = new enterCode();
                codeForm.StartPosition = FormStartPosition.CenterScreen;
                codeForm.ShowDialog();
                selectedRoom = codeForm.GetCode();
                roomname = codeForm.roomnaMe;
                if (selectedRoom == -1)
                {
                    is_join_a_room = false;
                    Cursor = Cursors.Default;
                    return;
                }
                var completed = await ConnectServer(selectedRoom, userID, usrrname, roomname);
                if (completed)
                {
                    DoAnPaint.Form1 drawingpanel = new DoAnPaint.Form1(serverIP, connection, selectedRoom, usrrname);
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
    }
}
