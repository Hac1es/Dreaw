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

namespace Dreaw
{
    public partial class code : Form
    {
        private string _verificationCode;
        public code(string verificationCode)
        {
            InitializeComponent();
            _verificationCode = verificationCode;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            string enteredCode = txtVerificationCode.Text.Trim();

            if (string.IsNullOrEmpty(enteredCode))
            {
                MessageBox.Show("Please enter the verification code!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (enteredCode != _verificationCode)
            {
                MessageBox.Show("Invalid verification code!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Khởi tạo và mở Form2
            newpw newForm = new newpw();
            newForm.Show();
            this.Hide();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
