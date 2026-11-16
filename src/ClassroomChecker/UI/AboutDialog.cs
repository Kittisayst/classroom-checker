using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ClassroomChecker.Core;
using ClassroomChecker.I18n;

namespace ClassroomChecker.UI
{
    /// <summary>
    /// ໜ້າກ່ຽວກັບໂປຣແກຣມ: ເວີຊັນ, ຜູ້ພັດທະນາ, ລິຂະສິດ, ລິ້ງ GitHub
    /// ແລະ ປຸ່ມກວດຫາ/ຕິດຕັ້ງອັບເດດ (ກົດເອງເທົ່ານັ້ນ ບໍ່ກວດອັດຕະໂນມັດ)
    /// </summary>
    public class AboutDialog : Form
    {
        private static readonly Color Brand = Color.FromArgb(26, 115, 232);
        private static readonly Color Muted = Color.FromArgb(100, 116, 139);
        private static readonly Color Ink = Color.FromArgb(15, 23, 42);
        private static readonly Color Ok = Color.FromArgb(22, 163, 74);
        private static readonly Color Warn = Color.FromArgb(217, 119, 6);
        private static readonly Color Bad = Color.FromArgb(220, 38, 38);

        private Label lblStatus;
        private ProgressBar progress;
        private TextBox txtNotes;
        private Button btnUpdate;
        private Button btnClose;

        private ReleaseInfo _release;
        private CancellationTokenSource _downloadCts;

        public AboutDialog()
        {
            BuildUi();
        }

