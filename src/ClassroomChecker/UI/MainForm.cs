using System;
using System.Drawing;
using System.Windows.Forms;
using ClassroomChecker.Config;
using ClassroomChecker.I18n;

namespace ClassroomChecker.UI
{
    public partial class MainForm : Form
    {
        private bool _isChangingLanguage = false;
        private FlowLayoutPanel _headerActions;
        private Button _btnAbout;
        private readonly ToolTip _headerToolTip = new ToolTip();

        public MainForm()
        {
            InitializeComponent();
            BuildHeaderLayout();
            try
            {
                using (var stream = typeof(MainForm).Assembly.GetManifestResourceStream("ClassroomChecker.app.ico"))
                {
                    if (stream != null)
                    {
                        this.Icon = new Icon(stream);
                    }
                }
            }
            catch { }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // ນຳໃຊ້ Font Phetsarath OT ທົ່ວທຸກ Controls ໃນ UI
            UiFontManager.ApplyUiFont(this);

            Localizer.LanguageChanged += OnLanguageChanged;

            // ຕັ້ງຄ່າ Dropdown ພາສາຕາມ config
            string currentLang = ConfigService.Instance.Current.language;
            _isChangingLanguage = true;
            if (currentLang != null && currentLang.Equals("en", StringComparison.OrdinalIgnoreCase))
            {
                cmbLanguage.SelectedIndex = 1; // English
            }
            else
            {
                cmbLanguage.SelectedIndex = 0; // Lao
            }
            _isChangingLanguage = false;

            ApplyLocalization();

            // ເມື່ອຄລິກສະຫຼັບມາແທັບ ປ້ອງກັນ, ຕິດຕັ້ງຊອບແວ ຫຼື Antivirus USB ໃຫ້ກວດສອບສະຖານະລະບົບທັນທີ
            this.tabMain.SelectedIndexChanged += (s, args) =>
            {
                if (tabMain.SelectedTab == tabSecurity && securityPanel != null)
                {
                    securityPanel.RefreshPolicyStatus();
                }
                else if (tabMain.SelectedTab == tabInstall && softwareInstallPanel != null)
                {
                    var task = softwareInstallPanel.RefreshGridAsync();
                }
                else if (tabMain.SelectedTab == tabUsbAntivirus && usbAntivirusPanel != null)
                {
                    usbAntivirusPanel.OnTabActivated();
                }
            };
        }

        /// <summary>
        /// ຈັດ header ໃໝ່: ປຸ່ມດ້ານຂວາຢູ່ໃນ FlowLayoutPanel ທີ່ຂະຫຍາຍຕາມຂໍ້ຄວາມ (ບໍ່ໃຫ້ຂໍ້ຄວາມແຕກແຖວ)
        /// ແລະ ຈຳກັດຄວາມກວ້າງ subtitle ບໍ່ໃຫ້ທັບປຸ່ມ (ຕັດເປັນ ... ແລະ ສະແດງເຕັມໃນ tooltip)
        /// </summary>
        private void BuildHeaderLayout()
        {
            pnlHeader.SuspendLayout();

            _headerActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 6, 0, 0),
                Margin = new Padding(0)
            };

