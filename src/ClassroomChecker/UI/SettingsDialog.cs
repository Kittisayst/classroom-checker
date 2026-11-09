using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ClassroomChecker.Config;
using ClassroomChecker.Core;
using ClassroomChecker.I18n;

namespace ClassroomChecker.UI
{
    public class SettingsDialog : Form
    {
        private TabControl tabControl;
        private TabPage tabGeneral;
        private TabPage tabWallpaper;
        private TabPage tabCleanup;
        private TabPage tabSecurity;

        // General
        private NumericUpDown numMinNumber;
        private NumericUpDown numMaxNumber;
        private ListBox lstRooms;
        private Button btnAddRoom;
        private Button btnEditRoom;
        private Button btnDeleteRoom;
        private TextBox txtSpecOutputDir;

        // Wallpaper & Label
        private TextBox txtLabelTemplate;
        private ComboBox cmbPosition;
        private NumericUpDown numFontSize;
        private TextBox txtColorHex;
        private Panel pnlColorSample;
        private Button btnPickColor;
        private CheckBox chkShadow;
        private NumericUpDown numMargin;
        private ComboBox cmbStyle;
        private CheckBox chkApplyAllUsers;

        // Cleanup
        private CheckBox chkUserTemp;
        private CheckBox chkWindowsTemp;
        private CheckBox chkRecycleBin;
        private CheckBox chkWindowsUpdate;
        private CheckBox chkBrowserCache;
        private Button btnOpenConfig;
        private Button btnResetDefaults;

        // Security / App Lock
        private CheckBox chkBlockUsb;
        private CheckBox chkBlockDownloads;
        private CheckBox chkBlockPopularGames;
        private ListBox lstBlockedExes;
        private Button btnAddBlockedExe;
        private Button btnDeleteBlockedExe;
        private Button btnApplyLockNow;
        private Button btnUnlockNow;

        // Bottom
        private Button btnSave;
        private Button btnCancel;

        public SettingsDialog()
        {
            InitializeComponent();
            UiFontManager.ApplyUiFont(this);
            LoadFromConfig(ConfigService.Instance.Current);
        }

