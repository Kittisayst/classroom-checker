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
    /// ໜ້າຕ່າງກູ້ໄຟລ໌ ແລະ ຂ້າໄວຣັສ USB Shortcut (USB Virus Cleaner & Vaccine Dialog)
    /// </summary>
    public class UsbCleanerDialog : Form
    {
        private Panel pnlHeader;
        private Label lblHeaderIcon;
        private Label lblHeaderTitle;
        private Label lblHeaderSubtitle;

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
        private ProgressBar prgStatus;
        private Label lblProgressText;

        private GroupBox grpLog;
        private TextBox txtLog;

        private Panel pnlFooter;
        private Button btnOpenDrive;
        private Button btnClose;

        private bool _isBusy = false;
        private List<UsbDriveItem> _detectedDrives = new List<UsbDriveItem>();

        public UsbCleanerDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = Localizer.T("usb_cleaner_dialog_title", "Classroom Checker — ກູ້ໄຟລ໌ & ຂ້າໄວຣັສ USB Shortcut (USB Rescue & Vaccine)");
            this.Size = new Size(820, 680);
            this.MinimumSize = new Size(720, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = UiFontManager.GetFont(9F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(248, 250, 252);

            try
            {
                using (Stream stream = typeof(UsbCleanerDialog).Assembly.GetManifestResourceStream("ClassroomChecker.app.ico"))
                {
                    if (stream != null) this.Icon = new Icon(stream);
                }
            }
            catch { }

            // -----------------------------------------------------------------
            // 1. Header Banner
            // -----------------------------------------------------------------
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 74,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(16, 12, 16, 12)
            };

            lblHeaderIcon = new Label
            {
                Text = "🛡️",
                Font = new Font("Segoe UI Emoji", 24F, FontStyle.Regular),
                ForeColor = Color.White,
                Dock = DockStyle.Left,
                Width = 52,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Panel pnlHeaderText = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(8, 0, 0, 0)
            };

            lblHeaderTitle = new Label
            {
                Text = Localizer.T("usb_cleaner_header_title", "ກູ້ໄຟລ໌ & ຂ້າໄວຣັສ USB Shortcut (USB Virus Cleaner & Vaccine)"),
                Font = UiFontManager.GetFont(11F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 24
            };

            lblHeaderSubtitle = new Label
            {
                Text = Localizer.T("usb_cleaner_header_desc", "ແກ້ໄຂບັນຫາ Flash Drive ຕິດໄວຣັສ Shortcut, ໂຟນເດີຫາຍ, ໄຟລ໌ກາຍເປັນ .exe, ກູ້ໄຟລ໌ຄືນ ແລະ ສ້າງວັກຊີນປ້ອງກັນ"),
                Font = UiFontManager.GetFont(8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Dock = DockStyle.Fill
            };

            pnlHeaderText.Controls.Add(lblHeaderSubtitle);
            pnlHeaderText.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(pnlHeaderText);
            pnlHeader.Controls.Add(lblHeaderIcon);

            // -----------------------------------------------------------------
            // 2. Drive Selection Group
            // -----------------------------------------------------------------
            grpDriveSelect = new GroupBox
            {
                Text = Localizer.T("usb_cleaner_grp_drive", "1. ເລືອກ Flash Drive / ໂຟນເດີ USB ທີ່ຕ້ອງການກູ້ໄຟລ໌ (Select Drive)"),
                Font = UiFontManager.GetFont(9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(14, 82),
                Size = new Size(776, 72),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(12, 8, 12, 8)
            };

            cmbDrives = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiFontManager.GetFont(9.5F, FontStyle.Regular),
                Location = new Point(14, 28),
                Size = new Size(470, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnRefreshDrives = new Button
            {
                Text = Localizer.T("usb_cleaner_btn_refresh_drives", "🔄 ກວດຫາ USB"),
                Font = UiFontManager.GetFont(8.5F, FontStyle.Regular),
                Location = new Point(492, 26),
                Size = new Size(130, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnRefreshDrives.Click += (s, e) => RefreshDrivesList();

            btnBrowseDrive = new Button
            {
                Text = Localizer.T("usb_cleaner_btn_browse_drive", "📁 ເລືອກໂຟນເດີ..."),
                Font = UiFontManager.GetFont(8.5F, FontStyle.Regular),
                Location = new Point(628, 26),
                Size = new Size(134, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnBrowseDrive.Click += btnBrowseDrive_Click;

            grpDriveSelect.Controls.Add(cmbDrives);
            grpDriveSelect.Controls.Add(btnRefreshDrives);
            grpDriveSelect.Controls.Add(btnBrowseDrive);

            // -----------------------------------------------------------------
            // 3. Options Group
            // -----------------------------------------------------------------
            grpOptions = new GroupBox
            {
                Text = Localizer.T("usb_cleaner_grp_options", "2. ຕົວເລືອກການກູ້ໄຟລ໌ ແລະ ຂ້າໄວຣັສ (Cleaning & Recovery Options)"),
                Font = UiFontManager.GetFont(9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(14, 160),
                Size = new Size(776, 142),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(12, 8, 12, 8)
            };

            chkUnhideFiles = new CheckBox
            {
                Text = Localizer.T("usb_opt_unhide", "✓ ປົດລັອກ ແລະ ກູ້ຄືນໄຟລ໌/ໂຟນເດີທັງໝົດທີ່ຖືກເຊື່ອງ (Unhide all hidden files & folders)"),
                Font = UiFontManager.GetFont(8.8F, FontStyle.Regular),
                Checked = true,
                Location = new Point(16, 22),
                Size = new Size(740, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkRemoveShortcuts = new CheckBox
            {
                Text = Localizer.T("usb_opt_shortcuts", "✓ ລຶບໄຟລ໌ Shortcut (.lnk) ປອມທີ່ແລ່ນ Script/CMD ໄວຣັສ (Remove Fake Shortcuts)"),
                Font = UiFontManager.GetFont(8.8F, FontStyle.Regular),
                Checked = true,
                Location = new Point(16, 45),
                Size = new Size(740, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkRemoveRogueScripts = new CheckBox
            {
                Text = Localizer.T("usb_opt_scripts", "✓ ລຶບໄຟລ໌ Script ໄວຣັສ (.vbs, .js, .wsf) ແລະ autorun.inf ປອມ"),
                Font = UiFontManager.GetFont(8.8F, FontStyle.Regular),
                Checked = true,
                Location = new Point(16, 68),
                Size = new Size(360, 22)
            };

            chkDetectFakeExes = new CheckBox
            {
                Text = Localizer.T("usb_opt_fake_exes", "✓ ກວດຈັບ ແລະ ລຶບໄຟລ໌ .exe ທີ່ປອມເປັນໂຟນເດີ (<folder>.exe)"),
                Font = UiFontManager.GetFont(8.8F, FontStyle.Regular),
                Checked = true,
                Location = new Point(386, 68),
                Size = new Size(370, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkKillMalwareProc = new CheckBox
            {
                Text = Localizer.T("usb_opt_kill_proc", "✓ ປິດ Process ໄວຣັສ Script ທີ່ກຳລັງແລ່ນ (wscript.exe, cscript.exe)"),
                Font = UiFontManager.GetFont(8.8F, FontStyle.Regular),
                Checked = true,
                Location = new Point(16, 91),
                Size = new Size(360, 22)
            };

            chkCreateVaccine = new CheckBox
            {
                Text = Localizer.T("usb_opt_vaccine", "✓ ສ້າງວັກຊີນປ້ອງກັນ USB (autorun.inf dummy folder)"),
                Font = UiFontManager.GetFont(8.8F, FontStyle.Regular),
                Checked = true,
                Location = new Point(386, 91),
                Size = new Size(370, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkImmunizeHost = new CheckBox
            {
                Text = Localizer.T("usb_opt_immunize_pc", "✓ ປິດ Windows AutoRun/AutoPlay ໃນເຄື່ອງນີ້ (NoDriveTypeAutoRun = 0xFF ປ້ອງກັນການຕິດເຊື້ອ)"),
                Font = UiFontManager.GetFont(8.8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(2, 132, 199),
                Checked = true,
                Location = new Point(16, 114),
                Size = new Size(740, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            grpOptions.Controls.AddRange(new Control[] {
                chkUnhideFiles, chkRemoveShortcuts, chkRemoveRogueScripts,
                chkDetectFakeExes, chkKillMalwareProc, chkCreateVaccine, chkImmunizeHost
            });

            // -----------------------------------------------------------------
            // 4. Action Bar (Start Button + Progress)
            // -----------------------------------------------------------------
            pnlAction = new Panel
            {
                Location = new Point(14, 308),
                Size = new Size(776, 56),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };

            btnStartClean = new Button
            {
                Text = Localizer.T("usb_cleaner_btn_start", "🚀 ເລີ່ມຕົ້ນກູ້ໄຟລ໌ & ຂ້າໄວຣັສ (Start Cleaning & Recovery)"),
                Font = UiFontManager.GetFont(10.5F, FontStyle.Bold),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(0, 4),
                Size = new Size(380, 46),
                Cursor = Cursors.Hand
            };
            btnStartClean.FlatAppearance.BorderSize = 0;
            btnStartClean.Click += btnStartClean_Click;

            prgStatus = new ProgressBar
            {
                Location = new Point(390, 8),
                Size = new Size(386, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };

            lblProgressText = new Label
            {
                Text = Localizer.T("usb_cleaner_lbl_ready", "ພ້ອມເຮັດວຽກ. ເລືອກ Flash Drive ແລ້ວກົດ 'ເລີ່ມຕົ້ນກູ້ໄຟລ໌'"),
                Font = UiFontManager.GetFont(8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(390, 30),
                Size = new Size(386, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true
            };

            pnlAction.Controls.Add(btnStartClean);
            pnlAction.Controls.Add(prgStatus);
            pnlAction.Controls.Add(lblProgressText);

            // -----------------------------------------------------------------
            // 5. Activity Log Group
            // -----------------------------------------------------------------
            grpLog = new GroupBox
            {
                Text = Localizer.T("usb_cleaner_grp_log", "3. ລາຍລະອຽດການດຳເນີນງານ (Execution Log)"),
                Font = UiFontManager.GetFont(9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(14, 370),
                Size = new Size(776, 204),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(10, 8, 10, 8)
            };

            txtLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9F, FontStyle.Regular),
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(241, 245, 249)
            };

            grpLog.Controls.Add(txtLog);

            // -----------------------------------------------------------------
            // 6. Footer (Open Drive & Close)
            // -----------------------------------------------------------------
            pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = Color.Transparent,
                Padding = new Padding(14, 8, 14, 8)
            };

            btnOpenDrive = new Button
            {
                Text = Localizer.T("usb_cleaner_btn_open_drive", "📂 ເປີດເບິ່ງໄຟລ໌ໃນ USB (Open Drive)"),
                Font = UiFontManager.GetFont(9F, FontStyle.Regular),
                Location = new Point(14, 10),
                Size = new Size(250, 34),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnOpenDrive.Click += btnOpenDrive_Click;

            btnClose = new Button
            {
                Text = Localizer.T("btn_close", "ປິດ (Close)"),
                Font = UiFontManager.GetFont(9F, FontStyle.Regular),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(660, 10),
                Size = new Size(130, 34),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.Close();

            pnlFooter.Controls.Add(btnOpenDrive);
            pnlFooter.Controls.Add(btnClose);

            // Add controls to Form
            this.Controls.Add(grpLog);
            this.Controls.Add(pnlAction);
            this.Controls.Add(grpOptions);
            this.Controls.Add(grpDriveSelect);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);

            this.Load += UsbCleanerDialog_Load;
            this.ResumeLayout(false);
        }

        private void UsbCleanerDialog_Load(object sender, EventArgs e)
        {
            RefreshDrivesList();
            AppendLog("🛡️ ໂປຣແກຣມກູ້ໄຟລ໌ ແລະ ຂ້າໄວຣັສ USB Shortcut ພ້ອມເຮັດວຽກ.");
            AppendLog("💡 ຄຳແນະນຳ: ສຽບ Flash Drive ແລ້ວເລືອກໄດຣຟ໌ດ້ານເທິງ ຈາກນັ້ນກົດ 'ເລີ່ມຕົ້ນກູ້ໄຟລ໌'.");
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
            }
            else
            {
                cmbDrives.Items.Add(Localizer.T("usb_no_drive_found", "--- ບໍ່ພົບ Flash Drive (ກະລຸນາສຽບ USB ຫຼື ກົດ 'ເລືອກໂຟນເດີ...') ---"));
                cmbDrives.SelectedIndex = 0;
            }
        }

        private void btnBrowseDrive_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = Localizer.T("usb_cleaner_browse_desc", "ເລືອກ Flash Drive ຫຼື ໂຟນເດີທີ່ຕ້ອງການກູ້ໄຟລ໌ ແລະ ຂ້າໄວຣັສ");
                fbd.ShowNewFolderButton = false;
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    string path = fbd.SelectedPath;
                    cmbDrives.Items.Insert(0, path + " (ໂຟນເດີທີ່ເລືອກ)");
                    cmbDrives.SelectedIndex = 0;
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

            // If user browsed custom folder, take first part
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
            AppendLog(string.Format("===> ເລີ່ມຕົ້ນຂະບວນການກູ້ໄຟລ໌ສຳລັບ {0} ({1}) <===", targetPath, DateTime.Now.ToString("HH:mm:ss")));

            UsbCleanResult result = null;

            await Task.Run(() =>
            {
                result = UsbVirusCleanerService.CleanAndRestoreDrive(options, msg =>
                {
                    this.BeginInvoke(new Action(() => AppendLog(msg)));
                });
            });

            SetUiBusy(false);

            if (result != null && result.Success)
            {
                lblProgressText.Text = "✓ ສຳເລັດຮຽບຮ້ອຍແລ້ວ!";
                lblProgressText.ForeColor = Color.FromArgb(16, 185, 129);

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
