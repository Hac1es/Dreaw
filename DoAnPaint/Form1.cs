using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using SkiaSharp;

using DoAnPaint.Utils;
using System.IO;
using Application = System.Windows.Forms.Application;
using Sprache;
using TrackBar = System.Windows.Forms.TrackBar;
using Color = System.Drawing.Color;
using TheArtOfDevHtmlRenderer.Adapters.Entities;
using static Guna.UI2.Native.WinApi;
using TheArtOfDevHtmlRenderer.Adapters;
using System.Reflection;
using SkiaSharp.Views.Desktop;
using System.Web;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR.Client;
using System.Web.UI.WebControls.WebParts;
using System.Windows.Markup;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json.Linq;

namespace DoAnPaint
{
    public partial class Form1 : Form
    {
        public Form1(string serverip,  int Roomid, string userName, SKBitmap btmap = null)
        {
            InitializeComponent();
            serverIP = serverip;
            RoomID = Roomid;
            if (btmap == null)
                bmp = new SKBitmap(ptbDrawing.Width, ptbDrawing.Height);
            else
                bmp = btmap;
            gr = new SKCanvas(bmp);
            this.userName = userName;
            #region Linh tinh
            /*Toàn bộ mọi thứ ở đây là liên quan tới UI
             * Logic: Nó làm 2 thứ:
            -Khi màu đổi: Đổi màu trên UI
            -Khi Command đổi: Đổi Control đang được lựa chọn trên UI + Đổi cái Scroll chọn giá trị
            */
            controls.AddRange(new Control[] { btnPen, btnCrayon, btnEraser, btnBezier, btnLine, btnRectangle, btnEllipse, btnPolygon, btnSelect, btnOCR, btnFill });
            // Đăng ký sự kiện thay đổi màu
            ColorChanged += newColor => ptbColor.BackColor = GetColor(newColor);

            // Đặt màu khởi tạo
            color = GetSKColor(Color.Black); // Thay đổi Color, Panel sẽ đổi màu

            // Đăng ký sự kiện thay đổi lệnh
            CommandChanged += (cmd) =>
            {
                controls.ForEach(ctrl => ctrl.BackColor = Color.Transparent);
                // Chọn Control nào, Control đó sẽ đổi màu
                switch (cmd)
                {
                    case Command.PENCIL:
                        btnPen.BackColor = Color.PaleTurquoise;
                        break;
                    case Command.ERASER:
                        btnEraser.BackColor = Color.PaleTurquoise;
                        break;
                    case Command.CRAYON:
                        btnCrayon.BackColor = Color.PaleTurquoise;
                        break;
                    case Command.LINE:
                        btnLine.BackColor = Color.PaleTurquoise;
                        break;
                    case Command.POLYGON:
                        btnPolygon.BackColor = Color.PaleTurquoise;
                        break;
                    case Command.CURVE:
                        btnBezier.BackColor = Color.PaleTurquoise;
                        break;
                    case Command.RECTANGLE:
                        btnRectangle.BackColor = Color.PaleTurquoise;
                        break;
                    case Command.ELLIPSE:
                        btnEllipse.BackColor = Color.PaleTurquoise;
                        break;
                    case Command.CURSOR:
                        btnSelect.BackColor = Color.PaleTurquoise;
                        break;
                    case Command.OCR:
                        btnOCR.BackColor = Color.PaleTurquoise;
                        break;
                    case Command.FILL:
                        btnFill.BackColor = Color.PaleTurquoise;
                        break;
                }
            };
            // Đăng ký sự kiện thay đổi lệnh
            CommandChanged += (cmd) => 
            { 
                if (cmd == Command.CRAYON || cmd == Command.ERASER) //Crayon và Eraser dùng một thanh chọn cỡ khác
                {
                    btnLineSize.Minimum = 2;
                    btnLineSize.Maximum = 10;
                    btnLineSize.TickFrequency = 2;
                    btnLineSize.SmallChange = 2;
                    btnLineSize.LargeChange = 2;
                    btnLineSize.Value = this.width = 4;
                    Tips.SetToolTip(btnLineSize, $"Pen/Border size: {btnLineSize.Value}");
                }
                else //Đám còn lại dùng một thanh chọn cỡ khác
                {
                    btnLineSize.Minimum = 1;
                    btnLineSize.Maximum = 10;
                    btnLineSize.TickFrequency = 1;
                    btnLineSize.SmallChange = 1;
                    btnLineSize.LargeChange = 1;
                    btnLineSize.Value = this.width = 2;
                    Tips.SetToolTip(btnLineSize, $"Pen/Border size: {btnLineSize.Value}");
                }
                btnLineSize.ResumeLayout();
            };
            // Đặt lệnh khởi tạo
            Cmd = Command.CURSOR;
            #endregion
        }

