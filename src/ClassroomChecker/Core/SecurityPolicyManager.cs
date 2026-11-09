using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using ClassroomChecker.Config;

namespace ClassroomChecker.Core
{
    /// <summary>
    /// ຈັດການນະໂຍບາຍຄວາມປອດໄພຂອງ Windows (Security Policies) ຜ່ານ Registry
    /// - ຫ້າມ Run exe ຈາກ USB Flash Drive ໂດຍຍົກເວັ້ນ (Whitelist) ClassroomChecker.exe
    /// - ຫ້າມ Run exe ຈາກ Downloads ແລະ Temp ແລະ Folders ທີ່ກຳນົດ (Safer Path Rules)
    /// - ບລັອກເກມ/ໂປຣແກຣມທີ່ບໍ່ໄດ້ຮັບອະນຸຍາດ (DisallowRun)
    /// </summary>
    public static class SecurityPolicyManager
    {
        private const string GUID_REMOVABLE_STORAGE = "{53f5630d-b6bf-47e0-c2f2-08050c7f5161}";
        private const string REG_PATH_REMOVABLE = @"SOFTWARE\Policies\Microsoft\Windows\RemovableStorageDevices";
        private const string REG_PATH_EXPLORER_POLICIES = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer";
        private const string REG_PATH_SAFER_BASE = @"SOFTWARE\Policies\Microsoft\Windows\Safer\CodeIdentifiers";
        private const string REG_PATH_SAFER_PATHS = @"SOFTWARE\Policies\Microsoft\Windows\Safer\CodeIdentifiers\0\Paths";
        private const string REG_PATH_SAFER_UNRESTRICTED = @"SOFTWARE\Policies\Microsoft\Windows\Safer\CodeIdentifiers\262144\Paths";

        private const string SAFER_GUID_PATH_PREFIX = "{D1A10001-B10C-4001-A001-";
        private const string SAFER_GUID_DOWNLOADS = "{D1A10001-B10C-4001-A001-000000000001}";
        private const string SAFER_GUID_TEMP = "{D1A10001-B10C-4001-A001-000000000002}";
        private const string SAFER_GUID_LOCAL_TEMP = "{D1A10001-B10C-4001-A001-000000000003}";

        private const string SAFER_GUID_USB_PREFIX = "{D1A10001-B10C-4001-B001-";
        private const string SAFER_GUID_WHITELIST_EXE = "{D1A10001-B10C-4001-W001-000000000001}";
        private const string SAFER_GUID_WHITELIST_APPNAME = "{D1A10001-B10C-4001-W001-000000000002}";

        // -----------------------------------------------------------------
        // 1. USB Execution Lock (with ClassroomChecker Whitelist)
        // -----------------------------------------------------------------

