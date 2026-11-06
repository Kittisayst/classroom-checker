using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using ClassroomChecker.Config;
using ClassroomChecker.Core;
using ClassroomChecker.I18n;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ໂມດູນດຶງຂໍ້ມູນ Hardware/OS ຜ່ານ WMI ແລະ Export ເປັນ Excel (.xlsx) ຕາມ cm-spec.md ຂໍ້ 5.6
    /// </summary>
    public class SpecExportModule : ICheckModule
    {
        public string Id
        {
            get { return "spec_export"; }
        }

        public string DisplayNameKey
        {
            get { return "module_spec"; }
        }

        public bool CanFix
        {
            get { return true; }
        }

        public CheckResult Check()
        {
            var details = new List<string>();
            try
            {
                string targetPath = GetTargetExcelPath();
                details.Add("Target Path: " + targetPath);

                if (File.Exists(targetPath))
                {
                    FileInfo fi = new FileInfo(targetPath);
                    string summary = string.Format("ມີໄຟລ໌ Spec Excel ແລ້ວ: {0} ({1:F1} KB, {2})",
                        Path.GetFileName(targetPath), fi.Length / 1024.0, fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
                    return new CheckResult(CheckStatus.Pass, summary, details);
                }
                else
                {
                    return new CheckResult(CheckStatus.Fail, "ຍັງບໍ່ທັນໄດ້ Export Spec ເຄື່ອງ (" + Path.GetFileName(targetPath) + ")", details);
                }
            }
            catch (Exception ex)
            {
                details.Add("Error: " + ex.Message);
                return new CheckResult(CheckStatus.Unknown, "ເກີດຂໍ້ຜິດພາດ: " + ex.Message, details);
            }
        }

        public CheckResult Fix()
        {
            var details = new List<string>();

            try
            {
                string targetPath = GetTargetExcelPath();
                string outputDir = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                AppConfig cfg = ConfigService.Instance.Current;
                string room = cfg.selected != null ? cfg.selected.room : "ຫ້ອງ A";
                int number = cfg.selected != null ? cfg.selected.number : 1;

                SimpleXlsxWriter writer = new SimpleXlsxWriter();

                // ----------------------------------------------------
                // Sheet 1: Machine Spec (Hardware & OS)
                // ----------------------------------------------------
                XlsxSheet specSheet = new XlsxSheet("Machine Spec");
                specSheet.AddHeader("Category (ໝວດໝູ່)", "Item (ລາຍການ)", "Value (ຄ່າ / ລາຍລະອຽດ)");

                // General Info
                specSheet.AddRow("General (ທົ່ວໄປ)", "Room (ຫ້ອງຮຽນ)", room);
                specSheet.AddRow("General (ທົ່ວໄປ)", "Computer Number (ເລກເຄື່ອງ)", number.ToString("D2"));
                specSheet.AddRow("General (ທົ່ວໄປ)", "Hostname (ຊື່ເຄື່ອງ)", Environment.MachineName);
                specSheet.AddRow("General (ທົ່ວໄປ)", "Current User (ບັນຊີ)", Environment.UserName);
                specSheet.AddRow("General (ທົ່ວໄປ)", "Export Date (ວັນທີ)", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                // System & BIOS
                string manufacturer, model, serial, board;
                GetSystemAndBiosInfo(out manufacturer, out model, out serial, out board);
                specSheet.AddRow("System & BIOS", "Manufacturer (ຍີ່ຫໍ້)", manufacturer);
                specSheet.AddRow("System & BIOS", "Model (ລຸ້ນ)", model);
                specSheet.AddRow("System & BIOS", "Serial Number", serial);
                specSheet.AddRow("System & BIOS", "Mainboard", board);

                // OS
                string osCaption, osVersion, osBuild, osArch;
                GetOsInfo(out osCaption, out osVersion, out osBuild, out osArch);
                specSheet.AddRow("Operating System", "OS Name", osCaption);
                specSheet.AddRow("Operating System", "Version", osVersion);
                specSheet.AddRow("Operating System", "Build Number", osBuild);
                specSheet.AddRow("Operating System", "Architecture", osArch);

                // CPU
                string cpuName;
                int cores, threads;
                GetCpuInfo(out cpuName, out cores, out threads);
                specSheet.AddRow("Processor (CPU)", "CPU Model", cpuName);
                specSheet.AddRow("Processor (CPU)", "Cores / Threads", string.Format("{0} Cores / {1} Threads", cores, threads));

                // RAM
                string totalRam;
                List<string> ramSlots;
                GetRamInfo(out totalRam, out ramSlots);
                specSheet.AddRow("Memory (RAM)", "Total Capacity (ຂະໜາດລວມ)", totalRam);
                for (int i = 0; i < ramSlots.Count; i++)
                {
                    specSheet.AddRow("Memory (RAM)", string.Format("RAM Slot {0}", i + 1), ramSlots[i]);
                }

                // Storage (Disks)
                List<string> physicalDisks;
                GetPhysicalDisks(out physicalDisks);
                for (int i = 0; i < physicalDisks.Count; i++)
                {
                    specSheet.AddRow("Storage (Disk)", string.Format("Physical Drive {0}", i + 1), physicalDisks[i]);
                }

                // Logical Drives
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        double totalGb = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                        double freeGb = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                        string val = string.Format("Total: {0:F1} GB, Free: {1:F1} GB ({2})", totalGb, freeGb, drive.VolumeLabel);
                        specSheet.AddRow("Storage (Partitions)", string.Format("Drive {0}", drive.Name), val);
                    }
                }

                // GPU
                List<string> gpus;
                GetGpuInfo(out gpus);
                for (int i = 0; i < gpus.Count; i++)
                {
                    specSheet.AddRow("Graphics (GPU)", string.Format("Display Adapter {0}", i + 1), gpus[i]);
                }

                // Network
                List<string> nets;
                GetNetworkInfo(out nets);
                for (int i = 0; i < nets.Count; i++)
                {
                    specSheet.AddRow("Network", string.Format("Interface {0}", i + 1), nets[i]);
                }

                writer.AddSheet(specSheet);

                // ----------------------------------------------------
                // Sheet 2: System Check Results (ຜົນການກວດສອບ)
                // ----------------------------------------------------
                XlsxSheet checkSheet = new XlsxSheet("Check Results");
                checkSheet.AddHeader("Module ID", "Check Item (ລາຍການກວດ)", "Status (ສະຖານະ)", "Summary (ຜົນສະຫຼຸບ)");

                var modulesToScan = new List<ICheckModule>
                {
                    new OfficeCheckModule(),
                    new WindowsLicenseCheckModule(),
                    new FontCheckModule(),
                    new KeyboardCheckModule(),
                    new PowerModule(),
                    new CleanupModule(),
                    new DriverCheckModule()
                };

                foreach (var mod in modulesToScan)
                {
                    CheckResult res;
                    try
                    {
                        res = mod.Check();
                    }
                    catch (Exception ex)
                    {
                        res = new CheckResult(CheckStatus.Unknown, ex.Message);
                    }

                    checkSheet.AddRow(
                        mod.Id,
                        Localizer.T(mod.DisplayNameKey),
                        res.Status.ToString(),
                        res.Summary
                    );
                }

                writer.AddSheet(checkSheet);

                // ບັນທຶກໄຟລ໌ Excel (.xlsx)
                writer.Save(targetPath);

                details.Add("Saved to: " + targetPath);
                string summaryMsg = string.Format("Export Spec ເປັນ Excel ສຳເລັດ: {0}", Path.GetFileName(targetPath));
                return new CheckResult(CheckStatus.Pass, summaryMsg, details);
            }
            catch (Exception ex)
            {
                details.Add("Error: " + ex.Message);
                return new CheckResult(CheckStatus.Fail, "ເກີດຂໍ້ຜິດພາດໃນການ Export Spec: " + ex.Message, details);
            }
        }

        public string GetTargetExcelPath()
        {
            AppConfig cfg = ConfigService.Instance.Current;
            string room = cfg.selected != null ? cfg.selected.room : "ຫ້ອງ A";
            int number = cfg.selected != null ? cfg.selected.number : 1;

            string safeRoom = SanitizeFileName(room);
            string fileName = string.Format("{0}-{1:D2}.xlsx", safeRoom, number);

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            try
            {
                string asmLoc = typeof(SpecExportModule).Assembly.Location;
                if (!string.IsNullOrEmpty(asmLoc))
                {
                    string asmDir = Path.GetDirectoryName(asmLoc);
                    if (!string.IsNullOrEmpty(asmDir) && Directory.Exists(asmDir))
                    {
                        baseDir = asmDir;
                    }
                }
            }
            catch { }

            string subDir = !string.IsNullOrEmpty(cfg.specOutputDir) ? cfg.specOutputDir : "specs";
            string targetDir = Path.Combine(baseDir, subDir);

            // ກວດສອບສິດການຂຽນ; ຖ້າຂຽນບໍ່ໄດ້ (ເຊັ່ນ baseDir ເປັນ System32 ຫຼື USB read-only)
            bool dirReady = false;
            try
            {
                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
                string testProbe = Path.Combine(targetDir, ".probe_" + Guid.NewGuid().ToString("N"));
                File.WriteAllText(testProbe, "ok");
                File.Delete(testProbe);
                dirReady = true;
            }
            catch { }

            if (!dirReady)
            {
                // ລອງ CurrentDirectory (ເຊັ່ນ ໂຟນເດີ workspace ຫຼື terminal)
                try
                {
                    string currDir = Path.Combine(Environment.CurrentDirectory, subDir);
                    if (!Directory.Exists(currDir)) Directory.CreateDirectory(currDir);
                    string testProbe = Path.Combine(currDir, ".probe_" + Guid.NewGuid().ToString("N"));
                    File.WriteAllText(testProbe, "ok");
                    File.Delete(testProbe);
                    targetDir = currDir;
                    dirReady = true;
                }
                catch { }
            }

            if (!dirReady)
            {
                // Fallback: LocalApplicationData
                try
                {
                    string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string localDir = Path.Combine(localApp, "ClassroomChecker", subDir);
                    if (!Directory.Exists(localDir)) Directory.CreateDirectory(localDir);
                    targetDir = localDir;
                    dirReady = true;
                }
                catch { }
            }

            if (!dirReady)
            {
                // Fallback ສຸດທ້າຍ: Temp folder
                string tempDir = Path.Combine(Path.GetTempPath(), "ClassroomChecker", subDir);
                try { if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir); } catch { }
                targetDir = tempDir;
            }

            return Path.Combine(targetDir, fileName);
        }

        private string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Room";
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()) + "/\\:*?\"<>|");
            string clean = Regex.Replace(name, "[" + invalidChars + "]", "_");
            return clean.Trim();
        }

        private void GetSystemAndBiosInfo(out string manufacturer, out string model, out string serial, out string board)
        {
            manufacturer = "Unknown";
            model = "Unknown";
            serial = "Unknown";
            board = "Unknown";

            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem"))
                foreach (ManagementObject mo in s.Get())
                {
                    if (mo["Manufacturer"] != null) manufacturer = mo["Manufacturer"].ToString();
                    if (mo["Model"] != null) model = mo["Model"].ToString();
                }
            }
            catch { }

            try
            {
                using (var s = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS"))
                foreach (ManagementObject mo in s.Get())
                {
                    if (mo["SerialNumber"] != null) serial = mo["SerialNumber"].ToString();
                }
            }
            catch { }

            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Manufacturer, Product, SerialNumber FROM Win32_BaseBoard"))
                foreach (ManagementObject mo in s.Get())
                {
                    string m = mo["Manufacturer"] != null ? mo["Manufacturer"].ToString() : "";
                    string p = mo["Product"] != null ? mo["Product"].ToString() : "";
                    board = (m + " " + p).Trim();
                }
            }
            catch { }
        }

        private void GetOsInfo(out string caption, out string version, out string build, out string arch)
        {
            caption = "Windows";
            version = Environment.OSVersion.Version.ToString();
            build = "Unknown";
            arch = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";

            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Caption, Version, BuildNumber, OSArchitecture FROM Win32_OperatingSystem"))
                foreach (ManagementObject mo in s.Get())
                {
                    if (mo["Caption"] != null) caption = mo["Caption"].ToString();
                    if (mo["Version"] != null) version = mo["Version"].ToString();
                    if (mo["BuildNumber"] != null) build = mo["BuildNumber"].ToString();
                    if (mo["OSArchitecture"] != null) arch = mo["OSArchitecture"].ToString();
                }
            }
            catch { }
        }

        private void GetCpuInfo(out string name, out int cores, out int threads)
        {
            name = "Unknown CPU";
            cores = Environment.ProcessorCount;
            threads = Environment.ProcessorCount;

            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor"))
                foreach (ManagementObject mo in s.Get())
                {
                    if (mo["Name"] != null) name = mo["Name"].ToString().Trim();
                    if (mo["NumberOfCores"] != null) cores = Convert.ToInt32(mo["NumberOfCores"]);
                    if (mo["NumberOfLogicalProcessors"] != null) threads = Convert.ToInt32(mo["NumberOfLogicalProcessors"]);
                }
            }
            catch { }
        }

        private void GetRamInfo(out string totalStr, out List<string> slots)
        {
            totalStr = "Unknown";
            slots = new List<string>();

            try
            {
                using (var s = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                foreach (ManagementObject mo in s.Get())
                {
                    if (mo["TotalPhysicalMemory"] != null)
                    {
                        double bytes = Convert.ToDouble(mo["TotalPhysicalMemory"]);
                        double gb = bytes / (1024.0 * 1024.0 * 1024.0);
                        totalStr = string.Format("{0:F1} GB", gb);
                    }
                }
            }
            catch { }

            try
            {
                using (var s = new ManagementObjectSearcher("SELECT DeviceLocator, Capacity, Speed, MemoryType FROM Win32_PhysicalMemory"))
                foreach (ManagementObject mo in s.Get())
                {
                    string loc = mo["DeviceLocator"] != null ? mo["DeviceLocator"].ToString() : "Slot";
                    double capGb = 0;
                    if (mo["Capacity"] != null)
                    {
                        capGb = Convert.ToDouble(mo["Capacity"]) / (1024.0 * 1024.0 * 1024.0);
                    }
                    string speed = mo["Speed"] != null ? mo["Speed"].ToString() + " MHz" : "";
                    slots.Add(string.Format("{0}: {1:F0} GB {2}", loc, capGb, speed));
                }
            }
            catch { }

            if (slots.Count == 0 && totalStr != "Unknown")
            {
                slots.Add("System Memory: " + totalStr);
            }
        }

        private void GetPhysicalDisks(out List<string> disks)
        {
            disks = new List<string>();

            // 1. ລອງ MSFT_PhysicalDisk ໃນ root\Microsoft\Windows\Storage (ອ່ານ SSD / HDD)
            try
            {
                ManagementScope scope = new ManagementScope(@"root\Microsoft\Windows\Storage");
                scope.Connect();
                ObjectQuery query = new ObjectQuery("SELECT FriendlyName, MediaType, Size FROM MSFT_PhysicalDisk");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        string name = mo["FriendlyName"] != null ? mo["FriendlyName"].ToString() : "Disk";
                        double gb = 0;
                        if (mo["Size"] != null)
                        {
                            gb = Convert.ToDouble(mo["Size"]) / (1024.0 * 1024.0 * 1024.0);
                        }
                        string mediaType = "Unspecified";
                        if (mo["MediaType"] != null)
                        {
                            int type = Convert.ToInt32(mo["MediaType"]);
                            if (type == 3) mediaType = "HDD";
                            else if (type == 4) mediaType = "SSD";
                            else if (type == 5) mediaType = "SCM";
                        }
                        disks.Add(string.Format("{0} ({1:F0} GB, {2})", name, gb, mediaType));
                    }
                }
            }
            catch { }

            // 2. Fallback: Win32_DiskDrive
            if (disks.Count == 0)
            {
                try
                {
                    using (var s = new ManagementObjectSearcher("SELECT Model, Size FROM Win32_DiskDrive"))
                    foreach (ManagementObject mo in s.Get())
                    {
                        string model = mo["Model"] != null ? mo["Model"].ToString() : "Disk";
                        double gb = 0;
                        if (mo["Size"] != null)
                        {
                            gb = Convert.ToDouble(mo["Size"]) / (1024.0 * 1024.0 * 1024.0);
                        }
                        disks.Add(string.Format("{0} ({1:F0} GB)", model, gb));
                    }
                }
                catch { }
            }
        }

        private void GetGpuInfo(out List<string> gpus)
        {
            gpus = new List<string>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Name, DriverVersion, AdapterRAM FROM Win32_VideoController"))
                foreach (ManagementObject mo in s.Get())
                {
                    string name = mo["Name"] != null ? mo["Name"].ToString() : "GPU";
                    string ver = mo["DriverVersion"] != null ? "Driver: " + mo["DriverVersion"].ToString() : "";
                    gpus.Add(string.Format("{0} ({1})", name, ver));
                }
            }
            catch { }
        }

        private void GetNetworkInfo(out List<string> nets)
        {
            nets = new List<string>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Description, MACAddress, IPAddress FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True"))
                foreach (ManagementObject mo in s.Get())
                {
                    string desc = mo["Description"] != null ? mo["Description"].ToString() : "Network Adapter";
                    string mac = mo["MACAddress"] != null ? mo["MACAddress"].ToString() : "";
                    string ipStr = "";
                    if (mo["IPAddress"] is string[])
                    {
                        ipStr = string.Join(", ", (string[])mo["IPAddress"]);
                    }
                    nets.Add(string.Format("{0} [MAC: {1}, IP: {2}]", desc, mac, ipStr));
                }
            }
            catch { }
        }
    }
}
