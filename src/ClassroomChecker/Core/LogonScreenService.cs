using System;
using System.Collections.Generic;
using System.IO;
using ClassroomChecker.Config;
using Microsoft.Win32;

namespace ClassroomChecker.Core
{
    /// <summary>
    /// ຕັ້ງໜ້າ Lock Screen / Welcome (Sign-in), ລັອກການປ່ຽນພື້ນຫຼັງ ແລະ ຂໍ້ຄວາມກ່ອນ Login.
    /// ທຸກຢ່າງເປັນຄ່າລະດັບເຄື່ອງ (HKLM) ຈຶ່ງມີຜົນກັບທຸກ user ແລະ ຕ້ອງໃຊ້ສິດ Admin.
    /// ເມື່ອປິດຕົວເລືອກໃດ ຈະລຶບຄ່າທີ່ເຄີຍຕັ້ງໄວ້ອອກ (ຄືນສະພາບເດີມ).
    /// </summary>
    public static class LogonScreenService
    {
        // Lock screen image (Windows 10 1703+ / 11). ເປັນທາງການສຳລັບ Education/Enterprise, ໃນ Pro ປົກກະຕິໃຊ້ໄດ້
        internal const string PersonalizationCspKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP";

        // DisableAcrylicBackgroundOnLogon (ປິດ blur ໜ້າ Welcome), DisableLogonBackgroundImage (ຖ້າ = 1 ໜ້າ Welcome ຈະເປັນສີລ້ວນ)
        internal const string SystemPoliciesKey = @"SOFTWARE\Policies\Microsoft\Windows\System";

        // NoChangingWallPaper (ຫ້າມປ່ຽນພື້ນຫຼັງ)
        internal const string ActiveDesktopPoliciesKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\ActiveDesktop";

        // legalnoticecaption / legalnoticetext (ຂໍ້ຄວາມກ່ອນ Login)
        internal const string WinlogonPoliciesKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";

        internal const string LockScreenFilePrefix = "lockscreen_";

        /// <summary>
        /// ນຳໃຊ້ທຸກຕົວເລືອກຕາມ config. ຄືນຂໍ້ຄວາມສະຫຼຸບ (ສຳລັບສະແດງ/ບັນທຶກ log). ບໍ່ throw.
        /// </summary>
        public static List<string> Apply(string wallpaperPath, LogonScreenConfig cfg)
        {
            var notes = new List<string>();
            if (cfg == null) cfg = new LogonScreenConfig();

            Run(notes, "✓ ຕັ້ງຮູບໜ້າ Lock / Welcome",
                () => { if (cfg.setLockScreen) SetLockScreenImage(wallpaperPath); else ClearLockScreenImage(); },
                cfg.setLockScreen, "ຕັ້ງຮູບໜ້າ Lock / Welcome");

            Run(notes, "✓ ປິດ blur ໜ້າ Welcome",
                () => SetMachineDword(SystemPoliciesKey, "DisableAcrylicBackgroundOnLogon", cfg.disableLogonBlur ? (int?)1 : null),
                cfg.disableLogonBlur, "ປິດ blur ໜ້າ Welcome");

            Run(notes, "✓ ລັອກບໍ່ໃຫ້ປ່ຽນພື້ນຫຼັງ",
                () => SetMachineDword(ActiveDesktopPoliciesKey, "NoChangingWallPaper", cfg.preventWallpaperChange ? (int?)1 : null),
                cfg.preventWallpaperChange, "ລັອກບໍ່ໃຫ້ປ່ຽນພື້ນຫຼັງ");

            Run(notes, "✓ ຂໍ້ຄວາມກ່ອນ Login",
                () => SetLogonNotice(cfg.showLogonNotice, cfg.noticeTitle, cfg.noticeText),
                cfg.showLogonNotice, "ຂໍ້ຄວາມກ່ອນ Login");

            return notes;
        }

        // ---------------------------------------------------------------- lock screen

