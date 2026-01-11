using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using System.Globalization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.IO;            // ใช้ File.Exists
using System.Diagnostics;   // ใช้ Process.Start

// ต้องมี iTextSharp package และตั้งค่าฟอนต์ไทย THSarabunNew.ttf ให้ถูกต้อง
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw; // เพิ่ม namespace สำหรับวาดเส้น (LineSeparator)

namespace test_pak
{
    public partial class scanqr : Form
    {
        // ตัวแปรเชื่อมต่อฐานข้อมูล
        private readonly string connStr = "Server=localhost;Database=leafy_farm;Uid=root;Pwd=;";
        private readonly string userEmail;
        private readonly long orderId;

        // ตัวแปรสถานะ
        private bool _slipUploaded = false;
        private string receiptPdfPath;

        // ====== ตั้งค่าร้าน/บัญชี PromptPay ======
        // ใช้เบอร์มือถือ (ลบ 0 ตัวหน้า แล้วต่อด้วย 0066)
        private readonly string promptPayTargetMobileRaw = "0635286734";

        public scanqr(string email, long orderId)
        {
            InitializeComponent();
            this.userEmail = email;
            this.orderId = orderId;
        }

        private void scanqr_Load(object sender, EventArgs e)
        {
            lblUserEmail.Text = userEmail;  // แสดงอีเมลลูกค้า
            btnConfirmPay.Enabled = false;
            SyncConfirmButtonStateFromDb(orderId);
            RefreshAll();                   // โหลด QR และยอดรวม
        }

        // =============== PIPE หลัก ===============
        private void RefreshAll()
        {
            decimal total = GetCartGrandTotal(userEmail);
            var breakdown = ComputeVatBreakdown(total, pricesIncludeVat: false, vatRate: 0.07m);
            ShowBreakdownOnLabels(breakdown);

            // สร้าง PromptPay QR
            string target0066 = NormalizeThaiMobileTo0066(promptPayTargetMobileRaw);
            string payload = BuildPromptPayPayload_Mobile(target0066, breakdown.GrandTotal);
            RenderQrToPictureBox(payload);
        }

        // =============== ส่วนจัดการข้อมูลลูกค้า ===============
        private struct CustomerInfo
        {
            public string FullName;
            public string Phone;
            public string Address;
        }

        private CustomerInfo GetCustomerInfo(string email)
        {
            // ค่าเริ่มต้น
            var info = new CustomerInfo { FullName = "ลูกค้า (ไม่ระบุ)", Phone = "-", Address = "-" };

            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    // ใช้ตาราง user_info และชื่อคอลัมน์ name, number
                    string sql = "SELECT name, number, address FROM user_info WHERE email = @em";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@em", email);
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                info.FullName = rd["name"] == DBNull.Value ? "-" : rd["name"].ToString();
                                info.Phone = rd["number"] == DBNull.Value ? "-" : rd["number"].ToString();
                                info.Address = rd["address"] == DBNull.Value ? "-" : rd["address"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting customer info: " + ex.Message);
            }

            return info;
        }

        private string GenerateRandomReceiptId()
        {
            var random = new Random();
            return random.Next(10000000, 99999999).ToString();
        }

        // =============== ส่วนคำนวณเงิน ===============
        private decimal GetCartGrandTotal(string email)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "SELECT COALESCE(SUM(totalprice),0) FROM cart WHERE user_email = @em";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@em", email);
                    object obj = cmd.ExecuteScalar();
                    if (obj == null || obj == DBNull.Value) return 0m;

