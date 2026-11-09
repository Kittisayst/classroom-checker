using System;
using System.Collections.Generic;
using ClassroomChecker.Config;
using ClassroomChecker.Core;
using ClassroomChecker.I18n;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ໂມດູນກວດສອບ ແລະ ລັອກການ Run ໂປຣແກຣມ, ເກມ ແລະ USB
    /// </summary>
    public class AppLockModule : ICheckModule
    {
        public string Id
        {
            get { return "app_lock"; }
        }

        public string DisplayNameKey
        {
            get { return "module_app_lock"; }
        }

        public bool CanFix
        {
            get { return true; }
        }

        public CheckResult Check()
        {
            var details = new List<string>();
            var cfg = ConfigService.Instance.Current.security;
            if (cfg == null) cfg = new SecurityConfig();

            try
            {
                bool usbBlocked = SecurityPolicyManager.IsUsbExecutionBlocked();
                bool dlBlocked = SecurityPolicyManager.IsDownloadsExecutionBlocked();
                bool disallowActive = SecurityPolicyManager.IsDisallowRunActive();
                int disallowCount = SecurityPolicyManager.GetDisallowedExesCount();

                details.Add("USB Deny Execute: " + (usbBlocked ? "LOCKED (✓)" : "UNLOCKED (✗)"));
                details.Add("Downloads & Temp Safer: " + (dlBlocked ? "LOCKED (✓)" : "UNLOCKED (✗)"));
                details.Add(string.Format("DisallowRun App Lock: {0} ({1} exes)", disallowActive ? "LOCKED (✓)" : "UNLOCKED (✗)", disallowCount));

                // ກວດທຽບກັບ Config ທີ່ຜູ້ໃຊ້ຕັ້ງໄວ້
                bool allPass = true;
                if (cfg.blockUsbExecution && !usbBlocked) allPass = false;
                if (cfg.blockDownloadsExecution && !dlBlocked) allPass = false;
                if (cfg.blockPopularGames && (!disallowActive || disallowCount == 0)) allPass = false;

                if (allPass)
                {
                    return new CheckResult(
                        CheckStatus.Pass,
                        "ເປີດລະບົບລັອກໂປຣແກຣມ, USB ແລະ ເກມແລ້ວ (Protected)",
                        details);
                }
                else
                {
                    return new CheckResult(
                        CheckStatus.Warning,
                        "ຍັງບໍ່ໄດ້ເປີດລະບົບລັອກ (USB/Downloads/ເກມ ຍັງບໍ່ຖືກບລັອກ)",
                        details);
                }
            }
            catch (Exception ex)
            {
                details.Add("Error: " + ex.Message);
                return new CheckResult(CheckStatus.Unknown, "ເກີດຂໍ້ຜິດພາດໃນການກວດສອບ: " + ex.Message, details);
            }
        }

        public CheckResult Fix()
        {
            var cfg = ConfigService.Instance.Current.security;
            if (cfg == null) cfg = new SecurityConfig();

            List<string> logDetails;
            bool success = SecurityPolicyManager.ApplyFullLock(cfg, out logDetails);

            if (success)
            {
                return new CheckResult(
                    CheckStatus.Pass,
                    "ເປີດລະບົບລັອກໂປຣແກຣມ, USB ແລະ ເກມສຳເລັດແລ້ວ",
                    logDetails);
            }
            else
            {
                return new CheckResult(
                    CheckStatus.Warning,
                    "ເປີດລະບົບລັອກບາງລາຍການບໍ່ສຳເລັດ (ອາດຕ້ອງການສິດ Administrator)",
                    logDetails);
            }
        }
    }
}
