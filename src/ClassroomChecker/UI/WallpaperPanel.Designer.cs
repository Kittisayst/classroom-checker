namespace ClassroomChecker.UI
{
    partial class WallpaperPanel
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.pnlControls = new System.Windows.Forms.Panel();
            this.grpWallpaperSettings = new System.Windows.Forms.GroupBox();
            this.btnDefaultBg = new System.Windows.Forms.Button();
            this.btnBrowseBg = new System.Windows.Forms.Button();
            this.txtBgPath = new System.Windows.Forms.TextBox();
            this.lblBgImage = new System.Windows.Forms.Label();
            this.numComputerNumber = new System.Windows.Forms.NumericUpDown();
            this.lblNumber = new System.Windows.Forms.Label();
            this.btnDeleteRoom = new System.Windows.Forms.Button();
            this.btnAddRoom = new System.Windows.Forms.Button();
            this.cmbRooms = new System.Windows.Forms.ComboBox();
            this.lblRoom = new System.Windows.Forms.Label();
            this.pnlPreview = new System.Windows.Forms.Panel();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.lblPreviewTitle = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblUserNote = new System.Windows.Forms.Label();
            this.btnQuickSetup = new System.Windows.Forms.Button();
            this.btnApplyWallpaper = new System.Windows.Forms.Button();
            this.pnlControls.SuspendLayout();
            this.grpWallpaperSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numComputerNumber)).BeginInit();
            this.pnlPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlControls
            // 
            this.pnlControls.Controls.Add(this.grpWallpaperSettings);
            this.pnlControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlControls.Location = new System.Drawing.Point(0, 0);
            this.pnlControls.Name = "pnlControls";
            this.pnlControls.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.pnlControls.Size = new System.Drawing.Size(920, 115);
            this.pnlControls.TabIndex = 0;
            // 
            // grpWallpaperSettings
            // 
            this.grpWallpaperSettings.Controls.Add(this.btnDefaultBg);
            this.grpWallpaperSettings.Controls.Add(this.btnBrowseBg);
            this.grpWallpaperSettings.Controls.Add(this.txtBgPath);
            this.grpWallpaperSettings.Controls.Add(this.lblBgImage);
            this.grpWallpaperSettings.Controls.Add(this.numComputerNumber);
            this.grpWallpaperSettings.Controls.Add(this.lblNumber);
            this.grpWallpaperSettings.Controls.Add(this.btnDeleteRoom);
            this.grpWallpaperSettings.Controls.Add(this.btnAddRoom);
            this.grpWallpaperSettings.Controls.Add(this.cmbRooms);
            this.grpWallpaperSettings.Controls.Add(this.lblRoom);
            this.grpWallpaperSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpWallpaperSettings.Font = new System.Drawing.Font("Phetsarath OT", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpWallpaperSettings.Location = new System.Drawing.Point(12, 8);
            this.grpWallpaperSettings.Name = "grpWallpaperSettings";
            this.grpWallpaperSettings.Size = new System.Drawing.Size(896, 99);
            this.grpWallpaperSettings.TabIndex = 0;
            this.grpWallpaperSettings.TabStop = false;
            this.grpWallpaperSettings.Text = "ກຳນົດຄ່າຮູບພື້ນຫຼັງ";
            // 
            // btnDefaultBg
            // 
            this.btnDefaultBg.Location = new System.Drawing.Point(420, 60);
            this.btnDefaultBg.Name = "btnDefaultBg";
            this.btnDefaultBg.Size = new System.Drawing.Size(105, 26);
            this.btnDefaultBg.TabIndex = 9;
            this.btnDefaultBg.Text = "ໃຊ້ຄ່າເລີ່ມຕົ້ນ";
            this.btnDefaultBg.UseVisualStyleBackColor = true;
            this.btnDefaultBg.Click += new System.EventHandler(this.btnDefaultBg_Click);
            // 
            // btnBrowseBg
            // 
            this.btnBrowseBg.Location = new System.Drawing.Point(325, 60);
            this.btnBrowseBg.Name = "btnBrowseBg";
            this.btnBrowseBg.Size = new System.Drawing.Size(90, 26);
            this.btnBrowseBg.TabIndex = 8;
            this.btnBrowseBg.Text = "ເລືອກຮູບ...";
            this.btnBrowseBg.UseVisualStyleBackColor = true;
            this.btnBrowseBg.Click += new System.EventHandler(this.btnBrowseBg_Click);
            // 
            // txtBgPath
            // 
            this.txtBgPath.Location = new System.Drawing.Point(95, 62);
            this.txtBgPath.Name = "txtBgPath";
            this.txtBgPath.ReadOnly = true;
            this.txtBgPath.Size = new System.Drawing.Size(220, 23);
            this.txtBgPath.TabIndex = 7;
            this.txtBgPath.Text = "bg.png";
            // 
            // lblBgImage
            // 
            this.lblBgImage.AutoSize = true;
            this.lblBgImage.Location = new System.Drawing.Point(16, 65);
            this.lblBgImage.Name = "lblBgImage";
            this.lblBgImage.Size = new System.Drawing.Size(65, 15);
            this.lblBgImage.TabIndex = 6;
            this.lblBgImage.Text = "ຮູບພື້ນຫຼັງ:";
            // 
            // numComputerNumber
            // 
            this.numComputerNumber.Font = new System.Drawing.Font("Phetsarath OT", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numComputerNumber.Location = new System.Drawing.Point(450, 25);
            this.numComputerNumber.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            this.numComputerNumber.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numComputerNumber.Name = "numComputerNumber";
            this.numComputerNumber.Size = new System.Drawing.Size(75, 24);
            this.numComputerNumber.TabIndex = 5;
            this.numComputerNumber.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.numComputerNumber.ValueChanged += new System.EventHandler(this.numComputerNumber_ValueChanged);
            // 
            // lblNumber
            // 
            this.lblNumber.AutoSize = true;
            this.lblNumber.Location = new System.Drawing.Point(385, 29);
            this.lblNumber.Name = "lblNumber";
            this.lblNumber.Size = new System.Drawing.Size(54, 15);
            this.lblNumber.TabIndex = 4;
            this.lblNumber.Text = "ເລກເຄື່ອງ:";
            // 
            // btnDeleteRoom
            // 
            this.btnDeleteRoom.Location = new System.Drawing.Point(295, 25);
            this.btnDeleteRoom.Name = "btnDeleteRoom";
            this.btnDeleteRoom.Size = new System.Drawing.Size(70, 25);
            this.btnDeleteRoom.TabIndex = 3;
            this.btnDeleteRoom.Text = "ລຶບຫ້ອງ";
            this.btnDeleteRoom.UseVisualStyleBackColor = true;
            this.btnDeleteRoom.Click += new System.EventHandler(this.btnDeleteRoom_Click);
            // 
            // btnAddRoom
            // 
            this.btnAddRoom.Location = new System.Drawing.Point(215, 25);
            this.btnAddRoom.Name = "btnAddRoom";
            this.btnAddRoom.Size = new System.Drawing.Size(75, 25);
            this.btnAddRoom.TabIndex = 2;
            this.btnAddRoom.Text = "+ ເພີ່ມຫ້ອງ";
            this.btnAddRoom.UseVisualStyleBackColor = true;
            this.btnAddRoom.Click += new System.EventHandler(this.btnAddRoom_Click);
            // 
            // cmbRooms
            // 
            this.cmbRooms.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRooms.FormattingEnabled = true;
            this.cmbRooms.Location = new System.Drawing.Point(95, 26);
            this.cmbRooms.Name = "cmbRooms";
            this.cmbRooms.Size = new System.Drawing.Size(110, 23);
            this.cmbRooms.TabIndex = 1;
            this.cmbRooms.SelectedIndexChanged += new System.EventHandler(this.cmbRooms_SelectedIndexChanged);
            // 
            // lblRoom
            // 
            this.lblRoom.AutoSize = true;
            this.lblRoom.Location = new System.Drawing.Point(16, 29);
            this.lblRoom.Name = "lblRoom";
            this.lblRoom.Size = new System.Drawing.Size(49, 15);
            this.lblRoom.TabIndex = 0;
            this.lblRoom.Text = "ຊື່ຫ້ອງ:";
            // 
            // pnlPreview
            // 
            this.pnlPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(245)))));
            this.pnlPreview.Controls.Add(this.picPreview);
            this.pnlPreview.Controls.Add(this.lblPreviewTitle);
            this.pnlPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPreview.Location = new System.Drawing.Point(0, 115);
            this.pnlPreview.Name = "pnlPreview";
            this.pnlPreview.Padding = new System.Windows.Forms.Padding(12);
            this.pnlPreview.Size = new System.Drawing.Size(920, 375);
            this.pnlPreview.TabIndex = 1;
            // 
            // picPreview
            // 
            this.picPreview.BackColor = System.Drawing.Color.Black;
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picPreview.Location = new System.Drawing.Point(12, 32);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(896, 331);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.TabIndex = 0;
            this.picPreview.TabStop = false;
            // 
            // lblPreviewTitle
            // 
            this.lblPreviewTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPreviewTitle.Font = new System.Drawing.Font("Phetsarath OT", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPreviewTitle.Location = new System.Drawing.Point(12, 12);
            this.lblPreviewTitle.Name = "lblPreviewTitle";
            this.lblPreviewTitle.Size = new System.Drawing.Size(896, 20);
            this.lblPreviewTitle.TabIndex = 1;
            this.lblPreviewTitle.Text = "ຕົວຢ່າງຮູບພື້ນຫຼັງ (Preview):";
            // 
            // pnlBottom
            // 
            this.pnlBottom.BackColor = System.Drawing.Color.White;
            this.pnlBottom.Controls.Add(this.lblStatus);
            this.pnlBottom.Controls.Add(this.lblUserNote);
            this.pnlBottom.Controls.Add(this.btnQuickSetup);
            this.pnlBottom.Controls.Add(this.btnApplyWallpaper);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 490);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.pnlBottom.Size = new System.Drawing.Size(920, 60);
            this.pnlBottom.TabIndex = 2;
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
            this.btnQuickSetup.Location = new System.Drawing.Point(475, 10);
            this.btnQuickSetup.Name = "btnQuickSetup";
            this.btnQuickSetup.Size = new System.Drawing.Size(200, 40);
            this.btnQuickSetup.TabIndex = 3;
            this.btnQuickSetup.Text = "⚡ ຕັ້ງຄ່າດ່ວນ";
            this.btnQuickSetup.UseVisualStyleBackColor = false;
            this.btnQuickSetup.Click += new System.EventHandler(this.btnQuickSetup_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Phetsarath OT", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
            this.lblStatus.Location = new System.Drawing.Point(16, 32);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(65, 15);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "ພ້ອມໃຊ້ງານ";
            // 
            // lblUserNote
            // 
            this.lblUserNote.AutoSize = true;
            this.lblUserNote.Font = new System.Drawing.Font("Phetsarath OT", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUserNote.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblUserNote.Location = new System.Drawing.Point(15, 11);
            this.lblUserNote.Name = "lblUserNote";
            this.lblUserNote.Size = new System.Drawing.Size(125, 17);
            this.lblUserNote.TabIndex = 1;
            this.lblUserNote.Text = "ຕັ້ງໃຫ້ບັນຊີ: {username}";
            // 
            // btnApplyWallpaper
            // 
            this.btnApplyWallpaper.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApplyWallpaper.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(115)))), ((int)(((byte)(232)))));
            this.btnApplyWallpaper.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnApplyWallpaper.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApplyWallpaper.Font = new System.Drawing.Font("Phetsarath OT", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnApplyWallpaper.ForeColor = System.Drawing.Color.White;
            this.btnApplyWallpaper.Location = new System.Drawing.Point(685, 10);
            this.btnApplyWallpaper.Name = "btnApplyWallpaper";
            this.btnApplyWallpaper.Size = new System.Drawing.Size(220, 40);
            this.btnApplyWallpaper.TabIndex = 0;
            this.btnApplyWallpaper.Text = "ກຳນົດພື້ນຫຼັງ (Set Wallpaper)";
            this.btnApplyWallpaper.UseVisualStyleBackColor = false;
            this.btnApplyWallpaper.Click += new System.EventHandler(this.btnApplyWallpaper_Click);
            // 
            // WallpaperPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlPreview);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlControls);
            this.Font = new System.Drawing.Font("Phetsarath OT", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "WallpaperPanel";
            this.Size = new System.Drawing.Size(920, 550);
            this.Load += new System.EventHandler(this.WallpaperPanel_Load);
            this.pnlControls.ResumeLayout(false);
            this.grpWallpaperSettings.ResumeLayout(false);
            this.grpWallpaperSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numComputerNumber)).EndInit();
            this.pnlPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlControls;
        private System.Windows.Forms.GroupBox grpWallpaperSettings;
        private System.Windows.Forms.Label lblRoom;
        private System.Windows.Forms.ComboBox cmbRooms;
        private System.Windows.Forms.Button btnAddRoom;
        private System.Windows.Forms.Button btnDeleteRoom;
        private System.Windows.Forms.Label lblNumber;
        private System.Windows.Forms.NumericUpDown numComputerNumber;
        private System.Windows.Forms.Label lblBgImage;
        private System.Windows.Forms.TextBox txtBgPath;
        private System.Windows.Forms.Button btnBrowseBg;
        private System.Windows.Forms.Button btnDefaultBg;
        private System.Windows.Forms.Panel pnlPreview;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Label lblPreviewTitle;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button btnQuickSetup;
        private System.Windows.Forms.Button btnApplyWallpaper;
        private System.Windows.Forms.Label lblUserNote;
        private System.Windows.Forms.Label lblStatus;
    }
}
