namespace ClassroomChecker.UI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.btnQuickSetup = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabWallpaper = new System.Windows.Forms.TabPage();
            this.tabScan = new System.Windows.Forms.TabPage();
            this.tabSecurity = new System.Windows.Forms.TabPage();
            this.tabInstall = new System.Windows.Forms.TabPage();
            this.scanPanel = new ClassroomChecker.UI.ScanPanel();
            this.wallpaperPanel = new ClassroomChecker.UI.WallpaperPanel();
            this.securityPanel = new ClassroomChecker.UI.SecurityPanel();
            this.softwareInstallPanel = new ClassroomChecker.UI.SoftwareInstallPanel();
            this.pnlHeader.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabWallpaper.SuspendLayout();
            this.tabScan.SuspendLayout();
            this.tabSecurity.SuspendLayout();
            this.tabInstall.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(115)))), ((int)(((byte)(232)))));
            this.pnlHeader.Controls.Add(this.btnQuickSetup);
            this.pnlHeader.Controls.Add(this.btnSettings);
            this.pnlHeader.Controls.Add(this.btnHelp);
            this.pnlHeader.Controls.Add(this.cmbLanguage);
            this.pnlHeader.Controls.Add(this.lblLanguage);
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Controls.Add(this.lblAppTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this.pnlHeader.Size = new System.Drawing.Size(984, 72);
            this.pnlHeader.TabIndex = 0;
            // 
            // btnQuickSetup
            // 
            this.btnQuickSetup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnQuickSetup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnQuickSetup.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnQuickSetup.FlatAppearance.BorderSize = 0;
            this.btnQuickSetup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnQuickSetup.Font = new System.Drawing.Font("Phetsarath OT", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuickSetup.ForeColor = System.Drawing.Color.White;
            this.btnQuickSetup.Location = new System.Drawing.Point(415, 17);
            this.btnQuickSetup.Name = "btnQuickSetup";
            this.btnQuickSetup.Size = new System.Drawing.Size(145, 36);
            this.btnQuickSetup.TabIndex = 4;
            this.btnQuickSetup.Text = "⚡ ຕັ້ງຄ່າດ່ວນ";
            this.btnQuickSetup.UseVisualStyleBackColor = false;
            this.btnQuickSetup.Click += new System.EventHandler(this.btnQuickSetup_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSettings.BackColor = System.Drawing.Color.White;
            this.btnSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Font = new System.Drawing.Font("Phetsarath OT", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(115)))), ((int)(((byte)(232)))));
            this.btnSettings.Location = new System.Drawing.Point(568, 17);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(100, 36);
            this.btnSettings.TabIndex = 5;
            this.btnSettings.Text = "⚙️ ຕັ້ງຄ່າ";
            this.btnSettings.UseVisualStyleBackColor = false;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHelp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.btnHelp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHelp.FlatAppearance.BorderSize = 0;
            this.btnHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHelp.Font = new System.Drawing.Font("Phetsarath OT", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHelp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.btnHelp.Location = new System.Drawing.Point(676, 17);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(95, 36);
            this.btnHelp.TabIndex = 6;
            this.btnHelp.Text = "📖 ຄູ່ມື";
            this.btnHelp.UseVisualStyleBackColor = false;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguage.Font = new System.Drawing.Font("Phetsarath OT", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Items.AddRange(new object[] {
            "ພາສາລາວ (Lao)",
            "English (EN)"});
            this.cmbLanguage.Location = new System.Drawing.Point(825, 22);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(143, 25);
            this.cmbLanguage.TabIndex = 3;
            this.cmbLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbLanguage_SelectedIndexChanged);
            // 
            // lblLanguage
            // 
            this.lblLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Font = new System.Drawing.Font("Phetsarath OT", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLanguage.ForeColor = System.Drawing.Color.White;
            this.lblLanguage.Location = new System.Drawing.Point(775, 26);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(46, 18);
            this.lblLanguage.TabIndex = 2;
            this.lblLanguage.Text = "ພາສາ:";
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(40, 15);
            this.lblLanguage.TabIndex = 2;
            this.lblLanguage.Text = "ພາສາ:";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Phetsarath OT", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(240)))), ((int)(((byte)(255)))));
            this.lblSubtitle.Location = new System.Drawing.Point(17, 40);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(434, 17);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "ເຄື່ອງມືກວດສອບ, ຕັ້ງ Wallpaper ແລະ ບຳລຸງຮັກສາຄອມພິວເຕີຫ້ອງຮຽນ (Portable)";
            // 
            // lblAppTitle
            // 
            this.lblAppTitle.AutoSize = true;
            this.lblAppTitle.Font = new System.Drawing.Font("Phetsarath OT", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAppTitle.ForeColor = System.Drawing.Color.White;
            this.lblAppTitle.Location = new System.Drawing.Point(16, 12);
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Size = new System.Drawing.Size(182, 25);
            this.lblAppTitle.TabIndex = 0;
            this.lblAppTitle.Text = "Classroom Checker";
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabWallpaper);
            this.tabMain.Controls.Add(this.tabScan);
            this.tabMain.Controls.Add(this.tabSecurity);
            this.tabMain.Controls.Add(this.tabInstall);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Font = new System.Drawing.Font("Phetsarath OT", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabMain.ItemSize = new System.Drawing.Size(140, 32);
            this.tabMain.Location = new System.Drawing.Point(0, 72);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(984, 589);
            this.tabMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabMain.TabIndex = 1;
            // 
            // tabWallpaper
            // 
            this.tabWallpaper.Controls.Add(this.wallpaperPanel);
            this.tabWallpaper.Location = new System.Drawing.Point(4, 36);
            this.tabWallpaper.Name = "tabWallpaper";
            this.tabWallpaper.Padding = new System.Windows.Forms.Padding(3);
            this.tabWallpaper.Size = new System.Drawing.Size(976, 549);
            this.tabWallpaper.TabIndex = 0;
            this.tabWallpaper.Text = "ພື້ນຫຼັງ";
            this.tabWallpaper.UseVisualStyleBackColor = true;
            // 
            // 
            // tabScan
            // 
            this.tabScan.Controls.Add(this.scanPanel);
            this.tabScan.Location = new System.Drawing.Point(4, 36);
            this.tabScan.Name = "tabScan";
            this.tabScan.Padding = new System.Windows.Forms.Padding(3);
            this.tabScan.Size = new System.Drawing.Size(976, 549);
            this.tabScan.TabIndex = 1;
            this.tabScan.Text = "ກວດສອບລະບົບ";
            this.tabScan.UseVisualStyleBackColor = true;
            // 
            // scanPanel
            // 
            this.scanPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scanPanel.Font = new System.Drawing.Font("Phetsarath OT", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scanPanel.Location = new System.Drawing.Point(3, 3);
            this.scanPanel.Name = "scanPanel";
            this.scanPanel.Size = new System.Drawing.Size(970, 543);
            this.scanPanel.TabIndex = 0;
            // 
            // wallpaperPanel
            // 
            this.wallpaperPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wallpaperPanel.Font = new System.Drawing.Font("Phetsarath OT", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wallpaperPanel.Location = new System.Drawing.Point(3, 3);
            this.wallpaperPanel.Name = "wallpaperPanel";
            this.wallpaperPanel.Size = new System.Drawing.Size(970, 543);
            this.wallpaperPanel.TabIndex = 0;
            // 
            // tabSecurity
            // 
            this.tabSecurity.Controls.Add(this.securityPanel);
            this.tabSecurity.Location = new System.Drawing.Point(4, 36);
            this.tabSecurity.Name = "tabSecurity";
            this.tabSecurity.Padding = new System.Windows.Forms.Padding(3);
            this.tabSecurity.Size = new System.Drawing.Size(976, 549);
            this.tabSecurity.TabIndex = 2;
            this.tabSecurity.Text = "ປ້ອງກັນ";
            this.tabSecurity.UseVisualStyleBackColor = true;
            // 
            // securityPanel
            // 
            this.securityPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.securityPanel.Font = new System.Drawing.Font("Phetsarath OT", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.securityPanel.Location = new System.Drawing.Point(3, 3);
            this.securityPanel.Name = "securityPanel";
            this.securityPanel.Size = new System.Drawing.Size(970, 543);
            this.securityPanel.TabIndex = 0;
            // 
            // tabInstall
            // 
            this.tabInstall.Controls.Add(this.softwareInstallPanel);
            this.tabInstall.Location = new System.Drawing.Point(4, 36);
            this.tabInstall.Name = "tabInstall";
            this.tabInstall.Padding = new System.Windows.Forms.Padding(3);
            this.tabInstall.Size = new System.Drawing.Size(976, 549);
            this.tabInstall.TabIndex = 3;
            this.tabInstall.Text = "ຕິດຕັ້ງຊອບແວ";
            this.tabInstall.UseVisualStyleBackColor = true;
            // 
            // softwareInstallPanel
            // 
            this.softwareInstallPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.softwareInstallPanel.Font = new System.Drawing.Font("Phetsarath OT", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.softwareInstallPanel.Location = new System.Drawing.Point(3, 3);
            this.softwareInstallPanel.Name = "softwareInstallPanel";
            this.softwareInstallPanel.Size = new System.Drawing.Size(970, 543);
            this.softwareInstallPanel.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 661);
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Phetsarath OT", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(860, 580);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Classroom Checker";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.tabMain.ResumeLayout(false);
            this.tabWallpaper.ResumeLayout(false);
            this.tabScan.ResumeLayout(false);
            this.tabSecurity.ResumeLayout(false);
            this.tabInstall.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Button btnQuickSetup;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.ComboBox cmbLanguage;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabWallpaper;
        private System.Windows.Forms.TabPage tabScan;
        private System.Windows.Forms.TabPage tabSecurity;
        private System.Windows.Forms.TabPage tabInstall;
        private ClassroomChecker.UI.ScanPanel scanPanel;
        private ClassroomChecker.UI.WallpaperPanel wallpaperPanel;
        private ClassroomChecker.UI.SecurityPanel securityPanel;
        private ClassroomChecker.UI.SoftwareInstallPanel softwareInstallPanel;
    }
}
