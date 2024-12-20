using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dreaw.WorldForm
{
    public partial class userRoom : Form
    {
        public int ID { get; }
        public string RoomnAme { get; }
        public userRoom(string roomname, string lastModified, int thisID)
        {
            InitializeComponent();
            #region FixBugUI
            this.WindowState = FormWindowState.Maximized;
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            Resolution objFormResizer = new Resolution();
            objFormResizer.ResizeForm(this, screenHeight, screenWidth);
            #endregion
            roomName.Text = roomname;
            RoomnAme = roomname;
            lastModi.Text = $"Last modified: {lastModified}";
            ID = thisID;
            this.TopLevel = false; // Cho phép Form được thêm vào Container
        }
    }
}
