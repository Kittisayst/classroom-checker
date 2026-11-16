using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
        /// ກວດສອບວ່າ ຮູບແບບວັນທີ່ເປັນ ວັນທີ່/ເດືອນ/ປີ (dd/MM/yyyy) ຫຼື ບໍ່
        /// </summary>
        public static bool IsDateFormatDmy(out string currentFormat, out string iDate)
        {
            currentFormat = string.Empty;
            iDate = string.Empty;

            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\International", false))
                {
                    if (key != null)
                    {
                        object fmt = key.GetValue("sShortDate");
                        object idt = key.GetValue("iDate");

                        currentFormat = fmt != null ? fmt.ToString() : string.Empty;
                        iDate = idt != null ? idt.ToString() : string.Empty;

                        if (string.Equals(currentFormat, "dd/MM/yyyy", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(currentFormat, "d/M/yyyy", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        // ກວດຖ້າເລີ່ມດ້ວຍ d ແລະ ມີ M ແລະ yyyy
                        string lower = currentFormat.ToLowerInvariant();
                        if (lower.StartsWith("d") && lower.Contains("m") && lower.EndsWith("yyyy") && iDate == "1")
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                currentFormat = "Error: " + ex.Message;
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
            catch { }

            detail = "Could not set timezone";
            return false;
        }

        /// <summary>
        /// ຕັ້ງຮູບແບບວັນທີ່ເປັນ dd/MM/yyyy (ວັນທີ່/ເດືອນ/ປີ) ແລະ ແຈ້ງເຕືອນ Windows ໃຫ້ອັບເດດທັນທີ
        /// </summary>
        public static bool SetDateFormatDmy(out string detail)
        {
            bool ok = false;
            try
            {
                // 1. ຂຽນໃສ່ HKCU Control Panel\International
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\International", true))
                {
                    if (key != null)
                    {
                        key.SetValue("sShortDate", TARGET_DATE_FORMAT, RegistryValueKind.String);
                        key.SetValue("sDate", TARGET_DATE_SEPARATOR, RegistryValueKind.String);
                        key.SetValue("iDate", TARGET_DATE_ORDER, RegistryValueKind.String);
                        key.SetValue("sTimeFormat", "HH:mm:ss", RegistryValueKind.String);
                        key.SetValue("sShortTime", "HH:mm", RegistryValueKind.String);
                        ok = true;
                    }
                }

                // 2. ລອງຂຽນໃສ່ .DEFAULT (ສຳລັບ User ໃໝ່ ແລະ ໜ້າຈໍ Login)
                try
                {
                    using (var defKey = Registry.Users.OpenSubKey(@".DEFAULT\Control Panel\International", true))
                    {
                        if (defKey != null)
                        {
                            defKey.SetValue("sShortDate", TARGET_DATE_FORMAT, RegistryValueKind.String);
                            defKey.SetValue("sDate", TARGET_DATE_SEPARATOR, RegistryValueKind.String);
                            defKey.SetValue("iDate", TARGET_DATE_ORDER, RegistryValueKind.String);
                        }
                    }
                }
                catch { }

                // 3. ແຈ້ງເຕືອນ Windows Shell (Taskbar, Clock, Explorer) ໃຫ້ Refresh ຮູບແບບວັນທີ່ທັນທີ
                try
                {
                    UIntPtr result;
                    SendMessageTimeout((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, UIntPtr.Zero, "intl", SMTO_ABORTIFHUNG, 2000, out result);
                }
                catch { }

                detail = ok ? "Date format set to dd/MM/yyyy (d/M/y)" : "Failed to write International registry";
                return ok;
            }
            catch (Exception ex)
            {
                detail = "Error setting date format: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// ນຳໃຊ້ທັງ Timezone +7 ແລະ ຮູບແບບວັນທີ່ dd/MM/yyyy
        /// </summary>
        public static bool ApplyAll(out List<string> logs)
        {
            logs = new List<string>();

            string tzMsg;
            bool tzOk = SetTimezoneUtc7(out tzMsg);
            logs.Add(tzMsg);

            string dateMsg;
            bool dateOk = SetDateFormatDmy(out dateMsg);
            logs.Add(dateMsg);

            return tzOk && dateOk;
        }
    }
}
