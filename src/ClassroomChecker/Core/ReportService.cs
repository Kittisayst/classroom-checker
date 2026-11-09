using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ClassroomChecker.Config;
using ClassroomChecker.I18n;

namespace ClassroomChecker.Core
{
    /// <summary>
    /// Log file ແລະ ລາຍງານອັດຕະໂນມັດ: ບັນທຶກລົງໂຟນເດີ logs/ ແລະ reports/ ຂ້າງ exe (USB).
    /// ຖ້າຂຽນບໍ່ໄດ້ (USB read-only) → fallback ໄປ %APPDATA%\ClassroomChecker\.
    /// ທຸກ method ຕ້ອງບໍ່ໂຍນ exception (ການ log ຜິດພາດຫ້າມທຳລາຍການເຮັດວຽກຫຼັກ).
    /// </summary>
    public static class ReportService
    {
        private static readonly object _lock = new object();

        /// <summary>ຊື່ເຄື່ອງ: "{ຫ້ອງ}-{ເລກ}" ທີ່ sanitize ແລ້ວ</summary>
        public static string GetMachineTag()
        {
            string room = "Room";
            int number = 0;
            try
            {
                var sel = ConfigService.Instance.Current.selected;
                if (sel != null)
                {
                    if (!string.IsNullOrWhiteSpace(sel.room)) room = sel.room;
                    number = sel.number;
                }
            }
            catch { }
            return SanitizeFileName(room + "-" + number);
        }

        /// <summary>ລຶບຕົວອັກສອນທີ່ Windows ຫ້າມໃຊ້ໃນຊື່ໄຟລ໌</summary>
        public static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        /// <summary>ຫາໂຟນເດີ output ທີ່ຂຽນໄດ້ (ຂ້າງ exe ກ່ອນ, ບໍ່ໄດ້ຈຶ່ງໃຊ້ AppData)</summary>
        private static string ResolveDir(string subFolder)
        {
            string primary = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, subFolder);
            try
            {
                Directory.CreateDirectory(primary);
                string probe = Path.Combine(primary, ".write_test");
                File.WriteAllText(probe, "x");
                File.Delete(probe);
                return primary;
            }
            catch
            {
                string fallback = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ClassroomChecker", subFolder);
                Directory.CreateDirectory(fallback);
                return fallback;
            }
        }

        /// <summary>ຂະໜາດສູງສຸດຂອງ log ກ່ອນຖືກຍ້າຍເປັນ .old (1 MB) — ກັນ log ໂຕຂຶ້ນບໍ່ມີທີ່ສິ້ນສຸດໃນ USB</summary>
        public const long MaxLogBytes = 1024 * 1024;

        /// <summary>ຖ້າ log ເກີນ MaxLogBytes: ຍ້າຍເປັນ {ຊື່}.log.old (ຂຽນທັບ .old ເກົ່າ) ແລ້ວເລີ່ມ log ໃໝ່. ເກັບໄວ້ 2 ໄຟລ໌</summary>
        public static void RotateIfTooLarge(string logPath)
        {
            try
            {
                var fi = new FileInfo(logPath);
                if (!fi.Exists || fi.Length <= MaxLogBytes) return;

                string old = logPath + ".old";
                if (File.Exists(old)) File.Delete(old);
                File.Move(logPath, old);
            }
            catch { }
        }

        /// <summary>ຕໍ່ທ້າຍ log: logs/{ຫ້ອງ}-{ເລກ}.log</summary>
        public static void Log(string message)
        {
            try
            {
                lock (_lock)
                {
                    string path = Path.Combine(ResolveDir("logs"), GetMachineTag() + ".log");
                    RotateIfTooLarge(path);
                    string line = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}{2}",
                        DateTime.Now, message, Environment.NewLine);
                    File.AppendAllText(path, line, Encoding.UTF8);
                }
            }
            catch { }
        }

        /// <summary>ສ້າງຂໍ້ຄວາມລາຍງານຈາກຜົນສະແກນ</summary>
        public static string BuildReport(IDictionary<ICheckModule, CheckResult> results, string summaryLine)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=======================================================================");
            sb.AppendLine("   CLASSROOM CHECKER — SYSTEM SCAN REPORT");
            sb.AppendLine("=======================================================================");
            sb.AppendLine("Room/No: " + GetMachineTag());
            sb.AppendLine("Machine: " + Environment.MachineName);
            sb.AppendLine("User:    " + Environment.UserName);
            sb.AppendLine("Date:    " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("-----------------------------------------------------------------------");

            foreach (var kv in results)
            {
                sb.AppendLine();
                sb.AppendLine(string.Format("[{0}] {1}", kv.Value.Status.ToString().ToUpper(), Localizer.T(kv.Key.DisplayNameKey)));
                sb.AppendLine("Summary: " + kv.Value.Summary);
                if (kv.Value.Details != null)
                {
                    foreach (var d in kv.Value.Details)
                    {
                        sb.AppendLine("  • " + d);
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("=======================================================================");
            sb.AppendLine(summaryLine);
            sb.AppendLine("=======================================================================");
            return sb.ToString();
        }

        /// <summary>
        /// ບັນທຶກລາຍງານອັດຕະໂນມັດ: reports/{ຫ້ອງ}-{ເລກ}_{yyyyMMdd_HHmm}.txt
        /// ສົ່ງຄືນ path ຖ້າສຳເລັດ, null ຖ້າລົ້ມເຫຼວ
        /// </summary>
        public static string AutoSaveReport(IDictionary<ICheckModule, CheckResult> results, string summaryLine)
        {
            try
            {
                if (results == null || results.Count == 0) return null;
                string name = string.Format("{0}_{1:yyyyMMdd_HHmm}.txt", GetMachineTag(), DateTime.Now);
                string path = Path.Combine(ResolveDir("reports"), name);
                File.WriteAllText(path, BuildReport(results, summaryLine), Encoding.UTF8);
                return path;
            }
            catch
            {
                return null;
            }
        }
    }
}
