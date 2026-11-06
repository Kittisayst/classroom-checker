using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using ClassroomChecker.Config;
using ClassroomChecker.Core;
using ClassroomChecker.Native;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ໂມດູນກວດສອບ ແລະ ລ້າງໄຟລ໌ຂີ້ເຫຍື້ອ/Temp ຢ່າງປອດໄພ ຕາມ cm-spec.md ຂໍ້ 5.4
    /// </summary>
    public class CleanupModule : ICheckModule
    {
        public string Id
        {
            get { return "cleanup"; }
        }

        public string DisplayNameKey
        {
            get { return "module_cleanup"; }
        }

        public bool CanFix
        {
            get { return true; }
        }

        public CheckResult Check()
        {
            var details = new List<string>();
            long totalBytes = 0;
            var cleanupCfg = ConfigService.Instance.Current.cleanup;

            try
            {
                // 1. User Temp (%TEMP%)
                if (cleanupCfg.userTemp)
                {
                    string userTemp = Path.GetTempPath();
                    long sz = CalculateFolderSize(userTemp, cleanupCfg.excludePaths);
                    totalBytes += sz;
                    details.Add(string.Format("User Temp ({0}): {1}", userTemp, FormatBytes(sz)));
                }

                // 2. Windows Temp (C:\Windows\Temp)
                if (cleanupCfg.windowsTemp)
                {
                    string winTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
                    long sz = CalculateFolderSize(winTemp, cleanupCfg.excludePaths);
                    totalBytes += sz;
                    details.Add(string.Format("Windows Temp ({0}): {1}", winTemp, FormatBytes(sz)));
                }

                // 3. Windows Update Cache (C:\Windows\SoftwareDistribution\Download)
                if (cleanupCfg.windowsUpdateCache)
                {
                    string updateCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");
                    long sz = CalculateFolderSize(updateCache, cleanupCfg.excludePaths);
                    totalBytes += sz;
                    details.Add(string.Format("Windows Update Cache: {0}", FormatBytes(sz)));
                }

                // 4. Recycle Bin
                if (cleanupCfg.recycleBin)
                {
                    details.Add("Recycle Bin: ພ້ອມລ້າງ");
                }

                string summary = string.Format("ພົບໄຟລ໌ Temp ທີ່ສາມາດລ້າງໄດ້: {0}", FormatBytes(totalBytes));

                // ຖ້າມີໄຟລ໌ຫຼາຍກວ່າ 50 MB ໃຫ້ແນະນຳໃຫ້ລ້າງ (Fail/Warning ເພື່ອໃຫ້ກົດ Fix ໄດ້)
                if (totalBytes > 50 * 1024 * 1024)
                {
                    return new CheckResult(CheckStatus.Fail, summary, details);
                }
                else
                {
                    return new CheckResult(CheckStatus.Pass, summary + " (ສະອາດດີ)", details);
                }
            }
            catch (Exception ex)
            {
                details.Add("Error: " + ex.Message);
                return new CheckResult(CheckStatus.Unknown, "ເກີດຂໍ້ຜິດພາດໃນການກວດໄຟລ໌ Temp: " + ex.Message, details);
            }
        }

        public CheckResult Fix()
        {
            var details = new List<string>();
            long totalFreedBytes = 0;
            int totalSkippedFiles = 0;
            var cleanupCfg = ConfigService.Instance.Current.cleanup;

            try
            {
                // 1. ລ້າງ User Temp
                if (cleanupCfg.userTemp)
                {
                    string userTemp = Path.GetTempPath();
                    int skipped;
                    long freed = CleanFolderContents(userTemp, cleanupCfg.excludePaths, out skipped);
                    totalFreedBytes += freed;
                    totalSkippedFiles += skipped;
                    details.Add(string.Format("User Temp: ລ້າງໄດ້ {0} (ຂ້າມ {1} ໄຟລ໌ທີ່ຖືກລັອກ)", FormatBytes(freed), skipped));
                }

                // 2. ລ້າງ Windows Temp
                if (cleanupCfg.windowsTemp)
                {
                    string winTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
                    int skipped;
                    long freed = CleanFolderContents(winTemp, cleanupCfg.excludePaths, out skipped);
                    totalFreedBytes += freed;
                    totalSkippedFiles += skipped;
                    details.Add(string.Format("Windows Temp: ລ້າງໄດ້ {0} (ຂ້າມ {1} ໄຟລ໌ທີ່ຖືກລັອກ)", FormatBytes(freed), skipped));
                }

                // 3. ລ້າງ Windows Update Cache (SoftwareDistribution\Download)
                if (cleanupCfg.windowsUpdateCache)
                {
                    string updateCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");
                    int skipped = 0;
                    long freed = 0;

                    bool serviceStopped = StopServiceSafely("wuauserv");
                    try
                    {
                        freed = CleanFolderContents(updateCache, cleanupCfg.excludePaths, out skipped);
                        totalFreedBytes += freed;
                        totalSkippedFiles += skipped;
                    }
                    finally
                    {
                        if (serviceStopped)
                        {
                            StartServiceSafely("wuauserv");
                        }
                    }

                    details.Add(string.Format("Update Cache: ລ້າງໄດ້ {0} (ຂ້າມ {1} ໄຟລ໌)", FormatBytes(freed), skipped));
                }

                // 4. ລ້າງ Recycle Bin
                if (cleanupCfg.recycleBin)
                {
                    try
                    {
                        int ret = NativeMethods.SHEmptyRecycleBin(
                            IntPtr.Zero,
                            null,
                            NativeMethods.SHERB_NOCONFIRMATION | NativeMethods.SHERB_NOPROGRESSUI | NativeMethods.SHERB_NOSOUND);
                        details.Add("Recycle Bin: ລ້າງຮຽບຮ້ອຍ (Result: " + ret + ")");
                    }
                    catch (Exception ex)
                    {
                        details.Add("Recycle Bin Error: " + ex.Message);
                    }
                }

                string summary = string.Format("ລ້າງໄຟລ໌ສຳເລັດ: ຄືນພື້ນທີ່ {0} (ຂ້າມ {1} ໄຟລ໌ທີ່ກຳລັງໃຊ້ງານ)",
                    FormatBytes(totalFreedBytes), totalSkippedFiles);

                return new CheckResult(CheckStatus.Pass, summary, details);
            }
            catch (Exception ex)
            {
                details.Add("Error: " + ex.Message);
                return new CheckResult(CheckStatus.Fail, "ເກີດຂໍ້ຜິດພາດໃນການລ້າງໄຟລ໌: " + ex.Message, details);
            }
        }

        private long CalculateFolderSize(string folderPath, List<string> excludePaths)
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) return 0;
            long size = 0;

            try
            {
                DirectoryInfo di = new DirectoryInfo(folderPath);

                // ກວດສອບໄຟລ໌
                foreach (FileInfo fi in di.GetFiles("*", SearchOption.AllDirectories))
                {
                    if (IsExcluded(fi.FullName, excludePaths)) continue;

                    // ຫ້າມ follow reparse point (symlink / junction)
                    if ((fi.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint) continue;

                    try
                    {
                        size += fi.Length;
                    }
                    catch { }
                }
            }
            catch { }

            return size;
        }

        private long CleanFolderContents(string folderPath, List<string> excludePaths, out int skippedCount)
        {
            skippedCount = 0;
            long freedBytes = 0;
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) return 0;

            DirectoryInfo di = new DirectoryInfo(folderPath);

            // 1. ລຶບໄຟລ໌ໃນໂຟນເດີແມ່
            FileInfo[] files;
            try
            {
                files = di.GetFiles("*", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                return 0;
            }

            foreach (FileInfo f in files)
            {
                if (IsExcluded(f.FullName, excludePaths)) continue;
                if ((f.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint) continue;

                try
                {
                    long len = f.Length;
                    f.Attributes = FileAttributes.Normal;
                    f.Delete();
                    freedBytes += len;
                }
                catch
                {
                    // ໄຟລ໌ຖືກລັອກ ຫຼື ສິດບໍ່ພໍ -> ຂ້າມ! ຫ້າມ crash!
                    skippedCount++;
                }
            }

            // 2. ລຶບໂຟນເດີຍ່ອຍ
            DirectoryInfo[] subDirs;
            try
            {
                subDirs = di.GetDirectories("*", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                return freedBytes;
            }

            foreach (DirectoryInfo sub in subDirs)
            {
                if (IsExcluded(sub.FullName, excludePaths)) continue;
                if ((sub.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint) continue;

                try
                {
                    // ຄິດໄລ່ຂະໜາດກ່ອນລຶບ
                    long subSize = CalculateFolderSize(sub.FullName, excludePaths);
                    sub.Delete(true);
                    freedBytes += subSize;
                }
                catch
                {
                    // ຖ້າລຶບທັງໂຟນເດີຍ່ອຍບໍ່ໄດ້ ໃຫ້ລອງລຶບໄຟລ໌ພາຍໃນເທົ່າທີ່ໄດ້
                    int subSkipped;
                    freedBytes += CleanFolderContents(sub.FullName, excludePaths, out subSkipped);
                    skippedCount += subSkipped;
                }
            }

            return freedBytes;
        }

        private bool IsExcluded(string path, List<string> excludePaths)
        {
            if (excludePaths == null || excludePaths.Count == 0) return false;
            foreach (string ex in excludePaths)
            {
                if (!string.IsNullOrEmpty(ex) && path.IndexOf(ex, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        private bool StopServiceSafely(string serviceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private void StartServiceSafely(string serviceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
                    }
                }
            }
            catch { }
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("F1") + " KB";
            if (bytes < 1024 * 1024 * 1024) return (bytes / (1024.0 * 1024.0)).ToString("F1") + " MB";
            return (bytes / (1024.0 * 1024.0 * 1024.0)).ToString("F2") + " GB";
        }
    }
}
