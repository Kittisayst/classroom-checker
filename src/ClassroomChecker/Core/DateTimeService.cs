using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using ClassroomChecker.Config;
using Microsoft.Win32;

namespace ClassroomChecker.Core
{
    /// <summary>
    /// ຈັດການການຕັ້ງຄ່າ Timezone (+7 SE Asia Standard Time) ແລະ ຮູບແບບວັນທີ່ (dd/MM/yyyy)
    /// </summary>
    public static class DateTimeService
    {
        public const string TARGET_TIMEZONE_ID = "SE Asia Standard Time";
        public const string TARGET_DATE_FORMAT = "dd/MM/yyyy";
        public const string TARGET_DATE_SEPARATOR = "/";
        public const string TARGET_DATE_ORDER = "1"; // 1 = d/M/y

        private const int HWND_BROADCAST = 0xffff;
        private const uint WM_SETTINGCHANGE = 0x001A;
        private const uint SMTO_ABORTIFHUNG = 0x0002;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            UIntPtr wParam,
            string lParam,
            uint fuFlags,
            uint uTimeout,
            out UIntPtr lpdwResult);

        /// <summary>
        /// ກວດສອບວ່າ Timezone ຂອງເຄື່ອງເປັນ UTC+7 (SE Asia Standard Time) ຫຼື ບໍ່
        /// </summary>
        public static bool IsTimezoneUtc7(out string currentTzId, out double offsetHours)
        {
            try
            {
                var local = TimeZoneInfo.Local;
                currentTzId = local.Id;
                offsetHours = local.BaseUtcOffset.TotalHours;

                if (local.Id.Equals(TARGET_TIMEZONE_ID, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (Math.Abs(offsetHours - 7.0) < 0.05)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                currentTzId = ex.Message;
                offsetHours = 0;
            }

            return false;
        }

        /// <summary>
        /// ຕັ້ງ Timezone ເປັນ SE Asia Standard Time (UTC+07:00 Bangkok, Hanoi, Jakarta)
        /// </summary>
        public static bool SetTimezoneUtc7(out string detail)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "tzutil.exe",
                    Arguments = "/s \"" + TARGET_TIMEZONE_ID + "\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var proc = Process.Start(psi))
                {
                    if (proc != null)
                    {
                        proc.WaitForExit(4000);
                        if (proc.ExitCode == 0)
                        {
                            detail = "Timezone set to SE Asia Standard Time (UTC+07:00)";
                            return true;
                        }
                        detail = "tzutil exit code: " + proc.ExitCode;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                detail = "tzutil error: " + ex.Message;
            }

            // Fallback Registry (ຖ້າ tzutil ບໍ່ຕອບສະໜອງ)
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\TimeZoneInformation", true))
                {
                    if (key != null)
                    {
                        key.SetValue("TimeZoneKeyName", TARGET_TIMEZONE_ID, RegistryValueKind.String);
                        key.SetValue("StandardName", TARGET_TIMEZONE_ID, RegistryValueKind.String);
                        key.SetValue("Bias", -420, RegistryValueKind.DWord); // -420 mins = +7h
                        detail = "Timezone set via Registry fallback";
                        return true;
                    }
                }
            }
            catch (Exception ex) { ReportService.LogError("DateTimeService.SetTimezoneUtc7", ex); }

            detail = "Could not set timezone";
            return false;
        }

        // =====================================================================
        // ຮູບແບບວັນທີ ແລະ ເວລາ (ຕາມ config dateTime)
        // =====================================================================

        /// <summary>ຮູບແບບວັນທີທີ່ເລືອກໄດ້ໃນໜ້າຕັ້ງຄ່າ</summary>
        public static readonly string[] SupportedDateFormats = { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "yyyy-MM-dd" };

        public const string TIME_24_SHORT = "HH:mm";
        public const string TIME_24_LONG = "HH:mm:ss";
        public const string TIME_12_SHORT = "h:mm tt";
        public const string TIME_12_LONG = "h:mm:ss tt";

        public static bool IsSupportedDateFormat(string format)
        {
            return format != null && Array.IndexOf(SupportedDateFormats, format) >= 0;
        }

        public static string GetShortTimePattern(bool use24Hour)
        {
            return use24Hour ? TIME_24_SHORT : TIME_12_SHORT;
        }

        public static string GetLongTimePattern(bool use24Hour)
        {
            return use24Hour ? TIME_24_LONG : TIME_12_LONG;
        }

        /// <summary>ຕົວແຍກວັນທີ (ຕົວອັກສອນທຳອິດທີ່ບໍ່ແມ່ນ d/M/y), ເຊັ່ນ "/" ຫຼື "-"</summary>
        internal static string GetDateSeparator(string dateFormat)
        {
            foreach (char c in dateFormat ?? string.Empty)
            {
                if (c != 'd' && c != 'M' && c != 'y') return c.ToString();
            }
            return TARGET_DATE_SEPARATOR;
        }

        /// <summary>ລຳດັບວັນທີຂອງ Windows (iDate): 0 = M/d/y, 1 = d/M/y, 2 = y/M/d</summary>
        internal static string GetDateOrder(string dateFormat)
        {
            if (string.IsNullOrEmpty(dateFormat)) return TARGET_DATE_ORDER;
            switch (dateFormat[0])
            {
                case 'M': return "0";
                case 'y': return "2";
                default: return "1";
            }
        }

        /// <summary>ຕົວຢ່າງວັນທີ + ເວລາ ຕາມຮູບແບບທີ່ເລືອກ (ໃຊ້ສະແດງໃນໜ້າຕັ້ງຄ່າ)</summary>
        public static string FormatPreview(DateTime value, string dateFormat, bool use24Hour)
        {
            string pattern = (IsSupportedDateFormat(dateFormat) ? dateFormat : TARGET_DATE_FORMAT)
                + " " + GetLongTimePattern(use24Hour);
            return value.ToString(pattern, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// ກວດວ່າຮູບແບບວັນທີ ແລະ ເວລາຂອງ user ປັດຈຸບັນ ກົງກັບ config ຫຼື ບໍ່
        /// </summary>
        public static bool IsDateTimeFormatOk(DateTimeConfig cfg, out bool dateOk, out bool timeOk, out string currentDate, out string currentTime)
        {
            if (cfg == null) cfg = new DateTimeConfig();
            dateOk = false;
            timeOk = false;
            currentDate = string.Empty;
            currentTime = string.Empty;

            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\International", false))
                {
                    if (key != null)
                    {
                        currentDate = (key.GetValue("sShortDate") ?? string.Empty).ToString();
                        currentTime = (key.GetValue("sShortTime") ?? string.Empty).ToString();
                        string iTime = (key.GetValue("iTime") ?? string.Empty).ToString();

                        dateOk = string.Equals(currentDate, cfg.dateFormat, StringComparison.Ordinal);
                        timeOk = string.Equals(currentTime, GetShortTimePattern(cfg.use24Hour), StringComparison.Ordinal)
                            && iTime == (cfg.use24Hour ? "1" : "0");
                    }
                }
            }
            catch (Exception ex)
            {
                currentDate = "Error: " + ex.Message;
            }

            return dateOk && timeOk;
        }

        /// <summary>
        /// ຕັ້ງຮູບແບບວັນທີ ແລະ ເວລາ (12/24 ຊົ່ວໂມງ) ຕາມ config ໃຫ້ user ປັດຈຸບັນ ແລະ ໜ້າ Login (.DEFAULT)
        /// ແລ້ວແຈ້ງ Windows ໃຫ້ອັບເດດໂມງ Taskbar ທັນທີ
        /// </summary>
        public static bool SetDateTimeFormat(DateTimeConfig cfg, out string detail)
        {
            if (cfg == null) cfg = new DateTimeConfig();
            string dateFormat = IsSupportedDateFormat(cfg.dateFormat) ? cfg.dateFormat : TARGET_DATE_FORMAT;

            try
            {
                bool ok = false;
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\International", true))
                {
                    if (key != null)
                    {
                        WriteInternational(key, dateFormat, cfg.use24Hour);
                        ok = true;
                    }
                }

                try
                {
                    using (var defKey = Registry.Users.OpenSubKey(@".DEFAULT\Control Panel\International", true))
                    {
                        if (defKey != null) WriteInternational(defKey, dateFormat, cfg.use24Hour);
                    }
                }
                catch (Exception ex) { ReportService.LogError("DateTimeService.SetDateTimeFormat", ex); }

                try
                {
                    UIntPtr result;
                    SendMessageTimeout((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, UIntPtr.Zero, "intl", SMTO_ABORTIFHUNG, 2000, out result);
                }
                catch { }

                detail = ok
                    ? string.Format("Date/time format set to {0} {1}", dateFormat, GetShortTimePattern(cfg.use24Hour))
                    : "Failed to write International registry";
                return ok;
            }
            catch (Exception ex)
            {
                detail = "Error setting date/time format: " + ex.Message;
                return false;
            }
        }

        private static void WriteInternational(RegistryKey key, string dateFormat, bool use24Hour)
        {
            key.SetValue("sShortDate", dateFormat, RegistryValueKind.String);
            key.SetValue("sDate", GetDateSeparator(dateFormat), RegistryValueKind.String);
            key.SetValue("iDate", GetDateOrder(dateFormat), RegistryValueKind.String);

            key.SetValue("sShortTime", GetShortTimePattern(use24Hour), RegistryValueKind.String);
            key.SetValue("sTimeFormat", GetLongTimePattern(use24Hour), RegistryValueKind.String);
            key.SetValue("iTime", use24Hour ? "1" : "0", RegistryValueKind.String);   // 1 = 24 ຊົ່ວໂມງ
            key.SetValue("iTLZero", use24Hour ? "1" : "0", RegistryValueKind.String); // 24h ມີເລກ 0 ນຳໜ້າ (09:05)
            key.SetValue("s1159", "AM", RegistryValueKind.String);
            key.SetValue("s2359", "PM", RegistryValueKind.String);
        }

        /// <summary>
        /// ນຳໃຊ້ທັງ Timezone +7 ແລະ ຮູບແບບວັນທີ/ເວລາ ຕາມ config
        /// </summary>
        public static bool ApplyAll(DateTimeConfig cfg, out List<string> logs)
        {
            logs = new List<string>();

            string tzMsg;
            bool tzOk = SetTimezoneUtc7(out tzMsg);
            logs.Add(tzMsg);

            string fmtMsg;
            bool fmtOk = SetDateTimeFormat(cfg, out fmtMsg);
            logs.Add(fmtMsg);

            return tzOk && fmtOk;
        }
    }
}
