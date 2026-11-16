using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ClassroomChecker.Core;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ໂມດູນກວດສອບໂປຣແກຣມພື້ນຖານທີ່ທຸກເຄື່ອງໃນຫ້ອງຄອມຕ້ອງມີ:
    /// PDF Reader (SumatraPDF), LaoScript 8, VLC ແລະ 7-Zip.
    /// ລາຍຊື່, ວິທີກວດ ແລະ ວິທີຕິດຕັ້ງ ໃຊ້ຮ່ວມກັບແທັບ "ຕິດຕັ້ງຊອບແວ" ຜ່ານ SoftwareInstallerService.
    /// ໂປຣແກຣມອື່ນ (Chrome, Brave, VS Code...) ໃຫ້ຕິດຕັ້ງດ້ວຍມືຈາກແທັບ "ຕິດຕັ້ງຊອບແວ".
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

        /// <summary>ແກ້ໄດ້ເມື່ອມີໄຟລ໌ຕິດຕັ້ງຂອງໂປຣແກຣມພື້ນຖານຢ່າງໜ້ອຍ 1 ໂຕ ໃນໂຟນເດີ installers/</summary>
        public bool CanFix
        {
            get
            {
                foreach (var pkg in SoftwareInstallerService.GetEssentialPackages())
                {
                    if (!string.IsNullOrEmpty(SoftwareInstallerService.FindInstallerFile(pkg))) return true;
                }
                return false;
            }
        }

        public CheckResult Check()
        {
            var details = new List<string>();
            var installed = new List<string>();
            var missing = new List<string>();

            details.Add("=== ໂປຣແກຣມພື້ນຖານທີ່ຕ້ອງມີ ===");
            foreach (var pkg in SoftwareInstallerService.GetEssentialPackages())
            {
                string version;
                string path;
                if (IsSatisfied(pkg, out version, out path))
                {
                    installed.Add(ShortName(pkg));
                    details.Add(string.Format("  ✓ {0} ({1}) - {2}", pkg.Name, version ?? "Installed", path));
                }
                else
                {
                    missing.Add(ShortName(pkg));
                    string usbFile = SoftwareInstallerService.FindInstallerFile(pkg);
                    details.Add(string.IsNullOrEmpty(usbFile)
                        ? string.Format("  ✗ {0}: ຍັງບໍ່ໄດ້ຕິດຕັ້ງ (ບໍ່ພົບໄຟລ໌ຕິດຕັ້ງໃນ installers/)", pkg.Name)
                        : string.Format("  ✗ {0}: ຍັງບໍ່ໄດ້ຕິດຕັ້ງ (ພົບໄຟລ໌ໃນ USB: {1} → ກົດ \"ແກ້ໄຂ\" ເພື່ອຕິດຕັ້ງ)",
                            pkg.Name, Path.GetFileName(usbFile)));
                }
            }

            // ສະແດງໂປຣແກຣມອື່ນເປັນຂໍ້ມູນເທົ່ານັ້ນ (ບໍ່ມີຜົນຕໍ່ຜົນກວດ)
            details.Add("");
            details.Add("=== ໂປຣແກຣມອື່ນ (ຕິດຕັ້ງດ້ວຍມືໃນແທັບ \"ຕິດຕັ້ງຊອບແວ\") ===");
            foreach (var pkg in SoftwareInstallerService.GetCatalog())
            {
                if (SoftwareInstallerService.IsEssential(pkg)) continue;
                string version;
                string path;
                details.Add(SoftwareInstallerService.IsInstalled(pkg, out version, out path)
                    ? string.Format("  ✓ {0} ({1})", pkg.Name, version ?? "Installed")
                    : string.Format("  - {0}: ບໍ່ໄດ້ຕິດຕັ້ງ", pkg.Name));
            }

            if (missing.Count == 0)
            {
                return new CheckResult(CheckStatus.Pass,
                    string.Format("ຕິດຕັ້ງຄົບແລ້ວ ({0})", string.Join(", ", installed.ToArray())),
                    details);
            }

            return new CheckResult(CheckStatus.Fail,
                string.Format("ຂາດ: {0}", string.Join(", ", missing.ToArray())),
                details);
        }

        /// <summary>
        /// ຕິດຕັ້ງສະເພາະໂປຣແກຣມພື້ນຖານທີ່ຍັງຂາດ ຈາກໂຟນເດີ installers/ ດ້ວຍຄຳສັ່ງ Silent ຂອງແຕ່ລະໂປຣແກຣມ
        /// </summary>
        public CheckResult Fix()
        {
            var details = new List<string>();
            int needed = 0;
            int succeeded = 0;

            foreach (var pkg in SoftwareInstallerService.GetEssentialPackages())
            {
                string version;
                string path;
                if (IsSatisfied(pkg, out version, out path)) continue;

                needed++;
                string installer = SoftwareInstallerService.FindInstallerFile(pkg);
                if (string.IsNullOrEmpty(installer))
                {
                    details.Add(string.Format("  ✗ {0}: ບໍ່ພົບໄຟລ໌ຕິດຕັ້ງ, ກະລຸນາວາງໄຟລ໌ Setup ໄວ້ໃນ installers/", pkg.Name));
                    continue;
                }

                details.Add(string.Format("ກຳລັງຕິດຕັ້ງ {0} ({1})...", pkg.Name, Path.GetFileName(installer)));

                // ຢູ່ໃນ background thread ຂອງ ScanRunner ແລ້ວ ຈຶ່ງລໍຖ້າຜົນໄດ້ໂດຍກົງ
                InstallResult result = SoftwareInstallerService.InstallAsync(pkg, installer).Result;

                // ຢືນຢັນອີກເທື່ອ ວ່າຕິດຕັ້ງແທ້ (ບໍ່ເຊື່ອພຽງ exit code)
                if (result.Success && SoftwareInstallerService.IsInstalled(pkg, out version, out path))
                {
                    succeeded++;
                    details.Add(string.Format("  ✓ {0}: ຕິດຕັ້ງສຳເລັດ ({1})", pkg.Name, version ?? "Installed"));
                }
                else if (result.Success)
                {
                    details.Add(string.Format("  ⚠ {0}: ຕົວຕິດຕັ້ງຈົບແລ້ວ ແຕ່ຍັງກວດບໍ່ພົບໂປຣແກຣມ", pkg.Name));
                }
                else
                {
                    details.Add(string.Format("  ✗ {0}: {1}", pkg.Name, result.Message));
                }
            }

            if (needed == 0)
            {
                return new CheckResult(CheckStatus.Pass, "ໂປຣແກຣມພື້ນຖານຕິດຕັ້ງຄົບແລ້ວ", details);
            }

            string summary = string.Format("ຕິດຕັ້ງສຳເລັດ {0}/{1} ໂປຣແກຣມ", succeeded, needed);
            CheckStatus status = succeeded == needed ? CheckStatus.Pass
                : succeeded > 0 ? CheckStatus.Warning
                : CheckStatus.Fail;
            return new CheckResult(status, summary, details);
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        /// <summary>
        /// ກວດວ່າໂປຣແກຣມນີ້ມີໃນເຄື່ອງແລ້ວ. ສຳລັບ PDF ຍອມຮັບ Adobe Reader ຫຼື Foxit ແທນ SumatraPDF ໄດ້
        /// </summary>
        private static bool IsSatisfied(SoftwarePackage pkg, out string version, out string path)
        {
            if (SoftwareInstallerService.IsInstalled(pkg, out version, out path)) return true;

            if (pkg.Id == "sumatrapdf")
            {
                path = FindFirstExisting(AlternativePdfReaders());
                if (path != null)
                {
                    version = GetFileVersion(path);
                    return true;
                }
            }
            return false;
        }

        private static string ShortName(SoftwarePackage pkg)
        {
            switch (pkg.Id)
            {
                case "sumatrapdf": return "PDF Reader";
                case "laoscript8": return "LaoScript 8";
                case "vlc": return "VLC";
                case "7zip": return "7-Zip";
                default: return pkg.Name;
            }
        }

        private static IEnumerable<string> AlternativePdfReaders()
        {
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            return new[]
            {
                Path.Combine(pf, @"Adobe\Acrobat DC\Acrobat\Acrobat.exe"),
                Path.Combine(pf, @"Adobe\Acrobat Reader DC\Reader\AcroRd32.exe"),
                Path.Combine(pf86, @"Adobe\Acrobat Reader DC\Reader\AcroRd32.exe"),
                Path.Combine(pf, @"Foxit Software\Foxit PDF Reader\FoxitPDFReader.exe"),
                Path.Combine(pf86, @"Foxit Software\Foxit PDF Reader\FoxitPDFReader.exe"),
                Path.Combine(pf86, @"Foxit Software\Foxit Reader\FoxitReader.exe")
            };
        }

        private static string FindFirstExisting(IEnumerable<string> paths)
        {
            foreach (string p in paths)
            {
                if (!string.IsNullOrEmpty(p) && File.Exists(p)) return p;
            }
            return null;
        }

        private static string GetFileVersion(string exePath)
        {
            try
            {
                var fvi = FileVersionInfo.GetVersionInfo(exePath);
                if (!string.IsNullOrEmpty(fvi.ProductVersion)) return fvi.ProductVersion.Trim();
                if (!string.IsNullOrEmpty(fvi.FileVersion)) return fvi.FileVersion.Trim();
            }
            catch { }
            return null;
        }
    }
}
