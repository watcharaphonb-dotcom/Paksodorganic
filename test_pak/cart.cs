using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace test_pak
{
    public partial class cart : Form
    {
        // 1. ตั้งค่าการเชื่อมต่อฐานข้อมูล
        private readonly string connStr = "Server=localhost;Database=leafy_farm;Uid=root;Pwd=;";
        private readonly string userEmail;

        // ตัวแปรคำนวณ
        private decimal grandTotal = 0;
        private bool hasStockIssue = false;

        public cart(string email)
        {
            InitializeComponent();
            this.userEmail = email;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        // ========================================================
        // 🎯 EVENT HANDLERS (ไปผูกในหน้า Design Properties)
        // ========================================================

        // 1. Form Load
        private void cart_Load(object sender, EventArgs e)
        {
            if (lblUser != null) lblUser.Text = $" {userEmail}";

            int contentWidth = (flowCart != null) ? flowCart.ClientSize.Width - 30 : 800;

            SetupHeader(contentWidth);
            LoadCartItems(contentWidth);
        }

        // 2. ปุ่มย้อนกลับ
        private void btnBack_Click(object sender, EventArgs e)
        {
            var shopForm = new Shop(userEmail);
            shopForm.Show();
            this.Close();
        }

        // 3. ปุ่มชำระเงิน
        private void btnCheckout_Click(object sender, EventArgs e)
        {
            if (grandTotal <= 0)
            {
                MessageBox.Show("ตะกร้าว่างเปล่า กรุณาเลือกสินค้าก่อน", "เตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (hasStockIssue)
            {
                MessageBox.Show("มีสินค้าบางรายการไม่เพียงพอในสต็อก \nกรุณาลดจำนวนหรือลบรายการที่เป็นสีแดง", "แจ้งเตือน");
                return;
            }

            try
            {
                var orderForm = new OrderForm(userEmail);
                orderForm.Show();
                this.Hide();
            }
            catch
            {
                MessageBox.Show("ไม่สามารถเปิดหน้ายืนยันคำสั่งซื้อได้ (ตรวจสอบว่ามี OrderForm.cs หรือไม่)");
            }
        }

        // Event เก่า (เก็บไว้กัน Error)
        private void button1_Click(object sender, EventArgs e) => btnCheckout_Click(sender, e);
        private void labelUser_Click(object sender, EventArgs e) { }
        private void lblTotal_Click(object sender, EventArgs e) { }

        // ==================================================
        // 🛍️ LOGIC
        // ==================================================

        private void LoadCartItems(int w)
        {
            if (flowCart == null) return;

            flowCart.Controls.Clear();
            grandTotal = 0;
            hasStockIssue = false;

            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT c.id, c.product_name, c.quantity, c.price, p.image, p.stock 
                                   FROM cart c 
                                   LEFT JOIN products p ON c.product_id = p.product_id 
                                   WHERE c.user_email = @email 
                                   ORDER BY c.id";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", userEmail);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                int cartId = Convert.ToInt32(rd["id"]);
                                string name = rd["product_name"].ToString();
                                int qty = Convert.ToInt32(rd["quantity"]);
                                decimal price = Convert.ToDecimal(rd["price"]);
                                int stock = (rd["stock"] != DBNull.Value) ? Convert.ToInt32(rd["stock"]) : 0;
                                byte[] imgData = rd["image"] as byte[];

                                bool isNotEnough = (qty > stock);
                                if (isNotEnough) hasStockIssue = true;

                                flowCart.Controls.Add(CreateCartRow(cartId, name, qty, price, stock, imgData, w, isNotEnough));
                                grandTotal += (price * qty);
                            }
                        }
                    }
                }
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading cart: " + ex.Message);
            }
        }

        private void UpdateUI()
        {
            if (lblTotal != null) lblTotal.Text = $"ยอดรวมสุทธิ: {grandTotal:N2} บาท";

            if (btnCheckout != null)
            {
                if (hasStockIssue || grandTotal <= 0)
                {
                    btnCheckout.Enabled = false;
                    btnCheckout.BackColor = Color.Gray;
                    btnCheckout.Text = (grandTotal <= 0) ? "ตะกร้าว่าง" : "สินค้าไม่พอ";
                }
                else
                {
                    btnCheckout.Enabled = true;
                    btnCheckout.BackColor = ColorTranslator.FromHtml("#acd137");
                    btnCheckout.Text = "ชำระเงิน";
                }
            }
        }

        // ฟังก์ชันสร้างแถวสินค้า (Row)
        private Panel CreateCartRow(int cartId, string name, int qty, decimal price, int stock, byte[] imgData, int rowWidth, bool isNotEnough)
        {
            Panel row = new Panel { Size = new Size(rowWidth, 100), BackColor = Color.White, Margin = new Padding(0, 0, 0, 5) };

            // รูปภาพ
            PictureBox pic = new PictureBox { Size = new Size(80, 80), Location = new Point(10, 10), SizeMode = PictureBoxSizeMode.Zoom };
            if (imgData != null) { using (var ms = new MemoryStream(imgData)) pic.Image = Image.FromStream(ms); }
            row.Controls.Add(pic);

            // ตำแหน่งคอลัมน์
            int colNameX = 110;
            int colQtyX = rowWidth - 600;
            int colPriceX = rowWidth - 400;
            int colTotalX = rowWidth - 250;
            int colDelX = rowWidth - 120;

            // ชื่อสินค้า
            Label lblName = new Label { Text = name, Font = new Font("Segoe UI", 12, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(colNameX, 10), Size = new Size(colQtyX - colNameX - 10, 40) };
            row.Controls.Add(lblName);

            // เตือนของหมด
            if (isNotEnough)
            {
                Label lblWarn = new Label { Text = $"⚠️ เหลือ {stock} ชิ้น", ForeColor = Color.Red, Location = new Point(colNameX, 50), AutoSize = true };
                row.Controls.Add(lblWarn);
                row.BackColor = Color.FromArgb(255, 235, 235);
            }

            // จำนวน (+/-)
            Panel pnlQty = new Panel { Location = new Point(colQtyX, 35), Size = new Size(120, 30) };
            Button btnMinus = new Button { Text = "-", Size = new Size(30, 30), Location = new Point(0, 0) };
            TextBox txtQty = new TextBox { Text = qty.ToString(), Size = new Size(40, 30), Location = new Point(35, 5), TextAlign = HorizontalAlignment.Center, ReadOnly = true };
            Button btnPlus = new Button { Text = "+", Size = new Size(30, 30), Location = new Point(80, 0) };

            btnMinus.Click += (s, e) => { if (qty > 1) UpdateQty(cartId, qty - 1, price); };
            btnPlus.Click += (s, e) => {
                if (qty < stock) UpdateQty(cartId, qty + 1, price);
                else MessageBox.Show($"สินค้ามีในสต็อกเพียง {stock} ชิ้น");
            };

            pnlQty.Controls.Add(btnMinus); pnlQty.Controls.Add(txtQty); pnlQty.Controls.Add(btnPlus);
            row.Controls.Add(pnlQty);

            // ราคา
            row.Controls.Add(new Label { Text = price.ToString("N2"), Location = new Point(colPriceX, 35), AutoSize = true, Font = new Font("Segoe UI", 10) });

            // ราคารวม
            row.Controls.Add(new Label { Text = (price * qty).ToString("N2"), Location = new Point(colTotalX, 35), AutoSize = true, ForeColor = Color.Green, Font = new Font("Segoe UI", 12, FontStyle.Bold) });

            // ✅ ปุ่มลบ (เปลี่ยนเป็นสีแดงสดแบบหน้า Shop)
            Button btnDel = new Button
            {
                Text = "ลบ",
                Location = new Point(colDelX, 30),
                Size = new Size(80, 35),
                BackColor = Color.Red, // ใช้สีแดงสด (Standard Red) ตามหน้า Shop
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDel.FlatAppearance.BorderSize = 0;
            btnDel.Click += (s, e) => { if (MessageBox.Show($"ยืนยันลบ '{name}' ออกจากตะกร้า?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) DeleteCartItem(cartId); };
            row.Controls.Add(btnDel);

            return row;
        }

        private void SetupHeader(int w)
        {
            if (flowCart == null) return;

            Panel header = new Panel { Size = new Size(w, 40), BackColor = ColorTranslator.FromHtml("#acd137"), Location = new Point(flowCart.Location.X, flowCart.Location.Y - 45) };
            this.Controls.Add(header);

            int colNameX = 110;
            int colQtyX = w - 600;
            int colPriceX = w - 400;
            int colTotalX = w - 250;
            int colDelX = w - 120;

            Action<string, int, ContentAlignment> addH = (txt, x, align) => {
                var lbl = new Label { Text = txt, Location = new Point(x, 10), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Size = new Size(100, 25), TextAlign = align };
                header.Controls.Add(lbl);
            };

            addH("สินค้า", colNameX, ContentAlignment.MiddleLeft);
            addH("จำนวน", colQtyX, ContentAlignment.MiddleCenter);
            addH("ราคา", colPriceX, ContentAlignment.MiddleLeft);
            addH("รวม", colTotalX, ContentAlignment.MiddleLeft);
            addH("ลบ", colDelX, ContentAlignment.MiddleCenter);
        }

        private void UpdateQty(int id, int q, decimal p)
        {
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    new MySqlCommand($"UPDATE cart SET quantity={q}, totalprice={q * p} WHERE id={id}", conn).ExecuteNonQuery();
                }
                LoadCartItems((flowCart != null) ? flowCart.Width - 30 : 800);
            }
            catch { }
        }

        private void DeleteCartItem(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    new MySqlCommand($"DELETE FROM cart WHERE id={id}", conn).ExecuteNonQuery();
                }
                LoadCartItems((flowCart != null) ? flowCart.Width - 30 : 800);
            }
            catch { }
        }
    }
}