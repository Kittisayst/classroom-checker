using System;
using System.Collections.Generic;
using System.Management;
using ClassroomChecker.Core;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ກວດສອບສະຖານະລິຂະສິດ Windows Activation ຜ່ານ WMI SoftwareLicensingProduct
    /// </summary>
    public class WindowsLicenseCheckModule : ICheckModule
    {
        public string Id
        {
            get { return "windows_license"; }
        }

        public string DisplayNameKey
        {
            get { return "module_windows_license"; }
        }

        public bool CanFix
        {
            get { return false; }
        }

        public CheckResult Check()
        {
            var details = new List<string>();

            try
            {
                string query = "SELECT LicenseStatus, Name, Description FROM SoftwareLicensingProduct WHERE PartialProductKey IS NOT NULL AND Name LIKE 'Windows%'";
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                using (ManagementObjectCollection col = searcher.Get())
                {
                    foreach (ManagementObject mo in col)
                    {
                        uint status = Convert.ToUInt32(mo["LicenseStatus"]);
                        string name = mo["Name"] != null ? mo["Name"].ToString() : "Windows";
                        string desc = mo["Description"] != null ? mo["Description"].ToString() : "";

                        details.Add(string.Format("{0} [{1}] - Status: {2}", name, desc, GetStatusDescription(status)));

                        if (status == 1) // 1 = Licensed
                        {
                            return new CheckResult(CheckStatus.Pass, "Windows ຖືກຕ້ອງຕາມລິຂະສິດ (Activated)", details);
                        }
                        else
                        {
                            return new CheckResult(CheckStatus.Fail, "Windows ຍັງບໍ່ທັນໄດ້ Activate (" + GetStatusDescription(status) + ")", details);
                        }
                    }
                }

                details.Add("ບໍ່ພົບຂໍ້ມູນ License ຈາກ WMI SoftwareLicensingProduct");
                return new CheckResult(CheckStatus.Unknown, "ກວດບໍ່ໄດ້ສະຖານະ Windows License", details);
            }
            catch (Exception ex)
            {
                details.Add("WMI Error: " + ex.Message);
                return new CheckResult(CheckStatus.Unknown, "ເກີດຂໍ້ຜິດພາດໃນການກວດ WMI: " + ex.Message, details);
            }
        }

        public CheckResult Fix()
        {
            return new CheckResult(CheckStatus.Unknown, "ບໍ່ຮອງຮັບການ Activate Windows ອັດຕະໂນມັດ");
        }

        private string GetStatusDescription(uint status)
        {
            switch (status)
            {
                case 0: return "Unlicensed";
                case 1: return "Licensed (Activated)";
                case 2: return "OOB Grace";
                case 3: return "OOT Grace";
                case 4: return "Non-Genuine Grace";
                case 5: return "Notification";
                case 6: return "Extended Grace";
                default: return "Code " + status;
            }
        }
    }
}
