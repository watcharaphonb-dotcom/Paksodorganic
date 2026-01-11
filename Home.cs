using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_pak
{
    public partial class Home : Form
    {
        public Home(){InitializeComponent();}

        

        private void Sign_up_Click(object sender, EventArgs e)
        {
        SignupForm signupForm = new SignupForm();
        signupForm.Show();
        this.Hide();
        }

        private void Sign_in_Click(object sender, EventArgs e)
        {
            var signin = new SigninForm(this); // ✅ ส่ง Form1 เข้าไป
            signin.Show();
            this.Hide();
        }

        private void Home_Load(object sender, EventArgs e)
        {

        }

        private void btnContact_Click(object sender, EventArgs e)
        {
            Contact contactForm = new Contact();
            this.Hide();
            contactForm.Show();
        }
    }

}
