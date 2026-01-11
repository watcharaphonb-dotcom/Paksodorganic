using MySql.Data.MySqlClient;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;


namespace test_pak
{
    public partial class OrderForm : Form
    {
        private readonly string _email; // เก็บอีเมลของผู้ใช้ที่ล็อกอิน
        public OrderForm(string email)
        {
            InitializeComponent();
            _email = email ?? ""; // กัน null

            this.Load += OrderForm_Load;
        }
        
        
        
        private void OrderForm_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_email))
            {
                MessageBox.Show("ไม่ได้รับค่าอีเมลจากหน้าก่อนหน้า");
                return;
            }

            label2.Text = _email; // ✅ โชว์อีเมลลูกค้าที่กำลังซื้อ
            LoadProfile(); // โหลดข้อมูลเมื่อเปิดหน้า
            LoadCartItems(); // ✅ โหลดสินค้าฝั่งขวา
        }

        private void LoadProfile()
        {
            try
            {
                using (var conn = new MySqlConnection(Program.ConnStr))
                {
                    conn.Open();
                    string sql = "SELECT name, number, address FROM user_info WHERE email=@em LIMIT 1";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@em", _email);
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                txtName.Text = rd["name"]?.ToString();
                                txtPhone.Text = rd["number"]?.ToString();
                                txtAddress.Text = rd["address"]?.ToString();
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบผู้ใช้ใน user_info ที่มีอีเมล: " + _email);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดข้อมูลล้มเหลว: " + ex.Message);
            }
        }

        private void btnSaveProfile_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_email))
                {
                    MessageBox.Show("ไม่พบอีเมลผู้ใช้ จึงอัปเดตไม่ได้");
                    return;
                }

                string name = txtName.Text.Trim();
                string phone = txtPhone.Text.Trim();
                string addr = txtAddress.Text.Trim();

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(addr))
                {
                    MessageBox.Show("กรุณากรอกข้อมูลให้ครบ (ชื่อ / เบอร์ / ที่อยู่)");
                    return;
                }

                if (!Regex.IsMatch(phone, @"^0\d{9}$"))
                {
                    MessageBox.Show("เบอร์โทรต้อง 10 หลัก และขึ้นต้นด้วย 0 (เช่น 0812345678)");
                    txtPhone.Focus();
                    return;
                }

                using (var conn = new MySqlConnection(Program.ConnStr))
                {
                    conn.Open();
                    string sql = @"UPDATE user_info
                                   SET name=@n, number=@p, address=@a
                                   WHERE email=@em";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@n", name);
                        cmd.Parameters.AddWithValue("@p", phone);
                        cmd.Parameters.AddWithValue("@a", addr);
                        cmd.Parameters.AddWithValue("@em", _email);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                            MessageBox.Show("บันทึกข้อมูลเรียบร้อย ✅");
                        else
                            MessageBox.Show("ไม่พบข้อมูลสำหรับอัปเดต");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("บันทึกไม่สำเร็จ: " + ex.Message);
            }
        }
    

    private void LoadCartItems()
        {
            // ให้ FlowLayout ดูสวย ๆ แนวตั้ง และเลื่อนสกรอลได้
            flowCartItems.SuspendLayout();
            flowCartItems.Controls.Clear();
            flowCartItems.FlowDirection = FlowDirection.TopDown;
            flowCartItems.WrapContents = false;
            flowCartItems.AutoScroll = true;

            decimal grandTotal = 0m;

            using (var conn = new MySqlConnection(Program.ConnStr))
            {
                conn.Open();

                // ดึงข้อมูลตะกร้า + รูปจาก products (ใช้ product_id ที่มีอยู่ใน cart)
                string sql = @"
            SELECT  c.product_id,
                    c.product_name,
                    c.quantity,
                    c.price,
                    c.totalprice,
                    p.image
            FROM cart c
            LEFT JOIN products p ON p.product_id = c.product_id
            WHERE c.user_email = @em
            ORDER BY c.product_id DESC;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@em", _email);
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            string name = rd["product_name"]?.ToString() ?? "";
                            int qty = rd["quantity"] != DBNull.Value ? Convert.ToInt32(rd["quantity"]) : 0;
                            decimal price = rd["price"] != DBNull.Value ? Convert.ToDecimal(rd["price"]) : 0m;
                            decimal total = rd["totalprice"] != DBNull.Value ? Convert.ToDecimal(rd["totalprice"]) : 0m;
                            byte[] imageBytes = rd["image"] as byte[];

                            grandTotal += total;

                            // === สร้างแถวรายการ (ไม่มีกรอบ) ===
                            var item = new Panel
                            {
                                Width = flowCartItems.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 8,
                                Height = 82,
                                BackColor = Color.Transparent,   // ไม่มีกรอบ/พื้น
                                Margin = new Padding(4, 4, 4, 10)
                            };

                            // รูปทางซ้าย
                            var pic = new PictureBox
                            {
                                Left = 0,
                                Top = 5,
                                Width = 70,
                                Height = 70,
                                SizeMode = PictureBoxSizeMode.Zoom,
                                BackColor = Color.Transparent
                            };
                            if (imageBytes != null && imageBytes.Length > 0)
                            {
                                using (var ms = new System.IO.MemoryStream(imageBytes))
                                    pic.Image = Image.FromStream(ms);
                            }
                            else
                            {
                                // ถ้าไม่มีรูป: ทำ placeholder ขาวล้วน
                                var ph = new Bitmap(70, 70);
                                using (var g = Graphics.FromImage(ph)) g.Clear(Color.White);
                                pic.Image = ph;
                            }
                            item.Controls.Add(pic);

                            int textLeft = pic.Right + 12;

                            // ชื่อสินค้า (ตัวหนา)
                            var lblName = new Label
                            {
                                AutoSize = false,
                                Left = textLeft,
                                Top = 5,
                                Width = item.Width - textLeft - 110,   // เว้นที่ให้ราคามุมขวา
                                Height = 22,
                                Text = name,
                                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                                ForeColor = Color.Black
                            };
                            item.Controls.Add(lblName);

                            // จำนวน + ราคา/หน่วย
                            var lblQtyPrice = new Label
                            {
                                AutoSize = false,
                                Left = textLeft,
                                Top = 32,
                                Width = item.Width - textLeft - 10,
                                Height = 20,
                                Text = $"x{qty}  ·  ฿{price:N2}/หน่วย",
                                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                                ForeColor = Color.DimGray
                            };
                            item.Controls.Add(lblQtyPrice);

                            // ราคาสุทธิ (ชิดขวา สีเขียว)
                            var lblLineTotal = new Label
                            {
                                AutoSize = true,
                                Top = 8,
                                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                                ForeColor = Color.FromArgb(0, 128, 0),
                                Text = $"฿{total:N2}",
                                Anchor = AnchorStyles.Top | AnchorStyles.Right
                            };
                            // จัดตำแหน่งให้ชิดขวา
                            lblLineTotal.Left = item.Width - lblLineTotal.PreferredWidth - 6;
                            item.Controls.Add(lblLineTotal);

                            // เส้นคั่นบาง ๆ (ไม่ใช่กรอบรายการ)
                            var sep = new Panel
                            {
                                Height = 1,
                                Width = item.Width,
                                Left = 0,
                                Top = item.Height - 1,
                                BackColor = Color.Gainsboro
                            };
                            item.Controls.Add(sep);

                            // ทำให้รีเลย์เอาต์ดีขึ้นเวลาขยาย pane
                            item.Resize += (s, ev) =>
                            {
                                lblName.Width = item.Width - textLeft - 110;
                                lblQtyPrice.Width = item.Width - textLeft - 10;
                                lblLineTotal.Left = item.Width - lblLineTotal.PreferredWidth - 6;
                                sep.Width = item.Width;
                            };

                            flowCartItems.Controls.Add(item);
                        }
                    }
                }
            }

            lblTotalPrice.Text = $"รวมทั้งหมด: ฿{grandTotal:N2}";
            flowCartItems.ResumeLayout();
        }


        private void btnConfirmOrder_Click_1(object sender, EventArgs e)
        {
            // 1️⃣ ดึงข้อมูลจาก cart ของลูกค้าคนนี้
            decimal total = 0;
            int newOrderId = 0;
            using (var conn = new MySqlConnection(Program.ConnStr))
            {
                conn.Open();

                // 2️⃣ รวมราคาทั้งหมดในตะกร้า
                string sumSql = "SELECT SUM(totalprice) FROM cart WHERE user_email=@em";
                using (var cmd = new MySqlCommand(sumSql, conn))
                {
                    cmd.Parameters.AddWithValue("@em", _email);
                    object obj = cmd.ExecuteScalar();
                    total = (obj == DBNull.Value) ? 0 : Convert.ToDecimal(obj);
                }

                // 3️⃣ เพิ่มออเดอร์ใหม่ในตาราง orders
                string insertSql = @"INSERT INTO orders (user_email, order_date, total_price, status)
                             VALUES (@em, NOW(), @total, 'pending')";
                using (var cmd = new MySqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@em", _email);
                    cmd.Parameters.AddWithValue("@total", total);
                    cmd.ExecuteNonQuery();

                    // ดึง id ของออเดอร์ที่เพิ่งเพิ่ม
                    newOrderId = (int)cmd.LastInsertedId;
                }

                // 4️⃣ ย้ายสินค้าจาก cart ไป order_items
                string copySql = @"INSERT INTO order_items (order_id, product_name, quantity, price, total_price)
                           SELECT @oid, product_name, quantity, price, totalprice FROM cart WHERE user_email=@em";
                using (var cmd = new MySqlCommand(copySql, conn))
                {
                    cmd.Parameters.AddWithValue("@oid", newOrderId);
                    cmd.Parameters.AddWithValue("@em", _email);
                    cmd.ExecuteNonQuery();
                }

            }

            MessageBox.Show("สั่งซื้อสำเร็จ! กรุณาชำระเงินต่อในขั้นตอนถัดไป");

            // 6️⃣ เปิดหน้า ScanQR พร้อมส่งอีเมลและ orderId ไปด้วย
            var scanForm = new scanqr(_email, (long)newOrderId);
            scanForm.FormClosed += (s, args) => this.Close();
            scanForm.Show();
            this.Hide();
        }

        private void lblTotalPrice_Click(object sender, EventArgs e)
        {

        }

        private void Back_Click(object sender, EventArgs e)
        {
            // สร้างหน้า Shop ใหม่ แล้วส่งอีเมลของผู้ใช้กลับไปด้วย (ถ้ามี)
            Shop shopForm = new Shop(_email);
            shopForm.Show();

            // ปิดหน้าปัจจุบัน
            this.Close();

        }
    }      
}