            _btnAbout = new Button
            {
                BackColor = btnHelp.BackColor,
                ForeColor = btnHelp.ForeColor,
                FlatStyle = FlatStyle.Flat,
                Font = btnHelp.Font,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            _btnAbout.FlatAppearance.BorderSize = 0;
            _btnAbout.Click += (s, e) =>
            {
                using (var dlg = new AboutDialog()) dlg.ShowDialog(this);
            };

            foreach (var btn in new[] { btnQuickSetup, btnSettings, btnHelp, _btnAbout })
            {
                pnlHeader.Controls.Remove(btn);
                btn.Anchor = AnchorStyles.None;
                btn.AutoSize = true;
                btn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                btn.MinimumSize = new Size(90, 36);
                btn.MaximumSize = new Size(0, 36);
                btn.Padding = new Padding(8, 0, 8, 0);
                btn.Margin = new Padding(4, 0, 4, 0);
                _headerActions.Controls.Add(btn);
            }

            pnlHeader.Controls.Remove(lblLanguage);
            lblLanguage.Anchor = AnchorStyles.None;
            lblLanguage.Margin = new Padding(12, 9, 4, 0);
            _headerActions.Controls.Add(lblLanguage);

            pnlHeader.Controls.Remove(cmbLanguage);
            cmbLanguage.Anchor = AnchorStyles.None;
            cmbLanguage.Width = 150;
            cmbLanguage.Margin = new Padding(0, 5, 0, 0);
            _headerActions.Controls.Add(cmbLanguage);

            pnlHeader.Controls.Add(_headerActions);

            lblSubtitle.AutoSize = false;
            lblSubtitle.AutoEllipsis = true;
            lblSubtitle.UseMnemonic = false; // ໃຫ້ "&" ສະແດງເປັນຕົວອັກສອນ ບໍ່ແມ່ນ shortcut
            lblSubtitle.Height = 22;

            pnlHeader.Resize += (s, e) => LayoutHeaderText();
            _headerActions.SizeChanged += (s, e) => LayoutHeaderText();

            pnlHeader.ResumeLayout(true);
            LayoutHeaderText();

            // ແທັບກວ້າງ 140px ບໍ່ພໍສຳລັບຊື່ພາສາອັງກິດ (ເຊັ່ນ "Protection & Lock")
            tabMain.ItemSize = new Size(170, tabMain.ItemSize.Height);
        }

        private void LayoutHeaderText()
        {
            if (_headerActions == null) return;
            int available = _headerActions.Left - lblSubtitle.Left - 12;
            lblSubtitle.Width = Math.Max(60, available);
        }

        private void OnLanguageChanged()
        {
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            this.Text = Localizer.T("app_title");
            lblLanguage.Text = Localizer.T("language");
            tabWallpaper.Text = Localizer.T("tab_wallpaper");
            tabScan.Text = Localizer.T("tab_scan");
            tabSecurity.Text = Localizer.T("tab_security");
            tabInstall.Text = Localizer.T("tab_install");
            tabUsbAntivirus.Text = Localizer.T("tab_usb_antivirus", "Antivirus USB");
            // header ໃຊ້ຂໍ້ຄວາມສັ້ນ ເພື່ອບໍ່ໃຫ້ກິນພື້ນທີ່ subtitle
            btnQuickSetup.Text = Localizer.T("btn_quick_setup_short");
            btnSettings.Text = Localizer.T("btn_settings");
            btnHelp.Text = Localizer.T("btn_help", "📖 ຄູ່ມື");
            if (_btnAbout != null) _btnAbout.Text = Localizer.T("btn_about");

            if (Localizer.CurrentLanguage == "en")
            {
                lblSubtitle.Text = "Classroom Computer Inspection, Wallpaper & Maintenance Tool (Portable)";
            }
            else
            {
                lblSubtitle.Text = "ເຄື່ອງມືກວດສອບ, ຕັ້ງ Wallpaper ແລະ ບຳລຸງຮັກສາຄອມພິວເຕີຫ້ອງຮຽນ (Portable)";
            }
            _headerToolTip.SetToolTip(lblSubtitle, lblSubtitle.Text);
            LayoutHeaderText();
        }

        private void btnQuickSetup_Click(object sender, EventArgs e)
        {
            var cfg = ConfigService.Instance.Current;
            string room = (cfg != null && cfg.selected != null && !string.IsNullOrEmpty(cfg.selected.room))
                ? cfg.selected.room
                : "ຫ້ອງຄອມພິວເຕີ 1";
            int number = (cfg != null && cfg.selected != null)
                ? cfg.selected.number
                : 1;

            using (var dlg = new QuickSetupDialog(room, number))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    wallpaperPanel.RefreshFromConfig();
                }
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            using (var dlg = new SettingsDialog())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    wallpaperPanel.RefreshFromConfig();
                    securityPanel.LoadFromConfig();
                    securityPanel.RefreshPolicyStatus();
                    MessageBox.Show(
                        Localizer.T("settings_saved_success"),
                        Localizer.T("app_title"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            using (var dlg = new UserGuideDialog())
            {
                dlg.ShowDialog(this);
            }
        }

        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isChangingLanguage) return;

            string targetLang = cmbLanguage.SelectedIndex == 1 ? "en" : "lo";
            Localizer.SetLanguage(targetLang);
        }
    }
}
