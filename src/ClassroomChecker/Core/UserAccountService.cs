using System;
using System.DirectoryServices;
using System.Management;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace ClassroomChecker.Core
{
    /// <summary>
    /// ປ່ຽນຊື່ບັນຊີ user ທີ່ກຳລັງ login ໃຫ້ກົງກັບຊື່ຄອມ (ທັງຊື່ login ແລະ ຊື່ທີ່ສະແດງໃນໜ້າ Welcome).
    /// ຮອງຮັບສະເພາະບັນຊີ Local ຂອງເຄື່ອງ (ບໍ່ແຕະບັນຊີ Domain).
    /// ໂຟນເດີ C:\Users\ຊື່ເກົ່າ ຍັງຄືເດີມ — Windows ບໍ່ປ່ຽນຊື່ໂຟນເດີ profile.
    /// </summary>
    public static class UserAccountService
    {
        private const string WinlogonKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";

        /// <summary>
        /// ຊື່ບັນຊີທີ່ login ຢູ່ໜ້າຈໍເຄື່ອງ (ບໍ່ແມ່ນບັນຊີ Admin ທີ່ໃຊ້ຍົກສິດ UAC).
        /// ຄືນ null ຖ້າບໍ່ແມ່ນບັນຊີ Local ຂອງເຄື່ອງນີ້.
        /// </summary>
        public static string GetInteractiveLocalUserName()
        {
            string full = null;
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        full = mo["UserName"] as string;
                        break;
                    }
                }
            }
            catch { }

            if (string.IsNullOrEmpty(full))
            {
                full = Environment.UserDomainName + "\\" + Environment.UserName;
            }

            string domain, user;
            SplitAccount(full, out domain, out user);
            if (string.IsNullOrEmpty(user)) return null;

            return string.Equals(domain, Environment.MachineName, StringComparison.OrdinalIgnoreCase) ? user : null;
        }

        /// <summary>SID ຂອງ user ທີ່ login ຢູ່ໜ້າຈໍ (ບັນຊີ Local), ຫຼື null</summary>
        public static string GetInteractiveUserSid()
        {
            try
            {
                string user = GetInteractiveLocalUserName();
                if (string.IsNullOrEmpty(user)) return null;
                var account = new System.Security.Principal.NTAccount(Environment.MachineName, user);
                return ((System.Security.Principal.SecurityIdentifier)account.Translate(
                    typeof(System.Security.Principal.SecurityIdentifier))).Value;
            }
            catch
            {
                return null;
            }
        }

        internal static void SplitAccount(string full, out string domain, out string user)
        {
            domain = string.Empty;
            user = full ?? string.Empty;
            int idx = user.IndexOf('\\');
            if (idx >= 0)
            {
                domain = user.Substring(0, idx);
                user = user.Substring(idx + 1);
            }
        }

        /// <summary>
        /// ສ້າງຊື່ user ຈາກຄຳນຳໜ້າ + ຊື່ຄອມ: ("LTVC", "COM1-01") → "LTVC-COM1-01".
        /// ຄຳນຳໜ້າປະວ່າງ ຈະໄດ້ຊື່ດຽວກັບຄອມ.
        /// </summary>
        public static string BuildUserName(string prefix, string computerName)
        {
            string p = (prefix ?? string.Empty).Trim().TrimEnd('-');
            string c = (computerName ?? string.Empty).Trim();
            return p.Length == 0 ? c : p + "-" + c;
        }

        /// <summary>ກົດຂອງ Windows: ສູງສຸດ 20 ຕົວ, ຫ້າມ " / \ [ ] : ; | = , + * ? &lt; &gt; @ ແລະ ຫ້າມເປັນຈຸດ/ຍະຫວ່າງລ້ວນ</summary>
        public static bool ValidateUserName(string name, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                error = "ຊື່ user ບໍ່ສາມາດຫວ່າງເປົ່າໄດ້";
                return false;
            }
            if (name.Length > 20)
            {
                error = "ຊື່ user ຍາວໄດ້ສູງສຸດ 20 ຕົວອັກສອນ";
                return false;
            }
            if (Regex.IsMatch(name, "[\"/\\\\\\[\\]:;|=,+*?<>@]"))
            {
                error = "ຊື່ user ຫ້າມມີຕົວອັກສອນ \" / \\ [ ] : ; | = , + * ? < > @";
                return false;
            }
            if (Regex.IsMatch(name, @"^[\s.]+$"))
            {
                error = "ຊື່ user ຫ້າມເປັນຈຸດ ຫຼື ຍະຫວ່າງລ້ວນ";
                return false;
            }
            return true;
        }

        /// <summary>
        /// ປ່ຽນຊື່ບັນຊີທີ່ login ຢູ່ເປັນ newName (ທັງຊື່ login ແລະ Full Name).
        /// ຖ້າເປີດ Auto-login ດ້ວຍຊື່ເກົ່າ ຈະອັບເດດ DefaultUserName ໃຫ້ນຳ. ມີຜົນສົມບູນຫຼັງ Sign out.
        /// </summary>
        public static bool RenameInteractiveUser(string newName, out string message)
        {
            newName = (newName ?? string.Empty).Trim();

            string error;
            if (!ValidateUserName(newName, out error))
            {
                message = error;
                return false;
            }

            string oldName = GetInteractiveLocalUserName();
            if (oldName == null)
            {
                message = "ຂ້າມການປ່ຽນຊື່ user: ບັນຊີທີ່ login ຢູ່ບໍ່ແມ່ນບັນຊີ Local ຂອງເຄື່ອງ (ເຊັ່ນ Domain)";
                return false;
            }

            try
            {
                using (var machine = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
                using (var user = machine.Children.Find(oldName, "user"))
                {
                    bool sameLogin = string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase);
                    if (!sameLogin)
                    {
                        if (UserExists(machine, newName))
                        {
                            message = string.Format("ມີບັນຊີຊື່ '{0}' ຢູ່ໃນເຄື່ອງແລ້ວ, ບໍ່ສາມາດປ່ຽນຊື່ '{1}' ໄດ້", newName, oldName);
                            return false;
                        }
                        user.Rename(newName);
                    }

                    user.Properties["FullName"].Value = newName;
                    user.CommitChanges();

                    UpdateAutoLogonName(oldName, newName);

                    ReportService.Log(string.Format("Renamed local user '{0}' -> '{1}'", oldName, newName));
                    message = sameLogin
                        ? string.Format("ຕັ້ງຊື່ທີ່ສະແດງຂອງ user '{0}' ແລ້ວ", newName)
                        : string.Format("ປ່ຽນຊື່ user '{0}' ເປັນ '{1}' ແລ້ວ (Sign out ເພື່ອໃຫ້ມີຜົນສົມບູນ)", oldName, newName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                ReportService.Log("Rename user failed: " + ex);
                message = "ປ່ຽນຊື່ user ບໍ່ສຳເລັດ: " + ex.Message;
                return false;
            }
        }

        /// <summary>true ຖ້າ user ທີ່ login ຢູ່ ມີຊື່ login ແລະ Full Name ກົງກັບ name ແລ້ວ (ຫຼື ບໍ່ແມ່ນບັນຊີ Local)</summary>
        public static bool IsInteractiveUserNamed(string name)
        {
            string current = GetInteractiveLocalUserName();
            if (current == null) return true;
            if (!string.Equals(current, name, StringComparison.OrdinalIgnoreCase)) return false;

            try
            {
                using (var user = new DirectoryEntry("WinNT://" + Environment.MachineName + "/" + current + ",user"))
                {
                    string fullName = user.Properties["FullName"].Value as string;
                    return string.Equals(fullName, name, StringComparison.Ordinal);
                }
            }
            catch
            {
                return true;
            }
        }

        private static bool UserExists(DirectoryEntry machine, string name)
        {
            try
            {
                using (machine.Children.Find(name, "user")) { return true; }
            }
            catch
            {
                return false;
            }
        }

        private static void UpdateAutoLogonName(string oldName, string newName)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(WinlogonKey, true))
                {
                    if (key == null) return;
                    string current = key.GetValue("DefaultUserName") as string;
                    if (string.Equals(current, oldName, StringComparison.OrdinalIgnoreCase))
                    {
                        key.SetValue("DefaultUserName", newName, RegistryValueKind.String);
                    }
                }
            }
            catch (Exception ex)
            {
                ReportService.Log("Update DefaultUserName failed: " + ex.Message);
            }
        }
    }
}
