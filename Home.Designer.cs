namespace test_pak
{
    partial class Home
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
            this.Sign_up = new System.Windows.Forms.Button();
            this.Sign_in = new System.Windows.Forms.Button();
            this.btnContact = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Sign_up
            // 
            this.Sign_up.BackColor = System.Drawing.Color.YellowGreen;
            this.Sign_up.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Sign_up.FlatAppearance.BorderSize = 0;
            this.Sign_up.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Sign_up.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Sign_up.Location = new System.Drawing.Point(221, 365);
            this.Sign_up.Name = "Sign_up";
            this.Sign_up.Size = new System.Drawing.Size(332, 59);
            this.Sign_up.TabIndex = 0;
            this.Sign_up.Text = "Create Account";
            this.Sign_up.UseVisualStyleBackColor = false;
            this.Sign_up.Click += new System.EventHandler(this.Sign_up_Click);
            // 
            // Sign_in
            // 
            this.Sign_in.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(228)))), ((int)(((byte)(153)))));
            this.Sign_in.FlatAppearance.BorderSize = 0;
            this.Sign_in.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Sign_in.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Sign_in.Location = new System.Drawing.Point(1091, 25);
            this.Sign_in.Name = "Sign_in";
            this.Sign_in.Size = new System.Drawing.Size(144, 48);
            this.Sign_in.TabIndex = 1;
            this.Sign_in.Text = "Log In";
            this.Sign_in.UseVisualStyleBackColor = false;
            this.Sign_in.Click += new System.EventHandler(this.Sign_in_Click);
            // 
            // btnContact
            // 
            this.btnContact.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(228)))), ((int)(((byte)(153)))));
            this.btnContact.FlatAppearance.BorderSize = 0;
            this.btnContact.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnContact.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnContact.Location = new System.Drawing.Point(941, 25);
            this.btnContact.Name = "btnContact";
            this.btnContact.Size = new System.Drawing.Size(144, 48);
            this.btnContact.TabIndex = 2;
            this.btnContact.Text = "Contact";
            this.btnContact.UseVisualStyleBackColor = false;
            this.btnContact.Click += new System.EventHandler(this.btnContact_Click);
            // 
            // Home
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::test_pak.Properties.Resources.Home__9_;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1262, 673);
            this.Controls.Add(this.btnContact);
            this.Controls.Add(this.Sign_in);
            this.Controls.Add(this.Sign_up);
            this.MaximizeBox = false;
            this.Name = "Home";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Home_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Sign_up;
        private System.Windows.Forms.Button Sign_in;
        private System.Windows.Forms.Button btnContact;
    }
}

