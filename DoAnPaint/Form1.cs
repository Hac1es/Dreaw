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

namespace DoAnPaint
{
    public partial class Form1 : Form
    {
        #region Fields
        private Graphics gr;
        private Bitmap bmp;
        private Command command;
        private Color colorr;
        bool isPainting = false;
        Point pointX, pointY;
        int x, y, sX, sY, cX, cY;
        Pen pen;
        readonly Pen eraser = new Pen(Color.White, 15);
        int width = 2;
        #region Sự kiện khi Color thay đổi
        // Property với sự kiện
        // Sự kiện xảy ra khi Color thay đổi
        public event Action<Color> ColorChanged;

        private Color color
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
        #endregion
        #region Sự kiện khi Command thay đổi
        List<Control> controls = new List<Control>();
        // Property với sự kiện
        // Sự kiện xảy ra khi Color thay đổi
        public event Action<Command> CommandChanged;
        private Command Cmd
        {
            get => command;
            set
            {
                if (command != value)
                {
                    command = value;
                    CommandChanged?.Invoke(value);
                }
            }
        }
        #endregion
        #endregion

        #region DrawMethods
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
            Color current = bitmap.GetPixel(x, y);
            if (current == b4)
            {
                ptStack.Push(new Point(x, y));
                bitmap.SetPixel(x, y, after);
            }
        }

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
        #endregion

        public Form1()
        {
            InitializeComponent();
            bmp = new Bitmap(ptbDrawing.Width, ptbDrawing.Height);
            gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);
            ptbDrawing.Image = bmp;
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            controls.AddRange(new Control[] { btnPen, btnEraser, btnBezier, btnLine, btnRectangle, btnEllipse, btnPolygon, btnSelect, btnOCR, btnFill });
            // Đăng ký sự kiện thay đổi màu
            ColorChanged += newColor => ptbColor.BackColor = newColor;

