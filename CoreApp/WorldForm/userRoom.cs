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
        public userRoom(string roomname, string lastModified, int thisID)
        {
            InitializeComponent();
            roomName.Text = roomname;
            lastModi.Text = $"Last modified: {lastModified}";
            ID = thisID;
            this.TopLevel = false; // Cho phép Form được thêm vào Container
        }
    }
}
