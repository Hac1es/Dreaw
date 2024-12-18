using DoAnPaint.Utils;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;

namespace DoAnPaint
{
    public partial class Form1
    {
        #region Supporters
        /// <summary>
        /// Chuyển sang dạng in hoa đầu
        /// </summary>
        private static string Capitalize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
        /// <summary>
        /// Convert System.Drawing.Color sang SKColor
        /// </summary>
        private static SKColor GetSKColor(Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }
        /// <summary>
        /// Convert SKColor sang System.Drawing.Color
        /// </summary>
        private static Color GetColor(SKColor color)
        {
            var tempcolor = Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
            return tempcolor;
        }
        /// <summary>
        /// Convert điểm của System.Drawing sang điểm của SkiaSharp
        /// </summary>
        private static SKPoint GetSKPoint(Point point)
        {
            return new SKPoint((int)(point.X), (int)(point.Y));
        }
        /// <param name="form">Form gọi ra noti này(this)</param>
        /// <param name="what">Thông báo gì: ok, error, warning, khác</param>
        /// <param name="msg">Tin nhắn cần hiện thị</param>
        /// <param name="flag">Có tự động đóng không?</param>
        private static void ShowNoti(Form form, string what, string msg, bool flag = true)
        {
            PopupNoti noti;
            noti = new PopupNoti(form, what, msg, flag);
            noti.StartPosition = FormStartPosition.Manual;
            noti.Location = noti.position;
            noti.Show();
        }
        /// <summary>
        /// Chỉnh cấu hình pen
        /// </summary>
        private void SetPen(ref SKPaint pen, DrawingData data)
        {
            pen.Color = (SKColor)data.color;
            pen.StrokeWidth = (int)data.width;
        }
        /// <summary>
        /// Chỉnh cấu hình crayon
        /// </summary>
        private void SetCrayon(ref SKPaint pen, DrawingData data)
        {
            pen.Shader = CrayonTexture((SKColor)data.color, (int)data.width);
            pen.StrokeWidth = (int)data.width * 4;
        }
        /// <summary>
        /// Chỉnh cấu hình eraser
        /// </summary>
        private void SetEraser(ref SKPaint pen, DrawingData data)
        {
            pen.Color = SKColors.White;
            pen.StrokeWidth = (int)data.width * 4;
        }
        /// <summary>
        /// Gọi PaintSurface
        /// </summary>
        private void RefreshCanvas(Command cmd)
        {
            if (ptbDrawing.InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    if (cmd == Command.PENCIL || cmd == Command.CRAYON || cmd == Command.ERASER)
                        ptbDrawing.Refresh();
                    else
                        ptbDrawing.Invalidate();
                }));
            }
            else
            {
                if(cmd == Command.PENCIL || cmd == Command.CRAYON || cmd == Command.ERASER)
                    ptbDrawing.Refresh();
                else
                    ptbDrawing.Invalidate();
            }    
        }
        /// <summary>
        /// Vẽ dữ liệu remote lên bitmap
        /// </summary>
        private void HandleDrawData(string msg, Command flag, SKCanvas canvas)
        {
            DrawingData data = JsonConvert.DeserializeObject<DrawingData>(msg);
            switch (flag)
            {
                case Command.PENCIL:
                    SetPen(ref pen, data);
                    canvas.DrawLine((SKPoint)data.PointX, (SKPoint)data.PointY, pen);
                    break;
                case Command.CRAYON:
                    SetCrayon(ref crayon, data);
                    canvas.DrawLine((SKPoint)data.PointX, (SKPoint)data.PointY, crayon);
                    break;
                case Command.ERASER:
                    SetEraser(ref pen, data);
                    canvas.DrawLine((SKPoint)data.PointX, (SKPoint)data.PointY, pen);
                    break;
                case Command.RECTANGLE:
                    SetPen(ref penenter, data);
                    canvas.DrawRect((int)data.startX, (int)data.startY, (int)data.endX, (int)data.endY, penenter);
                    break;
                case Command.LINE:
                    SetPen(ref pen, data);
                    canvas.DrawLine((int)data.startX, (int)data.startY, (int)data.endX, (int)data.endY, pen);
                    break;
                case Command.ELLIPSE:
                    SetPen(ref penenter, data);
                    canvas.DrawOval(new SKRect((float)data.startX, (float)data.startY, (float)data.endX, (float)data.endY), penenter);
                    break;
                case Command.POLYGON:
                    SetPen(ref penenter, data);
                    canvas.DrawPath(PolygonPath(data.Points), penenter);
                    break;
                case Command.CURVE:
                    SetPen(ref penenter, data);
                    canvas.DrawPath(CurvedPath(data.Points), penenter);
                    break;
                case Command.FILL:
                    canvas.DrawBitmap(data.Bitmap, 0, 0);
                    break;
                case Command.CLEAR:
                    if (data.startX.HasValue && data.startY.HasValue && data.endX.HasValue && data.endY.HasValue)
                        using (var brush = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.White, IsAntialias = true }) // Tạo bút vẽ màu trắng
                        {
                            canvas.DrawRect((int)data.startX, (int)data.startY, (int)data.endX, (int)data.endY, brush); // Fill màu trắng vào hình chữ nhật
                        }
                    else
                        gr.Clear(SKColors.White);
                    break;
                case Command.CURSOR:
                    var selectedd = new SKRect((float)data.startX, (float)data.startY, (float)data.endX, (float)data.endY);
                    canvas.DrawRect(selectedd, dotted_pen);
                    break;
            }
        }
        #endregion

        #region Fields
        private int RoomID;
        HubConnection connection; //Kết nối SignalR
        private readonly string serverIP;
        CancellationTokenSource cts_source = new CancellationTokenSource();
        private SKBitmap bmp; //Bitmap để vẽ
        private SKCanvas gr; //Graphic chính của Form vẽ
        private Command command; //Danh sách các lệnh(không dùng cái này, ta sẽ dùng property của nó)
        private SKColor colorr; //Màu(không dùng cái này, ta sẽ dùng property của nó)
        bool isPainting = false; //Có đang sử dụng tính năng không? 
        bool isDragging = false;
        bool isPreview = true;
        /* 
            List Points là hàng thật, thứ sẽ hiện thị lên canvas
            List TempPoints chỉ là preview, cập nhật liên tục theo vị trí chuột
         */
        List<SKPoint> Points = new List<SKPoint>();
        List<SKPoint> TempPoints = new List<SKPoint>();
        SKPoint pointX, pointY; //Dùng trong tính năng phải cập nhật vị trí liên tục(pen, eraser, crayon)
        int x, y, sX, sY, cX, cY;
        /* 
         x, y: cập nhật vị trí liên tục, dùng trong onPaint khi cần phải cho người dùng xem trước
        hình dạng (đường thẳng, hình chữ nhật, ..)
        sX, sY: (sizeX, sizeY) Kích thước của hình chữ nhật, hình tròn cần vẽ
        cX, cY: (currentX, currentY) Vị trị bắt đầu nhấn chuột xuống/Tọa độ bắt đầu của hình vẽ
         */
        int width = 2; //Độ dày khởi đầu nét bút
        SKRect selected = SKRect.Empty; //Khởi đầu cho vùng chọn, chưa chọn gì
        SemaphoreSlim canStart = new SemaphoreSlim(1);
        #region Sự kiện khi Color thay đổi
        // Property của colorr
        // Sự kiện xảy ra khi Color thay đổi
        public event Action<SKColor> ColorChanged; //event được kích khi color thay đổi
        private SKColor color //Sử dụng properties này để kiểm soát
        {
            get => colorr;
            set
            {
                if (colorr != value)
                {
                    colorr = value;
                    ColorChanged?.Invoke(colorr); // Gọi sự kiện khi giá trị thay đổi
                }
            }
        }
        #endregion //Sự kiện được đăng kí trong Constructor
        #region Sự kiện khi Command thay đổi
        List<Control> controls = new List<Control>();
        // Property với command
        // Sự kiện xảy ra khi Color thay đổi
        public event Action<Command> CommandChanged;
        private Command Cmd
        {
            get => command;
            set
            {
                //không cho phép đổi control khi chưa vẽ xong
                if (isPainting)
                {
                    ShowNoti(this, "warning", "Complete current action first!");
                    return;
                }
                if (command != value)
                {
                    command = value;
                    if (value != Command.CURSOR && value != Command.OCR)
                    {
                        selected = SKRect.Empty;
                        Status.Text = Capitalize(value.ToString());
                    }
                    else if (value == Command.OCR)
                        Status.Text = value.ToString();
                    else
                        Status.Text = Capitalize(value.ToString());
                    CommandChanged?.Invoke(value);
                }
            }
        }
        #endregion
        BlockingCollection<(string, Command, bool)> BOTQueue = new BlockingCollection<(string, Command, bool)>(); //Queue data vẽ
        BlockingCollection<(string, bool, string)> MSGQueue = new BlockingCollection<(string, bool, string)>();
        SKPaint pen = new SKPaint { IsAntialias = true }; //Bút chì
        SKPaint penenter = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke }; //Bút vẽ hình
        SKPaint crayon = new SKPaint { IsAntialias = true }; //Sáp màu
        SKPaint dotted_pen = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true, PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0) }; //Bút dùng trong chế độ chọn
        (string, Command) tempData = (null, Command.CURSOR); //Dữ liệu tạm
        string userName; //Tên người dùng
        #endregion

        #region Draw Methods
        /// <summary>
        /// Convert điểm trên pictureBox sang điểm trên Bitmap
        /// </summary>
        /// <param name="point">Truyền điểm vào để convert</param>
        private void Validate(SKBitmap bitmap, Stack<SKPoint> ptStack, float x, float y, SKColor b4, SKColor after)
        {
            //Tìm biên giới
            SKColor current = bitmap.GetPixel((int)x, (int)y);
            if (current == b4)
            {
                ptStack.Push(new SKPoint(x, y));
                bitmap.SetPixel((int)x, (int)y, after);
            }
        }
        public void FillUp(SKBitmap bitmap, int x, int y, SKColor New)
        {
            SKColor Old = bitmap.GetPixel(x, y);
            Stack<SKPoint> ptStack = new Stack<SKPoint>();
            ptStack.Push(new SKPoint(x, y));
            bitmap.SetPixel(x, y, New);
            if (Old == New) return;
            while (ptStack.Count > 0)
            {
                SKPoint pt = (SKPoint)ptStack.Pop();
                if (pt.X > 0 && pt.Y > 0 && pt.X < bitmap.Width - 1 && pt.Y < bitmap.Height - 1)
                {
                    Validate(bitmap, ptStack, pt.X - 1, pt.Y, Old, New);
                    Validate(bitmap, ptStack, pt.X, pt.Y - 1, Old, New);
                    Validate(bitmap, ptStack, pt.X + 1, pt.Y, Old, New);
                    Validate(bitmap, ptStack, pt.X, pt.Y + 1, Old, New);
                }
            }
        }
        /// <summary>
        /// Fill màu sử dụng DFS
        /// </summary>
        public Task<SKBitmap> FillUpAsync(SKBitmap bitmap, int x, int y, SKColor New)
        {
            return Task.Run(() =>
            {
                // Tạo một bitmap phụ có cùng kích thước với bitmap chính
                SKBitmap bufferBitmap = new SKBitmap(bmp.Width, bmp.Height);
                using (SKCanvas canvas = new SKCanvas(bufferBitmap))
                {
                    // Vẽ nội dung của bitmap chính lên bitmap phụ
                    canvas.DrawBitmap(bmp, 0, 0);
                }
                FillUp(bufferBitmap, x, y, New);
                return bufferBitmap; 
            });
        }
        /// <summary>
        /// Tạo ra đường cong để sử dụng sau
        /// </summary>
        public SKPath CurvedPath(List<SKPoint> points)
        {
            var path = new SKPath();

            if (points.Count < 2)
                return path;

            path.MoveTo(points[0]);

            for (int i = 1; i < points.Count - 1; i++)
            {
                SKPoint mid = new SKPoint(
                    (points[i].X + points[i + 1].X) / 2,
                    (points[i].Y + points[i + 1].Y) / 2
                );
                path.QuadTo(points[i], mid); // Sử dụng QuadTo thay vì ConicTo
            }

            path.LineTo(points.Last());
            return path;
        }
        /// <summary>
        /// Tạo ra đường đi của polygon(đa giác)
        /// </summary>
        public SKPath PolygonPath(List<SKPoint> points)
        {
            SKPath path = new SKPath();
            path.MoveTo(points[0]);
            if (points.Count < 2)
            {
                return path;
            }
            else
            {
                for (int i = 1; i < points.Count; i++)
                {
                    path.LineTo(points[i]);
                }
            }
            path.Close();
            return path;
        }
        /// <summary>
        /// Tạo ra Texture giả lập bút chì màu(Crayon)
        /// </summary>
        public SKShader CrayonTexture(SKColor color, int width)
        {
            int grainDensity = width * 50; //mật độ của các hạt màu
            int textureSize = width * 4; //Kích thước của texture
            SKBitmap texture = new SKBitmap(textureSize, textureSize);
            Random random = new Random();
            //Tạo ra các hạt mực ngẫu nhiên trên bề mặt của texture
            for (int i = 0; i < grainDensity; i++)
            {
                int x = random.Next(textureSize);
                int y = random.Next(textureSize);

                int alpha = random.Next(100, 200); // Độ trong suốt ngẫu nhiên
                SKColor grainColor = new SKColor(color.Red, color.Green, color.Blue, (byte)alpha);

                texture.SetPixel(x, y, grainColor); //Tạo ra hạt mực với vị trí ngẫu nhiên
                                                    //+ độ trong suốt ngầu nhiên + màu do người dùng chọn
            }
            // Tạo shader từ bitmap với chế độ lặp lại
            return SKShader.CreateBitmap(texture, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
        }
        #endregion

        #region Message Methods
        // Hàm hỗ trợ để thêm văn bản với màu sắc
        private void AppenddText(RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor; // Trở về màu mặc định
        }
        private void ShowMsg(string msg, bool where, string who)
        {
            if (!where && !chatPanel.Visible) 
                ShowNoti(this, "New Message!", $"{who}: {msg}");
            MessageBox.Invoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(who))
                {
                    AppenddText(MessageBox, $"{msg}{Environment.NewLine}", Color.Blue);
                }    
                else if (where)
                {
                    AppenddText(MessageBox, "You: ", Color.Indigo);
                    AppenddText(MessageBox, $"{msg}{Environment.NewLine}", MessageBox.ForeColor);
                }
                else
                {
                    AppenddText(MessageBox, $"{who}: ", Color.SteelBlue);
                    AppenddText(MessageBox, $"{msg}{Environment.NewLine}", MessageBox.ForeColor);
                }
            }));
        }
        #endregion

        #region ServerMethods
        /// <summary>
        /// Gửi dữ liệu vẽ
        /// </summary>
        /// <param name="data">Dữ liệu vẽ được JSONify</param>
        /// <param name="command">Lệnh vẽ</param>
        /// <param name="isPreview">Nét vẽ thật hay preview</param>
        private async void SendData(string data, Command command, bool isPreview)
        {
            await connection.InvokeAsync("BroadcastDraw", data, command, isPreview);
        }

        private async void SendMsg(string msg, string name)
        {
            await connection.InvokeAsync("BroadcastMsg", msg, name);
        }

        /// <summary>
        /// Lắng nghe tín hiệu
        /// </summary>
        private void ListenForSignal()
        {
            connection.On<string, Command, bool>("HandleDrawSignal", (dataa, commandd, isPrevieww) => 
            {
                BOTQueue.Add((dataa, commandd, isPrevieww));
            });
            connection.On<string, string>("HandleMessage", (mes, who) =>
            {
                MSGQueue.Add((mes, false, who));
            });
        }

        /// <summary>
        /// Consumer cho luồng vẽ
        /// </summary>
        private void DrawConsumer()
        {
            foreach (var item in BOTQueue.GetConsumingEnumerable())
            {
                canStart.Wait(0);
                var (dataa, commandd, isPrevieww) = item;
                isPreview = isPrevieww;
                tempData = (dataa, commandd);
                RefreshCanvas(commandd);
            }
        }

        /// <summary>
        /// Consumer cho luồng tin nhắn
        /// </summary>
        private void MsgConsumer()
        {
            foreach (var item in MSGQueue.GetConsumingEnumerable())
            {
                canStart.Wait(0);
                var (msg, where, who) = item;
                ShowMsg(msg, where, who);
            }
        }

        private async void GetPing(CancellationToken cts)
        {
            Ping ping = new Ping();
            var host = serverIP;
            while (!cts.IsCancellationRequested)
            {
                var reply = ping.Send(host);
                label6.BeginInvoke(new Action(() =>
                {
                    label6.Text = $"Ping: {reply.RoundtripTime} ms";
                }));
                await Task.Delay(3000);
            }    
        }
        #endregion
    }
}
