using System;
using System.Windows.Forms;

namespace test_pak
{
    internal static class Program
    {
        // ✅ ต้องเป็น static และชื่อให้ตรงกับที่ไฟล์อื่นเรียกใช้ (ConnStr)
        public static string ConnStr = "Server=127.0.0.1;Port=3306;Database=leafy_farm;Uid=root;Pwd=;CharSet=utf8mb4;SslMode=Preferred;AllowPublicKeyRetrieval=True;";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Home());
        }
    }
}
