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
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
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

            if (Localizer.CurrentLanguage == "en")
            {
                lblSubtitle.Text = "Classroom Computer Inspection, Wallpaper & Maintenance Tool (Portable)";
            }
            else
            {
                lblSubtitle.Text = "ເຄື່ອງມືກວດສອບ, ຕັ້ງ Wallpaper ແລະ ບຳລຸງຮັກສາຄອມພິວເຕີຫ້ອງຮຽນ (Portable)";
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
