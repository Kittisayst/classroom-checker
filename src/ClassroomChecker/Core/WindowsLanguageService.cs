using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace ClassroomChecker.Core
{
    /// <summary>ສະຖານະພາສາສະແດງຜົນຂອງ Windows ໃນເຄື່ອງ</summary>
    public class WindowsLanguageStatus
    {
        /// <summary>ພາສາສະແດງຜົນຂອງ user ທີ່ login ຢູ່ (ເຊັ່ນ en-US, zh-CN); null ຖ້າອ່ານບໍ່ໄດ້</summary>
        public string CurrentLanguage;
        /// <summary>ພາສາອັງກິດທີ່ຕິດຕັ້ງໃນເຄື່ອງ; null ຖ້າບໍ່ມີ</summary>
        public string InstalledEnglish;
        public bool IsLocked;
    }

    /// <summary>
    /// ຕັ້ງພາສາສະແດງຜົນຂອງ Windows ເປັນອັງກິດ ແລະ (ທາງເລືອກ) ລັອກບໍ່ໃຫ້ປ່ຽນ.
    /// ການລັອກໃຊ້ Group Policy ແບບ registry (Globalization.admx, ControlPanel.admx):
    ///   HKLM\SOFTWARE\Policies\Microsoft\MUI\Settings : PreferredUILanguages, MachineUILock
    ///   HKLM\...\Policies\Explorer : SettingsPageVisibility (ຊ່ອນໜ້າ Language ໃນ Settings)
    /// ມີຜົນຫຼັງ Sign out / Sign in. ບໍ່ແຕະລາຍການແປ້ນພິມ ຈຶ່ງຍັງພິມພາສາລາວໄດ້ຄືເກົ່າ.
    /// </summary>
    public static class WindowsLanguageService
    {
        internal const string MuiPolicyPath = @"SOFTWARE\Policies\Microsoft\MUI\Settings";
        internal const string ExplorerPolicyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer";
        internal const string UserDesktopPath = @"Control Panel\Desktop";
        private const string InstalledLanguagesPath = @"SYSTEM\CurrentControlSet\Control\MUI\UILanguages";
        private const string PageVisibilityValue = "SettingsPageVisibility";

        /// <summary>ໜ້າ ms-settings ທີ່ກ່ຽວກັບພາສາສະແດງຜົນ</summary>
        internal static readonly string[] LanguagePages =
        {
            "regionlanguage",
            "regionlanguage-adddisplaylanguage",
            "regionlanguage-setdisplaylanguage",
            "regionlanguage-languageoptions"
        };

        // ------------------------------------------------------------------ logic ລ້ວນໆ

        /// <summary>ເລືອກພາສາອັງກິດຈາກລາຍການທີ່ຕິດຕັ້ງ: en-US ກ່ອນ, ຈາກນັ້ນ en-GB ຫຼື en-* ອື່ນ; ບໍ່ມີ → null</summary>
        public static string FindInstalledEnglish(IEnumerable<string> installed)
        {
            if (installed == null) return null;
            var english = installed.Where(l => l != null && l.StartsWith("en-", StringComparison.OrdinalIgnoreCase)).ToList();
            return english.FirstOrDefault(l => l.Equals("en-US", StringComparison.OrdinalIgnoreCase))
                ?? english.FirstOrDefault(l => l.Equals("en-GB", StringComparison.OrdinalIgnoreCase))
                ?? english.FirstOrDefault();
        }

        /// <summary>
        /// ຄິດຄ່າ SettingsPageVisibility ໃໝ່ ໂດຍຮັກສາໜ້າທີ່ຄົນອື່ນຊ່ອນໄວ້. ຄືນ null = ໃຫ້ລຶບຄ່າອອກ.
        /// ຮູບແບບ: "hide:page1;page2" ຫຼື "showonly:page1;page2"
        /// </summary>
        public static string MergeHiddenPages(string existing, bool hide)
        {
            string value = (existing ?? "").Trim();
            bool showOnly = value.StartsWith("showonly:", StringComparison.OrdinalIgnoreCase);
            int colon = value.IndexOf(':');
            var pages = (colon >= 0 ? value.Substring(colon + 1) : "")
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => p.Length > 0)
                .ToList();

            Func<string, bool> isOurs = p => LanguagePages.Contains(p, StringComparer.OrdinalIgnoreCase);

            if (showOnly)
            {
                // ໂໝດ "ສະແດງສະເພາະ": ການຊ່ອນ = ເອົາໜ້າພາສາອອກຈາກລາຍການ; ການປົດລັອກບໍ່ປ່ຽນຫຍັງ
                if (!hide) return value;
                return "showonly:" + string.Join(";", pages.Where(p => !isOurs(p)));
            }

            if (hide)
            {
                foreach (string p in LanguagePages)
                {
                    if (!pages.Contains(p, StringComparer.OrdinalIgnoreCase)) pages.Add(p);
                }
            }
            else
            {
                pages.RemoveAll(p => isOurs(p));
            }
            return pages.Count == 0 ? null : "hide:" + string.Join(";", pages);
        }

        // ------------------------------------------------------------------ registry

        public static IList<string> GetInstalledLanguages()
        {
            using (var hklm = OpenMachine())
            using (var key = hklm.OpenSubKey(InstalledLanguagesPath))
            {
                return key != null ? key.GetSubKeyNames() : new string[0];
            }
        }

        public static WindowsLanguageStatus GetStatus()
        {
            var status = new WindowsLanguageStatus();
            try
            {
                status.InstalledEnglish = FindInstalledEnglish(GetInstalledLanguages());
                using (var machine = OpenMachine())
                {
                    status.IsLocked = IsLocked(machine);
                }
                using (var user = OpenInteractiveUser(false))
                {
                    status.CurrentLanguage = ReadUserLanguage(user);
                }
            }
            catch (Exception ex)
            {
                ReportService.LogError("WindowsLanguageService.GetStatus", ex);
            }
            return status;
        }

        internal static bool IsLocked(RegistryKey machineRoot)
        {
            using (var key = machineRoot.OpenSubKey(MuiPolicyPath))
            {
                if (key == null) return false;
                object lockValue = key.GetValue("MachineUILock");
                return !string.IsNullOrEmpty(key.GetValue("PreferredUILanguages") as string)
                    && lockValue is int && (int)lockValue == 1;
            }
        }

        internal static string ReadUserLanguage(RegistryKey userRoot)
        {
            if (userRoot == null) return null;
            using (var key = userRoot.OpenSubKey(UserDesktopPath))
            {
                if (key != null)
                {
                    string first = FirstString(key.GetValue("PreferredUILanguages"));
                    if (first != null) return first;
                }
            }
            using (var key = userRoot.OpenSubKey(UserDesktopPath + @"\MuiCached"))
            {
                return key != null ? FirstString(key.GetValue("MachinePreferredUILanguages")) : null;
            }
        }

        private static string FirstString(object value)
        {
            var many = value as string[];
            string s = many != null ? many.FirstOrDefault() : value as string;
            return string.IsNullOrEmpty(s) ? null : s;
        }

        /// <summary>
        /// ຕັ້ງພາສາອັງກິດໃຫ້ user ທີ່ login ຢູ່ ແລະ ລັອກ ຫຼື ປົດລັອກຕາມ lockLanguage.
        /// ໂຍນ InvalidOperationException ຖ້າເຄື່ອງບໍ່ມີພາສາອັງກິດຕິດຕັ້ງ (ບໍ່ປ່ຽນຫຍັງ)
        /// </summary>
        public static string Apply(bool lockLanguage)
        {
            string english = FindInstalledEnglish(GetInstalledLanguages());
            if (english == null)
                throw new InvalidOperationException("English display language is not installed on this PC");

            using (var machine = OpenMachine())
            using (var user = OpenInteractiveUser(true))
            {
                ApplyTo(machine, user, english, lockLanguage);
            }
            ReportService.Log(string.Format("WindowsLanguage: set {0}, locked={1}", english, lockLanguage));
            return english;
        }

        internal static void ApplyTo(RegistryKey machineRoot, RegistryKey userRoot, string language, bool lockLanguage)
        {
            // 1. ພາສາຂອງ user ປັດຈຸບັນ (ແກ້ເຄື່ອງທີ່ຖືກປ່ຽນເປັນພາສາອື່ນໄປແລ້ວ)
            if (userRoot != null)
            {
                using (var key = userRoot.CreateSubKey(UserDesktopPath))
                {
                    key.SetValue("PreferredUILanguages", new[] { language }, RegistryValueKind.MultiString);
                    // ຄ່າທີ່ລໍນຳໃຊ້ຕອນ Sign in ຈະທັບຄ່າເທິງ ຈຶ່ງຕ້ອງລຶບ
                    key.DeleteValue("PreferredUILanguagesPending", false);
                }
            }

            // 2. Policy ລັອກພາສາຂອງທັງເຄື່ອງ
            if (lockLanguage)
            {
                using (var key = machineRoot.CreateSubKey(MuiPolicyPath))
                {
                    key.SetValue("PreferredUILanguages", language, RegistryValueKind.String);
                    key.SetValue("MachineUILock", 1, RegistryValueKind.DWord);
                }
            }
            else
            {
                using (var key = machineRoot.OpenSubKey(MuiPolicyPath, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue("PreferredUILanguages", false);
                        key.DeleteValue("MachineUILock", false);
                    }
                }
            }

            // 3. ຊ່ອນ / ສະແດງໜ້າ Language ໃນ Settings
            using (var key = machineRoot.CreateSubKey(ExplorerPolicyPath))
            {
                string merged = MergeHiddenPages(key.GetValue(PageVisibilityValue) as string, lockLanguage);
                if (merged == null) key.DeleteValue(PageVisibilityValue, false);
                else key.SetValue(PageVisibilityValue, merged, RegistryValueKind.String);
            }
        }

        private static RegistryKey OpenMachine()
        {
            return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        }

        /// <summary>HKU\&lt;SID&gt; ຂອງ user ທີ່ login ຢູ່ໜ້າຈໍ (ບໍ່ແມ່ນ admin ທີ່ elevate); ຫາບໍ່ໄດ້ → HKCU</summary>
        private static RegistryKey OpenInteractiveUser(bool writable)
        {
            string sid = UserAccountService.GetInteractiveUserSid();
            if (!string.IsNullOrEmpty(sid))
            {
                RegistryKey key = Registry.Users.OpenSubKey(sid, writable);
                if (key != null) return key;
            }
            return RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
        }
    }
}
