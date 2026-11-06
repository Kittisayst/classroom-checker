using System;
using System.Drawing;
using System.Windows.Forms;
using ClassroomChecker.Core;
using ClassroomChecker.I18n;

namespace ClassroomChecker.UI
{
    public class DetailsDialog : Form
    {
        private TextBox txtDetails;
        private Label lblHeader;
        private Button btnClose;

        public DetailsDialog(string moduleName, CheckResult result)
        {
            InitializeComponent(moduleName, result);
        }

        private void InitializeComponent(string moduleName, CheckResult result)
        {
            this.Text = Localizer.T("details_dialog_title", moduleName);
            this.Size = new Size(580, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.Font = UiFontManager.GetFont(9F, FontStyle.Regular);

            lblHeader = new Label
            {
                Dock = DockStyle.Top,
                Height = 45,
                Padding = new Padding(12, 10, 12, 0),
                Font = UiFontManager.GetFont(10F, FontStyle.Bold),
                Text = string.Format("{0} — Status: {1}", moduleName, result.Status)
            };

            switch (result.Status)
            {
                case CheckStatus.Pass:
                    lblHeader.ForeColor = Color.FromArgb(46, 125, 50);
                    break;
                case CheckStatus.Fail:
                    lblHeader.ForeColor = Color.FromArgb(198, 40, 40);
                    break;
                case CheckStatus.Warning:
                    lblHeader.ForeColor = Color.FromArgb(239, 108, 0);
                    break;
                default:
                    lblHeader.ForeColor = Color.FromArgb(71, 85, 105);
                    break;
            }

            txtDetails = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                Font = new Font("Consolas", 9.5F, FontStyle.Regular)
            };

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Summary: " + result.Summary);
            sb.AppendLine(new string('-', 50));
            sb.AppendLine("Details:");
            if (result.Details != null && result.Details.Count > 0)
            {
                foreach (var line in result.Details)
                {
                    sb.AppendLine(" • " + line);
                }
            }
            else
            {
                sb.AppendLine(" (ບໍ່ມີລາຍລະອຽດເພີ່ມເຕີມ)");
            }
            txtDetails.Text = sb.ToString();

            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                Padding = new Padding(12, 8, 12, 8)
            };

            btnClose = new Button
            {
                Text = "Close",
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(470, 8),
                Size = new Size(85, 30)
            };
            pnlBottom.Controls.Add(btnClose);

            this.Controls.Add(txtDetails);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(lblHeader);

            this.CancelButton = btnClose;
        }
    }
}
