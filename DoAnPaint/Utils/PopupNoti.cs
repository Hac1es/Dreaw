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
        public PopupNoti(Form caller)
        {
            InitializeComponent();
            position.X = caller.Width - this.Width + 62;
            position.Y = caller.Height - this.Height;
        }
    }
}
