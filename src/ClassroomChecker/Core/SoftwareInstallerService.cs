using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ClassroomChecker.Core
{
    public class InstallResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int ExitCode { get; set; }

        public InstallResult(bool success, string message, int exitCode = 0)
        {
            Success = success;
            Message = message;
            ExitCode = exitCode;
        }
    }

    /// <summary>
    /// ບໍລິການກວດສອບ ແລະ ຕິດຕັ້ງຊອບແວພື້ນຖານແບບ Silent Unattended ຈາກ USB
    /// </summary>
    public static class SoftwareInstallerService
    {
        private static readonly List<SoftwarePackage> _catalog = new List<SoftwarePackage>();

        static SoftwareInstallerService()
        {
            InitializeCatalog();
        }

        private static void InitializeCatalog()
        {
            _catalog.Clear();

            // 1. 7-Zip
            _catalog.Add(new SoftwarePackage
            {
                Id = "7zip",
                Name = "7-Zip (64-bit)",
                Category = "Archive / Compression",
                Description = "ໂປຣແກຣມບີບອັດ ແລະ ແຕກໄຟລ໌ .zip, .rar, .7z ຟຣີແທ້ຕະຫຼອດຊີວິດ",
                LicenseType = "Open Source (LGPL)",
                ExeSearchName = "7zFM.exe",
                FallbackPaths = new string[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"7-Zip\7zFM.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"7-Zip\7zFM.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"7-Zip\7z.exe")
                },
                InstallerFilePatterns = new string[] { "*7z*.exe", "*7zip*.exe" },
                SilentArguments = "/S"
            });

            // 2. SumatraPDF
            _catalog.Add(new SoftwarePackage
            {
                Id = "sumatrapdf",
                Name = "SumatraPDF",
                Category = "PDF Reader",
                Description = "ໂປຣແກຣມອ່ານ PDF ນ້ຳໜັກເບົາຫຼາຍ, ເປີດໄຟລ໌ໄວ, ບໍ່ກິນ RAM",
                LicenseType = "Open Source (GPLv3)",
                ExeSearchName = "SumatraPDF.exe",
                FallbackPaths = new string[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"SumatraPDF\SumatraPDF.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"SumatraPDF\SumatraPDF.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"SumatraPDF\SumatraPDF.exe")
                },
                InstallerFilePatterns = new string[] { "*sumatra*.exe" },
                SilentArguments = "/S"
            });

            // 3. VLC Media Player
            _catalog.Add(new SoftwarePackage
            {
                Id = "vlc",
                Name = "VLC Media Player",
                Category = "Media Player",
                Description = "ເຄື່ອງຫຼິ້ນວິດີໂອ ແລະ ສຽງ ຄົບທຸກ Format (MP4, MKV, MP3) ໂດຍບໍ່ຕ້ອງລົງ Codec",
                LicenseType = "Open Source (GPL)",
                ExeSearchName = "vlc.exe",
                FallbackPaths = new string[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"VideoLAN\VLC\vlc.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"VideoLAN\VLC\vlc.exe")
                },
                InstallerFilePatterns = new string[] { "*vlc*.exe" },
                SilentArguments = "/S"
            });

            // 4. Brave Browser
            _catalog.Add(new SoftwarePackage
            {
                Id = "brave",
                Name = "Brave Browser",
                Category = "Web Browser",
                Description = "ເວັບບຣາວເຊີໄວ ແລະ ປອດໄພ, ບລັອກໂຄສະນາ ແລະ ສິ່ງລົບກວນອັດຕະໂນມັດ",
                LicenseType = "Open Source (Brave / MPL)",
                ExeSearchName = "brave.exe",
                FallbackPaths = new string[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"BraveSoftware\Brave-Browser\Application\brave.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"BraveSoftware\Brave-Browser\Application\brave.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"BraveSoftware\Brave-Browser\Application\brave.exe")
                },
                InstallerFilePatterns = new string[] { "*brave*.exe", "*bravesetup*.exe" },
                SilentArguments = "/silent /install"
            });

            // 5. Google Chrome
            _catalog.Add(new SoftwarePackage
            {
                Id = "chrome",
                Name = "Google Chrome",
                Category = "Web Browser",
                Description = "ເວັບບຣາວເຊີຍອດນິຍົມສຳລັບການຮຽນການສອນ ແລະ ເປີດເວັບທົ່ວໄປ",
                LicenseType = "Freeware (Google)",
                ExeSearchName = "chrome.exe",
                FallbackPaths = new string[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Google\Chrome\Application\chrome.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Google\Chrome\Application\chrome.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\Application\chrome.exe")
                },
                InstallerFilePatterns = new string[] { "*chrome*.exe", "*chromesetup*.exe" },
                SilentArguments = "/silent /install"
            });

            // 6. Notepad++
            _catalog.Add(new SoftwarePackage
            {
                Id = "notepadplusplus",
                Name = "Notepad++",
                Category = "Text / Code Editor",
                Description = "ໂປຣແກຣມແກ້ໄຂໂຄ້ດ ແລະ ຂໍ້ຄວາມ, ຮອງຮັບຫຼາຍພາສາຂຽນໂປຣແກຣມ",
                LicenseType = "Open Source (GPL)",
                ExeSearchName = "notepad++.exe",
                FallbackPaths = new string[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Notepad++\notepad++.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Notepad++\notepad++.exe")
                },
                InstallerFilePatterns = new string[] { "*npp*.exe", "*notepad++*.exe" },
                SilentArguments = "/S"
            });

            // 7. Lao Script for Windows (LaoScript 8)
            _catalog.Add(new SoftwarePackage
            {
                Id = "laoscript8",
                Name = "LaoScript 8 (Lao Script for Windows)",
                Category = "Lao Language / Fonts",
                Description = "ໂປຣແກຣມແປ້ນພິມ ແລະ ຕັດຄຳພາສາລາວ, ຟອນລາວ, ແປງລະຫັດຟອນເກົ່າ-ໃໝ່",
                LicenseType = "Freeware (Lao Script)",
                ExeSearchName = "laoscript8.exe",
                FallbackPaths = new string[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Lao Script for Windows\laoscript8.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Lao Script for Windows\laoscript8.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Lao Script for Windows 8\laoscript8.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Lao Script for Windows 8\laoscript8.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Lao Script for Windows\ls8.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Lao Script for Windows\ls8.exe")
                },
                InstallerFilePatterns = new string[] { "*laoscript*.exe", "*laoscript8*.exe", "*ls8*.exe" },
                SilentArguments = "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART"
            });

            // 8. VS Code
            _catalog.Add(new SoftwarePackage
            {
                Id = "vscode",
                Name = "Visual Studio Code",
                Category = "Development / IDE",
                Description = "ເຄື່ອງມືຂຽນໂຄ້ດຍອດນິຍົມສຳລັບຫ້ອງຮຽນຄອມພິວເຕີ ແລະ ໄອທີ",
                LicenseType = "Free (MIT / Microsoft)",
                ExeSearchName = "Code.exe",
                FallbackPaths = new string[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Microsoft VS Code\Code.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft VS Code\Code.exe")
                },
                InstallerFilePatterns = new string[] { "*vscode*.exe", "*code*.exe" },
                SilentArguments = "/VERYSILENT /NORESTART /MERGETASKS=!runcode"
            });

            // 9. GIMP
            _catalog.Add(new SoftwarePackage
            {
                Id = "gimp",
                Name = "GIMP",
                Category = "Graphics / Photo Editing",
                Description = "ໂປຣແກຣມຕັດຕໍ່ ແລະ ຕົກແຕ່ງຮູບພາບ Open Source ແທນ Photoshop",
                LicenseType = "Open Source (GPL)",
                ExeSearchName = "gimp-2.10.exe",
                FallbackPaths = new string[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"GIMP 2\bin\gimp-2.10.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"GIMP 3\bin\gimp-3.0.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"GIMP 2\bin\gimp-2.10.exe")
                },
                InstallerFilePatterns = new string[] { "*gimp*.exe" },
                SilentArguments = "/VERYSILENT /NORESTART"
            });
        }

        public static List<SoftwarePackage> GetCatalog()
        {
            return new List<SoftwarePackage>(_catalog);
        }

        public static string GetInstallersDirectory()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dir = Path.Combine(baseDir, "installers");
            if (!Directory.Exists(dir))
            {
                try { Directory.CreateDirectory(dir); } catch { }
            }
            return dir;
        }

        /// <summary>
        /// ກວດສອບວ່າຊອບແວນີ້ຕິດຕັ້ງໃນເຄື່ອງແລ້ວຫຼືຍັງ
        /// </summary>
        public static bool IsInstalled(SoftwarePackage pkg, out string version, out string installedPath)
        {
            version = null;
            installedPath = null;
            if (pkg == null) return false;

            // 1. ກວດສອບຈາກ Registry App Paths
            if (!string.IsNullOrEmpty(pkg.ExeSearchName))
            {
                string[] appPathKeys = {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + pkg.ExeSearchName,
                    @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\" + pkg.ExeSearchName
                };

                foreach (string subKey in appPathKeys)
                {
                    try
                    {
                        using (RegistryKey k = Registry.LocalMachine.OpenSubKey(subKey))
                        {
                            if (k != null)
                            {
                                object val = k.GetValue(null) ?? k.GetValue("");
                                if (val != null && File.Exists(val.ToString()))
                                {
                                    installedPath = val.ToString();
                                    version = GetFileVersion(installedPath);
                                    return true;
                                }
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        using (RegistryKey k = Registry.CurrentUser.OpenSubKey(subKey))
                        {
                            if (k != null)
                            {
                                object val = k.GetValue(null) ?? k.GetValue("");
                                if (val != null && File.Exists(val.ToString()))
                                {
                                    installedPath = val.ToString();
                                    version = GetFileVersion(installedPath);
                                    return true;
                                }
                            }
                        }
                    }
                    catch { }
                }
            }

            // 2. ກວດສອບຈາກ Fallback Paths
            if (pkg.FallbackPaths != null)
            {
                foreach (string p in pkg.FallbackPaths)
                {
                    if (!string.IsNullOrEmpty(p) && File.Exists(p))
                    {
                        installedPath = p;
                        version = GetFileVersion(installedPath);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// ຄົ້ນຫາໄຟລ໌ Setup ຂອງ Package ນີ້ໃນໂຟນເດີ installers/
        /// </summary>
        public static string FindInstallerFile(SoftwarePackage pkg)
        {
            if (pkg == null) return null;
            string instDir = GetInstallersDirectory();
            if (!Directory.Exists(instDir)) return null;

            try
            {
                // 1. ຄົ້ນຫາຕາມ Patterns ທີ່ກຳນົດ
                if (pkg.InstallerFilePatterns != null)
                {
                    foreach (string pattern in pkg.InstallerFilePatterns)
                    {
                        string[] matches = Directory.GetFiles(instDir, pattern);
                        if (matches.Length > 0)
                        {
                            return matches[0];
                        }
                    }
                }

                // 2. Fallback: ຄົ້ນຫາໄຟລ໌ທີ່ຊື່ມີ id ຂອງ package
                string[] allFiles = Directory.GetFiles(instDir);
                foreach (string f in allFiles)
                {
                    string ext = Path.GetExtension(f).ToLowerInvariant();
                    if (ext != ".exe" && ext != ".msi") continue;

                    string name = Path.GetFileNameWithoutExtension(f).ToLowerInvariant();
                    if (name.Contains(pkg.Id.ToLowerInvariant()))
                    {
                        return f;
                    }
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// ດຳເນີນການຕິດຕັ້ງຊອບແວແບບ Silent ໃນ Background Thread
        /// </summary>
        public static Task<InstallResult> InstallAsync(SoftwarePackage pkg, string installerPath)
        {
            return Task.Run(() =>
            {
                if (pkg == null) return new InstallResult(false, "Invalid package");
                if (string.IsNullOrEmpty(installerPath) || !File.Exists(installerPath))
                {
                    return new InstallResult(false, "ບໍ່ພົບໄຟລ໌ຕິດຕັ້ງ: " + installerPath);
                }

                string ext = Path.GetExtension(installerPath).ToLowerInvariant();
                bool isMsi = ext == ".msi";

                ProcessStartInfo psi = new ProcessStartInfo();

                if (isMsi)
                {
                    psi.FileName = "msiexec.exe";
                    psi.Arguments = string.Format("/i \"{0}\" /qn /norestart", installerPath);
                }
                else
                {
                    psi.FileName = installerPath;
                    psi.Arguments = !string.IsNullOrEmpty(pkg.SilentArguments) ? pkg.SilentArguments : "/S";
                }

                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;

                try
                {
                    using (Process p = Process.Start(psi))
                    {
                        if (p == null)
                        {
                            return new InstallResult(false, "ບໍ່ສາມາດເລີ່ມ Process ຕິດຕັ້ງໄດ້");
                        }

                        // ລໍຖ້າສູງສຸດ 3 ນາທີ (180 ວິນາທີ) ສຳລັບຊອບແວໃຫຍ່ໆ
                        bool finished = p.WaitForExit(180000);
                        if (!finished)
                        {
                            try { p.Kill(); } catch { }
                            return new InstallResult(false, "ການຕິດຕັ້ງໃຊ້ເວລາດົນເກີນກຳນົດ (Timeout 180s)");
                        }

                        int code = p.ExitCode;
                        // ExitCode 0 = Success, 3010 = Success (Reboot required)
                        if (code == 0 || code == 3010)
                        {
                            return new InstallResult(true, "ຕິດຕັ້ງ " + pkg.Name + " ສຳເລັດຮຽບຮ້ອຍແລ້ວ", code);
                        }
                        else
                        {
                            return new InstallResult(false, string.Format("ການຕິດຕັ້ງສິ້ນສຸດດ້ວຍ Exit Code: {0}", code), code);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new InstallResult(false, "ເກີດຂໍ້ຜິດພາດ: " + ex.Message);
                }
            });
        }

        private static string GetFileVersion(string exePath)
        {
            try
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(exePath);
                if (!string.IsNullOrEmpty(fvi.ProductVersion)) return fvi.ProductVersion.Trim();
                if (!string.IsNullOrEmpty(fvi.FileVersion)) return fvi.FileVersion.Trim();
            }
            catch { }
            return "Installed";
        }
    }
}
