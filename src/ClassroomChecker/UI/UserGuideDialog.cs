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
                Text = Localizer.T("guide_header_title", "📖 ຄູ່ມືການນຳໃຊ້ Classroom Checker v1.0"),
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
            tabStructure = CreateGuideTab("📁 ໂຄງສ້າງ USB", GetStructureContent());

            tabGuide.TabPages.AddRange(new TabPage[] {
                tabQuickSetup,
                tabWallpaper,
                tabScan,
                tabSecurity,
                tabInstall,
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
6. In a few seconds, the tool automatically executes 6 core administrative actions:
   ✔ Step 1: Render and apply high-resolution Desktop Wallpaper with Room & PC Number label.
   ✔ Step 2: Set Power Scheme to 'Never Sleep' (prevents machines sleeping during class).
   ✔ Step 3: Check and register the national Lao font (Phetsarath OT).
   ✔ Step 4: Check and register Lao Keyboard Layout (0454).
   ✔ Step 5: Clean safe temporary files, user temp, and system cache.
   ✔ Step 6: Export full machine hardware & software specs to an Excel (.xlsx) file.
7. Unplug the USB drive and plug it into the next PC — the computer number is already ready for the next machine!

💡 PRO-TIP: You can configure the starting number and room list in '⚙️ Settings'.";
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
6. ພາຍໃນບໍ່ເທົ່າໃດວິນາທີ ລະບົບຈະດຳເນີນການ 6 ຢ່າງໃຫ້ອັດຕະໂນມັດ:
   ✔ ຂັ້ນຕອນ 1: Render ແລະ ຕັ້ງຮູບພື້ນຫຼັງ Wallpaper ພ້ອມປ້າຍຊື່ຫ້ອງ ແລະ ເລກເຄື່ອງ
   ✔ ຂັ້ນຕອນ 2: ຕັ້ງຄ່າ Power Scheme ເປັນ Never Sleep (ປ້ອງກັນຈໍດັບ/ເຄື່ອງຫຼັບຕອນຮຽນ)
   ✔ ຂັ້ນຕອນ 3: ກວດສອບ ແລະ ລົງທະບຽນ Font ພາສາລາວ (Phetsarath OT)
   ✔ ຂັ້ນຕອນ 4: ກວດສອບ ແລະ ລົງທະບຽນ ແປ້ນພິມພາສາລາວ (Lao Keyboard 0454)
   ✔ ຂັ້ນຕອນ 5: ລ້າງໄຟລ໌ຂີ້ເຫຍື້ອ Temp/Cache ທີ່ປອດໄພ
   ✔ ຂັ້ນຕອນ 6: Export ຂໍ້ມູນສະເປັກເຄື່ອງລະອຽດເປັນໄຟລ໌ Excel (.xlsx) ເກັບໄວ້ໃນ USB
7. ຖອດ USB ໄປສຽບເຄື່ອງຕໍ່ໄປໄດ້ເລີຍ — ເລກເຄື່ອງຈະເພີ່ມຂຶ້ນໃຫ້ພ້ອມສຳລັບເຄື່ອງຕໍ່ໄປ!";
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
• Apply to All Users: Applies the wallpaper registry keys across all user accounts on the machine.";
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
• ຕັ້ງຄ່າໃຫ້ທຸກ User (Apply to All Users): ຕັ້ງຄ່າໃຫ້ທຸກ User Account ໃນ Windows";
        }

        private string GetScanContent()
        {
            bool isLo = Localizer.CurrentLanguage == "lo";
            if (!isLo)
            {
                return @"================================================================================
🔍 3. SYSTEM INSPECTION & SPEC EXPORT TAB
================================================================================

This tab runs 12 deep hardware and software inspection modules and provides one-click automatic fixing.

--------------------------------------------------------------------------------
THE 12 INSPECTION MODULES:
--------------------------------------------------------------------------------
1. MS Office: Checks installation & activation for Word, Excel, PowerPoint.
2. Windows License: Checks Windows genuine activation status via WMI/SoftwareLicensing.
3. Essential Software: Checks presence of Web Browser, 7-Zip/WinRAR, and PDF reader.
4. Font Phetsarath OT: Verifies national Lao font installation (auto-fixes by registering embedded font).
5. Lao Keyboard Layout: Checks Lao keyboard 00000454 (auto-fixes by registering layout).
6. Power Scheme (Never sleep): Verifies AC & DC sleep timeouts are 0s (auto-fixes via powercfg).
7. Temp Files Cleanup: Analyzes temporary files, cache, and Windows temp (auto-fixes safe deletion).
8. Device Drivers: Scans for missing or malfunctioning hardware drivers with yellow exclamation marks.
9. Disk Free Space: Evaluates system drive C: free space (<5% Fail, <10% Warn, >=10% Pass).
10. Windows Defender: Checks antivirus protection and real-time scanning status.
11. Security & App Lock: Checks whether student USB/Downloads/game restrictions are applied.
12. Export Machine Spec: Generates a complete hardware/software audit report into a multi-sheet Excel (.xlsx) file.

--------------------------------------------------------------------------------
ACTION BUTTONS:
--------------------------------------------------------------------------------
• 'ສະແກນ (Scan)': Runs all 12 checks concurrently with live progress reporting.
• 'ແກ້ໄຂທີ່ເລືອກ (Fix)': Automatically fixes all checked issues (Font, Keyboard, Power, Temp, Spec).
• 'ບັນທຶກລາຍງານ (.txt)': Saves a readable diagnostic text report into the reports/ folder.
• '📊 ລວມສະເປັກ (Master Excel)': Merges all individual machine .xlsx specs collected in specs/
  into a single master classroom audit summary workbook!";
            }

            return @"================================================================================
🔍 3. ແທັບ 'ກວດສອບລະບົບ' (SYSTEM INSPECTION)
================================================================================

ແທັບນີ້ໃຊ້ສຳລັບກວດສອບສະພາບຄວາມພ້ອມຂອງເຄື່ອງຄອມພິວເຕີທັງ Hardware ແລະ Software ຄົບທັງ 12 ໂມດູນ ພ້ອມປຸ່ມແກ້ໄຂອັດຕະໂນມັດໃນຄລິກດຽວ.

--------------------------------------------------------------------------------
ລາຍລະອຽດ 12 ໂມດູນທີ່ກວດສອບ:
--------------------------------------------------------------------------------
1. MS Office: ກວດການຕິດຕັ້ງ ແລະ ສະຖານະ Activation ຂອງ Word, Excel, PowerPoint
2. Windows License: ກວດສະຖານະ Windows Activation ວ່າເປັນ License ແທ້ ຫຼື ໝົດອາຍຸ
3. Essential Software: ກວດຊອບແວພື້ນຖານ (Chrome/Brave, 7-Zip, ໂປຣແກຣມອ່ານ PDF)
4. Font Phetsarath OT: ກວດສອບຟອນລາວ (ກົດ Fix ເພື່ອຕິດຕັ້ງຟອນຈາກຕົວໂປຣແກຣມໄດ້ທັນທີ)
5. Lao Keyboard Layout: ກວດແປ້ນພິມພາສາລາວ 0454 (ກົດ Fix ເພື່ອເພີ່ມແປ້ນພິມໃຫ້ທັນທີ)
6. Power Scheme (Never sleep): ກວດການຕັ້ງຄ່າບໍ່ໃຫ້ເຄື່ອງຫຼັບ (ກົດ Fix ຕັ້ງ Never Sleep ທັນທີ)
7. Temp Files Cleanup: ກວດຫາໄຟລ໌ຂີ້ເຫຍື້ອ Temp (ກົດ Fix ເພື່ອລ້າງໄຟລ໌ຂີ້ເຫຍື້ອປອດໄພ)
8. Device Drivers: ກວດສອບໄດຣເວີອຸປະກອນທີ່ມີບັນຫາ (ໄດຣເວີຂາດ ຫຼື ເຄື່ອງໝາຍຕົກໃຈສີເຫຼືອງ)
9. Disk Free Space: ກວດເນື້ອທີ່ວ່າງ Drive C: (<5% Fail, <10% Warn, >=10% Pass)
10. Windows Defender: ກວດສະຖານະ Antivirus ແລະ ການປ້ອງກັນ Real-time
11. Security & App Lock: ກວດເບິ່ງວ່ານະໂຍບາຍບລັອກ USB/Downloads/ເກມ ເປີດຢູ່ຫຼືບໍ່
12. Export Machine Spec: ກວດ ແລະ Export ສະເປັກເຄື່ອງ (CPU, RAM, Mainboard, Storage, GPU, IP, MAC) ເປັນ Excel (.xlsx)

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

--------------------------------------------------------------------------------
ACTION BUTTONS:
--------------------------------------------------------------------------------
• '🔒 ເປີດການລັອກ (Enable Lock)': Applies all selected security restrictions immediately to the machine.
• '🔓 ປິດການລັອກ (Disable Lock)': Removes all security restrictions and restores full default execution rights.
• '🔄 ກວດສອບ (Refresh)': Re-checks the current system lock state.
• '💾 ບັນທຶກ (Save)': Saves the security configuration to config.json.";
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

--------------------------------------------------------------------------------
ປຸ່ມສັ່ງການ:
--------------------------------------------------------------------------------
• '🔒 ເປີດການລັອກ': ເປີດໃຊ້ງານນະໂຍບາຍບລັອກທັນທີ ມີຜົນກັບທຸກ User ໃນເຄື່ອງ
• '🔓 ປິດການລັອກ': ປົດລັອກນະໂຍບາຍທັງໝົດກັບຄືນເປັນປົກກະຕິ
• '🔄 ກວດສອບ': ກວດສອບສະຖານະການລັອກປັດຈຸບັນຂອງເຄື່ອງ
• '💾 ບັນທຶກ': ບັນທຶກລາຍຊື່ເກມ ແລະ ນະໂຍບາຍລົງໄຟລ໌ config.json";
        }

        private string GetInstallContent()
        {
            bool isLo = Localizer.CurrentLanguage == "lo";
            if (!isLo)
            {
                return @"================================================================================
📦 5. OFFLINE SOFTWARE INSTALLER TAB
================================================================================

This tab allows administrators to install 11 essential, open-source, and free software packages silently in unattended mode from the USB drive without needing an internet connection.

--------------------------------------------------------------------------------
HOW TO USE:
--------------------------------------------------------------------------------
1. Download installer files on any computer with internet.
2. Place the setup files into the 'installers/' folder on your USB flash drive.
3. Plug the USB into the classroom PC and open Classroom Checker -> 'ຕິດຕັ້ງຊອບແວ' tab.
4. The table displays live detection status:
   • '✓ ຕິດຕັ້ງແລ້ວ': Package is already installed on the computer.
   • '✗ ຍັງບໍ່ທັນຕິດຕັ້ງ': Package is missing.
   • '✓ ພົບ: <filename>': Installer file was found in the USB installers/ folder.
5. Click '📥 ຕິດຕັ້ງ' on an individual row, or click '⚡ ຕິດຕັ້ງທີ່ຂາດທັງໝົດ'
   to batch-install all missing packages unattended!

--------------------------------------------------------------------------------
THE 11 SUPPORTED PACKAGES:
--------------------------------------------------------------------------------
1. 7-Zip (64-bit): File archiver (LGPL) -> Flags: /S
2. SumatraPDF: Fast, lightweight PDF reader (GPLv3) -> Flags: /S
3. VLC Media Player: Multi-format audio & video player (GPL) -> Flags: /S
4. Brave Browser: Fast, ad-free secure web browser (MPL) -> Flags: /silent /install
5. Google Chrome: Popular offline enterprise browser -> Flags: /silent /install
6. Notepad++: Source code & text editor (GPL) -> Flags: /S
7. LaoScript 8: Lao keyboard, word-wrapping & fonts -> Flags: /VERYSILENT
8. Visual Studio Code: Code editor for IT classrooms -> Flags: /VERYSILENT /NORESTART
9. GIMP: Photo editing & graphic design (GPL) -> Flags: /VERYSILENT /NORESTART
10. ShareX: Screen capture, annotation & video recorder (GPLv3) -> Flags: /VERYSILENT
11. RAMMap64: Sysinternals memory cleaner & standby list clearer (Freeware):
    • PORTABLE DEPLOYMENT: Automatically creates C:\Program Files\RAMMap,
      copies RAMMap64.exe, registers Windows App Paths, auto-accepts EULA,
      and creates a Desktop Shortcut named 'RAMMap (Clear RAM).lnk'!";
            }

            return @"================================================================================
📦 5. ແທັບ 'ຕິດຕັ້ງຊອບແວ' (OFFLINE SOFTWARE INSTALLER)
================================================================================

ແທັບນີ້ຊ່ວຍໃຫ້ຜູ້ດູແລລະບົບສາມາດຕິດຕັ້ງຊອບແວພື້ນຖານ ແລະ Open Source ທີ່ຈຳເປັນລົງໃນເຄື່ອງຫ້ອງຮຽນແບບງຽບໆ (Silent Unattended) ຈາກ USB ໂດຍບໍ່ຕ້ອງເຊື່ອມຕໍ່ອິນເຕີເນັດ ແລະ ບໍ່ຕ້ອງກົດ Next, Next, Finish.

--------------------------------------------------------------------------------
ວິທີການນຳໃຊ້:
--------------------------------------------------------------------------------
1. ດາວໂຫຼດໄຟລ໌ Setup ຂອງໂປຣແກຣມຕ່າງໆ ມາໃສ່ໄວ້ໃນໂຟນເດີ 'installers/' ໃນ USB.
2. ເປີດໂປຣແກຣມ Classroom Checker -> ເຂົ້າແທັບ 'ຕິດຕັ້ງຊອບແວ'.
3. ຕາຕະລາງຈະກວດສອບສະຖານະອັດຕະໂນມັດ:
   • '✓ ຕິດຕັ້ງແລ້ວ': ມີໂປຣແກຣມນີ້ໃນເຄື່ອງແລ້ວ
   • '✗ ຍັງບໍ່ທັນຕິດຕັ້ງ': ເຄື່ອງຍັງຂາດໂປຣແກຣມນີ້
   • '✓ ພົບ: <ຊື່ໄຟລ໌>': ພົບໄຟລ໌ Setup ໃນໂຟນເດີ installers/ ໃນ USB
4. ກົດປຸ່ມ '📥 ຕິດຕັ້ງ' ໃນແຖວທີ່ຕ້ອງການ, ຫຼື ກົດປຸ່ມ '⚡ ຕິດຕັ້ງທີ່ຂາດທັງໝົດ'
   ເພື່ອໃຫ້ລະບົບຕິດຕັ້ງທຸກໂປຣແກຣມທີ່ຍັງຂາດໃຫ້ອັດຕະໂນມັດໃນຄລິກດຽວ!

--------------------------------------------------------------------------------
ລາຍຊື່ 11 ໂປຣແກຣມທີ່ຮອງຮັບ:
--------------------------------------------------------------------------------
1. 7-Zip (64-bit): ໂປຣແກຣມບີບອັດ/ແຕກໄຟລ໌ (LGPL) -> Flag: /S
2. SumatraPDF: ໂປຣແກຣມເປີດອ່ານໄຟລ໌ PDF ໄວ ແລະ ເບົາ (GPLv3) -> Flag: /S
3. VLC Media Player: ເຄື່ອງຫຼິ້ນສື່ວິດີໂອ/ສຽງ ຄົບທຸກ Format (GPL) -> Flag: /S
4. Brave Browser: ເວັບບຣາວເຊີໄວ ແລະ ປອດໄພ ບລັອກໂຄສະນາ (MPL) -> Flag: /silent /install
5. Google Chrome: ເວັບບຣາວເຊີຍອດນິຍົມ -> Flag: /silent /install
6. Notepad++: ໂປຣແກຣມແກ້ໄຂໂຄ້ດ ແລະ ຂໍ້ຄວາມ (GPL) -> Flag: /S
7. LaoScript 8: ໂປຣແກຣມແປ້ນພິມ ແລະ ຕັດຄຳພາສາລາວ -> Flag: /VERYSILENT
8. Visual Studio Code: ເຄື່ອງມືຂຽນໂປຣແກຣມສຳລັບຫ້ອງຮຽນໄອທີ -> Flag: /VERYSILENT
9. GIMP: ໂປຣແກຣມຕັດຕໍ່ຕົກແຕ່ງຮູບພາບແທນ Photoshop -> Flag: /VERYSILENT
10. ShareX: ໂປຣແກຣມຖ່າຍພາບ/ອັດວິດີໂອໜ້າຈໍ ແລະ ໃສ່ລູກສອນ (GPLv3) -> Flag: /VERYSILENT
11. RAMMap64: ໂປຣແກຣມວິເຄາະ ແລະ Clear RAM (Microsoft Sysinternals):
    • ລະບົບ PORTABLE: ຄັດລອກລົງ C:\Program Files\RAMMap ອັດຕະໂນມັດ,
      ສ້າງ Desktop Shortcut 'RAMMap (Clear RAM).lnk', ລົງທະບຽນ App Paths (Win+R)
      ແລະ ຕັ້ງຄ່າ Auto-Accept EULA ໃຫ້ພ້ອມໃຊ້ງານທັນທີ!";
        }

        private string GetStructureContent()
        {
            bool isLo = Localizer.CurrentLanguage == "lo";
            if (!isLo)
            {
                return @"================================================================================
📁 6. USB DIRECTORY STRUCTURE & PERSISTENCE
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
📁 6. ໂຄງສ້າງໂຟນເດີ USB (DIRECTORY STRUCTURE & PERSISTENCE)
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
