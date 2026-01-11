using System;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace test_pak
{
    public partial class SigninForm : Form
  
    {

        private Home mainForm1;
        private readonly Home mainForm;
        public SigninForm(Home form)
        {
            InitializeComponent();
            mainForm = form;



        }
        public SigninForm()
        {
            InitializeComponent();
        }
        private void back_home_Click(object sender, EventArgs e)
        {
        mainForm?.Show();
        this.Close();
        }
        private bool IsValidEmail(string email)
        {
            // เช็คฟอร์แมตอีเมลแบบง่าย ๆ
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        // เปิด/ปิดปุ่ม Log in ตามการกรอกข้อมูล
        private void UpdateLoginButton()
        {
            bool emailOk = !string.IsNullOrWhiteSpace(textemail.Text) && textemail.Text != "Email";
            bool passOk = !string.IsNullOrWhiteSpace(textpass.Text) && textpass.Text != "Password";

            Log_in.Enabled = emailOk && passOk;   // ต้องกรอกครบทั้งสองช่อง
        }


        private void textpass_Leave(object sender, EventArgs e)
        {
            // ถ้าผู้ใช้ลบหมด → ใส่คำว่า Password กลับมา
            if (string.IsNullOrWhiteSpace(textpass.Text))
            {
                textpass.UseSystemPasswordChar = false;
                textpass.Text = "Password";
                textpass.ForeColor = Color.Gray;
            }
        }
        private void textpass_Enter(object sender, EventArgs e)
        {
            // ถ้าขึ้นคำว่า Password อยู่ ให้ลบออก
            if (textpass.Text == "Password")
            {
                textpass.Text = "";
                textpass.ForeColor = Color.Black;
                textpass.UseSystemPasswordChar = true; // ซ่อนตัวอักษร
            }
        }
        private void textpass_TextChanged(object sender, EventArgs e)
        {

        }

        private void Log_in_Click(object sender, EventArgs e)
        {
            // 1) อ่านค่าจากช่องกรอก
            string email = (textemail.Text == "Email") ? "" : textemail.Text.Trim();
            string password = (textpass.Text == "Password") ? "" : textpass.Text;

            // 2) ตรวจข้อมูลเบื้องต้น
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("กรุณากรอกอีเมลและรหัสผ่านให้ครบ", "ข้อมูลไม่ครบ",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!IsValidEmail(email))
            {
                MessageBox.Show("รูปแบบอีเมลไม่ถูกต้อง", "อีเมลไม่ถูกต้อง",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textemail.Focus();
                return;
            }
            if (password.Length < 6)
            {
                MessageBox.Show("รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร", "รหัสผ่านสั้นเกินไป",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textpass.Focus();
                return;
            }

            // 3) เช็คกรณีพิเศษ: แอดมินล็อกอิน (ฮาร์ดโค้ด)
            if (email.Equals("adminbim@gmail.com", StringComparison.OrdinalIgnoreCase)
                && password == "123456")
            {
                // แจ้งเตือนเล็กน้อยว่าเข้าระบบแอดมิน
                MessageBox.Show("ยินดีต้อนรับผู้ดูแลระบบ (Admin)");

                // ไปหน้า Adminform
                this.Hide();                // ซ่อนหน้าล็อกอิน (ให้ดูเรียบร้อย)
                var adminForm = new AdminForm();  // ชื่อคลาสควรตรงกับไฟล์ Adminform.cs
                adminForm.FormClosed += (s, args) => this.Close(); // ปิดโปรแกรมเมื่อปิดหน้าถัดไป
                adminForm.Show();

                return; // จบที่นี่ ไม่ต้องเช็คฐานข้อมูล
            }

            // 4) ไม่ใช่แอดมิน → ล็อกอินผู้ใช้ปกติ (เช็คฐานข้อมูลเหมือนเดิม)
            try
            {
                using (var conn = new MySqlConnection(Program.ConnStr))
                {
                    conn.Open();

                    string sql = "SELECT name, status FROM user_info WHERE email=@e AND password=@p LIMIT 1";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@e", email);
                        cmd.Parameters.AddWithValue("@p", password); // ถ้าเก็บแบบ hash ให้เปลี่ยนเป็นเทียบ hash

                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                string name = rd["name"].ToString();
                                string status = rd["status"].ToString();

                                MessageBox.Show($"ล็อกอินสำเร็จ! ยินดีต้อนรับ {name} ({status})");

                                // ไปหน้า Shop (ของเดิม)
                                this.Hide();
                                Shop shopForm = new Shop(email);
                                shopForm.FormClosed += (s, args) => this.Close();
                                shopForm.Show();
                            }
                            else
                            {
                                MessageBox.Show("อีเมลหรือรหัสผ่านไม่ถูกต้อง");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการเชื่อมต่อฐานข้อมูล\n" + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void textemail_Enter(object sender, EventArgs e)
        {
            // ถ้าขึ้นคำว่า Email อยู่ ให้ลบออก
            if (textemail.Text == "Email")
            {
                textemail.Text = "";
                textemail.ForeColor = Color.Black;
            }
        }

        private void textemail_TextChanged(object sender, EventArgs e)
        {
            // ตอนนี้ยังไม่ต้องทำอะไรในนี้ก็ได้
        }

        private void textemail_Leave(object sender, EventArgs e)
        {
            // ถ้าผู้ใช้ลบหมด → ใส่คำว่า Email กลับมา
            if (string.IsNullOrWhiteSpace(textemail.Text))
            {
                textemail.Text = "Email";
                textemail.ForeColor = Color.Gray;
            }
        }




       

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void SigninForm_Load_1(object sender, EventArgs e)
        {

        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            ForgotPasswordForm form = new ForgotPasswordForm();
            form.ShowDialog();
            this.Show();
        }
    }
}