        public void SetConn(HubConnection conn)
        {
            connection = conn;
        }

        //Chế độ Line
        private void btnLine_Click(object sender, EventArgs e)
        {
            setCursor(Cursorr.NONE);
            Cmd = Command.LINE;
        }

        //Chế độ không làm gì cả
        private void btnSelect_Click(object sender, EventArgs e)
        {
            Cmd = Command.CURSOR;
            selected = SKRect.Empty;
            setCursor(Cursorr.NONE);
        }

        /// <summary>
        /// Custom hình dạng con trỏ chuột
        /// </summary>
        /// <param name="cursor">Chế độ con trỏ chuột</param>
        public void setCursor(Cursorr cursor)
        {
            string template = @"..\..\..\DoAnPaint\Resources\{0}";
            string where;
            switch (cursor)
            {
                case Cursorr.PENCIL:
                    where = Cmd == Command.PENCIL ? string.Format(template, "Pencil.png") : "-1";
                    break;
                case Cursorr.ERASER:
                    where = Cmd == Command.ERASER ? string.Format(template, "Eraser.png") : "-1";
                    break;
                case Cursorr.FILL:
                    where = Cmd == Command.FILL ? string.Format(template, "Fill Color.png") : "-1";
                    break;
                case Cursorr.CRAYON:
                    where = Cmd == Command.CRAYON ? string.Format(template, "CrayonCursor.png") : "-1";
                    break;
                default:
                    where = null;
                    break;
            }
            if (where != null)
            {
                if (where == "-1") return;
                using (Bitmap bitmap = new Bitmap(where))
                {
                    IntPtr hIcon = bitmap.GetHicon();
                    Icon icon = Icon.FromHandle(hIcon);
                    ptbDrawing.Cursor = new Cursor(icon.Handle);
                }
            }
            else ptbDrawing.Cursor = Cursors.NoMove2D;
        }

        private void btnRectangle_Click(object sender, EventArgs e)
        {
            Cmd = Command.RECTANGLE;
            setCursor(Cursorr.NONE);
        }

        //Chế độ Ellipse
        private void btnEllipse_Click(object sender, EventArgs e)
        {
            Cmd = Command.ELLIPSE;
            setCursor(Cursorr.NONE);
        }

        //Chế độ vẽ đường cong Bezier
        private void btnBezier_Click(object sender, EventArgs e)
        {
            Cmd = Command.CURVE;
            setCursor(Cursorr.NONE);
        }

        // Chế độ vẽ đa giác
        private void btnPolygon_Click(object sender, EventArgs e)
        {
            Cmd = Command.POLYGON;
            setCursor(Cursorr.NONE);
        }

        //Shin cậu bé bút chì
        private void btnPen_Click(object sender, EventArgs e)
        {
            Cmd = Command.PENCIL;
            setCursor(Cursorr.PENCIL);
        }

        //Gôm
        private void btnEraser_Click(object sender, EventArgs e)
        {
            Cmd = Command.ERASER;
            setCursor(Cursorr.ERASER);
        }