        private void BuildUi()
        {
            SuspendLayout();
            Text = Localizer.T("about_title");
            ClientSize = new Size(560, 520);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = UiFontManager.GetFont(9.5F);

            Icon appIcon = LoadAppIcon();
            if (appIcon != null) Icon = appIcon;

            // ---------------------------------------------------------- header
            var header = new Panel { Dock = DockStyle.Top, Height = 112, BackColor = Brand };
            var pic = new PictureBox
            {
                Location = new Point(24, 24),
                Size = new Size(64, 64),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            pic.Image = LoadIconFrame(64);
            var lblName = new Label
            {
                Text = "Classroom Checker",
                Font = UiFontManager.GetFont(17F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(104, 24)
            };
            var lblVersion = new Label
            {
                Text = Localizer.T("about_version", UpdateService.CurrentVersionText),
                Font = UiFontManager.GetFont(10F),
                ForeColor = Color.FromArgb(219, 234, 254),
                AutoSize = true,
                Location = new Point(106, 62)
            };
            header.Controls.AddRange(new Control[] { pic, lblName, lblVersion });

            // ---------------------------------------------------------- info
            int y = 128;
            var lblDesc = new Label
            {
                Text = Localizer.T("about_description"),
                ForeColor = Ink,
                Location = new Point(24, y),
                Size = new Size(512, 44),
                UseMnemonic = false
            };
            y += 52;

            AddInfoRow(Localizer.T("about_developer"), UpdateService.RepoOwner, null, ref y);
            AddInfoRow(Localizer.T("about_license"), "MIT License", null, ref y);
            AddInfoRow("GitHub", UpdateService.RepoUrl.Replace("https://", ""), UpdateService.RepoUrl, ref y);

            // ---------------------------------------------------------- update box
            y += 10;
            var grp = new GroupBox
            {
                Text = Localizer.T("about_update_group"),
                Location = new Point(24, y),
                Size = new Size(512, 190),
                ForeColor = Ink
            };
            lblStatus = new Label
            {
                Text = Localizer.T("about_update_idle"),
                ForeColor = Muted,
                Location = new Point(14, 26),
                Size = new Size(484, 40),
                UseMnemonic = false
            };
            progress = new ProgressBar
            {
                Location = new Point(14, 68),
                Size = new Size(484, 14),
                Visible = false
            };
            txtNotes = new TextBox
            {
                Location = new Point(14, 88),
                Size = new Size(484, 58),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(248, 250, 252),
                Font = UiFontManager.GetFont(8.5F),
                Visible = false
            };
            btnUpdate = new Button
            {
                Text = Localizer.T("about_btn_check"),
                Location = new Point(14, 152),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(170, 32),
                Padding = new Padding(10, 0, 10, 0),
                BackColor = Brand,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = UiFontManager.GetFont(9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.Click += BtnUpdate_Click;
            grp.Controls.AddRange(new Control[] { lblStatus, progress, txtNotes, btnUpdate });

            // ---------------------------------------------------------- footer
            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(16, 10, 16, 0),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            btnClose = FooterButton(Localizer.T("btn_close"));
            btnClose.Click += (s, e) => Close();
            Button btnNotes = FooterButton(Localizer.T("about_btn_releases"));
            btnNotes.Click += (s, e) => OpenUrl(_release != null && !string.IsNullOrEmpty(_release.HtmlUrl) ? _release.HtmlUrl : UpdateService.ReleasesUrl);
            Button btnGitHub = FooterButton(Localizer.T("about_btn_github"));
            btnGitHub.Click += (s, e) => OpenUrl(UpdateService.RepoUrl);
            footer.Controls.AddRange(new Control[] { btnClose, btnNotes, btnGitHub });

            Controls.AddRange(new Control[] { lblDesc, grp, footer, header });
            CancelButton = btnClose;
            FormClosing += AboutDialog_FormClosing;
            ResumeLayout(true);
        }

        private void AddInfoRow(string caption, string value, string url, ref int y)
        {
            var cap = new Label
            {
                Text = caption,
                ForeColor = Muted,
                Location = new Point(24, y),
                Size = new Size(120, 24),
                UseMnemonic = false
            };
            Control val;
            if (url != null)
            {
                var link = new LinkLabel
                {
                    Text = value,
                    Location = new Point(148, y),
                    AutoSize = true,
                    LinkColor = Brand,
                    ActiveLinkColor = Brand
                };
                link.LinkClicked += (s, e) => OpenUrl(url);
                val = link;
            }
            else
            {
                val = new Label { Text = value, ForeColor = Ink, Location = new Point(148, y), AutoSize = true, UseMnemonic = false };
            }
            Controls.Add(cap);
            Controls.Add(val);
            y += 28;
        }

        private static Button FooterButton(string text)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(96, 32),
                Padding = new Padding(8, 0, 8, 0),
                Margin = new Padding(6, 0, 0, 0),
                BackColor = Color.White,
                ForeColor = Ink,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            return b;
        }

        // ------------------------------------------------------------------ ອັບເດດ

        private async void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (_downloadCts != null)
            {
                _downloadCts.Cancel(); // ປຸ່ມກາຍເປັນ "ຍົກເລີກ" ລະຫວ່າງດາວໂຫຼດ
                return;
            }
            if (_release != null && UpdateService.IsNewer(_release.Version, UpdateService.CurrentVersion))
                await DownloadAndInstallAsync();
            else
                await CheckAsync();
        }

        private async System.Threading.Tasks.Task CheckAsync()
        {
            SetBusy(true);
            SetStatus(Localizer.T("about_update_checking"), Muted);
            txtNotes.Visible = false;
            try
            {
                _release = await UpdateService.CheckLatestAsync();
                if (IsDisposed) return;

                if (!UpdateService.IsNewer(_release.Version, UpdateService.CurrentVersion))
                {
                    SetStatus(Localizer.T("about_update_latest", UpdateService.CurrentVersionText), Ok);
                    btnUpdate.Text = Localizer.T("about_btn_check");
                }
                else
                {
                    string ver = UpdateService.FormatVersion(_release.Version);
                    string date = _release.PublishedAt.HasValue ? _release.PublishedAt.Value.ToString("dd/MM/yyyy") : "-";
                    SetStatus(Localizer.T("about_update_available", ver, date), Warn);
                    txtNotes.Text = UpdateService.CleanNotes(_release.Notes);
                    txtNotes.Visible = txtNotes.Text.Length > 0;
                    btnUpdate.Text = Localizer.T("about_btn_install", ver);
                }
            }
            catch (Exception ex)
            {
                ReportService.LogError("AboutDialog.CheckAsync", ex);
                if (IsDisposed) return;
                SetStatus(Localizer.T("about_update_offline"), Bad);
            }
            finally
            {
                if (!IsDisposed) SetBusy(false);
            }
        }

        private async System.Threading.Tasks.Task DownloadAndInstallAsync()
        {
            string ver = UpdateService.FormatVersion(_release.Version);
            var confirm = MessageBox.Show(this,
                Localizer.T("about_install_confirm", ver),
                Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (confirm != DialogResult.OK) return;

            _downloadCts = new CancellationTokenSource();
            SetBusy(true);
            btnUpdate.Enabled = true; // ໃຊ້ເປັນປຸ່ມຍົກເລີກ
            btnUpdate.Text = Localizer.T("about_btn_cancel");
            progress.Value = 0;
            progress.Visible = true;
            SetStatus(Localizer.T("about_update_downloading", ver), Muted);

            string downloaded = null;
            try
            {
                var report = new Progress<int>(p => { if (!progress.IsDisposed) progress.Value = Math.Max(0, Math.Min(100, p)); });
                downloaded = await UpdateService.DownloadAsync(_release, report, _downloadCts.Token);
            }
            catch (OperationCanceledException)
            {
                if (!IsDisposed) SetStatus(Localizer.T("about_update_cancelled"), Muted);
            }
            catch (InvalidDataException ex)
            {
                ReportService.LogError("AboutDialog.DownloadAndInstallAsync", ex);
                if (!IsDisposed) SetStatus(Localizer.T("about_update_bad_file"), Bad);
            }
            catch (Exception ex)
            {
                ReportService.LogError("AboutDialog.DownloadAndInstallAsync", ex);
                if (!IsDisposed) SetStatus(Localizer.T("about_update_download_failed"), Bad);
            }
            finally
            {
                _downloadCts.Dispose();
                _downloadCts = null;
            }

            if (IsDisposed) return;
            progress.Visible = false;
            SetBusy(false);
            btnUpdate.Text = Localizer.T("about_btn_install", ver);
            if (downloaded == null) return;

            try
            {
                SetStatus(Localizer.T("about_update_installing"), Muted);
                UpdateService.InstallAndRestart(downloaded);
                Application.Exit();
            }
            catch (Exception ex)
            {
                // ສ່ວນຫຼາຍແມ່ນ USB ຖືກລັອກບໍ່ໃຫ້ຂຽນ ຫຼື ບໍ່ມີສິດ: exe ເກົ່າຍັງຢູ່ຄືເກົ່າ
                ReportService.LogError("AboutDialog.DownloadAndInstallAsync", ex);
                SetStatus(Localizer.T("about_update_install_failed", Path.GetDirectoryName(downloaded)), Bad);
            }
        }

        private void AboutDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_downloadCts != null) _downloadCts.Cancel();
        }

        private void SetBusy(bool busy)
        {
            btnUpdate.Enabled = !busy;
            UseWaitCursor = busy && _downloadCts == null;
        }

        private void SetStatus(string text, Color color)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = color;
        }

