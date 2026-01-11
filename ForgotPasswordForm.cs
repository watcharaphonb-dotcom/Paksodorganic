using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_pak
{
    public partial class ForgotPasswordForm : Form
    {
        public ForgotPasswordForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnResetPassword_Click(object sender, EventArgs e)
        {
            // 1. เช็คอีเมลในฐานข้อมูล
            string email = textemail.Text.Trim();

            using (var conn = new MySql.Data.MySqlClient.MySqlConnection(Program.ConnStr))
            {
                conn.Open();
                var cmd = new MySql.Data.MySqlClient.MySqlCommand("SELECT COUNT(*) FROM user_info WHERE email=@e", conn);
                cmd.Parameters.AddWithValue("@e", email);

                if ((long)cmd.ExecuteScalar() > 0) // ถ้าเจออีเมล
                {
                    // 2. เด้งไปหน้าใหม่! (ส่งอีเมลไปด้วย)
                    this.Hide(); // ซ่อนหน้านี้
                    new ResetPasswordForm(email).ShowDialog(); // เปิดหน้าใหม่
                    this.Close(); // ปิดหน้านี้
                }
                else
                {
                    MessageBox.Show("ไม่พบอีเมลนี้");
                }
            }
        }

        private void back_home_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textemail_Enter(object sender, EventArgs e)
        {
            if (textemail.Text == "Email")
            {
                textemail.Text = "";
                textemail.ForeColor = Color.Black;
            }
        }

        private void textemail_Leave(object sender, EventArgs e)
        {
            if (textemail.Text == "")
            {
                textemail.Text = "Email";
                textemail.ForeColor = Color.Gray;
            }
        }

        private void ForgotPasswordForm_Load(object sender, EventArgs e)
        {
            textemail.Text = "Email";
            textemail.ForeColor = Color.Gray;
        }

    }
}
