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

using DoAnPaint.Utils;
using System.IO;
using System.Windows.Documents;
using System.Windows;
using Point = System.Drawing.Point;
using Application = System.Windows.Forms.Application;
using Sprache;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using TrackBar = System.Windows.Forms.TrackBar;
using Button = System.Windows.Forms.Button;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Windows.Media;
using Pen = System.Drawing.Pen;
using Color = System.Drawing.Color;
using DashStyle = System.Drawing.Drawing2D.DashStyle;
using System.Windows.Media.Media3D;
using Brush = System.Drawing.Brush;
using System.Configuration;
using System.Drawing.Imaging;

namespace DoAnPaint
{
    public partial class Form1 : Form
    {
        private static string Capitalize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
        #region Fields
        private Graphics gr; //Graphic chính của Form vẽ
        private Bitmap bmp; //Bitmap để vẽ lệnh
        private Command command; //Danh sách các lệnh(không dùng cái này, ta sẽ dùng property của nó)
        private Color colorr; //Màu(không dùng cái này, ta sẽ dùng property của nó)
        bool isPainting = false; //Có đang sử dụng tính năng không? 
        bool isDragging = false;
        /* 
            List Points là hàng thật, thứ sẽ hiện thị lên canvas
            List TempPoints chỉ là preview, cập nhật liên tục theo vị trí chuột
         */
        List<Point> Points = new List<Point>();
        List<Point> TempPoints = new List<Point>();
        Point pointX, pointY; //Dùng trong tính năng phải cập nhật vị trí liên tục(pen, eraser, crayon)
        int x, y, sX, sY, cX, cY;
        /* 
         x, y: cập nhật vị trí liên tục, dùng trong onPaint khi cần phải cho người dùng xem trước
        hình dạng (đường thẳng, hình chữ nhật, ..)
        sX, sY: (sizeX, sizeY) Kích thước của hình chữ nhật, hình tròn cần vẽ
        cX, cY: (currentX, currentY) Vị trị bắt đầu nhấn chuột xuống/Tọa độ bắt đầu của hình vẽ
         */
        int width = 2; //Độ dày khởi đầu nét bút
        Rectangle selected = Rectangle.Empty;
        #region Sự kiện khi Color thay đổi
        // Property của colorr
        // Sự kiện xảy ra khi Color thay đổi
        public event Action<Color> ColorChanged; //event được kích khi color thay đổi
        private Color color //Sử dụng properties này để kiểm soát
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
                    MessageBox.Show("Complete current action first!");
                    return;
                }
                if (command != value)
                {
                    command = value;
                    if (value != Command.CURSOR)
                    {
                        selected = Rectangle.Empty;
                        Status.Text = Capitalize(value.ToString());
                    }
                    else Status.Text = Capitalize(value.ToString());
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
        static Point SetPoint(PictureBox pictureBox, Point point)
        {
            if (pictureBox.Image != null)
            {
                float pX = 1f * pictureBox.Image.Width / pictureBox.Width;
                float pY = 1f * pictureBox.Image.Height / pictureBox.Height;
                return new Point((int)(point.X * pX), (int)(point.Y * pY));
            }
            return point;    
        }
        private void Validate(Bitmap bitmap, Stack<Point> ptStack, int x, int y, Color b4, Color after)
        {
            //Tìm biên giới
            Color current = bitmap.GetPixel(x, y);
            if (current == b4)
            {
                ptStack.Push(new Point(x, y));
                bitmap.SetPixel(x, y, after);
            }
        }
        /// <summary>
        /// Fill màu sử dụng DFS
        /// </summary>
        public void FillUp(Bitmap bitmap, int x, int y, Color New)
        {
            Color Old = bitmap.GetPixel(x, y);
            Stack<Point> ptStack = new Stack<Point>();
            ptStack.Push(new Point(x, y));
            bitmap.SetPixel(x, y, New);
            if (Old == New) return;
            while (ptStack.Count > 0)
            {
                Point pt = (Point)ptStack.Pop();
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
        public GraphicsPath CurvedPath(List<Point> points) 
        {
            GraphicsPath path = new GraphicsPath();
            path.AddCurve(points.ToArray());
            return path;
        }
        /// <summary>
        /// Tạo ra đường đi của polygon(đa giác)
        /// </summary>
        public GraphicsPath PolygonPath(List<Point> points)
        {
            GraphicsPath path = new GraphicsPath();
            if (points.Count < 3)
            {
                path.AddLine(points[0], points[1]);
            }
            else
            {
                path.AddPolygon(points.ToArray());
            }
            return path;
        }
        /// <summary>
        /// Tạo ra Texture giả lập bút chì màu(Crayon)
        /// </summary>
        public TextureBrush CrayonTexture(Color color, int width) 
        {
            int grainDensity = width * 50; //mật độ của các hạt màu
            int textureSize = width * 4; //Kích thước của texture
            Bitmap texture = new Bitmap(textureSize, textureSize);
            Random random = new Random();

            using (Graphics g = Graphics.FromImage(texture))
            {
                g.Clear(Color.Transparent);

                //Tạo ra các hạt mực ngẫu nhiên trên bề mặt của texture
                for (int i = 0; i < grainDensity; i++)
                {
                    int x = random.Next(textureSize);
                    int y = random.Next(textureSize);

                    int alpha = random.Next(100, 200); // Độ trong suốt ngẫu nhiên
                    Color grainColor = Color.FromArgb(alpha, color); 

                    texture.SetPixel(x, y, grainColor); //Tạo ra hạt mực với vị trí ngẫu nhiên
                                                        //+ độ trong suốt ngầu nhiên + màu do người dùng chọn
                }
            }

            return new TextureBrush(texture);
        }
        #endregion

        public Form1()
        {
            InitializeComponent();
            bmp = new Bitmap(ptbDrawing.Width, ptbDrawing.Height); //Khởi tạo Bitmap
            gr = Graphics.FromImage(bmp); //Khởi tạo Graphics
            gr.Clear(Color.White);
            ptbDrawing.Image = bmp;
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            /*Toàn bộ mọi thứ ở đây là liên quan tới UI
             * Logic: Nó làm 2 thứ:
            -Khi màu đổi: Đổi màu trên UI
            -Khi Command đổi: Đổi Control đang được lựa chọn trên UI + Đổi cái Scroll chọn giá trị
            */
            controls.AddRange(new Control[] { btnPen, btnCrayon, btnEraser, btnBezier, btnLine, btnRectangle, btnEllipse, btnPolygon, btnSelect, btnOCR, btnFill });
            // Đăng ký sự kiện thay đổi màu
            ColorChanged += newColor => ptbColor.BackColor = newColor;

            // Đặt màu khởi tạo
            color = Color.Black; // Thay đổi Color, Panel sẽ đổi màu

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
                    case Command.BEIZER:
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
        }

        public Form1(Bitmap remoteBmp) //Constructor remote, trong trường hợp lấy phòng từ CSDL
        {
            InitializeComponent();
            bmp = remoteBmp;
            gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);
            ptbDrawing.Image = bmp;
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            controls.AddRange(new Control[] { btnPen, btnEraser, btnBezier, btnLine, btnRectangle, btnEllipse, btnPolygon, btnSelect, btnOCR, btnFill });
            ColorChanged += newColor => ptbColor.BackColor = newColor;

            color = Color.Black;
            CommandChanged += (cmd) =>
            {
                controls.ForEach(ctrl => ctrl.BackColor = Color.Transparent);
                switch (cmd)
                {
                    case Command.PENCIL:
                        btnPen.BackColor = Color.PaleTurquoise;
                        break;
                    case Command.ERASER:
                        btnEraser.BackColor = Color.PaleTurquoise;
                        break;
                    case Command.LINE:
                        btnLine.BackColor = Color.PaleTurquoise;
                        break;
                    case Command.POLYGON:
                        btnPolygon.BackColor = Color.PaleTurquoise;
                        break;
                    case Command.BEIZER:
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
            CommandChanged += (cmd) =>
            {
                if (cmd == Command.CRAYON || cmd == Command.ERASER)
                {
                    btnLineSize.Minimum = 2;
                    btnLineSize.Maximum = 10;
                    btnLineSize.TickFrequency = 2;
                    btnLineSize.SmallChange = 2;
                    btnLineSize.LargeChange = 2;
                    btnLineSize.Value = this.width = 4;
                    Tips.SetToolTip(btnLineSize, $"Pen/Border size: {btnLineSize.Value}");
                }
                else
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
            Cmd = Command.CURSOR;
        }

        //Sự kiện ấn chuột xuống
        private void mouseDown_Event(object sender, MouseEventArgs e)
        {
            if (Cmd != Command.BEIZER && Cmd != Command.CURSOR && Cmd != Command.POLYGON) isPainting = true;
            else if (Cmd == Command.CURSOR)
            {
                isDragging = true;
                isPainting = false;
                selected = Rectangle.Empty;
            }
            pointX = e.Location;
            cX = e.X;
            cY = e.Y;
            //Send(new[] {isPainting, pointY, cX, cY}, START);
        }

        //Sự kiện di chuyển chuột
        private void mouseMove_Event(object sender, MouseEventArgs e)
        {
            lbLocation.Text = e.Location.X + ", " + e.Location.Y + "px";
            if (!isPainting && Cmd != Command.CURSOR) return;
            if (Cmd == Command.PENCIL)
            {
                pointY = e.Location;
                using (Pen pen = new Pen(color, width))
                    gr.DrawLine(pen, pointX, pointY);
                pointX = pointY;
            }
            if (Cmd == Command.CRAYON)
            {
                pointY = e.Location;
                using (Pen pen = new Pen(CrayonTexture(color, width), width * 4))
                    gr.DrawLine(pen, pointX, pointY);
                pointX = pointY;
            }
            if (Cmd == Command.ERASER)
            {
                pointY = e.Location;
                using (Pen pen = new Pen(Color.White, width * 4))
                    gr.DrawLine(pen, pointX, pointY);
                pointX = pointY;
            }
            if (Cmd == Command.BEIZER || Cmd == Command.POLYGON)
            {
                if (TempPoints.Count > Points.Count)
                {
                    TempPoints[TempPoints.Count - 1] = e.Location;
                }
                else
                {
                    TempPoints.Add(e.Location);
                }
            }
            x = e.X;
            y = e.Y;
            sX = Math.Abs(e.X - cX);
            sY = Math.Abs(e.Y - cY);
            if (Cmd == Command.CURSOR && isDragging == true)
            {
                selected = new Rectangle(Math.Min(cX, x), Math.Min(cY, y), sX, sY);
            }    
            ptbDrawing.Invalidate();
        }


        /*Preview nét vẽ cho các tính năng Shape cho người dùng*/
        private void onPaint_Event(object sender, PaintEventArgs e)
        {
            Graphics paint_gr = e.Graphics;
            paint_gr.SmoothingMode = SmoothingMode.AntiAlias;
            if (isPainting)
            {
                using (Pen pen = new Pen(color, width))
                {
                    if (Cmd == Command.LINE)
                    {
                        paint_gr.DrawLine(pen, cX, cY, x, y);
                    }
                    if (Cmd == Command.RECTANGLE)
                    {
                        paint_gr.DrawRectangle(pen, Math.Min(cX, x), Math.Min(cY, y), sX, sY);
                    }
                    if (Cmd == Command.ELLIPSE)
                    {
                        paint_gr.DrawEllipse(pen, Math.Min(cX, x), Math.Min(cY, y), sX, sY);
                    }
                    if (Cmd == Command.BEIZER)
                    {
                        if (TempPoints.Count > 1)
                            paint_gr.DrawPath(pen, CurvedPath(TempPoints));
                    }
                    if (Cmd == Command.POLYGON)
                    {
                        if (TempPoints.Count > 1)
                            paint_gr.DrawPath(pen, PolygonPath(TempPoints));
                    }    
                }  
            }
            if (Cmd == Command.CURSOR && isDragging == true)
            {
                if (!selected.IsEmpty)
                {
                    Status.Text = $"Selected: {selected.X}, {selected.Y}, {selected.X + selected.Width}, {selected.Y + selected.Height}";
                    using (Pen penenter = new Pen(Color.Black, 1))
                    {
                        penenter.DashStyle = DashStyle.DashDotDot; // Viền nét đứt
                        paint_gr.DrawRectangle(penenter, selected);
                    }
                }
            }
        }


        //Chế độ Line
        private void btnLine_Click(object sender, EventArgs e)
        {
            setCursor(Cursorr.NONE);
            Cmd = Command.LINE;
        }

        //Sự kiện thả chuột
        private void mouseUp_Event(object sender, MouseEventArgs e)
        {
            if (Cmd != Command.BEIZER && Cmd != Command.POLYGON) isPainting = false;
            if (Cmd == Command.CURSOR) isDragging = false;
            sX = Math.Abs(e.X - cX);
            sY = Math.Abs(e.Y - cY);
            using (Pen pen = new Pen(color, width))
            {
                if (Cmd == Command.LINE)
                {
                    gr.DrawLine(pen, cX, cY, x, y);
                }
                if (Cmd == Command.RECTANGLE)
                {
                    gr.DrawRectangle(pen, Math.Min(cX, x), Math.Min(cY, y), sX, sY);
                }
                if (Cmd == Command.ELLIPSE)
                {
                    gr.DrawEllipse(pen, Math.Min(cX, x), Math.Min(cY, y), sX, sY);
                }
            }    
            ptbDrawing.Invalidate();
            //Send(..., Command.END);
        }

        //Chế độ không làm gì cả
        private void btnSelect_Click(object sender, EventArgs e)
        {
            Cmd = Command.CURSOR;
            selected = Rectangle.Empty;
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
            Cmd = Command.BEIZER;
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
                color = colorDialog.Color;  
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
            color = ptb.BackColor;
        }

        //Sự kiện click chuột
        private void ptbDrawing_MouseClick(object sender, MouseEventArgs e)
        {
            Point point = SetPoint(ptbDrawing, e.Location);
            if (Cmd == Command.FILL)
            {
                FillUp(bmp, point.X, point.Y, color);
                ptbDrawing.Invalidate();
            }
            // cảnh bảo người dùng nếu thiếu điểm tạo nên polygon/curve
            if (Cmd == Command.BEIZER || Cmd == Command.POLYGON)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (!isPainting)
                    {
                        isPainting = true;
                        Points.Add(e.Location);
                        TempPoints = Points.ToList();
                    }
                    else
                    {
                        Points.Add(e.Location);
                        TempPoints = Points.ToList();
                    }     
                }
                else if (Points.Any() && isPainting)
                {
                    Points.Add(e.Location);

                    // Kiểm tra nếu số lượng điểm < 3, hiển thị cảnh báo
                    if (Points.Count < 3 && Cmd == Command.POLYGON)
                    {
                        MessageBox.Show("Để vẽ polygon, bạn cần ít nhất 3 điểm.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Points.Clear();
                        TempPoints.Clear();
                        isPainting = false;
                        return; // Dừng việc vẽ nếu không đủ điểm
                    } else if (Points.Count < 3 && Cmd == Command.BEIZER)
                    {
                        MessageBox.Show("Để vẽ đường cong Bezier, bạn cần ít nhất 3 điểm.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Points.Clear();
                        TempPoints.Clear();
                        isPainting = false;
                        return; // Dừng việc vẽ nếu không đủ điểm
                    }

                    using (Pen pen = new Pen(color, width))
                    {
                        var path = Cmd == Command.BEIZER? CurvedPath(Points) : PolygonPath(Points);
                        gr.DrawPath(pen, path);
                    } 
                    Points.Clear();
                    TempPoints.Clear();
                    isPainting = false;
                }
            }   
        }

        //Xóa vùng được chọn
        private void btnClear_Click(object sender, EventArgs e)
        {
            if (selected != Rectangle.Empty) //Nếu đã chọn vùng
            {
                using (Brush whiteBrush = new SolidBrush(Color.White)) // Tạo bút vẽ màu trắng
                {
                    gr.FillRectangle(whiteBrush, selected); // Fill màu trắng vào hình chữ nhật
                }
            }
            else //Xóa hết
            {
                gr.Clear(Color.White);
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
            if (selected == Rectangle.Empty) {
                MessageBox.Show("Selected something first!");
                return;
            }
            var croped = bmp.Clone(selected, bmp.PixelFormat);
        }

        //Sự kiện double click
        private void ptbDrawing_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Cmd != Command.CURSOR) return;
            Color pixelColor = bmp.GetPixel(e.X, e.Y);
            color = pixelColor == Color.FromArgb(0, 0, 0, 0) ? Color.White : pixelColor;
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
            if (save.ShowDialog() == DialogResult.OK)
            {
                Bitmap savebmp = bmp.Clone(new Rectangle(0, 0, ptbDrawing.Width, ptbDrawing.Height), bmp.PixelFormat);
                savebmp.Save(save.FileName, ImageFormat.Jpeg);
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
