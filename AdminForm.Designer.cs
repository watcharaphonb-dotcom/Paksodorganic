namespace test_pak
{
    partial class AdminForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSalesReport = new System.Windows.Forms.Button();
            this.btnCustomers = new System.Windows.Forms.Button();
            this.btnProducts = new System.Windows.Forms.Button();
            this.btnSearch = new System.Windows.Forms.Button();
            this.comboMonth = new System.Windows.Forms.ComboBox();
            this.comboDay = new System.Windows.Forms.ComboBox();
            this.comboYear = new System.Windows.Forms.ComboBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.lblTotalSum = new System.Windows.Forms.Label();
            this.btnHome = new System.Windows.Forms.Button();
            this.btnBestSelling = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSalesReport
            // 
            this.btnSalesReport.BackColor = System.Drawing.Color.Transparent;
            this.btnSalesReport.FlatAppearance.BorderSize = 0;
            this.btnSalesReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalesReport.ForeColor = System.Drawing.Color.Transparent;
            this.btnSalesReport.Location = new System.Drawing.Point(28, 254);
            this.btnSalesReport.Margin = new System.Windows.Forms.Padding(5);
            this.btnSalesReport.Name = "btnSalesReport";
            this.btnSalesReport.Size = new System.Drawing.Size(71, 53);
            this.btnSalesReport.TabIndex = 3;
            this.btnSalesReport.UseVisualStyleBackColor = false;
            this.btnSalesReport.Click += new System.EventHandler(this.btnSalesReport_Click);
            // 
            // btnCustomers
            // 
            this.btnCustomers.BackColor = System.Drawing.Color.Transparent;
            this.btnCustomers.FlatAppearance.BorderSize = 0;
            this.btnCustomers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCustomers.ForeColor = System.Drawing.Color.Transparent;
            this.btnCustomers.Location = new System.Drawing.Point(28, 317);
            this.btnCustomers.Margin = new System.Windows.Forms.Padding(5);
            this.btnCustomers.Name = "btnCustomers";
            this.btnCustomers.Size = new System.Drawing.Size(71, 54);
            this.btnCustomers.TabIndex = 4;
            this.btnCustomers.UseVisualStyleBackColor = false;
            this.btnCustomers.Click += new System.EventHandler(this.btnCustomer);
            // 
            // btnProducts
            // 
            this.btnProducts.BackColor = System.Drawing.Color.Transparent;
            this.btnProducts.FlatAppearance.BorderSize = 0;
            this.btnProducts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProducts.ForeColor = System.Drawing.Color.Transparent;
            this.btnProducts.Location = new System.Drawing.Point(28, 381);
            this.btnProducts.Margin = new System.Windows.Forms.Padding(5);
            this.btnProducts.Name = "btnProducts";
            this.btnProducts.Size = new System.Drawing.Size(71, 59);
            this.btnProducts.TabIndex = 5;
            this.btnProducts.UseVisualStyleBackColor = false;
            this.btnProducts.Click += new System.EventHandler(this.btnProduct_Click);
            // 
            // btnSearch
            // 
            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(234)))), ((int)(((byte)(254)))));
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Nirmala UI", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSearch.ForeColor = System.Drawing.Color.Silver;
            this.btnSearch.Location = new System.Drawing.Point(1030, 167);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(5);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(186, 40);
            this.btnSearch.TabIndex = 6;
            this.btnSearch.Text = "แสดงรายงาน";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // comboMonth
            // 
            this.comboMonth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(239)))), ((int)(((byte)(239)))));
            this.comboMonth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboMonth.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboMonth.FormattingEnabled = true;
            this.comboMonth.Location = new System.Drawing.Point(463, 167);
            this.comboMonth.Margin = new System.Windows.Forms.Padding(5);
            this.comboMonth.Name = "comboMonth";
            this.comboMonth.Size = new System.Drawing.Size(217, 37);
            this.comboMonth.TabIndex = 12;
            this.comboMonth.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // comboDay
            // 
            this.comboDay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(239)))), ((int)(((byte)(239)))));
            this.comboDay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboDay.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboDay.FormattingEnabled = true;
            this.comboDay.Location = new System.Drawing.Point(216, 167);
            this.comboDay.Margin = new System.Windows.Forms.Padding(5);
            this.comboDay.Name = "comboDay";
            this.comboDay.Size = new System.Drawing.Size(124, 37);
            this.comboDay.TabIndex = 13;
            // 
            // comboYear
            // 
            this.comboYear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(239)))), ((int)(((byte)(239)))));
            this.comboYear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboYear.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboYear.FormattingEnabled = true;
            this.comboYear.Location = new System.Drawing.Point(780, 167);
            this.comboYear.Margin = new System.Windows.Forms.Padding(5);
            this.comboYear.Name = "comboYear";
            this.comboYear.Size = new System.Drawing.Size(191, 37);
            this.comboYear.TabIndex = 14;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(135, 222);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(1027, 374);
            this.dataGridView1.TabIndex = 17;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // lblTotalSum
            // 
            this.lblTotalSum.AutoSize = true;
            this.lblTotalSum.BackColor = System.Drawing.Color.Transparent;
            this.lblTotalSum.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblTotalSum.Font = new System.Drawing.Font("Nirmala UI", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalSum.Location = new System.Drawing.Point(927, 613);
            this.lblTotalSum.Name = "lblTotalSum";
            this.lblTotalSum.Size = new System.Drawing.Size(91, 38);
            this.lblTotalSum.TabIndex = 18;
            this.lblTotalSum.Text = "label2";
            this.lblTotalSum.Click += new System.EventHandler(this.lblTotal_Click);
            // 
            // btnHome
            // 
            this.btnHome.BackColor = System.Drawing.Color.Transparent;
            this.btnHome.FlatAppearance.BorderSize = 0;
            this.btnHome.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHome.ForeColor = System.Drawing.Color.Transparent;
            this.btnHome.Location = new System.Drawing.Point(28, 167);
            this.btnHome.Margin = new System.Windows.Forms.Padding(5);
            this.btnHome.Name = "btnHome";
            this.btnHome.Size = new System.Drawing.Size(71, 53);
            this.btnHome.TabIndex = 19;
            this.btnHome.UseVisualStyleBackColor = false;
            this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
            // 
            // btnBestSelling
            // 
            this.btnBestSelling.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(234)))), ((int)(((byte)(254)))));
            this.btnBestSelling.FlatAppearance.BorderSize = 0;
            this.btnBestSelling.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBestSelling.Font = new System.Drawing.Font("Nirmala UI", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBestSelling.ForeColor = System.Drawing.Color.Silver;
            this.btnBestSelling.Location = new System.Drawing.Point(1030, 115);
            this.btnBestSelling.Margin = new System.Windows.Forms.Padding(5);
            this.btnBestSelling.Name = "btnBestSelling";
            this.btnBestSelling.Size = new System.Drawing.Size(186, 42);
            this.btnBestSelling.TabIndex = 20;
            this.btnBestSelling.Text = "สินค้าขายดี";
            this.btnBestSelling.UseVisualStyleBackColor = false;
            this.btnBestSelling.Click += new System.EventHandler(this.btnBestSelling_Click);
            // 
            // AdminForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::test_pak.Properties.Resources.reporttttttttttttttttt;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1262, 673);
            this.Controls.Add(this.btnBestSelling);
            this.Controls.Add(this.btnHome);
            this.Controls.Add(this.lblTotalSum);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.comboYear);
            this.Controls.Add(this.comboDay);
            this.Controls.Add(this.comboMonth);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.btnProducts);
            this.Controls.Add(this.btnCustomers);
            this.Controls.Add(this.btnSalesReport);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Black;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.Name = "AdminForm";
            this.Text = "report";
            this.Load += new System.EventHandler(this.AdminForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnSalesReport;
        private System.Windows.Forms.Button btnCustomers;
        private System.Windows.Forms.Button btnProducts;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.ComboBox comboMonth;
        private System.Windows.Forms.ComboBox comboDay;
        private System.Windows.Forms.ComboBox comboYear;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label lblTotalSum;
        private System.Windows.Forms.Button btnHome;
        private System.Windows.Forms.Button btnBestSelling;
    }
}