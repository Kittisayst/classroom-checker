using System;
using System.Collections.Generic;
using ClassroomChecker.Config;
using ClassroomChecker.Core;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ໂມດູນ "ໂໝດເຄື່ອງສະເປັກຕ່ຳ": ກວດ RAM, ຊະນິດ disk, ພື້ນທີ່ວ່າງ ແລະ ການປັບແຕ່ງ Windows
    /// ທີ່ຊ່ວຍໃຫ້ເຄື່ອງ RAM ນ້ອຍໄວຂຶ້ນ. ໃຊ້ອັດຕະໂນມັດສະເພາະເຄື່ອງ RAM ≤ 4GB (ປ່ຽນໄດ້ໃນຕັ້ງຄ່າ).
    /// </summary>
    public class PerformanceModule : ICheckModule
    {
        public string Id
        {
            get { return "low_spec_performance"; }
        }

        public string DisplayNameKey
        {
            get { return "module_low_spec_performance"; }
        }

        public bool CanFix
        {
            get { return true; }
        }

        private static PerformanceConfig Config()
        {
            var cfg = ConfigService.Instance.Current;
            return (cfg != null && cfg.performance != null) ? cfg.performance : new PerformanceConfig();
        }

        private static string DiskLabel(SystemDiskType t)
        {
            return t == SystemDiskType.Ssd ? "SSD" : t == SystemDiskType.Hdd ? "HDD" : "ບໍ່ຮູ້ຊະນິດ";
        }

        public CheckResult Check()
        {
            var cfg = Config();
            var details = new List<string>();

            double ram = PerformanceTuningService.GetTotalRamGb();
            SystemDiskType disk = PerformanceTuningService.GetSystemDiskType();
            double freePct = PerformanceTuningService.GetSystemDriveFreePercent();

            details.Add(string.Format("RAM: {0:0.0} GB", ram));
            details.Add("Disk Windows: " + DiskLabel(disk));
            details.Add(string.Format("ພື້ນທີ່ວ່າງ Disk Windows: {0:0}%", freePct));
            if (disk == SystemDiskType.Hdd)
            {
                details.Add("💡 ແນະນຳ: ປ່ຽນ HDD ເປັນ SSD ຈະໄວຂຶ້ນຫຼາຍກວ່າການປັບ software ທຸກຢ່າງ");
            }
            if (freePct > 0 && freePct < 15)
            {
                details.Add("⚠ ພື້ນທີ່ວ່າງໜ້ອຍກວ່າ 15% — ໃຊ້ຫົວຂໍ້ ລ້າງໄຟລ໌ Temp ເພື່ອເພີ່ມພື້ນທີ່");
            }

            if (!PerformanceTuningService.IsApplicable(cfg.mode, cfg.autoMaxRamGb, ram))
            {
                details.Add(string.Format("ໂໝດ: {0} — ເຄື່ອງນີ້ບໍ່ຕ້ອງໃຊ້ໂໝດເຄື່ອງສະເປັກຕ່ຳ", cfg.mode));
                return new CheckResult(CheckStatus.Pass,
                    string.Format("RAM {0:0.0} GB, {1} — ບໍ່ຕ້ອງປັບ", ram, DiskLabel(disk)), details);
            }

            details.Add("");
            var missing = new List<string>();
            foreach (var t in PerformanceTuningService.GetStatus(disk))
            {
                details.Add((t.Applied ? "  ✓ " : "  ✗ ") + t.Name + (string.IsNullOrEmpty(t.Detail) ? "" : " — " + t.Detail));
                if (!t.Applied) missing.Add(t.Name);
            }

            if (missing.Count == 0)
            {
                return new CheckResult(CheckStatus.Pass,
                    string.Format("RAM {0:0.0} GB, {1} — ປັບແຕ່ງຄົບແລ້ວ", ram, DiskLabel(disk)), details);
            }

            return new CheckResult(CheckStatus.Warning,
                string.Format("RAM {0:0.0} GB, {1} — ຍັງບໍ່ໄດ້ປັບ {2} ລາຍການ", ram, DiskLabel(disk), missing.Count), details);
        }

        public CheckResult Fix()
        {
            var cfg = Config();
            double ram = PerformanceTuningService.GetTotalRamGb();
            if (!PerformanceTuningService.IsApplicable(cfg.mode, cfg.autoMaxRamGb, ram))
            {
                return new CheckResult(CheckStatus.Pass,
                    string.Format("ຂ້າມ: RAM {0:0.0} GB ບໍ່ຕ້ອງໃຊ້ໂໝດເຄື່ອງສະເປັກຕ່ຳ (ໂໝດ {1})", ram, cfg.mode));
            }

            var logs = new List<string>();
            bool ok = PerformanceTuningService.ApplyAll(PerformanceTuningService.GetSystemDiskType(), logs);
            return new CheckResult(ok ? CheckStatus.Pass : CheckStatus.Warning,
                ok ? "ປັບແຕ່ງເຄື່ອງສະເປັກຕ່ຳສຳເລັດ (Sign out / Restart ເພື່ອໃຫ້ມີຜົນຄົບ)"
                   : "ປັບແຕ່ງບາງລາຍການບໍ່ສຳເລັດ",
                logs);
        }
    }
}