            // Đặt màu khởi tạo
            color = Color.Black; // Thay đổi Color, Panel sẽ đổi màu
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
            Cmd = Command.CURSOR;
        }

        public Form1(Bitmap remoteBmp)
        {
            InitializeComponent();
            bmp = new Bitmap(ptbDrawing.Width, ptbDrawing.Height);
            gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);
            ptbDrawing.Image = bmp;
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            controls.AddRange(new Control[] { btnPen, btnEraser, btnBezier, btnLine, btnRectangle, btnEllipse, btnPolygon, btnSelect, btnOCR, btnFill });
            // Đăng ký sự kiện thay đổi màu
            ColorChanged += newColor => ptbColor.BackColor = newColor;

            // Đặt màu khởi tạo
            color = Color.Black; // Thay đổi Color, Panel sẽ đổi màu
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
            Cmd = Command.CURSOR;
        }

        //Sự kiện click chuột, gửi yêu cầu xử lý nhấn chuột đến presenter
        private void mouseDown_Event(object sender, MouseEventArgs e)
        {
            isPainting = true;
            pointY = e.Location;
            cX = e.X;
            cY = e.Y;
        }

        //Sự kiện di chuyển chuột, gửi yêu cầu xử lý di chuyển chuột đến presenter
        private void mouseMove_Event(object sender, MouseEventArgs e)
        {
            lbLocation.Text = e.Location.X + ", " + e.Location.Y + "px";
            if (!isPainting) return;
            if (Cmd == Command.PENCIL)
            {
                pointX = e.Location;
                pen = new Pen(color, width);
                gr.DrawLine(pen, pointX, pointY);
                pointY = pointX;
            }
            if (Cmd == Command.ERASER)
            {
                pointX = e.Location;
                gr.DrawLine(eraser, pointX, pointY);
                pointY = pointX;
            }
            ptbDrawing.Invalidate();
            x = e.X;
            y = e.Y;
            sX = e.X - cX;
            sY = e.Y - cY;
        }


        /*Xử lý sự kiện click chuột vẽ hình, gửi yêu cầu vẽ hình
        theo trạng thái hiện tại đến presenter*/
        private void onPaint_Event(object sender, PaintEventArgs e)
        {
            Graphics paint_gr = e.Graphics;
            paint_gr.SmoothingMode = SmoothingMode.AntiAlias;
            if (isPainting)
            {
                pen = new Pen(color, width);
                if (Cmd == Command.LINE)
                {
                    paint_gr.DrawLine(pen, cX, cY, x, y);
                }
                if (Cmd == Command.RECTANGLE)
                {
                    paint_gr.DrawRectangle(pen, cX, cY, sX, sY);
                }
                if (Cmd == Command.ELLIPSE)
                {
                    paint_gr.DrawEllipse(pen, cX, cY, sX, sY);
                }
            }  
        }


        //Xử lý sự kiện click chuột vẽ đường thẳng, gửi yêu cầu đến presenter
        private void btnLine_Click(object sender, EventArgs e)
        {
            setCursor(Cursorr.NONE);
            Cmd = Command.LINE;
        }

        //Sự kiện thả chuột, gửi yêu cầu xử lý thả chuột đến presenter
        private void mouseUp_Event(object sender, MouseEventArgs e)
        {
            isPainting = false;
            sX = x - cX;
            sY = y - cY;
            pen = new Pen(color, width);
            if (Cmd == Command.LINE)
            {
                gr.DrawLine(pen, cX, cY, x, y);
            }
            if (Cmd == Command.RECTANGLE)
            {
                gr.DrawRectangle(pen, cX, cY, sX, sY);
            } 
            if (Cmd == Command.ELLIPSE)
            {
                gr.DrawEllipse(pen, cX, cY, sX, sY);
            }
            ptbDrawing.Invalidate();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            Cmd = Command.CURSOR;
            setCursor(Cursorr.NONE);
        }

        public void setCursor(Cursorr cursor)
        {
            string template = @"..\..\Resources\{0}";
            string where;
            switch (cursor)
            {
                case Cursorr.PENCIL:
                    where = string.Format(template, "Pencil.png");
                    break;
                case Cursorr.ERASER:
                    where = string.Format(template, "Eraser.png");
                    break;
                case Cursorr.FILL:
                    where = string.Format(template, "Fill Color.png");
                    break;
                default:
                    where = null;
                    break;
            }
            if (where != null)
            {
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

        private void btnEllipse_Click(object sender, EventArgs e)
        {
            Cmd = Command.ELLIPSE;
            setCursor(Cursorr.NONE);
        }

        public void setDrawingRegionRectangle(Pen p, Rectangle rectangle, Graphics g)
        {
            g.DrawRectangle(p, rectangle);
        }

        private void btnBezier_Click(object sender, EventArgs e)
        {
            Cmd = Command.BEIZER;
            setCursor(Cursorr.NONE);
        }

        private void btnPolygon_Click(object sender, EventArgs e)
        {
            Cmd = Command.POLYGON;
            setCursor(Cursorr.NONE);
        }

        private void btnPen_Click(object sender, EventArgs e)
        {
            setCursor(Cursorr.PENCIL);
            Cmd = Command.PENCIL;
        }

        private void btnEraser_Click(object sender, EventArgs e)
        {
            setCursor(Cursorr.ERASER);
            Cmd = Command.ERASER;
        }

        private void ptbEditColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                color = colorDialog.Color;  
            }
        }

        private void btnLineSize_Scroll(object sender, EventArgs e)
        {
            this.width = btnLineSize.Value;
        }

        private void btnChangeColor_Click(object sender, EventArgs e)
        {
            PictureBox ptb = sender as PictureBox;
            color = ptb.BackColor;
        }

        private void ptbDrawing_MouseClick(object sender, MouseEventArgs e)
        {
            Point point = SetPoint(ptbDrawing, e.Location);
            if (Cmd == Command.FILL)
            {
                FillUp(bmp, point.X, point.Y, color);
                ptbDrawing.Invalidate();
            }  
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            gr.Clear(Color.White);
            ptbDrawing.Invalidate();
        }

        private void btnFill_Click(object sender, EventArgs e)
        {
            setCursor(Cursorr.FILL);
            Cmd = Command.FILL;
        }

        private void btnOCR_Click(object sender, EventArgs e)
        {
            setCursor(Cursorr.NONE);
            Cmd = Command.OCR;
        }

        private void ptbDrawing_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Color pixelColor = bmp.GetPixel(e.X, e.Y);
            color = pixelColor == Color.FromArgb(0, 0, 0, 0) ? Color.White : pixelColor;
        }

        public void setColor(Button btn, Color color)
        {
            btn.BackColor = color;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

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
