using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAnPaint.Utils
{
    using System;
    using System.Windows.Forms;

    public class UnclosableNoti : Form
    {
        public UnclosableNoti(string message, string title)
        {
            // Thiết lập form
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            ControlBox = false; // Vô hiệu hóa nút đóng
            ShowInTaskbar = false;
            MaximizeBox = false;
            MinimizeBox = false;

            // Thêm nhãn để hiển thị thông báo
            var label = new Label
            {
                Text = message,
                AutoSize = false,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            Controls.Add(label);
            Width = 300; // Chiều rộng form
            Height = 150; // Chiều cao form
        }
    }
}
