using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ClassroomChecker.I18n;

namespace ClassroomChecker.UI
{
    /// <summary>
    /// ໜ້າຕ່າງຄູ່ມືການນຳໃຊ້ລະບົບ Classroom Checker (User Guide & Manual)
    /// ອະທິບາຍລາຍລະອຽດການໃຊ້ງານທຸກແທັບ, ທຸກປຸ່ມ ແລະ ໂຄງສ້າງລະບົບຢ່າງລະອຽດ
    /// </summary>
    public class UserGuideDialog : Form
    {
        private TabControl tabGuide;
        private TabPage tabQuickSetup;
        private TabPage tabWallpaper;
        private TabPage tabScan;
        private TabPage tabSecurity;
        private TabPage tabInstall;
        private TabPage tabUsbAntivirus;
        private TabPage tabDateTimeIcons;
        private TabPage tabPerformance;
        private TabPage tabStructure;

        private Panel pnlHeader;
        private Label lblHeaderTitle;
        private Label lblHeaderSubtitle;
        private Panel pnlFooter;
        private Button btnOpenReadme;
        private Button btnClose;

        public UserGuideDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = Localizer.T("guide_dialog_title", "📖 ຄູ່ມືການນຳໃຊ້ລະບົບ Classroom Checker (User Guide)");
            this.Size = new Size(920, 640);
            this.MinimumSize = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = UiFontManager.GetFont(9F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(248, 250, 252);

            try
            {
                using (Stream stream = typeof(UserGuideDialog).Assembly.GetManifestResourceStream("ClassroomChecker.app.ico"))
                {
                    if (stream != null) this.Icon = new Icon(stream);
                }
            }
            catch { }

            // -------------------------------------------------------------
            // Header
            // -------------------------------------------------------------
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Color.FromArgb(26, 115, 232),
                Padding = new Padding(16, 10, 16, 10)
            };

