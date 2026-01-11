namespace test_pak
{
    partial class SigninForm
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
            this.back_home = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.textemail = new System.Windows.Forms.TextBox();
            this.textpass = new System.Windows.Forms.TextBox();
            this.Log_in = new System.Windows.Forms.Button();
            this.Forgot = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // back_home
            // 
            this.back_home.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(209)))), ((int)(((byte)(55)))));
            this.back_home.FlatAppearance.BorderSize = 0;
            this.back_home.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.back_home.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.back_home.Location = new System.Drawing.Point(43, 31);
            this.back_home.Name = "back_home";
            this.back_home.Size = new System.Drawing.Size(113, 45);
            this.back_home.TabIndex = 0;
            this.back_home.Text = "Back";
            this.back_home.UseVisualStyleBackColor = false;
            this.back_home.Click += new System.EventHandler(this.back_home_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(585, 417);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(0, 16);
            this.linkLabel1.TabIndex = 1;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // textemail
            // 
            this.textemail.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textemail.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textemail.Location = new System.Drawing.Point(435, 233);
            this.textemail.Name = "textemail";
            this.textemail.Size = new System.Drawing.Size(338, 27);
            this.textemail.TabIndex = 2;
            this.textemail.Text = "Email";
            this.textemail.TextChanged += new System.EventHandler(this.textemail_TextChanged);
            this.textemail.Enter += new System.EventHandler(this.textemail_Enter);
            this.textemail.Leave += new System.EventHandler(this.textemail_Leave);
            // 
            // textpass
            // 
            this.textpass.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textpass.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textpass.Location = new System.Drawing.Point(437, 313);
            this.textpass.Name = "textpass";
            this.textpass.Size = new System.Drawing.Size(362, 27);
            this.textpass.TabIndex = 3;
            this.textpass.Text = "Password";
            this.textpass.TextChanged += new System.EventHandler(this.textpass_TextChanged);
            this.textpass.Enter += new System.EventHandler(this.textpass_Enter);
            this.textpass.Leave += new System.EventHandler(this.textpass_Leave);
            // 
            // Log_in
            // 
            this.Log_in.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(209)))), ((int)(((byte)(55)))));
            this.Log_in.FlatAppearance.BorderSize = 0;
            this.Log_in.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Log_in.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Log_in.Location = new System.Drawing.Point(705, 372);
            this.Log_in.Name = "Log_in";
            this.Log_in.Size = new System.Drawing.Size(113, 45);
            this.Log_in.TabIndex = 4;
            this.Log_in.Text = "Log in";
            this.Log_in.UseVisualStyleBackColor = false;
            this.Log_in.Click += new System.EventHandler(this.Log_in_Click);
            // 
            // Forgot
            // 
            this.Forgot.ActiveLinkColor = System.Drawing.Color.DarkGray;
            this.Forgot.AutoSize = true;
            this.Forgot.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Forgot.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(162)))), ((int)(((byte)(181)))), ((int)(((byte)(255)))));
            this.Forgot.Location = new System.Drawing.Point(541, 444);
            this.Forgot.Name = "Forgot";
            this.Forgot.Size = new System.Drawing.Size(146, 22);
            this.Forgot.TabIndex = 5;
            this.Forgot.TabStop = true;
            this.Forgot.Text = "Forgot Password";
            this.Forgot.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // SigninForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::test_pak.Properties.Resources.signin__9_;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1262, 673);
            this.Controls.Add(this.Forgot);
            this.Controls.Add(this.Log_in);
            this.Controls.Add(this.textpass);
            this.Controls.Add(this.textemail);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.back_home);
            this.MaximizeBox = false;
            this.Name = "SigninForm";
            this.Text = "SigninForm";
            this.Load += new System.EventHandler(this.SigninForm_Load_1);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button back_home;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.TextBox textemail;
        private System.Windows.Forms.TextBox textpass;
        private System.Windows.Forms.Button Log_in;
        private System.Windows.Forms.LinkLabel Forgot;
    }
}