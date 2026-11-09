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

        public MainForm()
        {
            InitializeComponent();
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
            btnQuickSetup.Text = Localizer.T("btn_quick_setup");
            btnSettings.Text = Localizer.T("btn_settings");

            if (Localizer.CurrentLanguage == "en")
            {
                lblSubtitle.Text = "Classroom Computer Inspection, Wallpaper & Maintenance Tool (Portable)";
            }
            else
            {
                lblSubtitle.Text = "ເຄື່ອງມືກວດສອບ, ຕັ້ງ Wallpaper ແລະ ບຳລຸງຮັກສາຄອມພິວເຕີຫ້ອງຮຽນ (Portable)";
            }
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

        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isChangingLanguage) return;

            string targetLang = cmbLanguage.SelectedIndex == 1 ? "en" : "lo";
            Localizer.SetLanguage(targetLang);
        }
    }
}
