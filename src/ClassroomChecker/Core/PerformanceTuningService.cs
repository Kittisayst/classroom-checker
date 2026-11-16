using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Web.Script.Serialization;
using Microsoft.Win32;

namespace ClassroomChecker.Core
{
    public enum SystemDiskType
    {
        Unknown,
        Hdd,
        Ssd
    }

    /// <summary>ສະຖານະຂອງແຕ່ລະລາຍການປັບແຕ່ງ</summary>
    public class TweakStatus
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Applied { get; set; }
        public string Detail { get; set; }
    }

    /// <summary>
    /// ໂໝດເຄື່ອງສະເປັກຕ່ຳ: ປິດສ່ວນທີ່ບໍ່ຈຳເປັນຂອງ Windows ໃຫ້ເຄື່ອງ RAM ນ້ອຍໄວຂຶ້ນ.
    /// ທຸກການປ່ຽນແປງຈະບັນທຶກຄ່າເດີມໄວ້ໃນ performance_backup.json ເພື່ອ "ຄືນຄ່າເດີມ" ໄດ້.
    /// ບໍ່ແຕະ Windows Update, Defender, Pagefile ຫຼື Memory compression.
    /// </summary>
    public static class PerformanceTuningService
    {
        // ------------------------------------------------------------------ ລາຍການ

        public const string TWEAK_VISUAL = "visual_effects";
        public const string TWEAK_BACKGROUND_APPS = "background_apps";
        public const string TWEAK_WIDGETS = "widgets_news";
        public const string TWEAK_DELIVERY = "delivery_optimization";
        public const string TWEAK_SERVICES = "services";
        public const string TWEAK_STARTUP = "startup_apps";

        /// <summary>Service ທີ່ບໍ່ຈຳເປັນໃນຫ້ອງຮຽນ (ອີງຕາມຄູ່ມື Microsoft VDI). SysMain ເພີ່ມສະເພາະເຄື່ອງ HDD</summary>
        internal static readonly string[] ServicesToDisable =
        {
            "DiagTrack",       // Connected User Experiences and Telemetry
            "WSearch",         // Windows Search (indexing) — ຄົ້ນຫາຊື່ໂປຣແກຣມໃນ Start ຍັງໃຊ້ໄດ້
            "MapsBroker",      // Downloaded Maps Manager
            "lfsvc",           // Geolocation
            "XblAuthManager",  // Xbox Live Auth
            "XblGameSave",     // Xbox Live Game Save
            "XboxNetApiSvc",   // Xbox Live Networking
            "XboxGipSvc",      // Xbox Accessory Management
            "WerSvc",          // Windows Error Reporting
            "icssvc",          // Mobile Hotspot
            "Fax",
            "RetailDemo",
            "WMPNetworkSvc"    // Windows Media Player Network Sharing
        };

        internal const string SysMainService = "SysMain";

        /// <summary>ໂປຣແກຣມທີ່ເປີດພ້ອມ Windows ທີ່ບໍ່ຈຳເປັນໃນຫ້ອງຮຽນ (ກວດຈາກຊື່ ຫຼື ຄຳສັ່ງ)</summary>
        internal static readonly string[] StartupPatterns =
        {
            "onedrive", "microsoftedgeautolaunch", "teams", "spotify", "discord", "steam",
            "epicgameslauncher", "skype", "zoom", "riotclient", "garena", "utorrent", "bittorrent"
        };

        // ------------------------------------------------------------------ ຂໍ້ມູນເຄື່ອງ

        public static double GetTotalRamGb()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        return Convert.ToDouble(mo["TotalPhysicalMemory"]) / (1024.0 * 1024 * 1024);
                    }
                }
            }
            catch { }
            return 0;
        }

        /// <summary>ຊະນິດ disk ທີ່ຕິດຕັ້ງ Windows (C:) ຈາກ Storage WMI (MediaType 3 = HDD, 4 = SSD)</summary>
        public static SystemDiskType GetSystemDiskType()
        {
            try
            {
                char drive = char.ToUpperInvariant(Path.GetPathRoot(Environment.SystemDirectory)[0]);
                var scope = new ManagementScope(@"\\.\root\Microsoft\Windows\Storage");

                uint diskNumber = uint.MaxValue;
                using (var searcher = new ManagementObjectSearcher(scope,
                    new ObjectQuery("SELECT DiskNumber FROM MSFT_Partition WHERE DriveLetter = '" + drive + "'")))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        diskNumber = Convert.ToUInt32(mo["DiskNumber"]);
                        break;
                    }
                }
                if (diskNumber == uint.MaxValue) return SystemDiskType.Unknown;

                using (var searcher = new ManagementObjectSearcher(scope,
                    new ObjectQuery("SELECT MediaType, BusType FROM MSFT_PhysicalDisk WHERE DeviceId = '" + diskNumber + "'")))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        int media = Convert.ToInt32(mo["MediaType"]);
                        int bus = Convert.ToInt32(mo["BusType"]);
                        if (media == 4 || bus == 17) return SystemDiskType.Ssd; // 17 = NVMe
                        if (media == 3) return SystemDiskType.Hdd;
                    }
                }
            }
            catch { }
            return SystemDiskType.Unknown;
        }

        /// <summary>ເປີເຊັນພື້ນທີ່ວ່າງຂອງ disk Windows</summary>
        public static double GetSystemDriveFreePercent()
        {
            try
            {
                var d = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory));
                return d.TotalSize > 0 ? 100.0 * d.AvailableFreeSpace / d.TotalSize : 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>mode: "auto" = ສະເພາະເຄື່ອງ RAM ≤ maxRamGb, "on" = ທຸກເຄື່ອງ, "off" = ບໍ່ໃຊ້</summary>
        public static bool IsApplicable(string mode, int maxRamGb, double ramGb)
        {
            string m = (mode ?? "auto").Trim().ToLowerInvariant();
            if (m == "on") return true;
            if (m == "off") return false;
            // RAM 4GB ທີ່ Windows ລາຍງານອາດຢູ່ປະມານ 3.8-4.0 GB
            return ramGb > 0 && ramGb <= maxRamGb + 0.5;
        }

        // ------------------------------------------------------------------ ກວດສະຖານະ

        public static List<TweakStatus> GetStatus(SystemDiskType diskType)
        {
            var list = new List<TweakStatus>();
            RegistryKey userRoot = OpenTargetUserRoot();
            try
            {
                list.Add(new TweakStatus
                {
                    Id = TWEAK_VISUAL,
                    Name = "ປິດ Visual effects ແລະ Transparency",
                    Applied = userRoot != null
                        && ReadInt(userRoot, @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting") == 3
                        && ReadInt(userRoot, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency") == 0
                        && ReadString(userRoot, @"Control Panel\Desktop\WindowMetrics", "MinAnimate") == "0"
                });
            }
            finally
            {
                if (userRoot != null) userRoot.Dispose();
            }

            list.Add(new TweakStatus
            {
                Id = TWEAK_BACKGROUND_APPS,
                Name = "ປິດແອັບທີ່ແລ່ນໃນພື້ນຫຼັງ",
                Applied = ReadInt(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsRunInBackground") == 2
            });

            list.Add(new TweakStatus
            {
                Id = TWEAK_WIDGETS,
                Name = "ປິດ Widgets, ຂ່າວ, Copilot ແລະ Edge ພື້ນຫຼັງ",
                Applied = ReadInt(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests") == 0
                    && ReadInt(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Feeds", "EnableFeeds") == 0
                    && ReadInt(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "StartupBoostEnabled") == 0
            });

            list.Add(new TweakStatus
            {
                Id = TWEAK_DELIVERY,
                Name = "ບໍ່ແບ່ງ Windows Update ໃຫ້ເຄື່ອງອື່ນ (Delivery Optimization)",
                Applied = ReadInt(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode") == 99
            });

            var notDisabled = new List<string>();
            foreach (string svc in GetServicesToDisable(diskType))
            {
                int start = ReadInt(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\" + svc, "Start");
                if (start != -1 && start != 4) notDisabled.Add(svc);
            }
            list.Add(new TweakStatus
            {
                Id = TWEAK_SERVICES,
                Name = "ປິດ Service ທີ່ບໍ່ຈຳເປັນ" + (diskType == SystemDiskType.Hdd ? " (ລວມ SysMain ເພາະເປັນ HDD)" : ""),
                Applied = notDisabled.Count == 0,
                Detail = notDisabled.Count == 0 ? "" : "ຍັງເປີດຢູ່: " + string.Join(", ", notDisabled.ToArray())
            });

            var startupOn = FindUnwantedStartupEntries().Where(e => e.Enabled).Select(e => e.Name).ToList();
            list.Add(new TweakStatus
            {
                Id = TWEAK_STARTUP,
                Name = "ປິດໂປຣແກຣມທີ່ບໍ່ຈຳເປັນທີ່ເປີດພ້ອມ Windows",
                Applied = startupOn.Count == 0,
                Detail = startupOn.Count == 0 ? "" : "ຍັງເປີດຢູ່: " + string.Join(", ", startupOn.ToArray())
            });

            return list;
        }

        internal static List<string> GetServicesToDisable(SystemDiskType diskType)
        {
            var list = new List<string>(ServicesToDisable);
            if (diskType == SystemDiskType.Hdd) list.Add(SysMainService);
            return list;
        }

        // ------------------------------------------------------------------ ນຳໃຊ້

        /// <summary>ນຳໃຊ້ທຸກລາຍການ. ຄືນ log ຂອງແຕ່ລະລາຍການ. ບໍ່ throw</summary>
        public static bool ApplyAll(SystemDiskType diskType, List<string> logs)
        {
            var backup = LoadBackup();
            bool ok = true;

            ok &= Step(logs, "Visual effects", () => ApplyVisualEffects(backup));
            ok &= Step(logs, "Background apps", () =>
                SetDword(backup, Registry.LocalMachine, "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsRunInBackground", 2));
            ok &= Step(logs, "Widgets / News / Copilot / Edge", () => ApplyWidgetPolicies(backup));
            ok &= Step(logs, "Delivery Optimization", () =>
                SetDword(backup, Registry.LocalMachine, "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode", 99));
            ok &= Step(logs, "Services", () => DisableServices(backup, GetServicesToDisable(diskType), logs));
            ok &= Step(logs, "Startup apps", () => DisableStartupEntries(backup, logs));

            SaveBackup(backup);
            logs.Add("ບາງລາຍການ (Visual effects) ມີຜົນສົມບູນຫຼັງ Sign out / Restart");
            return ok;
        }

        private static bool Step(List<string> logs, string name, Action action)
        {
            try
            {
                action();
                logs.Add("✓ " + name);
                return true;
            }
            catch (Exception ex)
            {
                logs.Add("✗ " + name + ": " + ex.Message);
                return false;
            }
        }

        private static void ApplyVisualEffects(PerfBackup backup)
        {
            string rootName;
            RegistryKey root = OpenTargetUserRoot(out rootName);
            if (root == null) throw new InvalidOperationException("ບໍ່ພົບ registry ຂອງ user ທີ່ login");
            try
            {
                // 3 = Custom: ປິດ animation/ເງົາ/ໂປ່ງໃສ ແຕ່ຮັກສາ font smoothing ໃຫ້ຕົວໜັງສືລາວບໍ່ແຕກ
                SetDword(backup, root, rootName, @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 3);
                SetBinary(backup, root, rootName, @"Control Panel\Desktop", "UserPreferencesMask",
                    new byte[] { 0x90, 0x12, 0x03, 0x80, 0x10, 0x00, 0x00, 0x00 });
                SetString(backup, root, rootName, @"Control Panel\Desktop", "FontSmoothing", "2");
                SetString(backup, root, rootName, @"Control Panel\Desktop", "DragFullWindows", "0");
                SetString(backup, root, rootName, @"Control Panel\Desktop\WindowMetrics", "MinAnimate", "0");
                SetDword(backup, root, rootName, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAnimations", 0);
                SetDword(backup, root, rootName, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewAlphaSelect", 0);
                SetDword(backup, root, rootName, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewShadow", 0);
                SetDword(backup, root, rootName, @"Software\Microsoft\Windows\DWM", "EnableAeroPeek", 0);
                SetDword(backup, root, rootName, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 0);
            }
            finally
            {
                root.Dispose();
            }
        }

        private static void ApplyWidgetPolicies(PerfBackup backup)
        {
            var hklm = Registry.LocalMachine;
            SetDword(backup, hklm, "HKLM", @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0);              // Widgets (Win 11)
            SetDword(backup, hklm, "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\Windows Feeds", "EnableFeeds", 0);      // News and interests (Win 10)
            SetDword(backup, hklm, "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1);
            SetDword(backup, hklm, "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 1);
            SetDword(backup, hklm, "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableSoftLanding", 1);
            SetDword(backup, hklm, "HKLM", @"SOFTWARE\Policies\Microsoft\Edge", "StartupBoostEnabled", 0);
            SetDword(backup, hklm, "HKLM", @"SOFTWARE\Policies\Microsoft\Edge", "BackgroundModeEnabled", 0);
        }

        private static void DisableServices(PerfBackup backup, IEnumerable<string> services, List<string> logs)
        {
            foreach (string svc in services)
            {
                string path = @"SYSTEM\CurrentControlSet\Services\" + svc;
                int start = ReadInt(Registry.LocalMachine, path, "Start");
                if (start == -1) continue; // ບໍ່ມີ service ນີ້ໃນເຄື່ອງ

                if (!backup.services.ContainsKey(svc))
                {
                    backup.services[svc] = new ServiceBackup
                    {
                        start = start,
                        delayedAutostart = ReadInt(Registry.LocalMachine, path, "DelayedAutostart")
                    };
                }

                using (var key = Registry.LocalMachine.OpenSubKey(path, true))
                {
                    if (key != null) key.SetValue("Start", 4, RegistryValueKind.DWord); // 4 = Disabled
                }

                try
                {
                    using (var sc = new ServiceController(svc))
                    {
                        if (sc.Status == ServiceControllerStatus.Running && sc.CanStop)
                        {
                            sc.Stop();
                            sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logs.Add(string.Format("  ⚠ {0}: ປິດໄວ້ແລ້ວ ແຕ່ຢຸດທັນທີບໍ່ໄດ້ ({1}) — ມີຜົນຫຼັງ Restart", svc, ex.Message));
                }
            }
        }

        // ------------------------------------------------------------------ startup apps

        public class StartupEntry
        {
            public string Name;
            public string Command;
            public bool Enabled;
            public string RootName;      // "HKLM" ຫຼື "HKU\<sid>"
            public string ApprovedPath;  // ...\StartupApproved\Run ຫຼື Run32
        }

        /// <summary>ໂປຣແກຣມໃນ Run keys (ເຄື່ອງ ແລະ user ທີ່ login) ທີ່ກົງກັບລາຍຊື່ບໍ່ຈຳເປັນ</summary>
        public static List<StartupEntry> FindUnwantedStartupEntries()
        {
            var result = new List<StartupEntry>();
            const string approvedBase = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\";

            CollectStartup(result, Registry.LocalMachine, "HKLM", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", approvedBase + "Run");
            CollectStartup(result, Registry.LocalMachine, "HKLM", @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", approvedBase + "Run32");

            string rootName;
            RegistryKey userRoot = OpenTargetUserRoot(out rootName);
            if (userRoot != null)
            {
                using (userRoot)
                {
                    CollectStartup(result, userRoot, rootName, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", approvedBase + "Run");
                }
            }
            return result;
        }

        private static void CollectStartup(List<StartupEntry> result, RegistryKey root, string rootName, string runPath, string approvedPath)
        {
            try
            {
                using (var run = root.OpenSubKey(runPath, false))
                {
                    if (run == null) return;
                    foreach (string name in run.GetValueNames())
                    {
                        string cmd = (run.GetValue(name) ?? string.Empty).ToString();
                        if (!IsUnwantedStartup(name, cmd)) continue;

                        byte[] approved = null;
                        using (var ap = root.OpenSubKey(approvedPath, false))
                        {
                            if (ap != null) approved = ap.GetValue(name) as byte[];
                        }
                        result.Add(new StartupEntry
                        {
                            Name = name,
                            Command = cmd,
                            Enabled = IsStartupApprovedEnabled(approved),
                            RootName = rootName,
                            ApprovedPath = approvedPath
                        });
                    }
                }
            }
            catch { }
        }

        internal static bool IsUnwantedStartup(string name, string command)
        {
            string text = ((name ?? "") + " " + (command ?? "")).ToLowerInvariant();
            foreach (string p in StartupPatterns)
            {
                if (text.Contains(p)) return true;
            }
            return false;
        }

        /// <summary>StartupApproved: byte ທຳອິດເປັນເລກຄູ່ (02/06) = ເປີດ, ເລກຄີກ (03/07) = ປິດ; ບໍ່ມີຄ່າ = ເປີດ</summary>
        internal static bool IsStartupApprovedEnabled(byte[] value)
        {
            return value == null || value.Length == 0 || (value[0] & 0x01) == 0;
        }

        private static void DisableStartupEntries(PerfBackup backup, List<string> logs)
        {
            var disabled = new byte[] { 0x03, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach (var entry in FindUnwantedStartupEntries())
            {
                if (!entry.Enabled) continue;
                RegistryKey root = entry.RootName == "HKLM" ? Registry.LocalMachine : OpenRootByName(entry.RootName);
                if (root == null) continue;
                try
                {
                    SetBinary(backup, root, entry.RootName, entry.ApprovedPath, entry.Name, disabled);
                    logs.Add("  ປິດເປີດພ້ອມ Windows: " + entry.Name);
                }
                finally
                {
                    if (root != Registry.LocalMachine) root.Dispose();
                }
            }
        }

        // ------------------------------------------------------------------ ຄືນຄ່າເດີມ

        public static bool HasBackup()
        {
            var b = LoadBackup();
            return b.registry.Count > 0 || b.services.Count > 0;
        }

        /// <summary>ຄືນທຸກຄ່າທີ່ໂໝດນີ້ເຄີຍປ່ຽນ ຕາມໄຟລ໌ backup ແລ້ວລຶບໄຟລ໌ backup</summary>
        public static bool RestoreAll(List<string> logs)
        {
            var backup = LoadBackup();
            bool ok = true;

            foreach (var kv in backup.registry)
            {
                var e = kv.Value;
                try
                {
                    RegistryKey root = e.root == "HKLM" ? Registry.LocalMachine : OpenRootByName(e.root);
                    if (root == null)
                    {
                        logs.Add("⚠ ຂ້າມ (user ບໍ່ໄດ້ login): " + e.path + "\\" + e.name);
                        ok = false;
                        continue;
                    }
                    try
                    {
                        if (e.existed)
                        {
                            using (var key = root.CreateSubKey(e.path))
                            {
                                key.SetValue(e.name, DecodeValue(e), ParseKind(e.kind));
                            }
                        }
                        else
                        {
                            using (var key = root.OpenSubKey(e.path, true))
                            {
                                if (key != null) key.DeleteValue(e.name, false);
                            }
                        }
                    }
                    finally
                    {
                        if (root != Registry.LocalMachine) root.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    logs.Add("✗ " + e.path + "\\" + e.name + ": " + ex.Message);
                    ok = false;
                }
            }

            foreach (var kv in backup.services)
            {
                try
                {
                    string path = @"SYSTEM\CurrentControlSet\Services\" + kv.Key;
                    using (var key = Registry.LocalMachine.OpenSubKey(path, true))
                    {
                        if (key == null) continue;
                        key.SetValue("Start", kv.Value.start, RegistryValueKind.DWord);
                        if (kv.Value.delayedAutostart != -1)
                            key.SetValue("DelayedAutostart", kv.Value.delayedAutostart, RegistryValueKind.DWord);
                    }
                    logs.Add("✓ ຄືນ Service: " + kv.Key);
                }
                catch (Exception ex)
                {
                    logs.Add("✗ Service " + kv.Key + ": " + ex.Message);
                    ok = false;
                }
            }

            if (ok)
            {
                try { File.Delete(BackupFilePath); } catch { }
            }
            logs.Add("ຄືນຄ່າເດີມແລ້ວ — Restart ເຄື່ອງເພື່ອໃຫ້ Service ກັບມາເຮັດວຽກ");
            return ok;
        }

        // ------------------------------------------------------------------ registry helpers + backup

        internal class RegBackup
        {
            public string root { get; set; }
            public string path { get; set; }
            public string name { get; set; }
            public bool existed { get; set; }
            public string kind { get; set; }
            public string value { get; set; }
        }

        internal class ServiceBackup
        {
            public int start { get; set; }
            public int delayedAutostart { get; set; }
        }

        internal class PerfBackup
        {
            public Dictionary<string, RegBackup> registry { get; set; }
            public Dictionary<string, ServiceBackup> services { get; set; }

            public PerfBackup()
            {
                registry = new Dictionary<string, RegBackup>();
                services = new Dictionary<string, ServiceBackup>();
            }
        }

        internal static string BackupFilePath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "ClassroomChecker", "performance_backup.json");
            }
        }

        private static PerfBackup LoadBackup()
        {
            try
            {
                if (File.Exists(BackupFilePath))
                {
                    var b = new JavaScriptSerializer().Deserialize<PerfBackup>(File.ReadAllText(BackupFilePath));
                    if (b != null)
                    {
                        if (b.registry == null) b.registry = new Dictionary<string, RegBackup>();
                        if (b.services == null) b.services = new Dictionary<string, ServiceBackup>();
                        return b;
                    }
                }
            }
            catch { }
            return new PerfBackup();
        }

        private static void SaveBackup(PerfBackup backup)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(BackupFilePath));
            File.WriteAllText(BackupFilePath, new JavaScriptSerializer().Serialize(backup));
        }

        /// <summary>ບັນທຶກຄ່າເດີມ "ຄັ້ງທຳອິດ" ເທົ່ານັ້ນ (ການປັບຊ້ຳບໍ່ທັບຄ່າເດີມແທ້)</summary>
        private static void Remember(PerfBackup backup, RegistryKey root, string rootName, string path, string name)
        {
            string id = rootName + "|" + path + "|" + name;
            if (backup.registry.ContainsKey(id)) return;

            var entry = new RegBackup { root = rootName, path = path, name = name, existed = false };
            using (var key = root.OpenSubKey(path, false))
            {
                object v = key != null ? key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames) : null;
                if (v != null)
                {
                    entry.existed = true;
                    entry.kind = key.GetValueKind(name).ToString();
                    entry.value = EncodeValue(v);
                }
            }
            backup.registry[id] = entry;
        }

        private static void SetDword(PerfBackup backup, RegistryKey root, string rootName, string path, string name, int value)
        {
            Remember(backup, root, rootName, path, name);
            using (var key = root.CreateSubKey(path)) key.SetValue(name, value, RegistryValueKind.DWord);
        }

        private static void SetString(PerfBackup backup, RegistryKey root, string rootName, string path, string name, string value)
        {
            Remember(backup, root, rootName, path, name);
            using (var key = root.CreateSubKey(path)) key.SetValue(name, value, RegistryValueKind.String);
        }

        private static void SetBinary(PerfBackup backup, RegistryKey root, string rootName, string path, string name, byte[] value)
        {
            Remember(backup, root, rootName, path, name);
            using (var key = root.CreateSubKey(path)) key.SetValue(name, value, RegistryValueKind.Binary);
        }

        internal static string EncodeValue(object v)
        {
            if (v is byte[]) return Convert.ToBase64String((byte[])v);
            if (v is string[]) return string.Join("\u0001", (string[])v);
            return Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture);
        }

        private static object DecodeValue(RegBackup e)
        {
            switch (ParseKind(e.kind))
            {
                case RegistryValueKind.Binary: return Convert.FromBase64String(e.value ?? "");
                case RegistryValueKind.DWord: return unchecked((int)Convert.ToInt64(e.value, System.Globalization.CultureInfo.InvariantCulture));
                case RegistryValueKind.QWord: return Convert.ToInt64(e.value, System.Globalization.CultureInfo.InvariantCulture);
                case RegistryValueKind.MultiString: return (e.value ?? "").Split('\u0001');
                default: return e.value ?? "";
            }
        }

        private static RegistryValueKind ParseKind(string kind)
        {
            RegistryValueKind k;
            return Enum.TryParse(kind, out k) ? k : RegistryValueKind.String;
        }

        private static int ReadInt(RegistryKey root, string path, string name)
        {
            try
            {
                using (var key = root.OpenSubKey(path, false))
                {
                    object v = key != null ? key.GetValue(name) : null;
                    return v is int ? (int)v : -1;
                }
            }
            catch
            {
                return -1;
            }
        }

        private static string ReadString(RegistryKey root, string path, string name)
        {
            try
            {
                using (var key = root.OpenSubKey(path, false))
                {
                    return key != null ? key.GetValue(name) as string : null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Registry ຂອງ user ທີ່ login ຢູ່ໜ້າຈໍ (HKU\SID). ຖ້າຫາບໍ່ໄດ້ ໃຊ້ HKCU</summary>
        private static RegistryKey OpenTargetUserRoot()
        {
            string ignored;
            return OpenTargetUserRoot(out ignored);
        }

        private static RegistryKey OpenTargetUserRoot(out string rootName)
        {
            string sid = UserAccountService.GetInteractiveUserSid();
            if (!string.IsNullOrEmpty(sid))
            {
                RegistryKey k = OpenRootByName(@"HKU\" + sid);
                if (k != null)
                {
                    rootName = @"HKU\" + sid;
                    return k;
                }
            }
            rootName = @"HKU\" + System.Security.Principal.WindowsIdentity.GetCurrent().User.Value;
            return OpenRootByName(rootName);
        }

        private static RegistryKey OpenRootByName(string rootName)
        {
            if (rootName != null && rootName.StartsWith(@"HKU\", StringComparison.OrdinalIgnoreCase))
            {
                return Registry.Users.OpenSubKey(rootName.Substring(4), true);
            }
            return null;
        }
    }
}
