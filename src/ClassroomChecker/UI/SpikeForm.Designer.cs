namespace ClassroomChecker.UI
{
    partial class SpikeForm
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
            this.pnlTop = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.grpSettings = new System.Windows.Forms.GroupBox();
            this.txtCustomText = new System.Windows.Forms.TextBox();
            this.lblCustomText = new System.Windows.Forms.Label();
            this.cmbFontFamily = new System.Windows.Forms.ComboBox();
            this.lblFont = new System.Windows.Forms.Label();
            this.rdoTextRenderer = new System.Windows.Forms.RadioButton();
            this.rdoGdiPlus = new System.Windows.Forms.RadioButton();
            this.lblEngine = new System.Windows.Forms.Label();
            this.numComputerNumber = new System.Windows.Forms.NumericUpDown();
            this.lblNumber = new System.Windows.Forms.Label();
            this.txtRoomName = new System.Windows.Forms.TextBox();
            this.lblRoom = new System.Windows.Forms.Label();
            this.pnlPreview = new System.Windows.Forms.Panel();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.lblScreenRes = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnSaveImage = new System.Windows.Forms.Button();
            this.btnApplyWallpaper = new System.Windows.Forms.Button();
            this.pnlTop.SuspendLayout();
            this.grpSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numComputerNumber)).BeginInit();
            this.pnlPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(115)))), ((int)(((byte)(232)))));
            this.pnlTop.Controls.Add(this.lblSubtitle);
            this.pnlTop.Controls.Add(this.lblTitle);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this.pnlTop.Size = new System.Drawing.Size(960, 70);
            this.pnlTop.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Leelawadee UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(16, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(430, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Classroom Checker — Phase 0 Spike (Wallpaper)";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Leelawadee UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(240)))), ((int)(((byte)(255)))));
            this.lblSubtitle.Location = new System.Drawing.Point(17, 38);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(470, 17);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "ທົດສອບແຕ້ມຂໍ້ຄວາມພາສາລາວ (ສະຣະ/ວັນນະຍຸດ) ແລະ ຕັ້ງ Desktop Wallpaper ຈິງ";
            // 
            // grpSettings
            // 
            this.grpSettings.Controls.Add(this.txtCustomText);
            this.grpSettings.Controls.Add(this.lblCustomText);
            this.grpSettings.Controls.Add(this.cmbFontFamily);
            this.grpSettings.Controls.Add(this.lblFont);
            this.grpSettings.Controls.Add(this.rdoTextRenderer);
            this.grpSettings.Controls.Add(this.rdoGdiPlus);
            this.grpSettings.Controls.Add(this.lblEngine);
            this.grpSettings.Controls.Add(this.numComputerNumber);
            this.grpSettings.Controls.Add(this.lblNumber);
            this.grpSettings.Controls.Add(this.txtRoomName);
            this.grpSettings.Controls.Add(this.lblRoom);
            this.grpSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpSettings.Font = new System.Drawing.Font("Leelawadee UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpSettings.Location = new System.Drawing.Point(0, 70);
            this.grpSettings.Name = "grpSettings";
            this.grpSettings.Padding = new System.Windows.Forms.Padding(12);
            this.grpSettings.Size = new System.Drawing.Size(960, 120);
            this.grpSettings.TabIndex = 1;
            this.grpSettings.TabStop = false;
            this.grpSettings.Text = "ການຕັ້ງຄ່າທົດສອບ (Spike Controls)";
            // 
            // txtCustomText
            // 
            this.txtCustomText.Font = new System.Drawing.Font("Leelawadee UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCustomText.Location = new System.Drawing.Point(580, 80);
            this.txtCustomText.Name = "txtCustomText";
            this.txtCustomText.Size = new System.Drawing.Size(360, 23);
            this.txtCustomText.TabIndex = 10;
            this.txtCustomText.Text = "ທົດສອບ: ຕົ້ນໄມ້ ໝໍ້ ຜູ້ໃຫຍ່ ເສື້ອຜ້າ ໐໑໒";
            this.txtCustomText.TextChanged += new System.EventHandler(this.OnSettingChanged);
            // 
            // lblCustomText
            // 
            this.lblCustomText.AutoSize = true;
            this.lblCustomText.Location = new System.Drawing.Point(475, 83);
            this.lblCustomText.Name = "lblCustomText";
            this.lblCustomText.Size = new System.Drawing.Size(100, 15);
            this.lblCustomText.TabIndex = 9;
            this.lblCustomText.Text = "ຂໍ້ຄວາມທົດສອບສະຣະ:";
            // 
            // cmbFontFamily
            // 
            this.cmbFontFamily.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFontFamily.FormattingEnabled = true;
            this.cmbFontFamily.Location = new System.Drawing.Point(580, 35);
            this.cmbFontFamily.Name = "cmbFontFamily";
            this.cmbFontFamily.Size = new System.Drawing.Size(200, 23);
            this.cmbFontFamily.TabIndex = 8;
            this.cmbFontFamily.SelectedIndexChanged += new System.EventHandler(this.OnSettingChanged);
            // 
            // lblFont
            // 
            this.lblFont.AutoSize = true;
            this.lblFont.Location = new System.Drawing.Point(525, 38);
            this.lblFont.Name = "lblFont";
            this.lblFont.Size = new System.Drawing.Size(50, 15);
            this.lblFont.TabIndex = 7;
            this.lblFont.Text = "Font UI:";
            // 
            // rdoTextRenderer
            // 
            this.rdoTextRenderer.AutoSize = true;
            this.rdoTextRenderer.Checked = true;
            this.rdoTextRenderer.Location = new System.Drawing.Point(215, 81);
            this.rdoTextRenderer.Name = "rdoTextRenderer";
            this.rdoTextRenderer.Size = new System.Drawing.Size(217, 19);
            this.rdoTextRenderer.TabIndex = 6;
            this.rdoTextRenderer.TabStop = true;
            this.rdoTextRenderer.Text = "GDI / Uniscribe (TextRenderer.DrawText)";
            this.rdoTextRenderer.UseVisualStyleBackColor = true;
            this.rdoTextRenderer.CheckedChanged += new System.EventHandler(this.OnSettingChanged);
            // 
            // rdoGdiPlus
            // 
            this.rdoGdiPlus.AutoSize = true;
            this.rdoGdiPlus.Location = new System.Drawing.Point(75, 81);
            this.rdoGdiPlus.Name = "rdoGdiPlus";
            this.rdoGdiPlus.Size = new System.Drawing.Size(126, 19);
            this.rdoGdiPlus.TabIndex = 5;
            this.rdoGdiPlus.Text = "GDI+ (DrawString)";
            this.rdoGdiPlus.UseVisualStyleBackColor = true;
            this.rdoGdiPlus.CheckedChanged += new System.EventHandler(this.OnSettingChanged);
            // 
            // lblEngine
            // 
            this.lblEngine.AutoSize = true;
            this.lblEngine.Location = new System.Drawing.Point(20, 83);
            this.lblEngine.Name = "lblEngine";
            this.lblEngine.Size = new System.Drawing.Size(46, 15);
            this.lblEngine.TabIndex = 4;
            this.lblEngine.Text = "Engine:";
            // 
            // numComputerNumber
            // 
            this.numComputerNumber.Location = new System.Drawing.Point(325, 35);
            this.numComputerNumber.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            this.numComputerNumber.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numComputerNumber.Name = "numComputerNumber";
            this.numComputerNumber.Size = new System.Drawing.Size(75, 23);
            this.numComputerNumber.TabIndex = 3;
            this.numComputerNumber.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.numComputerNumber.ValueChanged += new System.EventHandler(this.OnSettingChanged);
            // 
            // lblNumber
            // 
            this.lblNumber.AutoSize = true;
            this.lblNumber.Location = new System.Drawing.Point(265, 38);
            this.lblNumber.Name = "lblNumber";
            this.lblNumber.Size = new System.Drawing.Size(54, 15);
            this.lblNumber.TabIndex = 2;
            this.lblNumber.Text = "ເລກເຄື່ອງ:";
            // 
            // txtRoomName
            // 
            this.txtRoomName.Location = new System.Drawing.Point(75, 35);
            this.txtRoomName.Name = "txtRoomName";
            this.txtRoomName.Size = new System.Drawing.Size(160, 23);
            this.txtRoomName.TabIndex = 1;
            this.txtRoomName.Text = "ຫ້ອງ A";
            this.txtRoomName.TextChanged += new System.EventHandler(this.OnSettingChanged);
            // 
            // lblRoom
            // 
            this.lblRoom.AutoSize = true;
            this.lblRoom.Location = new System.Drawing.Point(20, 38);
            this.lblRoom.Name = "lblRoom";
            this.lblRoom.Size = new System.Drawing.Size(49, 15);
            this.lblRoom.TabIndex = 0;
            this.lblRoom.Text = "ຊື່ຫ້ອງ:";
            // 
            // pnlPreview
            // 
            this.pnlPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(245)))));
            this.pnlPreview.Controls.Add(this.picPreview);
            this.pnlPreview.Controls.Add(this.lblScreenRes);
            this.pnlPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPreview.Location = new System.Drawing.Point(0, 190);
            this.pnlPreview.Name = "pnlPreview";
            this.pnlPreview.Padding = new System.Windows.Forms.Padding(16);
            this.pnlPreview.Size = new System.Drawing.Size(960, 360);
            this.pnlPreview.TabIndex = 2;
            // 
            // picPreview
            // 
            this.picPreview.BackColor = System.Drawing.Color.Black;
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picPreview.Location = new System.Drawing.Point(16, 40);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(928, 304);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.TabIndex = 0;
            this.picPreview.TabStop = false;
            // 
            // lblScreenRes
            // 
            this.lblScreenRes.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblScreenRes.Font = new System.Drawing.Font("Leelawadee UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblScreenRes.Location = new System.Drawing.Point(16, 16);
            this.lblScreenRes.Name = "lblScreenRes";
            this.lblScreenRes.Size = new System.Drawing.Size(928, 24);
            this.lblScreenRes.TabIndex = 1;
            this.lblScreenRes.Text = "Resolution ຈໍຫຼັກ: 1920 x 1080 | Preview ຂະໜາດຈິງ";
            // 
            // pnlBottom
            // 
            this.pnlBottom.BackColor = System.Drawing.Color.White;
            this.pnlBottom.Controls.Add(this.lblStatus);
            this.pnlBottom.Controls.Add(this.btnSaveImage);
            this.pnlBottom.Controls.Add(this.btnApplyWallpaper);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 550);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Padding = new System.Windows.Forms.Padding(16, 10, 16, 10);
            this.pnlBottom.Size = new System.Drawing.Size(960, 60);
            this.pnlBottom.TabIndex = 3;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Leelawadee UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
            this.lblStatus.Location = new System.Drawing.Point(16, 21);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(126, 17);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "ພ້ອມສຳລັບການທົດສອບ";
            // 
            // btnSaveImage
            // 
            this.btnSaveImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveImage.Font = new System.Drawing.Font("Leelawadee UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveImage.Location = new System.Drawing.Point(620, 12);
            this.btnSaveImage.Name = "btnSaveImage";
            this.btnSaveImage.Size = new System.Drawing.Size(140, 36);
            this.btnSaveImage.TabIndex = 1;
            this.btnSaveImage.Text = "ບັນທຶກຮູບ (.png)";
            this.btnSaveImage.UseVisualStyleBackColor = true;
            this.btnSaveImage.Click += new System.EventHandler(this.btnSaveImage_Click);
            // 
            // btnApplyWallpaper
            // 
            this.btnApplyWallpaper.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApplyWallpaper.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(115)))), ((int)(((byte)(232)))));
            this.btnApplyWallpaper.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnApplyWallpaper.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApplyWallpaper.Font = new System.Drawing.Font("Leelawadee UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnApplyWallpaper.ForeColor = System.Drawing.Color.White;
            this.btnApplyWallpaper.Location = new System.Drawing.Point(770, 12);
            this.btnApplyWallpaper.Name = "btnApplyWallpaper";
            this.btnApplyWallpaper.Size = new System.Drawing.Size(175, 36);
            this.btnApplyWallpaper.TabIndex = 0;
            this.btnApplyWallpaper.Text = "ຕັ້ງ Wallpaper ໃນເຄື່ອງຈິງ";
            this.btnApplyWallpaper.UseVisualStyleBackColor = false;
            this.btnApplyWallpaper.Click += new System.EventHandler(this.btnApplyWallpaper_Click);
            // 
            // SpikeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 610);
            this.Controls.Add(this.pnlPreview);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.grpSettings);
            this.Controls.Add(this.pnlTop);
            this.Font = new System.Drawing.Font("Leelawadee UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "SpikeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Classroom Checker — Phase 0 Spike";
            this.Load += new System.EventHandler(this.SpikeForm_Load);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.grpSettings.ResumeLayout(false);
            this.grpSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numComputerNumber)).EndInit();
            this.pnlPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.GroupBox grpSettings;
        private System.Windows.Forms.NumericUpDown numComputerNumber;
        private System.Windows.Forms.Label lblNumber;
        private System.Windows.Forms.TextBox txtRoomName;
        private System.Windows.Forms.Label lblRoom;
        private System.Windows.Forms.RadioButton rdoTextRenderer;
        private System.Windows.Forms.RadioButton rdoGdiPlus;
        private System.Windows.Forms.Label lblEngine;
        private System.Windows.Forms.ComboBox cmbFontFamily;
        private System.Windows.Forms.Label lblFont;
        private System.Windows.Forms.TextBox txtCustomText;
        private System.Windows.Forms.Label lblCustomText;
        private System.Windows.Forms.Panel pnlPreview;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Label lblScreenRes;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button btnApplyWallpaper;
        private System.Windows.Forms.Button btnSaveImage;
        private System.Windows.Forms.Label lblStatus;
    }
}
