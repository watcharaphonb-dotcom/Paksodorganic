using System;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace test_pak
{
    public partial class AdminForm : Form
    {
        private const string ConnStr = "Server=localhost;Database=leafy_farm;Uid=root;Pwd=;CharSet=utf8mb4;SslMode=None;";
        public AdminForm()
        {
            InitializeComponent();

        }

        private void AdminForm_Load(object sender, EventArgs e)
        {
            {
                // เติมวัน 1-31 (มีช่องว่างแปลว่า "ทุกวัน")
                comboDay.Items.Clear();
                comboDay.Items.Add(""); // ว่าง = ไม่กรอง
                for (int d = 1; d <= 31; d++) comboDay.Items.Add(d.ToString());

                // เติมเดือน 1-12 (มีช่องว่างแปลว่า "ทุกเดือน")
                comboMonth.Items.Clear();
                comboMonth.Items.Add("");
                for (int m = 1; m <= 12; m++) comboMonth.Items.Add(m.ToString());
                
                List<KeyValuePair<int?, string>> months = new List<KeyValuePair<int?, string>>();
                months.Add(new KeyValuePair<int?, string>(null, "")); // ✅ เพิ่ม null เป็น Key ได้แล้ว

                months.Add(new KeyValuePair<int?, string>(1, "มกราคม"));
                months.Add(new KeyValuePair<int?, string>(2, "กุมภาพันธ์"));
                months.Add(new KeyValuePair<int?, string>(3, "มีนาคม"));
                months.Add(new KeyValuePair<int?, string>(4, "เมษายน"));
                months.Add(new KeyValuePair<int?, string>(5, "พฤษภาคม"));
                months.Add(new KeyValuePair<int?, string>(6, "มิถุนายน"));
                months.Add(new KeyValuePair<int?, string>(7, "กรกฎาคม"));
                months.Add(new KeyValuePair<int?, string>(8, "สิงหาคม"));
                months.Add(new KeyValuePair<int?, string>(9, "กันยายน"));
                months.Add(new KeyValuePair<int?, string>(10, "ตุลาคม"));
                months.Add(new KeyValuePair<int?, string>(11, "พฤศจิกายน"));
                months.Add(new KeyValuePair<int?, string>(12, "ธันวาคม"));

                // 2. ผูกข้อมูล List เข้ากับ ComboBox
                comboMonth.DataSource = new BindingSource(months, null);
                comboMonth.DisplayMember = "Value"; // แสดงชื่อเดือน
                comboMonth.ValueMember = "Key";     // ค่าที่ใช้จริงในการดึงข้อมูล (1, 2,...)



                // เติมปี (ย้อนหลังสัก 5 ปี + ปีปัจจุบัน) มีช่องว่าง = "ทุกปี"
                comboYear.Items.Clear();
                comboYear.Items.Add("");
                int yearNow = DateTime.Now.Year;
                for (int y = yearNow - 5; y <= yearNow; y++) comboYear.Items.Add(y.ToString());

                // ให้เลือกค่าเริ่มต้นเป็นว่าง (แปลว่าโชว์ทุกวัน/เดือน/ปี)
                comboDay.SelectedIndex = 0;
                comboMonth.SelectedIndex = 0;
                comboYear.SelectedIndex = 0;

                // ตั้งค่า DataGridView เบื้องต้น
                dataGridView1.AutoGenerateColumns = true; // ให้สร้างคอลัมน์เองจาก DataTable

                // โหลดข้อมูลครั้งแรก (ยังไม่กรองอะไร)
                LoadOrders();
            }
        }



        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadOrders();
        }

        private void LoadOrders()
        {
            // 🚩 1. เพิ่ม Try-Catch เพื่อดักจับข้อผิดพลาดระหว่างการเชื่อมต่อ/ดึงข้อมูล
            try
            {
                int tempDay;
                int? day = int.TryParse(comboDay.Text, out tempDay) ? tempDay : (int?)null; ;
                
                int? month = comboMonth.SelectedValue as int?;
                
                int tempYear;
                int? year = int.TryParse(comboYear.Text, out tempYear) ? tempYear : (int?)null;

                string sql = @"
            SELECT
                DATE_FORMAT(order_date, '%d/%m/%Y') AS `วันที่`,
                user_email                      AS `ผู้ซื้อ`,
                total_price                      AS `รวมยอด`,
                receipt                          AS `ใบเสร็จ`,    
                payment_slip                     AS `สลิปโอนเงิน`  
            FROM orders
            WHERE 1=1
        ";

                var conditions = new List<string>();
                if (day.HasValue) conditions.Add("DAY(order_date)=@day");
                if (month.HasValue) conditions.Add("MONTH(order_date)=@month");
                if (year.HasValue) conditions.Add("YEAR(order_date)=@year");
                if (conditions.Count > 0) sql += " AND " + string.Join(" AND ", conditions);

                sql += " ORDER BY order_date DESC;";

                // 💡 ถ้าโปรแกรมค้าง/ค้างตอนกดปุ่ม 'แสดงรายงาน' ให้ตรวจสอบว่า Program.ConnStr ถูกต้องและ MySQL ทำงานอยู่
                using (var conn = new MySqlConnection(Program.ConnStr))
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (day.HasValue) cmd.Parameters.AddWithValue("@day", day.Value);
                    if (month.HasValue) cmd.Parameters.AddWithValue("@month", month.Value);
                    if (year.HasValue) cmd.Parameters.AddWithValue("@year", year.Value);

                    var dt = new DataTable();
                    using (var da = new MySqlDataAdapter(cmd)) { da.Fill(dt); }

                    // เพิ่มคอลัมน์รูปตัวอย่างจาก BLOB
                    if (!dt.Columns.Contains("ภาพสลิป"))
                        dt.Columns.Add("ภาพสลิป", typeof(Image));

                    foreach (DataRow row in dt.Rows)
                    {
                        // โค้ดเดิม: จัดการกรณีมีข้อมูลสลิป (ไม่เป็น DBNull.Value)
                        if (dt.Columns.Contains("สลิปโอนเงิน") && row["สลิปโอนเงิน"] != DBNull.Value)
                        {
                            try
                            {
                                var bytes = (byte[])row["สลิปโอนเงิน"];
                                using (var ms = new MemoryStream(bytes))
                                using (var full = Image.FromStream(ms))
                                {
                                    // ทำ thumbnail ให้โหลดไว (h=90)
                                    int h = 90;
                                    int w = (int)(full.Width * (h / (double)full.Height));
                                    row["ภาพสลิป"] = new Bitmap(full, new Size(Math.Max(60, w), h));
                                }
                            }
                            catch
                            {
                                row["ภาพสลิป"] = null;
                            }
                        }
                        // ➡️ ส่วนที่เพิ่ม (1): จัดการกรณีไม่มีข้อมูลสลิป (เป็น DBNull.Value)
                        else
                        {
                            row["ภาพสลิป"] = null; // ตั้งค่าเป็น null เพื่อให้แสดงเป็นช่องว่าง
                        }

                        // ➡️ ส่วนที่เพิ่ม (2): จัดการคอลัมน์ "ใบเสร็จ" เมื่อเป็นค่าว่าง (DBNull.Value)
                        if (dt.Columns.Contains("ใบเสร็จ") && row["ใบเสร็จ"] == DBNull.Value)
                        {
                            row["ใบเสร็จ"] = string.Empty; // ตั้งค่าเป็นช่องว่าง (string.Empty)
                        }
                    }

                    dataGridView1.DataSource = dt;

                    // 🚩 2. แก้ไข: ขยายคอลัมน์ให้เต็มพื้นที่ที่กำหนดไว้ในแนวนอน
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // 🚩 3. แก้ไข: กำหนดความสูงแถวให้เพียงพอสำหรับภาพ Thumbnail (90px)
                    dataGridView1.RowTemplate.Height = 95;

                    // ฟอร์แมตคอลัมน์ราคา
                    if (dataGridView1.Columns.Contains("รวมยอด"))
                    {
                        // ยกเลิกการตั้งค่า AutoSize เพื่อให้ Width=100 มีผล
                        dataGridView1.Columns["รวมยอด"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        dataGridView1.Columns["รวมยอด"].DefaultCellStyle.Format = "N2";
                        dataGridView1.Columns["รวมยอด"].Width = 100;
                        dataGridView1.Columns["รวมยอด"].DefaultCellStyle.Alignment =
                            DataGridViewContentAlignment.MiddleRight;
                    }

                    // ตั้งค่าคอลัมน์ 'ผู้ซื้อ' ให้ขยายตัวเป็นพิเศษ เนื่องจากอีเมลยาว
                    if (dataGridView1.Columns.Contains("ผู้ซื้อ"))
                    {
                        dataGridView1.Columns["ผู้ซื้อ"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }

                    // ซ่อน BLOB ดิบ
                    if (dataGridView1.Columns.Contains("สลิปโอนเงิน"))
                        dataGridView1.Columns["สลิปโอนเงิน"].Visible = false;

                    // ตั้งค่าคอลัมน์ภาพ
                    if (dataGridView1.Columns.Contains("ภาพสลิป"))
                    {
                        var imgCol = (DataGridViewImageColumn)dataGridView1.Columns["ภาพสลิป"];
                        imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                    }

                    // อัปเดตรวมยอด
                    UpdateTotalFromGrid();
                }
            }
            catch (Exception ex)
            {
                // 🚨 ถ้าบัคเป็น Crash แสดงข้อความแจ้งเตือนที่ชัดเจน
                MessageBox.Show($"พบข้อผิดพลาดในการโหลดข้อมูล: {ex.Message}\n\nโปรดตรวจสอบการเชื่อมต่อฐานข้อมูล (MySQL Server) และค่า Program.ConnStr", "ข้อผิดพลาดร้ายแรง", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void UpdateTotalFromGrid()
        {
            if (dataGridView1.DataSource == null)
            {
                lblTotalSum.Text = "รวมทั้งหมด: 0.00 บาท";
                return;
            }

            decimal sum = 0m;

            // ถ้า DataSource เป็น DataTable
            if (dataGridView1.DataSource is DataTable dt)
            {
                // ค้นหาคอลัมน์ชื่อ "รวมยอด"
                if (dt.Columns.Contains("รวมยอด"))
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        if (r["รวมยอด"] != DBNull.Value)
                        {
                            if (decimal.TryParse(r["รวมยอด"].ToString(), out decimal value))
                                sum += value;
                        }
                    }
                }
            }
            else
            {
                // ถ้าไม่ใช่ DataTable ก็วนในกริดตรง ๆ
                if (dataGridView1.Columns.Contains("รวมยอด"))
                {
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.IsNewRow) continue;
                        var val = row.Cells["รวมยอด"].Value;
                        if (val != null && decimal.TryParse(val.ToString(), out decimal value))
                            sum += value;
                    }
                }
            }

            lblTotalSum.Text = $"รวมทั้งหมด: {sum:N2} บาท";
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var colName = dataGridView1.Columns[e.ColumnIndex].Name;

            // 3.1 เปิดใบเสร็จ (PDF path) เหมือนเดิม
            if (colName == "ใบเสร็จ")
            {
                var path = dataGridView1.Rows[e.RowIndex].Cells["ใบเสร็จ"].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(path))
                {
                    try { Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
                    catch (Exception ex) { MessageBox.Show("เปิดไฟล์ไม่ได้: " + ex.Message); }
                }
                return;
            }

            // 3.2 เปิดสลิป (BLOB) ด้วยโปรแกรมดูภาพ (เขียนเป็นไฟล์ temp แล้วเปิด)
            if (colName == "ภาพสลิป")
            {
                // ดึง BLOB จากคอลัมน์ที่ซ่อนอยู่
                if (!dataGridView1.Columns.Contains("สลิปโอนเงิน")) return;

                var cellVal = dataGridView1.Rows[e.RowIndex].Cells["สลิปโอนเงิน"].Value;
                if (cellVal == null || cellVal == DBNull.Value) return;

                try
                {
                    var bytes = (byte[])cellVal;

                    // สร้างไฟล์ชั่วคราว (นามสกุล .png)
                    string tmp = Path.Combine(Path.GetTempPath(), $"slip_{Guid.NewGuid():N}.png");
                    File.WriteAllBytes(tmp, bytes); // ถ้าจริง ๆ เป็น .jpg ก็ยังเปิดได้โดยมาก

                    Process.Start(new ProcessStartInfo { FileName = tmp, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เปิดสลิปไม่ได้: " + ex.Message);
                }
            }
        }

        private void LoadBestSellingProducts()
        {

            try
            {
                // 1. ดึงค่าจาก ComboBoxes (ใช้หลักการเดียวกับ LoadOrders)
                int tempDay;
                int? day = int.TryParse(comboDay.Text, out tempDay) ? tempDay : (int?)null;

                int? month = comboMonth.SelectedValue as int?;

                int tempYear;
                int? year = int.TryParse(comboYear.Text, out tempYear) ? tempYear : (int?)null;

                // 2. สร้างคำสั่ง SQL สำหรับรวมยอดขายสินค้า
                string sql = @"
            SELECT
                T2.product_name AS `สินค้า`,
                SUM(T2.quantity) AS `จำนวนรวมที่ขายได้`
            FROM orders T1
            JOIN order_items T2 ON T1.order_id = T2.order_id 
            WHERE 1=1
        ";

                var conditions = new List<string>();
                if (day.HasValue) conditions.Add("DAY(T1.order_date)=@day");
                if (month.HasValue) conditions.Add("MONTH(T1.order_date)=@month");
                if (year.HasValue) conditions.Add("YEAR(T1.order_date)=@year");
                if (conditions.Count > 0) sql += " AND " + string.Join(" AND ", conditions);
                sql += " GROUP BY T2.product_name ORDER BY SUM(T2.quantity) DESC LIMIT 10;";

                using (var conn = new MySqlConnection(Program.ConnStr))
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (day.HasValue) cmd.Parameters.AddWithValue("@day", day.Value);
                    if (month.HasValue) cmd.Parameters.AddWithValue("@month", month.Value);
                    if (year.HasValue) cmd.Parameters.AddWithValue("@year", year.Value);

                    var dt = new DataTable();
                    using (var da = new MySqlDataAdapter(cmd)) { da.Fill(dt); }

                    // 3. แสดงผลใน DataGridView
                    dataGridView1.DataSource = dt;

                    // ตั้งค่า DataGridView (เนื่องจากรายงานนี้ไม่มีรูปภาพสลิป)
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView1.RowTemplate.Height = 30; // ใช้ความสูงปกติ

                    // ตั้งค่าคอลัมน์ จำนวนรวมที่ขายได้ ให้ชิดขวา
                    if (dataGridView1.Columns.Contains("จำนวนรวมที่ขายได้"))
                    {
                        dataGridView1.Columns["จำนวนรวมที่ขายได้"].DefaultCellStyle.Alignment =
                            DataGridViewContentAlignment.MiddleRight;
                    }

                    // ล้างส่วนรวมยอดทั้งหมด (ถ้ามี)
                    // (ถ้ามี Label ที่แสดงยอดรวมทั้งหมด ให้ตั้งค่า Label นั้นเป็น String.Empty)
                }
            } // ⬆️ จบ try block
            catch (Exception ex)
            {
                MessageBox.Show($"พบข้อผิดพลาดในการโหลดรายงานสินค้าขายดี: {ex.Message}", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } // ⬆️ จบ catch block
        }


        private void lblTotal_Click(object sender, EventArgs e)
        {

        }

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

        private void btnCustomer(object sender, EventArgs e)
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

        private void btnBestSelling_Click(object sender, EventArgs e)
        {
            LoadBestSellingProducts();
        }
    }
}
