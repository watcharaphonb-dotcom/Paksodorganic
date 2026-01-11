using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_pak
{
    public partial class OrderHistory : Form
    {
        private readonly string connStr = "Server=localhost;Database=leafy_farm;Uid=root;Pwd=;CharSet=utf8mb4;SslMode=Preferred;";
        private readonly string userEmail;
        private bool isLoading = false;

        // Model สำหรับเก็บข้อมูล
        private class OrderItemData
        {
            public string Name { get; set; }
            public int Qty { get; set; }
            public decimal Price { get; set; }
        }

        private class OrderData
        {
            public long Id { get; set; }
            public DateTime Date { get; set; }
            public decimal Total { get; set; }
            public string Status { get; set; }
            public string BuyerName { get; set; }
            public string Address { get; set; }
            public List<OrderItemData> Items { get; set; } = new List<OrderItemData>();
        }

        public OrderHistory(string email)
        {
            InitializeComponent();
            this.userEmail = email;

            this.DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            // ตั้งค่า DateTimePicker ให้มี CheckBox
            if (dtpDate != null)
            {
                dtpDate.ShowCheckBox = true;
                dtpDate.Checked = false;
                dtpDate.Format = DateTimePickerFormat.Short;
            }
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (btnHeader != null) btnHeader.Click += (s, ev) => { this.Close(); };
            if (btnSearch != null) { btnSearch.Click -= BtnSearch_Click; btnSearch.Click += BtnSearch_Click; }

            // โหลดครั้งแรก
            await LoadDataAsync(null, false, null);
        }

        // ✅ 1. อีเว้นท์กดปุ่มค้นหา
        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            bool useDate = dtpDate.Checked;
            DateTime? searchDate = dtpDate.Value.Date;

            await LoadDataAsync(keyword, useDate, searchDate);
        }

        // ✅ 2. โหลดข้อมูล
        private async Task LoadDataAsync(string keyword, bool useDate, DateTime? searchDate)
        {
            if (isLoading) return;
            isLoading = true;
            if (lblUserName != null) lblUserName.Text = userEmail;

            flowOrders.Controls.Clear();
            Label lblLoading = new Label { Text = "กำลังค้นหาข้อมูล...", AutoSize = true, Font = new Font("Segoe UI", 12), ForeColor = Color.Gray, Margin = new Padding(20) };
            flowOrders.Controls.Add(lblLoading);

            try
            {
                List<OrderData> orders = await Task.Run(() => FetchOrdersFromDb(keyword, useDate, searchDate));

                flowOrders.Controls.Clear();

                if (orders.Count == 0)
                {
                    Label empty = new Label { Text = "ไม่พบรายการที่ค้นหา", AutoSize = true, Font = new Font("Segoe UI", 14), ForeColor = Color.Red, Margin = new Padding(20) };
                    flowOrders.Controls.Add(empty);
                }
                else
                {
                    flowOrders.SuspendLayout();
                    foreach (var order in orders)
                    {
                        // ส่ง keyword ไปให้ฟังก์ชันสร้างการ์ดเพื่อทำไฮไลท์
                        flowOrders.Controls.Add(CreateOrderCard(order, keyword));
                    }
                    flowOrders.ResumeLayout();
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            finally { isLoading = false; }
        }

        // ✅ 3. ดึงข้อมูลจาก DB
        private List<OrderData> FetchOrdersFromDb(string keyword, bool useDate, DateTime? searchDate)
        {
            var orderDict = new Dictionary<long, OrderData>();
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"
                    SELECT o.order_id, o.order_date, o.total_price, o.status, u.name, u.address, oi.product_name, oi.quantity, oi.price
                    FROM orders o
                    JOIN user_info u ON o.user_email = u.email
                    LEFT JOIN order_items oi ON o.order_id = oi.order_id
                    WHERE o.user_email = @email ";

                // ใช้ Subquery เพื่อให้ได้ออเดอร์ทั้งบิล แม้สินค้าตรงแค่รายการเดียว
                if (!string.IsNullOrEmpty(keyword))
                {
                    sql += @" AND (
                                o.order_id LIKE @kw 
                                OR 
                                o.order_id IN (SELECT DISTINCT order_id FROM order_items WHERE product_name LIKE @kw)
                              ) ";
                }

                if (useDate && searchDate.HasValue)
                {
                    sql += " AND DATE(o.order_date) = DATE(@date) ";
                }

                sql += " ORDER BY o.order_date DESC, o.order_id DESC LIMIT 100";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@email", userEmail);
                    if (!string.IsNullOrEmpty(keyword)) cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");
                    if (useDate && searchDate.HasValue) cmd.Parameters.AddWithValue("@date", searchDate.Value);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            long oId = Convert.ToInt64(rd["order_id"]);
                            if (!orderDict.ContainsKey(oId))
                            {
                                orderDict[oId] = new OrderData
                                {
                                    Id = oId,
                                    Date = Convert.ToDateTime(rd["order_date"]),
                                    Total = Convert.ToDecimal(rd["total_price"]),
                                    Status = rd["status"].ToString(),
                                    BuyerName = rd["name"].ToString(),
                                    Address = rd["address"].ToString()
                                };
                            }
                            if (rd["product_name"] != DBNull.Value)
                            {
                                var pName = rd["product_name"].ToString();
                                if (!orderDict[oId].Items.Any(x => x.Name == pName))
                                {
                                    orderDict[oId].Items.Add(new OrderItemData { Name = pName, Qty = Convert.ToInt32(rd["quantity"]), Price = Convert.ToDecimal(rd["price"]) });
                                }
                            }
                        }
                    }
                }
            }
            return orderDict.Values.ToList();
        }

        // ✅ 4. สร้างการ์ด UI (ต่อเติมส่วนที่ขาดให้ครบ)
        private Panel CreateOrderCard(OrderData order, string highlightKeyword)
        {
            Panel card = new Panel();
            card.Width = flowOrders.ClientSize.Width - 30;
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Margin = new Padding(5, 5, 5, 20);

            int currentY = 15;

            // --- Header ---
            Label lblHeader = new Label
            {
                Text = $"Order #{order.Id} | วันที่: {order.Date:dd/MM/yyyy HH:mm}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.DarkSlateGray,
                AutoSize = true,
                Location = new Point(15, currentY)
            };
            if (!string.IsNullOrEmpty(highlightKeyword) && order.Id.ToString().Contains(highlightKeyword))
            {
                lblHeader.BackColor = Color.Yellow;
            }
            card.Controls.Add(lblHeader);

            // --- Status ---
            Label lblStatus = new Label
            {
                Text = (order.Status == "paid") ? "สถานะ: ชำระเงินแล้ว ✅" : "สถานะ: รอการชำระ ⚠️",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = (order.Status == "paid") ? Color.Green : Color.Red,
                AutoSize = true,
                Location = new Point(card.Width - 180, currentY),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            card.Controls.Add(lblStatus);
            currentY += 35;

            // --- Buyer Info ---
            Label lblBuyer = new Label
            {
                Text = $"ผู้ซื้อ: {order.BuyerName}\nที่อยู่: {order.Address}",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                MaximumSize = new Size(card.Width - 40, 0),
                Location = new Point(15, currentY)
            };
            card.Controls.Add(lblBuyer);
            currentY += lblBuyer.PreferredHeight + 15;

            Panel line1 = new Panel { BackColor = Color.LightGray, Height = 1, Width = card.Width - 30, Location = new Point(15, currentY) };
            card.Controls.Add(line1);
            currentY += 15;

            // --- Loop สินค้า ---
            foreach (var item in order.Items)
            {
                Label lblProd = new Label
                {
                    Text = "• " + item.Name,
                    Location = new Point(25, currentY),
                    Size = new Size(280, 25),
                    Font = new Font("Segoe UI", 10),
                    AutoEllipsis = true
                };

                // 🔴 Logic ไฮไลท์
                if (!string.IsNullOrEmpty(highlightKeyword) &&
                    item.Name.IndexOf(highlightKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    lblProd.BackColor = Color.FromArgb(255, 255, 200); // สีเหลืองอ่อน
                }

                card.Controls.Add(lblProd);

                Label lblDetail = new Label { Text = $"{item.Qty} x {item.Price:N0}", Location = new Point(320, currentY), Size = new Size(100, 25), Font = new Font("Segoe UI", 10), TextAlign = ContentAlignment.MiddleCenter };
                card.Controls.Add(lblDetail);

                Label lblSubTotal = new Label { Text = $"{(item.Qty * item.Price):N2} ฿", Location = new Point(card.Width - 140, currentY), Size = new Size(100, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Top | AnchorStyles.Right };
                card.Controls.Add(lblSubTotal);

                currentY += 30;
            }

            Panel line2 = new Panel { BackColor = Color.LightGray, Height = 1, Width = card.Width - 30, Location = new Point(15, currentY) };
            card.Controls.Add(line2);
            currentY += 15;

            // --- Grand Total ---
            Label lblGrandTotal = new Label
            {
                Text = $"ยอดสุทธิ: {order.Total:N2} บาท",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#acd137"),
                AutoSize = true,
                Location = new Point(card.Width - 250, currentY),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            lblGrandTotal.Location = new Point(card.Width - lblGrandTotal.PreferredWidth - 20, currentY);
            card.Controls.Add(lblGrandTotal);

            currentY += 50;
            card.Height = currentY;

            return card;
        }

        private void OrderHistory_Load(object sender, EventArgs e) { }
        private void btnSearch_Click_1(object sender, EventArgs e) { }
    }
}