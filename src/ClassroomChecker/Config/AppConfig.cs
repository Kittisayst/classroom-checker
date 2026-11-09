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

        public SelectedConfig()
        {
            room = "ຫ້ອງ A";
            number = 1;
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
        public LabelConfig label { get; set; }
        public List<string> requiredFonts { get; set; }
        public CleanupConfig cleanup { get; set; }
        public string specOutputDir { get; set; }

        public AppConfig()
        {
            version = 1;
            language = "lo";
            rooms = new List<string> { "ຫ້ອງ A", "ຫ້ອງ B" };
            computerNumberRange = new RangeConfig();
            selected = new SelectedConfig();
            background = new BackgroundConfig();
            label = new LabelConfig();
            requiredFonts = new List<string> { "Phetsarath OT" };
            cleanup = new CleanupConfig();
            specOutputDir = "specs";
        }
    }
}
