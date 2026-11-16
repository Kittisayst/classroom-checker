using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using ClassroomChecker.Config;
using Microsoft.Win32;

namespace ClassroomChecker.Core
{
    /// <summary>
    /// ຂໍ້ມູນໄດຣຟ໌ USB / Removable Drive
    /// </summary>
    public class UsbDriveItem
    {
        public string DriveLetter { get; set; }
        public string VolumeLabel { get; set; }
        public DriveType DriveType { get; set; }
        public long TotalSizeBytes { get; set; }
        public long FreeSizeBytes { get; set; }

        public string DisplayText
        {
            get
            {
                string label = string.IsNullOrEmpty(VolumeLabel) ? "USB Drive" : VolumeLabel;
                double totalGb = TotalSizeBytes / (1024.0 * 1024.0 * 1024.0);
                double freeGb = FreeSizeBytes / (1024.0 * 1024.0 * 1024.0);
                return string.Format("{0} ({1} - {2:F1} GB ວ່າງ / {3:F1} GB)", DriveLetter, label, freeGb, totalGb);
            }
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }

    /// <summary>
    /// ຕົວເລືອກໃນການກູ້ໄຟລ໌ ແລະ ຂ້າໄວຣັສ USB
    /// </summary>
    public class UsbCleanOptions
    {
        public string TargetPath { get; set; }
        public bool KillMalwareProcesses { get; set; } = true;
        public bool CleanStartupPersistence { get; set; } = true;
        public bool RemoveShortcuts { get; set; } = true;
        public bool RemoveRogueScripts { get; set; } = true;
        public bool DetectFakeFolderExes { get; set; } = true;
        public bool UnhideLegitimateFiles { get; set; } = true;
        public bool CreateUsbVaccine { get; set; } = true;
        public bool ImmunizeHostAutoRun { get; set; } = true;
    }

    /// <summary>
    /// ຜົນການກູ້ໄຟລ໌ ແລະ ຂ້າໄວຣັສ USB
    /// </summary>
    public class UsbCleanResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int KilledProcessesCount { get; set; }
        public int RemovedPersistenceCount { get; set; }
        public int ShortcutsRemovedCount { get; set; }
        public int RogueScriptsRemovedCount { get; set; }
        public int FakeExesRemovedCount { get; set; }
        public int FilesUnhiddenCount { get; set; }
        public int DirectoriesUnhiddenCount { get; set; }
        public bool VaccineCreated { get; set; }
        public bool HostAutoRunDisabled { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> CleanedItems { get; set; } = new List<string>();
    }

    /// <summary>
    /// ບໍລິການກູ້ໄຟລ໌ ແລະ ຂ້າໄວຣັສ USB Shortcut (USB Shortcut Virus Cleaner & Vaccine Service)
    /// ແກ້ໄຂບັນຫາ:
    /// 1. ໄວຣັສເຊື່ອງໂຟນເດີ ແລະ ໄຟລ໌ທັງໝົດດ້ວຍຄຸນສົມບັດ Hidden/System
    /// 2. ໄວຣັສສ້າງ .lnk shortcuts ປອມທີ່ແລ່ນ wscript/cmd/powershell
    /// 3. ໄວຣັສປອມ .exe ຊື່ຄືກັບໂຟນເດີ
    /// 4. ໄວຣັສແລ່ນ wscript.exe ຄ້າງໃນ RAM
    /// 5. ສ້າງວັກຊີນປ້ອງກັນ autorun.inf ແລະ ປິດ Windows AutoRun
    /// </summary>
    public static class UsbVirusCleanerService
    {
        public const string VACCINE_FOLDER_NAME = "autorun.inf";
        public const string VACCINE_SUBFOLDER_NAME = "cc_vaccine.protect";