            lblHeaderTitle = new Label
            {
                Text = Localizer.T("guide_header_title", "📖 ຄູ່ມືການນຳໃຊ້ Classroom Checker v1.1"),
                Font = UiFontManager.GetFont(12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(16, 10),
                Size = new Size(700, 26),
                AutoEllipsis = true
            };

            lblHeaderSubtitle = new Label
            {
                Text = Localizer.T("guide_header_subtitle", "ເຄື່ອງມື Admin ແບບ Portable ສຳລັບກວດສອບ, ຕັ້ງ Wallpaper, ລັອກຄວາມປອດໄພ ແລະ ຕິດຕັ້ງຊອບແວຫ້ອງຮຽນ"),
                Font = UiFontManager.GetFont(9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(230, 240, 255),
                Location = new Point(17, 36),
                Size = new Size(850, 22),
                AutoEllipsis = true
            };

            pnlHeader.Controls.AddRange(new Control[] { lblHeaderTitle, lblHeaderSubtitle });

            // -------------------------------------------------------------
            // Footer
            // -------------------------------------------------------------
            pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(16, 8, 16, 8)
            };

            btnOpenReadme = new Button
            {
                Text = Localizer.T("guide_btn_open_readme", "📄 ເປີດໄຟລ໌ README.md"),
                Dock = DockStyle.Left,
                Width = 180,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Font = UiFontManager.GetFont(9F, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnOpenReadme.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnOpenReadme.Click += (s, e) =>
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string readme = Path.Combine(baseDir, "README.md");
                if (!File.Exists(readme)) readme = Path.Combine(baseDir, @"..\..\README.md");
                if (File.Exists(readme))
                {
                    try { System.Diagnostics.Process.Start(readme); } catch { }
                }
                else
                {
                    MessageBox.Show("ບໍ່ພົບໄຟລ໌ README.md ໃນໂຟນເດີໂປຣແກຣມ", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            btnClose = new Button
            {
                Text = Localizer.T("btn_close", "ປິດໜ້າຕ່າງ"),
                Dock = DockStyle.Right,
                Width = 120,
                BackColor = Color.FromArgb(71, 85, 105),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = UiFontManager.GetFont(9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            pnlFooter.Controls.AddRange(new Control[] { btnOpenReadme, btnClose });

            // -------------------------------------------------------------
            // TabControl for Manual Categories
            // -------------------------------------------------------------
            tabGuide = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = UiFontManager.GetFont(9F, FontStyle.Regular),
                Padding = new Point(14, 8)
            };

            tabQuickSetup = CreateGuideTab("⚡ ຕັ້ງຄ່າດ່ວນ", GetQuickSetupContent());
            tabWallpaper = CreateGuideTab("🖼️ ພື້ນຫຼັງ & ເລກເຄື່ອງ", GetWallpaperContent());
            tabScan = CreateGuideTab("🔍 ກວດສອບລະບົບ", GetScanContent());
            tabSecurity = CreateGuideTab("🛡️ ປ້ອງກັນ & ລັອກ", GetSecurityContent());
            tabInstall = CreateGuideTab("📦 ຕິດຕັ້ງຊອບແວ", GetInstallContent());
            tabUsbAntivirus = CreateGuideTab("🦠 Antivirus USB", GetUsbAntivirusContent());
            tabDateTimeIcons = CreateGuideTab("⚙️ ວັນທີ & ໄອຄອນໜ້າຈໍ", GetDateTimeIconsContent());
            tabPerformance = CreateGuideTab("⚡ ເຄື່ອງສະເປັກຕ່ຳ", GetPerformanceContent());
            tabStructure = CreateGuideTab("📁 ໂຄງສ້າງ USB", GetStructureContent());

            tabGuide.TabPages.AddRange(new TabPage[] {
                tabQuickSetup,
                tabWallpaper,
                tabScan,
                tabSecurity,
                tabInstall,
                tabUsbAntivirus,
                tabDateTimeIcons,
                tabPerformance,
                tabStructure
            });

            this.Controls.Add(tabGuide);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlHeader);

            this.ResumeLayout(false);
        }

        private TabPage CreateGuideTab(string title, string content)
        {
            var page = new TabPage(title);
            page.Padding = new Padding(12);
            page.BackColor = Color.White;

            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                BackColor = Color.White,
                Font = UiFontManager.GetFont(9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(15, 23, 42),
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Text = content
            };

            page.Controls.Add(rtb);
            return page;
        }

        private string GetQuickSetupContent()
        {
            bool isLo = Localizer.CurrentLanguage == "lo";
            if (!isLo)
            {
                return @"================================================================================
⚡ 1. QUICK SETUP WORKFLOW (ONE-CLICK COMPUTER PROVISIONING)
================================================================================

The '⚡ Quick Setup' button located in the top-right header allows a teacher or system administrator to provision and configure a classroom PC in a matter of seconds.

--------------------------------------------------------------------------------
HOW TO USE QUICK SETUP:
--------------------------------------------------------------------------------
1. Insert your USB Flash Drive into the student computer.
2. Launch ClassroomChecker.exe (runs portably as Administrator).
3. Click the green '⚡ Quick Setup' button in the top header.
4. Review the selected Classroom Room and Computer Number.
   • The computer number automatically auto-increments (e.g. 01 -> 02 -> 03...)
     when moved from computer to computer!
5. Click '⚡ Start Quick Setup'.
6. The tool then runs 11 steps automatically:
   ✔ Step 1: Apply the desktop wallpaper with the Room & PC Number label
             (plus the lock/sign-in screen image and logon notice, if enabled in Settings).
   ✔ Step 2: Set Power Scheme to 'Never Sleep' (prevents machines sleeping during class).
   ✔ Step 3: Check and register the national Lao font (Phetsarath OT).
   ✔ Step 4: Check and register Lao Keyboard Layout (0454).
   ✔ Step 5: Set Timezone (+7), date format and 12/24-hour time (from Settings).
   ✔ Step 6: Show This PC / Control Panel and create Office shortcuts (existing ones are not duplicated).
   ✔ Step 7: Clean safe temporary files, user temp, and system cache.
   ✔ Step 8: Export full machine hardware & software specs to an Excel (.xlsx) file.
   ✔ Step 9: Disable Windows AutoRun & vaccinate against USB shortcut viruses.
   ✔ Step 10: Low-spec mode (only on PCs with RAM <= 4 GB when mode is 'Auto').
   ✔ Step 11: Rename the computer (e.g. COM1-01) and the signed-in user (e.g. USER-COM1-01).
7. Restart the PC when asked so the new name takes effect.
8. Unplug the USB drive and plug it into the next PC — the computer number is already ready for the next machine!

💡 PRO-TIP: Room list, starting number, user name prefix, date/time format and low-spec mode are all in '⚙️ Settings'.";
            }

            return @"================================================================================
⚡ 1. ຂັ້ນຕອນການໃຊ້ງານ 'ຕັ້ງຄ່າດ່ວນ' (QUICK SETUP) ໃນຄລິກດຽວ
================================================================================

ປຸ່ມ '⚡ ຕັ້ງຄ່າດ່ວນ' (ສີຂຽວຢູ່ມຸມຂວາເທິງຂອງໜ້າຈໍ) ອອກແບບມາສຳລັບຄູ-ອາຈານ ຫຼື Admin ທີ່ຕ້ອງການຕັ້ງຄ່າຄອມພິວເຕີຫ້ອງຮຽນຫຼາຍໆ ເຄື່ອງໃຫ້ໄວທີ່ສຸດ ໂດຍບໍ່ຕ້ອງກົດຫຼາຍຂັ້ນຕອນ.

--------------------------------------------------------------------------------
ຂັ້ນຕອນການໃຊ້ງານງ່າຍໆ:
--------------------------------------------------------------------------------
1. ສຽບ Flash Drive USB ໃສ່ຄອມພິວເຕີຫ້ອງຮຽນ.
2. ເປີດໂປຣແກຣມ ClassroomChecker.exe ຂຶ້ນມາ (Run ແບບ Portable ບໍ່ຕ້ອງລົງ).
3. ກົດປຸ່ມສີຂຽວ '⚡ ຕັ້ງຄ່າດ່ວນ' ຢູ່ແຖບດ້ານເທິງ.
4. ກວດເບິ່ງຊື່ຫ້ອງຮຽນ ແລະ ເລກເຄື່ອງ:
   • ລະບົບຈະ Auto-Increment ເລກເຄື່ອງໃຫ້ອັດຕະໂນມັດ (ເຊັ່ນ: 01 -> 02 -> 03...)
     ເມື່ອຍ້າຍ USB ໄປສຽບເຄື່ອງຖັດໄປ!
5. ກົດປຸ່ມ '⚡ ເລີ່ມຕັ້ງຄ່າດ່ວນ'.
6. ລະບົບຈະດຳເນີນການ 11 ຂັ້ນຕອນໃຫ້ອັດຕະໂນມັດ:
   ✔ ຂັ້ນຕອນ 1: ຕັ້ງຮູບພື້ນຫຼັງ Wallpaper ພ້ອມປ້າຍຊື່ຫ້ອງ ແລະ ເລກເຄື່ອງ
                (ພ້ອມຮູບໜ້າ Lock/Login ແລະ ຂໍ້ຄວາມກ່ອນ Login ຖ້າເປີດໄວ້ໃນ ຕັ້ງຄ່າ)
   ✔ ຂັ້ນຕອນ 2: ຕັ້ງຄ່າ Power Scheme ເປັນ Never Sleep (ປ້ອງກັນຈໍດັບ/ເຄື່ອງຫຼັບຕອນຮຽນ)
   ✔ ຂັ້ນຕອນ 3: ກວດສອບ ແລະ ລົງທະບຽນ Font ພາສາລາວ (Phetsarath OT)
   ✔ ຂັ້ນຕອນ 4: ກວດສອບ ແລະ ລົງທະບຽນ ແປ້ນພິມພາສາລາວ (Lao Keyboard 0454)
   ✔ ຂັ້ນຕອນ 5: ກຳນົດ Timezone (+7), ຮູບແບບວັນທີ ແລະ ເວລາ 12/24 ຊົ່ວໂມງ (ຕາມທີ່ຕັ້ງໄວ້)
   ✔ ຂັ້ນຕອນ 6: ສະແດງໄອຄອນ This PC, Control Panel ແລະ ສ້າງ Shortcut MS Office (ມີແລ້ວຈະບໍ່ສ້າງຊ້ຳ)
   ✔ ຂັ້ນຕອນ 7: ລ້າງໄຟລ໌ຂີ້ເຫຍື້ອ Temp/Cache ທີ່ປອດໄພ
   ✔ ຂັ້ນຕອນ 8: Export ຂໍ້ມູນສະເປັກເຄື່ອງລະອຽດເປັນໄຟລ໌ Excel (.xlsx) ເກັບໄວ້ໃນ USB
   ✔ ຂັ້ນຕອນ 9: ປິດ Windows AutoRun & ສ້າງວັກຊີນປ້ອງກັນ USB Shortcut
   ✔ ຂັ້ນຕອນ 10: ໂໝດເຄື່ອງສະເປັກຕ່ຳ (ໂໝດ 'ອັດຕະໂນມັດ' ຈະເຮັດສະເພາະເຄື່ອງ RAM ບໍ່ເກີນ 4 GB)
   ✔ ຂັ້ນຕອນ 11: ປ່ຽນຊື່ເຄື່ອງ (ເຊັ່ນ COM1-01) ແລະ ຊື່ User ທີ່ Login ຢູ່ (ເຊັ່ນ USER-COM1-01)
7. Restart ເຄື່ອງເມື່ອລະບົບຖາມ ເພື່ອໃຫ້ຊື່ໃໝ່ມີຜົນ.
8. ຖອດ USB ໄປສຽບເຄື່ອງຕໍ່ໄປໄດ້ເລີຍ — ເລກເຄື່ອງຈະເພີ່ມຂຶ້ນໃຫ້ພ້ອມສຳລັບເຄື່ອງຕໍ່ໄປ!

💡 ລາຍຊື່ຫ້ອງ, ເລກເລີ່ມຕົ້ນ, ຄຳນຳໜ້າຊື່ User, ຮູບແບບວັນທີ/ເວລາ ແລະ ໂໝດເຄື່ອງສະເປັກຕ່ຳ ຕັ້ງໄດ້ໃນ '⚙️ ຕັ້ງຄ່າ'.";
        }

        private string GetWallpaperContent()
        {
            bool isLo = Localizer.CurrentLanguage == "lo";
            if (!isLo)
            {
                return @"================================================================================
🖼️ 2. WALLPAPER & COMPUTER NUMBERING TAB
================================================================================

This tab allows you to customize and apply computer identification wallpapers with crisp Lao typography rendered directly onto the desktop image.

--------------------------------------------------------------------------------
KEY CONTROLS & FEATURES:
--------------------------------------------------------------------------------
1. Classroom Room (ຫ້ອງຮຽນ):
   • Dropdown selecting rooms (Default: ຫ້ອງຄອມພິວເຕີ 1 to ຫ້ອງຄອມພິວເຕີ 8).
   • Click '+ ເພີ່ມຫ້ອງ' to add new rooms or 'ລຶບຫ້ອງ' to delete.
2. Computer Number (ເລກເຄື່ອງ):
   • Up/Down numeric selector (1 to 99).
   • Formatted as 2-digit numbers on the wallpaper (e.g., '01', '02', '15').
3. Background Image (ຮູບພື້ນຫຼັງ):
   • Click 'ເລືອກຮູບ...' to pick custom images (.png, .jpg, .bmp).
   • Click 'ໃຊ້ຄ່າເລີ່ມຕົ້ນ' to restore the built-in high-resolution background.
4. Preview Window (ຕົວຢ່າງຮູບພື້ນຫຼັງ):
   • Real-time preview of how the wallpaper will appear on screen.
5. Buttons:
   • 'ກຳນົດພື້ນຫຼັງ (Set Wallpaper)': Applies the wallpaper to the current machine.
   • '⚡ ຕັ້ງຄ່າດ່ວນ (Quick Setup)': Shortcut to run the full setup sequence.

--------------------------------------------------------------------------------
ADVANCED SETTINGS IN ⚙️ SETTINGS DIALOG:
--------------------------------------------------------------------------------
• Label Template: Custom text format (e.g., '{room} - ເຄື່ອງ {num}').
• Label Position: TopCenter, BottomCenter, TopRight, TopLeft, etc.
• Font Size, Font Color (RGB Hex picker), and Text Shadow.
• Apply to All Users: Applies the wallpaper registry keys across all user accounts on the machine.

--------------------------------------------------------------------------------
🔒 LOCK & SIGN-IN SCREEN (⚙️ Settings → Lock & Login tab):
--------------------------------------------------------------------------------
• Use the same image for the Lock screen and the Sign-in (Welcome) screen.
• Turn off the blur on the sign-in screen so the room/PC label stays readable.
• Prevent students from changing the wallpaper.
• Show a notice (title + text) before sign-in, e.g. lab rules.
• Turning an option off restores the Windows default.

--------------------------------------------------------------------------------
✏️ RENAME COMPUTER & USER:
--------------------------------------------------------------------------------
• The computer is renamed to the room format (e.g. COM1-01).
• The signed-in local user is renamed too: prefix + computer name (default USER-COM1-01).
  Change the prefix (e.g. LTVC → LTVC-COM1-01) or turn this off in ⚙️ Settings → General.
• Both the login name and the display name change; restart to finish.";
            }

            return @"================================================================================
🖼️ 2. ແທັບ 'ພື້ນຫຼັງ' (WALLPAPER & LABELING)
================================================================================

ແທັບນີ້ໃຊ້ສຳລັບຕັ້ງຄ່າຮູບພື້ນຫຼັງໜ້າຈໍ desktop ພ້ອມຕິດປ້າຍຊື່ຫ້ອງຮຽນ ແລະ ເລກເຄື່ອງ ດ້ວຍ Font ພາສາລາວ (Phetsarath OT) ທີ່ຄົມຊັດສວຍງາມ.

--------------------------------------------------------------------------------
ປຸ່ມ ແລະ ຕົວເລືອກຕ່າງໆ:
--------------------------------------------------------------------------------
1. ຫ້ອງຮຽນ (Classroom Room):
   • ເລືອກຫ້ອງຮຽນຈາກລາຍຊື່ (ຄ່າເລີ່ມຕົ້ນ: ຫ້ອງຄອມພິວເຕີ 1 ຫາ ຫ້ອງຄອມພິວເຕີ 8)
   • ກົດປຸ່ມ '+ ເພີ່ມຫ້ອງ' ເພື່ອເພີ່ມຫ້ອງໃໝ່ ຫຼື ປຸ່ມ 'ລຶບຫ້ອງ' ເພື່ອລຶບ
2. ເລກເຄື່ອງ (Computer Number):
   • ປັບເລກເຄື່ອງ (1 ຫາ 99), ລະບົບຈະສະແດງເປັນເລກ 2 ຫຼັກສະເໝີ (ເຊັ່ນ: 01, 02, 10...)
3. ຮູບພື້ນຫຼັງ (Background Image):
   • ກົດ 'ເລືອກຮູບ...' ເພື່ອເລືອກຮູບໂລໂກ້ ຫຼື ຮູບໂຮງຮຽນຂອງທ່ານ (.png, .jpg, .bmp)
   • ກົດ 'ໃຊ້ຄ່າເລີ່ມຕົ້ນ' ເພື່ອກັບຄືນໄປໃຊ້ຮູບພື້ນຫຼັງສີຟ້າທີ່ຕິດມາກັບໂປຣແກຣມ
4. ກ່ອງຕົວຢ່າງ (Preview):
   • ສະແດງພາບຕົວຢ່າງແບບ Real-Time ໃຫ້ເຫັນກ່ອນກົດຕັ້ງຄ່າແທ້
5. ປຸ່ມສັ່ງການ:
   • ປຸ່ມ 'ກຳນົດພື້ນຫຼັງ (Set Wallpaper)': ຕັ້ງຄ່າ Wallpaper ທັນທີ
   • ປຸ່ມ '⚡ ຕັ້ງຄ່າດ່ວນ (Quick Setup)': ເປີດປັອບອັບຕັ້ງຄ່າຄົບວົງຈອນ

--------------------------------------------------------------------------------
ການຕັ້ງຄ່າເພີ່ມເຕີມໃນ ⚙️ 'ຕັ້ງຄ່າ':
--------------------------------------------------------------------------------
• ຮູບແບບປ້າຍຊື່ (Template): ປັບແຕ່ງຂໍ້ຄວາມໄດ້ຕາມໃຈ (ເຊັ່ນ: '{room} - ເຄື່ອງ {num}')
• ຕຳແໜ່ງປ້າຍຊື່: ເທິງກາງ (TopCenter), ລຸ່ມກາງ, ຂວາເທິງ, ຊ້າຍເທິງ...
• ຂະໜາດຕົວໜັງສື, ສີຕົວໜັງສື (Color Picker), ແລະ ເງົາຕົວໜັງສື (Text Shadow)
• ຕັ້ງຄ່າໃຫ້ທຸກ User (Apply to All Users): ຕັ້ງຄ່າໃຫ້ທຸກ User Account ໃນ Windows

--------------------------------------------------------------------------------
🔒 ໜ້າ LOCK & LOGIN (⚙️ ຕັ້ງຄ່າ → ແທັບ ໜ້າ Lock & Login):
--------------------------------------------------------------------------------
• ໃຊ້ຮູບດຽວກັນກັບໜ້າ Lock ແລະ ໜ້າ Login (Welcome)
• ປິດການເບລີ (Blur) ໃນໜ້າ Login ເພື່ອໃຫ້ອ່ານປ້າຍຊື່ຫ້ອງ/ເລກເຄື່ອງໄດ້ຊັດ
• ລັອກບໍ່ໃຫ້ນັກຮຽນປ່ຽນຮູບພື້ນຫຼັງ
• ສະແດງຂໍ້ຄວາມ (ຫົວຂໍ້ + ເນື້ອໃນ) ກ່ອນ Login ເຊັ່ນ ລະບຽບການນຳໃຊ້ຫ້ອງຄອມ
• ປິດຕົວເລືອກໃດ ລະບົບຈະກັບຄືນເປັນຄ່າເດີມຂອງ Windows

--------------------------------------------------------------------------------
✏️ ປ່ຽນຊື່ເຄື່ອງ ແລະ ຊື່ USER:
--------------------------------------------------------------------------------
• ປ່ຽນຊື່ເຄື່ອງຕາມຮູບແບບຫ້ອງຮຽນ (ເຊັ່ນ COM1-01)
• ປ່ຽນຊື່ User ທີ່ Login ຢູ່ນຳ: ຄຳນຳໜ້າ + ຊື່ເຄື່ອງ (ຄ່າເລີ່ມຕົ້ນ USER-COM1-01)
  ປ່ຽນຄຳນຳໜ້າ (ເຊັ່ນ LTVC → LTVC-COM1-01) ຫຼື ປິດໄດ້ໃນ ⚙️ ຕັ້ງຄ່າ → ທົ່ວໄປ
• ປ່ຽນທັງຊື່ Login ແລະ ຊື່ທີ່ສະແດງ; Restart ເຄື່ອງເພື່ອໃຫ້ມີຜົນສົມບູນ";
        }

        private string GetScanContent()
        {
            bool isLo = Localizer.CurrentLanguage == "lo";
            if (!isLo)
            {
                return @"================================================================================
🔍 3. SYSTEM INSPECTION & SPEC EXPORT TAB
================================================================================

This tab runs 17 hardware and software inspection modules and provides one-click automatic fixing.

--------------------------------------------------------------------------------
THE 17 INSPECTION MODULES:
--------------------------------------------------------------------------------
1. MS Office: Checks installation & activation for Word, Excel, PowerPoint.
2. Windows License: Checks Windows genuine activation status via WMI/SoftwareLicensing.
3. Essential Software: Checks PDF reader, LaoScript 8, VLC and 7-Zip.
   Fix installs only the missing ones from the USB 'installers\' folder.
4. Font Phetsarath OT: Verifies national Lao font installation (auto-fixes by registering embedded font).
5. Lao Keyboard Layout: Checks Lao keyboard 00000454 (auto-fixes by registering layout).
6. Date & Time: Checks timezone (+7), date format and 12/24-hour time from Settings.
7. Desktop Icons: Checks This PC, Control Panel and Office shortcuts.
8. Computer Name (Hostname): Checks if the hostname follows the classroom standard (e.g. COM1-01) or is a default (DESKTOP-XXXX).
9. Power Scheme (Never sleep): Verifies AC & DC sleep timeouts are 0s (auto-fixes via powercfg).
10. Temp Files Cleanup: Analyzes temporary files, cache, and Windows temp (auto-fixes safe deletion).
11. Low-spec Mode: Speeds up PCs with little RAM (see the 'Low-spec PCs' tab).
12. Device Drivers: Scans for missing or malfunctioning hardware drivers with yellow exclamation marks.
13. Disk Free Space: Evaluates system drive C: free space (<5% Fail, <10% Warn, >=10% Pass).
14. Windows Defender: Checks antivirus protection and real-time scanning status.
15. Security & App Lock: Checks whether student USB/Downloads/game restrictions are applied.
16. USB Vaccine & AutoRun Protection: Checks AutoRun state and immunizes flash drives against shortcut malware.
17. Export Machine Spec: Generates a complete hardware/software audit report into a multi-sheet Excel (.xlsx) file.

--------------------------------------------------------------------------------
ACTION BUTTONS:
--------------------------------------------------------------------------------
• 'ສະແກນ (Scan)': Runs all checks with live progress reporting.
• 'ແກ້ໄຂທີ່ເລືອກ (Fix)': Automatically fixes all checked issues.
• 'ບັນທຶກລາຍງານ (.txt)': Saves a readable diagnostic text report into the reports/ folder.
• '📊 ລວມສະເປັກ (Master Excel)': Merges all individual machine .xlsx specs collected in specs/
  into a single master classroom audit summary workbook!";
            }

            return @"================================================================================
🔍 3. ແທັບ 'ກວດສອບລະບົບ' (SYSTEM INSPECTION)
================================================================================

ແທັບນີ້ໃຊ້ສຳລັບກວດສອບສະພາບຄວາມພ້ອມຂອງເຄື່ອງຄອມພິວເຕີທັງ Hardware ແລະ Software ຄົບທັງ 17 ໂມດູນ ພ້ອມປຸ່ມແກ້ໄຂອັດຕະໂນມັດໃນຄລິກດຽວ.

--------------------------------------------------------------------------------
ລາຍລະອຽດ 17 ໂມດູນທີ່ກວດສອບ:
--------------------------------------------------------------------------------
1. MS Office: ກວດການຕິດຕັ້ງ ແລະ ສະຖານະ Activation ຂອງ Word, Excel, PowerPoint
2. Windows License: ກວດສະຖານະ Windows Activation ວ່າເປັນ License ແທ້ ຫຼື ໝົດອາຍຸ
3. ຊອບແວພື້ນຖານ: ກວດ PDF, LaoScript 8, VLC ແລະ 7-Zip
   (ກົດ Fix ຈະຕິດຕັ້ງສະເພາະຕົວທີ່ຂາດ ຈາກໂຟນເດີ 'installers\' ໃນ USB)
4. Font Phetsarath OT: ກວດສອບຟອນລາວ (ກົດ Fix ເພື່ອຕິດຕັ້ງຟອນຈາກຕົວໂປຣແກຣມໄດ້ທັນທີ)
5. Lao Keyboard Layout: ກວດແປ້ນພິມພາສາລາວ 0454 (ກົດ Fix ເພື່ອເພີ່ມແປ້ນພິມໃຫ້ທັນທີ)
6. ວັນທີ & ເວລາ: ກວດ Timezone (+7), ຮູບແບບວັນທີ ແລະ ເວລາ 12/24 ຊົ່ວໂມງ ຕາມທີ່ຕັ້ງໄວ້
7. ໄອຄອນໜ້າຈໍ: ກວດ This PC, Control Panel ແລະ Shortcut MS Office
8. Computer Name: ກວດຊື່ຄອມພິວເຕີວ່າເປັນຊື່ມາດຕະຖານຕາມຫ້ອງຮຽນ (COM1-01) ຫຼື ຍັງເປັນຊື່ເລີ່ມຕົ້ນ (DESKTOP-XXXX)
9. Power Scheme (Never sleep): ກວດການຕັ້ງຄ່າບໍ່ໃຫ້ເຄື່ອງຫຼັບ (ກົດ Fix ຕັ້ງ Never Sleep ທັນທີ)
10. Temp Files Cleanup: ກວດຫາໄຟລ໌ຂີ້ເຫຍື້ອ Temp (ກົດ Fix ເພື່ອລ້າງໄຟລ໌ຂີ້ເຫຍື້ອປອດໄພ)
11. ໂໝດເຄື່ອງສະເປັກຕ່ຳ: ເລັ່ງຄວາມໄວເຄື່ອງ RAM ນ້ອຍ (ເບິ່ງແທັບ 'ເຄື່ອງສະເປັກຕ່ຳ')
12. Device Drivers: ກວດສອບໄດຣເວີອຸປະກອນທີ່ມີບັນຫາ (ໄດຣເວີຂາດ ຫຼື ເຄື່ອງໝາຍຕົກໃຈສີເຫຼືອງ)
13. Disk Free Space: ກວດເນື້ອທີ່ວ່າງ Drive C: (<5% Fail, <10% Warn, >=10% Pass)
14. Windows Defender: ກວດສະຖານະ Antivirus ແລະ ການປ້ອງກັນ Real-time
15. Security & App Lock: ກວດເບິ່ງວ່ານະໂຍບາຍບລັອກ USB/Downloads/ເກມ ເປີດຢູ່ຫຼືບໍ່
16. USB Vaccine & AutoRun: ກວດສະຖານະ Windows AutoRun ແລະ ວັກຊີນປ້ອງກັນ USB Shortcut
17. Export Machine Spec: ກວດ ແລະ Export ສະເປັກເຄື່ອງ (CPU, RAM, Mainboard, Storage, GPU, IP, MAC) ເປັນ Excel (.xlsx)

--------------------------------------------------------------------------------
ປຸ່ມສັ່ງການ:
--------------------------------------------------------------------------------
• 'ສະແກນ (Scan)': ເລີ່ມກວດສອບທຸກໂມດູນພ້ອມກັນ ສະແດງຜົນ Pass / Warning / Fail
• 'ແກ້ໄຂທີ່ເລືອກ (Fix)': ແກ້ໄຂທຸກລາຍການທີ່ຕິກເລືອກອັດຕະໂນມັດໃນຄລິກດຽວ
• 'ບັນທຶກລາຍງານ (.txt)': ບັນທຶກລາຍງານຜົນການກວດສອບເປັນໄຟລ໌ Text ໃນໂຟນເດີ reports/
• '📊 ລວມສະເປັກ (Master Excel)': ລວມໄຟລ໌ສະເປັກ Excel ຂອງທຸກເຄື່ອງໃນຫ້ອງທີ່ຢູ່ໃນໂຟນເດີ specs/
  ມາເປັນໄຟລ໌ສະຫຼຸບໃຫຍ່ 1 ໄຟລ໌ (Master Summary) ພ້ອມສະຖິຕິຮວມສະດວກຕໍ່ການລາຍງານ!";
        }

        private string GetSecurityContent()
        {
            bool isLo = Localizer.CurrentLanguage == "lo";
            if (!isLo)
            {
                return @"================================================================================
🛡️ 4. SECURITY & APP/GAME LOCK TAB
================================================================================

This tab enforces system policies that prevent students from running unauthorized executables, playing games, or launching malware from USB drives or downloads.

--------------------------------------------------------------------------------
SECURITY POLICIES EXPLAINED:
--------------------------------------------------------------------------------
1. 🔒 Deny USB Execution (Block USB Flash Drive Exes):
   • Uses Windows Software Restriction Policies (Safer API Level 0 - Disallowed).
   • Blocks execution of .exe/.bat/.com from all removable drives (D:\ to Z:\).
   • SMART WHITELIST: ClassroomChecker.exe on USB is automatically whitelisted at Level 262144 (Unrestricted),
     ensuring the admin tool always runs smoothly while student games on USB are blocked!
2. 📁 Block Downloads & Temp:
   • Blocks execution of programs located in student Downloads and Temp folders.
3. 🚫 Block Popular Games Automatically:
   • Pre-configured with built-in signatures for Roblox, Steam, Garena, Epic Games,
     Minecraft, Discord, Valorant, and other popular distractions.
4. Blocked .exe List & Blocked Folders:
   • Click '➕ ເພີ່ມ .exe' to add custom program names (e.g., game.exe).
   • Click '📁 ເລືອກໂຟນເດີ...' to block entire directories.
5. 🛡️ USB Shortcut Virus Cleaner & Vaccine Tool:
   • Click '🚀 Open USB Cleaner & Vaccine...' to rescue infected USB flash drives.
   • Automatically terminates background wscript.exe/cscript.exe malware processes.
   • Removes fake .lnk shortcuts, rogue .vbs/.js scripts, and fake <folder>.exe files.
   • Unhides all legitimate files & directories (strips Hidden/System/ReadOnly flags).
   • Creates protected autorun.inf dummy vaccine directory to prevent reinfection.
   • Disables Windows AutoRun (NoDriveTypeAutoRun = 0xFF) to permanently protect lab PCs!

--------------------------------------------------------------------------------
ACTION BUTTONS:
--------------------------------------------------------------------------------
• '🔒 ເປີດການລັອກ (Enable Lock)': Applies all selected security restrictions immediately to the machine.
• '🔓 ປິດການລັອກ (Disable Lock)': Removes all security restrictions and restores full default execution rights.
• '🛡️ ກູ້ໄຟລ໌ USB (USB Rescue)': Opens the USB Virus Cleaner & Recovery dialog.
• '🔄 ກວດສອບ (Refresh)': Re-checks the current system lock state.
• '💾 ບັນທຶກ (Save)': Saves the security configuration to config.json.

--------------------------------------------------------------------------------
🌐 WINDOWS DISPLAY LANGUAGE:
--------------------------------------------------------------------------------
• 'Set to English': sets Windows menus back to English (e.g. after a student switched to Chinese).
• Tick '🔒 Lock' first to also prevent changing it and hide the Language pages in Settings.
• Untick '🔒 Lock' and press the button to unlock.
• Takes effect after signing out and in. Keyboard layouts (Lao input) are not changed.
• The PC must already have English installed.";
            }

            return @"================================================================================
🛡️ 4. ແທັບ 'ປ້ອງກັນ' (SECURITY & APP/GAME LOCK)
================================================================================

ແທັບນີ້ໃຊ້ສຳລັບລັອກນະໂຍບາຍຄວາມປອດໄພຂອງ Windows ເພື່ອປ້ອງກັນບໍ່ໃຫ້ນັກຮຽນ Run ເກມ, ໂປຣແກຣມທີ່ບໍ່ໄດ້ຮັບອະນຸຍາດ ຫຼື ໄວຣັສ ຈາກ Flash Drive ຫຼື ໂຟນເດີ Downloads.

--------------------------------------------------------------------------------
3 ນະໂຍບາຍຫຼັກທີ່ລະບົບຈັດການ:
--------------------------------------------------------------------------------
1. 🔒 ຫ້າມ Run ໂປຣແກຣມ/ເກມ ຈາກ Flash Drive USB:
   • ໃຊ້ລະບົບ Windows Safer API (Level 0 - Disallowed) ບລັອກທຸກໄຟລ໌ .exe/.bat ໃນ USB
   • ລະບົບ SMART WHITELIST: ໂປຣແກຣມ ClassroomChecker.exe ໃນ USB ຈະຖືກຍົກເວັ້ນ (Unrestricted Level 262144)
     ເຮັດໃຫ້ Admin ສາມາດສຽບ USB ແລະ ເປີດ ClassroomChecker ໄດ້ສະເໝີ ໃນຂະນະທີ່ເກມຂອງນັກຮຽນຈະຖືກບລັອກ!
2. 📁 ຫ້າມ Run ໂປຣແກຣມ ຈາກໂຟນເດີ Downloads ແລະ Temp:
   • ປ້ອງກັນນັກຮຽນດາວໂຫຼດເກມ ຫຼື ໂປຣແກຣມຕິດຕັ້ງມາໄວ້ໃນ Downloads ແລ້ວກົດ Run
3. 🚫 ບລັອກເກມຍອດຮິດອັດຕະໂນມັດ:
   • ບລັອກເກມອັດຕະໂນມັດ: Roblox, Steam, Garena, Epic Games, Minecraft, Discord, Valorant...
4. ລາຍຊື່ .exe ແລະ ໂຟນເດີທີ່ບລັອກເພີ່ມເຕີມ:
   • ກົດປຸ່ມ '➕ ເພີ່ມ .exe' ເພື່ອເພີ່ມຊື່ໄຟລ໌ໂປຣແກຣມທີ່ຕ້ອງການບລັອກ
   • ກົດປຸ່ມ '📁 ເລືອກໂຟນເດີ...' ເພື່ອບລັອກທຸກໂປຣແກຣມໃນໂຟນເດີນັ້ນໆ
5. 🛡️ ເຄື່ອງມືກູ້ໄຟລ໌ & ຂ້າໄວຣັສ USB Shortcut (USB Virus Cleaner & Vaccine):
   • ກົດປຸ່ມ '🚀 ເປີດເຄື່ອງມືກູ້ໄຟລ໌ USB...' ເພື່ອແກ້ໄຂ Flash Drive ຕິດໄວຣັສ Shortcut
   • ປິດ Process ໄວຣັສ Script ທີ່ກຳລັງແລ່ນໃນ RAM (wscript.exe, cscript.exe) ບໍ່ໃຫ້ຕິດຊ້ຳ
   • ລຶບໄຟລ໌ Shortcut ປອມ (.lnk), ໄຟລ໌ script ໄວຣັສ (.vbs, .js) ແລະ .exe ປອມທີ່ປອມເປັນໂຟນເດີ (<folder>.exe)
   • ປົດລັອກ ແລະ ກູ້ຄືນທຸກໄຟລ໌/ໂຟນເດີທີ່ຖືກໄວຣັສເຊື່ອງ (Unhide Hidden/System attributes) ທັນທີ
   • ສ້າງໂຟນເດີວັກຊີນປ້ອງກັນ autorun.inf ໃນ USB ປ້ອງກັນບໍ່ໃຫ້ໄວຣັສຂຽນ autorun.inf ໄດ້ອີກ
   • ປິດ Windows AutoRun/AutoPlay (NoDriveTypeAutoRun = 0xFF) ປ້ອງກັນເຄື່ອງຄອມພິວເຕີຫ້ອງຮຽນຖາວອນ!

--------------------------------------------------------------------------------
ປຸ່ມສັ່ງການ:
--------------------------------------------------------------------------------
• '🔒 ເປີດການລັອກ': ເປີດໃຊ້ງານນະໂຍບາຍບລັອກທັນທີ ມີຜົນກັບທຸກ User ໃນເຄື່ອງ
• '🔓 ປິດການລັອກ': ປົດລັອກນະໂຍບາຍທັງໝົດກັບຄືນເປັນປົກກະຕິ
• '🔄 ກວດສອບ': ກວດສອບສະຖານະການລັອກປັດຈຸບັນຂອງເຄື່ອງ
• '💾 ບັນທຶກ': ບັນທຶກລາຍຊື່ເກມ ແລະ ນະໂຍບາຍລົງໄຟລ໌ config.json

--------------------------------------------------------------------------------
🌐 ພາສາສະແດງຜົນຂອງ Windows:
--------------------------------------------------------------------------------
• ປຸ່ມ 'ຕັ້ງເປັນພາສາອັງກິດ': ປ່ຽນເມນູ Windows ກັບເປັນອັງກິດ (ເຊັ່ນ ເມື່ອນັກຮຽນປ່ຽນເປັນພາສາຈີນ)
• ໝາຍຊ່ອງ '🔒 ລັອກ' ກ່ອນກົດ ເພື່ອຫ້າມປ່ຽນພາສາ ແລະ ຊ່ອນໜ້າ Language ໃນ Settings
• ເອົາໝາຍຊ່ອງ '🔒 ລັອກ' ອອກ ແລ້ວກົດປຸ່ມ ເພື່ອປົດລັອກ
• ມີຜົນຫຼັງ Sign out ແລ້ວ Sign in ໃໝ່. ແປ້ນພິມພາສາລາວບໍ່ປ່ຽນ
• ເຄື່ອງຕ້ອງມີພາສາອັງກິດຕິດຕັ້ງຢູ່ແລ້ວ";
        }

        private string GetInstallContent()
        {
            bool isLo = Localizer.CurrentLanguage == "lo";
            if (!isLo)
            {
                return @"================================================================================
📦 5. SOFTWARE INSTALLER TAB (OFFLINE FOSS SUITE)
================================================================================

This tab provides a 1-click silent offline installer catalog for 11 essential open-source and free educational tools.

--------------------------------------------------------------------------------
SOFTWARE CATALOG (11 PACKAGES):
--------------------------------------------------------------------------------
1. 7-Zip: 64-bit file archiver (Silent: /S)
2. VLC Media Player: Multi-format multimedia player (Silent: /L=1033 /S)
3. Google Chrome: Enterprise web browser (Silent: /silent /install)
4. Brave Browser: Fast, private Chromium browser (Silent: --silent --install)
5. LibreOffice: Complete office suite with Writer, Calc, Impress (Silent: /qn ALLUSERS=1)
6. Sumatra PDF: Lightweight, lightning-fast PDF/ePub reader (Silent: -s)
7. Notepad++: Source code & text editor (Silent: /S)
8. VS Code: Modern code editor for programming classes (Silent: /VERYSILENT /NORESTART /MERGETASKS=!runcode,addcontextmenufiles,addcontextmenufolders,associatewithfiles,addtopath)
9. GIMP: Advanced image manipulation and graphic design program (Silent: /VERYSILENT /NORESTART /ALLUSERS)
10. Audacity: Audio recording and editing software (Silent: /VERYSILENT /NORESTART)
11. RAMMap (Sysinternals): Portable tool to purge standby and working set RAM.
    • Auto-copies to C:\Program Files\RAMMap\
    • Creates Desktop shortcut 'RAMMap (Clear RAM).lnk'
    • Registers 'rammap64' Run command & auto-accepts Sysinternals EULA in Registry!

--------------------------------------------------------------------------------
HOW TO USE:
--------------------------------------------------------------------------------
1. Copy installer files (.exe/.msi) into the 'installers\' folder on your USB drive.
2. The table displays current local status ('✓ ຕິດຕັ້ງແລ້ວ' or '✗ ຍັງບໍ່ທັນຕິດຕັ້ງ') and whether the setup file is found in USB.
3. Click '📥 ຕິດຕັ້ງ' on any individual software to install it silently.
4. Or click '⚡ ຕິດຕັ້ງທີ່ຂາດທັງໝົດ' to install all missing programs sequentially.

💡 The 4 essential programs (PDF reader, LaoScript 8, VLC, 7-Zip) are also checked and installed
   by the 'Essential Software' scan module. Everything else is installed manually from this tab.";
            }

            return @"================================================================================
📦 5. ແທັບ 'ຕິດຕັ້ງຊອບແວ' (SOFTWARE INSTALLER)
================================================================================

ແທັບນີ້ໃຊ້ສຳລັບຕິດຕັ້ງຊອບແວພື້ນຖານ ແລະ Open Source ທັງ 11 ໂປຣແກຣມແບບ Offline ຈາກ USB Flash Drive ໂດຍກົງ ບໍ່ຕ້ອງຕໍ່ເນັດ ແລະ ບໍ່ຕ້ອງກົດ Next, Next (Silent Install).

--------------------------------------------------------------------------------
ລາຍການຊອບແວທັງ 11 ໂປຣແກຣມ:
--------------------------------------------------------------------------------
1. 7-Zip: ໂປຣແກຣມບີບອັດໄຟລ໌ 64-bit (Silent: /S)
2. VLC Media Player: ໂປຣແກຣມເປີດວິດີໂອ ແລະ ສຽງທຸກນາມສະກຸນ (Silent: /L=1033 /S)
3. Google Chrome: ເວັບບຣາວເຊີຍອດນິຍົມ (Silent: /silent /install)
4. Brave Browser: ເວັບບຣາວເຊີປອດໄພ ບລັອກໂຄສະນາ (Silent: --silent --install)
5. LibreOffice: ຊຸດໂປຣແກຣມຫ້ອງການຟຣີ Writer, Calc, Impress (Silent: /qn ALLUSERS=1)
6. Sumatra PDF: ໂປຣແກຣມເປີດອ່ານ PDF ນ້ຳໜັກເບົາ ໂຫຼດໄວທີ່ສຸດ (Silent: -s)
7. Notepad++: ໂປຣແກຣມຂຽນໂຄ້ດ ແລະ ແກ້ໄຂຂໍ້ຄວາມ (Silent: /S)
8. VS Code: ໂປຣແກຣມ Code Editor ສຳລັບການຮຽນຂຽນໂປຣແກຣມ (Silent: /VERYSILENT /NORESTART ...)
9. GIMP: ໂປຣແກຣມຕັດຕໍ່ແຕ່ງຮູບພາບ (Silent: /VERYSILENT /NORESTART /ALLUSERS)
10. Audacity: ໂປຣແກຣມອັດສຽງ ແລະ ຕັດຕໍ່ສຽງ (Silent: /VERYSILENT /NORESTART)
11. RAMMap64 (Sysinternals): ເຄື່ອງມື Portable ສຳລັບລ້າງ RAM ທີ່ຄ້າງ
    • ຄັດລອກໄຟລ໌ລົງ C:\Program Files\RAMMap\ ອັດຕະໂນມັດ
    • ສ້າງ Shortcut ຊື່ 'RAMMap (Clear RAM).lnk' ໄວ້ໜ້າຈໍ Desktop
    • ລົງທະບຽນ App Paths (Win+R ພິມ rammap64 ໄດ້ທັນທີ) ແລະ ຕັ້ງຄ່າ Auto-Accept EULA!

--------------------------------------------------------------------------------
ຂັ້ນຕອນການນຳໃຊ້:
--------------------------------------------------------------------------------
1. ດາວໂຫຼດໄຟລ໌ Setup (.exe/.msi) ມາໄວ້ໃນໂຟນເດີ 'installers\' ໃນ USB Flash Drive.
2. ຕາຕະລາງຈະບອກສະຖານະວ່າໂປຣແກຣມໃດຕິດຕັ້ງແລ້ວ ('✓ ຕິດຕັ້ງແລ້ວ') ຫຼື ຂາດ ('✗ ຍັງບໍ່ທັນຕິດຕັ້ງ') ແລະ ພົບໄຟລ໌ໃນ USB ຫຼື ບໍ່.
3. ກົດປຸ່ມ '📥 ຕິດຕັ້ງ' ໃນແຕ່ລະແຖວເພື່ອເລືອກຕິດຕັ້ງສະເພາະໂປຣແກຣມທີ່ຕ້ອງການ (ບໍ່ບັງຄັບຕິດຕັ້ງທັງໝົດ).
4. ຫຼື ກົດປຸ່ມ '⚡ ຕິດຕັ້ງທີ່ຂາດທັງໝົດ' ດ້ານເທິງ ເພື່ອໃຫ້ລະບົບຕິດຕັ້ງທຸກຕົວທີ່ຍັງຂາດໃຫ້ອັດຕະໂນມັດ.

💡 ຊອບແວພື້ນຖານ 4 ໂປຣແກຣມ (PDF, LaoScript 8, VLC, 7-Zip) ຈະຖືກກວດ ແລະ ຕິດຕັ້ງໂດຍໂມດູນ
   'ຊອບແວພື້ນຖານ' ໃນແທັບກວດສອບລະບົບນຳ. ໂປຣແກຣມອື່ນໆ ໃຫ້ຕິດຕັ້ງດ້ວຍມືຈາກແທັບນີ້.";
        }

        private string GetUsbAntivirusContent()
        {
            bool isLo = Localizer.CurrentLanguage == "lo";
            if (!isLo)
            {
                return @"================================================================================
🦠 6. ANTIVIRUS USB TAB (RECOVERY & VACCINE)
================================================================================

This dedicated tab provides full-screen offline disinfection, file recovery, and AutoRun immunization for USB flash drives and lab computers.

--------------------------------------------------------------------------------
KEY CAPABILITIES:
--------------------------------------------------------------------------------
1. 📂 Recover Hidden Files & Folders (Unhide):
   • Shortcut worms hide genuine folders by setting Hidden, System, and Read-Only flags.
   • Automatically strips these flags recursively from all files and folders, immediately restoring student assignments and documents.
2. 🚫 Remove Fake Shortcuts (.lnk):
   • Detects and removes rogue .lnk shortcuts that mimic folders while executing cmd, wscript, or powershell.
3. 🗑️ Delete Rogue Scripts:
   • Removes malicious .vbs, .js, .wsf, and fake autorun.inf files disguised on the drive.
4. 🛑 Terminate Active Malware Memory Processes:
   • Kills rogue wscript.exe and cscript.exe processes actively running in RAM to prevent reinfection.
5. 💉 Create USB Vaccine:
   • Creates a protected, read-only dummy autorun.inf directory structure that blocks viruses from writing an autorun.inf file.
6. 🔒 Immunize Lab PC:
   • Sets NoDriveTypeAutoRun = 0xFF in Windows policies to completely disable AutoRun/AutoPlay on this PC.

--------------------------------------------------------------------------------
HOW TO USE:
--------------------------------------------------------------------------------
1. Plug in the USB Flash Drive.
2. Select the drive from the dropdown list on the left (or click 'Refresh Drives').
3. Keep the recommended options checked.
4. Click '🚀 Start Scan & Recovery'.
5. Watch live progress in the execution terminal on the right.
6. When finished, click '📂 Open USB Drive' to inspect your recovered files!";
            }

            return @"================================================================================
🦠 6. ແທັບ 'ANTIVIRUS USB' (ກູ້ໄຟລ໌ & ຂ້າໄວຣັສ FLASH DRIVE)
================================================================================

ແທັບສະເພາະສຳລັບກູ້ໄຟລ໌, ຂ້າໄວຣັສ Shortcut, ລຶບໄຟລ໌ປອມ ແລະ ສ້າງວັກຊີນປ້ອງກັນ Flash Drive ພ້ອມປິດ Windows AutoRun ໃຫ້ຄອມພິວເຕີຫ້ອງຮຽນ.

--------------------------------------------------------------------------------
ຄວາມສາມາດຫຼັກຂອງລະບົບ:
--------------------------------------------------------------------------------
1. 📂 ປົດລັອກ ແລະ ກູ້ໄຟລ໌/ໂຟນເດີທີ່ຖືກເຊື່ອງຄືນມາ (Unhide):
   • ໄວຣັສ Shortcut ມັກເຊື່ອງໂຟນເດີແທ້ໄວ້ດ້ວຍຄຸນສົມບັດ Hidden + System + ReadOnly
   • ລະບົບຈະປົດລັອກຄືນທຸກໄຟລ໌ ແລະ ໂຟນເດີຢ່າງປອດໄພ ເຮັດໃຫ້ເອກະສານ ແລະ ວຽກບ້ານປະກົດຂຶ້ນມາທັນທີ
2. 🚫 ລຶບໄຟລ໌ Shortcut (.lnk) ປອມ:
   • ກວດຈັບ ແລະ ລຶບໄຟລ໌ Shortcut ທີ່ປອມຊື່ຄືກັບໂຟນເດີ ແຕ່ແອບແຝງແລ່ນ Script/CMD
3. 🗑️ ລຶບໄຟລ໌ Script ໄວຣັສ:
   • ລຶບໄຟລ໌ .vbs, .js, .wsf ແລະ autorun.inf ປອມທີ່ຢູ່ໃນ Flash Drive
4. 🛑 ປິດ Process ໄວຣັສໃນ Memory:
   • ປິດ wscript.exe ແລະ cscript.exe ທີ່ກຳລັງແລ່ນແອບແຝງໃນ Windows ທັນທີ ບໍ່ໃຫ້ແຜ່ເຊື້ອຊ້ຳ
5. 💉 ສ້າງວັກຊີນປ້ອງກັນ USB (USB Vaccine):
   • ສ້າງໂຟນເດີປ້ອງກັນ autorun.inf ແບບພິເສດ (Protected Folder) ປ້ອງກັນບໍ່ໃຫ້ໄວຣັສຂຽນ autorun.inf ໄດ້ອີກ
6. 🔒 ປິດ Windows AutoRun/AutoPlay ໃນເຄື່ອງ:
   • ຕັ້ງຄ່າ NoDriveTypeAutoRun = 0xFF ເພື່ອປິດ AutoRun ຢ່າງຖາວອນ ປ້ອງກັນເຄື່ອງຫ້ອງຮຽນບໍ່ໃຫ້ຕິດໄວຣັສ USB ອີກ

--------------------------------------------------------------------------------
ຂັ້ນຕອນການນຳໃຊ້ງ່າຍໆ:
--------------------------------------------------------------------------------
1. ສຽບ Flash Drive USB ເຂົ້າເຄື່ອງ.
2. ເລືອກໄດຣຟ໌ USB ຈາກລາຍຊື່ Dropdown ດ້ານຊ້າຍ (ຫຼື ກົດ 'ກວດຫາ').
3. ຕົວເລືອກທັງ 7 ລາຍການຖືກເລືອກໄວ້ເປັນຄ່າແນະນຳແລ້ວ.
4. ກົດປຸ່ມໃຫຍ່ '🚀 ເລີ່ມຕົ້ນກູ້ໄຟລ໌ & ຂ້າໄວຣັສ'.
5. ເບິ່ງຜົນການເຮັດວຽກແບບ Real-Time ໃນໜ້າຈໍ Terminal ດ້ານຂວາ.
6. ເມື່ອສຳເລັດ ກົດປຸ່ມ '📂 ເປີດເບິ່ງໄຟລ໌ໃນ USB' ເພື່ອກວດເບິ່ງເອກະສານທີ່ກູ້ຄືນມາໄດ້ທັນທີ!";
        }

        private string GetDateTimeIconsContent()
        {
            bool isLo = Localizer.CurrentLanguage == "lo";

            if (!isLo)
            {
                return @"================================================================================
⚙️ 7. DATE/TIME FORMAT & DESKTOP ICONS
================================================================================

This section explains how Classroom Checker automatically configures international time/date standards and essential desktop icons for classroom computers.

--------------------------------------------------------------------------------
1. ⏱️ TIMEZONE & DATE FORMAT:
--------------------------------------------------------------------------------
• Timezone: Automatically set to UTC+07:00 (SE Asia Standard Time: Bangkok, Hanoi, Jakarta)
• Date Format: choose dd/MM/yyyy (default), d/M/yyyy, dd-MM-yyyy or yyyy-MM-dd
• Time Format: 24-hour (14:30) or 12-hour (2:30 PM)
• Choose both in ⚙️ Settings → Date & Time tab, with a live preview of the clock
• Applied to the signed-in user and the sign-in screen
• Live Refresh: Windows Taskbar, Clock and Explorer immediately reload without restart

--------------------------------------------------------------------------------
2. 🖥️ SYSTEM DESKTOP ICONS:
--------------------------------------------------------------------------------
• 'This PC': Unhides the native Windows Computer icon on the Desktop
• 'Control Panel': Unhides the native Control Panel icon on the Desktop
• Refreshes Windows Shell so icons appear immediately for all students

--------------------------------------------------------------------------------
3. 📑 MICROSOFT OFFICE SHORTCUTS:
--------------------------------------------------------------------------------
• Automatically detects Word (WINWORD.EXE), Excel (EXCEL.EXE), and PowerPoint (POWERPNT.EXE)
• Creates clean desktop shortcuts in Public Desktop so every student account has access:
  - Word.lnk
  - Excel.lnk
  - PowerPoint.lnk
• If a Word/Excel/PowerPoint shortcut already exists on any desktop (any name), it is not created again

--------------------------------------------------------------------------------
4. 📦 MANUAL SOFTWARE INSTALLATION:
--------------------------------------------------------------------------------
• Place your desired installer setup files (.exe, .msi) in the 'installers/' folder on the USB.
• Administrators can double-click and install them manually according to classroom needs.";
            }

            return @"================================================================================
⚙️ 7. ກຳນົດຮູບແບບວັນທີ & ໄອຄອນໜ້າຈໍ (DATE/TIME & DESKTOP ICONS)
================================================================================

ສ່ວນນີ້ອະທິບາຍລະບົບຕັ້ງຄ່າເວລາ, ວັນທີ່ ແລະ ໄອຄອນໜ້າຈໍທີ່ຈຳເປັນສຳລັບຄອມພິວເຕີຫ້ອງຮຽນ.

--------------------------------------------------------------------------------
1. ⏱️ TIMEZONE ແລະ ຮູບແບບວັນທີ່ (DATE FORMAT):
--------------------------------------------------------------------------------
• Timezone: ຕັ້ງເປັນ UTC+07:00 (SE Asia Standard Time: ບາງກອກ, ຮ່າໂນ້ຍ, ຈາກາຕາ)
• ຮູບແບບວັນທີ: ເລືອກ dd/MM/yyyy (ຄ່າເລີ່ມຕົ້ນ), d/M/yyyy, dd-MM-yyyy ຫຼື yyyy-MM-dd
• ຮູບແບບເວລາ: 24 ຊົ່ວໂມງ (14:30) ຫຼື 12 ຊົ່ວໂມງ (2:30 PM)
• ເລືອກໄດ້ໃນ ⚙️ ຕັ້ງຄ່າ → ແທັບ ວັນທີ & ເວລາ ພ້ອມຕົວຢ່າງໂມງແບບ Real-Time
• ມີຜົນກັບ User ທີ່ Login ຢູ່ ແລະ ໜ້າ Login
• ອັບເດດທັນທີ: Taskbar, ໂມງ ແລະ Windows Explorer ຈະປ່ຽນທັນທີໂດຍບໍ່ຕ້ອງ Restart

--------------------------------------------------------------------------------
2. 🖥️ ໄອຄອນລະບົບເທິງໜ້າຈໍ (SYSTEM DESKTOP ICONS):
--------------------------------------------------------------------------------
• 'This PC': ເປີດສະແດງໄອຄອນ This PC ໄວ້ເທິງໜ້າຈໍ Desktop
• 'Control Panel': ເປີດສະແດງໄອຄອນ Control Panel ໄວ້ເທິງໜ້າຈໍ Desktop
• Refresh Desktop ອັດຕະໂນມັດເພື່ອໃຫ້ໄອຄອນປະກົດຂຶ້ນທັນທີ

--------------------------------------------------------------------------------
3. 📑 ໄອຄອນ MICROSOFT OFFICE:
--------------------------------------------------------------------------------
• ກວດຄົ້ນຫາໄຟລ໌ Word (WINWORD.EXE), Excel (EXCEL.EXE), ແລະ PowerPoint (POWERPNT.EXE)
• ສ້າງ Shortcut ໄວ້ໜ້າຈໍ Public Desktop ເພື່ອໃຫ້ນັກຮຽນທຸກ User ເຫັນ ແລະ ເປີດໃຊ້ໄດ້ສະດວກ:
  - Word.lnk
  - Excel.lnk
  - PowerPoint.lnk
• ຖ້າມີ Shortcut ຂອງ Word/Excel/PowerPoint ຢູ່ໜ້າຈໍແລ້ວ (ຊື່ໃດກໍໄດ້) ລະບົບຈະບໍ່ສ້າງຊ້ຳ

--------------------------------------------------------------------------------
4. 📦 ການຕິດຕັ້ງຊອບແວດ້ວຍຕົນເອງ (MANUAL INSTALLATION):
--------------------------------------------------------------------------------
• ເກັບຮວບຮວມໄຟລ໌ Setup ຂອງໂປຣແກຣມຕ່າງໆ ໄວ້ໃນໂຟນເດີ 'installers/' ໃນ USB.
• ຄູ ຫຼື ຜູ້ດູແລລະບົບສາມາດເປີດໂຟນເດີ installers/ ແລ້ວຕິດຕັ້ງໂປຣແກຣມທີ່ຕ້ອງການໄດ້ດ້ວຍຕົນເອງ.";
        }

        private string GetPerformanceContent()
        {
            bool isLo = Localizer.CurrentLanguage == "lo";
            if (!isLo)
            {
                return @"================================================================================
⚡ 8. LOW-SPEC MODE (4 GB RAM / HDD PCs)
================================================================================

Turns off Windows features that a classroom PC does not need, so old PCs with little RAM
or a hard disk (HDD) start and open programs faster. Works on Windows 10 and 11.

--------------------------------------------------------------------------------
WHAT IT CHANGES:
--------------------------------------------------------------------------------
1. Visual effects: set to 'best performance' (keeps smooth fonts).
2. Background apps: Store apps may not run in the background.
3. Widgets / News & Interests, Copilot, Windows tips and Edge startup boost: turned off.
4. Delivery Optimization: no sharing of Windows updates with other PCs.
5. Services not needed in a lab: Telemetry, Search indexing, Maps, Geolocation, Xbox,
   Error Reporting, Mobile Hotspot, Fax, Retail Demo, WMP Network Sharing.
   SysMain (Superfetch) is turned off only on HDD PCs; SSD PCs keep it.
6. Startup apps: unneeded programs no longer start with Windows.

--------------------------------------------------------------------------------
HOW TO USE (⚙️ Settings → Disk Cleanup tab → Low-spec Mode):
--------------------------------------------------------------------------------
• Automatic (recommended): applied only on PCs with 4 GB RAM or less.
• On for every PC / Off.
• It runs as Quick Setup step 10 and as the 'Low-spec Mode' scan module.
• Restart the PC after applying so the services stop.

--------------------------------------------------------------------------------
UNDO:
--------------------------------------------------------------------------------
• The original values are saved on the PC the first time they are changed
  (C:\ProgramData\ClassroomChecker\performance_backup.json).
• Click '↩ Restore this PC's original settings' to put everything back, then restart.";
            }

            return @"================================================================================
⚡ 8. ໂໝດເຄື່ອງສະເປັກຕ່ຳ (ເຄື່ອງ RAM 4 GB / HDD)
================================================================================

ປິດສ່ວນຂອງ Windows ທີ່ຄອມຫ້ອງຮຽນບໍ່ຈຳເປັນໃຊ້ ເພື່ອໃຫ້ເຄື່ອງເກົ່າທີ່ RAM ນ້ອຍ ຫຼື ໃຊ້ HDD
ເປີດເຄື່ອງ ແລະ ເປີດໂປຣແກຣມໄດ້ໄວຂຶ້ນ. ໃຊ້ໄດ້ທັງ Windows 10 ແລະ 11.

--------------------------------------------------------------------------------
ສິ່ງທີ່ລະບົບປັບ:
--------------------------------------------------------------------------------
1. Visual Effects: ຕັ້ງເປັນ 'ປະສິດທິພາບສູງສຸດ' (ຍັງເກັບຕົວໜັງສືໃຫ້ຄົມຊັດ)
2. Background Apps: ບໍ່ໃຫ້ແອັບ Store ແລ່ນຢູ່ເບື້ອງຫຼັງ
3. ປິດ Widgets / News & Interests, Copilot, ຄຳແນະນຳ Windows ແລະ Edge Startup Boost
4. Delivery Optimization: ບໍ່ແບ່ງປັນ Windows Update ກັບເຄື່ອງອື່ນ
5. ປິດ Service ທີ່ຫ້ອງຄອມບໍ່ໃຊ້: Telemetry, Search Indexing, Maps, Geolocation, Xbox,
   Error Reporting, Mobile Hotspot, Fax, Retail Demo, WMP Network Sharing
   SysMain (Superfetch) ປິດສະເພາະເຄື່ອງ HDD; ເຄື່ອງ SSD ຍັງເປີດໄວ້
6. Startup Apps: ໂປຣແກຣມທີ່ບໍ່ຈຳເປັນຈະບໍ່ເປີດພ້ອມ Windows

--------------------------------------------------------------------------------
ວິທີໃຊ້ (⚙️ ຕັ້ງຄ່າ → ແທັບ ລ້າງໄຟລ໌ຂີ້ເຫຍື້ອ → ໂໝດເຄື່ອງສະເປັກຕ່ຳ):
--------------------------------------------------------------------------------
• ອັດຕະໂນມັດ (ແນະນຳ): ເຮັດສະເພາະເຄື່ອງ RAM 4 GB ລົງມາ
• ເປີດທຸກເຄື່ອງ / ປິດ (ບໍ່ປັບ)
• ເຮັດວຽກໃນ ຕັ້ງຄ່າດ່ວນ ຂັ້ນຕອນ 10 ແລະ ໂມດູນ 'ໂໝດເຄື່ອງສະເປັກຕ່ຳ' ໃນແທັບກວດສອບລະບົບ
• Restart ເຄື່ອງຫຼັງປັບ ເພື່ອໃຫ້ Service ຢຸດເຮັດວຽກ

--------------------------------------------------------------------------------
ຍົກເລີກ / ຄືນຄ່າເດີມ:
--------------------------------------------------------------------------------
• ຄ່າເດີມຖືກບັນທຶກໄວ້ໃນເຄື່ອງຕັ້ງແຕ່ການປ່ຽນຄັ້ງທຳອິດ
  (C:\ProgramData\ClassroomChecker\performance_backup.json)
• ກົດ '↩ ຄືນຄ່າເດີມຂອງເຄື່ອງນີ້' ເພື່ອຄືນທຸກຢ່າງ ແລ້ວ Restart ເຄື່ອງ";
        }

        private string GetStructureContent()
        {
            bool isLo = Localizer.CurrentLanguage == "lo";
            if (!isLo)
            {
                return @"================================================================================
📁 9. USB DIRECTORY STRUCTURE & PERSISTENCE
================================================================================

Classroom Checker is engineered as a true zero-installation portable tool. Everything it needs or generates lives directly in its directory on the USB Flash Drive.

--------------------------------------------------------------------------------
RECOMMENDED USB FLASH DRIVE LAYOUT:
--------------------------------------------------------------------------------
USB_DRIVE:\
  │
  ├── ClassroomChecker.exe      <-- Main portable single executable
  ├── config.json               <-- Application settings & rooms configuration
  ├── bg.png                    <-- Optional custom wallpaper background image
  │
  ├── installers\               <-- Folder where you put setup installers (.exe/.msi)
  │     ├── 7z2408-x64.exe
  │     ├── vlc-3.0.21-win64.exe
  │     ├── BraveBrowserSetup64.exe
  │     ├── RAMMap64.exe
  │     └── ...
  │
  ├── specs\                    <-- Automatically stores exported computer specs (.xlsx)
  │     ├── ຫ້ອງຄອມພິວເຕີ 1-01.xlsx
  │     ├── ຫ້ອງຄອມພິວເຕີ 1-02.xlsx
  │     └── MasterSummary_ຫ້ອງຄອມພິວເຕີ 1.xlsx
  │
  └── reports\                  <-- Stores diagnostic summary reports (.txt)
        ├── ຫ້ອງຄອມພິວເຕີ 1-01_Report.txt
        └── ...

--------------------------------------------------------------------------------
ROBUSTNESS & RESILIENCE:
--------------------------------------------------------------------------------
• Corrupt Config Protection: If config.json is corrupted, it is automatically backed up
  as 'config.json.bad' and healthy defaults are restored without crashing.
• Read-Only USB Fallback: If the USB is locked or read-only, Classroom Checker automatically
  redirects runtime configs to %AppData%\ClassroomChecker\config.json.";
            }

            return @"================================================================================
📁 9. ໂຄງສ້າງໂຟນເດີ USB (DIRECTORY STRUCTURE & PERSISTENCE)
================================================================================

ໂປຣແກຣມ Classroom Checker ເປັນ Portable 100% ບໍ່ຕ້ອງຕິດຕັ້ງ ທຸກຢ່າງທີ່ໂປຣແກຣມຕ້ອງການ ຫຼື ສ້າງຂຶ້ນຈະຖືກເກັບໄວ້ໃນ USB Flash Drive ຢ່າງເປັນລະບຽບຮຽບຮ້ອຍ.

--------------------------------------------------------------------------------
ໂຄງສ້າງໂຟນເດີແນະນຳໃນ USB FLASH DRIVE:
--------------------------------------------------------------------------------
USB_DRIVE:\
  │
  ├── ClassroomChecker.exe      <-- ໄຟລ໌ໂປຣແກຣມຫຼັກ Single Executable
  ├── config.json               <-- ໄຟລ໌ຕັ້ງຄ່າລະບົບ (ຫ້ອງຮຽນ, ນະໂຍບາຍລັອກ, ເລກເຄື່ອງ)
  ├── bg.png                    <-- ຮູບພື້ນຫຼັງ Wallpaper (ສາມາດປ່ຽນໄດ້ຕາມໃຈ)
  │
  ├── installers\               <-- ໂຟນເດີສຳລັບໂຍນໄຟລ໌ Setup ຊອບແວ (.exe/.msi)
  │     ├── 7z2408-x64.exe
  │     ├── vlc-3.0.21-win64.exe
  │     ├── BraveBrowserSetup64.exe
  │     ├── RAMMap64.exe
  │     └── ...
  │
  ├── specs\                    <-- ໂຟນເດີເກັບໄຟລ໌ສະເປັກເຄື່ອງ Excel ຂອງແຕ່ລະເຄື່ອງ
  │     ├── ຫ້ອງຄອມພິວເຕີ 1-01.xlsx
  │     ├── ຫ້ອງຄອມພິວເຕີ 1-02.xlsx
  │     └── MasterSummary_ຫ້ອງຄອມພິວເຕີ 1.xlsx  <-- ໄຟລ໌ສະຫຼຸບຮວມທຸກເຄື່ອງ
  │
  └── reports\                  <-- ໂຟນເດີເກັບໄຟລ໌ລາຍງານຜົນສະແກນ (.txt)
        ├── ຫ້ອງຄອມພິວເຕີ 1-01_Report.txt
        └── ...

--------------------------------------------------------------------------------
ຄວາມທົນທານ ແລະ ຄວາມປອດໄພຂອງລະບົບ:
--------------------------------------------------------------------------------
• ລະບົບປ້ອງກັນໄຟລ໌ Config ເສຍຫາຍ: ຖ້າໄຟລ໌ config.json ຖືກລຶບ ຫຼື ເສຍຫາຍ,
  ໂປຣແກຣມຈະສຳຮອງເປັນ 'config.json.bad' ແລະ ສ້າງຄ່າເລີ່ມຕົ້ນໃຫ້ໃໝ່ທັນທີ ໂດຍບໍ່ Error.
• ຮອງຮັບ USB Read-Only: ຖ້າ Flash Drive ເປັນ Read-Only, ລະບົບຈະບັນທຶກຄ່າລົງທີ່
  %AppData%\ClassroomChecker\config.json ແທນອັດຕະໂນມັດ.";
        }
    }
}
