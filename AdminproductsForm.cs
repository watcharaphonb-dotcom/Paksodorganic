using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;

namespace test_pak
{
    public partial class AdminproductsForm : Form
    {
        public static string ConnStr =
    "Server=127.0.0.1;Port=3306;Database=leafy_farm;Uid=root;Pwd=;CharSet=utf8mb4;SslMode=Preferred;AllowPublicKeyRetrieval=True;";
        private byte[] currentImageBytes = null;

        private enum EditMode { None, Insert, Edit }
        //private EditMode _mode = EditMode.None;

        public AdminproductsForm()
        {
            InitializeComponent();
        }

        private void AdminproductsForm_Load(object sender, EventArgs e)
        {
            dgvProducts.AutoGenerateColumns = true;
            LoadCategories();   // โหลดหมวดหมู่จากฐานข้อมูล
            LoadProducts();     // โหลดสินค้า
        }

        private void LoadProducts()
        {
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(@"
        SELECT product_id, name, category, price, stock, image, weight_grams
        FROM products ORDER BY product_id DESC;", conn))
            {
                conn.Open();
                var dt = new DataTable();
                using (var da = new MySqlDataAdapter(cmd)) da.Fill(dt);

                // ทำคอลัมน์พรีวิวรูป
                if (!dt.Columns.Contains("image_preview")) dt.Columns.Add("image_preview", typeof(Image));
                foreach (DataRow r in dt.Rows)
                {
                    if (r["image"] != DBNull.Value)
                    {
                        try
                        {
                            var bytes = (byte[])r["image"];
                            using (var ms = new MemoryStream(bytes))
                            using (var full = Image.FromStream(ms))
                            {
                                int h = 40; int w = (int)(full.Width * (h / (double)full.Height));
                                r["image_preview"] = new Bitmap(full, new Size(Math.Max(60, w), h));
                            }
                        }
                        catch { r["image_preview"] = null; }
                    }
                }

                dgvProducts.DataSource = dt;

                if (dgvProducts.Columns.Contains("image"))
                    dgvProducts.Columns["image"].Visible = false;

                if (dgvProducts.Columns.Contains("image_preview"))
                {
                    var imgCol = (DataGridViewImageColumn)dgvProducts.Columns["image_preview"];
                    imgCol.HeaderText = "รูป";
                    imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                    dgvProducts.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                    dgvProducts.RowTemplate.Height = 25;
                }

                if (dgvProducts.Columns.Contains("price"))
                {
                    dgvProducts.Columns["price"].DefaultCellStyle.Format = "N2";
                    dgvProducts.Columns["price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        private void RefreshGridAndSelect(int productId)
        {
            LoadProducts();                  // รีโหลดข้อมูลในกริด

            // เลือกแถวของ product_id ที่เพิ่งเพิ่ม และเลื่อนสกอลล์ไปหา
            foreach (DataGridViewRow r in dgvProducts.Rows)
            {
                if (!r.IsNewRow && r.Cells["product_id"]?.Value != null &&
                    int.TryParse(r.Cells["product_id"].Value.ToString(), out int id) &&
                    id == productId)
                {
                    r.Selected = true;
                    dgvProducts.CurrentCell = r.Cells[0];
                    dgvProducts.FirstDisplayedScrollingRowIndex = r.Index;
                    break;
                }
            }

            // (ไม่จำเป็นมาก แต่ช่วยบังคับรีเฟรชบายนด์ดิ้ง)
            var cm = (CurrencyManager)BindingContext[dgvProducts.DataSource];
            cm.Refresh();
            dgvProducts.Refresh();
            dgvProducts.RowTemplate.Height = 25;
        }


        private void LoadCategories()
        {
            comboCategory.Items.Clear();

            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand("SELECT DISTINCT category FROM products ORDER BY category ASC;", conn))
            {
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        string cat = rd["category"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(cat))
                            comboCategory.Items.Add(cat);
                    }
                }
            }

            // เพิ่ม option ให้ผู้ใช้สามารถพิมพ์หมวดใหม่ได้ด้วย
            comboCategory.DropDownStyle = ComboBoxStyle.DropDown; // ถ้าอยากให้เลือกอย่างเดียวใช้ DropDownList
        }



        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvProducts.Rows[e.RowIndex];
            txtProductId.Text = row.Cells["product_id"]?.Value?.ToString() ?? "";
            txtName.Text = row.Cells["name"]?.Value?.ToString() ?? "";
            comboCategory.Text = row.Cells["category"]?.Value?.ToString() ?? "";
            txtPrice.Text = row.Cells["price"]?.Value?.ToString() ?? "";
            txtStock.Text = row.Cells["stock"]?.Value?.ToString() ?? "";
            txtWeight.Text = row.Cells["weight_grams"]?.Value?.ToString() ?? "";

            // รูปภาพใน DB (BLOB)
            currentImageBytes = null;
            if (row.Cells["image"] != null && row.Cells["image"].Value != DBNull.Value)
            {
                currentImageBytes = (byte[])row.Cells["image"].Value;
                SetImageFromBytes(currentImageBytes);
            }
            else
            {
                picImage.Image = null;
            }
        }

        // ========= ปุ่มเลือกภาพ =========
        private void btnChooseImage_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog
            {
                Title = "เลือกภาพสินค้า",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    currentImageBytes = File.ReadAllBytes(ofd.FileName);
                    SetImageFromBytes(currentImageBytes);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // อ่านค่า + ตรวจง่าย ๆ
            string name = txtName.Text.Trim();
            string category = comboCategory.Text.Trim();

            if (!decimal.TryParse(txtPrice.Text.Trim(), out decimal price)) price = 0;
            if (!int.TryParse(txtStock.Text.Trim(), out int stock)) stock = 0;
            if (!int.TryParse(txtWeight.Text.Trim(), out int weight)) weight = 0;

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("กรุณากรอกชื่อสินค้า");
                return;
            }

            using (var conn = new MySqlConnection(ConnStr))
            {
                conn.Open();

                // ถ้ามี product_id แสดงว่าแก้ไข (UPDATE) ไม่งั้น INSERT
                bool isUpdate = int.TryParse(txtProductId.Text.Trim(), out int productId);

                if (!isUpdate)
                {
                    // INSERT
                    string sql = @"
                           INSERT INTO products (name, category, price, stock, image, weight_grams)
                           VALUES (@name, @category, @price, @stock, @image, @weight);
                           SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@category", category);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@stock", stock);
                        cmd.Parameters.AddWithValue("@weight", weight);
                        cmd.Parameters.Add("@image", MySqlDbType.Blob).Value = (object)currentImageBytes ?? DBNull.Value;

                        int newId = Convert.ToInt32(cmd.ExecuteScalar());
                        txtProductId.Text = newId.ToString();
                        MessageBox.Show("เพิ่มสินค้าเรียบร้อย");

                        // ✅ รีเฟรชกริด + เลือกแถวที่เพิ่งเพิ่ม
                        RefreshGridAndSelect(newId);
                    }
                }
                else
                {
                    // UPDATE (ถ้ามีรูปใหม่ → อัปเดตรูปด้วย, ถ้าไม่ → ไม่แตะคอลัมน์รูป)
                    string sqlWithImg = @"
                    UPDATE products SET 
                        name=@name, category=@category, price=@price, stock=@stock, 
                        image=@image, weight_grams=@weight
                    WHERE product_id=@id;";
                    string sqlNoImg = @"
                    UPDATE products SET 
                        name=@name, category=@category, price=@price, stock=@stock, 
                        weight_grams=@weight
                    WHERE product_id=@id;";

                    string sql = (currentImageBytes != null) ? sqlWithImg : sqlNoImg;

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", productId);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@category", category);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@stock", stock);
                        cmd.Parameters.AddWithValue("@weight", weight);
                        if (currentImageBytes != null)
                            cmd.Parameters.Add("@image", MySqlDbType.Blob).Value = currentImageBytes;

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("บันทึกการแก้ไขเรียบร้อย");

                        // คง selection ที่สินค้านั้น
                        RefreshGridAndSelect(productId);
                    }
                }
            }
            LoadProducts();     // รีเฟรชกริด
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtProductId.Text.Trim(), out int productId))
            {
                MessageBox.Show("กรุณาเลือกสินค้าที่ต้องการลบจากตาราง");
                return;
            }

            if (MessageBox.Show("ต้องการลบสินค้านี้หรือไม่?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                != DialogResult.Yes) return;

            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand("DELETE FROM products WHERE product_id=@id", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@id", productId);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("ลบสินค้าเรียบร้อย");
            ClearForm();
            LoadProducts();
        }

       
     

        // ========= Helpers =========
        private void ClearForm()
        {
            txtProductId.Clear();
            txtName.Clear();
            comboCategory.Text = "";
            txtPrice.Clear();
            txtStock.Clear();
            txtWeight.Clear();
            picImage.Image = null;
            currentImageBytes = null;
            txtName.Focus();
        }

        private void SetImageFromBytes(byte[] bytes)
        {
            if (bytes == null) { picImage.Image = null; return; }
            using (var ms = new MemoryStream(bytes))
            {
                picImage.Image = Image.FromStream(ms);
            }
        }
       
        private void dgvProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
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
    }
}
