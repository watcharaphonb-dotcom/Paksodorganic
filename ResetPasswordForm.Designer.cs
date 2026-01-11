namespace test_pak
{
    partial class ResetPasswordForm
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
            this.textNewPass = new System.Windows.Forms.TextBox();
            this.textConfirmPass = new System.Windows.Forms.TextBox();
            this.back_home = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textNewPass
            // 
            this.textNewPass.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textNewPass.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textNewPass.Location = new System.Drawing.Point(421, 236);
            this.textNewPass.Name = "textNewPass";
            this.textNewPass.Size = new System.Drawing.Size(374, 31);
            this.textNewPass.TabIndex = 4;
            this.textNewPass.Text = "New Password";
            this.textNewPass.TextChanged += new System.EventHandler(this.textemail_TextChanged);
            this.textNewPass.Enter += new System.EventHandler(this.textNewPass_Enter);
            this.textNewPass.Leave += new System.EventHandler(this.textNewPass_Leave);
            // 
            // textConfirmPass
            // 
            this.textConfirmPass.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textConfirmPass.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textConfirmPass.Location = new System.Drawing.Point(421, 322);
            this.textConfirmPass.Name = "textConfirmPass";
            this.textConfirmPass.Size = new System.Drawing.Size(374, 31);
            this.textConfirmPass.TabIndex = 5;
            this.textConfirmPass.Text = "Confirm";
            this.textConfirmPass.TextChanged += new System.EventHandler(this.textConfirmPass_TextChanged);
            this.textConfirmPass.Enter += new System.EventHandler(this.textConfirmPass_Enter);
            this.textConfirmPass.Leave += new System.EventHandler(this.textConfirmPass_Leave);
            // 
            // back_home
            // 
            this.back_home.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(209)))), ((int)(((byte)(55)))));
            this.back_home.FlatAppearance.BorderSize = 0;
            this.back_home.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.back_home.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.back_home.Location = new System.Drawing.Point(23, 26);
            this.back_home.Name = "back_home";
            this.back_home.Size = new System.Drawing.Size(113, 45);
            this.back_home.TabIndex = 6;
            this.back_home.Text = "Back";
            this.back_home.UseVisualStyleBackColor = false;
            this.back_home.Click += new System.EventHandler(this.back_home_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(209)))), ((int)(((byte)(55)))));
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(692, 397);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(113, 45);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // ResetPasswordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::test_pak.Properties.Resources.reset;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1262, 673);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.back_home);
            this.Controls.Add(this.textConfirmPass);
            this.Controls.Add(this.textNewPass);
            this.Name = "ResetPasswordForm";
            this.Text = "ResetPasswordForm";
            this.Load += new System.EventHandler(this.ResetPasswordForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textNewPass;
        private System.Windows.Forms.TextBox textConfirmPass;
        private System.Windows.Forms.Button back_home;
        private System.Windows.Forms.Button btnSave;
    }
}