        private void InitializeComponent()
        {
            this.Text = Localizer.T("settings_title");
            this.Size = new Size(640, 560);
            this.MinimumSize = new Size(600, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.BackColor = Color.FromArgb(248, 250, 252);

            try
            {
                using (var stream = typeof(SettingsDialog).Assembly.GetManifestResourceStream("ClassroomChecker.app.ico"))
                {
                    if (stream != null) this.Icon = new Icon(stream);
                }
            }
            catch { }

            // TabControl
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(14, 8)
            };

            // -----------------------------------------------------------------
            // Tab 1: General & Range
            // -----------------------------------------------------------------
            tabGeneral = new TabPage(Localizer.T("settings_tab_general"))
            {
                BackColor = Color.White,
                Padding = new Padding(16)
            };

            // Group: Number Range
            GroupBox grpRange = new GroupBox
            {
                Text = Localizer.T("settings_grp_number_range"),
                Location = new Point(16, 12),
                Size = new Size(570, 75),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblMin = new Label { Text = Localizer.T("settings_min_number"), Location = new Point(20, 30), AutoSize = true };
            numMinNumber = new NumericUpDown { Location = new Point(160, 28), Size = new Size(80, 25), Minimum = 1, Maximum = 999, Value = 1 };

            Label lblMax = new Label { Text = Localizer.T("settings_max_number"), Location = new Point(280, 30), AutoSize = true };
            numMaxNumber = new NumericUpDown { Location = new Point(420, 28), Size = new Size(80, 25), Minimum = 1, Maximum = 999, Value = 45 };

            grpRange.Controls.AddRange(new Control[] { lblMin, numMinNumber, lblMax, numMaxNumber });

            // Group: Rooms
            GroupBox grpRooms = new GroupBox
            {
                Text = Localizer.T("settings_grp_rooms"),
                Location = new Point(16, 95),
                Size = new Size(570, 200),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lstRooms = new ListBox
            {
                Location = new Point(20, 25),
                Size = new Size(390, 160),
                IntegralHeight = false
            };

            btnAddRoom = new Button
            {
                Text = Localizer.T("settings_btn_add_room"),
                Location = new Point(425, 25),
                Size = new Size(130, 32),
                Cursor = Cursors.Hand
            };
            btnAddRoom.Click += btnAddRoom_Click;

            btnEditRoom = new Button
            {
                Text = Localizer.T("settings_btn_edit_room"),
                Location = new Point(425, 65),
                Size = new Size(130, 32),
                Cursor = Cursors.Hand
            };
            btnEditRoom.Click += btnEditRoom_Click;

            btnDeleteRoom = new Button
            {
                Text = Localizer.T("settings_btn_delete_room"),
                Location = new Point(425, 105),
                Size = new Size(130, 32),
                Cursor = Cursors.Hand
            };
            btnDeleteRoom.Click += btnDeleteRoom_Click;

            grpRooms.Controls.AddRange(new Control[] { lstRooms, btnAddRoom, btnEditRoom, btnDeleteRoom });

            // Group: Output Directory
            GroupBox grpOutput = new GroupBox
            {
                Text = Localizer.T("settings_grp_output"),
                Location = new Point(16, 305),
                Size = new Size(570, 75),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblSpecDir = new Label { Text = Localizer.T("settings_spec_output_dir"), Location = new Point(20, 30), AutoSize = true };
            txtSpecOutputDir = new TextBox { Location = new Point(210, 27), Size = new Size(340, 25) };

            grpOutput.Controls.AddRange(new Control[] { lblSpecDir, txtSpecOutputDir });

            tabGeneral.Controls.AddRange(new Control[] { grpRange, grpRooms, grpOutput });

            // -----------------------------------------------------------------
            // Tab 2: Wallpaper & Label
            // -----------------------------------------------------------------
            tabWallpaper = new TabPage(Localizer.T("settings_tab_wallpaper"))
            {
                BackColor = Color.White,
                Padding = new Padding(16)
            };

            // Group: Label Settings
            GroupBox grpLabel = new GroupBox
            {
                Text = Localizer.T("settings_grp_label"),
                Location = new Point(16, 12),
                Size = new Size(570, 245),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblTpl = new Label { Text = Localizer.T("settings_label_template"), Location = new Point(20, 28), AutoSize = true };
            txtLabelTemplate = new TextBox { Location = new Point(190, 25), Size = new Size(360, 25) };
            Label lblHint = new Label
            {
                Text = "Placeholders: {room}, {number}, {hostname}, {ip}, {date}",
                Location = new Point(190, 52),
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8F, FontStyle.Regular)
            };

            Label lblPos = new Label { Text = Localizer.T("settings_label_position"), Location = new Point(20, 80), AutoSize = true };
            cmbPosition = new ComboBox
            {
                Location = new Point(190, 77),
                Size = new Size(160, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPosition.Items.AddRange(new object[] { "top-right", "top-left", "bottom-right", "bottom-left", "center" });

            Label lblSize = new Label { Text = Localizer.T("settings_label_font_size"), Location = new Point(20, 115), AutoSize = true };
            numFontSize = new NumericUpDown { Location = new Point(190, 112), Size = new Size(90, 25), Minimum = 20, Maximum = 200, Value = 72 };

            Label lblCol = new Label { Text = Localizer.T("settings_label_color"), Location = new Point(20, 150), AutoSize = true };
            txtColorHex = new TextBox { Location = new Point(190, 147), Size = new Size(90, 25), Text = "#FFFFFF" };
            pnlColorSample = new Panel
            {
                Location = new Point(290, 147),
                Size = new Size(40, 25),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            btnPickColor = new Button
            {
                Text = Localizer.T("settings_btn_pick_color"),
                Location = new Point(340, 146),
                Size = new Size(110, 27),
                Cursor = Cursors.Hand
            };
            btnPickColor.Click += btnPickColor_Click;
            txtColorHex.TextChanged += (s, e) => UpdateColorPreview();

            chkShadow = new CheckBox
            {
                Text = Localizer.T("settings_label_shadow"),
                Location = new Point(20, 185),
                AutoSize = true,
                Checked = true
            };

            Label lblMargin = new Label { Text = Localizer.T("settings_label_margin"), Location = new Point(20, 215), AutoSize = true };
            numMargin = new NumericUpDown { Location = new Point(190, 212), Size = new Size(90, 25), Minimum = 0, Maximum = 200, Value = 40 };

            grpLabel.Controls.AddRange(new Control[] {
                lblTpl, txtLabelTemplate, lblHint,
                lblPos, cmbPosition,
                lblSize, numFontSize,
                lblCol, txtColorHex, pnlColorSample, btnPickColor,
                chkShadow,
                lblMargin, numMargin
            });

            // Group: Wallpaper Style & Apply To All Users
            GroupBox grpWp = new GroupBox
            {
                Text = Localizer.T("settings_grp_wallpaper"),
                Location = new Point(16, 265),
                Size = new Size(570, 115),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblStyle = new Label { Text = Localizer.T("settings_wallpaper_style"), Location = new Point(20, 30), AutoSize = true };
            cmbStyle = new ComboBox
            {
                Location = new Point(190, 27),
                Size = new Size(160, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStyle.Items.AddRange(new object[] { "fill", "fit", "stretch", "center", "tile" });

            chkApplyAllUsers = new CheckBox
            {
                Text = Localizer.T("settings_apply_all_users"),
                Location = new Point(20, 70),
                AutoSize = true
            };

            grpWp.Controls.AddRange(new Control[] { lblStyle, cmbStyle, chkApplyAllUsers });

            tabWallpaper.Controls.AddRange(new Control[] { grpLabel, grpWp });

            // -----------------------------------------------------------------
            // Tab 3: Cleanup & System
            // -----------------------------------------------------------------
            tabCleanup = new TabPage(Localizer.T("settings_tab_cleanup"))
            {
                BackColor = Color.White,
                Padding = new Padding(16)
            };

            GroupBox grpCleanup = new GroupBox
            {
                Text = Localizer.T("settings_grp_cleanup_targets"),
                Location = new Point(16, 12),
                Size = new Size(570, 210),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkUserTemp = new CheckBox { Text = Localizer.T("settings_cleanup_user_temp"), Location = new Point(20, 30), AutoSize = true, Checked = true };
            chkWindowsTemp = new CheckBox { Text = Localizer.T("settings_cleanup_windows_temp"), Location = new Point(20, 65), AutoSize = true, Checked = true };
            chkRecycleBin = new CheckBox { Text = Localizer.T("settings_cleanup_recycle_bin"), Location = new Point(20, 100), AutoSize = true, Checked = true };
            chkWindowsUpdate = new CheckBox { Text = Localizer.T("settings_cleanup_update_cache"), Location = new Point(20, 135), AutoSize = true, Checked = true };
            chkBrowserCache = new CheckBox { Text = Localizer.T("settings_cleanup_browser_cache"), Location = new Point(20, 170), AutoSize = true, Checked = false };

            grpCleanup.Controls.AddRange(new Control[] { chkUserTemp, chkWindowsTemp, chkRecycleBin, chkWindowsUpdate, chkBrowserCache });

            GroupBox grpConfigMgmt = new GroupBox
            {
                Text = Localizer.T("settings_grp_config_mgmt"),
                Location = new Point(16, 235),
                Size = new Size(570, 95),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnOpenConfig = new Button
            {
                Text = Localizer.T("settings_btn_open_config"),
                Location = new Point(20, 35),
                Size = new Size(200, 34),
                Cursor = Cursors.Hand
            };
            btnOpenConfig.Click += btnOpenConfig_Click;

            btnResetDefaults = new Button
            {
                Text = Localizer.T("settings_btn_reset_defaults"),
                Location = new Point(240, 35),
                Size = new Size(200, 34),
                Cursor = Cursors.Hand
            };
            btnResetDefaults.Click += btnResetDefaults_Click;

            grpConfigMgmt.Controls.AddRange(new Control[] { btnOpenConfig, btnResetDefaults });

            tabCleanup.Controls.AddRange(new Control[] { grpCleanup, grpConfigMgmt });

            // -----------------------------------------------------------------
            // Tab 4: Security & App/Game Lock
            // -----------------------------------------------------------------
            tabSecurity = new TabPage(Localizer.T("settings_tab_security"))
            {
                BackColor = Color.White,
                Padding = new Padding(16)
            };

            // Group: Policies
            GroupBox grpSec = new GroupBox
            {
                Text = Localizer.T("settings_grp_security_policies"),
                Location = new Point(16, 12),
                Size = new Size(570, 125),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkBlockUsb = new CheckBox
            {
                Text = Localizer.T("settings_chk_block_usb"),
                Location = new Point(20, 26),
                AutoSize = true,
                Checked = true
            };

            chkBlockDownloads = new CheckBox
            {
                Text = Localizer.T("settings_chk_block_downloads"),
                Location = new Point(20, 56),
                AutoSize = true,
                Checked = true
            };

            chkBlockPopularGames = new CheckBox
            {
                Text = Localizer.T("settings_chk_block_popular_games"),
                Location = new Point(20, 86),
                AutoSize = true,
                Checked = true
            };

            grpSec.Controls.AddRange(new Control[] { chkBlockUsb, chkBlockDownloads, chkBlockPopularGames });

            // Group: Blocked Exes
            GroupBox grpExes = new GroupBox
            {
                Text = Localizer.T("settings_grp_blocked_exes"),
                Location = new Point(16, 145),
                Size = new Size(570, 160),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lstBlockedExes = new ListBox
            {
                Location = new Point(20, 25),
                Size = new Size(390, 120),
                IntegralHeight = false
            };

            btnAddBlockedExe = new Button
            {
                Text = Localizer.T("settings_btn_add_exe"),
                Location = new Point(425, 25),
                Size = new Size(130, 32),
                Cursor = Cursors.Hand
            };
            btnAddBlockedExe.Click += btnAddBlockedExe_Click;

            btnDeleteBlockedExe = new Button
            {
                Text = Localizer.T("settings_btn_delete_exe"),
                Location = new Point(425, 65),
                Size = new Size(130, 32),
                Cursor = Cursors.Hand
            };
            btnDeleteBlockedExe.Click += btnDeleteBlockedExe_Click;

            grpExes.Controls.AddRange(new Control[] { lstBlockedExes, btnAddBlockedExe, btnDeleteBlockedExe });

            // Group: Immediate Actions
            GroupBox grpActions = new GroupBox
            {
                Text = Localizer.T("settings_grp_security_actions"),
                Location = new Point(16, 312),
                Size = new Size(570, 75),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnApplyLockNow = new Button
            {
                Text = Localizer.T("settings_btn_lock_now"),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Phetsarath OT", 9F, FontStyle.Bold),
                Location = new Point(20, 26),
                Size = new Size(190, 34),
                Cursor = Cursors.Hand
            };
            btnApplyLockNow.FlatAppearance.BorderSize = 0;
            btnApplyLockNow.Click += btnApplyLockNow_Click;

            btnUnlockNow = new Button
            {
                Text = Localizer.T("settings_btn_unlock_now"),
                BackColor = Color.FromArgb(71, 85, 105),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Phetsarath OT", 9F, FontStyle.Regular),
                Location = new Point(225, 26),
                Size = new Size(170, 34),
                Cursor = Cursors.Hand
            };
            btnUnlockNow.FlatAppearance.BorderSize = 0;
            btnUnlockNow.Click += btnUnlockNow_Click;

            grpActions.Controls.AddRange(new Control[] { btnApplyLockNow, btnUnlockNow });

            tabSecurity.Controls.AddRange(new Control[] { grpSec, grpExes, grpActions });

            // Add Tabs
            tabControl.TabPages.AddRange(new TabPage[] { tabGeneral, tabWallpaper, tabCleanup, tabSecurity });

            // -----------------------------------------------------------------
            // Bottom Panel: Save & Cancel
            // -----------------------------------------------------------------
            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(16, 10, 16, 10)
            };

            btnSave = new Button
            {
                Text = Localizer.T("btn_save"),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Phetsarath OT", 9.5F, FontStyle.Bold),
                Location = new Point(380, 10),
                Size = new Size(115, 36),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;

            btnCancel = new Button
            {
                Text = Localizer.T("btn_cancel"),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Phetsarath OT", 9.5F, FontStyle.Regular),
                Location = new Point(505, 10),
                Size = new Size(100, 36),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

            pnlBottom.Controls.AddRange(new Control[] { btnSave, btnCancel });

            this.Controls.Add(tabControl);
            this.Controls.Add(pnlBottom);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadFromConfig(AppConfig cfg)
        {
            if (cfg == null) return;

            // Range
            int min = cfg.computerNumberRange != null ? cfg.computerNumberRange.min : 1;
            int max = cfg.computerNumberRange != null ? cfg.computerNumberRange.max : 45;
            numMinNumber.Value = Math.Max(1, Math.Min(999, min));
            numMaxNumber.Value = Math.Max(1, Math.Min(999, max));

            // Rooms
            lstRooms.Items.Clear();
            if (cfg.rooms != null)
            {
                foreach (var r in cfg.rooms)
                {
                    lstRooms.Items.Add(r);
                }
            }
            if (lstRooms.Items.Count > 0)
            {
                lstRooms.SelectedIndex = 0;
            }

            // Output dir
            txtSpecOutputDir.Text = !string.IsNullOrEmpty(cfg.specOutputDir) ? cfg.specOutputDir : "specs";

            // Label
            txtLabelTemplate.Text = cfg.label != null && !string.IsNullOrEmpty(cfg.label.template)
                ? cfg.label.template
                : "{room} - {number}";

            string pos = cfg.label != null && !string.IsNullOrEmpty(cfg.label.position)
                ? cfg.label.position.ToLowerInvariant()
                : "top-right";
            int posIdx = cmbPosition.Items.IndexOf(pos);
            cmbPosition.SelectedIndex = posIdx >= 0 ? posIdx : 0;

            numFontSize.Value = cfg.label != null && cfg.label.fontSizePx >= 20 && cfg.label.fontSizePx <= 200
                ? cfg.label.fontSizePx
                : 72;

            txtColorHex.Text = cfg.label != null && !string.IsNullOrEmpty(cfg.label.color)
                ? cfg.label.color
                : "#FFFFFF";
            UpdateColorPreview();

            chkShadow.Checked = cfg.label == null || cfg.label.shadow;

            numMargin.Value = cfg.label != null && cfg.label.marginPx >= 0 && cfg.label.marginPx <= 200
                ? cfg.label.marginPx
                : 40;

            // Wallpaper
            string style = cfg.background != null && !string.IsNullOrEmpty(cfg.background.style)
                ? cfg.background.style.ToLowerInvariant()
                : "fill";
            int styleIdx = cmbStyle.Items.IndexOf(style);
            cmbStyle.SelectedIndex = styleIdx >= 0 ? styleIdx : 0;

            chkApplyAllUsers.Checked = cfg.background != null && cfg.background.applyToAllUsers;

            // Cleanup
            if (cfg.cleanup != null)
            {
                chkUserTemp.Checked = cfg.cleanup.userTemp;
                chkWindowsTemp.Checked = cfg.cleanup.windowsTemp;
                chkRecycleBin.Checked = cfg.cleanup.recycleBin;
                chkWindowsUpdate.Checked = cfg.cleanup.windowsUpdateCache;
                chkBrowserCache.Checked = cfg.cleanup.browserCache;
            }

            // Security
            if (cfg.security != null)
            {
                chkBlockUsb.Checked = cfg.security.blockUsbExecution;
                chkBlockDownloads.Checked = cfg.security.blockDownloadsExecution;
                chkBlockPopularGames.Checked = cfg.security.blockPopularGames;

                lstBlockedExes.Items.Clear();
                if (cfg.security.blockedExes != null)
                {
                    foreach (var exe in cfg.security.blockedExes)
                    {
                        lstBlockedExes.Items.Add(exe);
                    }
                }
            }
        }

        private void UpdateColorPreview()
        {
            try
            {
                Color c = ColorTranslator.FromHtml(txtColorHex.Text.Trim());
                pnlColorSample.BackColor = c;
            }
            catch
            {
                // ignore invalid hex input
            }
        }

        private void btnPickColor_Click(object sender, EventArgs e)
        {
            using (var cd = new ColorDialog())
            {
                try
                {
                    cd.Color = ColorTranslator.FromHtml(txtColorHex.Text.Trim());
                }
                catch { }

                if (cd.ShowDialog(this) == DialogResult.OK)
                {
                    txtColorHex.Text = string.Format("#{0:X2}{1:X2}{2:X2}", cd.Color.R, cd.Color.G, cd.Color.B);
                    pnlColorSample.BackColor = cd.Color;
                }
            }
        }

        private void btnAddRoom_Click(object sender, EventArgs e)
        {
            string roomName = null;
            using (var dlg = new InputDialog(Localizer.T("prompt_add_room_title"), Localizer.T("prompt_add_room"), ""))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    roomName = dlg.InputText;
                }
            }

            if (string.IsNullOrEmpty(roomName)) return;

            roomName = roomName.Trim();
            if (string.IsNullOrEmpty(roomName))
            {
                MessageBox.Show(Localizer.T("room_name_empty"), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var item in lstRooms.Items)
            {
                if (string.Equals(item.ToString(), roomName, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(Localizer.T("room_exists"), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            lstRooms.Items.Add(roomName);
            lstRooms.SelectedItem = roomName;
        }

        private void btnEditRoom_Click(object sender, EventArgs e)
        {
            if (lstRooms.SelectedItem == null) return;
            string oldName = lstRooms.SelectedItem.ToString();

            string newName = null;
            using (var dlg = new InputDialog(Localizer.T("settings_prompt_rename_title"), Localizer.T("settings_prompt_rename_room"), oldName))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    newName = dlg.InputText;
                }
            }

            if (string.IsNullOrEmpty(newName)) return;

            newName = newName.Trim();
            if (string.IsNullOrEmpty(newName) || string.Equals(newName, oldName, StringComparison.OrdinalIgnoreCase)) return;

            foreach (var item in lstRooms.Items)
            {
                if (string.Equals(item.ToString(), newName, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(Localizer.T("room_exists"), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            int idx = lstRooms.SelectedIndex;
            lstRooms.Items[idx] = newName;
            lstRooms.SelectedIndex = idx;
        }

        private void btnDeleteRoom_Click(object sender, EventArgs e)
        {
            if (lstRooms.SelectedItem == null) return;
            if (lstRooms.Items.Count <= 1)
            {
                MessageBox.Show(Localizer.T("settings_validation_rooms_empty"), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string roomToDelete = lstRooms.SelectedItem.ToString();
            var res = MessageBox.Show(
                Localizer.T("confirm_delete_room", roomToDelete),
                Localizer.T("confirm_delete_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (res == DialogResult.Yes)
            {
                lstRooms.Items.Remove(roomToDelete);
                if (lstRooms.Items.Count > 0)
                {
                    lstRooms.SelectedIndex = 0;
                }
            }
        }

        private void btnOpenConfig_Click(object sender, EventArgs e)
        {
            try
            {
                string path = ConfigService.Instance.ActiveConfigPath;
                if (!File.Exists(path))
                {
                    ConfigService.Instance.Save();
                }
                Process.Start("notepad.exe", path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening config: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnResetDefaults_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(
                Localizer.T("settings_reset_confirm"),
                Localizer.T("settings_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (res == DialogResult.Yes)
            {
                LoadFromConfig(new AppConfig());
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validation
            if (numMinNumber.Value > numMaxNumber.Value)
            {
                MessageBox.Show(Localizer.T("settings_validation_range_error"), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (lstRooms.Items.Count == 0)
            {
                MessageBox.Show(Localizer.T("settings_validation_rooms_empty"), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cfg = ConfigService.Instance.Current;
            cfg.computerNumberRange.min = (int)numMinNumber.Value;
            cfg.computerNumberRange.max = (int)numMaxNumber.Value;

            cfg.rooms = new List<string>();
            foreach (var item in lstRooms.Items)
            {
                cfg.rooms.Add(item.ToString());
            }

            if (!string.IsNullOrEmpty(txtSpecOutputDir.Text.Trim()))
            {
                cfg.specOutputDir = txtSpecOutputDir.Text.Trim();
            }

            cfg.label.template = !string.IsNullOrEmpty(txtLabelTemplate.Text.Trim())
                ? txtLabelTemplate.Text.Trim()
                : "{room} - {number}";

            cfg.label.position = cmbPosition.SelectedItem != null
                ? cmbPosition.SelectedItem.ToString()
                : "top-right";

            cfg.label.fontSizePx = (int)numFontSize.Value;
            cfg.label.color = txtColorHex.Text.Trim();
            cfg.label.shadow = chkShadow.Checked;
            cfg.label.marginPx = (int)numMargin.Value;

            cfg.background.style = cmbStyle.SelectedItem != null
                ? cmbStyle.SelectedItem.ToString()
                : "fill";

            cfg.background.applyToAllUsers = chkApplyAllUsers.Checked;

            cfg.cleanup.userTemp = chkUserTemp.Checked;
            cfg.cleanup.windowsTemp = chkWindowsTemp.Checked;
            cfg.cleanup.recycleBin = chkRecycleBin.Checked;
            cfg.cleanup.windowsUpdateCache = chkWindowsUpdate.Checked;
            cfg.cleanup.browserCache = chkBrowserCache.Checked;

            if (cfg.security == null) cfg.security = new SecurityConfig();
            cfg.security.blockUsbExecution = chkBlockUsb.Checked;
            cfg.security.blockDownloadsExecution = chkBlockDownloads.Checked;
            cfg.security.blockPopularGames = chkBlockPopularGames.Checked;
            cfg.security.blockedExes = new List<string>();
            foreach (var item in lstBlockedExes.Items)
            {
                cfg.security.blockedExes.Add(item.ToString());
            }

            ConfigService.Instance.Save();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnAddBlockedExe_Click(object sender, EventArgs e)
        {
            string exe = null;
            using (var dlg = new InputDialog(Localizer.T("settings_prompt_add_exe_title"), Localizer.T("settings_prompt_add_exe"), "game.exe"))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    exe = dlg.InputText;
                }
            }

            if (string.IsNullOrEmpty(exe)) return;
            exe = exe.Trim().ToLowerInvariant();
            if (!exe.EndsWith(".exe"))
            {
                exe += ".exe";
            }

            foreach (var item in lstBlockedExes.Items)
            {
                if (string.Equals(item.ToString(), exe, StringComparison.OrdinalIgnoreCase)) return;
            }

            lstBlockedExes.Items.Add(exe);
        }

        private void btnDeleteBlockedExe_Click(object sender, EventArgs e)
        {
            if (lstBlockedExes.SelectedItem == null) return;
            lstBlockedExes.Items.Remove(lstBlockedExes.SelectedItem);
        }

        private void btnApplyLockNow_Click(object sender, EventArgs e)
        {
            var sec = new SecurityConfig
            {
                blockUsbExecution = chkBlockUsb.Checked,
                blockDownloadsExecution = chkBlockDownloads.Checked,
                blockPopularGames = chkBlockPopularGames.Checked,
                blockedExes = new List<string>()
            };
            foreach (var item in lstBlockedExes.Items)
            {
                sec.blockedExes.Add(item.ToString());
            }

            List<string> details;
            bool ok = SecurityPolicyManager.ApplyFullLock(sec, out details);

            if (ok)
            {
                MessageBox.Show(Localizer.T("settings_lock_applied_success"), Localizer.T("app_title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(Localizer.T("settings_lock_applied_error"), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnUnlockNow_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(Localizer.T("settings_unlock_confirm"), Localizer.T("settings_title"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                List<string> details;
                SecurityPolicyManager.UnlockAll(out details);
                MessageBox.Show(Localizer.T("settings_unlock_applied_success"), Localizer.T("app_title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
