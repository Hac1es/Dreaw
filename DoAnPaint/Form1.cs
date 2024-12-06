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
using Point = System.Drawing.Point;
using Application = System.Windows.Forms.Application;
using Sprache;
using TrackBar = System.Windows.Forms.TrackBar;
using MessageBox = System.Windows.Forms.MessageBox;
using Color = System.Drawing.Color;
using TheArtOfDevHtmlRenderer.Adapters.Entities;
using static Guna.UI2.Native.WinApi;
using TheArtOfDevHtmlRenderer.Adapters;
using System.Reflection;
using SkiaSharp.Views.Desktop;
using System.Web;
using System.Threading;
using System.Runtime.CompilerServices;

namespace DoAnPaint
{
    public partial class Form1 : Form
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
            if (flag == true)
                noti = new PopupNoti(form, what, msg);
            else
                noti = new PopupNoti(form, what, msg, false);
            noti.StartPosition = FormStartPosition.Manual;
            noti.Location = noti.position;
            noti.Show();
        }
        #endregion

        #region Fields
        private SKBitmap bmp; //Bitmap để vẽ
        private SKCanvas gr; //Graphic chính của Form vẽ
        private Command command; //Danh sách các lệnh(không dùng cái này, ta sẽ dùng property của nó)
        private SKColor colorr; //Màu(không dùng cái này, ta sẽ dùng property của nó)
        bool isPainting = false; //Có đang sử dụng tính năng không? 
        bool isDragging = false;
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
        #endregion

        #region DrawMethods
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
        /// <summary>
        /// Fill màu sử dụng DFS
        /// </summary>
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

        public Form1()
        {
            InitializeComponent();
            bmp = new SKBitmap(ptbDrawing.Width, ptbDrawing.Height);
            gr = new SKCanvas(bmp);
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

        public Form1(SKBitmap remoteBmp) //Constructor remote, trong trường hợp lấy phòng từ CSDL
        {
            InitializeComponent();
            bmp = remoteBmp;
            gr = new SKCanvas(bmp);
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
            string template = @"..\..\Resources\{0}";
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
                using (var brush = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.White, IsAntialias = true}) // Tạo bút vẽ màu trắng
                {
                    gr.DrawRect(selected, brush); // Fill màu trắng vào hình chữ nhật
                }
            }
            else //Xóa hết
            {
                gr.Clear(SKColors.White);
            }
            ptbDrawing.Invalidate();
        }

        //Fill màu
        private void btnFill_Click(object sender, EventArgs e)
        {
            Cmd = Command.FILL;
            setCursor(Cursorr.FILL);
        }

        //Ma thuật đen(Đọc chữ)
        private void btnOCR_Click(object sender, EventArgs e)
        {
            setCursor(Cursorr.NONE);
            Cmd = Command.OCR;
            if (selected == SKRect.Empty) {
                ShowNoti(this, "warning", "You haven't selected anything!");
                Cmd = Command.CURSOR;
                return;
            }
            SKBitmap croped = new SKBitmap();
            bmp.ExtractSubset(croped, new SKRectI((int)selected.Left, (int)selected.Top, (int)selected.Right, (int)selected.Bottom));
            ShowNoti(this, "Pending...", "Sending request to server...", false);
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
                var data = new DrawingData(null, null, true, pointX, null, e.X, e.Y, null, null, null, null);
            }
            else if (Cmd == Command.CURSOR)
            {
                isDragging = true;
                isPainting = false;
                selected = SKRect.Empty;
            }
            //Send(data, COMMAND.START);
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
            if (Cmd == Command.PENCIL)
            {
                pointY = GetSKPoint(e.Location);
                using (var pen = new SKPaint { Color = color, StrokeWidth = width, IsAntialias = true })
                    gr.DrawLine(pointX, pointY, pen);
                var data = new DrawingData(color, width, true, pointX, pointY, null, null, null, null, null, null);
                pointX = pointY;
                //Send(data, COMMAND.PENCIL);
            }
            if (Cmd == Command.CRAYON)
            {
                pointY = GetSKPoint(e.Location);
                using (var pen = new SKPaint { Shader = CrayonTexture(color, width), StrokeWidth = width * 4, IsAntialias = true })
                    gr.DrawLine(pointX, pointY, pen);
                var data = new DrawingData(color, width, true, pointX, pointY, null, null, null, null, null, null);
                pointX = pointY;
                //Send(data, COMMAND.CRAYON);
            }
            if (Cmd == Command.ERASER)
            {
                pointY = GetSKPoint(e.Location);
                using (var pen = new SKPaint { Color = SKColors.White, StrokeWidth = width * 4, IsAntialias = true })
                    gr.DrawLine(pointX, pointY, pen);
                var data = new DrawingData(color, width, true, pointX, pointY, null, null, null, null, null, null);
                pointX = pointY;
                //Send(data, COMMAND.CRAYON);
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
            }
            x = e.X;
            y = e.Y;
            sX = Math.Abs(e.X - cX);
            sY = Math.Abs(e.Y - cY);
            if (Cmd == Command.CURSOR && isDragging == true)
            {
                selected = new SKRect(Math.Min(cX, x), Math.Min(cY, y), Math.Min(cX, x) + sX, Math.Min(cY, y) + sY);
            }
            ptbDrawing.Invalidate();
        }
        //Sự kiện thả chuột

        private void ptbDrawing_MouseUp(object sender, MouseEventArgs e)
        {
            if (Cmd != Command.CURVE && Cmd != Command.POLYGON) isPainting = false;
            if (Cmd == Command.CURSOR) isDragging = false;
            sX = Math.Abs(e.X - cX);
            sY = Math.Abs(e.Y - cY);
            using (var pen = new SKPaint { Color = color, Style = SKPaintStyle.Stroke, StrokeWidth = width, IsAntialias = true })
            {
                if (Cmd == Command.LINE)
                {
                    gr.DrawLine(cX, cY, x, y, pen);
                }
                if (Cmd == Command.RECTANGLE)
                {
                    gr.DrawRect(Math.Min(cX, x), Math.Min(cY, y), sX, sY, pen);
                }
                if (Cmd == Command.ELLIPSE)
                {
                    gr.DrawOval(new SKRect(Math.Min(cX, x), Math.Min(cY, y), Math.Min(cX, x) + sX, Math.Min(cY, y) + sY), pen);
                }
            }
            ptbDrawing.Invalidate();
            //Send(..., Command.END);
        }
        //Sự kiện tô lên bề mặt ptbDrawing

        private void ptbDrawing_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas render_canvas = e.Surface.Canvas;
            render_canvas.Clear(SKColors.White); // Xóa nền trước khi vẽ
            if (bmp != null)
            {
                render_canvas.DrawBitmap(bmp, 0, 0); // Vẽ bitmap lên control
            }
            if (isPainting)
            {
                using (var pen = new SKPaint { Color = color, Style = SKPaintStyle.Stroke, StrokeWidth = width, IsAntialias = true })
                {
                    if (Cmd == Command.LINE)
                    {
                        render_canvas.DrawLine(cX, cY, x, y, pen);
                    }
                    if (Cmd == Command.RECTANGLE)
                    {
                        render_canvas.DrawRect(Math.Min(cX, x), Math.Min(cY, y), sX, sY, pen);
                    }
                    if (Cmd == Command.ELLIPSE)
                    {
                        render_canvas.DrawOval(new SKRect (Math.Min(cX, x), Math.Min(cY, y), Math.Min(cX, x) + sX, Math.Min(cY, y) + sY), pen);
                    }
                    if (Cmd == Command.CURVE)
                    {
                        if (TempPoints.Count > 1)
                            render_canvas.DrawPath(CurvedPath(TempPoints), pen);
                    }
                    if (Cmd == Command.POLYGON)
                    {
                        if (TempPoints.Count > 1)
                            render_canvas.DrawPath(PolygonPath(TempPoints), pen);
                    }
                }
            }
            if (Cmd == Command.CURSOR && isDragging == true)
            {
                if (!selected.IsEmpty)
                {
                    Status.Text = $"Selected: ({selected.Left}, {selected.Top}), ({selected.Right}, {selected.Bottom})";
                    using (var penenter = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true, PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0) })
                    {
                        render_canvas.DrawRect(selected, penenter);
                    }
                }
            }
        }
        //Sự kiện Click chuột
        private void ptbDrawing_MouseClick_1(object sender, MouseEventArgs e)
        {
            SKPoint point = GetSKPoint(e.Location);
            if (Cmd == Command.FILL)
            {
                FillUp(bmp, (int)point.X, (int)point.Y, color);
                ptbDrawing.Invalidate();
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
                    using (var pen = new SKPaint { Color = color, Style = SKPaintStyle.Stroke, StrokeWidth = width, IsAntialias = true })
                    {
                        var path = Cmd == Command.CURVE ? CurvedPath(Points) : PolygonPath(Points);
                        gr.DrawPath(path, pen);
                    }
                    Points.Clear();
                    TempPoints.Clear();
                    isPainting = false;
                    ptbDrawing.Invalidate(); // Yêu cầu vẽ lại
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

        private void notiTick_Tick(object sender, EventArgs e)
        {

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
            save.Filter = "Image(*.jpg) |*.jpg|(*.*)|*.*";
            save.Title = "Save Image";
            save.FileName = "Paint.jpeg";
            using (var stream = File.OpenWrite(save.FileName))
            {
                // Encode SKBitmap thành PNG và ghi vào stream
                bmp.Encode(stream, SKEncodedImageFormat.Png, 100); // 100 là mức độ nén (0-100)
            }
        }

        //Logout
        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
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
                msgBox.Clear();
                return true; // Chặn xử lý tiếp theo
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
