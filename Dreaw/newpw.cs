using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace Dreaw
{
    public partial class newpw : Form
    {
        public newpw()
        {
            InitializeComponent();
            
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            
            // Khởi tạo và mở Form2
            Loginform newForm = new Loginform();
            newForm.Show();
            this.Hide();
        }
       
    }
}
