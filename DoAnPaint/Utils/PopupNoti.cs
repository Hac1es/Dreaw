using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoAnPaint.Utils
{
    public partial class PopupNoti : Form
    {
        public Point position;
        private readonly bool timeout;
        private int TTL = 0; // Thời gian đã trôi qua (ms)
        private int displayTime = 1000; // Thời gian hiển thị thông báo (ms)
        /// <summary>
        /// Tạo pop-up notification
        /// </summary>
        /// <param name="caller">Form cha gọi ra cái noti</param>
        /// <param name="type">Loại thông báo: ok, error, warning, khác(điền gì cũng được)</param>
        /// <param name="msg">Thông báo cần hiện thị</param>
        /// <param name="flag">Có đặt thời gian hết hạn không?</param>
        public PopupNoti(Form caller, string type, string msg, bool flag = true)
        {
            InitializeComponent();
            position.X = caller.Width - this.Width + 62;
            position.Y = caller.Height - this.Height;
            timeout = flag;
            if (type == "ok")
            {
                NotiColor.BackColor = Color.LawnGreen;
                NotiPic.Image = Properties.Resources.Done;
                notiLabel.Text = "Success!";
                notiMessage.Text = msg;
            }
            else if (type == "error")
            {
                NotiColor.BackColor = Color.Red;
                NotiPic.Image = Properties.Resources.Error;
                notiLabel.Text = "Error!";
                notiMessage.Text = msg;
            } 
            else if (type == "warning")
            {
                NotiColor.BackColor = Color.Yellow;
                NotiPic.Image = Properties.Resources.General_Warning_Sign;
                notiLabel.Text = "Warning";
                notiMessage.Text = msg;
            }
            else
            {
                NotiColor.BackColor = Color.LightSkyBlue;
                NotiPic.Image = Properties.Resources.Done;
                notiLabel.Text = type;
                notiMessage.Text = msg;
            }
            if (timeout == true) 
                popupTimer.Start();
        }

        private void popupTimer_Tick(object sender, EventArgs e)
        {
            TTL += popupTimer.Interval; //Đủ 1s
            if (TTL >= displayTime) {
                popupTimer.Stop();
                this.Close();
            }
        }
    }
}
