using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassroomChecker.Core;
using ClassroomChecker.I18n;
using ClassroomChecker.Modules;

namespace ClassroomChecker.UI
{
    /// <summary>
    /// ແທັບ "ຂໍ້ມູນຄອມ": ສະແດງ Spec ຂອງເຄື່ອງ (CPU, RAM, ດິສກ໌, OS, GPU, Network...)
    /// ພ້ອມປຸ່ມ ກວດສອບ (ໂຫຼດໃໝ່), Export Excel ແລະ ເປີດໂຟນເດີ specs.
    /// ໃຊ້ຂໍ້ມູນຊຸດດຽວກັບ SpecExportModule ເພື່ອໃຫ້ໜ້າຈໍ ແລະ ໄຟລ໌ Excel ກົງກັນ.
    /// </summary>
    public class MachineInfoPanel : UserControl
    {
        private static readonly Color Ink = Color.FromArgb(15, 23, 42);
        private static readonly Color Muted = Color.FromArgb(100, 116, 139);
        private static readonly Color Border = Color.FromArgb(203, 213, 225);
        private static readonly Color Accent = Color.FromArgb(13, 148, 136);   // teal
        private static readonly Color AccentSoft = Color.FromArgb(240, 253, 250);
        private static readonly Color AccentLine = Color.FromArgb(153, 246, 228);
        private static readonly Color Ok = Color.FromArgb(22, 163, 74);
        private static readonly Color Warn = Color.FromArgb(217, 119, 6);
        private static readonly Color Bad = Color.FromArgb(220, 38, 38);

        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnCheck;
        private Button btnExport;
        private Button btnOpenFolder;

        private readonly Dictionary<string, Label> _cardCaptions = new Dictionary<string, Label>();
        private readonly Dictionary<string, Label> _cardValues = new Dictionary<string, Label>();

        private DataGridView grid;
        private ProgressBar progress;
        private Label lblStatus;

        private readonly ToolTip _toolTip = new ToolTip();
        private readonly SpecExportModule _spec = new SpecExportModule();
        private bool _loadedOnce;
        private bool _busy;
        private string _lastExportPath;

        public MachineInfoPanel()
        {
            BuildUi();
            ApplyLocalization();
        }

        // ------------------------------------------------------------------ UI