        private static void OpenUrl(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch (Exception ex) { ReportService.LogError("AboutDialog.OpenUrl", ex); }
        }

        /// <summary>
        /// ອ່ານຮູບຂະໜາດ size ຈາກ app.ico ໂດຍກົງ: ທຸກ frame ໃນ ico ເປັນ PNG ເຊິ່ງ Icon.ToBitmap()
        /// ຂອງ .NET Framework ອ່ານບໍ່ໄດ້. ບໍ່ພົບຂະໜາດທີ່ຕ້ອງການ → ໃຊ້ frame ໃຫຍ່ສຸດ
        /// </summary>
        private static Image LoadIconFrame(int size)
        {
            try
            {
                byte[] ico;
                using (Stream stream = typeof(AboutDialog).Assembly.GetManifestResourceStream("ClassroomChecker.app.ico"))
                {
                    if (stream == null) return null;
                    using (var ms = new MemoryStream()) { stream.CopyTo(ms); ico = ms.ToArray(); }
                }

                int count = BitConverter.ToUInt16(ico, 4);
                int bestOffset = -1, bestLength = 0, bestWidth = -1;
                for (int i = 0; i < count; i++)
                {
                    int entry = 6 + 16 * i;
                    int width = ico[entry] == 0 ? 256 : ico[entry];
                    int length = BitConverter.ToInt32(ico, entry + 8);
                    int offset = BitConverter.ToInt32(ico, entry + 12);
                    bool better = width == size || (bestWidth != size && width > bestWidth);
                    if (better) { bestOffset = offset; bestLength = length; bestWidth = width; }
                }
                if (bestOffset < 0) return null;

                using (var frame = new MemoryStream(ico, bestOffset, bestLength))
                using (Image img = Image.FromStream(frame))
                    return new Bitmap(img); // ສຳເນົາ ເພື່ອບໍ່ໃຫ້ຜູກກັບ stream ທີ່ປິດແລ້ວ
            }
            catch (Exception ex)
            {
                ReportService.LogError("AboutDialog.LoadIconFrame", ex);
                return null;
            }
        }

        private static Icon LoadAppIcon()
        {
            try
            {
                using (Stream stream = typeof(AboutDialog).Assembly.GetManifestResourceStream("ClassroomChecker.app.ico"))
                    return stream != null ? new Icon(stream) : null;
            }
            catch (Exception ex)
            {
                ReportService.LogError("AboutDialog.LoadAppIcon", ex);
                return null;
            }
        }
    }
}
