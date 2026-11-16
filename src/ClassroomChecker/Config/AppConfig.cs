using System.Collections.Generic;

namespace ClassroomChecker.Config
{
    public class RangeConfig
    {
        public int min { get; set; }
        public int max { get; set; }

        public RangeConfig()
        {
            min = 1;
            max = 45;
        }
    }

    public class SelectedConfig
    {
        public string room { get; set; }
        public int number { get; set; }
        public string computerName { get; set; }

        public SelectedConfig()
        {
            room = "ຫ້ອງຄອມພິວເຕີ 1";
            number = 1;
            computerName = "";
        }
    }

    public class BackgroundConfig
    {
        public string imagePath { get; set; }
        public string style { get; set; }

        /// <summary>
        /// true = ຕັ້ງ wallpaper ໃຫ້ທຸກ user ໃນເຄື່ອງ (ແກ້ registry hive ຂອງ user ອື່ນ + Default profile).
        /// ຄ່າເລີ່ມຕົ້ນ false ເພາະແຕະຕ້ອງ profile ຂອງຄົນອື່ນ.
        /// </summary>
        public bool applyToAllUsers { get; set; }

        public BackgroundConfig()
        {
            imagePath = "bg.png";
            style = "fill";
            applyToAllUsers = false;
        }
    }

    /// <summary>
    /// ຕັ້ງຄ່າໜ້າ Lock Screen / Welcome (Sign-in) ແລະ ນະໂຍບາຍທີ່ນຳໃຊ້ພ້ອມກັບປຸ່ມ "ກຳນົດພື້ນຫຼັງ"
    /// </summary>
    public class LogonScreenConfig
    {
        /// <summary>ໃຊ້ຮູບ wallpaper ດຽວກັນເປັນໜ້າ Lock Screen ແລະ ໜ້າ Welcome</summary>
        public bool setLockScreen { get; set; }

        /// <summary>ປິດການເຮັດຮູບມົວ (blur) ໃນໜ້າ Welcome ຂອງ Windows 10/11</summary>
        public bool disableLogonBlur { get; set; }

        /// <summary>ຫ້າມນັກຮຽນປ່ຽນຮູບພື້ນຫຼັງ Desktop</summary>
        public bool preventWallpaperChange { get; set; }

        /// <summary>ສະແດງຂໍ້ຄວາມກ່ອນ Login (Legal Notice) ທີ່ຕ້ອງກົດ OK ກ່ອນເຂົ້າລະບົບ</summary>
        public bool showLogonNotice { get; set; }
        public string noticeTitle { get; set; }
        public string noticeText { get; set; }

        public LogonScreenConfig()
        {
            setLockScreen = true;
            disableLogonBlur = true;
            preventWallpaperChange = true;
            showLogonNotice = false;
            noticeTitle = "ກົດລະບຽບຫ້ອງຄອມພິວເຕີ";
            noticeText = "1. ຫ້າມຫຼິ້ນເກມ ແລະ ຫ້າມຕິດຕັ້ງໂປຣແກຣມ\n2. ຫ້າມປ່ຽນການຕັ້ງຄ່າເຄື່ອງ\n3. ບັນທຶກວຽກໄວ້ໃນ USB ຂອງຕົນເອງ\n4. Log off ທຸກຄັ້ງຫຼັງໃຊ້ງານ";
        }
    }

    public class LabelConfig
    {
        public string template { get; set; }
        public string fontFamily { get; set; }
        public int fontSizePx { get; set; }
        public string color { get; set; }
        public bool shadow { get; set; }
        public string position { get; set; }
        public int marginPx { get; set; }

        public LabelConfig()
        {
            template = "{room} - {number}";
            fontFamily = "Phetsarath OT";
            fontSizePx = 72;
            color = "#FFFFFF";
            shadow = true;
            position = "top-right";
            marginPx = 40;
        }
    }

    public class CleanupConfig
    {
        public bool userTemp { get; set; }
        public bool windowsTemp { get; set; }
        public bool recycleBin { get; set; }
        public bool windowsUpdateCache { get; set; }
        public bool browserCache { get; set; }
        public List<string> excludePaths { get; set; }

        public CleanupConfig()
        {
            userTemp = true;
            windowsTemp = true;
            recycleBin = true;
            windowsUpdateCache = true;
            browserCache = false;
            excludePaths = new List<string>();
        }
    }

    public class SecurityConfig
    {
        public bool blockUsbExecution { get; set; }
        public bool blockDownloadsExecution { get; set; }
        public bool blockPopularGames { get; set; }
        public List<string> blockedExes { get; set; }
        public List<string> blockedPaths { get; set; }

        public SecurityConfig()
        {
            blockUsbExecution = true;
            blockDownloadsExecution = true;
            blockPopularGames = true;
            blockedExes = new List<string> {
                "roblox.exe",
                "robloxplayerlauncher.exe",
                "steam.exe",
                "epicgameslauncher.exe",
                "garena.exe",
                "minecraft.exe",
                "discord.exe"
            };
            blockedPaths = new List<string>();
        }
    }

    /// <summary>
    /// ໂຄງສ້າງການຕັ້ງຄ່າທັງໝົດຂອງ config.json ຕາມ cm-spec.md ຂໍ້ 4
    /// </summary>
    public class AppConfig
    {
        public int version { get; set; }
        public string language { get; set; }
        public List<string> rooms { get; set; }
        public RangeConfig computerNumberRange { get; set; }
        public SelectedConfig selected { get; set; }
        public BackgroundConfig background { get; set; }
        public LogonScreenConfig logonScreen { get; set; }
        public LabelConfig label { get; set; }
        public List<string> requiredFonts { get; set; }
        public CleanupConfig cleanup { get; set; }
        public SecurityConfig security { get; set; }
        public string specOutputDir { get; set; }

        public AppConfig()
        {
            version = 1;
            language = "lo";
            rooms = new List<string> {
                "ຫ້ອງຄອມພິວເຕີ 1",
                "ຫ້ອງຄອມພິວເຕີ 2",
                "ຫ້ອງຄອມພິວເຕີ 3",
                "ຫ້ອງຄອມພິວເຕີ 4",
                "ຫ້ອງຄອມພິວເຕີ 5",
                "ຫ້ອງຄອມພິວເຕີ 6",
                "ຫ້ອງຄອມພິວເຕີ 7",
                "ຫ້ອງຄອມພິວເຕີ 8"
            };
            computerNumberRange = new RangeConfig();
            selected = new SelectedConfig();
            background = new BackgroundConfig();
            logonScreen = new LogonScreenConfig();
            label = new LabelConfig();
            requiredFonts = new List<string> { "Phetsarath OT" };
            cleanup = new CleanupConfig();
            security = new SecurityConfig();
            specOutputDir = "specs";
        }
    }
}
