using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ClassroomChecker.Config;
using ClassroomChecker.Core;
using ClassroomChecker.I18n;
using ClassroomChecker.Modules;

namespace ClassroomChecker.UI
{
    public partial class ScanPanel : UserControl
    {
        private readonly List<ICheckModule> _modules = new List<ICheckModule>();
        private readonly Dictionary<ICheckModule, CheckResult> _results = new Dictionary<ICheckModule, CheckResult>();
        private readonly ScanRunner _runner = new ScanRunner();

        public ScanPanel()
        {
            InitializeComponent();
            RegisterModules();
        }

        private void RegisterModules()
        {
            _modules.Clear();
            _modules.Add(new OfficeCheckModule());
            _modules.Add(new WindowsLicenseCheckModule());
            _modules.Add(new EssentialSoftwareCheckModule());
            _modules.Add(new FontCheckModule());
            _modules.Add(new KeyboardCheckModule());
            _modules.Add(new PowerModule());
            _modules.Add(new CleanupModule());
            _modules.Add(new DriverCheckModule());
            _modules.Add(new SpecExportModule());
        }

        private void ScanPanel_Load(object sender, EventArgs e)
        {
            Localizer.LanguageChanged += OnLanguageChanged;

            _runner.ModuleStarted += OnModuleStarted;
            _runner.ModuleCompleted += OnModuleCompleted;

            PopulateGrid();
            ApplyLocalization();
        }

        private void PopulateGrid()
        {
            dgvResults.Rows.Clear();
            foreach (var mod in _modules)
            {
                int rowIndex = dgvResults.Rows.Add();
                DataGridViewRow row = dgvResults.Rows[rowIndex];

                row.Tag = mod;
                row.Cells[colCheck.Index].Value = true;
                row.Cells[colStatus.Index].Value = "-";
                row.Cells[colName.Index].Value = Localizer.T(mod.DisplayNameKey);
                row.Cells[colSummary.Index].Value = Localizer.T("status_ready");
                row.Cells[colDetails.Index].Value = Localizer.T("btn_view_details");
            }
        }

        public void ApplyLocalization()
        {
            btnScan.Text = Localizer.T("btn_scan");
            btnFixSelected.Text = Localizer.T("btn_fix_selected");
            chkSelectAll.Text = Localizer.T("col_check") + " " + (Localizer.CurrentLanguage == "lo" ? "ທັງໝົດ" : "All");
            btnSaveReport.Text = Localizer.T("btn_save_report", "ບັນທຶກລາຍງານ (.txt)");
            btnMergeSpecs.Text = Localizer.T("btn_merge_specs", "📊 ລວມສະເປັກ (Master Excel)");

            colCheck.HeaderText = "✓";
            colStatus.HeaderText = Localizer.T("col_status");
            colName.HeaderText = Localizer.T("col_name");
            colSummary.HeaderText = Localizer.T("col_summary");
            colDetails.HeaderText = Localizer.T("btn_view_details");

            // ອັບເດດຊື່ module ໃນ grid
            foreach (DataGridViewRow row in dgvResults.Rows)
            {
                var mod = row.Tag as ICheckModule;
                if (mod != null)
                {
                    row.Cells[colName.Index].Value = Localizer.T(mod.DisplayNameKey);
                    row.Cells[colDetails.Index].Value = Localizer.T("btn_view_details");

                    CheckResult res;
                    if (_results.TryGetValue(mod, out res))
                    {
                        UpdateRowStatusVisual(row, res.Status);
                    }
                }
            }

            UpdateSummaryLabel();
        }

        private void OnLanguageChanged()
        {
            ApplyLocalization();
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = chkSelectAll.Checked;
            foreach (DataGridViewRow row in dgvResults.Rows)
            {
                row.Cells[colCheck.Index].Value = isChecked;
            }
        }

