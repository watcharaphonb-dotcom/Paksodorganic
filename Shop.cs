using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_pak
{
    public partial class Shop : Form
    {
        private readonly string connStr = "Server=localhost;Database=leafy_farm;Uid=root;Pwd=;";
        private readonly string userEmail;

        private Label cartBadge;
        private int cartCount = 0;

        public Shop(string email)
        {
            InitializeComponent();
            this.userEmail = email;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void Shop_Load(object sender, EventArgs e)
        {
            if (label2 != null) label2.Text = userEmail;

            // 1. โหลดข้อมูล
            LoadCategories(); // ดึงหมวดหมู่จาก DB
            LoadProducts();   // ดึงสินค้าทั้งหมด

            // 2. Setup ตะกร้า
            SetupCartBadge();
            LoadCartCountFromDb();
            UpdateCartBadge();

            // 3. ผูก Event ให้ทำงาน (เผื่อยังไม่ได้ผูกใน Designer)
            // ค้นหา
            if (btnSearch != null) btnSearch.Click += BtnSearch_Click;
            if (txtSearch != null) txtSearch.KeyDown += TxtSearch_KeyDown;
            if (cmbCategory != null) cmbCategory.SelectedIndexChanged += CmbCategory_SelectedIndexChanged;

            // ปุ่มอื่นๆ (ถ้ายังไม่ผูก)
            //if (btnHistory != null) btnHistory.Click += BtnHistory_Click;
            //if (picUserIcon != null) picUserIcon.Click += PicUserIcon_Click;
            //if (btnCart != null) btnCart.Click += BtnCart_Click;

            // ปรับตำแหน่ง Badge
            this.Resize += (s, ev) => UpdateCartBadgePosition();
            if (btnCart != null) btnCart.LocationChanged += (s, ev) => UpdateCartBadgePosition();
        }

        // ปุ่มค้นหา
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        // กด Enter ในช่องค้นหา
        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplyFilter();
                e.SuppressKeyPress = true; // ปิดเสียงติ๊ง
            }
        }

        // เลือกหมวดหมู่
        private void CmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        // ไปหน้าประวัติ
        private void BtnHistory_Click(object sender, EventArgs e)
        {
            try
            {
                OrderHistory history = new OrderHistory(userEmail);
                history.Show();
                this.Hide();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        // ไปหน้าโปรไฟล์
        private void PicUserIcon_Click(object sender, EventArgs e)
        {
            try
            {
                UserProfile profile = new UserProfile(userEmail);
                profile.Show();
                this.Hide();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        // ไปหน้าตะกร้า
        private void BtnCart_Click(object sender, EventArgs e)
        {
            try
            {
                var cartForm = new cart(userEmail);
                cartForm.FormClosed += (s, args) => {
                    this.Show();
                    LoadCartCountFromDb();
                    UpdateCartBadge();
                };
                cartForm.Show();
                this.Hide();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        // ปุ่มย้อนกลับ (เผื่อมี)
        private void Back_Click(object sender, EventArgs e)
        {
            try
            {
                Home home = new Home();
                home.Show();
                this.Close();
            }
            catch { this.Close(); }
        }

        // ฟังก์ชันเก่าที่ค้างใน Designer (เก็บไว้กัน Error)
        private void button2_Click(object sender, EventArgs e) => BtnCart_Click(sender, e);
        private void label1_Click(object sender, EventArgs e) { }
        private void flowProducts_Paint(object sender, PaintEventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }

        // =============================================
        // 🛍️ LOGIC การโหลดสินค้า & หมวดหมู่ (แก้ไขใหม่)
        // =============================================

        // ✅ แก้ไข: ดึงหมวดหมู่จาก Database โดยตรง
        private void LoadCategories()
        {
            if (cmbCategory == null) return;

            // เก็บหมวดหมู่ปัจจุบันไว้ก่อน (ถ้ามี) เพื่อเลือกกลับหลังโหลดเสร็จ
            string currentSelection = cmbCategory.SelectedItem?.ToString();

            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("ทั้งหมด"); // ตัวเลือกแรกเสมอ

            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    // ดึงชื่อหมวดหมู่ที่ไม่ซ้ำกันจากตารางสินค้า
                    string sql = "SELECT DISTINCT category FROM products WHERE category IS NOT NULL AND category != '' ORDER BY category";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                cmbCategory.Items.Add(rd["category"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดหมวดหมู่ไม่สำเร็จ: " + ex.Message);
            }

            // เลือกค่าเดิม หรือเลือก "ทั้งหมด" เป็นค่าเริ่มต้น
            if (!string.IsNullOrEmpty(currentSelection) && cmbCategory.Items.Contains(currentSelection))
            {
                cmbCategory.SelectedItem = currentSelection;
            }
            else
            {
                cmbCategory.SelectedIndex = 0;
            }
        }

        // ฟังก์ชันกรองสินค้า (ค้นหา + หมวดหมู่)
        private void ApplyFilter()
        {
            if (txtSearch == null || cmbCategory == null) return;

            string keyword = txtSearch.Text.Trim();
            string category = cmbCategory.SelectedItem?.ToString();

            if (category == "ทั้งหมด") category = null; // ถ้าเลือกทั้งหมด ให้ค่าเป็น null เพื่อดึงทุกหมวด

            LoadProducts(keyword, category);
        }

        private void LoadProducts(string keyword = "", string category = null)
        {
            if (flowProducts == null) return;
            flowProducts.Controls.Clear();

            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    var sql = new StringBuilder();
                    sql.Append("SELECT product_id, name, category, weight_grams, price, stock, image ");
                    sql.Append("FROM products WHERE 1=1 ");

                    var cmd = new MySqlCommand();
                    cmd.Connection = conn;

                    // เงื่อนไขค้นหาชื่อ
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sql.Append("AND name LIKE @kw ");
                        cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");
                    }

                    // เงื่อนไขหมวดหมู่ (ถ้าไม่ใช่ null หรือ "ทั้งหมด")
                    if (!string.IsNullOrWhiteSpace(category) && category != "ทั้งหมด")
                    {
                        sql.Append("AND category = @cat ");
                        cmd.Parameters.AddWithValue("@cat", category);
                    }

                    sql.Append("ORDER BY product_id DESC;");
                    cmd.CommandText = sql.ToString();

                    bool hasProduct = false;
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            hasProduct = true;
                            int id = Convert.ToInt32(rd["product_id"]);
                            string name = rd["name"].ToString();
                            int weight = rd["weight_grams"] != DBNull.Value ? Convert.ToInt32(rd["weight_grams"]) : 0;
                            decimal price = Convert.ToDecimal(rd["price"]);
                            int stock = Convert.ToInt32(rd["stock"]);
                            byte[] imageBytes = rd["image"] as byte[];

                            var card = CreateProductCard(id, name, price, stock, weight, imageBytes);
                            flowProducts.Controls.Add(card);
                        }
                    }

                    if (!hasProduct) ShowNoProductLabel();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message);
            }
        }

        private void ShowNoProductLabel()
        {
            var lblNoItem = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Goldenrod,
                Text = "ไม่พบรายการสินค้า"
            };
            flowProducts.Controls.Add(lblNoItem);
            lblNoItem.Margin = new Padding((flowProducts.Width - lblNoItem.Width) / 2, 20, 0, 0);
        }

        // สร้างการ์ดสินค้า (UI)
        private Panel CreateProductCard(int id, string name, decimal price, int stock, int weight, byte[] imageBytes)
        {
            var card = new Panel
            {
                Width = 205,
                Height = 380, // เพิ่มความสูงการ์ดนิดนึงกันตกขอบ
                Margin = new Padding(10),
                BackColor = Color.White,
                Padding = new Padding(8)
            };

            // 1. รูปสินค้า
            var pic = new PictureBox
            {
                Left = 10,
                Top = 10,
                Width = 185,
                Height = 180,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            if (imageBytes != null && imageBytes.Length > 0)
            {
                using (var ms = new MemoryStream(imageBytes)) pic.Image = Image.FromStream(ms);
            }
            else
            {
                var bmp = new Bitmap(185, 180);
                using (var g = Graphics.FromImage(bmp)) { g.Clear(Color.WhiteSmoke); } // เปลี่ยนสีพื้นรูปไม่มีภาพให้ดูต่างนิดนึง
                pic.Image = bmp;
            }
            card.Controls.Add(pic);

            // 2. ชื่อสินค้า (แก้ไข: ใส่สีดำ และขยับลงมาไม่ให้ทับรูป)
            var lblName = new Label
            {
                Left = 10,
                Top = 200, // ขยับลงมาจากรูป (รูปจบที่ 190)
                Width = 185,
                Height = 45, // เผื่อชื่อยาว 2 บรรทัด
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.Black, // <--- สำคัญ! ต้องระบุสี ไม่งั้นอาจเป็นสีขาวจนมองไม่เห็น
                TextAlign = ContentAlignment.TopCenter, // จัดกึ่งกลางแนวนอน ชิดบน
                Text = name
            };
            card.Controls.Add(lblName);

            // 3. ราคา (ขยับลงมาตามชื่อสินค้า)
            var lblPrice = new Label
            {
                Left = 10,
                Top = 245, // ขยับลงมา
                Width = 185,
                Height = 30,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#77A740"),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = $"฿{price:N2}"
            };
            card.Controls.Add(lblPrice);

            // 4. น้ำหนัก
            var lblWeight = new Label
            {
                Left = 10,
                Top = 275, // ขยับลงมา
                Width = 185,
                Height = 20,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = $"น้ำหนัก: {weight:N0} กรัม"
            };
            card.Controls.Add(lblWeight);

            // 5. ตัวเลือกจำนวน
            var numQty = new NumericUpDown
            {
                Left = 20,
                Top = 310, // ขยับลงมา
                Width = 70,
                Height = 30,
                Minimum = 0,
                Maximum = Math.Max(stock, 1),
                Value = 0,
                Enabled = stock > 0,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            card.Controls.Add(numQty);

            // 6. ปุ่มหยิบใส่ตะกร้า
            var btnConfirm = new Button
            {
                Left = 95,
                Top = 308, // ขยับลงมาให้ตรงกับ numQty
                Width = 90,
                Height = 32,
                Text = stock > 0 ? "หยิบใส่ตะกร้า" : "หมด",
                Enabled = stock > 0,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = stock > 0 ? ColorTranslator.FromHtml("#ACD137") : Color.Gray,
                Cursor = stock > 0 ? Cursors.Hand : Cursors.No
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            card.Controls.Add(btnConfirm);

            btnConfirm.Click += (s, e) => {
                int qty = (int)numQty.Value;
                if (qty <= 0) { MessageBox.Show("กรุณาเลือกจำนวน"); return; }
                AddToCart(id, name, qty, price);
            };

            if (stock <= 0)
            {
                var soldOut = new Label
                {
                    Text = "Sold Out",
                    BackColor = Color.Red,
                    ForeColor = Color.White,
                    AutoSize = false,
                    Width = 80,
                    Height = 30,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Location = new Point(pic.Left, pic.Top)
                };
                card.Controls.Add(soldOut);
                soldOut.BringToFront();
            }

            return card;
        }

        private void AddToCart(int pid, string pname, int qty, decimal price)
        {
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string checkSql = "SELECT id, quantity FROM cart WHERE user_email = @em AND product_id = @pid";
                    int existingId = 0, existingQty = 0;

                    using (var cmdChk = new MySqlCommand(checkSql, conn))
                    {
                        cmdChk.Parameters.AddWithValue("@em", userEmail);
                        cmdChk.Parameters.AddWithValue("@pid", pid);
                        using (var rd = cmdChk.ExecuteReader())
                        {
                            if (rd.Read()) { existingId = Convert.ToInt32(rd["id"]); existingQty = Convert.ToInt32(rd["quantity"]); }
                        }
                    }

                    if (existingId > 0)
                    {
                        string upSql = "UPDATE cart SET quantity = @qty, totalprice = @total WHERE id = @id";
                        using (var cmdUp = new MySqlCommand(upSql, conn))
                        {
                            cmdUp.Parameters.AddWithValue("@qty", existingQty + qty);
                            cmdUp.Parameters.AddWithValue("@total", (existingQty + qty) * price);
                            cmdUp.Parameters.AddWithValue("@id", existingId);
                            cmdUp.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        string inSql = "INSERT INTO cart (user_email, product_id, product_name, quantity, price, totalprice) VALUES (@em, @pid, @pname, @qty, @price, @total)";
                        using (var cmdIn = new MySqlCommand(inSql, conn))
                        {
                            cmdIn.Parameters.AddWithValue("@em", userEmail);
                            cmdIn.Parameters.AddWithValue("@pid", pid);
                            cmdIn.Parameters.AddWithValue("@pname", pname);
                            cmdIn.Parameters.AddWithValue("@qty", qty);
                            cmdIn.Parameters.AddWithValue("@price", price);
                            cmdIn.Parameters.AddWithValue("@total", price * qty);
                            cmdIn.ExecuteNonQuery();
                        }
                    }
                }
                MessageBox.Show($"เพิ่ม {pname} จำนวน {qty} ชิ้น เรียบร้อย!");
                LoadCartCountFromDb();
                UpdateCartBadge();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding to cart: " + ex.Message);
            }
        }

        // =============================================
        // 🛒 Badge ตะกร้า
        // =============================================
        private void SetupCartBadge()
        {
            Control[] found = this.Controls.Find("cartBadge", true);
            if (found.Length > 0)
            {
                cartBadge = (Label)found[0];
            }
            else
            {
                // สร้างให้อัตโนมัติถ้าไม่ได้ลากวาง
                cartBadge = new Label { AutoSize = false, Size = new Size(20, 20), BackColor = Color.Red, ForeColor = Color.White, Font = new Font("Segoe UI", 8, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, Visible = false };
                this.Controls.Add(cartBadge);
                cartBadge.BringToFront();
            }
        }

        private void LoadCartCountFromDb()
        {
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "SELECT COALESCE(SUM(quantity),0) FROM cart WHERE user_email = @em";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@em", userEmail);
                        object o = cmd.ExecuteScalar();
                        cartCount = (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o);
                    }
                }
            }
            catch { cartCount = 0; }
        }

        private void UpdateCartBadge()
        {
            if (cartBadge == null) return;
            cartBadge.Visible = cartCount > 0;
            cartBadge.Text = cartCount.ToString();
            UpdateCartBadgePosition();
        }

        private void UpdateCartBadgePosition()
        {
            if (btnCart != null && cartBadge != null)
            {
                cartBadge.Location = new Point(btnCart.Right - 10, btnCart.Top - 5);
            }
        }
    }
}