using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassroomChecker.Core;
using ClassroomChecker.I18n;

namespace ClassroomChecker.UI
{
    /// <summary>
    /// ແທັບ "Antivirus USB" ໃນໜ້າຫຼັກ MainForm
    /// ເຄື່ອງມືສະແກນ, ຂ້າໄວຣັສ Shortcut, ກູ້ໄຟລ໌ທີ່ຖືກເຊື່ອງ ແລະ ສ້າງວັກຊີນປ້ອງກັນ Flash Drive
    /// </summary>
    public class UsbAntivirusPanel : UserControl
    {
        // 1. Top Header Banner
        private Panel pnlHeader;
        private Label lblHeaderIcon;
        private Label lblHeaderTitle;
        private Label lblHeaderSubtitle;
        private FlowLayoutPanel flpHeaderActions;
        private Label lblAutoRunBadge;
        private Button btnImmunizeHost;

        // 2. Main 2-Column Split
        private TableLayoutPanel tblContent;

        // Left Column (Controls & Options)
        private Panel pnlLeft;
        private GroupBox grpDriveSelect;
        private ComboBox cmbDrives;
        private Button btnRefreshDrives;
        private Button btnBrowseDrive;

        private GroupBox grpOptions;
        private CheckBox chkUnhideFiles;
        private CheckBox chkRemoveShortcuts;
        private CheckBox chkRemoveRogueScripts;
        private CheckBox chkDetectFakeExes;
        private CheckBox chkKillMalwareProc;
        private CheckBox chkCreateVaccine;
        private CheckBox chkImmunizeHost;

        private Panel pnlAction;
        private Button btnStartClean;
        private Button btnOpenDrive;
        private ProgressBar prgStatus;
        private Label lblProgressText;

        // Right Column (Execution Log & Summary)
        private GroupBox grpLog;
        private TextBox txtLog;
        private Panel pnlLogHeader;
        private Label lblLogStats;
        private Button btnClearLog;

        private bool _isBusy = false;
        private List<UsbDriveItem> _detectedDrives = new List<UsbDriveItem>();

        public UsbAntivirusPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Dock = DockStyle.Fill;
            this.Font = UiFontManager.GetFont(9F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Padding = new Padding(12);

            // -----------------------------------------------------------------
            // 1. Top Header Banner
            // -----------------------------------------------------------------
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 84,
                BackColor = Color.FromArgb(15, 23, 42), // Slate 900
                Padding = new Padding(14, 10, 14, 10)
            };

            lblHeaderIcon = new Label
            {
                Text = "🛡️",
                Font = new Font("Segoe UI Emoji", 24F, FontStyle.Regular),
                ForeColor = Color.White,
                Dock = DockStyle.Left,
                Width = 50,
                TextAlign = ContentAlignment.MiddleCenter
            };

            flpHeaderActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 6, 0, 0)
            };

            btnImmunizeHost = new Button
            {
                Text = "💉 ປິດ AutoRun ໃນເຄື່ອງ",
                BackColor = Color.FromArgb(14, 165, 233), // Sky 500
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = UiFontManager.GetFont(8.5F, FontStyle.Bold),
                Size = new Size(185, 36),
                Margin = new Padding(4, 0, 4, 0),
                Cursor = Cursors.Hand
            };
            btnImmunizeHost.FlatAppearance.BorderSize = 0;
            btnImmunizeHost.Click += btnImmunizeHost_Click;

            lblAutoRunBadge = new Label
            {
                Text = "Windows AutoRun: ປອດໄພ ✓",
                Font = UiFontManager.GetFont(8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(167, 243, 208), // Emerald 200
                BackColor = Color.FromArgb(6, 78, 59),    // Emerald 900
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(200, 36),
                Margin = new Padding(4, 0, 4, 0)
            };

            flpHeaderActions.Controls.Add(btnImmunizeHost);
            flpHeaderActions.Controls.Add(lblAutoRunBadge);

            Panel pnlHeaderText = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(10, 2, 8, 2)
            };

            lblHeaderTitle = new Label
            {
                Text = Localizer.T("usb_anti_header_title", "Antivirus USB & ກູ້ໄຟລ໌ Flash Drive (USB Shortcut Virus Cleaner & Vaccine)"),
                Font = UiFontManager.GetFont(11F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 26,
                AutoEllipsis = true
            };

            lblHeaderSubtitle = new Label
            {
                Text = Localizer.T("usb_anti_header_desc", "ແກ້ໄຂບັນຫາ Flash Drive ຕິດໄວຣັສ Shortcut, ໂຟນເດີຫາຍ, ໄຟລ໌ກາຍເປັນ .exe, ກູ້ໄຟລ໌ຄືນ, ສ້າງວັກຊີນປ້ອງກັນ ແລະ ປິດ AutoRun"),
                Font = UiFontManager.GetFont(8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            };

            pnlHeaderText.Controls.Add(lblHeaderSubtitle);
            pnlHeaderText.Controls.Add(lblHeaderTitle);

            pnlHeader.Controls.Add(pnlHeaderText);
            pnlHeader.Controls.Add(lblHeaderIcon);
            pnlHeader.Controls.Add(flpHeaderActions);

            // -----------------------------------------------------------------
            // 2. Main 2-Column TableLayout
            // -----------------------------------------------------------------
            tblContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 10, 0, 0)
            };
            tblContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56F));
            tblContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // =================================================================
            // LEFT COLUMN: Controls, Options & Start Action
            // =================================================================
            pnlLeft = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0, 0, 8, 0)
            };

            // 2.1 Drive Selection Group
            grpDriveSelect = new GroupBox
            {
                Text = Localizer.T("usb_cleaner_grp_drive", "1. ເລືອກ Flash Drive USB ທີ່ຕ້ອງການກູ້ໄຟລ໌ (Select Drive)"),
                Font = UiFontManager.GetFont(9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Dock = DockStyle.Top,
                Height = 84,
                // ຕັ້ງຄວາມກວ້າງເລີ່ມຕົ້ນໃຫ້ກົງກັບ Location ຂອງ control ລູກ ກ່ອນເພີ່ມ control
                // (ຖ້າບໍ່ຕັ້ງ GroupBox ຈະກວ້າງ 200px ແລ້ວ Anchor Right ຄິດໄລຍະຜິດ ເຮັດໃຫ້ control ລົ້ນ)
                Width = 456,
                Padding = new Padding(10, 8, 10, 8)
            };

            cmbDrives = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiFontManager.GetFont(9.5F, FontStyle.Regular),
                Location = new Point(10, 24),
                Size = new Size(206, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnRefreshDrives = new Button
            {
                Text = Localizer.T("usb_cleaner_btn_refresh_drives", "🔄 ກວດຫາ"),
                Font = UiFontManager.GetFont(8.5F, FontStyle.Regular),
                Location = new Point(222, 23),
                Size = new Size(110, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnRefreshDrives.Click += (s, e) => RefreshDrivesList();

            btnBrowseDrive = new Button
            {
                Text = Localizer.T("usb_cleaner_btn_browse_drive", "📁 ເລືອກ..."),
                Font = UiFontManager.GetFont(8.5F, FontStyle.Regular),
                Location = new Point(336, 23),
                Size = new Size(110, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnBrowseDrive.Click += btnBrowseDrive_Click;

            Label lblDriveHint = new Label
            {
                Text = Localizer.T("usb_drive_hint", "💡 ຄຳແນະນຳ: ລະບົບຈະກວດຫາ Removable Drive (Flash Drive) ອັດຕະໂນມັດ"),
                Font = UiFontManager.GetFont(8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(10, 58),
                Size = new Size(436, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true
            };

            grpDriveSelect.Controls.Add(cmbDrives);
            grpDriveSelect.Controls.Add(btnRefreshDrives);
            grpDriveSelect.Controls.Add(btnBrowseDrive);
            grpDriveSelect.Controls.Add(lblDriveHint);

            // 2.2 Options Group
            grpOptions = new GroupBox
            {
                Text = Localizer.T("usb_cleaner_grp_options", "2. ຕົວເລືອກການກູ້ໄຟລ໌ ແລະ ຂ້າໄວຣັສ (Options)"),
                Font = UiFontManager.GetFont(9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Dock = DockStyle.Top,
                Height = 224,
                Width = 452,
                Padding = new Padding(12, 6, 12, 6)
            };

            chkUnhideFiles = new CheckBox
            {
                Text = Localizer.T("usb_opt_unhide", "✓ ປົດລັອກ ແລະ ກູ້ຄືນທຸກໄຟລ໌/ໂຟນເດີທີ່ຖືກເຊື່ອງ (Unhide Files)"),
                Font = UiFontManager.GetFont(8.8F, FontStyle.Regular),
                Checked = true,
                Location = new Point(12, 22),
                Size = new Size(428, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkRemoveShortcuts = new CheckBox
            {
                Text = Localizer.T("usb_opt_shortcuts", "✓ ລຶບໄຟລ໌ Shortcut (.lnk) ປອມທີ່ແລ່ນ Script/CMD ໄວຣັສ"),
                Font = UiFontManager.GetFont(8.8F, FontStyle.Regular),
                Checked = true,
                Location = new Point(12, 49),
                Size = new Size(428, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkRemoveRogueScripts = new CheckBox
            {
                Text = Localizer.T("usb_opt_scripts", "✓ ລຶບໄຟລ໌ Script ໄວຣັສ (.vbs, .js, .wsf, autorun.inf)"),
                Font = UiFontManager.GetFont(8.8F, FontStyle.Regular),
                Checked = true,
                Location = new Point(12, 76),
                Size = new Size(428, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkDetectFakeExes = new CheckBox
            {
                Text = Localizer.T("usb_opt_fake_exes", "✓ ກວດຈັບ ແລະ ລຶບໄຟລ໌ .exe ທີ່ປອມເປັນໂຟນເດີ (<folder>.exe)"),
                Font = UiFontManager.GetFont(8.8F, FontStyle.Regular),
                Checked = true,
                Location = new Point(12, 103),
                Size = new Size(428, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkKillMalwareProc = new CheckBox
            {
                Text = Localizer.T("usb_opt_kill_proc", "✓ ປິດ Process ໄວຣັສ Script ທີ່ກຳລັງແລ່ນ (wscript.exe, cscript.exe)"),
                Font = UiFontManager.GetFont(8.8F, FontStyle.Regular),
                Checked = true,
                Location = new Point(12, 130),
                Size = new Size(428, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkCreateVaccine = new CheckBox
            {
                Text = Localizer.T("usb_opt_vaccine", "✓ ສ້າງວັກຊີນປ້ອງກັນ USB (Protected autorun.inf folder)"),
                Font = UiFontManager.GetFont(8.8F, FontStyle.Regular),
                Checked = true,
                Location = new Point(12, 157),
                Size = new Size(428, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkImmunizeHost = new CheckBox
            {
                Text = Localizer.T("usb_opt_immunize_pc", "✓ ປິດ Windows AutoRun/AutoPlay ໃນເຄື່ອງນີ້ (NoDriveTypeAutoRun)"),
                Font = UiFontManager.GetFont(8.8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(2, 132, 199), // Sky 600
                Checked = true,
                Location = new Point(12, 184),
                Size = new Size(428, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            grpOptions.Controls.AddRange(new Control[] {
                chkUnhideFiles, chkRemoveShortcuts, chkRemoveRogueScripts,
                chkDetectFakeExes, chkKillMalwareProc, chkCreateVaccine, chkImmunizeHost
            });

            // 2.3 Action Panel (Start Button & Open USB)
            pnlAction = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                Padding = new Padding(0, 10, 0, 0)
            };

            btnStartClean = new Button
            {
                Text = Localizer.T("usb_cleaner_btn_start", "🚀 ເລີ່ມຕົ້ນກູ້ໄຟລ໌ & ຂ້າໄວຣັສ (Start Scan & Recovery)"),
                Font = UiFontManager.GetFont(10F, FontStyle.Bold),
                BackColor = Color.FromArgb(16, 185, 129), // Emerald 500
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Top,
                Height = 44,
                Cursor = Cursors.Hand
            };
            btnStartClean.FlatAppearance.BorderSize = 0;
            btnStartClean.Click += btnStartClean_Click;

            prgStatus = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 16,
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };

            lblProgressText = new Label
            {
                Text = Localizer.T("usb_cleaner_lbl_ready", "ພ້ອມເຮັດວຽກ. ເລືອກ Flash Drive ແລ້ວກົດ 'ເລີ່ມຕົ້ນກູ້ໄຟລ໌'"),
                Font = UiFontManager.GetFont(8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105),
                Dock = DockStyle.Top,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            btnOpenDrive = new Button
            {
                Text = Localizer.T("usb_cleaner_btn_open_drive", "📂 ເປີດເບິ່ງໄຟລ໌ໃນ USB (Open Drive)"),
                Font = UiFontManager.GetFont(8.8F, FontStyle.Regular),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(15, 23, 42),
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Bottom,
                Height = 30,
                Cursor = Cursors.Hand
            };
            btnOpenDrive.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnOpenDrive.Click += btnOpenDrive_Click;

            pnlAction.Controls.Add(btnOpenDrive);
            pnlAction.Controls.Add(lblProgressText);
            pnlAction.Controls.Add(prgStatus);
            pnlAction.Controls.Add(btnStartClean);

            pnlLeft.Controls.Add(pnlAction);
            pnlLeft.Controls.Add(grpOptions);
            pnlLeft.Controls.Add(grpDriveSelect);

            // =================================================================
            // RIGHT COLUMN: Terminal Log & Statistics Summary
            // =================================================================
            grpLog = new GroupBox
            {
                Text = Localizer.T("usb_cleaner_grp_log", "3. ລາຍລະອຽດການດຳເນີນງານ (Execution Log & Summary)"),
                Font = UiFontManager.GetFont(9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 8, 10, 10)
            };

            pnlLogHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 28,
                Padding = new Padding(0, 0, 0, 4)
            };

            lblLogStats = new Label
            {
                Text = "ສະຖານະ: ພ້ອມສະແກນ",
                Font = UiFontManager.GetFont(8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            btnClearLog = new Button
            {
                Text = "🧹 ລ້າງ Log",
                Font = UiFontManager.GetFont(8F, FontStyle.Regular),
                Dock = DockStyle.Right,
                Width = 80,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnClearLog.Click += (s, e) => txtLog.Clear();

            pnlLogHeader.Controls.Add(lblLogStats);
            pnlLogHeader.Controls.Add(btnClearLog);

            txtLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9F, FontStyle.Regular),
                BackColor = Color.FromArgb(15, 23, 42),      // Dark navy
                ForeColor = Color.FromArgb(241, 245, 249)     // Bright slate
            };

            grpLog.Controls.Add(txtLog);
            grpLog.Controls.Add(pnlLogHeader);

            tblContent.Controls.Add(pnlLeft, 0, 0);
            tblContent.Controls.Add(grpLog, 1, 0);

            // Add all to UserControl
            this.Controls.Add(tblContent);
            this.Controls.Add(pnlHeader);

            this.Load += UsbAntivirusPanel_Load;
            this.ResumeLayout(false);
        }

        private void UsbAntivirusPanel_Load(object sender, EventArgs e)
        {
            Localizer.LanguageChanged += OnLanguageChanged;
            ApplyLocalization();
            RefreshDrivesList();
            UpdateHostAutoRunStatus();

            AppendLog("🛡️ ລະບົບ Antivirus USB & ກູ້ໄຟລ໌ Flash Drive ພ້ອມເຮັດວຽກ.");
            AppendLog("💡 ວິທີໃຊ້ງານ: ສຽບ Flash Drive USB -> ເລືອກໄດຣຟ໌ດ້ານຊ້າຍ -> ກົດ 'ເລີ່ມຕົ້ນກູ້ໄຟລ໌'.");
        }

        public void OnTabActivated()
        {
            RefreshDrivesList();
            UpdateHostAutoRunStatus();
        }

        private void OnLanguageChanged()
        {
            ApplyLocalization();
            UpdateHostAutoRunStatus();
        }

        public void ApplyLocalization()
        {
            lblHeaderTitle.Text = Localizer.T("usb_anti_header_title", "Antivirus USB & ກູ້ໄຟລ໌ Flash Drive");
            lblHeaderSubtitle.Text = Localizer.T("usb_anti_header_desc", "ແກ້ໄຂບັນຫາ Flash Drive ຕິດໄວຣັສ Shortcut, ໂຟນເດີຫາຍ, ໄຟລ໌ກາຍເປັນ .exe, ກູ້ໄຟລ໌ຄືນ, ສ້າງວັກຊີນປ້ອງກັນ ແລະ ປິດ AutoRun");
            btnImmunizeHost.Text = Localizer.T("usb_rescue_btn_immunize", "💉 ປິດ AutoRun ໃນເຄື່ອງ");

            grpDriveSelect.Text = Localizer.T("usb_cleaner_grp_drive", "1. ເລືອກ Flash Drive USB ທີ່ຕ້ອງການກູ້ໄຟລ໌ (Select Drive)");
            btnRefreshDrives.Text = Localizer.T("usb_cleaner_btn_refresh_drives", "🔄 ກວດຫາ");
            btnBrowseDrive.Text = Localizer.T("usb_cleaner_btn_browse_drive", "📁 ເລືອກ...");

            grpOptions.Text = Localizer.T("usb_cleaner_grp_options", "2. ຕົວເລືອກການກູ້ໄຟລ໌ ແລະ ຂ້າໄວຣັສ (Options)");
            chkUnhideFiles.Text = Localizer.T("usb_opt_unhide", "✓ ປົດລັອກ ແລະ ກູ້ຄືນທຸກໄຟລ໌/ໂຟນເດີທີ່ຖືກເຊື່ອງ (Unhide Files)");
            chkRemoveShortcuts.Text = Localizer.T("usb_opt_shortcuts", "✓ ລຶບໄຟລ໌ Shortcut (.lnk) ປອມທີ່ແລ່ນ Script/CMD ໄວຣັສ");
            chkRemoveRogueScripts.Text = Localizer.T("usb_opt_scripts", "✓ ລຶບໄຟລ໌ Script ໄວຣັສ (.vbs, .js, .wsf, autorun.inf)");
            chkDetectFakeExes.Text = Localizer.T("usb_opt_fake_exes", "✓ ກວດຈັບ ແລະ ລຶບໄຟລ໌ .exe ທີ່ປອມເປັນໂຟນເດີ (<folder>.exe)");
            chkKillMalwareProc.Text = Localizer.T("usb_opt_kill_proc", "✓ ປິດ Process ໄວຣັສ Script ທີ່ກຳລັງແລ່ນ (wscript.exe, cscript.exe)");
            chkCreateVaccine.Text = Localizer.T("usb_opt_vaccine", "✓ ສ້າງວັກຊີນປ້ອງກັນ USB (Protected autorun.inf folder)");
            chkImmunizeHost.Text = Localizer.T("usb_opt_immunize_pc", "✓ ປິດ Windows AutoRun/AutoPlay ໃນເຄື່ອງນີ້ (NoDriveTypeAutoRun)");

            btnStartClean.Text = Localizer.T("usb_cleaner_btn_start", "🚀 ເລີ່ມຕົ້ນກູ້ໄຟລ໌ & ຂ້າໄວຣັສ (Start Scan & Recovery)");
            btnOpenDrive.Text = Localizer.T("usb_cleaner_btn_open_drive", "📂 ເປີດເບິ່ງໄຟລ໌ໃນ USB (Open Drive)");
            grpLog.Text = Localizer.T("usb_cleaner_grp_log", "3. ລາຍລະອຽດການດຳເນີນງານ (Execution Log & Summary)");
        }

        private void UpdateHostAutoRunStatus()
        {
            bool disabled = UsbVirusCleanerService.IsHostAutoRunDisabled();
            if (disabled)
            {
                lblAutoRunBadge.Text = Localizer.T("usb_badge_autorun_safe", "AutoRun: ປິດແລ້ວ (ປອດໄພ ✓)");
                lblAutoRunBadge.ForeColor = Color.FromArgb(167, 243, 208); // Emerald 200
                lblAutoRunBadge.BackColor = Color.FromArgb(6, 78, 59);    // Emerald 900
                btnImmunizeHost.Visible = false;
            }
            else
            {
                lblAutoRunBadge.Text = Localizer.T("usb_badge_autorun_risk", "AutoRun: ເປີດຢູ່ (ສ່ຽງ ⚠️)");
                lblAutoRunBadge.ForeColor = Color.FromArgb(254, 202, 202); // Red 200
                lblAutoRunBadge.BackColor = Color.FromArgb(127, 29, 29);   // Red 900
                btnImmunizeHost.Visible = true;
            }
        }

        private void RefreshDrivesList()
        {
            cmbDrives.Items.Clear();
            _detectedDrives = UsbVirusCleanerService.GetRemovableDrives();

            if (_detectedDrives.Count > 0)
            {
                foreach (var d in _detectedDrives)
                {
                    cmbDrives.Items.Add(d.DisplayText);
                }
                cmbDrives.SelectedIndex = 0;
                lblLogStats.Text = string.Format("ພົບ Flash Drive: {0} ໄດຣຟ໌", _detectedDrives.Count);
            }
            else
            {
                cmbDrives.Items.Add(Localizer.T("usb_no_drive_found", "--- ບໍ່ພົບ Flash Drive (ກະລຸນາສຽບ USB ຫຼື ກົດ 'ເລືອກ...') ---"));
                cmbDrives.SelectedIndex = 0;
                lblLogStats.Text = "ບໍ່ພົບ Flash Drive";
            }
        }

        private void btnBrowseDrive_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = Localizer.T("usb_cleaner_browse_desc", "ເລືອກ Flash Drive ຫຼື ໂຟນເດີທີ່ຕ້ອງການກູ້ໄຟລ໌ ແລະ ຂ້າໄວຣັສ");
                fbd.ShowNewFolderButton = false;
                if (fbd.ShowDialog(this.FindForm()) == DialogResult.OK)
                {
                    string path = fbd.SelectedPath;
                    cmbDrives.Items.Insert(0, path + " (ໂຟນເດີທີ່ເລືອກ)");
                    cmbDrives.SelectedIndex = 0;
                    lblLogStats.Text = "ເລືອກໂຟນເດີ: " + path;
                }
            }
        }

        private string GetSelectedTargetPath()
        {
            if (cmbDrives.SelectedIndex < 0) return null;
            string selectedText = cmbDrives.SelectedItem.ToString();

            if (selectedText.StartsWith("---")) return null;

            if (_detectedDrives != null && cmbDrives.SelectedIndex < _detectedDrives.Count)
            {
                return _detectedDrives[cmbDrives.SelectedIndex].DriveLetter;
            }

            int parenIdx = selectedText.IndexOf(" (");
            if (parenIdx > 0)
            {
                return selectedText.Substring(0, parenIdx).Trim();
            }

            return selectedText.Trim();
        }

        private async void btnStartClean_Click(object sender, EventArgs e)
        {
            if (_isBusy) return;

            string targetPath = GetSelectedTargetPath();
            if (string.IsNullOrEmpty(targetPath) || !Directory.Exists(targetPath))
            {
                MessageBox.Show(
                    Localizer.T("usb_cleaner_err_no_drive", "ກະລຸນາສຽບ Flash Drive USB ຫຼື ເລືອກໂຟນເດີທີ່ຕ້ອງການກູ້ໄຟລ໌ກ່ອນ!"),
                    Localizer.T("app_title"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var options = new UsbCleanOptions
            {
                TargetPath = targetPath,
                UnhideLegitimateFiles = chkUnhideFiles.Checked,
                RemoveShortcuts = chkRemoveShortcuts.Checked,
                RemoveRogueScripts = chkRemoveRogueScripts.Checked,
                DetectFakeFolderExes = chkDetectFakeExes.Checked,
                KillMalwareProcesses = chkKillMalwareProc.Checked,
                CreateUsbVaccine = chkCreateVaccine.Checked,
                ImmunizeHostAutoRun = chkImmunizeHost.Checked
            };

            SetUiBusy(true);
            txtLog.Clear();
            AppendLog(string.Format("===> ເລີ່ມຕົ້ນຂະບວນການກູ້ໄຟລ໌ & ຂ້າໄວຣັສ ສຳລັບ {0} ({1}) <===", targetPath, DateTime.Now.ToString("HH:mm:ss")));

            UsbCleanResult result = null;

            await Task.Run(() =>
            {
                result = UsbVirusCleanerService.CleanAndRestoreDrive(options, msg =>
                {
                    if (!this.IsDisposed)
                    {
                        this.BeginInvoke(new Action(() => AppendLog(msg)));
                    }
                });
            });

            SetUiBusy(false);
            UpdateHostAutoRunStatus();

            if (result != null && result.Success)
            {
                lblProgressText.Text = "✓ ສຳເລັດຮຽບຮ້ອຍແລ້ວ!";
                lblProgressText.ForeColor = Color.FromArgb(16, 185, 129);
                lblLogStats.Text = string.Format("ກູ້ໄຟລ໌ໄດ້ {0} ໄຟລ໌, ລຶບ Shortcut {1} ໄຟລ໌", result.FilesUnhiddenCount, result.ShortcutsRemovedCount);

                string summary = string.Format(
                    "🎉 ກູ້ໄຟລ໌ ແລະ ຂ້າໄວຣັສສຳເລັດແລ້ວ!\r\n\r\n" +
                    "• ປົດລັອກໄຟລ໌ທີ່ຖືກເຊື່ອງຄືນມາ: {0} ໄຟລ໌ ({1} ໂຟນເດີ)\r\n" +
                    "• ລຶບໄຟລ໌ Shortcut ປອມ: {2} ໄຟລ໌\r\n" +
                    "• ລຶບໄຟລ໌ Script ໄວຣັສ: {3} ໄຟລ໌\r\n" +
                    "• ລຶບໄຟລ໌ .exe ປອມ: {4} ໄຟລ໌\r\n" +
                    "• ປິດ Process ໄວຣັສໃນເຄື່ອງ: {5} processes\r\n" +
                    "• ສ້າງວັກຊີນປ້ອງກັນ USB: {6}\r\n" +
                    "• ປິດ Windows AutoRun ໃນເຄື່ອງ: {7}\r\n\r\n" +
                    "ທ່ານຕ້ອງການເປີດເບິ່ງໄຟລ໌ໃນ USB ຕອນນີ້ເລີຍບໍ່?",
                    result.FilesUnhiddenCount, result.DirectoriesUnhiddenCount,
                    result.ShortcutsRemovedCount, result.RogueScriptsRemovedCount, result.FakeExesRemovedCount,
                    result.KilledProcessesCount,
                    result.VaccineCreated ? "ສຳເລັດ (✓)" : "ຂ້າມ",
                    result.HostAutoRunDisabled ? "ສຳເລັດ (✓)" : "ຂ້າມ");

                if (MessageBox.Show(summary, "ຜົນການກູ້ໄຟລ໌ USB", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    OpenTargetFolder(targetPath);
                }
            }
            else
            {
                lblProgressText.Text = "⚠️ ເກີດຂໍ້ຜິດພາດໃນການກູ້ໄຟລ໌";
                lblProgressText.ForeColor = Color.FromArgb(239, 68, 68);
                MessageBox.Show(result != null ? result.Message : "Unknown error", "ຂໍ້ຜິດພາດ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImmunizeHost_Click(object sender, EventArgs e)
        {
            bool ok = UsbVirusCleanerService.ImmunizeHostMachine();
            UpdateHostAutoRunStatus();
            if (ok)
            {
                MessageBox.Show(
                    Localizer.T("usb_immunize_success"),
                    Localizer.T("app_title"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    "ບໍ່ສາມາດປິດ AutoRun ໄດ້ (ກະລຸນາກວດສອບສິດ Administrator)",
                    "ຂໍ້ຜິດພາດ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void btnOpenDrive_Click(object sender, EventArgs e)
        {
            string path = GetSelectedTargetPath();
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                OpenTargetFolder(path);
            }
            else
            {
                MessageBox.Show("ບໍ່ພົບໄດຣຟ໌ USB ທີ່ເລືອກ", "ແຈ້ງເຕືອນ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenTargetFolder(string path)
        {
            try
            {
                Process.Start("explorer.exe", "\"" + path + "\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show("ບໍ່ສາມາດເປີດ Explorer: " + ex.Message);
            }
        }

        private void SetUiBusy(bool busy)
        {
            _isBusy = busy;
            btnStartClean.Enabled = !busy;
            btnRefreshDrives.Enabled = !busy;
            btnBrowseDrive.Enabled = !busy;
            cmbDrives.Enabled = !busy;
            chkUnhideFiles.Enabled = !busy;
            chkRemoveShortcuts.Enabled = !busy;
            chkRemoveRogueScripts.Enabled = !busy;
            chkDetectFakeExes.Enabled = !busy;
            chkKillMalwareProc.Enabled = !busy;
            chkCreateVaccine.Enabled = !busy;
            chkImmunizeHost.Enabled = !busy;

            prgStatus.Visible = busy;
            if (busy)
            {
                prgStatus.Style = ProgressBarStyle.Marquee;
                prgStatus.MarqueeAnimationSpeed = 30;
                lblProgressText.Text = "⏳ ກຳລັງກວດສອບ ແລະ ກູ້ໄຟລ໌... ກະລຸນາລໍຖ້າ";
                lblProgressText.ForeColor = Color.FromArgb(2, 132, 199);
            }
        }

        private void AppendLog(string message)
        {
            if (this.IsDisposed) return;
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText(string.Format("[{0}] {1}\r\n", timestamp, message));
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }
    }
}
