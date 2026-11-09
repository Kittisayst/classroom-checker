using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Media;
using System.Windows.Forms;
using ClassroomChecker.Config;
using ClassroomChecker.Core;
using ClassroomChecker.I18n;

namespace ClassroomChecker.UI
{
    public class QuickSetupDialog : Form
    {
        private readonly string _room;
        private readonly int _number;
        private readonly QuickSetupRunner _runner;

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblRoomBadge;
        private Panel pnlSteps;
        private Label[] lblStepItems;
        private Label[] lblStepIcons;
        private ProgressBar prgSetup;
        private Label lblStatusMessage;
        private Panel pnlBottom;
        private Button btnStart;
        private Button btnClose;
        private CheckBox chkAutoClose;
        private Timer autoCloseTimer;
        private int autoCloseCountdown = 3;

        public event Action SetupCompleted;

        public QuickSetupDialog(string room, int number)
        {
            _room = room;
            _number = number;
            _runner = new QuickSetupRunner();

            InitializeComponent();
            UiFontManager.ApplyUiFont(this);
        }

        private void InitializeComponent()
        {
            this.Text = Localizer.T("quick_setup_title");
            this.Size = new Size(580, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.BackColor = Color.FromArgb(248, 250, 252);

            try
            {
                using (var stream = typeof(QuickSetupDialog).Assembly.GetManifestResourceStream("ClassroomChecker.app.ico"))
                {
                    if (stream != null) this.Icon = new Icon(stream);
                }
            }
            catch { }

            // 1. Header Panel
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.FromArgb(16, 185, 129), // Emerald 500
                Padding = new Padding(20, 14, 20, 14)
            };

            lblTitle = new Label
            {
                Text = "⚡ " + Localizer.T("quick_setup_title"),
                Font = UiFontManager.GetFont(13F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(18, 14),
                AutoSize = true
            };

            lblRoomBadge = new Label
            {
                Text = string.Format("ຫ້ອງ: {0}  |  ເລກເຄື່ອງ: {1:D2}  |  Host: {2}", _room, _number, Environment.MachineName),
                Font = UiFontManager.GetFont(9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(236, 253, 245),
                Location = new Point(20, 48),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblRoomBadge);

            // 2. Steps Panel
            pnlSteps = new Panel
            {
                Location = new Point(20, 100),
                Size = new Size(525, 255),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(16, 12, 16, 12)
            };

            string[] stepTitles = new string[]
            {
                string.Format("1. ຕັ້ງ Wallpaper Desktop (ຫ້ອງ {0} - ເຄື່ອງ {1:D2}) ດ້ວຍ Font Phetsarath OT", _room, _number),
                "2. ຕັ້ງຄ່າ Power Scheme ເປັນ Never Sleep (AC & DC = 0)",
                "3. ກວດສອບ ແລະ ຕິດຕັ້ງ Font Phetsarath OT (ຖ້າຂາດ)",
                "4. ກວດສອບ ແລະ ຕິດຕັ້ງ ແປ້ນພິມພາສາລາວ (0454:00000454)",
                "5. ລ້າງໄຟລ໌ຂີ້ເຫຍື້ອ Temp/Cache ປອດໄພ",
                string.Format("6. Export Spec ເຄື່ອງເປັນ Excel (.xlsx) ລົງໂຟນເດີ specs/")
            };

            lblStepItems = new Label[6];
            lblStepIcons = new Label[6];

            for (int i = 0; i < 6; i++)
            {
                int y = 14 + (i * 38);

                lblStepIcons[i] = new Label
                {
                    Text = "○",
                    Font = UiFontManager.GetFont(11F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(148, 163, 184), // Slate 400
                    Location = new Point(16, y - 2),
                    Size = new Size(24, 26),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                lblStepItems[i] = new Label
                {
                    Text = stepTitles[i],
                    Font = UiFontManager.GetFont(9.5F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(71, 85, 105), // Slate 600
                    Location = new Point(44, y),
                    Size = new Size(460, 30)
                };

                pnlSteps.Controls.Add(lblStepIcons[i]);
                pnlSteps.Controls.Add(lblStepItems[i]);
            }

            // 3. Progress Bar & Status Message
            prgSetup = new ProgressBar
            {
                Location = new Point(20, 368),
                Size = new Size(525, 20),
                Minimum = 0,
                Maximum = 6,
                Value = 0
            };

            lblStatusMessage = new Label
            {
                Text = "ກົດ \"ເລີ່ມຕົ້ນຕັ້ງຄ່າ\" ເພື່ອດຳເນີນການທຸກຂັ້ນຕອນອັດຕະໂນມັດ",
                Font = UiFontManager.GetFont(9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(20, 394),
                Size = new Size(525, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // 4. Bottom Panel
            pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(20, 12, 20, 12)
            };

            chkAutoClose = new CheckBox
            {
                Text = "ປິດໜ້າຕ່າງອັດຕະໂນມັດເມື່ອສຳເລັດ",
                Font = UiFontManager.GetFont(9F, FontStyle.Regular),
                Location = new Point(20, 18),
                Size = new Size(240, 26),
                Checked = true
            };

            btnStart = new Button
            {
                Text = "⚡ ເລີ່ມຕົ້ນຕັ້ງຄ່າ",
                Font = UiFontManager.GetFont(9.5F, FontStyle.Bold),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(275, 14),
                Size = new Size(140, 36),
                Cursor = Cursors.Hand
            };
            btnStart.Click += BtnStart_Click;

            btnClose = new Button
            {
                Text = "ປິດ (Close)",
                Font = UiFontManager.GetFont(9.5F, FontStyle.Regular),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(425, 14),
                Size = new Size(120, 36),
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.Close();

            pnlBottom.Controls.Add(chkAutoClose);
            pnlBottom.Controls.Add(btnStart);
            pnlBottom.Controls.Add(btnClose);

            this.Controls.Add(pnlSteps);
            this.Controls.Add(prgSetup);
            this.Controls.Add(lblStatusMessage);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlHeader);

            // Hook runner progress
            _runner.StepProgress += Runner_StepProgress;

            // Timer for countdown auto-close
            autoCloseTimer = new Timer { Interval = 1000 };
            autoCloseTimer.Tick += AutoCloseTimer_Tick;
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnClose.Enabled = false;
            lblStatusMessage.Text = "ກຳລັງຕັ້ງຄ່າລະບົບ...";
            lblStatusMessage.ForeColor = Color.FromArgb(37, 99, 235);

            bool success = await _runner.RunSetupAsync(_room, _number);

            try
            {
                SystemSounds.Asterisk.Play();
            }
            catch { }

            int nextNum = _runner.NextNumber > 0 ? _runner.NextNumber : _number + 1;
            bool isMaxReached = _runner.IsMaxReached;

            if (success)
            {
                if (isMaxReached)
                {
                    lblStatusMessage.Text = string.Format("✓ ສຳເລັດທຸກຂັ້ນຕອນ! ⚠ ຮອດເລກເຄື່ອງສູງສຸດແລ້ວ ({0:D2})", nextNum);
                    lblStatusMessage.ForeColor = Color.FromArgb(22, 163, 74);
                }
                else
                {
                    lblStatusMessage.Text = string.Format("✓ ສຳເລັດທຸກຂັ້ນຕອນ! ເລກເຄື່ອງຖືກປັບເປັນ {0:D2} ແລ້ວ (ພ້ອມສຽບເຄື່ອງຕໍ່ໄປ)", nextNum);
                    lblStatusMessage.ForeColor = Color.FromArgb(22, 163, 74);
                }
            }
            else
            {
                lblStatusMessage.Text = "ຕັ້ງຄ່າສຳເລັດບາງສ່ວນ (ມີບາງຂໍ້ຜິດພາດ ກະລຸນາກວດສອບ)";
                lblStatusMessage.ForeColor = Color.FromArgb(217, 119, 6);
            }

            btnClose.Enabled = true;
            btnClose.Focus();

            var handler = SetupCompleted;
            if (handler != null) handler();

            // ຖ້າຮອດເລກສູງສຸດແລ້ວ ຢ່າ auto-close ເພື່ອໃຫ້ Admin ຮັບຮູ້ວ່າສິ້ນສຸດຫ້ອງແລ້ວ
            if (chkAutoClose.Checked && !isMaxReached)
            {
                autoCloseCountdown = 2;
                autoCloseTimer.Start();
            }
        }

        private void AutoCloseTimer_Tick(object sender, EventArgs e)
        {
            autoCloseCountdown--;
            if (autoCloseCountdown <= 0)
            {
                autoCloseTimer.Stop();
                this.Close();
            }
            else
            {
                btnClose.Text = string.Format("ປິດ ({0}s)", autoCloseCountdown);
            }
        }

        private void Runner_StepProgress(object sender, QuickSetupStepEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => Runner_StepProgress(sender, e)));
                return;
            }

            int idx = e.StepIndex - 1;
            if (idx >= 0 && idx < 6)
            {
                if (e.IsCompleted)
                {
                    if (e.IsSuccess)
                    {
                        lblStepIcons[idx].Text = "✓";
                        lblStepIcons[idx].ForeColor = Color.FromArgb(22, 163, 74); // Green
                        lblStepItems[idx].ForeColor = Color.FromArgb(15, 23, 42);
                        lblStepItems[idx].Font = UiFontManager.GetFont(9.5F, FontStyle.Bold);
                    }
                    else
                    {
                        lblStepIcons[idx].Text = "✗";
                        lblStepIcons[idx].ForeColor = Color.FromArgb(220, 38, 38); // Red
                        lblStepItems[idx].ForeColor = Color.FromArgb(220, 38, 38);
                    }
                }
                else
                {
                    lblStepIcons[idx].Text = "▶";
                    lblStepIcons[idx].ForeColor = Color.FromArgb(37, 99, 235); // Blue
                    lblStepItems[idx].ForeColor = Color.FromArgb(37, 99, 235);
                    lblStatusMessage.Text = "ກຳລັງເຮັດ: " + e.StepName + "...";
                }
            }

            prgSetup.Value = Math.Min(e.StepIndex, 6);
        }
    }
}
