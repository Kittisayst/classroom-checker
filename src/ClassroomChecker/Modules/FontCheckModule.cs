using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using ClassroomChecker.Config;
using ClassroomChecker.Core;
using ClassroomChecker.Native;
using Microsoft.Win32;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ໂມດູນກວດສອບ ແລະ ຕິດຕັ້ງ Fonts ພາສາລາວ (Phetsarath OT) ຕາມ cm-spec.md ຂໍ້ 5.2
    /// </summary>
    public class FontCheckModule : ICheckModule
    {
        public string Id
        {
            get { return "font"; }
        }

        public string DisplayNameKey
        {
            get { return "module_font"; }
        }

        public bool CanFix
        {
            get { return true; }
        }

        public CheckResult Check()
        {
            var details = new List<string>();
            var requiredFonts = ConfigService.Instance.Current.requiredFonts;
            if (requiredFonts == null || requiredFonts.Count == 0)
            {
                requiredFonts = new List<string> { "Phetsarath OT" };
            }

            var missingFonts = new List<string>();

            try
            {
                using (InstalledFontCollection installed = new InstalledFontCollection())
                {
                    var installedFamilies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var f in installed.Families)
                    {
                        installedFamilies.Add(f.Name);
                    }

                    foreach (string req in requiredFonts)
                    {
                        if (installedFamilies.Contains(req))
                        {
                            details.Add("Font '" + req + "': ຕິດຕັ້ງແລ້ວ (Installed)");
                        }
                        else
                        {
                            details.Add("Font '" + req + "': ຍັງບໍ່ໄດ້ຕິດຕັ້ງ (Missing)");
                            missingFonts.Add(req);
                        }
                    }
                }

                if (missingFonts.Count == 0)
                {
                    string summary = string.Format("ຕິດຕັ້ງ Font ຄົບຖ້ວນ ({0})", string.Join(", ", requiredFonts.ToArray()));
                    return new CheckResult(CheckStatus.Pass, summary, details);
                }
                else
                {
                    string summary = string.Format("ຂາດ Font: {0}", string.Join(", ", missingFonts.ToArray()));
                    return new CheckResult(CheckStatus.Fail, summary, details);
                }
            }
            catch (Exception ex)
            {
                details.Add("Error: " + ex.Message);
                return new CheckResult(CheckStatus.Unknown, "ເກີດຂໍ້ຜິດພາດໃນການກວດ Font: " + ex.Message, details);
            }
        }

        public CheckResult Fix()
        {
            var details = new List<string>();

            try
            {
                // 1. ກຽມ byte stream ຂອງ PhetsarathOT.ttf (ຈາກ embedded resource ຫຼື ໂຟນເດີ fonts/ ຂ້າງ exe)
                byte[] fontBytes = LoadPhetsarathFontBytes();
                if (fontBytes == null || fontBytes.Length == 0)
                {
                    return new CheckResult(CheckStatus.Fail, "ບໍ່ພົບໄຟລ໌ PhetsarathOT.ttf ສຳລັບຕິດຕັ້ງ", details);
                }

                // 2. ເປົ້າໝາຍປາຍທາງ: C:\Windows\Fonts (ລະດັບລະບົບ)
                string winFontsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
                string destPath = Path.Combine(winFontsDir, "PhetsarathOT.ttf");

                bool writeSuccess = false;
                try
                {
                    File.WriteAllBytes(destPath, fontBytes);
                    writeSuccess = true;
                    details.Add("Copied to System Fonts: " + destPath);
                }
                catch (Exception ex)
                {
                    details.Add("System Fonts Write Warning: " + ex.Message);
                }

                // ຖ້າຂຽນລົງ Windows\Fonts ບໍ່ໄດ້ (ເຊັ່ນ ສິດບໍ່ພໍ) ໃຫ້ fallback ໄປ User Fonts
                if (!writeSuccess)
                {
                    string userFontsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Fonts");
                    if (!Directory.Exists(userFontsDir))
                    {
                        Directory.CreateDirectory(userFontsDir);
                    }
                    destPath = Path.Combine(userFontsDir, "PhetsarathOT.ttf");
                    File.WriteAllBytes(destPath, fontBytes);
                    details.Add("Copied to User Fonts: " + destPath);
                }

                // 3. ລົງທະບຽນໃນ Registry
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", true))
                    {
                        if (key != null)
                        {
                            key.SetValue("Phetsarath OT (TrueType)", Path.GetFileName(destPath));
                            details.Add("Registered in HKLM Fonts registry");
                        }
                    }
                }
                catch { }

                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Fonts", true))
                    {
                        if (key != null)
                        {
                            key.SetValue("Phetsarath OT (TrueType)", destPath);
                            details.Add("Registered in HKCU Fonts registry");
                        }
                    }
                }
                catch { }

                // 4. ເອີ້ນ P/Invoke AddFontResourceW
                int added = NativeMethods.AddFontResourceW(destPath);
                details.Add("AddFontResourceW returned: " + added);

                // 5. Broadcast WM_FONTCHANGE
                IntPtr result;
                NativeMethods.SendMessageTimeout(
                    (IntPtr)NativeMethods.HWND_BROADCAST,
                    (uint)NativeMethods.WM_FONTCHANGE,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    (uint)NativeMethods.SMTO_ABORTIFHUNG,
                    1000,
                    out result);

                // ສະແກນຊ້ຳເພື່ອຢືນຢັນ
                var recheck = Check();
                if (recheck.Status == CheckStatus.Pass)
                {
                    return new CheckResult(CheckStatus.Pass, "ຕິດຕັ້ງ Font Phetsarath OT ສຳເລັດແລ້ວ!", details);
                }
                else
                {
                    return new CheckResult(CheckStatus.Warning, "ຕິດຕັ້ງແລ້ວ ແຕ່ລະບົບອາດຕ້ອງການ Refresh ແອັບ", details);
                }
            }
            catch (Exception ex)
            {
                details.Add("Error: " + ex.Message);
                return new CheckResult(CheckStatus.Fail, "ເກີດຂໍ້ຜິດພາດໃນການຕິດຕັ້ງ Font: " + ex.Message, details);
            }
        }

        private byte[] LoadPhetsarathFontBytes()
        {
            // 1. ລອງອ່ານຈາກໂຟນເດີ fonts/ ຂ້າງ exe
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string localFont = Path.Combine(exeDir, "fonts", "PhetsarathOT.ttf");
            if (File.Exists(localFont))
            {
                return File.ReadAllBytes(localFont);
            }

            // 2. ລອງອ່ານຈາກ EmbeddedResource
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resNames = {
                "ClassroomChecker.Assets.Fonts.PhetsarathOT.ttf",
                "ClassroomChecker.Assets.PhetsarathOT.ttf"
            };

            foreach (string name in resNames)
            {
                using (Stream stream = asm.GetManifestResourceStream(name))
                {
                    if (stream != null)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            return ms.ToArray();
                        }
                    }
                }
            }

            return null;
        }
    }
}
