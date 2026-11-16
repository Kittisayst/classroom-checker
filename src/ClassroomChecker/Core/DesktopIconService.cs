using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
        public const string GUID_CONTROL_PANEL_1 = "{5399E694-6CE5-4D6C-AC35-41A4388A305A}";
        public const string GUID_CONTROL_PANEL_2 = "{5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}";

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

                        object cp1 = key.GetValue(GUID_CONTROL_PANEL_1);
                        object cp2 = key.GetValue(GUID_CONTROL_PANEL_2);
                        if ((cp1 is int && (int)cp1 == 0) || (cp2 is int && (int)cp2 == 0))
                        {
                            controlPanel = true;
                        }
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
                        key.SetValue(GUID_CONTROL_PANEL_1, 0, RegistryValueKind.DWord);
                        key.SetValue(GUID_CONTROL_PANEL_2, 0, RegistryValueKind.DWord);
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
                            key.SetValue(GUID_CONTROL_PANEL_1, 0, RegistryValueKind.DWord);
                            key.SetValue(GUID_CONTROL_PANEL_2, 0, RegistryValueKind.DWord);
                        }
                    }
                }
                catch { }

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

        /// <summary>
        /// ກວດສອບວ່າ ມີ Shortcut ຂອງ Word, Excel, PowerPoint ຢູ່ໜ້າຈໍແລ້ວ ຫຼື ບໍ່
        /// </summary>
        public static bool AreOfficeShortcutsPresent(out bool hasWord, out bool hasExcel, out bool hasPpt)
        {
            hasWord = false;
            hasExcel = false;
            hasPpt = false;

            var desktopDirs = new List<string>();
            try
            {
                string pub = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
                if (Directory.Exists(pub)) desktopDirs.Add(pub);
            }
            catch { }
            try
            {
                string usr = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (Directory.Exists(usr) && !desktopDirs.Contains(usr)) desktopDirs.Add(usr);
            }
            catch { }

            foreach (var dir in desktopDirs)
            {
                try
                {
                    foreach (var file in Directory.GetFiles(dir, "*.lnk"))
                    {
                        string name = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
                        if (name.Contains("word")) hasWord = true;
                        if (name.Contains("excel")) hasExcel = true;
                        if (name.Contains("powerpoint") || name.Contains("powerpnt")) hasPpt = true;
                    }
                }
                catch { }
            }

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
            catch { }

            if (string.IsNullOrEmpty(targetDir))
            {
                targetDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            if (string.IsNullOrEmpty(targetDir) || !Directory.Exists(targetDir))
            {
                logs.Add("Could not resolve Desktop folder");
                return false;
            }

            var apps = new[]
            {
                new { Exe = "WINWORD.EXE", Lnk = "Word.lnk", Desc = "Microsoft Word" },
                new { Exe = "EXCEL.EXE", Lnk = "Excel.lnk", Desc = "Microsoft Excel" },
                new { Exe = "POWERPNT.EXE", Lnk = "PowerPoint.lnk", Desc = "Microsoft PowerPoint" }
            };

            foreach (var app in apps)
            {
                try
                {
                    string exePath = FindOfficeExe(app.Exe);
                    if (string.IsNullOrEmpty(exePath))
                    {
                        logs.Add(string.Format("Skipped {0}: Executable not found on this machine", app.Desc));
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
