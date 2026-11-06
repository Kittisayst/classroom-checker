using System;
using System.Collections.Generic;
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
            get { return false; } // CanFix = false ໃນ Phase ນີ້
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
            return new CheckResult(CheckStatus.Unknown, "ການຕິດຕັ້ງ Driver ອັດຕະໂນມັດ ບໍ່ຮອງຮັບໃນເຟສນີ້ (CanFix=false)");
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
