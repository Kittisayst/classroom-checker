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
    /// UserControl ສຳລັບແທັບ "ຕິດຕັ້ງຊອບແວ" (Software Installer) ໃນໜ້າຫຼັກ MainForm
    /// ສະແດງຕາຕະລາງຊອບແວ Open Source / ພື້ນຖານ, ສະຖານະໃນເຄື່ອງ, ໄຟລ໌ໃນ USB ແລະ ປຸ່ມກົດຕິດຕັ້ງ
    /// </summary>
    public class SoftwareInstallPanel : UserControl
    {
        private Panel pnlHeader;
        private Label lblHeaderTitle;
        private Label lblHeaderSubtitle;
        private Button btnOpenFolder;
        private Button btnRefresh;
        private Button btnInstallAllMissing;

        private DataGridView dgvSoftware;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colCategory;
        private DataGridViewTextBoxColumn colLocalStatus;
        private DataGridViewTextBoxColumn colUsbStatus;
        private DataGridViewButtonColumn colAction;

        private Panel pnlBottom;
        private ProgressBar prgInstall;
        private Label lblSummary;
        private Label lblTip;

        private bool _isBusy = false;

        public SoftwareInstallPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Dock = DockStyle.Fill;
            this.Font = new Font("Phetsarath OT", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Padding = new Padding(12);

            // -------------------------------------------------------------
            // 1. Top Header Banner Card
            // -------------------------------------------------------------
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 74,
                BackColor = Color.FromArgb(238, 242, 255), // Light Indigo
                Padding = new Padding(14, 10, 14, 10)
            };
            pnlHeader.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, pnlHeader.ClientRectangle,
                    Color.FromArgb(199, 210, 254), ButtonBorderStyle.Solid);
            };

            lblHeaderTitle = new Label
            {
                Text = "📦 ລະບົບຕິດຕັ້ງຊອບແວພື້ນຖານ (Offline Software Installer)",
                Font = new Font("Phetsarath OT", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 27, 75),
                Location = new Point(14, 12),
                Size = new Size(500, 24),
                AutoEllipsis = true
            };

            lblHeaderSubtitle = new Label
            {
                Text = "ຕິດຕັ້ງໂປຣແກຣມ Open Source ແລະ ໂປຣແກຣມພື້ນຖານແບບ Silent ອັດຕະໂນມັດ ຈາກໂຟນເດີ installers/ ໃນ USB",
                Font = new Font("Phetsarath OT", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(79, 70, 229),
                Location = new Point(14, 38),
                Size = new Size(520, 22),
                AutoEllipsis = true
            };

            btnInstallAllMissing = new Button
            {
                Text = "⚡ ຕິດຕັ້ງທີ່ຂາດທັງໝົດ",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(16, 185, 129), // Emerald
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Phetsarath OT", 9F, FontStyle.Bold),
                Location = new Point(780, 18),
                Size = new Size(160, 36),
                Cursor = Cursors.Hand
            };
            btnInstallAllMissing.FlatAppearance.BorderSize = 0;
            btnInstallAllMissing.Click += btnInstallAllMissing_Click;

            btnOpenFolder = new Button
            {
                Text = "📁 ເປີດໂຟນເດີ installers",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Phetsarath OT", 8.5F, FontStyle.Regular),
                Location = new Point(620, 18),
                Size = new Size(152, 36),
                Cursor = Cursors.Hand
            };
            btnOpenFolder.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnOpenFolder.Click += btnOpenFolder_Click;

            btnRefresh = new Button
            {
                Text = "🔄 ກວດສອບໃໝ່",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Phetsarath OT", 8.5F, FontStyle.Regular),
                Location = new Point(505, 18),
                Size = new Size(108, 36),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefresh.Click += async (s, e) => await RefreshGridAsync();

            pnlHeader.Controls.AddRange(new Control[] {
                lblHeaderTitle, lblHeaderSubtitle,
                btnRefresh, btnOpenFolder, btnInstallAllMissing
            });

            // -------------------------------------------------------------
            // 2. DataGridView for Software Catalog
            // -------------------------------------------------------------
            dgvSoftware = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                RowTemplate = { Height = 44 },
                ColumnHeadersHeight = 36,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                EnableHeadersVisualStyles = false
            };
            dgvSoftware.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvSoftware.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
            dgvSoftware.ColumnHeadersDefaultCellStyle.Font = new Font("Phetsarath OT", 9F, FontStyle.Bold);

            colName = new DataGridViewTextBoxColumn
            {
                HeaderText = "ຊື່ໂປຣແກຣມ (Software)",
                Width = 260,
                ReadOnly = true
            };

            colCategory = new DataGridViewTextBoxColumn
            {
                HeaderText = "ໝວດໝູ່ (Category)",
                Width = 160,
                ReadOnly = true
            };

            colLocalStatus = new DataGridViewTextBoxColumn
            {
                HeaderText = "ສະຖານະໃນເຄື່ອງ (Installed)",
                Width = 190,
                ReadOnly = true
            };

            colUsbStatus = new DataGridViewTextBoxColumn
            {
                HeaderText = "ໄຟລ໌ໃນ USB (installers/)",
                Width = 210,
                ReadOnly = true
            };

            colAction = new DataGridViewButtonColumn
            {
                HeaderText = "ດຳເນີນການ (Action)",
                Width = 120,
                FlatStyle = FlatStyle.Flat
            };

            dgvSoftware.Columns.AddRange(new DataGridViewColumn[] {
                colName, colCategory, colLocalStatus, colUsbStatus, colAction
            });

            dgvSoftware.CellContentClick += dgvSoftware_CellContentClick;
            dgvSoftware.CellFormatting += dgvSoftware_CellFormatting;

            // -------------------------------------------------------------
            // 3. Bottom Status Bar & Progress
            // -------------------------------------------------------------
            pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                Padding = new Padding(0, 6, 0, 0)
            };

            prgInstall = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 6,
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Visible = false
            };

            lblSummary = new Label
            {
                Font = new Font("Phetsarath OT", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(4, 14),
                Size = new Size(500, 20),
                Text = "ກຳລັງໂຫຼດຂໍ້ມູນ..."
            };

            lblTip = new Label
            {
                Font = new Font("Phetsarath OT", 8.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(4, 38),
                Size = new Size(940, 20),
                Text = "💡 ຄຳແນະນຳ: ພຽງແຕ່ດາວໂຫຼດໄຟລ໌ Setup (.exe/.msi) ມາໂຍນໃສ່ໂຟນເດີ installers/ ໃນ USB ລະບົບຈະກວດພົບ ແລະ ຕິດຕັ້ງງຽບໆໃຫ້ທັນທີ."
            };

            pnlBottom.Controls.AddRange(new Control[] { prgInstall, lblSummary, lblTip });

            // Main Hierarchy (Dock layout)
            this.Controls.Add(dgvSoftware);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlHeader);

            this.Load += SoftwareInstallPanel_Load;
            this.ResumeLayout(false);
        }

        private async void SoftwareInstallPanel_Load(object sender, EventArgs e)
        {
            Localizer.LanguageChanged += OnLanguageChanged;
            ApplyLocalization();
            await RefreshGridAsync();
        }

        private void OnLanguageChanged()
        {
            ApplyLocalization();
        }

        public void ApplyLocalization()
        {
            lblHeaderTitle.Text = Localizer.T("installer_title", "📦 ລະບົບຕິດຕັ້ງຊອບແວພື້ນຖານ (Offline Software Installer)");
            lblHeaderSubtitle.Text = Localizer.T("installer_subtitle", "ຕິດຕັ້ງໂປຣແກຣມ Open Source ແລະ ໂປຣແກຣມພື້ນຖານແບບ Silent ອັດຕະໂນມັດ ຈາກໂຟນເດີ installers/ ໃນ USB");
            btnOpenFolder.Text = Localizer.T("installer_btn_open_folder", "📁 ເປີດໂຟນເດີ installers");
            btnRefresh.Text = Localizer.T("installer_btn_refresh", "🔄 ກວດສອບໃໝ່");
            btnInstallAllMissing.Text = Localizer.T("installer_btn_install_all", "⚡ ຕິດຕັ້ງທີ່ຂາດທັງໝົດ");

            colName.HeaderText = Localizer.T("installer_col_name", "ຊື່ໂປຣແກຣມ (Software)");
            colCategory.HeaderText = Localizer.T("installer_col_category", "ໝວດໝູ່ (Category)");
            colLocalStatus.HeaderText = Localizer.T("installer_col_local_status", "ສະຖານະໃນເຄື່ອງ (Installed)");
            colUsbStatus.HeaderText = Localizer.T("installer_col_usb_status", "ໄຟລ໌ໃນ USB (installers/)");
            colAction.HeaderText = Localizer.T("installer_col_action", "ດຳເນີນການ (Action)");

            lblTip.Text = Localizer.T("installer_tip", "💡 ຄຳແນະນຳ: ພຽງແຕ່ດາວໂຫຼດໄຟລ໌ Setup (.exe/.msi) ມາໂຍນໃສ່ໂຟນເດີ installers/ ໃນ USB ລະບົບຈະກວດພົບ ແລະ ຕິດຕັ້ງງຽບໆໃຫ້ທັນທີ.");
        }

        public async Task RefreshGridAsync()
        {
            if (_isBusy) return;

            dgvSoftware.Rows.Clear();
            var catalog = SoftwareInstallerService.GetCatalog();

            int installedCount = 0;
            int availableInUsbCount = 0;

            await Task.Run(() =>
            {
                // Background inspect
            });

            foreach (var pkg in catalog)
            {
                string ver;
                string path;
                bool isInst = SoftwareInstallerService.IsInstalled(pkg, out ver, out path);

                string usbFile = SoftwareInstallerService.FindInstallerFile(pkg);
                bool hasUsb = !string.IsNullOrEmpty(usbFile);

                if (isInst) installedCount++;
                if (hasUsb) availableInUsbCount++;

                int idx = dgvSoftware.Rows.Add();
                var row = dgvSoftware.Rows[idx];
                row.Tag = pkg;

                row.Cells[colName.Index].Value = pkg.Name + " (" + pkg.LicenseType + ")";
                row.Cells[colCategory.Index].Value = pkg.Category;

                if (isInst)
                {
                    row.Cells[colLocalStatus.Index].Value = "✓ ຕິດຕັ້ງແລ້ວ (" + ver + ")";
                }
                else
                {
                    row.Cells[colLocalStatus.Index].Value = "✗ ຍັງບໍ່ທັນຕິດຕັ້ງ";
                }

                if (hasUsb)
                {
                    row.Cells[colUsbStatus.Index].Value = "✓ ພົບ: " + Path.GetFileName(usbFile);
                }
                else
                {
                    row.Cells[colUsbStatus.Index].Value = "⚠️ ບໍ່ພົບໄຟລ໌ໃນ USB";
                }

                var btnCell = row.Cells[colAction.Index] as DataGridViewButtonCell;
                if (btnCell != null)
                {
                    if (isInst)
                    {
                        btnCell.Value = "✓ ຕິດຕັ້ງແລ້ວ";
                    }
                    else if (hasUsb)
                    {
                        btnCell.Value = "📥 ຕິດຕັ້ງ";
                    }
                    else
                    {
                        btnCell.Value = "ບໍ່ມີຕົວຕິດຕັ້ງ";
                    }
                }
            }

            lblSummary.Text = string.Format(
                "ພົບຕົວຕິດຕັ້ງໃນ USB: {0}/{1} ໂປຣແກຣມ  |  ຕິດຕັ້ງໃນເຄື່ອງແລ້ວ: {2}/{1}",
                availableInUsbCount, catalog.Count, installedCount);

            btnInstallAllMissing.Enabled = (availableInUsbCount > 0) && (installedCount < catalog.Count);
        }

        private void dgvSoftware_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvSoftware.Rows.Count) return;

            var row = dgvSoftware.Rows[e.RowIndex];

            // Local status styling
            if (e.ColumnIndex == colLocalStatus.Index && e.Value != null)
            {
                string text = e.Value.ToString();
                if (text.StartsWith("✓"))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(4, 120, 87); // Green
                    e.CellStyle.Font = new Font("Phetsarath OT", 9F, FontStyle.Bold);
                }
                else
                {
                    e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28); // Red
                }
            }

            // USB status styling
            if (e.ColumnIndex == colUsbStatus.Index && e.Value != null)
            {
                string text = e.Value.ToString();
                if (text.StartsWith("✓"))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(4, 120, 87); // Green
                }
                else
                {
                    e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9); // Amber
                }
            }
        }

        private async void dgvSoftware_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != colAction.Index || _isBusy) return;

            var row = dgvSoftware.Rows[e.RowIndex];
            var pkg = row.Tag as SoftwarePackage;
            if (pkg == null) return;

            string installerPath = SoftwareInstallerService.FindInstallerFile(pkg);
            if (string.IsNullOrEmpty(installerPath))
            {
                MessageBox.Show(
                    "ບໍ່ພົບໄຟລ໌ Setup ຂອງ " + pkg.Name + " ໃນໂຟນເດີ installers/\n\nກະລຸນາດາວໂຫຼດໄຟລ໌ຕິດຕັ້ງມາໃສ່ໃນໂຟນເດີ installers/ ໃນ USB ກ່ອນ.",
                    "Missing Installer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string ver, instPath;
            if (SoftwareInstallerService.IsInstalled(pkg, out ver, out instPath))
            {
                var confirm = MessageBox.Show(
                    pkg.Name + " ຕິດຕັ້ງໃນເຄື່ອງແລ້ວ (" + ver + "). ທ່ານຕ້ອງການຕິດຕັ້ງທັບໃໝ່ແທ້ບໍ່?",
                    "Re-install",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;
            }

            SetBusy(true, "ກຳລັງຕິດຕັ້ງ " + pkg.Name + " ແບບ Silent...");
            try
            {
                var result = await SoftwareInstallerService.InstallAsync(pkg, installerPath);
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(result.Message, "Install Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                SetBusy(false);
                await RefreshGridAsync();
            }
        }

        private async void btnInstallAllMissing_Click(object sender, EventArgs e)
        {
            if (_isBusy) return;

            var catalog = SoftwareInstallerService.GetCatalog();
            var missingWithInstaller = new List<Tuple<SoftwarePackage, string>>();

            foreach (var pkg in catalog)
            {
                string ver, path;
                if (!SoftwareInstallerService.IsInstalled(pkg, out ver, out path))
                {
                    string inst = SoftwareInstallerService.FindInstallerFile(pkg);
                    if (!string.IsNullOrEmpty(inst))
                    {
                        missingWithInstaller.Add(Tuple.Create(pkg, inst));
                    }
                }
            }

            if (missingWithInstaller.Count == 0)
            {
                MessageBox.Show(
                    "ບໍ່ມີໂປຣແກຣມທີ່ຕ້ອງຕິດຕັ້ງ ຫຼື ບໍ່ພົບໄຟລ໌ setup ໃນ USB\n(ກະລຸນາກັອບປີ້ໄຟລ໌ຕິດຕັ້ງໃສ່ໂຟນເດີ installers/ ກ່ອນ)",
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var res = MessageBox.Show(
                string.Format("ລະບົບຈະດຳເນີນການຕິດຕັ້ງ {0} ໂປຣແກຣມແບບ Silent ອັດຕະໂນມັດ.\n\nທ່ານຕ້ອງການເລີ່ມຕົ້ນຕິດຕັ້ງເລີຍບໍ່?", missingWithInstaller.Count),
                "Confirm Silent Batch Install",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (res != DialogResult.Yes) return;

            SetBusy(true, "ກຳລັງເລີ່ມຕົ້ນການຕິດຕັ້ງ...");
            int successCount = 0;

            try
            {
                for (int i = 0; i < missingWithInstaller.Count; i++)
                {
                    var item = missingWithInstaller[i];
                    lblSummary.Text = string.Format("ກຳລັງຕິດຕັ້ງ ({0}/{1}): {2}...", i + 1, missingWithInstaller.Count, item.Item1.Name);

                    var r = await SoftwareInstallerService.InstallAsync(item.Item1, item.Item2);
                    if (r.Success) successCount++;
                }

                MessageBox.Show(
                    string.Format("ດຳເນີນການຕິດຕັ້ງສຳເລັດ {0}/{1} ໂປຣແກຣມ!", successCount, missingWithInstaller.Count),
                    "Batch Install Completed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            finally
            {
                SetBusy(false);
                await RefreshGridAsync();
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                string dir = SoftwareInstallerService.GetInstallersDirectory();
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                Process.Start("explorer.exe", dir);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ບໍ່ສາມາດເປີດໂຟນເດີໄດ້: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetBusy(bool busy, string statusText = "")
        {
            _isBusy = busy;
            prgInstall.Visible = busy;
            btnInstallAllMissing.Enabled = !busy;
            btnRefresh.Enabled = !busy;
            btnOpenFolder.Enabled = !busy;
            dgvSoftware.Enabled = !busy;

            if (!string.IsNullOrEmpty(statusText))
            {
                lblSummary.Text = statusText;
            }
        }
    }
}
