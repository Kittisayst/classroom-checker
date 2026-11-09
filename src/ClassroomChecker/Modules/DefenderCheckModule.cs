using System;
using System.Collections.Generic;
using System.Management;
using ClassroomChecker.Core;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ກວດ Windows Defender ແບບ offline ຜ່ານ WMI (root\Microsoft\Windows\Defender, MSFT_MpComputerStatus):
    /// ເປີດ real-time protection ຢູ່ບໍ່ ແລະ signature ເກົ່າເກີນ 30 ວັນບໍ່.
    /// ຖ້າໃຊ້ antivirus ຍີ່ຫໍ້ອື່ນ (namespace ນີ້ບໍ່ມີ) → Unknown ບໍ່ແມ່ນ Fail.
    /// </summary>
    public class DefenderCheckModule : ICheckModule
    {
        private const int MaxSignatureAgeDays = 30;

        public string Id { get { return "defender"; } }
        public string DisplayNameKey { get { return "module_defender"; } }
        public bool CanFix { get { return false; } }

        public CheckResult Check()
        {
            var details = new List<string>();
            try
            {
                var scope = new ManagementScope(@"\\.\root\Microsoft\Windows\Defender");
                scope.Connect();

                using (var searcher = new ManagementObjectSearcher(scope,
                    new ObjectQuery("SELECT * FROM MSFT_MpComputerStatus")))
                using (ManagementObjectCollection col = searcher.Get())
                {
                    foreach (ManagementObject mo in col)
                    {
                        bool av = ReadBool(mo, "AntivirusEnabled");
                        bool rtp = ReadBool(mo, "RealTimeProtectionEnabled");
                        details.Add("AntivirusEnabled: " + av);
                        details.Add("RealTimeProtectionEnabled: " + rtp);

                        DateTime? sigDate = null;
                        object raw = mo["AntivirusSignatureLastUpdated"];
                        if (raw != null)
                        {
                            sigDate = ManagementDateTimeConverter.ToDateTime(raw.ToString());
                            details.Add("SignatureLastUpdated: " + sigDate.Value.ToString("yyyy-MM-dd HH:mm"));
                        }

                        if (!av || !rtp)
                        {
                            return new CheckResult(CheckStatus.Fail, "Windows Defender ຖືກປິດ (Real-time protection OFF)", details);
                        }

                        if (sigDate.HasValue)
                        {
                            int age = (int)(DateTime.Now - sigDate.Value).TotalDays;
                            if (age > MaxSignatureAgeDays)
                            {
                                return new CheckResult(CheckStatus.Warning,
                                    string.Format("Defender ເປີດຢູ່ ແຕ່ definitions ເກົ່າ {0} ວັນ", age), details);
                            }
                            return new CheckResult(CheckStatus.Pass,
                                string.Format("Defender ເປີດຢູ່ (definitions ອາຍຸ {0} ວັນ)", age), details);
                        }

                        return new CheckResult(CheckStatus.Pass, "Defender ເປີດຢູ່", details);
                    }
                }

                return new CheckResult(CheckStatus.Unknown, "ບໍ່ພົບຂໍ້ມູນ Windows Defender", details);
            }
            catch (Exception ex)
            {
                // ມັກເກີດເມື່ອໃຊ້ antivirus ອື່ນ ຫຼື Defender ຖືກຖອດອອກ
                details.Add("WMI Error: " + ex.Message);
                return new CheckResult(CheckStatus.Unknown,
                    "ກວດ Windows Defender ບໍ່ໄດ້ (ອາດໃຊ້ antivirus ອື່ນ)", details);
            }
        }

        public CheckResult Fix()
        {
            return new CheckResult(CheckStatus.Unknown, "ບໍ່ຮອງຮັບການແກ້ໄຂອັດຕະໂນມັດ");
        }

        private static bool ReadBool(ManagementBaseObject mo, string prop)
        {
            object v = mo[prop];
            return v != null && Convert.ToBoolean(v);
        }
    }
}
