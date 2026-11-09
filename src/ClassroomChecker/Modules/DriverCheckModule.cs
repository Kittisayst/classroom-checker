using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using ClassroomChecker.Core;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ກວດສອບອຸປະກອນທີ່ຂາດ Driver ຫຼື ມີຂໍ້ຜິດພາດ ຜ່ານ WMI Win32_PnPEntity ຕາມ cm-spec.md ຂໍ້ 5.5
    /// </summary>
    public class DriverCheckModule : ICheckModule
    {
        public string Id
        {
            get { return "driver"; }
        }

        public string DisplayNameKey
        {
            get { return "module_driver"; }
        }

        public bool CanFix
        {
            get { return true; } // Fix = ຕິດຕັ້ງຈາກໂຟນເດີ drivers/ ຂ້າງ exe
        }

        public CheckResult Check()
        {
            var details = new List<string>();
            int missingDriverCount = 0;
            int errorDeviceCount = 0;

            try
            {
                string query = "SELECT Name, DeviceID, ConfigManagerErrorCode FROM Win32_PnPEntity WHERE ConfigManagerErrorCode <> 0";
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                using (ManagementObjectCollection col = searcher.Get())
                {
                    foreach (ManagementObject mo in col)
                    {
                        uint errCode = Convert.ToUInt32(mo["ConfigManagerErrorCode"]);
                        string name = mo["Name"] != null ? mo["Name"].ToString() : "Unknown Device";
                        string deviceId = mo["DeviceID"] != null ? mo["DeviceID"].ToString() : "";

                        string errDesc = GetErrorCodeDescription(errCode);
                        string itemInfo = string.Format("{0} (Code {1}: {2}) [ID: {3}]", name, errCode, errDesc, deviceId);
                        details.Add(itemInfo);

                        if (errCode == 28)
                        {
                            missingDriverCount++;
                        }
                        else
                        {
                            errorDeviceCount++;
                        }
                    }
                }

                if (missingDriverCount == 0 && errorDeviceCount == 0)
                {
                    return new CheckResult(CheckStatus.Pass, "Driver ຄົບຖ້ວນ ບໍ່ພົບອຸປະກອນທີ່ມີບັນຫາ (All Drivers Installed)", details);
                }
                else if (missingDriverCount > 0)
                {
                    string summary = string.Format("ພົບອຸປະກອນທີ່ຂາດ Driver ຈຳນວນ {0} ລາຍການ", missingDriverCount);
                    if (errorDeviceCount > 0)
                    {
                        summary += string.Format(" (ແລະ ພົບຂໍ້ຜິດພາດອື່ນ {0} ລາຍການ)", errorDeviceCount);
                    }
                    return new CheckResult(CheckStatus.Fail, summary, details);
                }
                else
                {
                    string summary = string.Format("ພົບອຸປະກອນທີ່ມີຂໍ້ຜິດພາດ {0} ລາຍການ (Driver Error)", errorDeviceCount);
                    return new CheckResult(CheckStatus.Warning, summary, details);
                }
            }
            catch (Exception ex)
            {
                details.Add("WMI Error: " + ex.Message);
                return new CheckResult(CheckStatus.Unknown, "ເກີດຂໍ້ຜິດພາດໃນການກວດ WMI Driver: " + ex.Message, details);
            }
        }

        public CheckResult Fix()
        {
            // ຕິດຕັ້ງ driver ຈາກໂຟນເດີ drivers/ ຂ້າງ exe (ໃຊ້ໄດ້ offline) ດ້ວຍ pnputil.
            // ໝາຍເຫດ: ບໍ່ parse ຂໍ້ຄວາມ output ຂອງ pnputil (ປ່ຽນຕາມພາສາ Windows) ໃຊ້ exit code ແລະ ກວດຊ້ຳແທນ
            var details = new List<string>();
            try
            {
                string driversDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DriversFolderName);
                if (!Directory.Exists(driversDir) || Directory.GetFiles(driversDir, "*.inf", SearchOption.AllDirectories).Length == 0)
                {
                    details.Add("Expected folder: " + driversDir);
                    return new CheckResult(CheckStatus.Unknown,
                        "ບໍ່ພົບໄຟລ໌ driver (.inf) ໃນໂຟນເດີ " + DriversFolderName + "/ ຂ້າງ exe — ໃສ່ driver ໄວ້ກ່ອນ", details);
                }

                int infCount = Directory.GetFiles(driversDir, "*.inf", SearchOption.AllDirectories).Length;
                details.Add(string.Format("Found {0} .inf file(s) in {1}", infCount, driversDir));

                string pnputil = Path.Combine(Environment.SystemDirectory, "pnputil.exe");
                int exitCode = RunProcess(pnputil,
                    string.Format("/add-driver \"{0}\" /subdirs /install", Path.Combine(driversDir, "*.inf")),
                    details);
                details.Add("pnputil exit code: " + exitCode);

                // 0 = ສຳເລັດ, 3010 = ສຳເລັດ ແຕ່ຕ້ອງ restart
                bool rebootNeeded = exitCode == 3010;
                if (exitCode != 0 && exitCode != 3010)
                {
                    details.Add("pnputil reported an error (some drivers may not match this hardware — this can be normal)");
                }

                // ໃຫ້ Windows ສະແກນອຸປະກອນໃໝ່ (ບໍ່ສຳຄັນຖ້າ Windows ເກົ່າບໍ່ຮອງຮັບ)
                try { RunProcess(pnputil, "/scan-devices", details); } catch { }

                CheckResult after = Check();
                foreach (string d in after.Details) details.Add("After: " + d);

                string note = rebootNeeded ? " (ຕ້ອງ Restart ເຄື່ອງ)" : "";
                if (after.Status == CheckStatus.Pass)
                {
                    return new CheckResult(CheckStatus.Pass, "ຕິດຕັ້ງ Driver ສຳເລັດ — ບໍ່ພົບອຸປະກອນທີ່ມີບັນຫາແລ້ວ" + note, details);
                }

                // ຍັງມີບັນຫາເຫຼືອ: ຖ້າຕ້ອງ restart ໃຫ້ເປັນ Warning (ອາດຫາຍຫຼັງ restart)
                CheckStatus st = rebootNeeded ? CheckStatus.Warning : after.Status;
                return new CheckResult(st, "ຕິດຕັ້ງ Driver ແລ້ວ ແຕ່ຍັງເຫຼືອບັນຫາ: " + after.Summary + note, details);
            }
            catch (Exception ex)
            {
                details.Add("Error: " + ex.Message);
                return new CheckResult(CheckStatus.Fail, "ຕິດຕັ້ງ Driver ບໍ່ສຳເລັດ: " + ex.Message, details);
            }
        }

        private const string DriversFolderName = "drivers";
        private const int PnpUtilTimeoutMs = 10 * 60 * 1000; // driver ຫຼາຍຕົວອາດໃຊ້ເວລາດົນ

        /// <summary>ລັນໂປຣແກຣມ console ແບບບໍ່ມີ window ແລ້ວເກັບ output ໃສ່ details; ສົ່ງຄືນ exit code</summary>
        private static int RunProcess(string fileName, string arguments, List<string> details)
        {
            var psi = new ProcessStartInfo(fileName, arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process p = Process.Start(psi))
            {
                // ອ່ານ output ແບບ async ເພື່ອກັນ buffer ເຕັມແລ້ວ deadlock
                var outTask = p.StandardOutput.ReadToEndAsync();
                var errTask = p.StandardError.ReadToEndAsync();

                if (!p.WaitForExit(PnpUtilTimeoutMs))
                {
                    try { p.Kill(); } catch { }
                    throw new TimeoutException("pnputil timed out after 10 minutes");
                }

                p.WaitForExit(); // ໃຫ້ stream flush ຄົບ
                foreach (string line in (outTask.Result + Environment.NewLine + errTask.Result)
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    details.Add("  " + line.Trim());
                }
                return p.ExitCode;
            }
        }

        private string GetErrorCodeDescription(uint code)
        {
            switch (code)
            {
                case 1: return "Device not configured correctly";
                case 10: return "Device cannot start";
                case 14: return "Device needs restart";
                case 22: return "Device is disabled";
                case 28: return "Drivers for this device are not installed (Missing Driver)";
                case 43: return "Device stopped due to reported problems";
                default: return "ConfigManager error " + code;
            }
        }
    }
}
