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

namespace DoAnPaint
{
    public partial class Form1 : Form
    {
        #region Fields
        private Graphics gr;
        private Bitmap bmp;
        private Command command;
        bool isPainting = false;
        Color color;
        Point pointX, pointy;
        int x, y, sX, sY, cX, cY;
        Pen pen = new Pen(Color.Black, 2);
        readonly Pen eraser = new Pen(Color.White, 4);
        int width;
        #endregion

        #region DrawMethods
        static Point SetPoint(PictureBox pictureBox, Point point)
        {
            float pX = 1f * pictureBox.Image.Width / pictureBox.Width;
            float pY = 1f * pictureBox.Image.Height / pictureBox.Height;
            return new Point((int)(point.X * pX), (int)(point.Y * pY));
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

        }
        #endregion

        public Form1()
        {
            InitializeComponent();
            bmp = new Bitmap(ptbDrawing.Width, ptbDrawing.Height);
            gr = ptbDrawing.CreateGraphics();
        }

        public Form1(Bitmap remoteBmp)
        {
            InitializeComponent();
            bmp = remoteBmp;
            gr = Graphics.FromImage(remoteBmp);
        }

        //Sự kiện click chuột, gửi yêu cầu xử lý nhấn chuột đến presenter
        private void mouseDown_Event(object sender, MouseEventArgs e)
        {
       
        }

        //Sự kiện di chuyển chuột, gửi yêu cầu xử lý di chuyển chuột đến presenter
        private void mouseMove_Event(object sender, MouseEventArgs e)
        {
            lbLocation.Text = e.Location.X + ", " + e.Location.Y + "px";
        }

        //Callback gọi vẽ lại hình
        public void refreshDrawing()
        {
            ptbDrawing.Invalidate();
        }

        /*Xử lý sự kiện click chuột vẽ hình, gửi yêu cầu vẽ hình
        theo trạng thái hiện tại đến presenter*/
        private void onPaint_Event(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        }

        //Callback để tiến hành vẽ hình

        //Xử lý sự kiện click chuột vẽ đường thẳng, gửi yêu cầu đến presenter
        private void btnLine_Click(object sender, EventArgs e)
        {
            
        }

        //Sự kiện thả chuột, gửi yêu cầu xử lý thả chuột đến presenter
        private void mouseUp_Event(object sender, MouseEventArgs e)
        {
            
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            
        }

        public void setCursor(Cursor cursor)
        {
            ptbDrawing.Cursor = cursor;
        }

        private void btnRectangle_Click(object sender, EventArgs e)
        {
            
        }

        private void btnEllipse_Click(object sender, EventArgs e)
        {
            
        }

        public void setDrawingRegionRectangle(Pen p, Rectangle rectangle, Graphics g)
        {
            g.DrawRectangle(p, rectangle);
        }

        private void btnGroup_Click(object sender, EventArgs e)
        {
            
        }

        private void btnUnGroup_Click(object sender, EventArgs e)
        {
            
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            
        }

        private void btnBezier_Click(object sender, EventArgs e)
        {
            
        }

        private void btnPolygon_Click(object sender, EventArgs e)
        {
            
        }


        private void btnPen_Click(object sender, EventArgs e)
        {
            
        }

        private void btnEraser_Click(object sender, EventArgs e)
        {
            
        }

        private void ptbEditColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                
            }
        }

        public void setColor(Color color)
        {
            ptbColor.BackColor = color;
        }

        private void btnLineSize_Scroll(object sender, EventArgs e)
        {
            
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnChangeColor_Click(object sender, EventArgs e)
        {
            PictureBox ptb = sender as PictureBox;
            ptbColor.BackColor = ptb.BackColor;
        }

        private void ptbDrawing_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        private void pictureBox31_Click(object sender, EventArgs e)
        {
            
        }


        private void btnClear_Click(object sender, EventArgs e)
        {
            
        }

        private void btnFill_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn.BackColor.Equals(Color.Indigo))
                Tips.SetToolTip(btnFill, "Fill Shape: On");
            else
                Tips.SetToolTip(btnFill, "Fill Shape: Off");
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
