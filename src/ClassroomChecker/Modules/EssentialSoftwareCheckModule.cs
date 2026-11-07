using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ClassroomChecker.Core;
using ClassroomChecker.I18n;
using Microsoft.Win32;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ໂມດູນກວດສອບຊອບແວພື້ນຖານທີ່ຈຳເປັນສຳລັບຫ້ອງຮຽນຄອມພິວເຕີ (Web Browser, Archive, PDF Reader, Dev Tools)
    /// </summary>
    public class EssentialSoftwareCheckModule : ICheckModule
    {
        public string Id
        {
            get { return "essential_software"; }
        }

        public string DisplayNameKey
        {
            get { return "module_essential_software"; }
        }

        public bool CanFix
        {
            get
            {
                string instDir = GetInstallersDirectory();
                return Directory.Exists(instDir) && Directory.GetFiles(instDir, "*.exe").Length > 0;
            }
        }

        public CheckResult Check()
        {
            var details = new List<string>();
            var installedCategories = new List<string>();
            var missingCategories = new List<string>();

            bool hasBrowser = false;
            string browserSummary = string.Empty;

            bool hasArchive = false;
            string archiveSummary = string.Empty;

            bool hasPdf = false;
            string pdfSummary = string.Empty;

            // -----------------------------------------------------------------
            // 1. Web Browsers (Chrome, Edge, Firefox)
            // -----------------------------------------------------------------
            details.Add("=== 1. Web Browsers (ບຣາວເຊີທ່ອງເວັບ) ===");
            
            string chromePath = FindChrome();
            if (!string.IsNullOrEmpty(chromePath))
            {
                hasBrowser = true;
                string ver = GetProductVersion(chromePath);
                browserSummary = "Chrome " + ver;
                details.Add(string.Format("  ✓ Google Chrome ({0}) - {1}", ver, chromePath));
            }
            else
            {
                details.Add("  ✗ Google Chrome: ບໍ່ພົບ (Not Installed)");
            }

            string edgePath = FindEdge();
            if (!string.IsNullOrEmpty(edgePath))
            {
                if (!hasBrowser)
                {
                    hasBrowser = true;
                    browserSummary = "Edge " + GetProductVersion(edgePath);
                }
                details.Add(string.Format("  ✓ Microsoft Edge ({0}) - {1}", GetProductVersion(edgePath), edgePath));
            }
            else
            {
                details.Add("  ✗ Microsoft Edge: ບໍ່ພົບ (Not Installed)");
            }

            string firefoxPath = FindFirefox();
            if (!string.IsNullOrEmpty(firefoxPath))
            {
                if (!hasBrowser)
                {
                    hasBrowser = true;
                    browserSummary = "Firefox " + GetProductVersion(firefoxPath);
                }
                details.Add(string.Format("  ✓ Mozilla Firefox ({0}) - {1}", GetProductVersion(firefoxPath), firefoxPath));
            }

            if (hasBrowser) installedCategories.Add(browserSummary);
            else missingCategories.Add("Browser (Chrome/Edge)");

            // -----------------------------------------------------------------
            // 2. Archive / Compression Tools (7-Zip, WinRAR)
            // -----------------------------------------------------------------
            details.Add("");
            details.Add("=== 2. File Compression (ໂປຣແກຣມຈັດການໄຟລ໌ບີບອັດ) ===");

            string sevenZipPath = FindSevenZip();
            if (!string.IsNullOrEmpty(sevenZipPath))
            {
                hasArchive = true;
                string ver = GetProductVersion(sevenZipPath);
                archiveSummary = "7-Zip " + ver;
                details.Add(string.Format("  ✓ 7-Zip ({0}) - {1}", ver, sevenZipPath));
            }
            else
            {
                details.Add("  ✗ 7-Zip: ບໍ່ພົບ (Not Installed)");
            }

            string winRarPath = FindWinRar();
            if (!string.IsNullOrEmpty(winRarPath))
            {
                hasArchive = true;
                if (string.IsNullOrEmpty(archiveSummary))
                {
                    archiveSummary = "WinRAR " + GetProductVersion(winRarPath);
                }
                details.Add(string.Format("  ✓ WinRAR ({0}) - {1}", GetProductVersion(winRarPath), winRarPath));
            }
            else
            {
                details.Add("  ✗ WinRAR: ບໍ່ພົບ (Not Installed)");
            }

            if (hasArchive) installedCategories.Add(archiveSummary);
            else missingCategories.Add("Archive (7-Zip/WinRAR)");

            // -----------------------------------------------------------------
            // 3. PDF Readers (Adobe Acrobat, Foxit, Sumatra, Edge)
            // -----------------------------------------------------------------
            details.Add("");
            details.Add("=== 3. PDF Readers (ໂປຣແກຣມອ່ານ PDF) ===");

            string adobePath = FindAdobeReader();
            if (!string.IsNullOrEmpty(adobePath))
            {
                hasPdf = true;
                string ver = GetProductVersion(adobePath);
                pdfSummary = "Adobe Reader " + ver;
                details.Add(string.Format("  ✓ Adobe Acrobat / Reader ({0}) - {1}", ver, adobePath));
            }
            else
            {
                details.Add("  ✗ Adobe Acrobat Reader: ບໍ່ພົບ");
            }

            string foxitPath = FindFoxitReader();
            if (!string.IsNullOrEmpty(foxitPath))
            {
                hasPdf = true;
                if (string.IsNullOrEmpty(pdfSummary)) pdfSummary = "Foxit Reader " + GetProductVersion(foxitPath);
                details.Add(string.Format("  ✓ Foxit PDF Reader ({0}) - {1}", GetProductVersion(foxitPath), foxitPath));
            }

            string sumatraPath = FindSumatraPdf();
            if (!string.IsNullOrEmpty(sumatraPath))
            {
                hasPdf = true;
                if (string.IsNullOrEmpty(pdfSummary)) pdfSummary = "SumatraPDF " + GetProductVersion(sumatraPath);
                details.Add(string.Format("  ✓ SumatraPDF ({0}) - {1}", GetProductVersion(sumatraPath), sumatraPath));
            }

            if (!hasPdf && !string.IsNullOrEmpty(edgePath))
            {
                hasPdf = true;
                pdfSummary = "MS Edge (Built-in PDF)";
                details.Add("  ℹ MS Edge Built-in PDF Reader (ພ້ອມໃຊ້ງານ)");
            }

            if (hasPdf) installedCategories.Add(pdfSummary);
            else missingCategories.Add("PDF Reader");

            // -----------------------------------------------------------------
            // 4. Study & Programming Tools (VS Code, Python, etc.)
            // -----------------------------------------------------------------
            details.Add("");
            details.Add("=== 4. Study & Programming Tools (ເຄື່ອງມືຮຽນ ແລະ ພັດທະນາ) ===");

            string vscodePath = FindVsCode();
            if (!string.IsNullOrEmpty(vscodePath))
            {
                details.Add(string.Format("  ✓ Visual Studio Code ({0}) - {1}", GetProductVersion(vscodePath), vscodePath));
            }
            else
            {
                details.Add("  - VS Code: ບໍ່ໄດ້ຕິດຕັ້ງ");
            }

            string pythonPath = FindPython();
            if (!string.IsNullOrEmpty(pythonPath))
            {
                details.Add(string.Format("  ✓ Python ({0}) - {1}", GetProductVersion(pythonPath), pythonPath));
            }
            else
            {
                details.Add("  - Python: ບໍ່ໄດ້ຕິດຕັ້ງ");
            }

            string devCppPath = FindDevCpp();
            if (!string.IsNullOrEmpty(devCppPath))
            {
                details.Add(string.Format("  ✓ Dev-C++ ({0}) - {1}", GetProductVersion(devCppPath), devCppPath));
            }

            // -----------------------------------------------------------------
            // ຄິດໄລ່ Status ແລະ Summary
            // -----------------------------------------------------------------
            if (hasBrowser && hasArchive && hasPdf)
            {
                string summary = string.Format("ຕິດຕັ້ງໂປຣແກຣມພື້ນຖານຄົບຖ້ວນ ({0})", string.Join(", ", installedCategories.ToArray()));
                return new CheckResult(CheckStatus.Pass, summary, details);
            }
            else if (hasBrowser || hasArchive)
            {
                string summary = string.Format("ຂາດບາງໂປຣແກຣມ: {0} (ຕິດຕັ້ງແລ້ວ: {1})",
                    string.Join(", ", missingCategories.ToArray()),
                    string.Join(", ", installedCategories.ToArray()));
                return new CheckResult(CheckStatus.Warning, summary, details);
            }
            else
            {
                string summary = "ບໍ່ພົບຊອບແວພື້ນຖານທີ່ຈຳເປັນ (Chrome, 7-Zip, PDF Reader)";
                return new CheckResult(CheckStatus.Fail, summary, details);
            }
        }

        public CheckResult Fix()
        {
            var details = new List<string>();
            string instDir = GetInstallersDirectory();

            if (!Directory.Exists(instDir))
            {
                return new CheckResult(CheckStatus.Fail, "ບໍ່ພົບໂຟນເດີ installers/ ໃນ USB ສຳລັບຕິດຕັ້ງ", details);
            }

            string[] exeFiles = Directory.GetFiles(instDir, "*.exe");
            if (exeFiles.Length == 0)
            {
                return new CheckResult(CheckStatus.Fail, "ບໍ່ພົບໄຟລ໌ຕິດຕັ້ງ (.exe) ໃນໂຟນເດີ installers/", details);
            }

            int installedCount = 0;
            foreach (string exe in exeFiles)
            {
                string fileName = Path.GetFileName(exe);
                details.Add("ກຳລັງຕິດຕັ້ງ: " + fileName + "...");

                try
                {
                    string args = "/S"; // Default silent flag for NSIS/7-Zip
                    if (fileName.IndexOf("chrome", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        args = "/silent /install";
                    }
                    else if (fileName.IndexOf("winrar", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        args = "/s";
                    }

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = exe,
                        Arguments = args,
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };

                    using (Process p = Process.Start(psi))
                    {
                        if (p != null)
                        {
                            p.WaitForExit(60000); // 60s timeout
                            details.Add("  ✓ ຕິດຕັ້ງສຳເລັດ: " + fileName);
                            installedCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    details.Add("  ✗ ຜິດພາດ: " + fileName + " (" + ex.Message + ")");
                }
            }

            return new CheckResult(
                installedCount > 0 ? CheckStatus.Pass : CheckStatus.Warning,
                string.Format("ດຳເນີນການຕິດຕັ້ງໂປຣແກຣມສຳເລັດ {0} ລາຍການ", installedCount),
                details);
        }

        // =====================================================================
        // Detection Helpers
        // =====================================================================

        private string FindChrome()
        {
            return FindAppExecutable("chrome.exe", new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Google\Chrome\Application\chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Google\Chrome\Application\chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\Application\chrome.exe")
            });
        }

        private string FindEdge()
        {
            return FindAppExecutable("msedge.exe", new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft\Edge\Application\msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft\Edge\Application\msedge.exe")
            });
        }

        private string FindFirefox()
        {
            return FindAppExecutable("firefox.exe", new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Mozilla Firefox\firefox.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Mozilla Firefox\firefox.exe")
            });
        }

        private string FindSevenZip()
        {
            string p = FindAppExecutable("7zFM.exe", new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"7-Zip\7zFM.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"7-Zip\7zFM.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"7-Zip\7z.exe")
            });

            if (!string.IsNullOrEmpty(p)) return p;

            try
            {
                using (RegistryKey k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\7-Zip"))
                {
                    if (k != null)
                    {
                        object val = k.GetValue("Path");
                        if (val != null)
                        {
                            string candidate = Path.Combine(val.ToString(), "7zFM.exe");
                            if (File.Exists(candidate)) return candidate;
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        private string FindWinRar()
        {
            return FindAppExecutable("WinRAR.exe", new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"WinRAR\WinRAR.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"WinRAR\WinRAR.exe")
            });
        }

        private string FindAdobeReader()
        {
            return FindAppExecutable("AcroRd32.exe", new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Adobe\Acrobat DC\Acrobat\Acrobat.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Adobe\Acrobat Reader DC\Reader\AcroRd32.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Adobe\Acrobat Reader DC\Reader\AcroRd32.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Adobe\Reader 11.0\Reader\AcroRd32.exe")
            });
        }

        private string FindFoxitReader()
        {
            return FindAppExecutable("FoxitPDFReader.exe", new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Foxit Software\Foxit PDF Reader\FoxitPDFReader.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Foxit Software\Foxit PDF Reader\FoxitPDFReader.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Foxit Software\Foxit Reader\FoxitReader.exe")
            });
        }

        private string FindSumatraPdf()
        {
            return FindAppExecutable("SumatraPDF.exe", new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"SumatraPDF\SumatraPDF.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"SumatraPDF\SumatraPDF.exe")
            });
        }

        private string FindVsCode()
        {
            return FindAppExecutable("Code.exe", new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Microsoft VS Code\Code.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft VS Code\Code.exe")
            });
        }

        private string FindPython()
        {
            string p = FindAppExecutable("python.exe", new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Python\Python311\python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Python\Python312\python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Python\Python310\python.exe"),
                @"C:\Python311\python.exe",
                @"C:\Python312\python.exe",
                @"C:\Python310\python.exe",
                @"C:\Python39\python.exe"
            });

            if (!string.IsNullOrEmpty(p)) return p;

            // Check PATH environment
            try
            {
                string pathEnv = Environment.GetEnvironmentVariable("PATH");
                if (!string.IsNullOrEmpty(pathEnv))
                {
                    string[] paths = pathEnv.Split(';');
                    foreach (string dir in paths)
                    {
                        if (string.IsNullOrEmpty(dir)) continue;
                        string cand = Path.Combine(dir.Trim(), "python.exe");
                        if (File.Exists(cand) && cand.IndexOf("WindowsApps", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            return cand;
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        private string FindDevCpp()
        {
            return FindAppExecutable("devcpp.exe", new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Embarcadero\Dev-Cpp\devcpp.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Dev-Cpp\devcpp.exe"),
                @"C:\Dev-Cpp\devcpp.exe"
            });
        }

        private string FindAppExecutable(string exeName, string[] fallbackCandidates)
        {
            // 1. Registry App Paths
            string[] appPathKeys = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + exeName,
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\" + exeName
            };

            foreach (string subKey in appPathKeys)
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(subKey))
                    {
                        if (key != null)
                        {
                            object val = key.GetValue(null);
                            if (val != null)
                            {
                                string path = val.ToString().Trim('\"');
                                if (File.Exists(path)) return path;
                            }
                        }
                    }
                }
                catch { }

                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(subKey))
                    {
                        if (key != null)
                        {
                            object val = key.GetValue(null);
                            if (val != null)
                            {
                                string path = val.ToString().Trim('\"');
                                if (File.Exists(path)) return path;
                            }
                        }
                    }
                }
                catch { }
            }

            // 2. Fallback Candidates
            if (fallbackCandidates != null)
            {
                foreach (string candidate in fallbackCandidates)
                {
                    if (!string.IsNullOrEmpty(candidate) && File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }

        private string GetProductVersion(string exePath)
        {
            try
            {
                if (File.Exists(exePath))
                {
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(exePath);
                    if (!string.IsNullOrEmpty(fvi.ProductVersion))
                    {
                        return fvi.ProductVersion.Trim();
                    }
                    if (!string.IsNullOrEmpty(fvi.FileVersion))
                    {
                        return fvi.FileVersion.Trim();
                    }
                }
            }
            catch { }
            return "Installed";
        }

        private string GetInstallersDirectory()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, "installers");
        }
    }
}