        public static bool IsUsbExecutionBlocked()
        {
            try
            {
                // 1. ກວດ Safer rules ສຳລັບ Drive ທີ່ເປັນ Removable/USB (ລະບົບໃໝ່ທີ່ມີ Whitelist)
                using (var pathsKey = Registry.LocalMachine.OpenSubKey(REG_PATH_SAFER_PATHS, false))
                {
                    if (pathsKey != null)
                    {
                        foreach (var sk in pathsKey.GetSubKeyNames())
                        {
                            if (sk.StartsWith(SAFER_GUID_USB_PREFIX, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }

                // 2. Fallback ກວດ legacy RemovableStorageDevices
                using (var key = Registry.LocalMachine.OpenSubKey(REG_PATH_REMOVABLE + @"\" + GUID_REMOVABLE_STORAGE, false))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("Deny_Execute");
                        if (val is int && (int)val == 1) return true;
                    }
                }

                using (var key = Registry.LocalMachine.OpenSubKey(REG_PATH_REMOVABLE, false))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("Deny_Execute");
                        if (val is int && (int)val == 1) return true;
                    }
                }
            }
            catch { }

            return false;
        }

        public static bool SetUsbExecutionBlocked(bool block)
        {
            try
            {
                // ລ້າງ legacy RemovableStorageDevices ສະເໝີ ເພື່ອບໍ່ໃຫ້ບລັອກ USB ທັງໝົດແບບບໍ່ມີ Whitelist
                ClearLegacyRemovableStorageDeny();

                if (block)
                {
                    EnsureSaferBaseInitialized();

                    // ຊອກຫາ drive ທີ່ເປັນ Fixed (ເຊັ່ນ C:, D: ຖ້າເປັນ HDD/SSD ພາຍໃນເຄື່ອງ)
                    var fixedDrives = new HashSet<char>();
                    try
                    {
                        foreach (var d in DriveInfo.GetDrives())
                        {
                            if (d.DriveType == DriveType.Fixed)
                            {
                                fixedDrives.Add(char.ToUpperInvariant(d.Name[0]));
                            }
                        }
                    }
                    catch { }

                    string sysDrive = Path.GetPathRoot(Environment.SystemDirectory);
                    if (!string.IsNullOrEmpty(sysDrive))
                    {
                        fixedDrives.Add(char.ToUpperInvariant(sysDrive[0]));
                    }

                    // ລຶບ USB rules ເກົ່າອອກກ່ອນ
                    DeleteUsbSaferRules();

                    // ສ້າງ Safer Level 0 (Disallowed) rules ສຳລັບ Drive ທີ່ບໍ່ແມ່ນ Fixed (D ເຖິງ Z)
                    for (char c = 'D'; c <= 'Z'; c++)
                    {
                        if (fixedDrives.Contains(c)) continue; // ບໍ່ບລັອກ drive ພາຍໃນເຄື່ອງ

                        string guid = string.Format("{{D1A10001-B10C-4001-B001-{0:D12}}}", (int)c);
                        CreateSaferPathRule(guid, c + @":\*", "ClassroomChecker: Block Removable Drive " + c);
                    }

                    // ສ້າງ Whitelist ໃນ Level 262144 (Unrestricted) ເພື່ອໃຫ້ ClassroomChecker.exe ເປີດຈາກ USB ໄດ້
                    EnsureClassroomCheckerWhitelisted();
                }
                else
                {
                    DeleteUsbSaferRules();

                    // ຖ້າບໍ່ມີ Safer rule ອື່ນເຫຼືອ (Downloads/Temp ຫຼື Custom) ໃຫ້ລຶບ Whitelist ນຳ
                    if (!IsDownloadsExecutionBlocked() && !IsAnyOtherFolderRuleActive())
                    {
                        DeleteAllClassroomCheckerWhitelistRules();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void ClearLegacyRemovableStorageDeny()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(REG_PATH_REMOVABLE + @"\" + GUID_REMOVABLE_STORAGE, true))
                {
                    if (key != null) key.DeleteValue("Deny_Execute", false);
                }
            }
            catch { }
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(REG_PATH_REMOVABLE, true))
                {
                    if (key != null) key.DeleteValue("Deny_Execute", false);
                }
            }
            catch { }
        }

        // -----------------------------------------------------------------
        // 2. DisallowRun (Blocked Exes)
        // -----------------------------------------------------------------

        public static bool IsDisallowRunActive()
        {
            try
            {
                // ກວດທັງ HKLM ແລະ HKCU
                using (var key = Registry.LocalMachine.OpenSubKey(REG_PATH_EXPLORER_POLICIES, false))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("DisallowRun");
                        if (val is int && (int)val == 1) return true;
                    }
                }

                using (var key = Registry.CurrentUser.OpenSubKey(REG_PATH_EXPLORER_POLICIES, false))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("DisallowRun");
                        if (val is int && (int)val == 1) return true;
                    }
                }
            }
            catch { }

            return false;
        }

