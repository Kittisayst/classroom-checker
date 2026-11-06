using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using ClassroomChecker.Core;
using ClassroomChecker.Native;
using Microsoft.Win32;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ກວດສອບ ແລະ ຕິດຕັ້ງແປ້ນພິມພາສາລາວ (LANGID 0x0454 / KLID 00000454) ຕາມ cm-spec.md ຂໍ້ 5.2 ແລະ ADR-0003
    /// </summary>
    public class KeyboardCheckModule : ICheckModule
    {
        public const int LAO_LANGID = 0x0454;
        public const string LAO_KLID = "00000454";

        public string Id
        {
            get { return "keyboard"; }
        }

        public string DisplayNameKey
        {
            get { return "module_keyboard"; }
        }

        public bool CanFix
        {
            get { return true; }
        }

        public CheckResult Check()
        {
            var details = new List<string>();
            bool hasLaoLayout = false;

            try
            {
                // 1. ກວດຜ່ານ GetKeyboardLayoutList P/Invoke
                int count = NativeMethods.GetKeyboardLayoutList(0, null);
                if (count > 0)
                {
                    IntPtr[] layouts = new IntPtr[count];
                    NativeMethods.GetKeyboardLayoutList(count, layouts);

                    for (int i = 0; i < count; i++)
                    {
                        long hkl = layouts[i].ToInt64();
                        int langId = (int)(hkl & 0xFFFF);
                        int deviceId = (int)((hkl >> 16) & 0xFFFF);

                        string langName = "Unknown";
                        try
                        {
                            langName = new CultureInfo(langId).DisplayName;
                        }
                        catch { }

                        details.Add(string.Format("Layout {0}: LangID 0x{1:X4} ({2}), DeviceID 0x{3:X4}",
                            i + 1, langId, langName, deviceId));

                        if (langId == LAO_LANGID || deviceId == LAO_LANGID)
                        {
                            hasLaoLayout = true;
                        }
                    }
                }

                // 2. ກວດ Registry HKCU\Keyboard Layout\Preload ເພີ່ມເຕີມ
                using (RegistryKey preloadKey = Registry.CurrentUser.OpenSubKey(@"Keyboard Layout\Preload"))
                {
                    if (preloadKey != null)
                    {
                        string[] valNames = preloadKey.GetValueNames();
                        foreach (string valName in valNames)
                        {
                            string klid = preloadKey.GetValue(valName) as string;
                            if (!string.IsNullOrEmpty(klid))
                            {
                                if (klid.EndsWith(LAO_KLID, StringComparison.OrdinalIgnoreCase) ||
                                    klid.EndsWith("0454", StringComparison.OrdinalIgnoreCase))
                                {
                                    hasLaoLayout = true;
                                    details.Add("Preload Registry: ພົບ KLID 00000454 (ລາວ)");
                                }
                            }
                        }
                    }
                }

                if (hasLaoLayout)
                {
                    return new CheckResult(CheckStatus.Pass, "ຕິດຕັ້ງແປ້ນພິມພາສາລາວຮຽບຮ້ອຍແລ້ວ (Lao Keyboard Installed)", details);
                }
                else
                {
                    return new CheckResult(CheckStatus.Fail, "ຍັງບໍ່ທັນໄດ້ຕິດຕັ້ງແປ້ນພິມພາສາລາວ (Missing Lao Keyboard)", details);
                }
            }
            catch (Exception ex)
            {
                details.Add("Error: " + ex.Message);
                return new CheckResult(CheckStatus.Unknown, "ເກີດຂໍ້ຜິດພາດໃນການກວດແປ້ນພິມ: " + ex.Message, details);
            }
        }

        public CheckResult Fix()
        {
            var details = new List<string>();

            try
            {
                // ວິທີທີ 1: P/Invoke InstallLayoutOrTip (input.dll)
                bool installed = false;
                try
                {
                    installed = NativeMethods.InstallLayoutOrTip("0454:00000454", 0);
                    details.Add("InstallLayoutOrTip('0454:00000454') returned: " + installed);
                }
                catch (Exception ex)
                {
                    details.Add("InstallLayoutOrTip failed: " + ex.Message);
                }

                // ຖ້າວິທີທີ 1 ບໍ່ສຳເລັດ ໃຫ້ໃຊ້ ວິທີທີ 2 (Fallback): PowerShell Non-destructive Append (ADR-0003)
                if (!installed)
                {
                    string psCommand = "$list = Get-WinUserLanguageList; if (-not ($list | Where-Object { $_.LanguageTag -like 'lo*' })) { $list.Add('lo-LA'); Set-WinUserLanguageList $list -Force }";
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"" + psCommand + "\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using (Process p = Process.Start(psi))
                    {
                        p.WaitForExit(10000);
                        details.Add("PowerShell Set-WinUserLanguageList exited with code " + p.ExitCode);
                    }
                }

                // ສະແກນຊ້ຳເພື່ອຢືນຢັນ
                var recheck = Check();
                if (recheck.Status == CheckStatus.Pass)
                {
                    return new CheckResult(CheckStatus.Pass, "ຕິດຕັ້ງແປ້ນພິມພາສາລາວສຳເລັດແລ້ວ!", details);
                }
                else
                {
                    return new CheckResult(CheckStatus.Warning, "ດຳເນີນການແລ້ວ ແຕ່ລະບົບອາດຕ້ອງການ Sign out/Restart ເພື່ອໃຫ້ Taskbar ສະແດງ", details);
                }
            }
            catch (Exception ex)
            {
                details.Add("Error: " + ex.Message);
                return new CheckResult(CheckStatus.Fail, "ເກີດຂໍ້ຜິດພາດໃນການຕິດຕັ້ງແປ້ນພິມ: " + ex.Message, details);
            }
        }
    }
}
