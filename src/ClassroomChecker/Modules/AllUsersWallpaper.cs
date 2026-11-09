using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using ClassroomChecker.Native;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ຕັ້ງ wallpaper ໃຫ້ user ອື່ນໃນເຄື່ອງ (ນອກຈາກ user ທີ່ login ຢູ່) — ເປັນທາງເລືອກ, ເປີດດ້ວຍ
    /// config.json: background.applyToAllUsers = true (ຄ່າເລີ່ມຕົ້ນ false).
    ///
    /// ວິທີ: ຂຽນຄ່າ Control Panel\Desktop (Wallpaper / WallpaperStyle / TileWallpaper) ລົງ registry hive ຂອງ
    ///  - ທຸກ profile ທ້ອງຖິ່ນ (ຖ້າ user ກຳລັງ login → hive ຖືກ load ໃນ HKEY_USERS ຢູ່ແລ້ວ; ຖ້າບໍ່ → reg load ຊົ່ວຄາວ)
    ///  - Default profile (C:\Users\Default\NTUSER.DAT) ເພື່ອໃຫ້ user ໃໝ່ທີ່ຈະຖືກສ້າງໃນອະນາຄົດໄດ້ຮູບນີ້ນຳ
    /// ຂອບເຂດ: ແຕະສະເພາະ 3 ຄ່ານີ້ ແລະ ບໍ່ລຶບຫຍັງ. user ທີ່ login ຢູ່ຈະເຫັນການປ່ຽນຕອນ login ຄັ້ງຕໍ່ໄປ.
    /// ທຸກຂັ້ນຕອນຕ້ອງບໍ່ໂຍນ exception (ຄວາມລົ້ມເຫຼວຂອງ user ໜຶ່ງຫ້າມກະທົບ user ອື່ນ ຫຼື ການຕັ້ງຂອງ user ປັດຈຸບັນ).
    /// </summary>
    public static class AllUsersWallpaper
    {
        private const string DesktopKey = @"Control Panel\Desktop";

        /// <summary>ສົ່ງຄືນຂໍ້ຄວາມສະຫຼຸບ (ຈຳນວນສຳເລັດ/ລົ້ມເຫຼວ) ແລະ ຕື່ມລາຍລະອຽດໃສ່ details</summary>
        public static string Apply(string imagePath, NativeMethods.WallpaperStyle style, List<string> details)
        {
            int ok = 0, failed = 0;
            string styleValue, tileValue;
            GetStyleValues(style, out styleValue, out tileValue);

            // wallpaper ຕ້ອງຢູ່ບ່ອນທີ່ທຸກ user ອ່ານໄດ້ (ProgramData) ບໍ່ດັ່ງນັ້ນ user ອື່ນຈະບໍ່ເຫັນຮູບ
            string common = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            if (!imagePath.StartsWith(common, StringComparison.OrdinalIgnoreCase))
            {
                details.Add("Skipped: image is not in a shared folder (" + imagePath + ")");
                return "ຂ້າມການຕັ້ງໃຫ້ user ອື່ນ: ຮູບບໍ່ໄດ້ຢູ່ໃນໂຟນເດີທີ່ໃຊ້ຮ່ວມກັນ";
            }

            string currentSid = null;
            try { currentSid = System.Security.Principal.WindowsIdentity.GetCurrent().User.Value; } catch { }

            // 1) ທຸກ profile ທ້ອງຖິ່ນ
            foreach (KeyValuePair<string, string> profile in EnumerateLocalProfiles())
            {
                string sid = profile.Key;
                if (string.Equals(sid, currentSid, StringComparison.OrdinalIgnoreCase)) continue; // ຕັ້ງໄປແລ້ວ

                string label = "profile " + profile.Value + " (" + sid + ")";
                bool done = WriteToUser(sid, Path.Combine(profile.Value, "NTUSER.DAT"), imagePath, styleValue, tileValue, details, label);
                if (done) ok++; else failed++;
            }

            // 2) Default profile (user ໃໝ່ໃນອະນາຄົດ)
            string defaultDat = Path.Combine(
                Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)) ?? @"C:\Users",
                "Default", "NTUSER.DAT");
            if (File.Exists(defaultDat))
            {
                bool done = WriteToUser(null, defaultDat, imagePath, styleValue, tileValue, details, "Default profile");
                if (done) ok++; else failed++;
            }
            else
            {
                details.Add("Default profile NTUSER.DAT not found: " + defaultDat);
            }

            return string.Format("ຕັ້ງ wallpaper ໃຫ້ user ອື່ນ: ສຳເລັດ {0}, ລົ້ມເຫຼວ {1}", ok, failed);
        }

        private static void GetStyleValues(NativeMethods.WallpaperStyle style, out string styleValue, out string tileValue)
        {
            tileValue = "0";
            switch (style)
            {
                case NativeMethods.WallpaperStyle.Fit: styleValue = "6"; break;
                case NativeMethods.WallpaperStyle.Stretch: styleValue = "2"; break;
                case NativeMethods.WallpaperStyle.Center: styleValue = "0"; break;
                case NativeMethods.WallpaperStyle.Tile: styleValue = "0"; tileValue = "1"; break;
                default: styleValue = "10"; break; // Fill
            }
        }

        /// <summary>SID → ໂຟນເດີ profile ຂອງ user ທ້ອງຖິ່ນ/domain ປົກກະຕິ (S-1-5-21-*), ຂ້າມ account ລະບົບ</summary>
        private static List<KeyValuePair<string, string>> EnumerateLocalProfiles()
        {
            var list = new List<KeyValuePair<string, string>>();
            try
            {
                using (RegistryKey root = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList"))
                {
                    if (root == null) return list;
                    foreach (string sid in root.GetSubKeyNames())
                    {
                        if (!sid.StartsWith("S-1-5-21-", StringComparison.OrdinalIgnoreCase)) continue;
                        using (RegistryKey k = root.OpenSubKey(sid))
                        {
                            string path = k != null ? k.GetValue("ProfileImagePath") as string : null;
                            if (string.IsNullOrEmpty(path)) continue;
                            path = Environment.ExpandEnvironmentVariables(path);
                            if (Directory.Exists(path)) list.Add(new KeyValuePair<string, string>(sid, path));
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>
        /// ຂຽນຄ່າລົງ hive ຂອງ user ໜຶ່ງຄົນ. sid != null ແລະ hive ຖືກ load ຢູ່ແລ້ວ (user login) → ຂຽນໂດຍກົງ.
        /// ຖ້າບໍ່ → reg load ເຂົ້າ HKEY_USERS\CC_TMP_xxx, ຂຽນ, ແລ້ວ unload ສະເໝີ.
        /// </summary>
        private static bool WriteToUser(string sid, string ntuserDat, string imagePath,
            string styleValue, string tileValue, List<string> details, string label)
        {
            try
            {
                // user ທີ່ login ຢູ່ (ຄົນອື່ນ, ເຊັ່ນ fast user switching) → hive ຖືກ load ແລ້ວ
                if (sid != null)
                {
                    using (RegistryKey loaded = Registry.Users.OpenSubKey(sid))
                    {
                        if (loaded != null)
                        {
                            WriteValues(Registry.Users, sid, imagePath, styleValue, tileValue);
                            details.Add("OK (loaded hive): " + label);
                            return true;
                        }
                    }
                }

                if (!File.Exists(ntuserDat))
                {
                    details.Add("SKIP (no NTUSER.DAT): " + label);
                    return false;
                }

                string temp = "CC_TMP_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                if (RunReg("load \"HKU\\" + temp + "\" \"" + ntuserDat + "\"") != 0)
                {
                    details.Add("FAIL (reg load, hive may be in use): " + label);
                    return false;
                }

                bool wrote = false;
                try
                {
                    WriteValues(Registry.Users, temp, imagePath, styleValue, tileValue);
                    wrote = true;
                }
                finally
                {
                    // ຕ້ອງ unload ສະເໝີ; handle ທີ່ຄ້າງ (GC) ອາດເຮັດໃຫ້ unload ລົ້ມ ຈຶ່ງລອງຊ້ຳ
                    bool unloaded = false;
                    for (int i = 0; i < 5 && !unloaded; i++)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        unloaded = RunReg("unload \"HKU\\" + temp + "\"") == 0;
                        if (!unloaded) System.Threading.Thread.Sleep(300);
                    }
                    if (!unloaded) details.Add("WARNING: could not unload temporary hive HKU\\" + temp);
                }

                details.Add((wrote ? "OK: " : "FAIL: ") + label);
                return wrote;
            }
            catch (Exception ex)
            {
                details.Add("FAIL (" + ex.Message + "): " + label);
                return false;
            }
        }

        private static void WriteValues(RegistryKey hive, string subRoot, string imagePath, string styleValue, string tileValue)
        {
            using (RegistryKey key = hive.CreateSubKey(subRoot + "\\" + DesktopKey))
            {
                if (key == null) throw new InvalidOperationException("cannot open " + DesktopKey);
                key.SetValue("Wallpaper", imagePath, RegistryValueKind.String);
                key.SetValue("WallpaperStyle", styleValue, RegistryValueKind.String);
                key.SetValue("TileWallpaper", tileValue, RegistryValueKind.String);
            }
        }

        private static int RunReg(string arguments)
        {
            var psi = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "reg.exe"), arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (Process p = Process.Start(psi))
            {
                p.StandardOutput.ReadToEnd();
                p.StandardError.ReadToEnd();
                if (!p.WaitForExit(30000))
                {
                    try { p.Kill(); } catch { }
                    return -1;
                }
                return p.ExitCode;
            }
        }
    }
}