        /// <summary>
        /// ລາຍຊື່ໄດຣຟ໌ USB / Removable ທັງໝົດທີ່ພົບໃນເຄື່ອງ
        /// </summary>
        public static List<UsbDriveItem> GetRemovableDrives()
        {
            var result = new List<UsbDriveItem>();
            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                foreach (var d in drives)
                {
                    if (!d.IsReady) continue;

                    if (d.DriveType == DriveType.Removable)
                    {
                        result.Add(new UsbDriveItem
                        {
                            DriveLetter = d.RootDirectory.FullName,
                            VolumeLabel = d.VolumeLabel,
                            DriveType = d.DriveType,
                            TotalSizeBytes = d.TotalSize,
                            FreeSizeBytes = d.TotalFreeSpace
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ReportService.Log("GetRemovableDrives error: " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// ປິດ Process ໄວຣັສ script ທີ່ມັກແລ່ນຄ້າງໃນ RAM (wscript.exe, cscript.exe)
        /// </summary>
        public static int KillHostMalwareProcesses(Action<string> logCallback = null)
        {
            int killedCount = 0;
            string[] targetProcessNames = new string[] { "wscript", "cscript" };

            foreach (string procName in targetProcessNames)
            {
                try
                {
                    Process[] procs = Process.GetProcessesByName(procName);
                    foreach (var p in procs)
                    {
                        try
                        {
                            string id = p.Id.ToString();
                            p.Kill();
                            p.WaitForExit(1500);
                            killedCount++;
                            if (logCallback != null)
                            {
                                logCallback(string.Format("✓ ປິດ Process ໄວຣັສ: {0}.exe (PID: {1})", procName, id));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (logCallback != null)
                            {
                                logCallback(string.Format("⚠️ ບໍ່ສາມາດປິດ {0}.exe: {1}", procName, ex.Message));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ReportService.Log("KillHostMalwareProcesses (" + procName + ") error: " + ex.Message);
                }
            }

            return killedCount;
        }

        /// <summary>
        /// ກວດສອບວ່າ Windows AutoRun/AutoPlay ຖືກປິດແລ້ວບໍ່ (NoDriveTypeAutoRun = 0xFF)
        /// </summary>
        public static bool IsHostAutoRunDisabled()
        {
            try
            {
                // ກວດສອບທັງ HKCU ແລະ HKLM
                object valHkcu = Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                    "NoDriveTypeAutoRun", null);

                object valHklm = Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                    "NoDriveTypeAutoRun", null);

                bool hkcuDisabled = false;
                if (valHkcu != null)
                {
                    int intVal = Convert.ToInt32(valHkcu);
                    // 0xFF = 255 (ປິດທຸກ Drive Type), 0x91 / 0x95 (ປິດ Removable)
                    if (intVal == 255 || (intVal & 0x04) != 0) hkcuDisabled = true;
                }

                bool hklmDisabled = false;
                if (valHklm != null)
                {
                    int intVal = Convert.ToInt32(valHklm);
                    if (intVal == 255 || (intVal & 0x04) != 0) hklmDisabled = true;
                }

                return hkcuDisabled || hklmDisabled;
            }
            catch (Exception ex)
            {
                ReportService.Log("IsHostAutoRunDisabled error: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// ສ້າງການປ້ອງກັນໃນ Windows ດ້ວຍການປິດ AutoRun/AutoPlay ຢ່າງສົມບູນ (Immunize Host)
        /// </summary>
        public static bool ImmunizeHostMachine(Action<string> logCallback = null)
        {
            bool success = true;
            try
            {
                // 1. HKCU Explorer Policies -> NoDriveTypeAutoRun = 0xFF (255)
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"))
                {
                    if (key != null)
                    {
                        key.SetValue("NoDriveTypeAutoRun", 0xFF, RegistryValueKind.DWord);
                    }
                }

                // 2. HKCU AutoplayHandlers -> DisableAutoplay = 1
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers"))
                {
                    if (key != null)
                    {
                        key.SetValue("DisableAutoplay", 1, RegistryValueKind.DWord);
                    }
                }

                // 3. HKLM Explorer Policies (ຖ້າມີສິດ Admin)
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer"))
                    {
                        if (key != null)
                        {
                            key.SetValue("NoDriveTypeAutoRun", 0xFF, RegistryValueKind.DWord);
                            key.SetValue("HonorAutorunSetting", 1, RegistryValueKind.DWord);
                        }
                    }
                }
                catch { }

                if (logCallback != null)
                {
                    logCallback("✓ ຕັ້ງຄ່າປິດ Windows AutoRun/AutoPlay (NoDriveTypeAutoRun = 0xFF) ສຳເລັດ");
                }
            }
            catch (Exception ex)
            {
                success = false;
                if (logCallback != null)
                {
                    logCallback("⚠️ ບໍ່ສາມາດຕັ້ງຄ່າ AutoRun: " + ex.Message);
                }
            }
            return success;
        }

        /// <summary>
        /// ກວດສອບ ແລະ ລຶບຄ່າ Startup Persistence ທີ່ໄວຣັສມັກຝັງໄວ້ໃນ HKCU/HKLM Run ແລະ Startup Folder
        /// </summary>
        public static int CleanHostStartupPersistence(Action<string> logCallback = null)
        {
            int cleaned = 0;
            string[] runKeys = new string[]
            {
                @"Software\Microsoft\Windows\CurrentVersion\Run",
                @"Software\Microsoft\Windows\CurrentVersion\RunOnce"
            };

            foreach (string subKeyPath in runKeys)
            {
                // Check HKCU
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(subKeyPath, true))
                    {
                        if (key != null)
                        {
                            foreach (string valName in key.GetValueNames())
                            {
                                string valData = Convert.ToString(key.GetValue(valName));
                                if (IsSuspiciousStartupValue(valData))
                                {
                                    key.DeleteValue(valName, false);
                                    cleaned++;
                                    if (logCallback != null)
                                    {
                                        logCallback(string.Format("✓ ລຶບໄວຣັສ Startup ໃນ Registry: HKCU\\{0}\\{1} -> {2}", subKeyPath, valName, valData));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ReportService.Log("CleanHostStartupPersistence HKCU error: " + ex.Message);
                }

                // Check HKLM
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(subKeyPath, true))
                    {
                        if (key != null)
                        {
                            foreach (string valName in key.GetValueNames())
                            {
                                string valData = Convert.ToString(key.GetValue(valName));
                                if (IsSuspiciousStartupValue(valData))
                                {
                                    key.DeleteValue(valName, false);
                                    cleaned++;
                                    if (logCallback != null)
                                    {
                                        logCallback(string.Format("✓ ລຶບໄວຣັສ Startup ໃນ Registry: HKLM\\{0}\\{1} -> {2}", subKeyPath, valName, valData));
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            // Check User Startup Folder
            try
            {
                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                if (Directory.Exists(startupFolder))
                {
                    string[] files = Directory.GetFiles(startupFolder);
                    foreach (string file in files)
                    {
                        string ext = Path.GetExtension(file).ToLowerInvariant();
                        if (ext == ".vbs" || ext == ".js" || ext == ".wsf")
                        {
                            try
                            {
                                File.Delete(file);
                                cleaned++;
                                if (logCallback != null)
                                {
                                    logCallback(string.Format("✓ ລຶບໄຟລ໌ໄວຣັສໃນ Startup Folder: {0}", Path.GetFileName(file)));
                                }
                            }
                            catch { }
                        }
                        else if (ext == ".lnk")
                        {
                            if (IsMaliciousShortcut(file, out string reason))
                            {
                                try
                                {
                                    File.Delete(file);
                                    cleaned++;
                                    if (logCallback != null)
                                    {
                                        logCallback(string.Format("✓ ລຶບ Shortcut ໄວຣັສໃນ Startup: {0} ({1})", Path.GetFileName(file), reason));
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportService.Log("Clean Startup Folder error: " + ex.Message);
            }

            return cleaned;
        }

        private static bool IsSuspiciousStartupValue(string valData)
        {
            if (string.IsNullOrEmpty(valData)) return false;
            string lower = valData.ToLowerInvariant();

            // ຖ້າແລ່ນ script ຜ່ານ wscript / cscript
            if (lower.Contains("wscript.exe") || lower.Contains("cscript.exe") || lower.Contains("mshta.exe"))
            {
                return true;
            }

            // ຖ້າແລ່ນ .vbs, .js, .wsf ໂດຍກົງ
            if (lower.EndsWith(".vbs") || lower.EndsWith(".js") || lower.EndsWith(".wsf") ||
                lower.Contains(".vbs\"") || lower.Contains(".js\"") || lower.Contains(".wsf\"") ||
                lower.Contains(".vbs ") || lower.Contains(".js ") || lower.Contains(".wsf "))
            {
                return true;
            }

            // ຖ້າແລ່ນ cmd / powershell ທີ່ເອີ້ນ wscript ຫຼື %temp%
            if ((lower.Contains("cmd.exe") || lower.Contains("powershell.exe")) &&
                (lower.Contains("wscript") || lower.Contains("temp\\") || lower.Contains("appdata\\")))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// ກວດສອບວ່າໄຟລ໌ Shortcut (.lnk) ເປັນໄຟລ໌ໄວຣັສ ຫຼື ບໍ່
        /// </summary>
        public static bool IsMaliciousShortcut(string lnkPath, out string reason)
        {
            reason = string.Empty;
            if (string.IsNullOrEmpty(lnkPath) || !File.Exists(lnkPath)) return false;

            try
            {
                // 1. ກວດສອບຜ່ານ COM WScript.Shell
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType != null)
                {
                    object shell = Activator.CreateInstance(shellType);
                    try
                    {
                        object shortcut = shellType.InvokeMember("CreateShortcut",
                            BindingFlags.InvokeMethod, null, shell, new object[] { lnkPath });

                        if (shortcut != null)
                        {
                            Type scType = shortcut.GetType();
                            string targetPath = Convert.ToString(scType.InvokeMember("TargetPath",
                                BindingFlags.GetProperty, null, shortcut, null)) ?? "";
                            string arguments = Convert.ToString(scType.InvokeMember("Arguments",
                                BindingFlags.GetProperty, null, shortcut, null)) ?? "";

                            string combined = (targetPath + " " + arguments).ToLowerInvariant();

                            // ກວດສອບ Target ແລະ Arguments
                            if (combined.Contains("wscript.exe") || combined.Contains("cscript.exe") ||
                                combined.Contains("mshta.exe") || combined.Contains("cmd.exe /c") ||
                                combined.Contains("cmd.exe /k") || combined.Contains("powershell"))
                            {
                                if (combined.Contains(".vbs") || combined.Contains(".js") || combined.Contains("attrib") ||
                                    combined.Contains("start explorer") || combined.Contains("wscript"))
                                {
                                    reason = "Shortcut ແລ່ນຄຳສັ່ງ script/cmd ອັນຕະລາຍ";
                                    return true;
                                }
                            }

                            if (targetPath.ToLowerInvariant().EndsWith(".vbs") || targetPath.ToLowerInvariant().EndsWith(".js"))
                            {
                                reason = "Shortcut ຊີ້ໄປຫາໄຟລ໌ Script";
                                return true;
                            }
                        }
                    }
                    finally
                    {
                        try { System.Runtime.InteropServices.Marshal.ReleaseComObject(shell); } catch { }
                    }
                }
            }
            catch
            {
                // Fallback to binary check
            }

            // 2. Binary Fallback Check ສຳລັບໄຟລ໌ .lnk (ຖ້າ COM ໃຊ້ບໍ່ໄດ້)
            try
            {
                byte[] bytes = File.ReadAllBytes(lnkPath);
                if (bytes.Length > 0 && bytes.Length < 100000) // .lnk ປົກກະຕິມີຂະໜາດນ້ອຍ
                {
                    string contentAscii = Encoding.ASCII.GetString(bytes).ToLowerInvariant();
                    string contentUnicode = Encoding.Unicode.GetString(bytes).ToLowerInvariant();

                    if (contentAscii.Contains("wscript.exe") || contentUnicode.Contains("wscript.exe") ||
                        contentAscii.Contains("cscript.exe") || contentUnicode.Contains("cscript.exe") ||
                        (contentAscii.Contains("cmd.exe") && (contentAscii.Contains(".vbs") || contentAscii.Contains("attrib"))) ||
                        (contentUnicode.Contains("cmd.exe") && (contentUnicode.Contains(".vbs") || contentUnicode.Contains("attrib"))))
                    {
                        reason = "ພົບຄຳສັ່ງ wscript/cmd ໄວຣັສໃນເນື້ອໃນ Shortcut";
                        return true;
                    }
                }
            }
            catch { }

            // 3. ກວດສອບວ່າ shortcut ມີຊື່ດຽວກັນກັບ Folder ໃນ Drive (ຮູບແບບມາດຕະຖານຂອງ Shortcut Virus)
            try
            {
                string dir = Path.GetDirectoryName(lnkPath);
                string baseName = Path.GetFileNameWithoutExtension(lnkPath);
                if (!string.IsNullOrEmpty(dir) && !string.IsNullOrEmpty(baseName))
                {
                    string matchingDir = Path.Combine(dir, baseName);
                    if (Directory.Exists(matchingDir))
                    {
                        // ມີ Directory ຊື່ຄືກັນ! ກວດສອບອີກເທື່ອໜຶ່ງວ່າ Directory ນັ້ນຖືກເຊື່ອງໄວ້ຫຼືບໍ່
                        DirectoryInfo di = new DirectoryInfo(matchingDir);
                        if ((di.Attributes & FileAttributes.Hidden) != 0 || (di.Attributes & FileAttributes.System) != 0)
                        {
                            reason = string.Format("Shortcut ປອມຊື່ດຽວກັນກັບໂຟນເດີທີ່ຖືກເຊື່ອງ '{0}'", baseName);
                            return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// ສ້າງວັກຊີນປ້ອງກັນ autorun.inf ໃນ USB Drive
        /// ຫຼັກການ: ລະບົບ Windows ຈະບໍ່ອະນຸຍາດໃຫ້ສ້າງ File ຊື່ autorun.inf ໄດ້ ຖ້າມີ Folder ຊື່ autorun.inf ຢູ່ແລ້ວ.
        /// ພວກເຮົາສ້າງ Folder 'autorun.inf' ພ້ອມ subfolder ແລະ ຕັ້ງ attribute ເປັນ Read-Only + System + Hidden.
        /// </summary>
        public static bool CreateUsbVaccine(string driveOrFolderPath, Action<string> logCallback = null)
        {
            try
            {
                string targetDir = driveOrFolderPath;
                if (!string.IsNullOrEmpty(targetDir))
                {
                    if (targetDir.Length == 2 && targetDir[1] == ':')
                    {
                        targetDir += "\\";
                    }
                }

                string vaccineDirPath = Path.Combine(targetDir, VACCINE_FOLDER_NAME);

                // ຖ້າມີໄຟລ໌ autorun.inf ຢູ່ແລ້ວ (ອາດເປັນໄວຣັສ), ລຶບໄຟລ໌ນັ້ນອອກກ່ອນ
                if (File.Exists(vaccineDirPath))
                {
                    try
                    {
                        File.SetAttributes(vaccineDirPath, FileAttributes.Normal);
                        File.Delete(vaccineDirPath);
                        if (logCallback != null) logCallback("✓ ລຶບໄຟລ໌ autorun.inf ເກົ່າອອກຮຽບຮ້ອຍ");
                    }
                    catch (Exception ex)
                    {
                        if (logCallback != null) logCallback("⚠️ ບໍ່ສາມາດລຶບໄຟລ໌ autorun.inf ເກົ່າ: " + ex.Message);
                    }
                }

                // ສ້າງໂຟນເດີ autorun.inf
                if (!Directory.Exists(vaccineDirPath))
                {
                    Directory.CreateDirectory(vaccineDirPath);
                }

                // ສ້າງ subfolder ປ້ອງກັນພາຍໃນ
                string protectSubDir = Path.Combine(vaccineDirPath, VACCINE_SUBFOLDER_NAME);
                if (!Directory.Exists(protectSubDir))
                {
                    Directory.CreateDirectory(protectSubDir);
                }

                // ຂຽນໄຟລ໌ຄຳອະທິບາຍພາຍໃນ
                string readmeFile = Path.Combine(vaccineDirPath, "readme_vaccine.txt");
                File.WriteAllText(readmeFile,
                    "Classroom Checker USB Vaccine\r\n" +
                    "ໂຟນເດີນີ້ສ້າງຂຶ້ນເພື່ອປ້ອງກັນໄວຣັສ USB Shortcut ແລະ AutoRun Malware ບໍ່ໃຫ້ສາມາດສ້າງ autorun.inf ໄດ້.\r\n" +
                    "ກະລຸນາຢ່າລຶບໂຟນເດີນີ້ (Do not delete).\r\n",
                    Encoding.UTF8);

                // ຕັ້ງຄຸນສົມບັດ ReadOnly + Hidden + System
                File.SetAttributes(readmeFile, FileAttributes.ReadOnly | FileAttributes.Hidden);
                File.SetAttributes(protectSubDir, FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System);
                File.SetAttributes(vaccineDirPath, FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System);

                if (logCallback != null)
                {
                    logCallback(string.Format("✓ ສ້າງວັກຊີນປ້ອງກັນ USB สำเร็จແລ້ວ: {0}", vaccineDirPath));
                }
                return true;
            }
            catch (Exception ex)
            {
                if (logCallback != null)
                {
                    logCallback("⚠️ ເກີດຂໍ້ຜິດພາດໃນການສ້າງວັກຊີນ: " + ex.Message);
                }
                ReportService.Log("CreateUsbVaccine error: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// ດຳເນີນການກູ້ໄຟລ໌ ແລະ ຂ້າໄວຣັສ USB ຕາມຕົວເລືອກທີ່ກຳນົດ
        /// </summary>
        public static UsbCleanResult CleanAndRestoreDrive(UsbCleanOptions options, Action<string> logCallback = null)
        {
            var result = new UsbCleanResult();

            if (options == null || string.IsNullOrEmpty(options.TargetPath))
            {
                result.Success = false;
                result.Message = "ບໍ່ໄດ້ລະບຸ Drive ຫຼື Folder ທີ່ຕ້ອງການກວດສອບ";
                return result;
            }

            string targetPath = options.TargetPath;
            if (!Directory.Exists(targetPath))
            {
                result.Success = false;
                result.Message = string.Format("ບໍ່ພົບໄດຣຟ໌ ຫຼື ໂຟນເດີ: {0}", targetPath);
                return result;
            }

            if (logCallback != null)
            {
                logCallback(string.Format("🚀 ເລີ່ມຕົ້ນກູ້ໄຟລ໌ ແລະ ຂ້າໄວຣັສສຳລັບ: {0}", targetPath));
            }

            // -----------------------------------------------------------------
            // ຂັ້ນຕອນທີ 1: ປິດ Process ໄວຣັສ wscript.exe / cscript.exe
            // -----------------------------------------------------------------
            if (options.KillMalwareProcesses)
            {
                if (logCallback != null) logCallback("⏳ [1/6] ກຳລັງກວດສອບ ແລະ ປິດ Process ໄວຣັສທີ່ກຳລັງແລ່ນ...");
                result.KilledProcessesCount = KillHostMalwareProcesses(logCallback);
                if (result.KilledProcessesCount == 0 && logCallback != null)
                {
                    logCallback("✓ ບໍ່ພົບ Process wscript/cscript ທີ່ໜ້າສົງໄສກຳລັງແລ່ນ");
                }
            }

            // -----------------------------------------------------------------
            // ຂັ້ນຕອນທີ 2: ລຶບຄ່າ Startup Persistence ໃນ Registry / Startup Folder
            // -----------------------------------------------------------------
            if (options.CleanStartupPersistence)
            {
                if (logCallback != null) logCallback("⏳ [2/6] ກຳລັງກວດສອບ Startup Persistence ໃນ Windows...");
                result.RemovedPersistenceCount = CleanHostStartupPersistence(logCallback);
                if (result.RemovedPersistenceCount == 0 && logCallback != null)
                {
                    logCallback("✓ ບໍ່ພົບຄ່າໄວຣັສຝັງຕົວໃນ Startup Registry");
                }
            }

            // -----------------------------------------------------------------
            // ຂັ້ນຕອນທີ 3: ກວດສອບ ແລະ ລຶບໄຟລ໌ໄວຣັສ Shortcut, Script, Fake Exe ໃນ USB
            // -----------------------------------------------------------------
            if (logCallback != null) logCallback("⏳ [3/6] ກຳລັງສະແກນຫາໄຟລ໌ໄວຣັສ ແລະ Shortcuts ໃນ USB...");
            ScanAndCleanDirectoryFiles(targetPath, options, result, logCallback);

            // -----------------------------------------------------------------
            // ຂັ້ນຕອນທີ 4: ປົດລັອກ ແລະ ກູ້ຄືນໄຟລ໌ທັງໝົດທີ່ຖືກເຊື່ອງ (Unhide)
            // -----------------------------------------------------------------
            if (options.UnhideLegitimateFiles)
            {
                if (logCallback != null) logCallback("⏳ [4/6] ກຳລັງປົດລັອກຄຸນສົມບັດ Hidden/System ເພື່ອກູ້ໄຟລ໌ຄືນ...");
                UnhideFilesAndFoldersRecursively(targetPath, result, logCallback);
            }

            // -----------------------------------------------------------------
            // ຂັ້ນຕອນທີ 5: ສ້າງວັກຊີນປ້ອງກັນໃນ USB
            // -----------------------------------------------------------------
            if (options.CreateUsbVaccine)
            {
                if (logCallback != null) logCallback("⏳ [5/6] ກຳລັງສ້າງວັກຊີນປ້ອງກັນ autorun.inf ໃນ USB...");
                result.VaccineCreated = CreateUsbVaccine(targetPath, logCallback);
            }

            // -----------------------------------------------------------------
            // ຂັ້ນຕອນທີ 6: ປິດ Windows AutoRun/AutoPlay ໃນເຄື່ອງ
            // -----------------------------------------------------------------
            if (options.ImmunizeHostAutoRun)
            {
                if (logCallback != null) logCallback("⏳ [6/6] ກຳລັງຕັ້ງຄ່າປິດ Windows AutoRun/AutoPlay...");
                result.HostAutoRunDisabled = ImmunizeHostMachine(logCallback);
            }

            result.Success = true;
            result.Message = string.Format(
                "ກູ້ໄຟລ໌ສຳເລັດ! ປົດລັອກໄຟລ໌ຄືນໄດ້ {0} ໄຟລ໌ ({1} ໂຟນເດີ), ລຶບ Shortcut ໄວຣັສ {2} ໄຟລ໌, ລຶບ Script {3} ໄຟລ໌, ລຶບ .exe ປອມ {4} ໄຟລ໌",
                result.FilesUnhiddenCount, result.DirectoriesUnhiddenCount,
                result.ShortcutsRemovedCount, result.RogueScriptsRemovedCount, result.FakeExesRemovedCount);

            if (logCallback != null)
            {
                logCallback("--------------------------------------------------");
                logCallback("✨ " + result.Message);
            }

            return result;
        }

        private static void ScanAndCleanDirectoryFiles(string targetDir, UsbCleanOptions options, UsbCleanResult result, Action<string> logCallback)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(targetDir);
                if (!dirInfo.Exists) return;

                // 1. Scan Root Files
                FileInfo[] files = dirInfo.GetFiles("*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    string ext = file.Extension.ToLowerInvariant();
                    string fileName = file.Name.ToLowerInvariant();

                    // ຂ້າມໄຟລ໌ລະບົບແທ້
                    if (fileName == "desktop.ini" || fileName == "thumbs.db") continue;

                    // ກວດສອບ autorun.inf (ຖ້າເປັນໄຟລ໌)
                    if (fileName == "autorun.inf" && options.RemoveRogueScripts)
                    {
                        try
                        {
                            File.SetAttributes(file.FullName, FileAttributes.Normal);
                            file.Delete();
                            result.RogueScriptsRemovedCount++;
                            result.CleanedItems.Add(file.FullName);
                            if (logCallback != null) logCallback("✓ ລຶບໄຟລ໌ໄວຣັສ: autorun.inf");
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add("autorun.inf: " + ex.Message);
                        }
                        continue;
                    }

                    // ກວດສອບໄຟລ໌ .vbs, .js, .wsf ໃນ root
                    if ((ext == ".vbs" || ext == ".js" || ext == ".wsf") && options.RemoveRogueScripts)
                    {
                        try
                        {
                            File.SetAttributes(file.FullName, FileAttributes.Normal);
                            file.Delete();
                            result.RogueScriptsRemovedCount++;
                            result.CleanedItems.Add(file.FullName);
                            if (logCallback != null) logCallback("✓ ລຶບໄຟລ໌ script ໄວຣັສ: " + file.Name);
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(file.Name + ": " + ex.Message);
                        }
                        continue;
                    }

                    // ກວດສອບ .lnk Shortcuts
                    if (ext == ".lnk" && options.RemoveShortcuts)
                    {
                        if (IsMaliciousShortcut(file.FullName, out string reason))
                        {
                            try
                            {
                                File.SetAttributes(file.FullName, FileAttributes.Normal);
                                file.Delete();
                                result.ShortcutsRemovedCount++;
                                result.CleanedItems.Add(file.FullName);
                                if (logCallback != null)
                                {
                                    logCallback(string.Format("✓ ລຶບ Shortcut ໄວຣັສ: {0} ({1})", file.Name, reason));
                                }
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add(file.Name + ": " + ex.Message);
                            }
                            continue;
                        }
                    }

                    // ກວດສອບ Fake Folder .exe (ຕົວຢ່າງ: Photos.exe ເມື່ອມີໂຟນເດີ Photos)
                    if (ext == ".exe" && options.DetectFakeFolderExes)
                    {
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(file.FullName);
                        string matchingFolder = Path.Combine(dirInfo.FullName, nameWithoutExt);

                        bool isFakeFolder = false;
                        if (Directory.Exists(matchingFolder))
                        {
                            isFakeFolder = true;
                        }
                        else if (nameWithoutExt.Equals("New Folder", StringComparison.OrdinalIgnoreCase) ||
                                 nameWithoutExt.Equals("Documents", StringComparison.OrdinalIgnoreCase) ||
                                 nameWithoutExt.Equals("Photos", StringComparison.OrdinalIgnoreCase) ||
                                 nameWithoutExt.Equals("USB Drive", StringComparison.OrdinalIgnoreCase))
                        {
                            isFakeFolder = true;
                        }

                        if (isFakeFolder)
                        {
                            try
                            {
                                File.SetAttributes(file.FullName, FileAttributes.Normal);
                                file.Delete();
                                result.FakeExesRemovedCount++;
                                result.CleanedItems.Add(file.FullName);
                                if (logCallback != null)
                                {
                                    logCallback(string.Format("✓ ລຶບໄຟລ໌ .exe ປອມທີ່ປອມເປັນໂຟນເດີ: {0}", file.Name));
                                }
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add(file.Name + ": " + ex.Message);
                            }
                            continue;
                        }
                    }
                }

                // 2. Scan Subdirectories for Rogue Scripts & Fake Shortcuts (Up to 2 levels)
                DirectoryInfo[] subDirs = dirInfo.GetDirectories();
                foreach (var subDir in subDirs)
                {
                    string dirName = subDir.Name.ToLowerInvariant();
                    if (dirName == "$recycle.bin" || dirName == "system volume information" || dirName == VACCINE_FOLDER_NAME)
                    {
                        continue;
                    }

                    // ກວດສອບໂຟນເດີແປກປອມທີ່ໄວຣັສມັກສ້າງເຊັ່ນ: "__", "indexer", "drive"
                    try
                    {
                        FileInfo[] subFiles = subDir.GetFiles();
                        foreach (var subFile in subFiles)
                        {
                            string subExt = subFile.Extension.ToLowerInvariant();
                            if ((subExt == ".vbs" || subExt == ".js") && options.RemoveRogueScripts)
                            {
                                // ຖ້າຊື່ຄືກັບ Drive.vbs ຫຼື autorun.vbs
                                string sName = subFile.Name.ToLowerInvariant();
                                if (sName.Contains("drive") || sName.Contains("autorun") || sName.Contains("helper") || sName.Contains("system"))
                                {
                                    try
                                    {
                                        File.SetAttributes(subFile.FullName, FileAttributes.Normal);
                                        subFile.Delete();
                                        result.RogueScriptsRemovedCount++;
                                        if (logCallback != null) logCallback("✓ ລຶບໄຟລ໌ script ໄວຣັສ: " + subFile.FullName);
                                    }
                                    catch { }
                                }
                            }
                            else if (subExt == ".lnk" && options.RemoveShortcuts)
                            {
                                if (IsMaliciousShortcut(subFile.FullName, out string reason))
                                {
                                    try
                                    {
                                        File.SetAttributes(subFile.FullName, FileAttributes.Normal);
                                        subFile.Delete();
                                        result.ShortcutsRemovedCount++;
                                        if (logCallback != null) logCallback("✓ ລຶບ Shortcut ໄວຣັສ: " + subFile.FullName);
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add("ScanAndCleanDirectoryFiles: " + ex.Message);
            }
        }

        private static void UnhideFilesAndFoldersRecursively(string rootPath, UsbCleanResult result, Action<string> logCallback)
        {
            try
            {
                DirectoryInfo rootDir = new DirectoryInfo(rootPath);
                UnhideDirectoryCore(rootDir, result, logCallback);
            }
            catch (Exception ex)
            {
                result.Errors.Add("Unhide error: " + ex.Message);
            }
        }

        private static void UnhideDirectoryCore(DirectoryInfo dir, UsbCleanResult result, Action<string> logCallback)
        {
            string name = dir.Name.ToLowerInvariant();
            // ຂ້າມໂຟນເດີພິເສດຂອງ Windows ແລະ ໂຟນເດີ Vaccine
            if (name == "$recycle.bin" || name == "system volume information" || name == VACCINE_FOLDER_NAME)
            {
                return;
            }

            // 1. Unhide Directory
            try
            {
                FileAttributes dirAttr = dir.Attributes;
                if ((dirAttr & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly)) != 0)
                {
                    FileAttributes cleanAttr = dirAttr & ~(FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly);
                    if (cleanAttr == 0) cleanAttr = FileAttributes.Directory;
                    dir.Attributes = cleanAttr;
                    result.DirectoriesUnhiddenCount++;
                }
            }
            catch { }

            // 2. Unhide Files in this Directory
            try
            {
                FileInfo[] files = dir.GetFiles();
                foreach (var file in files)
                {
                    string fName = file.Name.ToLowerInvariant();
                    if (fName == "desktop.ini" || fName == "thumbs.db") continue;

                    try
                    {
                        FileAttributes fAttr = file.Attributes;
                        if ((fAttr & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly)) != 0)
                        {
                            FileAttributes cleanAttr = fAttr & ~(FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly);
                            if (cleanAttr == 0) cleanAttr = FileAttributes.Normal;
                            file.Attributes = cleanAttr;
                            result.FilesUnhiddenCount++;
                        }
                    }
                    catch { }
                }
            }
            catch { }

            // 3. Recurse into subdirectories
            try
            {
                DirectoryInfo[] subDirs = dir.GetDirectories();
                foreach (var sub in subDirs)
                {
                    UnhideDirectoryCore(sub, result, logCallback);
                }
            }
            catch { }
        }
    }
}