        private async void btnScan_Click(object sender, EventArgs e)
        {
            if (_runner.IsRunning) return;

            var selectedModules = new List<ICheckModule>();
            foreach (DataGridViewRow row in dgvResults.Rows)
            {
                bool isChecked = Convert.ToBoolean(row.Cells[colCheck.Index].Value);
                if (isChecked && row.Tag is ICheckModule)
                {
                    selectedModules.Add((ICheckModule)row.Tag);
                }
            }

            if (selectedModules.Count == 0)
            {
                MessageBox.Show("ກະລຸນາເລືອກຢ່າງໜ້ອຍ 1 ລາຍການເພື່ອກວດສອບ", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetUiScanningState(true, selectedModules.Count);

            try
            {
                await _runner.RunCheckAsync(selectedModules);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ເກີດຂໍ້ຜິດພາດ: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetUiScanningState(false, 0);
                UpdateSummaryLabel();
                CheckFixButtonState();
            }
        }

        private async void btnFixSelected_Click(object sender, EventArgs e)
        {
            if (_runner.IsRunning) return;

            var fixableModules = new List<ICheckModule>();
            foreach (DataGridViewRow row in dgvResults.Rows)
            {
                bool isChecked = Convert.ToBoolean(row.Cells[colCheck.Index].Value);
                var mod = row.Tag as ICheckModule;
                if (isChecked && mod != null && mod.CanFix)
                {
                    CheckResult lastRes;
                    if (_results.TryGetValue(mod, out lastRes) && lastRes.Status != CheckStatus.Pass)
                    {
                        fixableModules.Add(mod);
                    }
                }
            }

            if (fixableModules.Count == 0) return;

            SetUiScanningState(true, fixableModules.Count);

            try
            {
                await _runner.RunFixAsync(fixableModules);

                // ຫຼັງແກ້ → ສະແກນຊ້ຳອັດຕະໂນມັດເພື່ອຢືນຢັນ (ຕາມ spec 5.7)
                await _runner.RunCheckAsync(fixableModules);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ເກີດຂໍ້ຜິດພາດໃນການແກ້ໄຂ: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetUiScanningState(false, 0);
                UpdateSummaryLabel();
                CheckFixButtonState();
            }
        }

        private void SetUiScanningState(bool scanning, int total)
        {
            btnScan.Enabled = !scanning;
            btnFixSelected.Enabled = !scanning;
            chkSelectAll.Enabled = !scanning;

            if (scanning)
            {
                progressBar.Value = 0;
                progressBar.Maximum = total;
                progressBar.Visible = true;
            }
            else
            {
                progressBar.Visible = false;
                lblProgress.Text = Localizer.T("scan_completed");
            }
        }

        private void OnModuleStarted(ICheckModule mod, int index, int total)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnModuleStarted(mod, index, total)));
                return;
            }

            lblProgress.Text = Localizer.T("scan_in_progress", Localizer.T(mod.DisplayNameKey), index, total);
            progressBar.Value = Math.Min(index, progressBar.Maximum);