        private void BuildUi()
        {
            SuspendLayout();
            Dock = DockStyle.Fill;
            Size = new Size(970, 543);
            Font = UiFontManager.GetFont(9F);
            BackColor = Color.FromArgb(248, 250, 252);
            Padding = new Padding(12);

            // ---------------------------------------------------------- header
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 88,
                BackColor = AccentSoft,
                Padding = new Padding(12, 10, 12, 10)
            };
            header.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, header.ClientRectangle, AccentLine, ButtonBorderStyle.Solid);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 0)
            };
            btnExport = ActionButton(true);
            btnExport.Click += async (s, e) => await ExportAsync();
            btnOpenFolder = ActionButton(false);
            btnOpenFolder.Click += (s, e) => OpenSpecsFolder();
            btnCheck = ActionButton(false);
            btnCheck.Click += async (s, e) => await LoadSpecAsync();
            actions.Controls.AddRange(new Control[] { btnExport, btnOpenFolder, btnCheck });

            var headerText = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(4) };
            lblTitle = new Label
            {
                Font = UiFontManager.GetFont(10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(19, 78, 74),
                Dock = DockStyle.Top,
                Height = 26,
                AutoEllipsis = true,
                UseMnemonic = false
            };
            lblSubtitle = new Label
            {
                Font = UiFontManager.GetFont(8.5F),
                ForeColor = Accent,
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                UseMnemonic = false
            };
            headerText.Controls.Add(lblSubtitle);
            headerText.Controls.Add(lblTitle);
            header.Controls.Add(headerText);
            header.Controls.Add(actions);

            // ---------------------------------------------------------- summary cards
            var cards = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 78,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(0, 10, 0, 6),
                BackColor = Color.Transparent
            };
            for (int i = 0; i < 4; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            cards.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            string[] cardKeys = { "cpu", "ram", "disk", "os" };
            for (int i = 0; i < cardKeys.Length; i++)
            {
                cards.Controls.Add(BuildCard(cardKeys[i], i == cardKeys.Length - 1), i, 0);
            }

            // ---------------------------------------------------------- grid
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 32 },
                ColumnHeadersHeight = 36,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(226, 232, 240)
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Ink;
            grid.ColumnHeadersDefaultCellStyle.Font = UiFontManager.GetFont(9F, FontStyle.Bold);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(204, 251, 241);
            grid.DefaultCellStyle.SelectionForeColor = Ink;
            grid.DefaultCellStyle.ForeColor = Ink;
            grid.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCategory", FillWeight = 140, MinimumWidth = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colItem", FillWeight = 170, MinimumWidth = 130 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colValue", FillWeight = 420, MinimumWidth = 200 });
            grid.Columns["colCategory"].DefaultCellStyle.Font = UiFontManager.GetFont(9F, FontStyle.Bold);
            grid.Columns["colCategory"].DefaultCellStyle.ForeColor = Color.FromArgb(19, 78, 74);
            grid.Columns["colItem"].DefaultCellStyle.ForeColor = Muted;

            // ---------------------------------------------------------- bottom status
            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 34, Padding = new Padding(0, 6, 0, 0) };
            progress = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 5,
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Visible = false
            };
            lblStatus = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Muted,
                AutoEllipsis = true,
                UseMnemonic = false,
                TextAlign = ContentAlignment.MiddleLeft
            };
            bottom.Controls.Add(lblStatus);
            bottom.Controls.Add(progress);

            Controls.Add(grid);
            Controls.Add(bottom);
            Controls.Add(cards);
            Controls.Add(header);
            ResumeLayout(true);
        }

        private Control BuildCard(string key, bool last)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, last ? 0 : 10, 0),
                Padding = new Padding(12, 6, 10, 6)
            };
            card.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Border, ButtonBorderStyle.Solid);
                using (var b = new SolidBrush(Accent)) e.Graphics.FillRectangle(b, 0, 0, 4, card.Height);
            };
            var caption = new Label
            {
                Dock = DockStyle.Top,
                Height = 20,
                ForeColor = Muted,
                Font = UiFontManager.GetFont(8.5F),
                UseMnemonic = false
            };
            var value = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Ink,
                Font = UiFontManager.GetFont(9.5F, FontStyle.Bold),
                AutoEllipsis = true,
                UseMnemonic = false,
                Text = "—"
            };
            card.Controls.Add(value);
            card.Controls.Add(caption);
            _cardCaptions[key] = caption;
            _cardValues[key] = value;
            return card;
        }

        private static Button ActionButton(bool primary)
        {
            var b = new Button
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(110, 38),
                Padding = new Padding(8, 0, 8, 0),
                Margin = new Padding(4, 0, 4, 0),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = primary ? Color.FromArgb(16, 185, 129) : Color.White,
                ForeColor = primary ? Color.White : Color.FromArgb(30, 41, 59),
                Font = UiFontManager.GetFont(9F, primary ? FontStyle.Bold : FontStyle.Regular)
            };
            if (primary) b.FlatAppearance.BorderSize = 0;
            else b.FlatAppearance.BorderColor = Border;
            return b;
        }

        public void ApplyLocalization()
        {
            lblTitle.Text = Localizer.T("machine_title");
            lblSubtitle.Text = Localizer.T("machine_subtitle");
            btnCheck.Text = Localizer.T("machine_btn_check");
            btnExport.Text = Localizer.T("machine_btn_export");
            btnOpenFolder.Text = Localizer.T("machine_btn_open_folder");
            _cardCaptions["cpu"].Text = Localizer.T("machine_card_cpu");
            _cardCaptions["ram"].Text = Localizer.T("machine_card_ram");
            _cardCaptions["disk"].Text = Localizer.T("machine_card_disk");
            _cardCaptions["os"].Text = Localizer.T("machine_card_os");
            grid.Columns["colCategory"].HeaderText = Localizer.T("machine_col_category");
            grid.Columns["colItem"].HeaderText = Localizer.T("machine_col_item");
            grid.Columns["colValue"].HeaderText = Localizer.T("machine_col_value");
            if (!_loadedOnce && !_busy) SetStatus(Localizer.T("machine_status_idle"), Muted);
        }

        // ------------------------------------------------------------------ actions

        /// <summary>ເອີ້ນເມື່ອເປີດແທັບ: ໂຫຼດຂໍ້ມູນເທື່ອທຳອິດເທົ່ານັ້ນ (WMI ຊ້າ ບໍ່ດຶງຕອນເປີດໂປຣແກຣມ)</summary>
        public void OnTabActivated()
        {
            if (!_loadedOnce && !_busy)
            {
                var task = LoadSpecAsync();
            }
        }

        private async Task LoadSpecAsync()
        {
            if (_busy) return;
            SetBusy(true, Localizer.T("machine_status_loading"));
            try
            {
                List<SpecRow> rows = await Task.Run(() =>
                {
                    string serial;
                    return _spec.CollectSpec(out serial);
                });
                if (IsDisposed) return;
                ShowRows(rows);
                _loadedOnce = true;
                SetStatus(Localizer.T("machine_status_loaded", DateTime.Now.ToString("HH:mm:ss"), rows.Count), Ok);
            }
            catch (Exception ex)
            {
                ReportService.LogError("MachineInfoPanel.LoadSpecAsync", ex);
                if (!IsDisposed) SetStatus(Localizer.T("machine_status_error", ex.Message), Bad);
            }
            finally
            {
                if (!IsDisposed) SetBusy(false, null);
            }
        }

        private void ShowRows(List<SpecRow> rows)
        {
            grid.SuspendLayout();
            grid.Rows.Clear();
            string lastCategory = null;
            bool shade = false;
            foreach (SpecRow r in rows)
            {
                bool first = r.Category != lastCategory;
                if (first) shade = !shade;
                int i = grid.Rows.Add(first ? r.Category : "", r.Item, r.Value);
                grid.Rows[i].DefaultCellStyle.BackColor = shade ? Color.White : Color.FromArgb(248, 250, 252);
                lastCategory = r.Category;
            }
            grid.ResumeLayout();
            if (grid.Rows.Count > 0) grid.ClearSelection();

            SpecSummary s = SpecSummary.From(rows);
            SetCard("cpu", s.Cpu);
            SetCard("ram", s.Ram);
            SetCard("disk", s.Disk);
            SetCard("os", s.Os);
        }

        private void SetCard(string key, string text)
        {
            Label v = _cardValues[key];
            v.Text = string.IsNullOrWhiteSpace(text) ? "—" : text;
            _toolTip.SetToolTip(v, v.Text); // ຄ່າຍາວ (ເຊັ່ນ ຊື່ CPU) ຖືກຕັດເປັນ ... ຈຶ່ງສະແດງເຕັມໃນ tooltip
        }

        private async Task ExportAsync()
        {
            if (_busy) return;
            SetBusy(true, Localizer.T("machine_status_exporting"));
            CheckResult result = null;
            try
            {
                result = await Task.Run(() => _spec.Fix());
            }
            catch (Exception ex)
            {
                ReportService.LogError("MachineInfoPanel.ExportAsync", ex);
                result = new CheckResult(CheckStatus.Fail, ex.Message);
            }
            finally
            {
                if (!IsDisposed) SetBusy(false, null);
            }
            if (IsDisposed) return;

            _lastExportPath = SpecSummary.FindSavedPath(result);
            Color color = result.Status == CheckStatus.Pass ? Ok : result.Status == CheckStatus.Warning ? Warn : Bad;
            string text = result.Status == CheckStatus.Fail || result.Status == CheckStatus.Unknown
                ? Localizer.T("machine_status_export_failed", result.Summary)
                : result.Summary;
            SetStatus(text, color);
            ReportService.Log("[MachineInfo] Export: " + result.Summary);
        }

        private void OpenSpecsFolder()
        {
            try
            {
                if (!string.IsNullOrEmpty(_lastExportPath) && File.Exists(_lastExportPath))
                {
                    Process.Start("explorer.exe", "/select,\"" + _lastExportPath + "\"");
                    return;
                }
                string dir = Path.GetDirectoryName(_spec.GetTargetExcelPath());
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                Process.Start("explorer.exe", "\"" + dir + "\"");
            }
            catch (Exception ex)
            {
                ReportService.LogError("MachineInfoPanel.OpenSpecsFolder", ex);
                SetStatus(Localizer.T("machine_status_error", ex.Message), Bad);
            }
        }

        private void SetBusy(bool busy, string message)
        {
            _busy = busy;
            btnCheck.Enabled = !busy;
            btnExport.Enabled = !busy;
            progress.Visible = busy;
            UseWaitCursor = busy;
            if (busy && message != null) SetStatus(message, Muted);
        }

        private void SetStatus(string text, Color color)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = color;
        }
    }

    /// <summary>ຂໍ້ມູນສະຫຼຸບສຳລັບບັດ 4 ໃບດ້ານເທິງ (ອ່ານຈາກແຖວ Spec)</summary>
    internal class SpecSummary
    {
        public string Cpu;
        public string Ram;
        public string Disk;
        public string Os;

        /// <summary>SpecExportModule.Fix() ບອກບ່ອນບັນທຶກໃນ Details ເປັນ "Saved to: ..."</summary>
        public static string FindSavedPath(CheckResult result)
        {
            if (result == null || result.Details == null) return null;
            const string prefix = "Saved to: ";
            string line = result.Details.FirstOrDefault(d => d != null && d.StartsWith(prefix, StringComparison.Ordinal));
            return line != null ? line.Substring(prefix.Length).Trim() : null;
        }

        public static SpecSummary From(IList<SpecRow> rows)
        {
            Func<string, string, string> find = (cat, itemStart) =>
            {
                SpecRow r = rows.FirstOrDefault(x => x.Category == cat && x.Item.StartsWith(itemStart, StringComparison.Ordinal));
                return r != null ? r.Value : null;
            };

            string cores = find(SpecRow.Cpu, "Cores");
            string cpu = find(SpecRow.Cpu, "CPU Model");
            if (!string.IsNullOrEmpty(cpu) && !string.IsNullOrEmpty(cores)) cpu += "  ·  " + cores;

            string slots = rows.Count(x => x.Category == SpecRow.Ram && x.Item.StartsWith("RAM Slot", StringComparison.Ordinal)).ToString();
            string ram = find(SpecRow.Ram, "Total");
            if (!string.IsNullOrEmpty(ram) && slots != "0") ram += "  ·  " + slots + " slot";

            string os = find(SpecRow.Os, "OS Name");
            string build = find(SpecRow.Os, "Build");
            if (!string.IsNullOrEmpty(os) && !string.IsNullOrEmpty(build)) os += " (" + build + ")";

            return new SpecSummary
            {
                Cpu = cpu,
                Ram = ram,
                Disk = find(SpecRow.Disk, "Physical Drive 1"),
                Os = os
            };
        }
    }
}
