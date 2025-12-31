namespace WiFi_hotspot_helper
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.hotspot_name = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.hotspot_password = new System.Windows.Forms.TextBox();
            this.checkBox_autoRun = new System.Windows.Forms.CheckBox();
            this.checkBox_autoManage = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.textBox_log = new System.Windows.Forms.TextBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.radioButton_Mobile = new System.Windows.Forms.RadioButton();
            this.radioButton_netsh = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.hotspot_name);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(259, 51);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "热点名称";
            // 
            // hotspot_name
            // 
            this.hotspot_name.Location = new System.Drawing.Point(6, 15);
            this.hotspot_name.Name = "hotspot_name";
            this.hotspot_name.Size = new System.Drawing.Size(243, 21);
            this.hotspot_name.TabIndex = 0;
            this.hotspot_name.TextChanged += new System.EventHandler(this.hotspot_name_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.hotspot_password);
            this.groupBox2.Location = new System.Drawing.Point(12, 69);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(259, 53);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "热点密码";
            // 
            // hotspot_password
            // 
            this.hotspot_password.Location = new System.Drawing.Point(6, 25);
            this.hotspot_password.Name = "hotspot_password";
            this.hotspot_password.Size = new System.Drawing.Size(243, 21);
            this.hotspot_password.TabIndex = 0;
            this.hotspot_password.TextChanged += new System.EventHandler(this.hotspot_password_TextChanged);
            // 
            // checkBox_autoRun
            // 
            this.checkBox_autoRun.AutoSize = true;
            this.checkBox_autoRun.Location = new System.Drawing.Point(277, 109);
            this.checkBox_autoRun.Name = "checkBox_autoRun";
            this.checkBox_autoRun.Size = new System.Drawing.Size(72, 16);
            this.checkBox_autoRun.TabIndex = 2;
            this.checkBox_autoRun.Text = "开机运行";
            this.checkBox_autoRun.UseVisualStyleBackColor = true;
            this.checkBox_autoRun.CheckedChanged += new System.EventHandler(this.checkBox_autoRun_CheckedChanged);
            // 
            // checkBox_autoManage
            // 
            this.checkBox_autoManage.AutoSize = true;
            this.checkBox_autoManage.Location = new System.Drawing.Point(361, 109);
            this.checkBox_autoManage.Name = "checkBox_autoManage";
            this.checkBox_autoManage.Size = new System.Drawing.Size(72, 16);
            this.checkBox_autoManage.TabIndex = 3;
            this.checkBox_autoManage.Text = "自动管理";
            this.checkBox_autoManage.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.textBox_log);
            this.groupBox4.Location = new System.Drawing.Point(12, 220);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(427, 100);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "日志";
            // 
            // textBox_log
            // 
            this.textBox_log.AcceptsReturn = true;
            this.textBox_log.AcceptsTab = true;
            this.textBox_log.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_log.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_log.Location = new System.Drawing.Point(3, 17);
            this.textBox_log.Multiline = true;
            this.textBox_log.Name = "textBox_log";
            this.textBox_log.ReadOnly = true;
            this.textBox_log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_log.Size = new System.Drawing.Size(421, 80);
            this.textBox_log.TabIndex = 0;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.button2);
            this.groupBox5.Controls.Add(this.button1);
            this.groupBox5.Controls.Add(this.comboBox1);
            this.groupBox5.Controls.Add(this.label1);
            this.groupBox5.Controls.Add(this.textBox1);
            this.groupBox5.Location = new System.Drawing.Point(18, 129);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(415, 85);
            this.groupBox5.TabIndex = 8;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "绑定的适配器";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(304, 50);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "绑定";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(9, 50);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(49, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "刷新";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(64, 52);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(234, 20);
            this.comboBox1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "此适配器连接后再使用热点：";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(174, 26);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(235, 21);
            this.textBox1.TabIndex = 0;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.radioButton_Mobile);
            this.groupBox6.Controls.Add(this.radioButton_netsh);
            this.groupBox6.Location = new System.Drawing.Point(277, 12);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(162, 91);
            this.groupBox6.TabIndex = 9;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "执行方式";
            // 
            // radioButton_Mobile
            // 
            this.radioButton_Mobile.AutoSize = true;
            this.radioButton_Mobile.Location = new System.Drawing.Point(6, 42);
            this.radioButton_Mobile.Name = "radioButton_Mobile";
            this.radioButton_Mobile.Size = new System.Drawing.Size(107, 16);
            this.radioButton_Mobile.TabIndex = 1;
            this.radioButton_Mobile.TabStop = true;
            this.radioButton_Mobile.Text = "Mobile Hotspot";
            this.radioButton_Mobile.UseVisualStyleBackColor = true;
            this.radioButton_Mobile.CheckedChanged += new System.EventHandler(this.radioButton_Mobile_CheckedChanged);
            // 
            // radioButton_netsh
            // 
            this.radioButton_netsh.AutoSize = true;
            this.radioButton_netsh.Location = new System.Drawing.Point(6, 20);
            this.radioButton_netsh.Name = "radioButton_netsh";
            this.radioButton_netsh.Size = new System.Drawing.Size(53, 16);
            this.radioButton_netsh.TabIndex = 0;
            this.radioButton_netsh.TabStop = true;
            this.radioButton_netsh.Text = "netsh";
            this.radioButton_netsh.UseVisualStyleBackColor = true;
            this.radioButton_netsh.CheckedChanged += new System.EventHandler(this.radioButton_netsh_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 332);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.checkBox_autoManage);
            this.Controls.Add(this.checkBox_autoRun);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WiFi 热点助手";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox hotspot_name;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox hotspot_password;
        private System.Windows.Forms.CheckBox checkBox_autoRun;
        private System.Windows.Forms.CheckBox checkBox_autoManage;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox textBox_log;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.RadioButton radioButton_Mobile;
        private System.Windows.Forms.RadioButton radioButton_netsh;
    }
}

