using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace test_pak
{
    public partial class ResetPasswordForm : Form
    {
        string userEmail; // ตัวแปรรับอีเมล

        // Constructor รับค่าอีเมล
        public ResetPasswordForm(string email)
        {
            InitializeComponent();
            this.userEmail = email;
        }

        // ปุ่ม Save
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textNewPass.Text) || string.IsNullOrWhiteSpace(textConfirmPass.Text))
            {
                MessageBox.Show("กรุณากรอกรหัสผ่านให้ครบ");
                return;
            }

            if (textNewPass.Text != textConfirmPass.Text)
            {
                MessageBox.Show("รหัสผ่านยืนยันไม่ตรงกัน");
                return;
            }

            try
            {
                using (var conn = new MySqlConnection(Program.ConnStr))
                {
                    conn.Open();
                    string sql = "UPDATE user_info SET password = @p WHERE email = @e";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@p", textNewPass.Text);
                        cmd.Parameters.AddWithValue("@e", userEmail);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("เปลี่ยนรหัสผ่านสำเร็จ! กรุณาล็อกอินใหม่");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // ========================================================
        //  👇 ใส่ 2 ตัวนี้เพิ่มเข้าไป เพื่อแก้ Error สีแดงครับ 👇
        // ========================================================

        private void back_home_Click(object sender, EventArgs e)
        {
            this.Close(); // กดปุ่ม Back แล้วปิดหน้านี้
        }

        private void textemail_TextChanged(object sender, EventArgs e)
        {
            // ปล่อยว่างไว้ก็ได้ครับ (เพื่อแก้ Error เฉยๆ)
        }

        private void textNewPass_Enter(object sender, EventArgs e)
        {
            if (textNewPass.Text == "New Password")
            {
                textNewPass.Text = ""; // ลบคำว่า New Password ออก
                textNewPass.ForeColor = Color.Black; // เปลี่ยนสีตัวอักษรเป็นสีดำ
            }
        }

        private void textNewPass_Leave(object sender, EventArgs e)
        {
            if (textNewPass.Text == "") // ถ้าไม่ได้กรอกอะไร
            {
                textNewPass.Text = "New Password"; // ใส่คำเดิมกลับไป
                textNewPass.ForeColor = Color.Gray; // เปลี่ยนกลับเป็นสีเทา
            }
        }


        private void ResetPasswordForm_Load(object sender, EventArgs e)
        {
            // 1. บังคับตั้งค่าเริ่มต้นให้ตรงกับโค้ดเป๊ะๆ
            textNewPass.Text = "New Password";
            textNewPass.ForeColor = Color.Gray;

            // 2. ตรงนี้สำคัญ! บังคับให้เป็นคำว่า "Confirm Password"
            textConfirmPass.Text = "Confirm Password";
            textConfirmPass.ForeColor = Color.Gray;
        }

        private void textConfirmPass_TextChanged(object sender, EventArgs e)
        {

        }

        private void textConfirmPass_Enter(object sender, EventArgs e)
        {
            if (textConfirmPass.Text == "Confirm Password")
            {
                textConfirmPass.Text = "";
                textConfirmPass.ForeColor = Color.Black;
            }
        }

        private void textConfirmPass_Leave(object sender, EventArgs e)
        {
            if (textConfirmPass.Text == "")
            {
                textConfirmPass.Text = "Confirm Password";
                textConfirmPass.ForeColor = Color.Gray;
            }
        }
    }
}