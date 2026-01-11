using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO; // ✅ เพิ่มเพื่อจัดการไฟล์รูปภาพ
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_pak
{
    public partial class AdmincustomerForm : Form
    {
        private const string ConnStr =
            "Server=localhost;Database=leafy_farm;Uid=root;Pwd=;CharSet=utf8mb4;SslMode=Preferred;";

        // ✅ ตัวแปรเก็บไฟล์รูปภาพปัจจุบัน (สำหรับบันทึกลง DB)
        private byte[] _currentImageBytes = null;

        public AdmincustomerForm()
        {
            InitializeComponent();
            dataGridViewCustomers.SelectionChanged += dataGridViewCustomers_SelectionChanged;

            // ✅ เชื่อมปุ่มอัปโหลดอัตโนมัติ (ถ้าตั้งชื่อปุ่มว่า btnUploadImage ในหน้า Design)
            if (btnUploadImage != null) btnUploadImage.Click += btnUploadImage_Click;

            LoadCustomers(); // โหลดข้อมูลเริ่มต้น
        }

        // =====================================================
        // โหลดข้อมูลลูกค้า + รองรับค้นหา (เพิ่ม profile_image)
        // =====================================================
        private void LoadCustomers(string keyword = null)
        {
            using (var conn = new MySqlConnection(ConnStr))
            {
                // ✅ เพิ่ม profile_image ใน Query
                var sb = new StringBuilder();
                sb.AppendLine(@"SELECT id, email, number AS phone, name, address, password, profile_image
                                FROM user_info
                                WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    sb.AppendLine(@"AND ( email    LIKE @kw
                                     OR number  LIKE @kw
                                     OR name    LIKE @kw
                                     OR address LIKE @kw )");
                }

                sb.AppendLine("ORDER BY id DESC;");

                using (var cmd = new MySqlCommand(sb.ToString(), conn))
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                        cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");

                    var dt = new DataTable();
                    using (var da = new MySqlDataAdapter(cmd)) da.Fill(dt);

                    if (!dt.Columns.Contains("password_mask"))
                        dt.Columns.Add("password_mask", typeof(string));
                    foreach (DataRow r in dt.Rows) r["password_mask"] = "••••••••";

                    var g = dataGridViewCustomers;
                    g.DataSource = null;
                    g.Columns.Clear();
                    g.AutoGenerateColumns = true;
                    g.ReadOnly = true;
                    g.EditMode = DataGridViewEditMode.EditProgrammatically;
                    g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    g.MultiSelect = false;
                    g.AllowUserToAddRows = false;
                    g.AllowUserToDeleteRows = false;
                    g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    g.DataSource = dt;

                    // ✅ ซ่อนคอลัมน์ที่ไม่จำเป็น (รหัสผ่าน และ รูปภาพ binary)
                    if (g.Columns.Contains("password")) g.Columns["password"].Visible = false;
                    if (g.Columns.Contains("profile_image")) g.Columns["profile_image"].Visible = false;

                    if (g.Rows.Count > 0)
                    {
                        g.ClearSelection();
                        g.Rows[0].Selected = true;
                        g.CurrentCell = g.Rows[0].Cells["email"];
                        // ข้อมูลจะถูกโหลดเข้า TextBox/PictureBox ผ่าน SelectionChanged
                    }
                    else
                    {
                        ClearInputs();
                    }
                }
            }
        }

        // =====================================================
        // ✅ ปุ่มเลือกรูปภาพ (btnUploadImage)
        // =====================================================
        private void btnUploadImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // แสดงตัวอย่างใน PictureBox
                    if (picProfile != null)
                    {
                        picProfile.Image = Image.FromFile(ofd.FileName);
                        picProfile.SizeMode = PictureBoxSizeMode.Zoom; // หรือ StretchImage ตามชอบ
                    }

                    // เก็บไฟล์เป็น byte array เตรียมบันทึก
                    _currentImageBytes = File.ReadAllBytes(ofd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("อัปโหลดรูปไม่สำเร็จ: " + ex.Message);
                }
            }
        }

        // =====================================================
        // ปุ่มเพิ่มข้อมูล (เพิ่มการบันทึกรูปภาพ)
        // =====================================================
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInputsForUpsert(requirePassword: true, out var msg))
            {
                MessageBox.Show(msg, "แจ้งเตือน");
                return;
            }

            using (var conn = new MySqlConnection(ConnStr))
            // ✅ เพิ่ม profile_image ใน INSERT
            using (var cmd = new MySqlCommand(
                @"INSERT INTO user_info (email, number, name, address, password, profile_image)
                  VALUES (@e, @n, @name, @a, @p, @img);", conn))
            {
                cmd.Parameters.AddWithValue("@e", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@n", txtPhone.Text.Trim());
                cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@a", txtAddress.Text.Trim());
                cmd.Parameters.AddWithValue("@p", HashPassword(txtPassword.Text));

                // ✅ ส่งข้อมูลรูปภาพ
                if (_currentImageBytes != null)
                    cmd.Parameters.AddWithValue("@img", _currentImageBytes);
                else
                    cmd.Parameters.AddWithValue("@img", DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadCustomers(txtSearch?.Text?.Trim());
            ClearInputs();
            MessageBox.Show("เพิ่มลูกค้าเรียบร้อยแล้ว");
        }

        // =====================================================
        // ปุ่มอัปเดตข้อมูล (เพิ่มการอัปเดตรูปภาพ)
        // =====================================================
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtId.Text, out var id))
            {
                MessageBox.Show("กรุณาเลือกแถวที่จะปรับปรุงข้อมูล", "แจ้งเตือน");
                return;
            }

            if (!ValidateInputsForUpsert(requirePassword: false, out var msg))
            {
                MessageBox.Show(msg, "แจ้งเตือน");
                return;
            }

            bool changePw = !string.IsNullOrWhiteSpace(txtPassword.Text);

            // ✅ เพิ่ม profile_image ใน UPDATE
            string sql = @"UPDATE user_info
                           SET email=@e, number=@n, name=@name, address=@a, profile_image=@img"
                         + (changePw ? ", password=@p" : "")
                         + " WHERE id=@id;";

            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@e", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@n", txtPhone.Text.Trim());
                cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@a", txtAddress.Text.Trim());
                cmd.Parameters.AddWithValue("@id", id);

                if (changePw) cmd.Parameters.AddWithValue("@p", HashPassword(txtPassword.Text));

                // ✅ ส่งข้อมูลรูปภาพ (ใช้รูปปัจจุบัน หรือรูปใหม่ที่อัปโหลด)
                if (_currentImageBytes != null)
                    cmd.Parameters.AddWithValue("@img", _currentImageBytes);
                else
                    cmd.Parameters.AddWithValue("@img", DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadCustomers(txtSearch?.Text?.Trim());
            ClearInputs();
            MessageBox.Show("ปรับปรุงข้อมูลเรียบร้อยแล้ว");
        }

        // =====================================================
        // ปุ่มลบข้อมูล
        // =====================================================
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtId.Text, out var id))
            {
                MessageBox.Show("กรุณาเลือกแถวที่จะลบ", "แจ้งเตือน");
                return;
            }

            var confirm = MessageBox.Show("ต้องการลบลูกค้าคนนี้หรือไม่?", "ยืนยัน",
                                          MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand("DELETE FROM user_info WHERE id=@id;", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadCustomers(txtSearch?.Text?.Trim());
            ClearInputs();
            MessageBox.Show("ลบข้อมูลเรียบร้อยแล้ว");
        }

        // =====================================================
        // ปุ่มเคลียร์ข้อมูล (เพิ่มเคลียร์รูปภาพ)
        // =====================================================
        private void btnClear_Click(object sender, EventArgs e) => ClearInputs();

        private void ClearInputs()
        {
            txtId.Text = "";
            txtEmail.Text = "";
            txtPhone.Text = "";
            txtName.Text = "";
            txtAddress.Text = "";
            txtPassword.Text = "";

            // ✅ เคลียร์รูปภาพ
            _currentImageBytes = null;
            if (picProfile != null) picProfile.Image = null;
        }

        // =====================================================
        // ปุ่มค้นหา / พิมพ์ Enter เพื่อค้นหา
        // =====================================================
        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadCustomers(txtSearch?.Text?.Trim());
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                LoadCustomers(txtSearch?.Text?.Trim());
            }
        }

        // =====================================================
        // เมื่อคลิกเลือกใน DataGridView (โหลดรูปภาพด้วย)
        // =====================================================
        private void dataGridViewCustomers_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewCustomers.CurrentRow?.DataBoundItem is DataRowView drv)
            {
                txtId.Text = drv["id"]?.ToString();
                txtEmail.Text = drv["email"]?.ToString();
                txtPhone.Text = drv["phone"]?.ToString();
                txtName.Text = drv["name"]?.ToString();
                txtAddress.Text = drv["address"]?.ToString();
                txtPassword.Text = ""; // ไม่โชว์รหัสจริง

                // ✅ โหลดรูปภาพจาก DB มาแสดง
                if (drv["profile_image"] != DBNull.Value)
                {
                    byte[] imgData = (byte[])drv["profile_image"];
                    _currentImageBytes = imgData; // เก็บไว้เผื่อกด Update โดยไม่เปลี่ยนรูป

                    if (picProfile != null)
                    {
                        using (var ms = new MemoryStream(imgData))
                        {
                            picProfile.Image = Image.FromStream(ms);
                            picProfile.SizeMode = PictureBoxSizeMode.Zoom;
                        }
                    }
                }
                else
                {
                    _currentImageBytes = null;
                    if (picProfile != null) picProfile.Image = null; // หรือใส่รูป Default
                }
            }
        }

        // =====================================================
        // ฟังก์ชันช่วยตรวจอินพุต + แฮชรหัสผ่าน
        // =====================================================
        private bool ValidateInputsForUpsert(bool requirePassword, out string msg)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text) ||
                string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                msg = "กรุณากรอก Email, Phone, Name และ Address ให้ครบ";
                return false;
            }
            if (requirePassword && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                msg = "เพิ่มผู้ใช้ใหม่ต้องกรอกรหัสผ่าน";
                return false;
            }
            msg = null;
            return true;
        }

        private static string HashPassword(string plain) =>
            BCrypt.Net.BCrypt.HashPassword(plain.Trim(), workFactor: 12);

        private void btnHome_Click(object sender, EventArgs e)
        {
            this.Hide();
            Home home = new Home();
            home.ShowDialog();
            this.Show();
        }

        private void btnSalesReport_Click(object sender, EventArgs e)
        {
            this.Hide();
            AdminForm adminForm = new AdminForm();  // หน้า Sales Report
            adminForm.ShowDialog();
            this.Show();
        }

        private void btnCustomer_Click(object sender, EventArgs e)
        {
            this.Hide();
            AdmincustomerForm customerForm = new AdmincustomerForm();
            customerForm.ShowDialog();
            this.Show();
        }

        private void btnProduct_Click(object sender, EventArgs e)
        {
            this.Hide();
            AdminproductsForm productForm = new AdminproductsForm();
            productForm.ShowDialog();
            this.Show();
        }

        private void txtEmail_TextChanged(object sender, EventArgs e) { }
        private void txtSearch_TextChanged(object sender, EventArgs e) { }
        private void AdmincustomerForm_Load(object sender, EventArgs e) { }
    }
}