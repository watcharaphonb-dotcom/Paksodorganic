using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace test_pak
{
    public partial class SignupForm : Form
    {
        // ตั้งค่า Connection String ให้เหมือนหน้าอื่นๆ
        private readonly string connStr = "Server=localhost;Database=leafy_farm;Uid=root;Pwd=;CharSet=utf8mb4;SslMode=Preferred;";

        // ตัวแปรเก็บไฟล์รูปภาพ
        private byte[] _profileImageBytes = null;

        public SignupForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void SignupForm_Load(object sender, EventArgs e)
        {
            // ตั้งค่า Placeholder
            SetupPlaceholder(textname, "Name");
            SetupPlaceholder(textphone, "Phone");
            SetupPlaceholder(textaddress, "Address");
            SetupPlaceholder(textemail, "Email");
            SetupPlaceholder(textpassword, "Password", isPassword: true);

            textphone.MaxLength = 10;
            textpassword.MaxLength = 64;

            textname.KeyPress += Name_KeyPress;
            textphone.KeyPress += Phone_KeyPress;

            // เชื่อมปุ่มอัปโหลด (ถ้ามีปุ่ม btnUpload ในหน้า Design)
           // if (btnUpload != null) btnUpload.Click += BtnUpload_Click;
        }

        // ==========================================
        // 📸 ส่วนจัดการรูปภาพ (เพิ่มใหม่)
        // ==========================================
        private void BtnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // แสดงตัวอย่างรูป
                    if (picProfile != null)
                    {
                        picProfile.Image = Image.FromFile(ofd.FileName);
                        picProfile.SizeMode = PictureBoxSizeMode.StretchImage; // หรือ Zoom
                    }

                    // แปลงไฟล์เป็น Byte array เพื่อเตรียมบันทึกลง DB
                    _profileImageBytes = File.ReadAllBytes(ofd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("อัปโหลดรูปไม่สำเร็จ: " + ex.Message);
                }
            }
        }

        // ==========================================
        // ✅ ปุ่มสมัครสมาชิก (แก้ไข SQL)
        // ==========================================
        private void Sign_up_Click(object sender, EventArgs e)
        {
            // 1. อ่านค่าจากช่องกรอก
            string name = (textname.Tag as string) == "PH" ? "" : textname.Text.Trim();
            string phone = (textphone.Tag as string) == "PH" ? "" : textphone.Text.Trim();
            string address = (textaddress.Tag as string) == "PH" ? "" : textaddress.Text.Trim();
            string email = (textemail.Tag as string) == "PH" ? "" : textemail.Text.Trim();
            string password = (textpassword.Tag as string) == "PH" ? "" : textpassword.Text;
            string status = "customer";

            // 2. ตรวจสอบข้อมูลว่าง
            if (name == "" || phone == "" || address == "" || email == "" || password == "")
            {
                MessageBox.Show("กรุณากรอกข้อมูลให้ครบทุกช่อง", "แจ้งเตือน");
                return;
            }

            // 3. ตรวจสอบรูปแบบ (Regex)
            if (!_nameOk.IsMatch(name))
            {
                MessageBox.Show("ชื่อให้ใช้เฉพาะภาษาไทย/อังกฤษ และเว้นวรรคเท่านั้น");
                textname.Focus(); return;
            }
            if (!_phoneOk.IsMatch(phone))
            {
                MessageBox.Show("เบอร์โทรต้องมี 10 ตัว และขึ้นต้นด้วย 0");
                textphone.Focus(); return;
            }
            if (!_emailOk.IsMatch(email))
            {
                MessageBox.Show("รูปแบบอีเมลไม่ถูกต้อง");
                textemail.Focus(); return;
            }
            if (password.Length < 6)
            {
                MessageBox.Show("รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร");
                textpassword.Focus(); return;
            }

            // 4. บันทึกลงฐานข้อมูล
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    // 4.1 เช็คอีเมลซ้ำ
                    using (var check = new MySqlCommand("SELECT COUNT(*) FROM user_info WHERE email=@e", conn))
                    {
                        check.Parameters.AddWithValue("@e", email);
                        int exists = Convert.ToInt32(check.ExecuteScalar());
                        if (exists > 0)
                        {
                            MessageBox.Show("อีเมลนี้ถูกใช้สมัครแล้ว", "แจ้งเตือน");
                            return;
                        }
                    }

                    // 4.2 บันทึกข้อมูล (เพิ่ม profile_image)
                    string sql = @"INSERT INTO user_info (name, number, address, email, password, status, profile_image)
                                   VALUES (@n, @num, @addr, @e, @p, @s, @img)";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@n", name);
                        cmd.Parameters.AddWithValue("@num", phone);
                        cmd.Parameters.AddWithValue("@addr", address);
                        cmd.Parameters.AddWithValue("@e", email);
                        cmd.Parameters.AddWithValue("@p", password);
                        cmd.Parameters.AddWithValue("@s", status);

                        // ใส่รูปภาพ (ถ้าไม่ได้อัปโหลด จะเป็น null)
                        if (_profileImageBytes != null)
                        {
                            cmd.Parameters.AddWithValue("@img", _profileImageBytes);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@img", DBNull.Value);
                        }

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            MessageBox.Show("สมัครสมาชิกสำเร็จ!", "สำเร็จ");

                            // ไปหน้า Signin
                            try
                            {
                                var signin = new SigninForm();
                                signin.Show();
                                this.Close();
                            }
                            catch { this.Close(); }
                        }
                        else
                        {
                            MessageBox.Show("บันทึกข้อมูลไม่สำเร็จ", "ผิดพลาด");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด:\n" + ex.Message, "Error");
            }
        }

        // ==========================================
        // 🛠️ Helper Methods (Validation & UI)
        // ==========================================
        private static readonly Regex _nameOk = new Regex(@"^[A-Za-z\u0E00-\u0E7F ]+$");
        private static readonly Regex _phoneOk = new Regex(@"^0\d{9}$");
        private static readonly Regex _emailOk = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private void Name_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            bool ok = char.IsControl(c) || char.IsWhiteSpace(c) || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '\u0E00' && c <= '\u0E7F');
            if (!ok) e.Handled = true;
        }

        private void Phone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        }

        private void SetupPlaceholder(TextBox tb, string placeholder, bool isPassword = false)
        {
            Action ShowPH = () => {
                tb.Tag = "PH";
                tb.ForeColor = Color.Gray;
                tb.Text = placeholder;
                if (isPassword) tb.UseSystemPasswordChar = false;
            };

            Action HidePH = () => {
                tb.Tag = null;
                tb.ForeColor = Color.Black;
                tb.Clear();
                if (isPassword) tb.UseSystemPasswordChar = true;
            };

            tb.Enter += (s, e) => { if ((tb.Tag as string) == "PH") HidePH(); };
            tb.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(tb.Text)) ShowPH(); };
            ShowPH();
        }

        // ปุ่มย้อนกลับ
        private void back_home_Click_1(object sender, EventArgs e)
        {
            try
            {
                Home homeForm = new Home();
                homeForm.Show();
                this.Close();
            }
            catch { this.Close(); }
        }

        // ฟังก์ชันเปล่ากัน Error
        private void button1_Click(object sender, EventArgs e) { }
        private void textphon_TextChanged(object sender, EventArgs e) { }
        private void textname_TextChanged(object sender, EventArgs e) { }
        private void textaddress_TextChanged(object sender, EventArgs e) { }
        private void textpassword_TextChanged(object sender, EventArgs e) { }
    }
}