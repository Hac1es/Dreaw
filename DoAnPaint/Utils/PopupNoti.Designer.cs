namespace DoAnPaint.Utils
{
    partial class PopupNoti
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.NotiColor = new Guna.UI2.WinForms.Guna2Panel();
            this.notiLabel = new System.Windows.Forms.Label();
            this.notiMessage = new System.Windows.Forms.Label();
            this.NotiPic = new Guna.UI2.WinForms.Guna2PictureBox();
            this.popupTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.NotiPic)).BeginInit();
            this.SuspendLayout();
            // 
            // NotiColor
            // 
            this.NotiColor.BackColor = System.Drawing.Color.LawnGreen;
            this.NotiColor.Location = new System.Drawing.Point(285, 0);
            this.NotiColor.Name = "NotiColor";
            this.NotiColor.Size = new System.Drawing.Size(29, 70);
            this.NotiColor.TabIndex = 0;
            // 
            // notiLabel
            // 
            this.notiLabel.AutoSize = true;
            this.notiLabel.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.notiLabel.Location = new System.Drawing.Point(68, 12);
            this.notiLabel.Name = "notiLabel";
            this.notiLabel.Size = new System.Drawing.Size(49, 25);
            this.notiLabel.TabIndex = 2;
            this.notiLabel.Text = "Type";
            // 
            // notiMessage
            // 
            this.notiMessage.AutoSize = true;
            this.notiMessage.Font = new System.Drawing.Font("Segoe UI Semilight", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.notiMessage.Location = new System.Drawing.Point(70, 40);
            this.notiMessage.Name = "notiMessage";
            this.notiMessage.Size = new System.Drawing.Size(34, 17);
            this.notiMessage.TabIndex = 3;
            this.notiMessage.Text = "Type";
            // 
            // NotiPic
            // 
            this.NotiPic.Image = global::DoAnPaint.Properties.Resources.Done;
            this.NotiPic.ImageRotate = 0F;
            this.NotiPic.Location = new System.Drawing.Point(12, 12);
            this.NotiPic.Name = "NotiPic";
            this.NotiPic.Size = new System.Drawing.Size(41, 45);
            this.NotiPic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.NotiPic.TabIndex = 1;
            this.NotiPic.TabStop = false;
            // 
            // popupTimer
            // 
            this.popupTimer.Interval = 15;
            this.popupTimer.Tick += new System.EventHandler(this.popupTimer_Tick);
            // 
            // PopupNoti
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(298, 69);
            this.Controls.Add(this.notiMessage);
            this.Controls.Add(this.notiLabel);
            this.Controls.Add(this.NotiPic);
            this.Controls.Add(this.NotiColor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PopupNoti";
            this.Text = "PopupNoti";
            ((System.ComponentModel.ISupportInitialize)(this.NotiPic)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Guna.UI2.WinForms.Guna2Panel NotiColor;
        private Guna.UI2.WinForms.Guna2PictureBox NotiPic;
        private System.Windows.Forms.Label notiLabel;
        private System.Windows.Forms.Label notiMessage;
        private System.Windows.Forms.Timer popupTimer;
    }
}