using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml;
using ClassroomChecker.Config;

namespace ClassroomChecker.Core
{
    public class MachineSpecSummary
    {
        public string SourceFile { get; set; }
        public string Room { get; set; }
        public int Number { get; set; }
        public string Hostname { get; set; }
        public string User { get; set; }
        public string ExportDate { get; set; }

        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
        public string Board { get; set; }

        public string OsName { get; set; }
        public string OsVersion { get; set; }
        public string OsBuild { get; set; }
        public string OsArch { get; set; }

        public string CpuModel { get; set; }
        public string CoresThreads { get; set; }

        public string TotalRam { get; set; }
        public List<string> RamSlots { get; set; }

        public List<string> PhysicalDisks { get; set; }
        public List<string> LogicalDrives { get; set; }

        public List<string> Gpus { get; set; }
        public List<string> Networks { get; set; }

        public Dictionary<string, string> CheckStatuses { get; set; }
        public Dictionary<string, string> CheckSummaries { get; set; }

        public MachineSpecSummary()
        {
            Room = "Unknown";
            Number = 0;
            Hostname = "Unknown";
            User = "Unknown";
            ExportDate = "Unknown";

            Manufacturer = "-";
            Model = "-";
            Serial = "-";
            Board = "-";

            OsName = "-";
            OsVersion = "-";
            OsBuild = "-";
            OsArch = "-";

            CpuModel = "-";
            CoresThreads = "-";

            TotalRam = "-";
            RamSlots = new List<string>();

            PhysicalDisks = new List<string>();
            LogicalDrives = new List<string>();

            Gpus = new List<string>();
            Networks = new List<string>();

            CheckStatuses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            CheckSummaries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// ບໍລິການລວມໄຟລ໌ Excel Spec ຂອງທຸກເຄື່ອງໃນຫ້ອງຮຽນ ເປັນ 1 Master Summary Spreadsheet
    /// </summary>
    public static class MasterSummaryService
    {
        /// <summary>
        /// ອ່ານຂໍ້ມູນ Spec ຈາກໄຟລ໌ .xlsx ດ່ຽວຂອງແຕ່ລະເຄື່ອງ
        /// </summary>
        public static MachineSpecSummary ParseSingleMachineXlsx(string xlsxPath)
        {
            var summary = new MachineSpecSummary { SourceFile = xlsxPath };

            try
            {
                using (FileStream fs = new FileStream(xlsxPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    // 1. ອ່ານ Sheet 1: Machine Spec
                    var sheet1Entry = zip.GetEntry("xl/worksheets/sheet1.xml");
                    if (sheet1Entry != null)
                    {
                        using (Stream stream = sheet1Entry.Open())
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load(stream);

                            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                            nsmgr.AddNamespace("s", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");

                            XmlNodeList rowNodes = doc.SelectNodes("//s:sheetData/s:row", nsmgr);
                            if (rowNodes != null)
                            {
                                foreach (XmlNode rowNode in rowNodes)
                                {
                                    XmlNodeList cellNodes = rowNode.SelectNodes("s:c", nsmgr);
                                    if (cellNodes == null || cellNodes.Count < 3) continue;

                                    string category = GetCellInlineText(cellNodes[0], nsmgr);
                                    string item = GetCellInlineText(cellNodes[1], nsmgr);
                                    string val = GetCellInlineText(cellNodes[2], nsmgr);

                                    // Map items
                                    if (item.IndexOf("Room", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.Room = val;
                                    else if (item.IndexOf("Computer Number", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        int num;
                                        if (int.TryParse(val, out num)) summary.Number = num;
                                    }
                                    else if (item.IndexOf("Hostname", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.Hostname = val;
                                    else if (item.IndexOf("Current User", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.User = val;
                                    else if (item.IndexOf("Export Date", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.ExportDate = val;
                                    else if (item.IndexOf("Manufacturer", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.Manufacturer = val;
                                    else if (item.IndexOf("Model", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.Model = val;
                                    else if (item.IndexOf("Serial Number", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.Serial = val;
                                    else if (item.IndexOf("Mainboard", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.Board = val;
                                    else if (item.IndexOf("OS Name", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.OsName = val;
                                    else if (item.IndexOf("Version", StringComparison.OrdinalIgnoreCase) >= 0 && category.IndexOf("Operating", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.OsVersion = val;
                                    else if (item.IndexOf("Architecture", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.OsArch = val;
                                    else if (item.IndexOf("CPU Model", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.CpuModel = val;
                                    else if (item.IndexOf("Cores / Threads", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.CoresThreads = val;
                                    else if (item.IndexOf("Total Capacity", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.TotalRam = val;
                                    else if (item.IndexOf("RAM Slot", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.RamSlots.Add(val);
                                    else if (item.IndexOf("Physical Drive", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.PhysicalDisks.Add(val);
                                    else if (item.IndexOf("Drive ", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.LogicalDrives.Add(item + ": " + val);
                                    else if (item.IndexOf("Display Adapter", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.Gpus.Add(val);
                                    else if (item.IndexOf("Interface", StringComparison.OrdinalIgnoreCase) >= 0)
                                        summary.Networks.Add(val);
                                }
                            }
                        }
                    }

                    // 2. ອ່ານ Sheet 2: Check Results
                    var sheet2Entry = zip.GetEntry("xl/worksheets/sheet2.xml");
                    if (sheet2Entry != null)
                    {
                        using (Stream stream = sheet2Entry.Open())
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load(stream);

                            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                            nsmgr.AddNamespace("s", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");

                            XmlNodeList rowNodes = doc.SelectNodes("//s:sheetData/s:row", nsmgr);
                            if (rowNodes != null)
                            {
                                foreach (XmlNode rowNode in rowNodes)
                                {
                                    XmlNodeList cellNodes = rowNode.SelectNodes("s:c", nsmgr);
                                    if (cellNodes == null || cellNodes.Count < 4) continue;

                                    string modId = GetCellInlineText(cellNodes[0], nsmgr);
                                    string modName = GetCellInlineText(cellNodes[1], nsmgr);
                                    string status = GetCellInlineText(cellNodes[2], nsmgr);
                                    string sumTxt = GetCellInlineText(cellNodes[3], nsmgr);

                                    if (!string.IsNullOrEmpty(modId) && !modId.Equals("Module ID", StringComparison.OrdinalIgnoreCase))
                                    {
                                        summary.CheckStatuses[modId] = status;
                                        summary.CheckSummaries[modId] = sumTxt;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            return summary;
        }

        private static string GetCellInlineText(XmlNode cellNode, XmlNamespaceManager nsmgr)
        {
            if (cellNode == null) return string.Empty;
            XmlNode textNode = cellNode.SelectSingleNode("s:is/s:t", nsmgr) ?? cellNode.SelectSingleNode("s:v", nsmgr);
            return textNode != null ? textNode.InnerText : string.Empty;
        }

        /// <summary>
        /// ອ່ານໄຟລ໌ specs ທັງໝົດໃນໂຟນເດີ
        /// </summary>
        public static List<MachineSpecSummary> LoadAllMachineSpecs(string specsDir, string roomFilter = null)
        {
            var results = new List<MachineSpecSummary>();
            if (string.IsNullOrEmpty(specsDir) || !Directory.Exists(specsDir))
            {
                return results;
            }

            string[] files = Directory.GetFiles(specsDir, "*.xlsx");
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                // ຂ້າມ Master Summary ແລະ temporary files
                if (fileName.StartsWith("Master_Summary", StringComparison.OrdinalIgnoreCase) ||
                    fileName.StartsWith("~$", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var spec = ParseSingleMachineXlsx(file);
                if (spec != null && spec.Number > 0)
                {
                    if (string.IsNullOrEmpty(roomFilter) || spec.Room.Equals(roomFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(spec);
                    }
                }
            }

            // ຮຽງຕາມເລກເຄື່ອງ 01, 02, 03...
            results.Sort((a, b) => a.Number.CompareTo(b.Number));
            return results;
        }

        /// <summary>
        /// ສ້າງໄຟລ໌ Master Summary Excel (.xlsx) ລວມທຸກເຄື່ອງໃນຫ້ອງ
        /// </summary>
        public static string GenerateMasterSummary(string specsDir, string roomName)
        {
            if (string.IsNullOrEmpty(specsDir) || !Directory.Exists(specsDir))
            {
                throw new DirectoryNotFoundException("Specs directory not found: " + specsDir);
            }

            List<MachineSpecSummary> machines = LoadAllMachineSpecs(specsDir, roomName);
            if (machines.Count == 0)
            {
                // ລອງໂຫຼດທຸກເຄື່ອງ ຖ້າບໍ່ມີ filter room
                machines = LoadAllMachineSpecs(specsDir, null);
            }

            if (machines.Count == 0)
            {
                throw new InvalidOperationException("No machine spec files found in " + specsDir);
            }

            string targetRoom = !string.IsNullOrEmpty(roomName) ? roomName : machines[0].Room;
            string safeRoom = SanitizeFileName(targetRoom);
            string outputPath = Path.Combine(specsDir, string.Format("Master_Summary_{0}.xlsx", safeRoom));

            // ຄິດໄລ່ KPIs
            int total = machines.Count;
            int winOk = 0, officeOk = 0, fontOk = 0, kbdOk = 0, powerOk = 0, driverOk = 0;

            foreach (var m in machines)
            {
                string s;
                if (m.CheckStatuses.TryGetValue("windows_license", out s) && s.Equals("Pass", StringComparison.OrdinalIgnoreCase)) winOk++;
                if (m.CheckStatuses.TryGetValue("office", out s) && s.Equals("Pass", StringComparison.OrdinalIgnoreCase)) officeOk++;
                if (m.CheckStatuses.TryGetValue("font", out s) && s.Equals("Pass", StringComparison.OrdinalIgnoreCase)) fontOk++;
                if (m.CheckStatuses.TryGetValue("keyboard", out s) && s.Equals("Pass", StringComparison.OrdinalIgnoreCase)) kbdOk++;
                if (m.CheckStatuses.TryGetValue("power", out s) && s.Equals("Pass", StringComparison.OrdinalIgnoreCase)) powerOk++;
                if (m.CheckStatuses.TryGetValue("driver", out s) && s.Equals("Pass", StringComparison.OrdinalIgnoreCase)) driverOk++;
            }

            SimpleXlsxWriter writer = new SimpleXlsxWriter();

            // -----------------------------------------------------------------
            // Sheet 1: Lab Overview (ຕາຕະລາງໃຫຍ່ສະຫຼຸບທຸກເຄື່ອງ)
            // -----------------------------------------------------------------
            XlsxSheet overviewSheet = new XlsxSheet("Lab Overview");
            overviewSheet.AddHeader("CLASSROOM COMPUTERS MASTER INVENTORY — ໃບສະຫຼຸບສະເປັກຄອມພິວເຕີຫ້ອງຮຽນ");
            overviewSheet.AddRow("Room (ຫ້ອງຮຽນ)", targetRoom, "Total Computers (ຈຳນວນເຄື່ອງ)", total.ToString(), "Export Date (ວັນທີອອກລາຍງານ)", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            overviewSheet.AddRow("Windows Activation", string.Format("{0} / {1} ({2:F0}%)", winOk, total, (winOk * 100.0 / total)),
                                 "MS Office Ready", string.Format("{0} / {1} ({2:F0}%)", officeOk, total, (officeOk * 100.0 / total)),
                                 "Phetsarath Font Ready", string.Format("{0} / {1}", fontOk, total));
            overviewSheet.AddRow("Lao Keyboard Ready", string.Format("{0} / {1}", kbdOk, total),
                                 "Power Never Sleep", string.Format("{0} / {1}", powerOk, total),
                                 "Drivers Healthy", string.Format("{0} / {1}", driverOk, total));
            overviewSheet.AddRow(""); // Blank separator

            // Main Table Header (18 columns)
            overviewSheet.AddHeader(
                "PC #",
                "Hostname",
                "User",
                "Processor (CPU)",
                "Cores/Threads",
                "Total RAM",
                "Primary Storage",
                "Logical Drives (Free Space)",
                "Display GPU",
                "Network (IP & MAC)",
                "Operating System",
                "Windows License",
                "MS Office",
                "Lao Font",
                "Lao Keyboard",
                "Power Scheme",
                "Driver Status",
                "Inspected Date"
            );

            foreach (var m in machines)
            {
                string primDisk = m.PhysicalDisks.Count > 0 ? m.PhysicalDisks[0] : "-";
                string logDrives = string.Join("; ", m.LogicalDrives.ToArray());
                string gpu = m.Gpus.Count > 0 ? m.Gpus[0] : "-";
                string net = m.Networks.Count > 0 ? m.Networks[0] : "-";

                string winLic = m.CheckStatuses.ContainsKey("windows_license") ? m.CheckStatuses["windows_license"] : "-";
                string offLic = m.CheckStatuses.ContainsKey("office") ? m.CheckStatuses["office"] : "-";
                string fontSt = m.CheckStatuses.ContainsKey("font") ? m.CheckStatuses["font"] : "-";
                string kbdSt = m.CheckStatuses.ContainsKey("keyboard") ? m.CheckStatuses["keyboard"] : "-";
                string pwrSt = m.CheckStatuses.ContainsKey("power") ? m.CheckStatuses["power"] : "-";
                string drvSt = m.CheckStatuses.ContainsKey("driver") ? m.CheckStatuses["driver"] : "-";

                overviewSheet.AddRow(
                    m.Number.ToString("D2"),
                    m.Hostname,
                    m.User,
                    m.CpuModel,
                    m.CoresThreads,
                    m.TotalRam,
                    primDisk,
                    logDrives,
                    gpu,
                    net,
                    m.OsName + " (" + m.OsArch + ")",
                    winLic,
                    offLic,
                    fontSt,
                    kbdSt,
                    pwrSt,
                    drvSt,
                    m.ExportDate
                );
            }

            writer.AddSheet(overviewSheet);

            // -----------------------------------------------------------------
            // Sheet 2: Hardware Slots & Components
            // -----------------------------------------------------------------
            XlsxSheet hwSheet = new XlsxSheet("Hardware Components");
            hwSheet.AddHeader("PC #", "Hostname", "Manufacturer", "Model", "Serial", "Mainboard", "Total RAM", "RAM Slots Breakdown", "Storage Drives Breakdown", "GPU Adapters");

            foreach (var m in machines)
            {
                hwSheet.AddRow(
                    m.Number.ToString("D2"),
                    m.Hostname,
                    m.Manufacturer,
                    m.Model,
                    m.Serial,
                    m.Board,
                    m.TotalRam,
                    string.Join(" | ", m.RamSlots.ToArray()),
                    string.Join(" | ", m.PhysicalDisks.ToArray()),
                    string.Join(" | ", m.Gpus.ToArray())
                );
            }

            writer.AddSheet(hwSheet);

            // -----------------------------------------------------------------
            // Sheet 3: Full Checks Matrix
            // -----------------------------------------------------------------
            XlsxSheet checkMatrix = new XlsxSheet("Check Summaries");
            checkMatrix.AddHeader("PC #", "Hostname", "MS Office Summary", "Windows License Summary", "Font Summary", "Keyboard Summary", "Power Summary", "Cleanup Summary", "Driver Summary");

            foreach (var m in machines)
            {
                Func<string, string> getSum = (id) => m.CheckSummaries.ContainsKey(id) ? m.CheckSummaries[id] : "-";
                checkMatrix.AddRow(
                    m.Number.ToString("D2"),
                    m.Hostname,
                    getSum("office"),
                    getSum("windows_license"),
                    getSum("font"),
                    getSum("keyboard"),
                    getSum("power"),
                    getSum("cleanup"),
                    getSum("driver")
                );
            }

            writer.AddSheet(checkMatrix);

            // ບັນທຶກໄຟລ໌ Master
            writer.Save(outputPath);
            return outputPath;
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Lab";
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()) + "/\\:*?\"<>|");
            string clean = Regex.Replace(name, "[" + invalidChars + "]", "_");
            return clean.Trim();
        }
    }
}
