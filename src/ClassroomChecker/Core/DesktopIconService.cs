using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace ClassroomChecker.Core
{
    /// <summary>
    /// ຈັດການການຕັ້ງຄ່າໄອຄອນໜ້າຈໍ Desktop:
    /// - ໄອຄອນລະບົບ Windows: This PC, Control Panel
    /// - ໄອຄອນ MS Office: Word, Excel, PowerPoint
    /// </summary>
    public static class DesktopIconService
    {
        public const string GUID_THIS_PC = "{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
        // CLSID ທີ່ໜ້າ "Desktop icon settings" ຂອງ Windows ໃຊ້ສຳລັບ Control Panel
        public const string GUID_CONTROL_PANEL = "{5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}";
        // CLSID ທີ່ບໍ່ມີຢູ່ຈິງ ເຊິ່ງຮຸ່ນເກົ່າເຄີຍຂຽນໄວ້ → ລຶບອອກ
        private const string GUID_CONTROL_PANEL_INVALID = "{5399E694-6CE5-4D6C-AC35-41A4388A305A}";

        private const string REG_PATH_HIDE_ICONS_NEW = @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel";
        private const string REG_PATH_HIDE_ICONS_CLASSIC = @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\ClassicStartMenu";

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private const uint SHCNE_ASSOCCHANGED = 0x08000000;
        private const uint SHCNF_FLUSHNOWAIT = 0x2000;

        /// <summary>
        /// ກວດສອບວ່າ ໄອຄອນລະບົບ This PC ແລະ Control Panel ເປີດສະແດງຢູ່ໜ້າຈໍ ຫຼື ບໍ່
        /// </summary>
        public static bool AreSystemIconsEnabled(out bool thisPc, out bool controlPanel)
        {
            thisPc = false;
            controlPanel = false;

            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(REG_PATH_HIDE_ICONS_NEW, false))
                {
                    if (key != null)
                    {
                        object pcVal = key.GetValue(GUID_THIS_PC);
                        if (pcVal is int && (int)pcVal == 0) thisPc = true;

                        object cp = key.GetValue(GUID_CONTROL_PANEL);
                        if (cp is int && (int)cp == 0) controlPanel = true;
                    }
                }
            }
            catch { }

            return thisPc && controlPanel;
        }

        /// <summary>
        /// ເປີດໃຫ້ສະແດງໄອຄອນ This PC ແລະ Control Panel ເທິງ Desktop
        /// </summary>
        public static bool EnableSystemIcons(out List<string> logs)
        {
            logs = new List<string>();
            bool success = true;

            try
            {
                // 1. ຂຽນໃສ່ NewStartPanel (Windows 10/11)
                using (var key = Registry.CurrentUser.CreateSubKey(REG_PATH_HIDE_ICONS_NEW))
                {
                    if (key != null)
                    {
                        key.SetValue(GUID_THIS_PC, 0, RegistryValueKind.DWord);
                        key.DeleteValue(GUID_CONTROL_PANEL_INVALID, false);
                        key.SetValue(GUID_CONTROL_PANEL, 0, RegistryValueKind.DWord);
                        logs.Add("Enabled This PC & Control Panel in NewStartPanel");
                    }
                    else success = false;
                }

                // 2. ຂຽນໃສ່ ClassicStartMenu
                try
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(REG_PATH_HIDE_ICONS_CLASSIC))
                    {
                        if (key != null)
                        {
                            key.SetValue(GUID_THIS_PC, 0, RegistryValueKind.DWord);
                            key.DeleteValue(GUID_CONTROL_PANEL_INVALID, false);
                            key.SetValue(GUID_CONTROL_PANEL, 0, RegistryValueKind.DWord);
                        }
                    }
                }
                catch (Exception ex) { ReportService.LogError("DesktopIconService.EnableSystemIcons", ex); }

                // 3. ແຈ້ງ Windows Shell ໃຫ້ Refresh Desktop ທັນທີ
                try
                {
                    SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSHNOWAIT, IntPtr.Zero, IntPtr.Zero);
                    logs.Add("Desktop notified to refresh icons");
                }
                catch { }
            }
            catch (Exception ex)
            {
                logs.Add("Error setting system icons: " + ex.Message);
                success = false;
            }

            return success;
        }

        // =====================================================================
        // MS Office shortcuts
        // =====================================================================

        internal class OfficeApp
        {
            public string Exe;
            public string Lnk;
            public string Desc;
            public string[] NamePrefixes;
        }

        internal static readonly OfficeApp[] OfficeApps =
        {
            new OfficeApp { Exe = "WINWORD.EXE", Lnk = "Word.lnk", Desc = "Microsoft Word", NamePrefixes = new[] { "word", "microsoft word", "microsoft office word" } },
            new OfficeApp { Exe = "EXCEL.EXE", Lnk = "Excel.lnk", Desc = "Microsoft Excel", NamePrefixes = new[] { "excel", "microsoft excel", "microsoft office excel" } },
            new OfficeApp { Exe = "POWERPNT.EXE", Lnk = "PowerPoint.lnk", Desc = "Microsoft PowerPoint", NamePrefixes = new[] { "powerpoint", "microsoft powerpoint", "microsoft office powerpoint" } }
        };

        /// <summary>
        /// ໂຟນເດີ Desktop ທີ່ຕ້ອງກວດ: Public Desktop, Desktop ຂອງ user ທີ່ເປີດໂປຣແກຣມ
        /// ແລະ Desktop ຂອງ user ທີ່ login ຢູ່ໜ້າຈໍ (ອາດຕ່າງກັນເມື່ອຍົກສິດ UAC ດ້ວຍບັນຊີ Admin ອື່ນ)
        /// </summary>
        internal static List<string> GetDesktopDirectories()
        {
            var dirs = new List<string>();
            Action<string> add = d =>
            {
                if (string.IsNullOrEmpty(d) || !Directory.Exists(d)) return;
                foreach (var x in dirs)
                {
                    if (string.Equals(x.TrimEnd('\\'), d.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase)) return;
                }
                dirs.Add(d);
            };

            try { add(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)); } catch { }
            try { add(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)); } catch { }
            try { add(GetInteractiveUserDesktop()); } catch { }
            return dirs;
        }

        private static string GetInteractiveUserDesktop()
        {
            string user = UserAccountService.GetInteractiveLocalUserName();
            if (string.IsNullOrEmpty(user)) return null;

            var sid = (SecurityIdentifier)new NTAccount(Environment.MachineName, user).Translate(typeof(SecurityIdentifier));
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\" + sid.Value, false))
            {
                string profile = key != null ? key.GetValue("ProfileImagePath") as string : null;
                if (string.IsNullOrEmpty(profile)) return null;
                profile = Environment.ExpandEnvironmentVariables(profile);

                string oneDriveDesktop = Path.Combine(profile, "OneDrive", "Desktop");
                return Directory.Exists(oneDriveDesktop) ? oneDriveDesktop : Path.Combine(profile, "Desktop");
            }
        }

        /// <summary>
        /// ຊອກ shortcut ຂອງໂປຣແກຣມນີ້ທີ່ມີຢູ່ແລ້ວ: ກວດຈາກໄຟລ໌ປາຍທາງ (.exe) ຂອງ shortcut ກ່ອນ,
        /// ຖ້າອ່ານປາຍທາງບໍ່ໄດ້ (ເຊັ່ນ shortcut ຂອງ Office ແບບ advertised) ໃຫ້ກວດຈາກຊື່ໄຟລ໌
        /// </summary>
        internal static string FindExistingOfficeShortcut(OfficeApp app, IEnumerable<string> desktopDirs)
        {
            foreach (string dir in desktopDirs)
            {
                string[] files;
                try { files = Directory.GetFiles(dir, "*.lnk"); }
                catch { continue; }

                foreach (string file in files)
                {
                    // Office ແບບ MSI ສ້າງ shortcut ທີ່ປາຍທາງເປັນໄອຄອນໃນ C:\Windows\Installer ຈຶ່ງຕ້ອງກວດຊື່ນຳ
                    string target = ReadShortcutTarget(file);
                    if (!string.IsNullOrEmpty(target)
                        && string.Equals(Path.GetFileName(target), app.Exe, StringComparison.OrdinalIgnoreCase)) return file;
                    if (IsOfficeShortcutName(Path.GetFileNameWithoutExtension(file), app)) return file;
                }
            }
            return null;
        }

        /// <summary>ຊື່ shortcut ຂຶ້ນຕົ້ນດ້ວຍຊື່ໂປຣແກຣມ ("Word", "Word 2016", "Microsoft Word") ແຕ່ບໍ່ແມ່ນ "WordPad"</summary>
        internal static bool IsOfficeShortcutName(string name, OfficeApp app)
        {
            string n = (name ?? string.Empty).Trim().ToLowerInvariant();
            foreach (string prefix in app.NamePrefixes)
            {
                if (n == prefix) return true;
                if (n.StartsWith(prefix) && !char.IsLetter(n[prefix.Length])) return true;
            }
            return false;
        }

        private static string ReadShortcutTarget(string lnkPath)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return null;
                object shell = Activator.CreateInstance(shellType);
                object shortcut = shellType.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { lnkPath });
                return shortcut.GetType().InvokeMember("TargetPath", System.Reflection.BindingFlags.GetProperty, null, shortcut, null) as string;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// ກວດວ່າມີ Shortcut ຂອງ Word, Excel, PowerPoint ຢູ່ Desktop ແລ້ວ ຫຼື ບໍ່
        /// (ໂປຣແກຣມທີ່ບໍ່ໄດ້ຕິດຕັ້ງ ຖືວ່າຜ່ານ)
        /// </summary>
        public static bool AreOfficeShortcutsPresent(out bool hasWord, out bool hasExcel, out bool hasPpt)
        {
            var dirs = GetDesktopDirectories();
            hasWord = FindOfficeExe(OfficeApps[0].Exe) == null || FindExistingOfficeShortcut(OfficeApps[0], dirs) != null;
            hasExcel = FindOfficeExe(OfficeApps[1].Exe) == null || FindExistingOfficeShortcut(OfficeApps[1], dirs) != null;
            hasPpt = FindOfficeExe(OfficeApps[2].Exe) == null || FindExistingOfficeShortcut(OfficeApps[2], dirs) != null;
            return hasWord && hasExcel && hasPpt;
        }

        /// <summary>
        /// ຊອກຫາທີ່ຢູ່ຂອງໄຟລ໌ Office Executable (Word, Excel, PowerPoint)
        /// </summary>
        public static string FindOfficeExe(string exeName)
        {
            if (string.IsNullOrEmpty(exeName)) return null;
            if (!exeName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) exeName += ".exe";

            // 1. App Paths ໃນ Registry
            string[] appPathsKeys = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + exeName,
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\" + exeName
            };

            foreach (var sub in appPathsKeys)
            {
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(sub, false))
                    {
                        if (key != null)
                        {
                            object val = key.GetValue(null); // Default value
                            if (val != null && File.Exists(val.ToString()))
                            {
                                return val.ToString();
                            }
                        }
                    }
                }
                catch { }
            }

            // 2. ຊອກຫາຕາມໂຟນເດີມາດຕະຖານ
            string[] candidates = {
                @"C:\Program Files\Microsoft Office\root\Office16\" + exeName,
                @"C:\Program Files (x86)\Microsoft Office\root\Office16\" + exeName,
                @"C:\Program Files\Microsoft Office\Office16\" + exeName,
                @"C:\Program Files (x86)\Microsoft Office\Office16\" + exeName,
                @"C:\Program Files\Microsoft Office\Office15\" + exeName,
                @"C:\Program Files (x86)\Microsoft Office\Office15\" + exeName,
                @"C:\Program Files\Microsoft Office\Office14\" + exeName,
                @"C:\Program Files (x86)\Microsoft Office\Office14\" + exeName
            };

            foreach (var path in candidates)
            {
                try
                {
                    if (File.Exists(path)) return path;
                }
                catch { }
            }

            return null;
        }

        /// <summary>
        /// ສ້າງ Shortcut ສຳລັບ Word, Excel, PowerPoint ໄວ້ໜ້າຈໍ Desktop
        /// </summary>
        public static bool CreateOfficeShortcuts(out List<string> logs)
        {
            logs = new List<string>();
            bool allOk = true;

            // ເລືອກ Desktop Directory: ພະຍາຍາມໃຊ້ Public Desktop ເພື່ອໃຫ້ທຸກ User ເຫັນ, ຖ້າຂຽນບໍ່ໄດ້ໃຫ້ໃຊ້ User Desktop
            string targetDir = null;
            try
            {
                string pub = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
                if (!string.IsNullOrEmpty(pub) && Directory.Exists(pub))
                {
                    // ທົດສອບສິດຂຽນ
                    string testFile = Path.Combine(pub, "test_perm_" + Guid.NewGuid().ToString("N") + ".tmp");
                    try
                    {
                        File.WriteAllText(testFile, "test");
                        File.Delete(testFile);
                        targetDir = pub;
                    }
                    catch { }
                }
            }
            catch (Exception ex) { ReportService.LogError("DesktopIconService.CreateOfficeShortcuts", ex); }

            if (string.IsNullOrEmpty(targetDir))
            {
                targetDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            if (string.IsNullOrEmpty(targetDir) || !Directory.Exists(targetDir))
            {
                logs.Add("Could not resolve Desktop folder");
                return false;
            }

            var desktopDirs = GetDesktopDirectories();

            foreach (var app in OfficeApps)
            {
                try
                {
                    string exePath = FindOfficeExe(app.Exe);
                    if (string.IsNullOrEmpty(exePath))
                    {
                        logs.Add(string.Format("Skipped {0}: Executable not found on this machine", app.Desc));
                        continue;
                    }

                    // ມີ shortcut ຢູ່ແລ້ວ (Public ຫຼື Desktop ຂອງ user) → ບໍ່ສ້າງຊ້ຳ
                    string existing = FindExistingOfficeShortcut(app, desktopDirs);
                    if (existing != null)
                    {
                        logs.Add(string.Format("Skipped {0}: shortcut already exists ({1})", app.Desc, existing));
                        continue;
                    }

                    string shortcutPath = Path.Combine(targetDir, app.Lnk);
                    bool created = CreateShortcutInternal(shortcutPath, exePath, app.Desc);
                    if (created)
                    {
                        logs.Add(string.Format("Created shortcut: {0} -> {1}", app.Lnk, exePath));
                    }
                    else
                    {
                        logs.Add(string.Format("Failed to create shortcut for {0}", app.Desc));
                        allOk = false;
                    }
                }
                catch (Exception ex)
                {
                    logs.Add(string.Format("Error creating {0}: {1}", app.Desc, ex.Message));
                    allOk = false;
                }
            }

            try
            {
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSHNOWAIT, IntPtr.Zero, IntPtr.Zero);
            }
            catch { }

            return allOk;
        }

        /// <summary>
        /// ສ້າງ Windows .lnk Shortcut ຜ່ານ Windows Script Host Shell COM
        /// </summary>
        public static bool CreateShortcutInternal(string shortcutPath, string targetPath, string description)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return false;

                object shell = Activator.CreateInstance(shellType);
                if (shell == null) return false;

                object shortcut = shellType.InvokeMember(
                    "CreateShortcut",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    shell,
                    new object[] { shortcutPath });

                if (shortcut == null) return false;

                Type shortcutType = shortcut.GetType();
                shortcutType.InvokeMember(
                    "TargetPath",
                    System.Reflection.BindingFlags.SetProperty,
                    null,
                    shortcut,
                    new object[] { targetPath });

                shortcutType.InvokeMember(
                    "WorkingDirectory",
                    System.Reflection.BindingFlags.SetProperty,
                    null,
                    shortcut,
                    new object[] { Path.GetDirectoryName(targetPath) });

                if (!string.IsNullOrEmpty(description))
                {
                    shortcutType.InvokeMember(
                        "Description",
                        System.Reflection.BindingFlags.SetProperty,
                        null,
                        shortcut,
                        new object[] { description });
                }

                shortcutType.InvokeMember(
                    "Save",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    shortcut,
                    null);

                return File.Exists(shortcutPath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ນຳໃຊ້ທັງໄອຄອນລະບົບ (This PC, Control Panel) ແລະ ໄອຄອນ MS Office
        /// </summary>
        public static bool ApplyAll(out List<string> logs)
        {
            logs = new List<string>();

            List<string> sysLogs;
            bool sysOk = EnableSystemIcons(out sysLogs);
            logs.AddRange(sysLogs);

            List<string> officeLogs;
            bool officeOk = CreateOfficeShortcuts(out officeLogs);
            logs.AddRange(officeLogs);

            return sysOk && officeOk;
        }
    }
}