        /// <summary>
        /// ຄັດລອກຮູບໄປເປັນໄຟລ໌ຊື່ໃໝ່ທຸກຄັ້ງ (Windows cache ຮູບຕາມ path, ຖ້າໃຊ້ຊື່ເດີມຮູບອາດບໍ່ປ່ຽນ)
        /// ແລ້ວຊີ້ PersonalizationCSP ໄປຫາໄຟລ໌ນັ້ນ
        /// </summary>
        public static void SetLockScreenImage(string wallpaperPath)
        {
            if (string.IsNullOrEmpty(wallpaperPath) || !File.Exists(wallpaperPath))
            {
                throw new FileNotFoundException("ບໍ່ພົບຮູບ wallpaper", wallpaperPath);
            }

            string dir = Path.GetDirectoryName(wallpaperPath);
            string target = Path.Combine(dir, BuildLockScreenFileName(DateTime.Now));
            File.Copy(wallpaperPath, target, true);
            DeleteOldLockScreenFiles(dir, target);

            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(PersonalizationCspKey))
            {
                key.SetValue("LockScreenImagePath", target, RegistryValueKind.String);
                key.SetValue("LockScreenImageUrl", target, RegistryValueKind.String);
                key.SetValue("LockScreenImageStatus", 1, RegistryValueKind.DWord);
            }

            // ຖ້າມີ policy ປິດຮູບໜ້າ Welcome ຢູ່ ໜ້າ Welcome ຈະເປັນສີລ້ວນ ຈຶ່ງຕ້ອງເອົາອອກ
            SetMachineDword(SystemPoliciesKey, "DisableLogonBackgroundImage", null);
        }

        public static void ClearLockScreenImage()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(PersonalizationCspKey, true))
            {
                if (key == null) return;
                // ລຶບສະເພາະຄ່າທີ່ໂປຣແກຣມນີ້ຕັ້ງ (path ຢູ່ໃນໂຟນເດີ ClassroomChecker)
                string current = key.GetValue("LockScreenImagePath") as string;
                if (current == null || current.IndexOf("ClassroomChecker", StringComparison.OrdinalIgnoreCase) < 0) return;

                key.DeleteValue("LockScreenImagePath", false);
                key.DeleteValue("LockScreenImageUrl", false);
                key.DeleteValue("LockScreenImageStatus", false);
            }
        }

        internal static string BuildLockScreenFileName(DateTime now)
        {
            return LockScreenFilePrefix + now.ToString("yyyyMMdd_HHmmss") + ".png";
        }

        internal static void DeleteOldLockScreenFiles(string dir, string keepPath)
        {
            foreach (string f in Directory.GetFiles(dir, LockScreenFilePrefix + "*.png"))
            {
                if (string.Equals(f, keepPath, StringComparison.OrdinalIgnoreCase)) continue;
                try { File.Delete(f); } catch { }
            }
        }

        // ---------------------------------------------------------------- logon notice

        public static void SetLogonNotice(bool enabled, string title, string text)
        {
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(WinlogonPoliciesKey))
            {
                if (enabled && !string.IsNullOrWhiteSpace(text))
                {
                    key.SetValue("legalnoticecaption", (title ?? string.Empty).Trim(), RegistryValueKind.String);
                    key.SetValue("legalnoticetext", NormalizeNoticeText(text), RegistryValueKind.String);
                }
                else
                {
                    // Windows ໃຊ້ຄ່າວ່າງ = ບໍ່ສະແດງ
                    key.SetValue("legalnoticecaption", string.Empty, RegistryValueKind.String);
                    key.SetValue("legalnoticetext", string.Empty, RegistryValueKind.String);
                }
            }
        }

        /// <summary>ແປງການຂຶ້ນແຖວທຸກແບບໃຫ້ເປັນ CRLF ເພື່ອໃຫ້ໜ້າ Login ສະແດງຫຼາຍແຖວໄດ້ຖືກຕ້ອງ</summary>
        internal static string NormalizeNoticeText(string text)
        {
            if (text == null) return string.Empty;
            return text.Replace("\r\n", "\n").Replace("\r", "\n").Trim().Replace("\n", "\r\n");
        }

        // ---------------------------------------------------------------- helpers

        /// <summary>value = null ຄືລຶບຄ່າອອກ</summary>
        private static void SetMachineDword(string subKey, string name, int? value)
        {
            if (value.HasValue)
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(subKey))
                {
                    key.SetValue(name, value.Value, RegistryValueKind.DWord);
                }
            }
            else
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(subKey, true))
                {
                    if (key != null) key.DeleteValue(name, false);
                }
            }
        }

        private static void Run(List<string> notes, string okNote, Action action, bool enabled, string label)
        {
            try
            {
                action();
                if (enabled) notes.Add(okNote);
            }
            catch (Exception ex)
            {
                notes.Add("✗ " + label + ": " + ex.Message);
            }
        }
    }
}