        public static int GetDisallowedExesCount()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(REG_PATH_EXPLORER_POLICIES + @"\DisallowRun", false))
                {
                    if (key != null) return key.ValueCount;
                }
                using (var key = Registry.CurrentUser.OpenSubKey(REG_PATH_EXPLORER_POLICIES + @"\DisallowRun", false))
                {
                    if (key != null) return key.ValueCount;
                }
            }
            catch { }
            return 0;
        }

        public static bool SetDisallowedExes(bool active, IEnumerable<string> exes)
        {
            try
            {
                List<string> cleanExes = new List<string>();
                if (exes != null)
                {
                    foreach (var e in exes)
                    {
                        if (!string.IsNullOrEmpty(e) && !cleanExes.Contains(e.Trim(), StringComparer.OrdinalIgnoreCase))
                        {
                            cleanExes.Add(e.Trim());
                        }
                    }
                }

                // 1. ຂຽນໃສ່ HKLM (ຜົນກັບທຸກ User ໃນເຄື່ອງ)
                WriteDisallowRunToRoot(Registry.LocalMachine, active, cleanExes);

                // 2. ຂຽນໃສ່ HKCU (Current User)
                WriteDisallowRunToRoot(Registry.CurrentUser, active, cleanExes);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void WriteDisallowRunToRoot(RegistryKey root, bool active, List<string> exes)
        {
            try
            {
                if (active && exes.Count > 0)
                {
                    using (var polKey = root.CreateSubKey(REG_PATH_EXPLORER_POLICIES))
                    {
                        if (polKey != null) polKey.SetValue("DisallowRun", 1, RegistryValueKind.DWord);
                    }

                    // ລຶບ SubKey ເກົ່າອອກກ່ອນ ແລ້ວສ້າງໃໝ່ ເພື່ອໃຫ້ Index ສະອາດ
                    try { root.DeleteSubKeyTree(REG_PATH_EXPLORER_POLICIES + @"\DisallowRun", false); } catch { }

                    using (var disKey = root.CreateSubKey(REG_PATH_EXPLORER_POLICIES + @"\DisallowRun"))
                    {
                        if (disKey != null)
                        {
                            for (int i = 0; i < exes.Count; i++)
                            {
                                disKey.SetValue((i + 1).ToString(), exes[i], RegistryValueKind.String);
                            }
                        }
                    }
                }
                else
                {
                    using (var polKey = root.OpenSubKey(REG_PATH_EXPLORER_POLICIES, true))
                    {
                        if (polKey != null) polKey.SetValue("DisallowRun", 0, RegistryValueKind.DWord);
                    }
                    try { root.DeleteSubKeyTree(REG_PATH_EXPLORER_POLICIES + @"\DisallowRun", false); } catch { }
                }
            }
            catch { }
        }

        // -----------------------------------------------------------------
        // 3. Safer Path Rules (Block Downloads, Temp & Folders)
        // -----------------------------------------------------------------

        public static bool IsDownloadsExecutionBlocked()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(REG_PATH_SAFER_PATHS + @"\" + SAFER_GUID_DOWNLOADS, false))
                {
                    if (key != null)
                    {
                        object flags = key.GetValue("SaferFlags");
                        if (flags is int && (int)flags == 0) return true;
                    }
                }
            }
            catch { }

            return false;
        }

        public static bool SetDownloadsExecutionBlocked(bool block, IEnumerable<string> additionalPaths = null)
        {
            return SetPathRulesBlocked(block, additionalPaths);
        }

        public static bool SetPathRulesBlocked(bool blockDownloadsAndTemp, IEnumerable<string> additionalPaths)
        {
            try
            {
                bool hasCustom = additionalPaths != null && additionalPaths.Any(p => !string.IsNullOrEmpty(p));
                if (blockDownloadsAndTemp || hasCustom)
                {
                    // 1. ເປີດລະບົບ Safer CodeIdentifiers ພື້ນຖານ
                    EnsureSaferBaseInitialized();

                    // 2. ລຶບສະເພາະ Path Rules ເກົ່າຂອງ ClassroomChecker (Folder / Downloads / Temp)
                    DeletePathFolderSaferRules();

                    // 3. ສ້າງ Path Rules ຫ້າມ Downloads & Temp ຖ້າເລືອກ
                    if (blockDownloadsAndTemp)
                    {
                        CreateSaferPathRule(SAFER_GUID_DOWNLOADS, @"%USERPROFILE%\Downloads\*", "ClassroomChecker: Block Downloads");
                        CreateSaferPathRule(SAFER_GUID_TEMP, @"%TEMP%\*", "ClassroomChecker: Block Temp");
                        CreateSaferPathRule(SAFER_GUID_LOCAL_TEMP, @"%USERPROFILE%\AppData\Local\Temp\*", "ClassroomChecker: Block Local Temp");
                    }

                    // 4. ໂຟນເດີເພີ່ມເຕີມທີ່ຄູເລືອກ (ລັອກທຸກ exe ໃນໂຟນເດີນັ້ນ)
                    if (additionalPaths != null)
                    {
                        int customIdx = 100;
                        foreach (var p in additionalPaths)
                        {
                            if (!string.IsNullOrEmpty(p))
                            {
                                string customGuid = string.Format("{{D1A10001-B10C-4001-A001-{0:D12}}}", customIdx++);
                                string pathPattern = p.Trim();
                                if (!pathPattern.EndsWith(@"\*") && !pathPattern.EndsWith("/*"))
                                {
                                    if (pathPattern.EndsWith(@"\") || pathPattern.EndsWith("/"))
                                    {
                                        pathPattern += "*";
                                    }
                                    else
                                    {
                                        pathPattern += @"\*";
                                    }
                                }
                                CreateSaferPathRule(customGuid, pathPattern, "ClassroomChecker: Blocked Folder " + p);
                            }
                        }
                    }

                    // Whitelist ClassroomChecker ສະເໝີ
                    EnsureClassroomCheckerWhitelisted();
                }
                else
                {
                    DeletePathFolderSaferRules();
                    if (!IsUsbExecutionBlocked())
                    {
                        DeleteAllClassroomCheckerWhitelistRules();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void EnsureSaferBaseInitialized()
        {
            try
            {
                using (var baseKey = Registry.LocalMachine.CreateSubKey(REG_PATH_SAFER_BASE))
                {
                    if (baseKey != null)
                    {
                        baseKey.SetValue("AuthenticodeEnabled", 0, RegistryValueKind.DWord);
                        baseKey.SetValue("DefaultLevel", 262144, RegistryValueKind.DWord); // 0x40000 = Unrestricted default
                        baseKey.SetValue("PolicyScope", 0, RegistryValueKind.DWord);      // 0 = All Users
                        baseKey.SetValue("TransparentEnabled", 1, RegistryValueKind.DWord); // 1 = All software except libraries
                    }
                }
            }
            catch { }
        }

        private static void EnsureClassroomCheckerWhitelisted()
        {
            try
            {
                CreateSaferWhitelistRule(SAFER_GUID_WHITELIST_EXE, @"*\ClassroomChecker.exe", "ClassroomChecker: Whitelisted Admin Tool");

                string currentExeName = Path.GetFileName(AppDomain.CurrentDomain.FriendlyName);
                if (!string.IsNullOrEmpty(currentExeName) && !currentExeName.Equals("ClassroomChecker.exe", StringComparison.OrdinalIgnoreCase))
                {
                    CreateSaferWhitelistRule(SAFER_GUID_WHITELIST_APPNAME, @"*\" + currentExeName, "ClassroomChecker: Whitelisted Host Process");
                }
            }
            catch { }
        }

        private static void DeletePathFolderSaferRules()
        {
            try
            {
                using (var pathsKey = Registry.LocalMachine.OpenSubKey(REG_PATH_SAFER_PATHS, true))
                {
                    if (pathsKey != null)
                    {
                        foreach (var sk in pathsKey.GetSubKeyNames())
                        {
                            if (sk.StartsWith(SAFER_GUID_PATH_PREFIX, StringComparison.OrdinalIgnoreCase))
                            {
                                pathsKey.DeleteSubKeyTree(sk, false);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private static void DeleteUsbSaferRules()
        {
            try
            {
                using (var pathsKey = Registry.LocalMachine.OpenSubKey(REG_PATH_SAFER_PATHS, true))
                {
                    if (pathsKey != null)
                    {
                        foreach (var sk in pathsKey.GetSubKeyNames())
                        {
                            if (sk.StartsWith(SAFER_GUID_USB_PREFIX, StringComparison.OrdinalIgnoreCase))
                            {
                                pathsKey.DeleteSubKeyTree(sk, false);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private static void DeleteAllClassroomCheckerSaferRules()
        {
            try
            {
                using (var pathsKey = Registry.LocalMachine.OpenSubKey(REG_PATH_SAFER_PATHS, true))
                {
                    if (pathsKey != null)
                    {
                        foreach (var sk in pathsKey.GetSubKeyNames())
                        {
                            if (sk.StartsWith("{D1A10001-", StringComparison.OrdinalIgnoreCase))
                            {
                                pathsKey.DeleteSubKeyTree(sk, false);
                            }
                        }
                    }
                }
            }
            catch { }

            DeleteAllClassroomCheckerWhitelistRules();
        }

        private static void DeleteAllClassroomCheckerWhitelistRules()
        {
            try
            {
                using (var pathsKey = Registry.LocalMachine.OpenSubKey(REG_PATH_SAFER_UNRESTRICTED, true))
                {
                    if (pathsKey != null)
                    {
                        foreach (var sk in pathsKey.GetSubKeyNames())
                        {
                            if (sk.StartsWith("{D1A10001-", StringComparison.OrdinalIgnoreCase))
                            {
                                pathsKey.DeleteSubKeyTree(sk, false);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private static bool IsAnyOtherFolderRuleActive()
        {
            try
            {
                using (var pathsKey = Registry.LocalMachine.OpenSubKey(REG_PATH_SAFER_PATHS, false))
                {
                    if (pathsKey != null)
                    {
                        foreach (var sk in pathsKey.GetSubKeyNames())
                        {
                            if (sk.StartsWith(SAFER_GUID_PATH_PREFIX, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        private static void CreateSaferPathRule(string guid, string itemData, string description)
        {
            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(REG_PATH_SAFER_PATHS + @"\" + guid))
                {
                    if (key != null)
                    {
                        key.SetValue("ItemData", itemData, RegistryValueKind.String);
                        key.SetValue("SaferFlags", 0, RegistryValueKind.DWord); // 0 = Disallowed
                        key.SetValue("Description", description, RegistryValueKind.String);
                    }
                }
            }
            catch { }
        }

        private static void CreateSaferWhitelistRule(string guid, string itemData, string description)
        {
            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(REG_PATH_SAFER_UNRESTRICTED + @"\" + guid))
                {
                    if (key != null)
                    {
                        key.SetValue("ItemData", itemData, RegistryValueKind.String);
                        key.SetValue("SaferFlags", 0, RegistryValueKind.DWord); // 0 = Unrestricted rule
                        key.SetValue("Description", description, RegistryValueKind.String);
                    }
                }
            }
            catch { }
        }

        // -----------------------------------------------------------------
        // Batch Operations
        // -----------------------------------------------------------------

        /// <summary>
        /// ນຳໃຊ້ນະໂຍບາຍລັອກທັງໝົດຕາມ SecurityConfig
        /// </summary>
        public static bool ApplyFullLock(SecurityConfig config, out List<string> logDetails)
        {
            logDetails = new List<string>();
            if (config == null)
            {
                config = new SecurityConfig();
            }

            bool allOk = true;

            // 1. USB Lock (with ClassroomChecker Whitelist)
            if (config.blockUsbExecution)
            {
                bool r = SetUsbExecutionBlocked(true);
                allOk &= r;
                logDetails.Add("USB Execution Lock (with ClassroomChecker Whitelist): " + (r ? "ENABLED" : "FAILED"));
            }
            else
            {
                SetUsbExecutionBlocked(false);
                logDetails.Add("USB Execution Lock: DISABLED (by config)");
            }

            // 2. Downloads & Temp Lock + Custom Folders
            if (config.blockDownloadsExecution || (config.blockedPaths != null && config.blockedPaths.Count > 0))
            {
                bool r = SetPathRulesBlocked(config.blockDownloadsExecution, config.blockedPaths);
                allOk &= r;
                int customCount = config.blockedPaths != null ? config.blockedPaths.Count : 0;
                logDetails.Add(string.Format("Path Restriction: {0} (Downloads={1}, CustomFolders={2})",
                    r ? "ENABLED" : "FAILED", config.blockDownloadsExecution, customCount));
            }
            else
            {
                SetPathRulesBlocked(false, null);
                logDetails.Add("Path Restriction: DISABLED (by config)");
            }

            // 3. Disallowed Exes
            List<string> exesToBlock = new List<string>();
            if (config.blockPopularGames && config.blockedExes != null)
            {
                exesToBlock.AddRange(config.blockedExes);
            }
            if (exesToBlock.Count > 0)
            {
                bool r = SetDisallowedExes(true, exesToBlock);
                allOk &= r;
                logDetails.Add(string.Format("DisallowRun App Lock: ENABLED ({0} blocked exes)", exesToBlock.Count));
            }
            else
            {
                SetDisallowedExes(false, null);
                logDetails.Add("DisallowRun App Lock: DISABLED (empty list)");
            }

            return allOk;
        }

        /// <summary>
        /// ປົດລັອກນະໂຍບາຍທັງໝົດ (Unlock All)
        /// </summary>
        public static bool UnlockAll(out List<string> logDetails)
        {
            logDetails = new List<string>();
            bool r1 = SetUsbExecutionBlocked(false);
            bool r2 = SetDownloadsExecutionBlocked(false, null);
            bool r3 = SetDisallowedExes(false, null);

            logDetails.Add("USB Execution Lock & Whitelist: REMOVED");
            logDetails.Add("Downloads/Temp & Folder Safer Rules: REMOVED");
            logDetails.Add("DisallowRun Rules: REMOVED");

            return r1 && r2 && r3;
        }
    }
}
