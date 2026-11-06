using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ClassroomChecker.Native
{
    /// <summary>
    /// ລວບລວມ Native Win32 API (P/Invoke) ສຳລັບການເຮັດວຽກກັບ Windows Shell ແລະ Desktop
    /// </summary>
    public static class NativeMethods
    {
        public const uint SPI_SETDESKWALLPAPER = 0x0014;
        public const uint SPIF_UPDATEINIFILE = 0x01;
        public const uint SPIF_SENDCHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfo(
            uint uAction,
            uint uParam,
            string lpvParam,
            uint fuWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetKeyboardLayoutList(int nBuff, [Out] IntPtr[] lpList);

        // --- Font Installation ---
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int AddFontResourceW(string lpFileName);

        public const int HWND_BROADCAST = 0xffff;
        public const int WM_FONTCHANGE = 0x001D;
        public const int SMTO_ABORTIFHUNG = 0x0002;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            IntPtr lParam,
            uint fuFlags,
            uint uTimeout,
            out IntPtr lpdwResult);

        // --- Keyboard Installation (input.dll) ---
        [DllImport("input.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool InstallLayoutOrTip(string psz, uint dwFlags);

        // --- Recycle Bin Cleanup (shell32.dll) ---
        public const uint SHERB_NOCONFIRMATION = 0x00000001;
        public const uint SHERB_NOPROGRESSUI   = 0x00000002;
        public const uint SHERB_NOSOUND        = 0x00000004;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, uint dwFlags);

        // --- Power Management (powrprof.dll) ---
        [DllImport("powrprof.dll")]
        public static extern uint PowerGetActiveScheme(IntPtr UserRootPowerKey, out IntPtr ActivePolicyGuid);

        [DllImport("powrprof.dll")]
        public static extern uint PowerReadACValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            out uint AcValueIndex);

        [DllImport("powrprof.dll")]
        public static extern uint PowerReadDCValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            out uint DcValueIndex);

        [DllImport("kernel32.dll")]
        public static extern IntPtr LocalFree(IntPtr hMem);

        // Standard Power Setting GUIDs
        public static readonly Guid GUID_VIDEO_SUBGROUP = new Guid("7516b93f-937e-42c1-8f5b-61b2286e1245");
        public static readonly Guid GUID_VIDEO_POWERDOWN_TIMEOUT = new Guid("3c0bc30f-dc79-40ea-8a80-bb8c42440468");

        public static readonly Guid GUID_SLEEP_SUBGROUP = new Guid("238c9fa8-0aad-41ed-83f4-97be242c8f20");
        public static readonly Guid GUID_STANDBY_TIMEOUT = new Guid("29f6c1db-86da-48c5-9fdb-f2b67b1f44da");
        public static readonly Guid GUID_HIBERNATE_TIMEOUT = new Guid("9d7815e6-7ee4-464a-88b3-442960d77891");

        public static readonly Guid GUID_DISK_SUBGROUP = new Guid("0012ee47-9041-4b5d-9b77-535fba8b1442");
        public static readonly Guid GUID_DISK_POWERDOWN_TIMEOUT = new Guid("6738e2c4-e8a5-449a-a16a-605d3021b72a");

        /// <summary>
        /// ຮູບແບບການສະແດງຜົນຂອງ Wallpaper
        /// </summary>
        public enum WallpaperStyle
        {
            Fill,       // 10, 0
            Fit,        // 6, 0
            Stretch,    // 2, 0
            Center,     // 0, 0
            Tile        // 0, 1
        }

        /// <summary>
        /// ຕັ້ງຮູບພື້ນຫຼັງ desktop ໂດຍຂຽນຄ່າລົງ Registry HKCU ແລະ ສັ່ງ SystemParametersInfo
        /// </summary>
        public static bool SetDesktopWallpaper(string imagePath, WallpaperStyle style = WallpaperStyle.Fill)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                throw new FileNotFoundException("ບໍ່ພົບໄຟລ໌ຮູບພື້ນຫຼັງ:", imagePath);
            }

            try
            {
                // 1. ຕັ້ງຄ່າ Registry ໃນ HKCU\Control Panel\Desktop
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
                {
                    if (key != null)
                    {
                        string styleValue = "10";
                        string tileValue = "0";

                        switch (style)
                        {
                            case WallpaperStyle.Fill:
                                styleValue = "10";
                                tileValue = "0";
                                break;
                            case WallpaperStyle.Fit:
                                styleValue = "6";
                                tileValue = "0";
                                break;
                            case WallpaperStyle.Stretch:
                                styleValue = "2";
                                tileValue = "0";
                                break;
                            case WallpaperStyle.Center:
                                styleValue = "0";
                                tileValue = "0";
                                break;
                            case WallpaperStyle.Tile:
                                styleValue = "0";
                                tileValue = "1";
                                break;
                        }

                        key.SetValue("WallpaperStyle", styleValue);
                        key.SetValue("TileWallpaper", tileValue);
                    }
                }

                // 2. ເອີ້ນ Win32 API ເພື່ອ apply ທັນທີ ແລະ broadcast ແຈ້ງ Shell
                bool success = SystemParametersInfo(
                    SPI_SETDESKWALLPAPER,
                    0,
                    imagePath,
                    SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SetDesktopWallpaper error: " + ex.Message);
                return false;
            }
        }
    }
}