                    decimal total;
                    decimal.TryParse(obj.ToString(), out total);
                    return total;
                }
            }
        }

        private (decimal Net, decimal Vat, decimal GrandTotal) ComputeVatBreakdown(decimal total, bool pricesIncludeVat, decimal vatRate)
        {
            if (vatRate < 0) vatRate = 0;
            if (!pricesIncludeVat)
            {
                decimal net = total;
                decimal vat = Math.Round(net * vatRate, 2, MidpointRounding.AwayFromZero);
                return (net, vat, net + vat);
            }
            else
            {
                decimal net = Math.Round(total / (1 + vatRate), 2, MidpointRounding.AwayFromZero);
                decimal vat = total - net;
                return (net, vat, total);
            }
        }

        private void ShowBreakdownOnLabels((decimal Net, decimal Vat, decimal GrandTotal) b)
        {
            if (lblNet != null) lblNet.Text = $"(Net): ฿{b.Net:N2}";
            if (lblVat != null) lblVat.Text = $" (VAT 7%): ฿{b.Vat:N2}";
            if (lblSubtotal != null) lblSubtotal.Text = $" (Subtotal): ฿{(b.Net + b.Vat):N2}";
            if (lblGrand != null) lblGrand.Text = $" (Grand Total): ฿{b.GrandTotal:N2}";
        }

        // =============== ส่วน QR Code PromptPay ===============
        private string NormalizeThaiMobileTo0066(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile)) throw new ArgumentException("กรุณาใส่หมายเลขมือถือ PromptPay");
            string m = mobile.Trim().Replace(" ", "").Replace("-", "");
            if (m.StartsWith("0066")) return m;
            if (m.StartsWith("+66")) return "00" + m.Substring(1);
            if (m.StartsWith("0") && m.Length >= 10) return "0066" + m.Substring(1);
            if (m.Length >= 9 && char.IsDigit(m[0])) return "0066" + m;
            throw new ArgumentException("รูปแบบหมายเลข PromptPay ไม่ถูกต้อง");
        }

        private string BuildPromptPayPayload_Mobile(string targetMobile0066, decimal amount)
        {
            string TLV(string tag, string value) { return tag + value.Length.ToString("00") + value; }

            string mai = TLV("29", TLV("00", "A000000677010111") + TLV("01", targetMobile0066));
            string payload =
                TLV("00", "01") +
                TLV("01", "12") +
                mai +
                TLV("52", "0000") +
                TLV("53", "764") +
                TLV("54", amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)) +
                TLV("58", "TH") +
                TLV("59", "SHOP") +
                TLV("60", "BANGKOK");

            string crc = CalcCrc16Ccitt(payload + "6304").ToUpperInvariant();
            return payload + TLV("63", crc);
        }

        private string CalcCrc16Ccitt(string s)
        {
            byte[] data = Encoding.ASCII.GetBytes(s);
            ushort crc = 0xFFFF;
            foreach (byte b in data)
            {
                crc ^= (ushort)(b << 8);
                for (int i = 0; i < 8; i++)
                    crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1);
            }
            return crc.ToString("X4");
        }

        private void RenderQrToPictureBox(string payload)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions { Height = 200, Width = 200, Margin = 1 }
            };
            picQR.Image = writer.Write(payload);
        }

        // =============== ส่วนอัปโหลดสลิป ===============
        private void btnUploadSlip_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = "เลือกสลิปโอนเงิน",
                Filter = "รูปภาพ (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            picSlip.Image = System.Drawing.Image.FromFile(ofd.FileName);

            byte[] slipBytes = System.IO.File.ReadAllBytes(ofd.FileName);
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string up = "UPDATE orders SET payment_slip=@b, status='paid', paid_at=NOW() WHERE order_id=@id";

                using (var cmd = new MySqlCommand(up, conn))
                {
                    cmd.Parameters.AddWithValue("@b", slipBytes);
                    cmd.Parameters.AddWithValue("@id", orderId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("อัปโหลดสลิปเรียบร้อย ✅\nกด 'ยืนยันการชำระ' เพื่อออกใบเสร็จ PDF");
            _slipUploaded = true;
            btnConfirmPay.Enabled = true;
        }

        // =============== ส่วนสร้าง PDF ใบเสร็จ (ปรับปรุงใหม่: เส้นคมชัด, หน้ากระชับ) ===============
        private Tuple<string, System.Drawing.Image> GenerateReceiptPdf(long oid)
        {
            string folderPath = @"C:\ปี 3\C#\slip";
            if (!System.IO.Directory.Exists(folderPath)) System.IO.Directory.CreateDirectory(folderPath);

            string filePath = System.IO.Path.Combine(folderPath, $"Receipt_{oid}.pdf");

            string status = "";
            decimal subtotal = 0;
            DateTime? paidAt = null;
            var dtItems = new DataTable();

            var customer = GetCustomerInfo(userEmail);
            string receiptId = oid.ToString().PadLeft(8, '0');

            using (var conn = new MySql.Data.MySqlClient.MySqlConnection(connStr))
            {
                conn.Open();
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(
                  "SELECT total_price, status, paid_at FROM orders WHERE order_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", oid);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            subtotal = Convert.ToDecimal(rd["total_price"]);
                            status = rd["status"].ToString();
                            paidAt = rd["paid_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["paid_at"]);
                        }
                    }
                }

                using (var da = new MySql.Data.MySqlClient.MySqlDataAdapter(
                  "SELECT product_name, quantity, price, total_price FROM order_items WHERE order_id=@id", conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@id", oid);
                    da.Fill(dtItems);
                }
            }

            // คำนวณ VAT และ GrandTotal 
            decimal vatRate = 0.07m;
            decimal vatAmount = Math.Round(subtotal * vatRate, 2);
            decimal grandTotal = subtotal + vatAmount;

            // ** คำนวณความสูงหน้าแบบกระชับ (Compact) **
            int baseHeight = 320; // ลดความสูงตั้งต้น
            int linePerItem = 25; // ความสูงต่อรายการ
            int extra = 60;       // เผื่อท้ายกระดาษนิดเดียว
            int pageHeight = baseHeight + linePerItem * dtItems.Rows.Count + extra;
            if (pageHeight < 400) pageHeight = 400; // ขั้นต่ำ 400

            var pageSize = new iTextSharp.text.Rectangle(226f, pageHeight);

            using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                var doc = new iTextSharp.text.Document(pageSize, 10, 10, 10, 10);
                iTextSharp.text.pdf.PdfWriter.GetInstance(doc, fs);
                doc.Open();

                string fontPath = @"C:\ปี 3\C#\font\THSarabunNew.ttf";
                if (!System.IO.File.Exists(fontPath))
                {
                    MessageBox.Show("ไม่พบไฟล์ฟอนต์ที่: " + fontPath);
                    return null;
                }

                var bf = iTextSharp.text.pdf.BaseFont.CreateFont(fontPath, iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.EMBEDDED);
                var fontTitle = new iTextSharp.text.Font(bf, 16, iTextSharp.text.Font.BOLD);
                var fontHeader = new iTextSharp.text.Font(bf, 12, iTextSharp.text.Font.BOLD);
                var fontN = new iTextSharp.text.Font(bf, 11, iTextSharp.text.Font.NORMAL);
                var fontB = new iTextSharp.text.Font(bf, 11, iTextSharp.text.Font.BOLD);

                // ** สร้างวัตถุสำหรับเส้นคั่น (LineSeparator) เพื่อความคมชัด **
                LineSeparator lineSeparator = new LineSeparator(0.5f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, -1);

                // 1. โลโก้
                try
                {
                    string logoPath = @"C:\ปี 3\C#\รูป\logo.png";
                    if (System.IO.File.Exists(logoPath))
                    {
                        var logo = iTextSharp.text.Image.GetInstance(logoPath);
                        logo.ScaleToFit(80f, 80f);
                        logo.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                        logo.SpacingAfter = 2f;
                        doc.Add(logo);
                    }
                }
                catch { }

                var center = iTextSharp.text.Element.ALIGN_CENTER;
                var left = iTextSharp.text.Element.ALIGN_LEFT;
                var right = iTextSharp.text.Element.ALIGN_RIGHT;

                // 2. ชื่อเอกสาร
                doc.Add(new iTextSharp.text.Paragraph("ใบเสร็จรับเงิน / RECEIPT", fontTitle) { Alignment = center, SpacingAfter = 5f });

                // เส้นคั่นที่ 1
                doc.Add(new Chunk(lineSeparator));
                doc.Add(new iTextSharp.text.Paragraph(" ", new iTextSharp.text.Font(bf, 4))); // เว้นนิดหน่อย

                // 3. ข้อมูลวันที่และลูกค้า
                string dateStr = paidAt.HasValue ? paidAt.Value.ToString("dd/MM/yyyy HH:mm", new CultureInfo("th-TH")) : DateTime.Now.ToString("dd/MM/yyyy HH:mm", new CultureInfo("th-TH"));

                // ใช้ Paragraph แบบลดระยะบรรทัด (Leading)
                var pInfo = new iTextSharp.text.Paragraph();
                pInfo.SetLeading(12f, 0f); // ลดระยะห่างระหว่างบรรทัด
                pInfo.Add(new Chunk($"วันที่: {dateStr}\n", fontN));
                pInfo.Add(new Chunk($"ลูกค้า: {customer.FullName}\n", fontN));
                pInfo.Add(new Chunk($"เบอร์โทร: {customer.Phone}\n", fontN));
                pInfo.Add(new Chunk($"ที่อยู่: {customer.Address}", fontN));
                doc.Add(pInfo);

                // เส้นคั่นที่ 2
                doc.Add(new iTextSharp.text.Paragraph(" ", new iTextSharp.text.Font(bf, 4)));
                doc.Add(new Chunk(lineSeparator));
                doc.Add(new iTextSharp.text.Paragraph(" ", new iTextSharp.text.Font(bf, 4)));

                // 4. ตารางรายการสินค้า
                var t = new iTextSharp.text.pdf.PdfPTable(2) { WidthPercentage = 100 };
                t.SetWidths(new float[] { 70, 30 });
                t.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                // หัวตาราง
                t.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("รายการสินค้า", fontHeader)) { Border = iTextSharp.text.Rectangle.NO_BORDER, HorizontalAlignment = left, PaddingBottom = 2 });
                t.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("ราคา", fontHeader)) { Border = iTextSharp.text.Rectangle.NO_BORDER, HorizontalAlignment = right, PaddingBottom = 2 });

                int i = 1;
                foreach (DataRow r in dtItems.Rows)
                {
                    string pName = r["product_name"].ToString();
                    decimal qty = Convert.ToDecimal(r["quantity"]);
                    decimal price = Convert.ToDecimal(r["price"]);
                    decimal totalItem = Convert.ToDecimal(r["total_price"]);

                    // จัดรูปแบบสินค้า: ชื่อสินค้าอยู่บน, จำนวน x ราคาอยู่ล่าง
                    string detailLeft = $"{i}. {pName}\n    {qty:N0} x {price:N2}";

                    var cLeft = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(detailLeft, fontN));
                    cLeft.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    cLeft.PaddingBottom = 4; // ระยะห่างระหว่างรายการ
                    t.AddCell(cLeft);

                    // ราคารวมของรายการ
                    string detailRight = $"\n{totalItem:N2}";
                    var cRight = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(detailRight, fontN));
                    cRight.HorizontalAlignment = right;
                    cRight.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    cRight.PaddingBottom = 4;
                    t.AddCell(cRight);

                    i++;
                }
                doc.Add(t);

                // เส้นคั่นที่ 3
                doc.Add(new iTextSharp.text.Paragraph(" ", new iTextSharp.text.Font(bf, 4)));
                doc.Add(new Chunk(lineSeparator));
                doc.Add(new iTextSharp.text.Paragraph(" ", new iTextSharp.text.Font(bf, 4)));

                // 5. สรุปยอดเงิน
                var tTotal = new iTextSharp.text.pdf.PdfPTable(2) { WidthPercentage = 100 };
                tTotal.SetWidths(new float[] { 60, 40 });
                tTotal.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                void AddTotalRow(string label, string val, iTextSharp.text.Font f)
                {
                    tTotal.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(label, f)) { Border = 0, HorizontalAlignment = left, PaddingBottom = 2 });
                    tTotal.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(val, f)) { Border = 0, HorizontalAlignment = right, PaddingBottom = 2 });
                }

                AddTotalRow("ราคา:", $"{subtotal:N2} บาท", fontN);
                AddTotalRow("ภาษีมูลค่าเพิ่ม(7%):", $"{vatAmount:N2} บาท", fontN);
                AddTotalRow("ยอดรวมสุทธิ:", $"{grandTotal:N2} บาท", fontB);

                doc.Add(tTotal);

                // 6. Footer
                doc.Add(new iTextSharp.text.Paragraph(" ", new iTextSharp.text.Font(bf, 10))); // เว้นนิดหน่อย
                doc.Add(new iTextSharp.text.Paragraph("***ขอบคุณที่อุดหนุนร้าน PAKSOD ORGANIC ค่ะ***", fontN) { Alignment = center });

                doc.Close();
            }

            System.Drawing.Image preview = null;
            if (picSlip.Image != null)
            {
                preview = new Bitmap((System.Drawing.Image)picSlip.Image, new Size(220, 300));
            }

            return Tuple.Create(filePath, preview);
        }

        private void btnConfirmPay_Click_1(object sender, EventArgs e)
        {
            if (!_slipUploaded)
            {
                MessageBox.Show("กรุณาอัปโหลดสลิปก่อนยืนยันการชำระเงิน");
                return;
            }

            if (!DeductStockForOrder(orderId)) return;

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string clearSql = "DELETE FROM cart WHERE user_email=@em";
                using (var cmd = new MySqlCommand(clearSql, conn))
                {
                    cmd.Parameters.AddWithValue("@em", userEmail);
                    cmd.ExecuteNonQuery();
                }
            }

            try
            {
                var result = GenerateReceiptPdf(orderId);
                if (result == null) { MessageBox.Show("ยกเลิกการสร้างใบเสร็จ"); return; }

                receiptPdfPath = result.Item1;
                SaveReceiptPathToDb(orderId, receiptPdfPath);

                Process.Start(receiptPdfPath);

                btnConfirmPay.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถเปิดไฟล์ใบเสร็จได้\n" + ex.Message);
            }
        }

        private void SaveReceiptPathToDb(long oid, string pdfPath)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "UPDATE orders SET receipt = @path WHERE order_id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@path", pdfPath ?? "");
                    cmd.Parameters.AddWithValue("@id", oid);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private bool DeductStockForOrder(long oid)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        string chkSql = "SELECT COUNT(*) FROM order_items oi JOIN products p ON p.name = oi.product_name WHERE oi.order_id = @oid AND p.stock < oi.quantity";
                        using (var chk = new MySqlCommand(chkSql, conn, tx))
                        {
                            chk.Parameters.AddWithValue("@oid", oid);
                            var lack = Convert.ToInt32(chk.ExecuteScalar());
                            if (lack > 0)
                            {
                                tx.Rollback();
                                MessageBox.Show("ตัดสต็อกไม่สำเร็จ: มีสินค้าสต็อกไม่พอ");
                                return false;
                            }
                        }

                        string updSql = "UPDATE products p JOIN order_items oi ON oi.order_id = @oid AND oi.product_name = p.name SET p.stock = p.stock - oi.quantity";
                        using (var upd = new MySqlCommand(updSql, conn, tx))
                        {
                            upd.Parameters.AddWithValue("@oid", oid);
                            upd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        try { tx.Rollback(); } catch { }
                        MessageBox.Show("ตัดสต็อกไม่สำเร็จ: " + ex.Message);
                        return false;
                    }
                }
            }
        }

        private void SyncConfirmButtonStateFromDb(long oid)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "SELECT payment_slip IS NOT NULL FROM orders WHERE order_id=@id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", oid);
                    var hasSlip = cmd.ExecuteScalar();
                    bool ok = false;
                    if (hasSlip != null && hasSlip != DBNull.Value)
                    {
                        ok = Convert.ToInt32(hasSlip) == 1;
                    }
                    _slipUploaded = ok;
                    btnConfirmPay.Enabled = ok;
                }
            }
        }
    }
}