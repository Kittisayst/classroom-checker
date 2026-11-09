using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ClassroomChecker.Config;
using ClassroomChecker.Core;
using ClassroomChecker.I18n;

namespace ClassroomChecker.UI
{
    /// <summary>
    /// UserControl ສຳລັບແທັບ "ປ້ອງກັນ" (Security & App Lock) ໃນໜ້າຫຼັກ MainForm
    /// ສະແດງສະຖານະ, ຕົວເລືອກນະໂຍບາຍ, ລາຍຊື່ເກມ/ໂປຣແກຣມ ແລະ ໂຟນເດີທີ່ບລັອກ
    /// </summary>
    public class SecurityPanel : UserControl
    {
        private Panel pnlStatus;
        private Label lblStatusIcon;
        private Label lblStatusTitle;
        private Label lblStatusDesc;
        private Button btnRefresh;
        private Button btnLockNow;
        private Button btnUnlockNow;

        private GroupBox grpPolicies;
        private CheckBox chkBlockUsb;
        private CheckBox chkBlockDownloads;
        private CheckBox chkBlockPopularGames;

        private TableLayoutPanel tblLists;
        private GroupBox grpBlockedExes;
        private ListBox lstBlockedExes;
        private Button btnAddExe;
        private Button btnDeleteExe;

        private GroupBox grpBlockedPaths;
        private ListBox lstBlockedPaths;
        private Button btnBrowseFolder;
        private Button btnDeletePath;

        private Panel pnlBottom;
        private Label lblNote;
        private Button btnSaveConfig;

