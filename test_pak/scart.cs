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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace test_pak
{
    public partial class scart : Form
    {
        private readonly string connStr = "Server=localhost;Database=leafy_farm;Uid=root;Pwd=;CharSet=utf8mb4;SslMode=None;";
        private readonly string userEmail;  // รับมาจาก Shop

        // ✅ รับอีเมลจากหน้า Shop
        public scart(string email)
        {
            InitializeComponent();
            userEmail = email;
        }


        private void dataGridViewCart_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var g = dataGridViewCart;
            if (g.Columns[e.ColumnIndex].Name != "colDelete") return;

            int id = Convert.ToInt32(g.Rows[e.RowIndex].Cells["id"].Value);

            var confirm = MessageBox.Show("ต้องการลบรายการนี้หรือไม่?", "ยืนยัน",
                                          MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            DeleteCartItem(id);           // ลบใน DB
            g.Rows.RemoveAt(e.RowIndex);  // ลบในกริด
            UpdateGrandTotal();           // คำนวณยอดใหม่
        }

        private void cart_Load(object sender, EventArgs e)
        {
            labelUser.Text = userEmail;
            LoadCartItems();
            ConfigureGrid();
            BindGridEvents();
            UpdateGrandTotal();
        }

        private void LoadCartItems()
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"
            SELECT 
                c.id,
                c.user_email,
                c.product_id,
                c.product_name,
                c.quantity,
                c.price,
                c.totalprice,
                p.image AS product_image
            FROM cart c
            LEFT JOIN products p ON p.product_id = c.product_id
            WHERE c.user_email = @em
            ORDER BY c.id;";
                using (var da = new MySqlDataAdapter(sql, conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@em", userEmail);
                    da.Fill(dt);
                }
            }

            dataGridViewCart.AutoGenerateColumns = false;
            dataGridViewCart.Columns.Clear();

            // ✅ คอลัมน์รูปสินค้า
            var imgCol = new DataGridViewImageColumn
            {
                HeaderText = "",
                Name = "colImg",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                Width = 60
            };
            dataGridViewCart.Columns.Add(imgCol);

            // ซ่อน id เอาไว้ใช้อ้างอิงเวลาลบ/แก้จำนวน
            dataGridViewCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "id",
                DataPropertyName = "id",
                Visible = false
            });

            // ✅ ชื่อสินค้k 
            dataGridViewCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "product_name",
                HeaderText = "ชื่อสินค้า",
                DataPropertyName = "product_name",
                Width = 250
            });

            // ✅ จำนวน
            dataGridViewCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "quantity",
                HeaderText = "จำนวน",
                DataPropertyName = "quantity",
                Width = 70
            });

            // ✅ ราคาต่อชิ้น
            dataGridViewCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "price",
                HeaderText = "ราคาต่อชิ้น",
                DataPropertyName = "price",
                Width = 70,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });

            // ✅ รวม
            dataGridViewCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "totalprice",
                HeaderText = "ราคารวม",
                DataPropertyName = "totalprice",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });

            dataGridViewCart.DataSource = dt;

            // ✅ แปลง byte[] → รูปภาพ
            foreach (DataGridViewRow row in dataGridViewCart.Rows)
            {
                if (row.DataBoundItem is DataRowView drv)
                {
                    var bytes = drv["product_image"] as byte[];
                    if (bytes != null && bytes.Length > 0)
                    {
                        using (var ms = new MemoryStream(bytes))
                            row.Cells["colImg"].Value = Image.FromStream(ms);
                    }
                }
            }

            // ✅ ทำหัวตารางให้เป็นสีเขียว
            dataGridViewCart.EnableHeadersVisualStyles = false;
            dataGridViewCart.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#acd137");
            dataGridViewCart.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewCart.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        }

        private void ConfigureGrid()
        {
            var g = dataGridViewCart;
            g.ReadOnly = false;               // ให้แก้ไขได้ (แต่จะล็อกบางคอลัมน์ไว้)
            g.AllowUserToAddRows = false;     // ไม่ให้เพิ่มแถวจากตารางโดยตรง
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;


            // ✅ ตั้งค่าความสูงของแถวให้เท่ากันทุกแถว
            g.RowTemplate.Height = 70;

            // ตั้งชื่อหัวคอลัมน์ให้อ่านง่าย
            g.Columns["product_name"].HeaderText = "ชื่อสินค้า";
            g.Columns["product_name"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            g.Columns["quantity"].HeaderText = "จำนวน";
            g.Columns["quantity"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            g.Columns["price"].HeaderText = "ราคาต่อชิ้น";
            g.Columns["price"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            g.Columns["totalprice"].HeaderText = "ราคารวม";
            g.Columns["totalprice"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;


            // ปรับขนาดคอลัมน์ให้ดูดีและอ่านง่าย
            dataGridViewCart.Columns["product_name"].Width = 260;  // ชื่อสินค้า
            dataGridViewCart.Columns["quantity"].Width = 110;       // จำนวน
            dataGridViewCart.Columns["price"].Width = 110;         // ราคาต่อชิ้น
            dataGridViewCart.Columns["totalprice"].Width = 110;    // ราคารวม



            // ล็อกคอลัมน์ที่ไม่ให้แก้                                                                                                       
            if (g.Columns["user_email"] != null)
                g.Columns["user_email"].ReadOnly = true;
            g.Columns["product_name"].ReadOnly = true;
            g.Columns["price"].ReadOnly = true;       // ให้แก้เฉพาะ quantity
            g.Columns["totalprice"].ReadOnly = true;  // ให้โปรแกรมคำนวณ

            // เพิ่มปุ่มลบ 1 คอลัมน์ หากยังไม่มี
            const string delColName = "colDelete";
            if (!g.Columns.Contains(delColName))
            {
                var btnDel = new DataGridViewButtonColumn
                {
                    Name = delColName,
                    HeaderText = "ลบ",
                    Text = "ลบ",
                    UseColumnTextForButtonValue = true,
                    Width = 80
                };
                g.Columns.Add(btnDel);

            }

        }
        private void BindGridEvents()
        {
            dataGridViewCart.CellEndEdit -= dataGridViewCart_CellEndEdit;
            dataGridViewCart.CellEndEdit += dataGridViewCart_CellEndEdit;

            dataGridViewCart.CellClick -= dataGridViewCart_CellClick;
            dataGridViewCart.CellClick += dataGridViewCart_CellClick;
        }


      

        // >>> ตัวนี้คือ handler ที่หายไป <<<
        private void dataGridViewCart_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var g = dataGridViewCart;
            var colName = g.Columns[e.ColumnIndex].Name;
            if (colName != "quantity") return;

            var row = g.Rows[e.RowIndex];

            if (!int.TryParse(Convert.ToString(row.Cells["quantity"]?.Value), out var qty) || qty < 1)
            {
                MessageBox.Show("จำนวนต้องเป็นตัวเลขมากกว่าหรือเท่ากับ 1");
                // ถ้ามี id → รีโหลดค่าจาก DB กลับ
                if (int.TryParse(Convert.ToString(row.Cells["id"]?.Value), out var idReload))
                    ReloadRow(idReload, e.RowIndex);
                else
                    row.Cells["quantity"].Value = 1;
                return;
            }

            decimal price = 0m;
            decimal.TryParse(Convert.ToString(row.Cells["price"]?.Value), out price);

            var total = qty * price;
            row.Cells["totalprice"].Value = total;

            // >>> เพิ่มบรรทัดนี้ให้เหมือนต้นฉบับ <<<
            if (int.TryParse(Convert.ToString(row.Cells["id"]?.Value), out var id))
                UpdateQuantityAndTotal(id, qty, total);

            UpdateGrandTotal();
        }

        // 3) กดปุ่มลบในคอลัมน์ "ลบ"
        private void UpdateGrandTotal()
        {
            decimal sum = 0m;

            if (dataGridViewCart.DataSource is DataTable dt)
            {
                foreach (DataRow r in dt.Rows)
                {
                    if (r.RowState == DataRowState.Deleted) continue;
                    if (r["totalprice"] != DBNull.Value)
                        sum += Convert.ToDecimal(r["totalprice"]);
                }
            }
            else
            {
                foreach (DataGridViewRow row in dataGridViewCart.Rows)
                {
                    if (row.IsNewRow) continue;
                    var val = row.Cells["totalprice"]?.Value;
                    if (val != null && decimal.TryParse(val.ToString(), out var tp))
                        sum += tp;
                }
            }

            if (lblGrandTotal != null)
                lblGrandTotal.Text = $"รวมทั้งหมด: ฿{sum:N2}";
        }
        private void UpdateQuantityAndTotal(int id, int qty, decimal total)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"UPDATE cart 
                       SET quantity = @q, totalprice = @t
                       WHERE id = @id AND user_email = @em";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@q", qty);
                    cmd.Parameters.AddWithValue("@t", total);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@em", userEmail);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void DeleteCartItem(int id)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"DELETE FROM cart WHERE id = @id AND user_email = @em";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@em", userEmail);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void ReloadRow(int id, int rowIndex)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT quantity, price, totalprice 
                       FROM cart 
                       WHERE id = @id AND user_email = @em";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@em", userEmail);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            dataGridViewCart.Rows[rowIndex].Cells["quantity"].Value = rd.GetInt32("quantity");
                            dataGridViewCart.Rows[rowIndex].Cells["price"].Value = rd.GetDecimal("price");
                            dataGridViewCart.Rows[rowIndex].Cells["totalprice"].Value = rd.GetDecimal("totalprice");
                        }
                    }
                }
            }
        }

    }
}
