using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace test_pak
{
    public partial class UserProfile : Form
    {
        // ตั้งค่าการเชื่อมต่อฐานข้อมูล
        private readonly string connStr = "Server=localhost;Database=leafy_farm;Uid=root;Pwd=;";
        private readonly string currentUserEmail;

        // ตัวแปรสำหรับรูปภาพ
        private byte[] _currentImageBytes = null;
        private bool _isImageChanged = false;

        public UserProfile(string email)
        {
            InitializeComponent();
            this.currentUserEmail = email;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        // ========================================================
        // 🎯 EVENT HANDLERS (เชื่อมกับชื่อปุ่มที่คุณตั้งเอง)
        // ========================================================

        // 1. โหลดข้อมูลเมื่อเปิดหน้าจอ (Form Load)
        private void UserProfile_Load(object sender, EventArgs e)
        {
            // ✅ เพิ่มบรรทัดนี้: ล็อกช่องอีเมลไม่ให้พิมพ์แก้ไขได้
            if (textemail != null)
            {
                textemail.ReadOnly = true;
                textemail.BackColor = SystemColors.Control; // (ตัวเลือกเสริม) เปลี่ยนสีพื้นหลังให้ดูเหมือนถูกล็อก
            }
            LoadUserData();
        }

        // 2. ปุ่มอัปโหลดรูป (button)
        private void button_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    picture.Image = Image.FromFile(ofd.FileName);
                    picture.SizeMode = PictureBoxSizeMode.Zoom;
                    _currentImageBytes = File.ReadAllBytes(ofd.FileName);
                    _isImageChanged = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("อัปโหลดรูปไม่สำเร็จ: " + ex.Message);
                }
            }
        }

        // 3. ปุ่มบันทึกข้อมูล (button2)
        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textname.Text))
            {
                MessageBox.Show("กรุณากรอกชื่อ");
                return;
            }

            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql;

                    // ตรวจสอบว่ามีการเปลี่ยนรูปภาพหรือไม่
                    if (_isImageChanged && _currentImageBytes != null)
                    {
                        sql = @"UPDATE user_info 
                                SET name=@name, number=@number, address=@address, password=@password, profile_image=@img 
                                WHERE email=@email";
                    }
                    else
                    {
                        sql = @"UPDATE user_info 
                                SET name=@name, number=@number, address=@address, password=@password 
                                WHERE email=@email";
                    }

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        // ใช้ชื่อ TextBox ตามที่คุณตั้ง
                        cmd.Parameters.AddWithValue("@name", textname.Text);
                        cmd.Parameters.AddWithValue("@number", textphone.Text);
                        cmd.Parameters.AddWithValue("@address", textaddress.Text);
                        cmd.Parameters.AddWithValue("@password", textpassword.Text);
                        cmd.Parameters.AddWithValue("@email", currentUserEmail); // ใช้อีเมลเดิมเป็นตัวระบุ

                        if (_isImageChanged && _currentImageBytes != null)
                        {
                            cmd.Parameters.AddWithValue("@img", _currentImageBytes);
                        }

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("บันทึกข้อมูลเรียบร้อยแล้ว ✅", "สำเร็จ");
                        }
                        else
                        {
                            MessageBox.Show("ไม่พบข้อมูลผู้ใช้ หรือไม่มีการเปลี่ยนแปลง", "แจ้งเตือน");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("บันทึกข้อมูลล้มเหลว: " + ex.Message);
            }
        }

        // 4. ปุ่มย้อนกลับ (back_home)
        private void back_home_Click(object sender, EventArgs e)
        {
            try
            {
                var shop = new Shop(currentUserEmail);
                shop.Show();
                this.Close();
            }
            catch
            {
                this.Close();
            }
        }

        // 5. ปุ่มลบบัญชี (button3)
        private void button3_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show(
                "คุณแน่ใจหรือไม่ว่าจะลบบัญชีนี้ถาวร? \nการกระทำนี้ไม่สามารถย้อนกลับได้!",
                "ยืนยันการลบบัญชี",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    using (var conn = new MySqlConnection(connStr))
                    {
                        conn.Open();

                        // ลบตะกร้าสินค้าก่อน
                        string sqlCart = "DELETE FROM cart WHERE user_email = @email";
                        using (var cmdCart = new MySqlCommand(sqlCart, conn))
                        {
                            cmdCart.Parameters.AddWithValue("@email", currentUserEmail);
                            cmdCart.ExecuteNonQuery();
                        }

                        // ลบข้อมูลผู้ใช้
                        string sqlUser = "DELETE FROM user_info WHERE email = @email";
                        using (var cmdUser = new MySqlCommand(sqlUser, conn))
                        {
                            cmdUser.Parameters.AddWithValue("@email", currentUserEmail);
                            int result = cmdUser.ExecuteNonQuery();

                            if (result > 0)
                            {
                                MessageBox.Show("ลบบัญชีเรียบร้อยแล้ว", "สำเร็จ");
                                try
                                {
                                    Home home = new Home(); // กลับไปหน้าแรก
                                    home.Show();
                                }
                                catch { Application.Exit(); }
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("ไม่สามารถลบบัญชีได้ (ไม่พบผู้ใช้)", "ผิดพลาด");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาดในการลบบัญชี: " + ex.Message);
                }
            }
        }

        // ==========================================
        // 💾 ฟังก์ชันโหลดข้อมูล
        // ==========================================
        private void LoadUserData()
        {
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT name, number, address, email, password, profile_image 
                                   FROM user_info WHERE email = @email";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", currentUserEmail);
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                // ใส่ข้อมูลลง TextBox ตามชื่อที่คุณตั้ง
                                if (textname != null) textname.Text = rd["name"].ToString();
                                if (textphone != null) textphone.Text = rd["number"].ToString();
                                if (textaddress != null) textaddress.Text = rd["address"].ToString();
                                if (textemail != null) textemail.Text = rd["email"].ToString();
                                if (textpassword != null) textpassword.Text = rd["password"].ToString();

                                // โหลดรูปภาพ
                                if (rd["profile_image"] != DBNull.Value && picture != null)
                                {
                                    byte[] imgData = (byte[])rd["profile_image"];
                                    using (var ms = new MemoryStream(imgData))
                                    {
                                        picture.Image = Image.FromStream(ms);
                                        picture.SizeMode = PictureBoxSizeMode.Zoom;
                                    }
                                    _currentImageBytes = imgData;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดข้อมูลไม่สำเร็จ: " + ex.Message);
            }
        }

        private void textname_TextChanged(object sender, EventArgs e)
        {

        }

        private void picture_Click(object sender, EventArgs e)
        {

        }
    }
}