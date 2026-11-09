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
    /// - ຫ້າມ Run exe ຈາກ USB Flash Drive
    /// - ຫ້າມ Run exe ຈາກ Downloads ແລະ Temp (Safer Path Rules)
    /// - ບລັອກເກມ/ໂປຣແກຣມທີ່ບໍ່ໄດ້ຮັບອະນຸຍາດ (DisallowRun)
    /// </summary>
    public static class SecurityPolicyManager
    {
        private const string GUID_REMOVABLE_STORAGE = "{53f5630d-b6bf-47e0-c2f2-08050c7f5161}";
        private const string REG_PATH_REMOVABLE = @"SOFTWARE\Policies\Microsoft\Windows\RemovableStorageDevices";
        private const string REG_PATH_EXPLORER_POLICIES = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer";
        private const string REG_PATH_SAFER_BASE = @"SOFTWARE\Policies\Microsoft\Windows\Safer\CodeIdentifiers";
        private const string REG_PATH_SAFER_PATHS = @"SOFTWARE\Policies\Microsoft\Windows\Safer\CodeIdentifiers\0\Paths";

        private const string SAFER_GUID_DOWNLOADS = "{D1A10001-B10C-4001-A001-000000000001}";
        private const string SAFER_GUID_TEMP = "{D1A10001-B10C-4001-A001-000000000002}";
        private const string SAFER_GUID_LOCAL_TEMP = "{D1A10001-B10C-4001-A001-000000000003}";

        // -----------------------------------------------------------------
        // 1. USB Execution Lock
        // -----------------------------------------------------------------

        public static bool IsUsbExecutionBlocked()
        {
            try
            {
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
                if (block)
                {
                    using (var key = Registry.LocalMachine.CreateSubKey(REG_PATH_REMOVABLE + @"\" + GUID_REMOVABLE_STORAGE))
                    {
                        if (key != null) key.SetValue("Deny_Execute", 1, RegistryValueKind.DWord);
                    }
                    using (var key = Registry.LocalMachine.CreateSubKey(REG_PATH_REMOVABLE))
                    {
                        if (key != null) key.SetValue("Deny_Execute", 1, RegistryValueKind.DWord);
                    }
                }
                else
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(REG_PATH_REMOVABLE + @"\" + GUID_REMOVABLE_STORAGE, true))
                    {
                        if (key != null) key.DeleteValue("Deny_Execute", false);
                    }
                    using (var key = Registry.LocalMachine.OpenSubKey(REG_PATH_REMOVABLE, true))
                    {
                        if (key != null) key.DeleteValue("Deny_Execute", false);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
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
        // 3. Safer Path Rules (Block Downloads & Temp execution)
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
            try
            {
                if (block)
                {
                    // 1. ເປີດລະບົບ Safer CodeIdentifiers ພື້ນຖານ
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

                    // 2. ສ້າງ Path Rules ຫ້າມ Downloads & Temp
                    CreateSaferPathRule(SAFER_GUID_DOWNLOADS, @"%USERPROFILE%\Downloads\*", "ClassroomChecker: Block Downloads");
                    CreateSaferPathRule(SAFER_GUID_TEMP, @"%TEMP%\*", "ClassroomChecker: Block Temp");
                    CreateSaferPathRule(SAFER_GUID_LOCAL_TEMP, @"%USERPROFILE%\AppData\Local\Temp\*", "ClassroomChecker: Block Local Temp");

                    // 3. ໂຟນເດີເພີ່ມເຕີມທີ່ຄູລະບຸ
                    if (additionalPaths != null)
                    {
                        int customIdx = 100;
                        foreach (var p in additionalPaths)
                        {
                            if (!string.IsNullOrEmpty(p))
                            {
                                string customGuid = string.Format("{{D1A10001-B10C-4001-A001-{0:D12}}}", customIdx++);
                                string pathPattern = p.EndsWith(@"\") || p.EndsWith(@"/") ? p + "*" : p + @"\*";
                                CreateSaferPathRule(customGuid, pathPattern, "ClassroomChecker: Custom Block Path");
                            }
                        }
                    }
                }
                else
                {
                    // ລຶບ Rules ຂອງ ClassroomChecker ອອກ
                    DeleteSaferPathRule(SAFER_GUID_DOWNLOADS);
                    DeleteSaferPathRule(SAFER_GUID_TEMP);
                    DeleteSaferPathRule(SAFER_GUID_LOCAL_TEMP);

                    for (int i = 100; i < 200; i++)
                    {
                        string customGuid = string.Format("{{D1A10001-B10C-4001-A001-{0:D12}}}", i);
                        DeleteSaferPathRule(customGuid);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
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

        private static void DeleteSaferPathRule(string guid)
        {
            try
            {
                using (var pathsKey = Registry.LocalMachine.OpenSubKey(REG_PATH_SAFER_PATHS, true))
                {
                    if (pathsKey != null)
                    {
                        pathsKey.DeleteSubKeyTree(guid, false);
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

            // 1. USB Lock
            if (config.blockUsbExecution)
            {
                bool r = SetUsbExecutionBlocked(true);
                allOk &= r;
                logDetails.Add("USB Deny Execute: " + (r ? "ENABLED" : "FAILED"));
            }
            else
            {
                SetUsbExecutionBlocked(false);
                logDetails.Add("USB Deny Execute: DISABLED (by config)");
            }

            // 2. Downloads & Temp Lock
            if (config.blockDownloadsExecution)
            {
                bool r = SetDownloadsExecutionBlocked(true, config.blockedPaths);
                allOk &= r;
                logDetails.Add("Downloads & Temp Path Restriction: " + (r ? "ENABLED" : "FAILED"));
            }
            else
            {
                SetDownloadsExecutionBlocked(false, null);
                logDetails.Add("Downloads & Temp Path Restriction: DISABLED (by config)");
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

            logDetails.Add("USB Deny Execute: REMOVED");
            logDetails.Add("Downloads/Temp Safer Rules: REMOVED");
            logDetails.Add("DisallowRun Rules: REMOVED");

            return r1 && r2 && r3;
        }
    }
}