            var row = FindRowForModule(mod);
            if (row != null)
            {
                row.Cells[colStatus.Index].Value = "...";
                row.Cells[colSummary.Index].Value = "Scanning...";
            }
        }

        private void OnModuleCompleted(ICheckModule mod, CheckResult res, int index, int total)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnModuleCompleted(mod, res, index, total)));
                return;
            }

            _results[mod] = res;

            var row = FindRowForModule(mod);
            if (row != null)
            {
                UpdateRowStatusVisual(row, res.Status);
                row.Cells[colSummary.Index].Value = res.Summary;
            }
        }

        private void UpdateRowStatusVisual(DataGridViewRow row, CheckStatus status)
        {
            switch (status)
            {
                case CheckStatus.Pass:
                    row.Cells[colStatus.Index].Value = "✓ " + Localizer.T("status_pass");
                    row.Cells[colStatus.Index].Style.ForeColor = Color.FromArgb(22, 163, 74); // Green
                    row.Cells[colStatus.Index].Style.Font = UiFontManager.GetFont(9.5F, FontStyle.Bold);
                    break;
                case CheckStatus.Fail:
                    row.Cells[colStatus.Index].Value = "✗ " + Localizer.T("status_fail");
                    row.Cells[colStatus.Index].Style.ForeColor = Color.FromArgb(220, 38, 38); // Red
                    row.Cells[colStatus.Index].Style.Font = UiFontManager.GetFont(9.5F, FontStyle.Bold);
                    break;
                case CheckStatus.Warning:
                    row.Cells[colStatus.Index].Value = "⚠ " + Localizer.T("status_warning");
                    row.Cells[colStatus.Index].Style.ForeColor = Color.FromArgb(217, 119, 6); // Amber
                    row.Cells[colStatus.Index].Style.Font = UiFontManager.GetFont(9.5F, FontStyle.Bold);
                    break;
                default:
                    row.Cells[colStatus.Index].Value = "? " + Localizer.T("status_unknown");
                    row.Cells[colStatus.Index].Style.ForeColor = Color.FromArgb(100, 116, 139); // Gray
                    row.Cells[colStatus.Index].Style.Font = UiFontManager.GetFont(9.5F, FontStyle.Regular);
                    break;
            }
        }

        private DataGridViewRow FindRowForModule(ICheckModule mod)
        {
            foreach (DataGridViewRow row in dgvResults.Rows)
            {
                if (row.Tag == mod) return row;
            }
            return null;
        }

        private void UpdateSummaryLabel()
        {
            if (_results.Count == 0)
            {
                lblSummary.Text = Localizer.T("status_ready");
                lblSummary.ForeColor = Color.FromArgb(30, 41, 59);
                return;
            }

            int passCount = 0;
            int total = _results.Count;

            foreach (var kv in _results)
            {
                if (kv.Value.Status == CheckStatus.Pass) passCount++;
            }

            lblSummary.Text = Localizer.T("scan_summary_passed", passCount, total);
            if (passCount == total)
            {
                lblSummary.ForeColor = Color.FromArgb(22, 163, 74);
            }
            else
            {
                lblSummary.ForeColor = Color.FromArgb(220, 38, 38);
            }
        }

        private void CheckFixButtonState()
        {
            bool hasFixable = false;
            foreach (DataGridViewRow row in dgvResults.Rows)
            {
                var mod = row.Tag as ICheckModule;
                if (mod != null && mod.CanFix)
                {
                    CheckResult res;
                    if (_results.TryGetValue(mod, out res) && res.Status != CheckStatus.Pass)
                    {
                        hasFixable = true;
                        break;
                    }
                }
            }
            btnFixSelected.Enabled = hasFixable;
        }

        private void dgvResults_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == colDetails.Index)
            {
                DataGridViewRow row = dgvResults.Rows[e.RowIndex];
                var mod = row.Tag as ICheckModule;
                if (mod != null)
                {
                    CheckResult res;
                    if (!_results.TryGetValue(mod, out res))
                    {
                        res = new CheckResult(CheckStatus.Unknown, "ຍັງບໍ່ໄດ້ຮັບການສະແກນ (Not scanned yet)");
                    }

                    using (DetailsDialog dlg = new DetailsDialog(Localizer.T(mod.DisplayNameKey), res))
                    {
                        dlg.ShowDialog(this);
                    }
                }
            }
        }

        private void btnSaveReport_Click(object sender, EventArgs e)
        {
            if (_results.Count == 0)
            {
                MessageBox.Show("ກະລຸນາສະແກນກ່ອນບັນທຶກລາຍງານ", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text File (*.txt)|*.txt";
                sfd.FileName = string.Format("ScanReport_{0}_{1}.txt", Environment.MachineName, DateTime.Now.ToString("yyyyMMdd_HHmm"));

                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("=======================================================================");
                        sb.AppendLine("   CLASSROOM CHECKER — SYSTEM SCAN REPORT");
                        sb.AppendLine("=======================================================================");
                        sb.AppendLine("Machine: " + Environment.MachineName);
                        sb.AppendLine("User:    " + Environment.UserName);
                        sb.AppendLine("Date:    " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        sb.AppendLine("-----------------------------------------------------------------------");

                        foreach (var kv in _results)
                        {
                            sb.AppendLine();
                            sb.AppendLine(string.Format("[{0}] {1}", kv.Value.Status.ToString().ToUpper(), Localizer.T(kv.Key.DisplayNameKey)));
                            sb.AppendLine("Summary: " + kv.Value.Summary);
                            if (kv.Value.Details != null && kv.Value.Details.Count > 0)
                            {
                                foreach (var d in kv.Value.Details)
                                {
                                    sb.AppendLine("  • " + d);
                                }
                            }
                        }

                        sb.AppendLine();
                        sb.AppendLine("=======================================================================");
                        sb.AppendLine(lblSummary.Text);
                        sb.AppendLine("=======================================================================");

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("ບັນທຶກລາຍງານຮຽບຮ້ອຍແລ້ວ!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ເກີດຂໍ້ຜິດພາດໃນການບັນທຶກ: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnMergeSpecs_Click(object sender, EventArgs e)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string specsDir = Path.Combine(baseDir, "specs");

                if (!Directory.Exists(specsDir))
                {
                    MessageBox.Show(
                        Localizer.T("merge_no_files", "ບໍ່ພົບໂຟນເດີ specs ຫຼື ບໍ່ມີໄຟລ໌ສະເປັກທີ່ຈະລວມ"),
                        "Notice",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                var cfg = ConfigService.Instance.Current;
                string room = (cfg != null && cfg.selected != null && !string.IsNullOrEmpty(cfg.selected.room))
                    ? cfg.selected.room
                    : "Lab";

                string masterPath = MasterSummaryService.GenerateMasterSummary(specsDir, room);

                if (string.IsNullOrEmpty(masterPath) || !File.Exists(masterPath))
                {
                    MessageBox.Show(
                        Localizer.T("merge_no_files", "ບໍ່ພົບໄຟລ໌ສະເປັກທີ່ຈະລວມ"),
                        "Notice",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                var res = MessageBox.Show(
                    Localizer.T("merge_success", "ລວມສະເປັກສຳເລັດແລ້ວ! ບັນທຶກໄວ້ທີ່:\n{0}\n\nຕ້ອງການເປີດໄຟລ໌ດຽວນີ້ບໍ່?", masterPath),
                    Localizer.T("app_title"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (res == DialogResult.Yes)
                {
                    Process.Start(masterPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Localizer.T("merge_error", "ເກີດຂໍ້ຜິດພາດໃນການລວມສະເປັກ:\n{0}", ex.Message),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
