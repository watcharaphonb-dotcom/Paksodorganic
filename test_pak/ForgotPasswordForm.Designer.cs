namespace test_pak
{
    partial class ForgotPasswordForm
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
            this.textemail = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // back_home
            // 
            this.back_home.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(209)))), ((int)(((byte)(55)))));
            this.back_home.FlatAppearance.BorderSize = 0;
            this.back_home.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.back_home.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.back_home.Location = new System.Drawing.Point(33, 30);
            this.back_home.Name = "back_home";
            this.back_home.Size = new System.Drawing.Size(113, 45);
            this.back_home.TabIndex = 1;
            this.back_home.Text = "Back";
            this.back_home.UseVisualStyleBackColor = false;
            this.back_home.Click += new System.EventHandler(this.back_home_Click);
            // 
            // textemail
            // 
            this.textemail.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textemail.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textemail.Location = new System.Drawing.Point(446, 271);
            this.textemail.Name = "textemail";
            this.textemail.Size = new System.Drawing.Size(374, 31);
            this.textemail.TabIndex = 3;
            this.textemail.Text = "Email";
            this.textemail.Enter += new System.EventHandler(this.textemail_Enter);
            this.textemail.Leave += new System.EventHandler(this.textemail_Leave);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(209)))), ((int)(((byte)(55)))));
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(577, 355);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(253, 45);
            this.button1.TabIndex = 4;
            this.button1.Text = "Reset Password";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.btnResetPassword_Click);
            // 
            // ForgotPasswordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::test_pak.Properties.Resources.Forgot2;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1262, 673);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textemail);
            this.Controls.Add(this.back_home);
            this.Name = "ForgotPasswordForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.ForgotPasswordForm_Load);
            this.Click += new System.EventHandler(this.back_home_Click);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button back_home;
        private System.Windows.Forms.TextBox textemail;
        private System.Windows.Forms.Button button1;
    }
}