        //Custom màu
        private void ptbEditColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                color = GetSKColor(colorDialog.Color);  
            }
        }

        //Chọn độ dày nét vẽ
        private void btnLineSize_Scroll(object sender, EventArgs e)
        {
            TrackBar trackBar = sender as TrackBar;
            if (Cmd == Command.CRAYON || Cmd == Command.ERASER) //Nếu dùng crayon hay eraser
            {
                // Nếu giá trị là lẻ, cộng thêm 1 để làm tròn
                if (trackBar.Value % 2 != 0)
                {
                    trackBar.Value += 1;
                }
            }
            this.width = btnLineSize.Value;
            Tips.Show($"Pen/Border size: {btnLineSize.Value}", btnLineSize);
        }

        //Chọn màu
        private void btnChangeColor_Click(object sender, EventArgs e)
        {
            PictureBox ptb = sender as PictureBox;
            color = GetSKColor(ptb.BackColor);
        }

        //Xóa vùng được chọn
        private void btnClear_Click(object sender, EventArgs e)
        {
            if (selected != SKRect.Empty) //Nếu đã chọn vùng
            {
                var data = new DrawingData(null, null, null, null, (int)selected.Left, (int)selected.Top, (int)selected.Right, (int)selected.Bottom);
                string msg = JsonConvert.SerializeObject(data);
                BOTQueue.Add((msg, Command.CLEAR, false));
                SendData(msg, Command.CLEAR, false);
            }
            else //Xóa hết
            {
                var data = new DrawingData();
                string msg = JsonConvert.SerializeObject(data);
                BOTQueue.Add((msg, Command.CLEAR, false));
                SendData(msg, Command.CLEAR, false);
            }
            selected = SKRect.Empty;
        }

        //Fill màu
        private void btnFill_Click(object sender, EventArgs e)
        {
            Cmd = Command.FILL;
            setCursor(Cursorr.FILL);
        }

        //Ma thuật đen(Đọc chữ)
        private async void btnOCR_Click(object sender, EventArgs e)
        {
            setCursor(Cursorr.NONE);
            Cmd = Command.OCR;
            if (selected == SKRect.Empty) {
                ShowNoti(this, "warning", "You haven't selected anything!");
                Cmd = Command.CURSOR;
                return;
            }
            if (isPainting)
            {
                ShowNoti(this, "warning", "Completed last action first!");
                return;
            }
            isPainting = true;
            const string serverAdd = "https://localhost:7183";
            SKBitmap croped = new SKBitmap();
            bmp.ExtractSubset(croped, new SKRectI((int)selected.Left, (int)selected.Top, (int)selected.Right, (int)selected.Bottom));
            var content = new MultipartFormDataContent();
            SKImage image = SKImage.FromPixels(croped.PeekPixels());
            SKData encoded = image.Encode(SKEncodedImageFormat.Jpeg, 100); // Đảm bảo định dạng JPEG
            Stream stream = encoded.AsStream();

            var fileUpload = new StreamContent(stream);
            fileUpload.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // Gắn MIME type chính xác
            content.Add(fileUpload, "file", "ocrCropped.jpg"); // Tên file với phần mở rộng rõ ràng

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"{serverAdd}/ocr", content);
                if (response.IsSuccessStatusCode)
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
                else if (response.StatusCode == (HttpStatusCode)422)
                {
                    MessageBox.Show("Chữ mày xấu vãi cả lồn");
                }
                else
                {
                    MessageBox.Show("Lỗi server");
                }    
            }
            isPainting = false;
            Cmd = Command.CURSOR;
        }

        //Sự kiện ấn chuột xuống
        private void ptbDrawing_MouseDown(object sender, MouseEventArgs e)
        {
            pointX = GetSKPoint(e.Location);
            cX = e.X;
            cY = e.Y;
            if (Cmd != Command.CURVE && Cmd != Command.CURSOR && Cmd != Command.POLYGON)
            {
                isPainting = true;
            }
            else if (Cmd == Command.CURSOR)
            {
                isDragging = true;
                isPainting = false;
                selected = SKRect.Empty;
            }
        }

        //Sự kiện di chuyển chuột
        private void ptbDrawing_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isPainting && !isDragging)
            {
                lbLocation.Text = $"{e.X}, {e.Y} px";
                return;
            }
            lbLocation.Text = $"{e.X}, {e.Y} px";
            if (Cmd == Command.PENCIL || Cmd == Command.CRAYON || Cmd == Command.ERASER)
            {
                pointY = GetSKPoint(e.Location);
                var data = new DrawingData(color, width, pointX, pointY);
                string msg = JsonConvert.SerializeObject(data);
                BOTQueue.Add((msg, Cmd, false));
                SendData(msg, Cmd, false);
                pointX = pointY;
            }
            if (Cmd == Command.CURVE || Cmd == Command.POLYGON)
            {
                if (TempPoints.Count > Points.Count)
                {
                    TempPoints[TempPoints.Count - 1] = GetSKPoint(e.Location);                    
                }
                else
                {
                    TempPoints.Add(GetSKPoint(e.Location));
                }
                var data = new DrawingData(color, width, null, null, null, null, null, null, TempPoints);
                string msg = JsonConvert.SerializeObject(data);
                BOTQueue.Add((msg, Cmd, true));
                SendData(msg, Cmd, true);
            }
            x = e.X;
            y = e.Y;
            sX = Math.Abs(e.X - cX);
            sY = Math.Abs(e.Y - cY);
            if (Cmd == Command.LINE)
            {
                var data = new DrawingData(color, width, null, null, cX, cY, x, y);
                string msg = JsonConvert.SerializeObject(data);
                BOTQueue.Add((msg, Cmd, true));
                SendData(msg, Cmd, true);
            }
            if (Cmd == Command.RECTANGLE)
            {
                var data = new DrawingData(color, width, null, null, Math.Min(cX, x), Math.Min(cY, y), sX, sY);
                string msg = JsonConvert.SerializeObject(data);
                BOTQueue.Add((msg, Cmd, true));
                SendData(msg, Cmd, true);
            } 
            if (Cmd == Command.ELLIPSE)
            {
                var data = new DrawingData(color, width, null, null, Math.Min(cX, x), Math.Min(cY, y), Math.Min(cX, x) + sX, Math.Min(cY, y) + sY);
                string msg = JsonConvert.SerializeObject(data);
                BOTQueue.Add((msg, Cmd, true));
                SendData(msg, Cmd, true);
            }    
            if (Cmd == Command.CURSOR && isDragging == true)
            {
                selected = new SKRect(Math.Min(cX, x), Math.Min(cY, y), Math.Min(cX, x) + sX, Math.Min(cY, y) + sY);
                Status.Text = $"Selected: ({selected.Left}, {selected.Top}), ({selected.Right}, {selected.Bottom})";
                var data = new DrawingData(null, null, null, null, (int)selected.Left, (int)selected.Top, (int)selected.Right, (int)selected.Bottom);
                string msg = JsonConvert.SerializeObject(data);
                BOTQueue.Add((msg, Cmd, true));
            }
        }

        //Sự kiện thả chuột
        private void ptbDrawing_MouseUp(object sender, MouseEventArgs e)
        {
            if (Cmd != Command.CURVE && Cmd != Command.POLYGON) isPainting = false;
            if (Cmd == Command.CURSOR && isDragging == true)
            {
                isDragging = false;
                var data = new DrawingData(null, null, null, null, 0, 0, 0, 0);
                string msg = JsonConvert.SerializeObject(data);
                BOTQueue.Add((msg, Cmd, true));
            } 
                
            if (Cmd == Command.LINE)
            {
                var data = new DrawingData(color, width, null, null, cX, cY, x, y);
                string msg = JsonConvert.SerializeObject(data);
                BOTQueue.Add((msg, Cmd, false));
                SendData(msg, Cmd, false);
            }
            if (Cmd == Command.RECTANGLE)
            {
                var data = new DrawingData(color, width, null, null, Math.Min(cX, x), Math.Min(cY, y), sX, sY);
                string msg = JsonConvert.SerializeObject(data);
                BOTQueue.Add((msg, Cmd, false));
                SendData(msg, Cmd, false);
            }
            if (Cmd == Command.ELLIPSE)
            {
                var data = new DrawingData(color, width, null, null, Math.Min(cX, x), Math.Min(cY, y), Math.Min(cX, x) + sX, Math.Min(cY, y) + sY);
                string msg = JsonConvert.SerializeObject(data);
                BOTQueue.Add((msg, Cmd, false));
                SendData(msg, Cmd, false);
            }
        }

        //Sự kiện tô lên bề mặt ptbDrawing(được gọi khi Invalidate)
        private void ptbDrawing_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas render_canvas = e.Surface.Canvas;
            render_canvas.Clear(SKColors.White); // Xóa nền trước khi vẽ
            var (currentData, currentFlag) = tempData;           
            if (!isPreview && currentData != null)
            {
                HandleDrawData(currentData, currentFlag, gr);
                render_canvas.DrawBitmap(bmp, 0, 0);
            }
            else if (currentData != null)
            {
                render_canvas.DrawBitmap(bmp, 0, 0);
                HandleDrawData(currentData, currentFlag, render_canvas);
            }
            else
                render_canvas.DrawBitmap(bmp, 0, 0);
        }
        //Sự kiện Click chuột
        private async void ptbDrawing_MouseClick_1(object sender, MouseEventArgs e)
        {
            SKPoint point = GetSKPoint(e.Location);
            if (Cmd == Command.FILL)
            {
                var filled_bmp = await FillUpAsync(bmp, (int)point.X, (int)point.Y, color);
                var data = new DrawingData(null, null, null, null, null, null, null, null, null, filled_bmp);
                var msg = JsonConvert.SerializeObject(data);
                BOTQueue.Add((msg, Command.FILL, false));
                SendData(msg, Command.FILL, false);
            }
            if (Cmd == Command.CURVE || Cmd == Command.POLYGON)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (!isPainting)
                    {
                        isPainting = true;
                        Points.Add(GetSKPoint(e.Location));
                        TempPoints = Points.ToList();
                    }
                    else
                    {
                        Points.Add(GetSKPoint(e.Location));
                        TempPoints = Points.ToList();
                    }
                }
                else if (Points.Any() && isPainting)
                {
                    Points.Add(GetSKPoint(e.Location));
                    if (Points.Count < 3)
                    {
                        ShowNoti(this, "error", "You need at least 3 points!");
                        Points.Clear();
                        TempPoints.Clear();
                        isPainting = false;
                    }
                    else
                    {
                        var data = new DrawingData(color, width, null, null, null, null, null, null, Points);
                        string msg = JsonConvert.SerializeObject(data);
                        BOTQueue.Add((msg, Cmd, false));
                        SendData(msg, Cmd, false);
                        Points.Clear();
                        TempPoints.Clear();
                        isPainting = false;
                    }
                }
            }
        }

        //DoubleClick để lấy màu tại ví trí chuột
        private void ptbDrawing_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {
            if (Cmd != Command.CURSOR) return;
            SKColor pixelColor = bmp.GetPixel(e.X, e.Y);
            color = pixelColor == new SKColor(0, 0, 0, 0) ? SKColors.White : pixelColor;
        }

        //Crayon Shin-chan
        private void btnCrayon_Click(object sender, EventArgs e)
        {
            Cmd = Command.CRAYON;
            setCursor(Cursorr.CRAYON);
        }

        //Này không quan trọng lắm kệ đi
        private void ptbColor_MouseEnter(object sender, EventArgs e)
        {
            Tips.SetToolTip(ptbColor, $"{ptbColor.BackColor}");
        }

        //Lưu bức vẽ về máy
        private void btnSave_Click(object sender, EventArgs e)
        {
            var save = new SaveFileDialog();
            save.Filter = "Image(*.png) |*.png|(*.*)|*.*";
            save.Title = "Save Image";
            save.FileName = "Paint.jpeg";
            if (save.ShowDialog() == DialogResult.OK)
            {
                // Lấy đường dẫn tệp người dùng chọn
                string filePath = save.FileName;

                try
                {
                    StopConsumers();
                    using (var image = SKImage.FromBitmap(bmp))
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 80))
                    {
                        // save the data to a stream
                        using (var stream = File.OpenWrite(filePath))
                        {
                            data.SaveTo(stream);
                        }
                    }
                    MessageBox.Show("File saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    StartConsumers();
                }
            }
        }

        //Logout
        private async void button1_Click(object sender, EventArgs e)
        {
            await connection.StopAsync();
            cts_source.Cancel();
            this.Dispose();
        }

        //Không quan trọng
        private void chatPanel_Paint(object sender, PaintEventArgs e)
        {
            // Lấy Graphics object để vẽ
            Graphics g = e.Graphics;

            // Khởi tạo vùng rectangle của panel
            Rectangle rect = chatPanel.ClientRectangle;

            // Tạo SolidBrush để vẽ
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, Color.Black)))
            {
                // Vẽ gradient lên toàn bộ panel
                g.FillRectangle(brush, rect);
            }
        }

        //Mở chat
        //Shift + Enter để mở chat
        //Enter để gửi chat
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Xử lý Shift + Enter
            if (keyData == (Keys.Shift | Keys.Enter))
            {
                chatPanel.Visible = !chatPanel.Visible;
                msgBox.Focus();
                return true; // Chặn xử lý tiếp theo
            }

            // Xử lý Enter khi chatPanel hiển thị
            if (keyData == Keys.Enter && chatPanel.Visible)
            {
                MSGQueue.Add((msgBox.Text, true, ""));
                SendMsg(msgBox.Text, userName);
                msgBox.Clear();
                return true; // Chặn xử lý tiếp theo
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RoomIDShow.Text = $"Room ID: {RoomID}";
            gr.Clear(SKColors.White);
            ptbDrawing.Invalidate();
            var token = cts_source.Token;
            _ = Task.Run(() => GetPing(token), token);
            _ = Task.Run(() => DrawConsumer());
            _ = Task.Run(() => MsgConsumer());
            _ = Task.Run(() => ListenForSignal());
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cts_source.Cancel();
        }

        private async void pictureBox21_Click(object sender, EventArgs e)
        {
            string stringtoSend;
            using (var image = bmp.Encode(SKEncodedImageFormat.Png, 100))
            {
                stringtoSend = Convert.ToBase64String(image.ToArray());
            }
            var BlockUI = new UnclosableNoti("Waiting....", "Saving this Bitmap");
            BlockUI.Show();
            StopConsumers();
            await connection.InvokeAsync("SaveBitmap", stringtoSend);
            BlockUI.Close();
            StartConsumers();
        }
    }
}
