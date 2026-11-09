using System;
using System.Collections.Generic;
using System.IO;
using ClassroomChecker.Core;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ກວດພື້ນທີ່ວ່າງຂອງ drive ລະບົບ (ປົກກະຕິ C:). ເຫຼືອ &lt; 10% = Warning, &lt; 5% = Fail.
    /// ກວດຢ່າງດຽວ ບໍ່ແກ້ (ການລ້າງໄຟລ໌ໃຫ້ໃຊ້ CleanupModule).
    /// </summary>
    public class DiskSpaceCheckModule : ICheckModule
    {
        internal const double WarnPercent = 10.0;
        internal const double FailPercent = 5.0;

        internal static CheckStatus EvaluateThreshold(double freePercent)
        {
            if (freePercent < FailPercent) return CheckStatus.Fail;
            if (freePercent < WarnPercent) return CheckStatus.Warning;
            return CheckStatus.Pass;
        }

        public string Id { get { return "disk_space"; } }
        public string DisplayNameKey { get { return "module_disk_space"; } }
        public bool CanFix { get { return false; } }

        public CheckResult Check()
        {
            var details = new List<string>();
            try
            {
                string sysRoot = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
                CheckStatus worst = CheckStatus.Pass;
                string sysSummary = null;

                foreach (DriveInfo d in DriveInfo.GetDrives())
                {
                    try
                    {
                        if (d.DriveType != DriveType.Fixed || !d.IsReady) continue;

                        double pct = d.TotalSize > 0 ? d.AvailableFreeSpace * 100.0 / d.TotalSize : 0;
                        details.Add(string.Format("{0} free {1:F1} GB / {2:F1} GB ({3:F1}%)",
                            d.Name, d.AvailableFreeSpace / 1073741824.0, d.TotalSize / 1073741824.0, pct));

                        // ຕັດສິນສະເພາະ drive ລະບົບ; drive ອື່ນສະແດງໃນລາຍລະອຽດເທົ່ານັ້ນ
                        if (string.Equals(d.Name, sysRoot, StringComparison.OrdinalIgnoreCase))
                        {
                            sysSummary = string.Format("{0} ເຫຼືອ {1:F1} GB ({2:F1}%)",
                                d.Name, d.AvailableFreeSpace / 1073741824.0, pct);
                            worst = EvaluateThreshold(pct);
                        }
                    }
                    catch (Exception ex)
                    {
                        details.Add(d.Name + " error: " + ex.Message);
                    }
                }

                if (sysSummary == null)
                {
                    return new CheckResult(CheckStatus.Unknown, "ກວດພື້ນທີ່ drive ລະບົບບໍ່ໄດ້", details);
                }
                return new CheckResult(worst, sysSummary, details);
            }
            catch (Exception ex)
            {
                details.Add("Error: " + ex.Message);
                return new CheckResult(CheckStatus.Unknown, "ເກີດຂໍ້ຜິດພາດ: " + ex.Message, details);
            }
        }

        public CheckResult Fix()
        {
            return new CheckResult(CheckStatus.Unknown, "ບໍ່ຮອງຮັບການແກ້ໄຂອັດຕະໂນມັດ (ໃຫ້ໃຊ້ ລ້າງໄຟລ໌ Temp)");
        }
    }
}
