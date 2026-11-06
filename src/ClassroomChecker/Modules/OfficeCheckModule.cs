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
    /// ກວດສອບການຕິດຕັ້ງ MS Office (Word, Excel, PowerPoint) ແລະ ສະຖານະ License (best-effort)
    /// </summary>
    public class OfficeCheckModule : ICheckModule
    {
        public string Id
        {
            get { return "office"; }
        }

        public string DisplayNameKey
        {
            get { return "module_office"; }
        }

        public bool CanFix
        {
            get { return false; }
        }

        public CheckResult Check()
        {
            var details = new List<string>();
            bool hasWord = false;
            bool hasExcel = false;
            bool hasPpt = false;
            string detectedVersion = string.Empty;

            try
            {
                // 1. ກວດສອບ Word
                string wordPath = FindAppPath("WINWORD.EXE");
                if (!string.IsNullOrEmpty(wordPath) && File.Exists(wordPath))
                {
                    hasWord = true;
                    string ver = GetOfficeFriendlyVersion(wordPath);
                    if (string.IsNullOrEmpty(detectedVersion)) detectedVersion = ver;
                    details.Add("Word: " + wordPath + " (" + ver + ")");
                }
                else
                {
                    details.Add("Word: ບໍ່ພົບ WINWORD.EXE");
                }

                // 2. ກວດສອບ Excel
                string excelPath = FindAppPath("EXCEL.EXE");
                if (!string.IsNullOrEmpty(excelPath) && File.Exists(excelPath))
                {
                    hasExcel = true;
                    string ver = GetOfficeFriendlyVersion(excelPath);
                    if (string.IsNullOrEmpty(detectedVersion)) detectedVersion = ver;
                    details.Add("Excel: " + excelPath + " (" + ver + ")");
                }
                else
                {
                    details.Add("Excel: ບໍ່ພົບ EXCEL.EXE");
                }

                // 3. ກວດສອບ PowerPoint
                string pptPath = FindAppPath("POWERPNT.EXE");
                if (!string.IsNullOrEmpty(pptPath) && File.Exists(pptPath))
                {
                    hasPpt = true;
                    string ver = GetOfficeFriendlyVersion(pptPath);
                    if (string.IsNullOrEmpty(detectedVersion)) detectedVersion = ver;
                    details.Add("PowerPoint: " + pptPath + " (" + ver + ")");
                }
                else
                {
                    details.Add("PowerPoint: ບໍ່ພົບ POWERPNT.EXE");
                }

                // 4. ກວດສອບ License (Best-effort ຜ່ານ ospp.vbs)
                string licenseStatus = CheckOfficeLicense();
                details.Add("License: " + licenseStatus);

                // ປະເມີນຜົນ
                if (hasWord && hasExcel && hasPpt)
                {
                    string summary = string.Format("MS Office {0} (ຄົບທັງ 3 ໂປຣແກຣມ, {1})",
                        !string.IsNullOrEmpty(detectedVersion) ? detectedVersion : "", licenseStatus);
                    return new CheckResult(CheckStatus.Pass, summary, details);
                }
                else if (hasWord || hasExcel || hasPpt)
                {
                    string summary = "MS Office ຕິດຕັ້ງບໍ່ຄົບ (ຂາດບາງໂປຣແກຣມ)";
                    return new CheckResult(CheckStatus.Warning, summary, details);
                }
                else
                {
                    string summary = "ບໍ່ພົບການຕິດຕັ້ງ MS Office ໃນເຄື່ອງ";
                    return new CheckResult(CheckStatus.Fail, summary, details);
                }
            }
            catch (Exception ex)
            {
                details.Add("Error: " + ex.Message);
                return new CheckResult(CheckStatus.Unknown, "ເກີດຂໍ້ຜິດພາດໃນການກວດ Office: " + ex.Message, details);
            }
        }

        public CheckResult Fix()
        {
            return new CheckResult(CheckStatus.Unknown, "ບໍ່ຮອງຮັບການຕິດຕັ້ງ Office ອັດຕະໂນມັດ (CanFix=false)");
        }

        private string FindAppPath(string exeName)
        {
            string[] subKeys = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + exeName,
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\" + exeName
            };

            foreach (string subKey in subKeys)
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(subKey))
                    {
                        if (key != null)
                        {
                            object val = key.GetValue(null); // (Default)
                            if (val != null)
                            {
                                string path = val.ToString().Trim('\"');
                                if (File.Exists(path))
                                {
                                    return path;
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            // Fallback: ຊອກຫາໃນ Program Files ມາດຕະຖານ
            string[] programDirs = {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            string[] officeFolders = { "root\\Office16", "Office16", "Office15", "Office14" };

            foreach (string progDir in programDirs)
            {
                if (string.IsNullOrEmpty(progDir)) continue;
                foreach (string offFolder in officeFolders)
                {
                    string candidate = Path.Combine(progDir, "Microsoft Office", offFolder, exeName);
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }

        private string GetOfficeFriendlyVersion(string exePath)
        {
            try
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(exePath);
                int major = fvi.FileMajorPart;
                if (major == 16) return "2016/2019/2021/365";
                if (major == 15) return "2013";
                if (major == 14) return "2010";
                if (major == 12) return "2007";
                return "v" + major;
            }
            catch
            {
                return "Unknown version";
            }
        }

        private string CheckOfficeLicense()
        {
            try
            {
                string osppPath = FindOsppScript();
                if (string.IsNullOrEmpty(osppPath) || !File.Exists(osppPath))
                {
                    return "ກວດບໍ່ໄດ້ (ບໍ່ພົບ ospp.vbs)";
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cscript.exe",
                    Arguments = string.Format("//nologo \"{0}\" /dstatus", osppPath),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process proc = Process.Start(psi))
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(8000);

                    if (output.IndexOf("---LICENSED---", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return "Licensed (ຖືກລິຂະສິດ)";
                    }
                    if (output.IndexOf("---NOTIFICATIONS---", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        output.IndexOf("---OOB_GRACE---", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return "Grace Period / ຍັງບໍ່ Activate";
                    }

                    return "ກວດບໍ່ໄດ້ແນ່ນອນ (Unknown)";
                }
            }
            catch (Exception ex)
            {
                return "ກວດບໍ່ໄດ້ (" + ex.Message + ")";
            }
        }

        private string FindOsppScript()
        {
            string[] programDirs = {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };
            string[] officeFolders = { "root\\Office16", "Office16", "Office15", "Office14" };

            foreach (string dir in programDirs)
            {
                if (string.IsNullOrEmpty(dir)) continue;
                foreach (string off in officeFolders)
                {
                    string ospp = Path.Combine(dir, "Microsoft Office", off, "ospp.vbs");
                    if (File.Exists(ospp))
                    {
                        return ospp;
                    }
                }
            }
            return null;
        }
    }
}