        public SecurityPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Size = new Size(970, 543);
            this.Dock = DockStyle.Fill;
            this.Font = new Font("Phetsarath OT", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Padding = new Padding(12);

            // -------------------------------------------------------------
            // 1. Top Status Banner Card
            // -------------------------------------------------------------
            pnlStatus = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                BackColor = Color.FromArgb(236, 253, 245),
                Padding = new Padding(12, 10, 12, 10)
            };
            pnlStatus.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, pnlStatus.ClientRectangle,
                    Color.FromArgb(167, 243, 208), ButtonBorderStyle.Solid);
            };

            FlowLayoutPanel flpStatusActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 0)
            };

            btnUnlockNow = new Button
            {
                Text = "🔓 ປິດການລັອກ",
                BackColor = Color.FromArgb(71, 85, 105),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Phetsarath OT", 9F, FontStyle.Regular),
                Size = new Size(130, 38),
                Margin = new Padding(4, 0, 4, 0),
                Cursor = Cursors.Hand
            };
            btnUnlockNow.FlatAppearance.BorderSize = 0;
            btnUnlockNow.Click += btnUnlockNow_Click;

            btnLockNow = new Button
            {
                Text = "🔒 ເປີດການລັອກ",
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Phetsarath OT", 9F, FontStyle.Bold),
                Size = new Size(150, 38),
                Margin = new Padding(4, 0, 4, 0),
                Cursor = Cursors.Hand
            };
            btnLockNow.FlatAppearance.BorderSize = 0;
            btnLockNow.Click += btnLockNow_Click;

            btnRefresh = new Button
            {
                Text = "🔄 ກວດສອບ",
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Phetsarath OT", 8.5F, FontStyle.Regular),
                Size = new Size(100, 38),
                Margin = new Padding(4, 0, 4, 0),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefresh.Click += (s, e) => RefreshPolicyStatus();

            flpStatusActions.Controls.Add(btnUnlockNow);
            flpStatusActions.Controls.Add(btnLockNow);
            flpStatusActions.Controls.Add(btnRefresh);

            lblStatusIcon = new Label
            {
                Text = "🛡️",
                Font = new Font("Segoe UI Emoji", 20F, FontStyle.Regular),
                Dock = DockStyle.Left,
                Width = 46,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Panel pnlStatusInfo = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(8, 4, 8, 4)
            };

            lblStatusTitle = new Label
            {
                Text = "ລະບົບປ້ອງກັນ ແລະ ລັອກໂປຣແກຣມ / ເກມ",
                Font = new Font("Phetsarath OT", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Top,
                Height = 26,
                AutoEllipsis = true
            };

            lblStatusDesc = new Label
            {
                Text = "ກຳລັງກວດສອບສະຖານະ...",
                Font = new Font("Phetsarath OT", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(4, 120, 87),
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            };

            pnlStatusInfo.Controls.Add(lblStatusDesc);
            pnlStatusInfo.Controls.Add(lblStatusTitle);

            pnlStatus.Controls.Add(pnlStatusInfo);
            pnlStatus.Controls.Add(lblStatusIcon);
            pnlStatus.Controls.Add(flpStatusActions);

            // -------------------------------------------------------------
            // 2. Policies Options Group
            // -------------------------------------------------------------
            grpPolicies = new GroupBox
            {
                Dock = DockStyle.Top,
                Height = 112,
                Text = "ນະໂຍບາຍຄວາມປອດໄພ (Security Policies)",
                Font = new Font("Phetsarath OT", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(14, 10, 14, 10)
            };

            chkBlockUsb = new CheckBox
            {
                Text = "🔒 ຫ້າມ Run ໂປຣແກຣມ/ເກມ ຈາກ Flash Drive USB (ຍົກເວັ້ນ ClassroomChecker ໃຊ້ງານໄດ້ປົກກະຕິ)",
                Font = new Font("Phetsarath OT", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(16, 24),
                Size = new Size(720, 24),
                AutoSize = true,
                Cursor = Cursors.Hand
            };

            chkBlockDownloads = new CheckBox
            {
                Text = "📁 ຫ້າມ Run ໂປຣແກຣມ ຈາກໂຟນເດີ Downloads ແລະ Temp (Windows Safer Rules)",
                Font = new Font("Phetsarath OT", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(16, 52),
                Size = new Size(720, 24),
                AutoSize = true,
                Cursor = Cursors.Hand
            };

            chkBlockPopularGames = new CheckBox
            {
                Text = "🚫 ບລັອກເກມຍອດຮິດອັດຕະໂນມັດ (Roblox, Steam, Garena, Epic, Minecraft, Discord...)",
                Font = new Font("Phetsarath OT", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(16, 80),
                Size = new Size(720, 24),
                AutoSize = true,
                Cursor = Cursors.Hand
            };

            grpPolicies.Controls.AddRange(new Control[] {
                chkBlockUsb, chkBlockDownloads, chkBlockPopularGames
            });

            // -------------------------------------------------------------
            // 3. Side-by-Side Lists (Exes & Folders)
            // -------------------------------------------------------------
            tblLists = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 8, 0, 8)
            };
            tblLists.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblLists.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblLists.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Left: Blocked Exes
            grpBlockedExes = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "ລາຍຊື່ .exe ທີ່ບລັອກ (Blocked Programs List)",
                Font = new Font("Phetsarath OT", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(10, 8, 10, 8)
            };

            lstBlockedExes = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9.5F, FontStyle.Regular),
                IntegralHeight = false
            };

            Panel pnlExesButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                Padding = new Padding(0, 4, 0, 0)
            };

            btnAddExe = new Button
            {
                Text = "➕ ເພີ່ມ .exe",
                Font = new Font("Phetsarath OT", 9F, FontStyle.Regular),
                Location = new Point(0, 4),
                Size = new Size(120, 30),
                Cursor = Cursors.Hand
            };
            btnAddExe.Click += btnAddExe_Click;

            btnDeleteExe = new Button
            {
                Text = "🗑️ ລຶບອອກ",
                Font = new Font("Phetsarath OT", 9F, FontStyle.Regular),
                Location = new Point(128, 4),
                Size = new Size(95, 30),
                Cursor = Cursors.Hand
            };
            btnDeleteExe.Click += btnDeleteExe_Click;

            pnlExesButtons.Controls.AddRange(new Control[] { btnAddExe, btnDeleteExe });
            grpBlockedExes.Controls.Add(lstBlockedExes);
            grpBlockedExes.Controls.Add(pnlExesButtons);

            // Right: Blocked Paths
            grpBlockedPaths = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "📁 ບລັອກທັງໂຟນເດີ (Lock all .exe in folder)",
                Font = new Font("Phetsarath OT", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(10, 8, 10, 8)
            };

            lstBlockedPaths = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9.5F, FontStyle.Regular),
                IntegralHeight = false
            };

            Panel pnlPathsButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                Padding = new Padding(0, 4, 0, 0)
            };

            btnBrowseFolder = new Button
            {
                Text = "📁 ເລືອກໂຟນເດີ...",
                Font = new Font("Phetsarath OT", 9F, FontStyle.Regular),
                Location = new Point(0, 4),
                Size = new Size(140, 30),
                Cursor = Cursors.Hand
            };
            btnBrowseFolder.Click += btnBrowseFolder_Click;

            btnDeletePath = new Button
            {
                Text = "🗑️ ລຶບອອກ",
                Font = new Font("Phetsarath OT", 9F, FontStyle.Regular),
                Location = new Point(148, 4),
                Size = new Size(95, 30),
                Cursor = Cursors.Hand
            };
            btnDeletePath.Click += btnDeletePath_Click;

            pnlPathsButtons.Controls.AddRange(new Control[] { btnBrowseFolder, btnDeletePath });
            grpBlockedPaths.Controls.Add(lstBlockedPaths);
            grpBlockedPaths.Controls.Add(pnlPathsButtons);

            tblLists.Controls.Add(grpBlockedExes, 0, 0);
            tblLists.Controls.Add(grpBlockedPaths, 1, 0);

            // -------------------------------------------------------------
            // 4. Bottom Note & Save Action Bar
            // -------------------------------------------------------------
            pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                Padding = new Padding(0, 6, 0, 0)
            };

            btnSaveConfig = new Button
            {
                Text = "💾 ບັນທຶກການຕັ້ງຄ່າ",
                Dock = DockStyle.Right,
                Width = 160,
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Phetsarath OT", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSaveConfig.FlatAppearance.BorderSize = 0;
            btnSaveConfig.Click += btnSaveConfig_Click;

            lblNote = new Label
            {
                Text = "💡 ໝາຍເຫດ: ການລັອກຈະມີຜົນທັນທີກັບທຸກ User ໃນເຄື່ອງ. ສາມາດປິດການລັອກຄືນໄດ້ທຸກເວລາດ້ວຍປຸ່ມ 'ປິດການລັອກ'.",
                Font = new Font("Phetsarath OT", 8.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 116, 139),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            pnlBottom.Controls.Add(lblNote);
            pnlBottom.Controls.Add(btnSaveConfig);

            // -------------------------------------------------------------
            // Main Controls Hierarchy (Dock order matters)
            // -------------------------------------------------------------
            this.Controls.Add(tblLists);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(grpPolicies);
            this.Controls.Add(pnlStatus);

            this.Load += SecurityPanel_Load;
            this.ResumeLayout(false);
        }

        private void SecurityPanel_Load(object sender, EventArgs e)
        {
            Localizer.LanguageChanged += OnLanguageChanged;

            LoadFromConfig();
            ApplyLocalization();
            RefreshPolicyStatus();
        }

        private void OnLanguageChanged()
        {
            ApplyLocalization();
            RefreshPolicyStatus();
        }

        public void ApplyLocalization()
        {
            lblStatusTitle.Text = Localizer.T("security_panel_title", "ລະບົບປ້ອງກັນ ແລະ ລັອກໂປຣແກຣມ / ເກມ");
            btnRefresh.Text = Localizer.T("security_btn_refresh", "🔄 ກວດສອບ");
            btnLockNow.Text = Localizer.T("settings_btn_lock_now");
            btnUnlockNow.Text = Localizer.T("settings_btn_unlock_now");

            grpPolicies.Text = Localizer.T("settings_grp_security_policies");
            chkBlockUsb.Text = Localizer.T("settings_chk_block_usb");
            chkBlockDownloads.Text = Localizer.T("settings_chk_block_downloads");
            chkBlockPopularGames.Text = Localizer.T("settings_chk_block_popular_games");

            grpBlockedExes.Text = Localizer.T("settings_grp_blocked_exes");
            btnAddExe.Text = Localizer.T("settings_btn_add_exe");
            btnDeleteExe.Text = Localizer.T("settings_btn_delete_exe");

            grpBlockedPaths.Text = Localizer.T("settings_grp_blocked_paths");
            btnBrowseFolder.Text = Localizer.T("settings_btn_browse_folder");
            btnDeletePath.Text = Localizer.T("settings_btn_delete_folder");

            btnSaveConfig.Text = Localizer.T("btn_save");
            lblNote.Text = Localizer.T("security_panel_note", "💡 ໝາຍເຫດ: ການລັອກຈະມີຜົນທັນທີກັບທຸກ User ໃນເຄື່ອງ. ສາມາດປິດການລັອກຄືນໄດ້ທຸກເວລາດ້ວຍປຸ່ມ 'ປິດການລັອກ'.");
        }

        public void LoadFromConfig()
        {
            var cfg = ConfigService.Instance.Current.security;
            if (cfg == null) cfg = new SecurityConfig();

            chkBlockUsb.Checked = cfg.blockUsbExecution;
            chkBlockDownloads.Checked = cfg.blockDownloadsExecution;
            chkBlockPopularGames.Checked = cfg.blockPopularGames;

            lstBlockedExes.Items.Clear();
            if (cfg.blockedExes != null)
            {
                foreach (var exe in cfg.blockedExes)
                {
                    if (!string.IsNullOrEmpty(exe)) lstBlockedExes.Items.Add(exe);
                }
            }

            lstBlockedPaths.Items.Clear();
            if (cfg.blockedPaths != null)
            {
                foreach (var p in cfg.blockedPaths)
                {
                    if (!string.IsNullOrEmpty(p)) lstBlockedPaths.Items.Add(p);
                }
            }
        }

        private void SaveToConfig()
        {
            var cfg = ConfigService.Instance.Current;
            if (cfg.security == null) cfg.security = new SecurityConfig();

            cfg.security.blockUsbExecution = chkBlockUsb.Checked;
            cfg.security.blockDownloadsExecution = chkBlockDownloads.Checked;
            cfg.security.blockPopularGames = chkBlockPopularGames.Checked;

            cfg.security.blockedExes = new List<string>();
            foreach (var item in lstBlockedExes.Items)
            {
                cfg.security.blockedExes.Add(item.ToString());
            }

            cfg.security.blockedPaths = new List<string>();
            foreach (var item in lstBlockedPaths.Items)
            {
                cfg.security.blockedPaths.Add(item.ToString());
            }

            ConfigService.Instance.Save();
        }

        public void RefreshPolicyStatus()
        {
            try
            {
                bool usbBlocked = SecurityPolicyManager.IsUsbExecutionBlocked();
                bool dlBlocked = SecurityPolicyManager.IsDownloadsExecutionBlocked();
                bool disallowActive = SecurityPolicyManager.IsDisallowRunActive();
                int disallowCount = SecurityPolicyManager.GetDisallowedExesCount();

                bool anyLocked = usbBlocked || dlBlocked || disallowActive;
                bool fullLocked = usbBlocked && dlBlocked && disallowActive;

                if (fullLocked)
                {
                    pnlStatus.BackColor = Color.FromArgb(236, 253, 245);
                    lblStatusIcon.Text = "🛡️";
                    lblStatusDesc.ForeColor = Color.FromArgb(4, 120, 87);
                    lblStatusDesc.Text = Localizer.T("security_status_full_locked",
                        "🔒 ລະບົບປ້ອງກັນກຳລັງເຮັດວຽກ: USB, Downloads/Temp ແລະ ເກມຖືກບລັອກຮຽບຮ້ອຍແລ້ວ (Whitelisted ClassroomChecker)");
                }
                else if (anyLocked)
                {
                    pnlStatus.BackColor = Color.FromArgb(254, 249, 195);
                    lblStatusIcon.Text = "⚠️";
                    lblStatusDesc.ForeColor = Color.FromArgb(161, 98, 7);
                    lblStatusDesc.Text = string.Format(
                        Localizer.T("security_status_partial", "⚠️ ເປີດລັອກບາງສ່ວນ: USB={0}, Downloads={1}, ເກມ={2} ({3} exes)"),
                        usbBlocked ? "✓" : "✗",
                        dlBlocked ? "✓" : "✗",
                        disallowActive ? "✓" : "✗",
                        disallowCount);
                }
                else
                {
                    pnlStatus.BackColor = Color.FromArgb(254, 242, 242);
                    lblStatusIcon.Text = "🔓";
                    lblStatusDesc.ForeColor = Color.FromArgb(185, 28, 28);
                    lblStatusDesc.Text = Localizer.T("security_status_unlocked",
                        "⚠️ ຍັງບໍ່ໄດ້ເປີດການລັອກ: ນັກຮຽນຍັງສາມາດ Run ເກມ ຫຼື ໂປຣແກຣມຈາກ USB ແລະ Downloads ໄດ້");
                }

                pnlStatus.Invalidate();
            }
            catch (Exception ex)
            {
                lblStatusDesc.Text = "Error: " + ex.Message;
            }
        }

        private void btnAddExe_Click(object sender, EventArgs e)
        {
            string exeName = null;
            using (var dlg = new InputDialog(
                Localizer.T("settings_prompt_add_exe_title"),
                Localizer.T("settings_prompt_add_exe"),
                ""))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    exeName = dlg.InputText;
                }
            }

            if (string.IsNullOrEmpty(exeName)) return;

            exeName = exeName.Trim().ToLowerInvariant();
            if (!exeName.EndsWith(".exe")) exeName += ".exe";

            foreach (var item in lstBlockedExes.Items)
            {
                if (string.Equals(item.ToString(), exeName, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("ໂປຣແກຣມນີ້ມີໃນລາຍຊື່ແລ້ວ!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            lstBlockedExes.Items.Add(exeName);
            lstBlockedExes.SelectedItem = exeName;
        }

        private void btnDeleteExe_Click(object sender, EventArgs e)
        {
            if (lstBlockedExes.SelectedItem == null) return;
            int idx = lstBlockedExes.SelectedIndex;
            lstBlockedExes.Items.RemoveAt(idx);
            if (lstBlockedExes.Items.Count > 0)
            {
                lstBlockedExes.SelectedIndex = Math.Min(idx, lstBlockedExes.Items.Count - 1);
            }
        }

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = Localizer.T("settings_prompt_browse_folder");
                fbd.ShowNewFolderButton = false;
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    string path = fbd.SelectedPath;
                    if (string.IsNullOrEmpty(path)) return;

                    foreach (var item in lstBlockedPaths.Items)
                    {
                        if (string.Equals(item.ToString(), path, StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("ໂຟນເດີນີ້ມີໃນລາຍຊື່ແລ້ວ!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    lstBlockedPaths.Items.Add(path);
                    lstBlockedPaths.SelectedItem = path;
                }
            }
        }

        private void btnDeletePath_Click(object sender, EventArgs e)
        {
            if (lstBlockedPaths.SelectedItem == null) return;
            int idx = lstBlockedPaths.SelectedIndex;
            lstBlockedPaths.Items.RemoveAt(idx);
            if (lstBlockedPaths.Items.Count > 0)
            {
                lstBlockedPaths.SelectedIndex = Math.Min(idx, lstBlockedPaths.Items.Count - 1);
            }
        }

        private void btnLockNow_Click(object sender, EventArgs e)
        {
            SaveToConfig();
            var cfg = ConfigService.Instance.Current.security;

            List<string> details;
            bool ok = SecurityPolicyManager.ApplyFullLock(cfg, out details);

            RefreshPolicyStatus();

            if (ok)
            {
                MessageBox.Show(
                    Localizer.T("settings_lock_applied_success"),
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    Localizer.T("settings_lock_applied_error"),
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void btnUnlockNow_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(
                Localizer.T("settings_unlock_confirm"),
                Localizer.T("confirm_delete_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (res != DialogResult.Yes) return;

            List<string> details;
            bool ok = SecurityPolicyManager.UnlockAll(out details);

            RefreshPolicyStatus();

            if (ok)
            {
                MessageBox.Show(
                    Localizer.T("settings_unlock_applied_success"),
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            SaveToConfig();
            MessageBox.Show(
                Localizer.T("settings_saved_success"),
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
