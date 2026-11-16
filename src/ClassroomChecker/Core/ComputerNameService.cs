using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace ClassroomChecker.Core
{
    /// <summary>
    /// ບໍລິການຈັດການຊື່ຄອມພິວເຕີ (Computer Name / Hostname Service)
    /// ກວດສອບ, ສ້າງຊື່ມາດຕະຖານຕາມຫ້ອງ ແລະ ເລກເຄື່ອງ (ເຊັ່ນ COM1-01), ປ່ຽນຊື່ຜ່ານ WMI & Win32 API
    /// </summary>
    public static class ComputerNameService
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetComputerNameEx(int NameType, string lpBuffer);

        private const int ComputerNamePhysicalDnsHostname = 5;

        /// <summary>
        /// ດຶງຊື່ Hostname ປັດຈຸບັນຂອງເຄື່ອງ
        /// </summary>
        public static string GetCurrentName()
        {
            try
            {
                return Environment.MachineName;
            }
            catch
            {
                return "UNKNOWN";
            }
        }

        /// <summary>
        /// ດຶງຊື່ໃໝ່ທີ່ກຳລັງລໍຖ້າ Restart (ຖ້າມີ)
        /// ປຽບທຽບ HKLM\...\ComputerName ກັບ ActiveComputerName
        /// </summary>
        public static string GetPendingName()
        {
            try
            {
                string activeName = "";
                string configuredName = "";

                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName"))
                {
                    if (key != null) activeName = (key.GetValue("ComputerName") as string) ?? "";
                }

                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName"))
                {
                    if (key != null) configuredName = (key.GetValue("ComputerName") as string) ?? "";
                }

                if (!string.IsNullOrEmpty(configuredName) &&
                    !string.IsNullOrEmpty(activeName) &&
                    !configuredName.Equals(activeName, StringComparison.OrdinalIgnoreCase))
                {
                    return configuredName.ToUpperInvariant();
                }
            }
            catch (Exception ex)
            {
                ReportService.Log("GetPendingName error: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// ກວດສອບວ່າມີການປ່ຽນຊື່ເຄື່ອງທີ່ລໍຖ້າ Restart ຢູ່ຫຼືບໍ່
        /// </summary>
        public static bool IsRestartPending()
        {
            return !string.IsNullOrEmpty(GetPendingName());
        }

        /// <summary>
        /// ສ້າງຊື່ມາດຕະຖານອັດຕະໂນມັດ ຈາກຊື່ຫ້ອງ ແລະ ເລກເຄື່ອງ
        /// ຕົວຢ່າງ: "ຫ້ອງຄອມພິວເຕີ 1", 1 -> "COM1-01"
        ///         "ຫ້ອງ Lab 2", 5 -> "COM2-05"
        /// </summary>
        public static string GenerateDefaultName(string roomName, int number)
        {
            string prefix = "COM1";

            if (!string.IsNullOrEmpty(roomName))
            {
                // ຊອກຫາຕົວເລກໃນຊື່ຫ້ອງຮຽນ ເຊັ່ນ "ຫ້ອງຄອມພິວເຕີ 2" -> 2
                Match match = Regex.Match(roomName, @"\d+");
                if (match.Success)
                {
                    prefix = "COM" + match.Value;
                }
                else
                {
                    // ຖ້າບໍ່ມີຕົວເລກ ລອງດຶງຕົວອັກສອນພາສາອັງກິດ (ຖ້າມີ)
                    Match engMatch = Regex.Match(roomName, @"[A-Za-z]+");
                    if (engMatch.Success)
                    {
                        string eng = engMatch.Value.ToUpperInvariant();
                        if (eng.Length > 5) eng = eng.Substring(0, 5);
                        prefix = eng;
                    }
                }
            }

            int safeNumber = Math.Max(1, Math.Min(999, number));
            string result = string.Format("{0}-{1:D2}", prefix, safeNumber);

            // NetBIOS Hostname limit: 15 chars max
            if (result.Length > 15)
            {
                result = result.Substring(0, 15);
            }

            return result.ToUpperInvariant();
        }

        /// <summary>
        /// ລ້າງຕົວອັກສອນທີ່ບໍ່ຖືກຕ້ອງ ແລະ ປັບໃຫ້ຖືກຕ້ອງຕາມມາດຕະຖານ NetBIOS/DNS (A-Z, 0-9, -)
        /// </summary>
        public static string SanitizeName(string rawInput)
        {
            if (string.IsNullOrWhiteSpace(rawInput)) return "";

            string cleaned = rawInput.Trim().ToUpperInvariant();

            // ປ່ຽນອັກຂະລະພິເສດ ຫຼື ຍະຫວ່າງ ເປັນຂີດກາງ (-)
            cleaned = Regex.Replace(cleaned, @"[^A-Z0-9\-]", "-");

            // ລຶບຂີດກາງທີ່ຊ້ຳກັນ ເຊັ່ນ -- ເປັນ -
            cleaned = Regex.Replace(cleaned, @"\-+", "-");

            // ຕັດຂີດກາງທາງໜ້າ ແລະ ທາງຫຼັງອອກ
            cleaned = cleaned.Trim('-');

            if (cleaned.Length > 15)
            {
                cleaned = cleaned.Substring(0, 15).TrimEnd('-');
            }

            return cleaned;
        }

        /// <summary>
        /// ກວດສອບຄວາມຖືກຕ້ອງຂອງຊື່ຄອມພິວເຕີຕາມມາດຕະຖານ Windows
        /// </summary>
        public static bool ValidateName(string name, out string error)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(name))
            {
                error = "ຊື່ຄອມພິວເຕີບໍ່ສາມາດຫວ່າງເປົ່າໄດ້ (Computer name cannot be empty)";
                return false;
            }

            string trimmed = name.Trim();

            if (trimmed.Length < 1 || trimmed.Length > 15)
            {
                error = "ຊື່ຄອມພິວເຕີຕ້ອງມີຄວາມຍາວ 1 - 15 ຕົວອັກສອນ (Must be 1-15 characters)";
                return false;
            }

            // ຕ້ອງປະກອບດ້ວຍ A-Z, 0-9 ແລະ - ເທົ່ານັ້ນ ຫ້າມຂຶ້ນຕົ້ນ ຫຼື ລົງທ້າຍດ້ວຍຂີດກາງ
            if (!Regex.IsMatch(trimmed, @"^[A-Za-z0-9]([A-Za-z0-9\-]*[A-Za-z0-9])?$"))
            {
                error = "ຊື່ຕ້ອງປະກອບດ້ວຍ A-Z, 0-9 ແລະ ຂີດກາງ (-) ເທົ່ານັ້ນ, ຫ້າມຂຶ້ນຕົ້ນ ຫຼື ລົງທ້າຍດ້ວຍຂີດກາງ";
                return false;
            }

            // ບໍ່ຄວນເປັນຕົວເລກລ້ວນໆ
            if (Regex.IsMatch(trimmed, @"^\d+$"))
            {
                error = "ຊື່ຄອມພິວເຕີບໍ່ຄວນເປັນຕົວເລກລ້ວນໆ (Cannot be only numbers)";
                return false;
            }

            return true;
        }

        /// <summary>
        /// ປ່ຽນຊື່ຄອມພິວເຕີໃນ Windows ຜ່ານ WMI ແລະ Win32 API / Registry
        /// </summary>
        public static bool RenameComputer(string targetName, out string statusMessage)
        {
            statusMessage = "";

            string sanitized = SanitizeName(targetName);
            string valError;
            if (!ValidateName(sanitized, out valError))
            {
                statusMessage = valError;
                return false;
            }

            string current = GetCurrentName();
            if (current.Equals(sanitized, StringComparison.OrdinalIgnoreCase))
            {
                statusMessage = string.Format("ຊື່ເຄື່ອງປັດຈຸບັນແມ່ນ {0} ຢູ່ແລ້ວ", sanitized);
                return true;
            }

            string pending = GetPendingName();
            if (!string.IsNullOrEmpty(pending) && pending.Equals(sanitized, StringComparison.OrdinalIgnoreCase))
            {
                statusMessage = string.Format("ປ່ຽນຊື່ເຄື່ອງເປັນ {0} ແລ້ວ (ກະລຸນາ Restart ເພື່ອໃຫ້ມີຜົນສົມບູນ)", sanitized);
                return true;
            }

            ReportService.Log(string.Format("Renaming computer from '{0}' to '{1}'...", current, sanitized));

            bool wmiSuccess = false;
            uint wmiReturnCode = 9999;

            // 1. ລອງປ່ຽນຜ່ານ WMI Win32_ComputerSystem.Rename
            try
            {
                using (var csClass = new ManagementClass("Win32_ComputerSystem"))
                using (var instances = csClass.GetInstances())
                {
                    foreach (ManagementObject cs in instances)
                    {
                        var inParams = cs.GetMethodParameters("Rename");
                        inParams["Name"] = sanitized;
                        var outParams = cs.InvokeMethod("Rename", inParams, null);
                        if (outParams != null && outParams["ReturnValue"] != null)
                        {
                            wmiReturnCode = Convert.ToUInt32(outParams["ReturnValue"]);
                            if (wmiReturnCode == 0)
                            {
                                wmiSuccess = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportService.Log("WMI Rename exception: " + ex.Message);
            }

            // 2. Win32 API SetComputerNameEx Fallback
            bool win32Success = false;
            try
            {
                win32Success = SetComputerNameEx(ComputerNamePhysicalDnsHostname, sanitized);
            }
            catch (Exception ex)
            {
                ReportService.Log("SetComputerNameEx exception: " + ex.Message);
            }

            // 3. ອັບເດດ Registry ໂດຍກົງເພື່ອຄວາມແນ່ນອນ (HKLM\SYSTEM\CurrentControlSet\...)
            bool regSuccess = false;
            try
            {
                using (var compNameKey = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName"))
                {
                    if (compNameKey != null)
                    {
                        compNameKey.SetValue("ComputerName", sanitized, RegistryValueKind.String);
                        regSuccess = true;
                    }
                }

                using (var tcpKey = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters"))
                {
                    if (tcpKey != null)
                    {
                        tcpKey.SetValue("NV Hostname", sanitized, RegistryValueKind.String);
                        tcpKey.SetValue("Hostname", sanitized, RegistryValueKind.String);
                    }
                }
            }
            catch (Exception ex)
            {
                ReportService.Log("Registry rename update exception: " + ex.Message);
            }

            if (wmiSuccess || win32Success || regSuccess)
            {
                statusMessage = string.Format("ປ່ຽນຊື່ເຄື່ອງເປັນ {0} ສຳເລັດແລ້ວ! (ກະລຸນາ Restart ເພື່ອໃຫ້ມີຜົນສົມບູນ)", sanitized);
                ReportService.Log(statusMessage);
                return true;
            }
            else
            {
                statusMessage = string.Format("ບໍ່ສາມາດປ່ຽນຊື່ເຄື່ອງໄດ້ (WMI Code: {0}). ກະລຸນາ Run as Administrator.", wmiReturnCode);
                ReportService.Log(statusMessage);
                return false;
            }
        }

        /// <summary>
        /// ສັ່ງ Restart ຄອມພິວເຕີເພື່ອໃຫ້ຊື່ໃໝ່ມີຜົນສົມບູນ
        /// </summary>
        public static void RestartComputer(int delaySeconds = 5)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "shutdown.exe",
                    Arguments = string.Format("/r /t {0} /c \"Classroom Checker: Restarting to apply new computer name...\"", Math.Max(0, delaySeconds)),
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                ReportService.Log("RestartComputer error: " + ex.Message);
            }
        }
    }
}
