using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClassroomChecker.Config;
using ClassroomChecker.Core;
using ClassroomChecker.I18n;
using ClassroomChecker.Modules;

namespace ClassroomChecker.Tests
{
    /// <summary>
    /// Unit tests ແບບບໍ່ຕ້ອງໃຊ້ NuGet (ໃຊ້ໄດ້ offline): console exe ທີ່ມີ test runner ນ້ອຍໆໃນຕົວ.
    /// ທົດສອບສະເພາະ logic ທີ່ບໍ່ແຕະລະບົບ Windows (config, scan runner, xlsx, ຊື່ໄຟລ໌, ລາຍງານ).
    /// Exit code 0 = ຜ່ານທັງໝົດ, 1 = ມີຂໍ້ຜິດພາດ.
    /// </summary>
    internal static class Program
    {
        private static int _passed;
        private static int _failed;

        private static int Main()
        {
            // ConfigService ເປັນ singleton ທີ່ໃຊ້ໂຟນເດີຂອງ exe → ທົດສອບກ່ອນສຸດ ແລະ ເກັບກວາດຫຼັງຈົບ
            Run("Config: corrupt JSON -> backup .bad + defaults", Config_CorruptJson_BacksUpAndRecovers);
            Run("Config: missing fields filled with defaults", Config_MissingFields_GetDefaults);
            Run("Config: save then load keeps values (Lao room name)", Config_SaveLoad_RoundTrip);

            Run("FileName: invalid chars sanitized", FileName_InvalidCharsReplaced);
            Run("FileName: Lao text preserved", FileName_LaoPreserved);

            Run("ScanRunner: module exception -> Unknown, runner not stuck", ScanRunner_ModuleThrows_NotStuck);
            Run("ScanRunner: event handler exception -> runner not stuck", ScanRunner_HandlerThrows_NotStuck);
            Run("ScanRunner: concurrent run is rejected", ScanRunner_ConcurrentRun_Rejected);
            Run("ScanRunner: Fix progress counts only fixable modules", ScanRunner_FixProgress_OnlyFixable);

            Run("Xlsx: write then parse round-trip (Lao, & < >)", Xlsx_RoundTrip);

            Run("Report: contains status, summary and details", Report_ContainsContent);
            Run("Log: rotates to .old when over limit", Log_RotatesWhenTooLarge);
            Run("Log: small log is left alone", Log_SmallLogNotRotated);

            Run("DiskSpace: threshold evaluation (<5% Fail, <10% Warn, >=10% Pass)", DiskSpace_ThresholdEvaluation);
            Run("SpecExport: IsKnownSerial filters OEM placeholders", SpecExport_IsKnownSerial);
            Run("SpecExport: duplicate detection when serial known", SpecExport_IsDifferentMachine_KnownSerial);
            Run("SpecExport: duplicate detection when serial unknown falls back to hostname", SpecExport_IsDifferentMachine_FallbackToHostname);
            Run("QuickSetup: PC number auto-increment and max limit boundary", QuickSetup_CalculateNextNumber);
            Run("I18n: btn_save_report localized in both lo and en", I18n_BtnSaveReport_Localized);
            Run("I18n: Settings keys localized in both lo and en", I18n_SettingsKeys_Localized);
            Run("SecurityConfig: defaults include USB, Downloads, and popular games", SecurityConfig_Defaults);
            Run("AppLockModule: metadata and check result contract", AppLockModule_MetadataAndContract);
            Run("I18n: tab_install and installer keys localized", I18n_TabInstall_Localized);
            Run("SoftwareInstaller: catalog contains 11 FOSS/essential packages", SoftwareInstaller_Catalog_PopulatedAndValid);
            Run("SoftwareInstaller: CreateShortcut generates valid .lnk via COM", SoftwareInstaller_CreateShortcut);
            Run("SoftwareInstaller: RAMMap64 is flagged as portable", SoftwareInstaller_RamMapIsPortable);
            Run("SecurityStatusInfo: lock state evaluation and config matching", SecurityStatusInfo_LockStateEvaluation);
            Run("I18n: security button states localized in lo and en", I18n_SecurityLockButtons_Localized);
            Run("DateTimeService: timezone and date format contract", DateTimeService_ConstantsAndContract);
            Run("DesktopIconService: system icons and office shortcuts contract", DesktopIconService_ConstantsAndContract);
            Run("UsbVirusCleanerService: clean, unhide, remove scripts and create vaccine", UsbVirusCleanerService_CleanAndRestore_Contract);
            Run("I18n: UsbCleaner and Vaccine keys localized in lo and en", I18n_UsbCleaner_Localized);
            Run("ComputerNameService: name generation, sanitization and validation contract", ComputerNameService_Contract);
            Run("ComputerNameModule: metadata and check result contract", ComputerNameModule_MetadataAndContract);
            Run("EssentialSoftware: required list is PDF, LaoScript 8, VLC, 7-Zip", EssentialSoftware_RequiredList);
            Run("LogonScreen: config defaults and notice text normalization", LogonScreen_ConfigAndNotice);
            Run("LogonScreen: lock screen file naming and cleanup of old files", LogonScreen_LockScreenFiles);
            Run("I18n: logon screen keys localized in lo and en", I18n_LogonScreen_Localized);
            Run("UserAccount: user name validation, account parsing and defaults", UserAccount_ValidationAndDefaults);
            Run("DateTime: date formats, 12/24-hour patterns and preview", DateTime_FormatsAndPreview);
            Run("DesktopIcons: Office shortcut detection (no duplicates)", DesktopIcons_OfficeShortcutDetection);
            Run("Performance: low-spec rules, safe service list and defaults", Performance_RulesAndDefaults);

            Cleanup();

            Console.WriteLine();
            Console.WriteLine("Passed: {0}  Failed: {1}", _passed, _failed);
            return _failed == 0 ? 0 : 1;
        }

        // ---------------------------------------------------------------- harness

        private static void Run(string name, Action test)
        {
            try
            {
                test();
                _passed++;
                Console.WriteLine("[PASS] " + name);
            }
            catch (Exception ex)
            {
                _failed++;
                Console.WriteLine("[FAIL] " + name + "\n       " + ex.Message);
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        private static void AreEqual<T>(T expected, T actual, string what)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new Exception(string.Format("{0}: expected <{1}> but was <{2}>", what, expected, actual));
        }

        private static string ConfigPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"); }
        }

        private static void Cleanup()
        {
            try { File.Delete(ConfigPath); } catch { }
            try { File.Delete(ConfigPath + ".bad"); } catch { }
        }

        // ---------------------------------------------------------------- config

        private static void Config_CorruptJson_BacksUpAndRecovers()
        {
            File.WriteAllText(ConfigPath, "{ this is not json !!!");
            try { File.Delete(ConfigPath + ".bad"); } catch { }

            ConfigService.Instance.Load();

            Assert(ConfigService.Instance.Current != null, "Current should not be null after corrupt load");
            Assert(File.Exists(ConfigPath + ".bad"), "config.json.bad should be created");
            AreEqual("lo", ConfigService.Instance.Current.language, "default language");
            AreEqual(45, ConfigService.Instance.Current.computerNumberRange.max, "default max");
        }

        private static void Config_MissingFields_GetDefaults()
        {
            File.WriteAllText(ConfigPath, "{\"language\":\"en\",\"rooms\":[]}");
            ConfigService.Instance.Load();

            var c = ConfigService.Instance.Current;
            AreEqual("en", c.language, "language kept");
            Assert(c.rooms != null && c.rooms.Count > 0, "empty rooms list should be refilled");
            Assert(c.selected != null, "selected should be defaulted");
            Assert(c.label != null && c.cleanup != null && c.background != null, "nested sections defaulted");
        }

        private static void Config_SaveLoad_RoundTrip()
        {
            var c = ConfigService.Instance.Current;
            c.selected.room = "ຫ້ອງ ທົດສອບ";
            c.selected.number = 17;
            Assert(ConfigService.Instance.Save(), "Save should succeed");

            c.selected.room = "x";
            c.selected.number = 1;
            ConfigService.Instance.Load();

            AreEqual("ຫ້ອງ ທົດສອບ", ConfigService.Instance.Current.selected.room, "room");
            AreEqual(17, ConfigService.Instance.Current.selected.number, "number");
        }

        // ---------------------------------------------------------------- file names

        private static void FileName_InvalidCharsReplaced()
        {
            string s = ReportService.SanitizeFileName("a\\b/c:d*e?f\"g<h>i|j");
            foreach (char bad in Path.GetInvalidFileNameChars())
                Assert(s.IndexOf(bad) < 0, "still contains invalid char code " + (int)bad);
            Assert(s.StartsWith("a") && s.EndsWith("j"), "valid chars should remain");
        }

        private static void FileName_LaoPreserved()
        {
            AreEqual("ຫ້ອງ A-05", ReportService.SanitizeFileName("ຫ້ອງ A-05"), "Lao name");
        }

        // ---------------------------------------------------------------- scan runner

        private class FakeModule : ICheckModule
        {
            public Func<CheckResult> OnCheck = () => new CheckResult(CheckStatus.Pass, "ok");
            public Func<CheckResult> OnFix = () => new CheckResult(CheckStatus.Pass, "fixed");
            public string Name;
            public bool Fixable;

            public string Id { get { return Name; } }
            public string DisplayNameKey { get { return Name; } }
            public bool CanFix { get { return Fixable; } }
            public CheckResult Check() { return OnCheck(); }
            public CheckResult Fix() { return OnFix(); }
        }

        private static T Wait<T>(Task<T> t)
        {
            if (!t.Wait(TimeSpan.FromSeconds(15))) throw new Exception("timeout waiting for runner");
            return t.Result;
        }

        private static void ScanRunner_ModuleThrows_NotStuck()
        {
            var bad = new FakeModule { Name = "bad", OnCheck = () => { throw new InvalidOperationException("boom"); } };
            var good = new FakeModule { Name = "good" };
            var runner = new ScanRunner();

            var res = Wait(runner.RunCheckAsync(new List<ICheckModule> { bad, good }));

            AreEqual(CheckStatus.Unknown, res[bad].Status, "throwing module -> Unknown");
            AreEqual(CheckStatus.Pass, res[good].Status, "later module still runs");
            Assert(!runner.IsRunning, "IsRunning should be false afterwards");

            res = Wait(runner.RunCheckAsync(new List<ICheckModule> { good }));
            AreEqual(1, res.Count, "second run works");
        }

        private static void ScanRunner_HandlerThrows_NotStuck()
        {
            var good = new FakeModule { Name = "good" };
            var runner = new ScanRunner();
            runner.ModuleCompleted += (m, r, i, n) => { throw new Exception("ui handler bug"); };
            runner.AllCompleted += d => { throw new Exception("ui handler bug 2"); };

            var res = Wait(runner.RunCheckAsync(new List<ICheckModule> { good }));

            AreEqual(1, res.Count, "results still returned");
            Assert(!runner.IsRunning, "IsRunning should be false afterwards");
        }

        private static void ScanRunner_ConcurrentRun_Rejected()
        {
            var gate = new System.Threading.ManualResetEventSlim(false);
            var slow = new FakeModule { Name = "slow", OnCheck = () => { gate.Wait(5000); return new CheckResult(CheckStatus.Pass, "ok"); } };
            var runner = new ScanRunner();

            var first = runner.RunCheckAsync(new List<ICheckModule> { slow });
            Assert(runner.IsRunning, "should be running");

            // RunAsync เป็น async method → exception อยู่ใน Task (ຖືກໂຍນເມື່ອ await)
            var second = runner.RunCheckAsync(new List<ICheckModule> { slow });
            bool rejected = false;
            try { second.Wait(TimeSpan.FromSeconds(5)); }
            catch (AggregateException ae) { rejected = ae.InnerException is InvalidOperationException; }
            Assert(rejected, "second concurrent run must fault with InvalidOperationException");
            Assert(runner.IsRunning, "first run must still be running after rejected second run");

            gate.Set();
            Wait(first);
            Assert(!runner.IsRunning, "IsRunning reset after first completes");
        }

        private static void ScanRunner_FixProgress_OnlyFixable()
        {
            var a = new FakeModule { Name = "a", Fixable = true };
            var b = new FakeModule { Name = "b", Fixable = false };
            var c = new FakeModule { Name = "c", Fixable = true };
            var runner = new ScanRunner();
            var started = new List<string>();
            runner.ModuleStarted += (m, i, n) => { lock (started) started.Add(m.Id + ":" + i + "/" + n); };

            var res = Wait(runner.RunFixAsync(new List<ICheckModule> { a, b, c }));

            AreEqual(2, res.Count, "only fixable modules in results");
            AreEqual("a:1/2,c:2/2", string.Join(",", started), "progress sequence");
        }

        // ---------------------------------------------------------------- xlsx

        private static void Xlsx_RoundTrip()
        {
            string path = Path.Combine(Path.GetTempPath(), "cc_test_" + Guid.NewGuid().ToString("N") + ".xlsx");
            try
            {
                var sheet = new XlsxSheet("Machine Spec");
                sheet.AddHeader("Category", "Item", "Value");
                sheet.AddRow("General", "Room", "ຫ້ອງ A & <B>");
                sheet.AddRow("General", "Computer Number", "07");
                sheet.AddRow("General", "Hostname", "PC-07");
                sheet.AddRow("System & BIOS", "Serial Number", "SN123");

                var w = new SimpleXlsxWriter();
                w.AddSheet(sheet);
                w.Save(path);

                var s = MasterSummaryService.ParseSingleMachineXlsx(path);

                AreEqual("ຫ້ອງ A & <B>", s.Room, "room (Lao + XML special chars)");
                AreEqual(7, s.Number, "number");
                AreEqual("PC-07", s.Hostname, "hostname");
                AreEqual("SN123", s.Serial, "serial");
            }
            finally
            {
                try { File.Delete(path); } catch { }
            }
        }

        // ---------------------------------------------------------------- log rotation

        private static string TempLog()
        {
            return Path.Combine(Path.GetTempPath(), "cc_log_" + Guid.NewGuid().ToString("N") + ".log");
        }

        private static void Log_RotatesWhenTooLarge()
        {
            string log = TempLog();
            try
            {
                File.WriteAllText(log + ".old", "stale");
                File.WriteAllBytes(log, new byte[ReportService.MaxLogBytes + 10]);

                ReportService.RotateIfTooLarge(log);

                Assert(!File.Exists(log), "original log should be moved away");
                Assert(File.Exists(log + ".old"), ".old should exist");
                Assert(new FileInfo(log + ".old").Length > ReportService.MaxLogBytes, "stale .old should be overwritten by the big log");
            }
            finally
            {
                try { File.Delete(log); File.Delete(log + ".old"); } catch { }
            }
        }

        private static void Log_SmallLogNotRotated()
        {
            string log = TempLog();
            try
            {
                File.WriteAllText(log, "small");
                ReportService.RotateIfTooLarge(log);
                Assert(File.Exists(log), "small log must stay");
                Assert(!File.Exists(log + ".old"), "no .old for small log");

                ReportService.RotateIfTooLarge(log + ".missing"); // ຕ້ອງບໍ່ໂຍນ exception
            }
            finally
            {
                try { File.Delete(log); } catch { }
            }
        }

        // ---------------------------------------------------------------- report

        private static void Report_ContainsContent()
        {
            var results = new Dictionary<ICheckModule, CheckResult>();
            var m = new FakeModule { Name = "module_power" };
            results[m] = new CheckResult(CheckStatus.Fail, "summary-xyz", new List<string> { "detail-abc" });

            string text = ReportService.BuildReport(results, "PASSED 0/1");

            Assert(text.Contains("[FAIL]"), "status tag");
            Assert(text.Contains("summary-xyz"), "summary");
            Assert(text.Contains("detail-abc"), "details");
            Assert(text.Contains("PASSED 0/1"), "summary line");
            Assert(text.Contains(Environment.MachineName), "machine name");
        }

        // ---------------------------------------------------------------- disk & duplicate spec & quick setup

        private static void DiskSpace_ThresholdEvaluation()
        {
            AreEqual(CheckStatus.Fail, DiskSpaceCheckModule.EvaluateThreshold(0.0), "0% free");
            AreEqual(CheckStatus.Fail, DiskSpaceCheckModule.EvaluateThreshold(4.9), "4.9% free");
            AreEqual(CheckStatus.Warning, DiskSpaceCheckModule.EvaluateThreshold(5.0), "5.0% free");
            AreEqual(CheckStatus.Warning, DiskSpaceCheckModule.EvaluateThreshold(9.9), "9.9% free");
            AreEqual(CheckStatus.Pass, DiskSpaceCheckModule.EvaluateThreshold(10.0), "10.0% free");
            AreEqual(CheckStatus.Pass, DiskSpaceCheckModule.EvaluateThreshold(45.5), "45.5% free");
        }

        private static void SpecExport_IsKnownSerial()
        {
            Assert(!SpecExportModule.IsKnownSerial(null), "null serial");
            Assert(!SpecExportModule.IsKnownSerial(""), "empty serial");
            Assert(!SpecExportModule.IsKnownSerial("   "), "whitespace serial");
            Assert(!SpecExportModule.IsKnownSerial("-"), "dash serial");
            Assert(!SpecExportModule.IsKnownSerial("Unknown"), "Unknown serial");
            Assert(!SpecExportModule.IsKnownSerial("unknown"), "lowercase unknown serial");
            Assert(!SpecExportModule.IsKnownSerial("To Be Filled By O.E.M."), "OEM placeholder serial");
            Assert(!SpecExportModule.IsKnownSerial("Default string"), "Default string serial");

            Assert(SpecExportModule.IsKnownSerial("PF123456"), "valid Lenovo serial");
            Assert(SpecExportModule.IsKnownSerial("  CND12345AB  "), "valid serial with spaces");
        }

        private static void SpecExport_IsDifferentMachine_KnownSerial()
        {
            // ທັງສອງມີ Serial ທີ່ຖືກຕ້ອງ: ຖ້າ Serial ດຽວກັນ -> ເຄື່ອງດຽວກັນ (ແມ່ນແຕ່ປ່ຽນ Hostname)
            Assert(!SpecExportModule.IsDifferentMachine("PC-OLD", "SN100", "PC-NEW", "SN100"),
                "same serial should be considered same machine even if hostname changed");

            // ຖ້າ Serial ຕ່າງກັນ -> ຄົນລະເຄື່ອງ (ແມ່ນແຕ່ Hostname ດຽວກັນ)
            Assert(SpecExportModule.IsDifferentMachine("PC-01", "SN100", "PC-01", "SN200"),
                "different serial should be considered different machine even if hostname is same");
        }

        private static void SpecExport_IsDifferentMachine_FallbackToHostname()
        {
            // ຖ້າ Serial ບໍ່ຮູ້ຈັກ (OEM placeholder) -> ປຽບທຽບດ້ວຍ Hostname
            Assert(!SpecExportModule.IsDifferentMachine("PC-01", "Default string", "PC-01", "To Be Filled By O.E.M."),
                "unknown serials with same hostname should be considered same machine");

            Assert(SpecExportModule.IsDifferentMachine("PC-01", "To Be Filled By O.E.M.", "PC-02", "To Be Filled By O.E.M."),
                "unknown serials with different hostname should be considered different machine");
        }

        private static void QuickSetup_CalculateNextNumber()
        {
            bool isMax;

            int n1 = QuickSetupRunner.CalculateNextNumber(1, 45, out isMax);
            AreEqual(2, n1, "1 -> 2");
            Assert(!isMax, "1 is not max");

            int n44 = QuickSetupRunner.CalculateNextNumber(44, 45, out isMax);
            AreEqual(45, n44, "44 -> 45");
            Assert(!isMax, "44 is not max");

            int n45 = QuickSetupRunner.CalculateNextNumber(45, 45, out isMax);
            AreEqual(45, n45, "45 stays 45");
            Assert(isMax, "45 is max");

            int n50 = QuickSetupRunner.CalculateNextNumber(50, 45, out isMax);
            AreEqual(45, n50, "50 clamped to 45");
            Assert(isMax, "50 is max");
        }

        private static void I18n_BtnSaveReport_Localized()
        {
            Localizer.SetLanguage("lo");
            string loVal = Localizer.T("btn_save_report");
            Assert(!string.IsNullOrEmpty(loVal) && loVal != "btn_save_report", "btn_save_report in Lao should not be key name");
            AreEqual("ບັນທຶກລາຍງານ (.txt)", loVal, "Lao translation");

            Localizer.SetLanguage("en");
            string enVal = Localizer.T("btn_save_report");
            Assert(!string.IsNullOrEmpty(enVal) && enVal != "btn_save_report", "btn_save_report in English should not be key name");
            AreEqual("Save Report (.txt)", enVal, "English translation");

            // Restore default
            Localizer.SetLanguage("lo");
        }

        private static void I18n_SettingsKeys_Localized()
        {
            Localizer.SetLanguage("lo");
            string loBtn = Localizer.T("btn_settings");
            string loTitle = Localizer.T("settings_title");
            string loTabSec = Localizer.T("tab_security");
            Assert(!string.IsNullOrEmpty(loBtn) && loBtn != "btn_settings", "btn_settings in Lao");
            Assert(!string.IsNullOrEmpty(loTitle) && loTitle != "settings_title", "settings_title in Lao");
            Assert(!string.IsNullOrEmpty(loTabSec) && loTabSec != "tab_security", "tab_security in Lao");

            Localizer.SetLanguage("en");
            string enBtn = Localizer.T("btn_settings");
            string enTitle = Localizer.T("settings_title");
            string enTabSec = Localizer.T("tab_security");
            Assert(!string.IsNullOrEmpty(enBtn) && enBtn != "btn_settings", "btn_settings in English");
            Assert(!string.IsNullOrEmpty(enTitle) && enTitle != "settings_title", "settings_title in English");
            Assert(!string.IsNullOrEmpty(enTabSec) && enTabSec != "tab_security", "tab_security in English");

            Localizer.SetLanguage("lo");
        }

        private static void SecurityConfig_Defaults()
        {
            var sec = new SecurityConfig();
            Assert(sec.blockUsbExecution, "blockUsbExecution default true");
            Assert(sec.blockDownloadsExecution, "blockDownloadsExecution default true");
            Assert(sec.blockPopularGames, "blockPopularGames default true");
            Assert(sec.blockedExes != null && sec.blockedExes.Count >= 5, "blockedExes contains popular games");
            Assert(sec.blockedExes.Contains("roblox.exe"), "contains roblox.exe");
            Assert(sec.blockedExes.Contains("steam.exe"), "contains steam.exe");
        }

        private static void AppLockModule_MetadataAndContract()
        {
            var mod = new AppLockModule();
            AreEqual("app_lock", mod.Id, "Module Id");
            AreEqual("module_app_lock", mod.DisplayNameKey, "DisplayNameKey");
            Assert(mod.CanFix, "CanFix should be true");

            var res = mod.Check();
            Assert(res != null, "CheckResult should not be null");
            Assert(res.Status == CheckStatus.Pass || res.Status == CheckStatus.Warning || res.Status == CheckStatus.Unknown, "Valid status");
            Assert(res.Details != null && res.Details.Count > 0, "Details populated");
        }

        private static void I18n_TabInstall_Localized()
        {
            Localizer.SetLanguage("lo");
            string loTab = Localizer.T("tab_install");
            string loTitle = Localizer.T("installer_title");
            Assert(!string.IsNullOrEmpty(loTab) && loTab != "tab_install", "tab_install in Lao");
            Assert(!string.IsNullOrEmpty(loTitle) && loTitle != "installer_title", "installer_title in Lao");

            Localizer.SetLanguage("en");
            string enTab = Localizer.T("tab_install");
            string enTitle = Localizer.T("installer_title");
            Assert(!string.IsNullOrEmpty(enTab) && enTab != "tab_install", "tab_install in English");
            Assert(!string.IsNullOrEmpty(enTitle) && enTitle != "installer_title", "installer_title in English");

            Localizer.SetLanguage("lo");
        }

        private static void SoftwareInstaller_Catalog_PopulatedAndValid()
        {
            var catalog = SoftwareInstallerService.GetCatalog();
            Assert(catalog != null && catalog.Count == 11, "catalog has 11 packages");

            bool has7z = false;
            bool hasVlc = false;
            bool hasLaoScript = false;
            bool hasSumatra = false;
            bool hasBrave = false;
            bool hasShareX = false;
            bool hasRamMap = false;

            foreach (var pkg in catalog)
            {
                Assert(!string.IsNullOrEmpty(pkg.Id), "pkg Id non-empty");
                Assert(!string.IsNullOrEmpty(pkg.Name), "pkg Name non-empty");
                Assert(!string.IsNullOrEmpty(pkg.Category), "pkg Category non-empty");
                Assert(!string.IsNullOrEmpty(pkg.SilentArguments), "pkg SilentArguments non-empty");
                Assert(pkg.InstallerFilePatterns != null && pkg.InstallerFilePatterns.Length > 0, "pkg Patterns non-empty");

                if (pkg.Id == "7zip") has7z = true;
                if (pkg.Id == "vlc") hasVlc = true;
                if (pkg.Id == "laoscript8") hasLaoScript = true;
                if (pkg.Id == "sumatrapdf") hasSumatra = true;
                if (pkg.Id == "brave") hasBrave = true;
                if (pkg.Id == "sharex") hasShareX = true;
                if (pkg.Id == "rammap64") hasRamMap = true;
            }

            Assert(has7z, "contains 7-Zip");
            Assert(hasVlc, "contains VLC");
            Assert(hasLaoScript, "contains LaoScript 8");
            Assert(hasSumatra, "contains SumatraPDF");
            Assert(hasBrave, "contains Brave Browser");
            Assert(hasShareX, "contains ShareX");
            Assert(hasRamMap, "contains RAMMap64");
        }

        private static void SoftwareInstaller_CreateShortcut()
        {
            string tmpDir = Path.Combine(Path.GetTempPath(), "cc_sc_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmpDir);
            try
            {
                string targetPath = @"C:\Windows\notepad.exe";
                string shortcutPath = Path.Combine(tmpDir, "TestShortcut.lnk");

                bool created = SoftwareInstallerService.CreateShortcut(shortcutPath, targetPath, "Test shortcut description");
                Assert(created, "CreateShortcut returns true");
                Assert(File.Exists(shortcutPath), "shortcut file exists on disk");
                Assert(new FileInfo(shortcutPath).Length > 0, "shortcut file has non-zero size");
            }
            finally
            {
                try { Directory.Delete(tmpDir, true); } catch { }
            }
        }

        private static void SoftwareInstaller_RamMapIsPortable()
        {
            var catalog = SoftwareInstallerService.GetCatalog();
            var rammap = catalog.Find(p => p.Id == "rammap64");
            Assert(rammap != null, "rammap found in catalog");
            Assert(rammap.IsPortable, "rammap is marked IsPortable");
            AreEqual("RAMMap", rammap.PortableTargetSubDir, "PortableTargetSubDir is RAMMap");
            Assert(!string.IsNullOrEmpty(rammap.ShortcutName), "ShortcutName is specified");
        }

        private static void SecurityStatusInfo_LockStateEvaluation()
        {
            var status = new SecurityStatusInfo();
            Assert(!status.HasAnyLockActive, "fresh status has no locks");
            Assert(!status.IsFullyLockedStandard, "fresh status is not fully locked standard");

            var cfg = new SecurityConfig
            {
                blockUsbExecution = true,
                blockDownloadsExecution = true,
                blockPopularGames = true,
                blockedExes = new List<string> { "game.exe" }
            };

            Assert(!status.IsFullyLockedForConfig(cfg), "unlocked status does not match full config");

            // Partially locked: USB only
            status.UsbBlocked = true;
            Assert(status.HasAnyLockActive, "USB only is recognized as any lock active");
            Assert(!status.IsFullyLockedForConfig(cfg), "USB only does not satisfy full config");

            // Partially locked: USB + Downloads
            status.DownloadsBlocked = true;
            Assert(!status.IsFullyLockedForConfig(cfg), "USB + Downloads does not satisfy full config");

            // Fully locked: USB + Downloads + Games with > 0 exes
            status.DisallowRunActive = true;
            status.DisallowedExesCount = 1;
            Assert(status.HasAnyLockActive, "all active is any active");
            Assert(status.IsFullyLockedStandard, "standard full lock satisfied");
            Assert(status.IsFullyLockedForConfig(cfg), "full config satisfied");

            // If config only wants USB, USB alone satisfies it
            var usbOnlyCfg = new SecurityConfig { blockUsbExecution = true, blockDownloadsExecution = false, blockPopularGames = false };
            var usbOnlyStatus = new SecurityStatusInfo { UsbBlocked = true };
            Assert(usbOnlyStatus.IsFullyLockedForConfig(usbOnlyCfg), "USB only status satisfies USB only config");

            // If config wants nothing, IsFullyLockedForConfig returns false
            var emptyCfg = new SecurityConfig { blockUsbExecution = false, blockDownloadsExecution = false, blockPopularGames = false };
            Assert(!usbOnlyStatus.IsFullyLockedForConfig(emptyCfg), "Empty config cannot be considered locked");
        }

        private static void I18n_SecurityLockButtons_Localized()
        {
            Localizer.SetLanguage("lo");
            AreEqual("✓ ລັອກຮຽບຮ້ອຍ", Localizer.T("security_btn_locked"), "security_btn_locked in lo");
            AreEqual("✓ ບໍ່ໄດ້ລັອກ", Localizer.T("security_btn_unlocked"), "security_btn_unlocked in lo");

            Localizer.SetLanguage("en");
            AreEqual("✓ Already Locked", Localizer.T("security_btn_locked"), "security_btn_locked in en");
            AreEqual("✓ Unlocked", Localizer.T("security_btn_unlocked"), "security_btn_unlocked in en");
        }

        private static void DateTimeService_ConstantsAndContract()
        {
            AreEqual("SE Asia Standard Time", DateTimeService.TARGET_TIMEZONE_ID, "target timezone is SE Asia (+7)");
            AreEqual("dd/MM/yyyy", DateTimeService.TARGET_DATE_FORMAT, "target date format is dd/MM/yyyy");
            AreEqual("1", DateTimeService.TARGET_DATE_ORDER, "target date order is 1 (d/M/y)");

            var mod = new DateTimeCheckModule();
            AreEqual("datetime_format", mod.Id, "DateTimeCheckModule Id");
            AreEqual("module_datetime_format", mod.DisplayNameKey, "DateTimeCheckModule DisplayNameKey");
            Assert(mod.CanFix, "DateTimeCheckModule CanFix is true");

            var res = mod.Check();
            Assert(res != null, "CheckResult non-null");
            Assert(res.Status == CheckStatus.Pass || res.Status == CheckStatus.Warning || res.Status == CheckStatus.Unknown, "Valid CheckStatus");
        }

        private static void DesktopIconService_ConstantsAndContract()
        {
            AreEqual("{20D04FE0-3AEA-1069-A2D8-08002B30309D}", DesktopIconService.GUID_THIS_PC, "This PC GUID");
            AreEqual("{5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}", DesktopIconService.GUID_CONTROL_PANEL, "Control Panel GUID (Desktop icon settings)");

            var mod = new DesktopIconsModule();
            AreEqual("desktop_icons", mod.Id, "DesktopIconsModule Id");
            AreEqual("module_desktop_icons", mod.DisplayNameKey, "DesktopIconsModule DisplayNameKey");
            Assert(mod.CanFix, "DesktopIconsModule CanFix is true");

            var res = mod.Check();
            Assert(res != null, "CheckResult non-null");
            Assert(res.Status == CheckStatus.Pass || res.Status == CheckStatus.Warning || res.Status == CheckStatus.Unknown, "Valid CheckStatus");

            // Test CreateShortcutInternal with a temporary file
            string tmpDir = Path.Combine(Path.GetTempPath(), "cc_icon_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmpDir);
            try
            {
                string targetExe = @"C:\Windows\notepad.exe";
                string lnkPath = Path.Combine(tmpDir, "TestIcon.lnk");
                bool created = DesktopIconService.CreateShortcutInternal(lnkPath, targetExe, "Test Icon Description");
                Assert(created, "CreateShortcutInternal succeeded");
                Assert(File.Exists(lnkPath), "TestIcon.lnk exists on disk");
            }
            finally
            {
                try { Directory.Delete(tmpDir, true); } catch { }
            }
        }

        private static void UsbVirusCleanerService_CleanAndRestore_Contract()
        {
            var mod = new UsbVaccineModule();
            AreEqual("usb_vaccine", mod.Id, "UsbVaccineModule Id");
            AreEqual("module_usb_vaccine", mod.DisplayNameKey, "UsbVaccineModule DisplayNameKey");
            Assert(mod.CanFix, "UsbVaccineModule CanFix is true");

            var checkRes = mod.Check();
            Assert(checkRes != null, "UsbVaccineModule CheckResult non-null");

            // Test CleanAndRestoreDrive in isolated temp directory
            string testDir = Path.Combine(Path.GetTempPath(), "cc_usb_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(testDir);

            try
            {
                // 1. Legitimate directory and file (hidden by malware)
                string realFolder = Path.Combine(testDir, "Documents");
                Directory.CreateDirectory(realFolder);
                new DirectoryInfo(realFolder).Attributes = FileAttributes.Directory | FileAttributes.Hidden | FileAttributes.System;

                string realFile = Path.Combine(realFolder, "Assignment.docx");
                File.WriteAllText(realFile, "Real user document");
                File.SetAttributes(realFile, FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly);

                // 2. Rogue autorun.inf file
                string badAutorun = Path.Combine(testDir, "autorun.inf");
                File.WriteAllText(badAutorun, "[autorun]\r\nopen=wscript.exe helper.vbs");

                // 3. Rogue script
                string badScript = Path.Combine(testDir, "helper.vbs");
                File.WriteAllText(badScript, "WScript.Echo \"malware\"");

                // 4. Fake folder exe (Documents.exe matching folder Documents)
                string fakeExe = Path.Combine(testDir, "Documents.exe");
                File.WriteAllText(fakeExe, "fake pe header");

                // Run cleaning
                var options = new UsbCleanOptions
                {
                    TargetPath = testDir,
                    KillMalwareProcesses = false, // Don't kill external processes in unit test
                    CleanStartupPersistence = false,
                    RemoveShortcuts = true,
                    RemoveRogueScripts = true,
                    DetectFakeFolderExes = true,
                    UnhideLegitimateFiles = true,
                    CreateUsbVaccine = true,
                    ImmunizeHostAutoRun = false
                };

                var cleanResult = UsbVirusCleanerService.CleanAndRestoreDrive(options);

                Assert(cleanResult.Success, "CleanAndRestoreDrive succeeded");
                Assert(cleanResult.FilesUnhiddenCount >= 1, "Unhid at least 1 file");
                Assert(cleanResult.DirectoriesUnhiddenCount >= 1, "Unhid at least 1 directory");
                Assert(cleanResult.RogueScriptsRemovedCount >= 2, "Removed rogue autorun.inf and helper.vbs");
                Assert(cleanResult.FakeExesRemovedCount >= 1, "Removed fake folder exe");
                Assert(cleanResult.VaccineCreated, "Vaccine directory created");

                // Check file states
                Assert(!File.Exists(badScript), "helper.vbs deleted");
                Assert(!File.Exists(fakeExe), "Documents.exe fake exe deleted");
                Assert(Directory.Exists(realFolder), "Real folder preserved");
                Assert(File.Exists(realFile), "Real file preserved");

                // Verify attributes unhidden
                var dirAttr = new DirectoryInfo(realFolder).Attributes;
                Assert((dirAttr & (FileAttributes.Hidden | FileAttributes.System)) == 0, "Folder is unhidden");

                var fileAttr = File.GetAttributes(realFile);
                Assert((fileAttr & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly)) == 0, "File is unhidden");

                // Verify vaccine folder exists (as a directory, not a file)
                string vaccineDir = Path.Combine(testDir, UsbVirusCleanerService.VACCINE_FOLDER_NAME);
                Assert(Directory.Exists(vaccineDir), "autorun.inf is now a vaccine directory");
                Assert(Directory.Exists(Path.Combine(vaccineDir, UsbVirusCleanerService.VACCINE_SUBFOLDER_NAME)), "Vaccine subfolder exists");
            }
            finally
            {
                try
                {
                    // Vaccine folder has ReadOnly/System attributes, strip them before delete
                    string vaccineDir = Path.Combine(testDir, UsbVirusCleanerService.VACCINE_FOLDER_NAME);
                    if (Directory.Exists(vaccineDir))
                    {
                        foreach (var f in Directory.GetFiles(vaccineDir, "*", SearchOption.AllDirectories))
                        {
                            try { File.SetAttributes(f, FileAttributes.Normal); } catch { }
                        }
                        foreach (var d in Directory.GetDirectories(vaccineDir, "*", SearchOption.AllDirectories))
                        {
                            try { new DirectoryInfo(d).Attributes = FileAttributes.Directory; } catch { }
                        }
                        new DirectoryInfo(vaccineDir).Attributes = FileAttributes.Directory;
                    }
                    Directory.Delete(testDir, true);
                }
                catch { }
            }
        }

        private static void I18n_UsbCleaner_Localized()
        {
            Localizer.SetLanguage("lo");
            Assert(!string.IsNullOrEmpty(Localizer.T("module_usb_vaccine")), "lo: module_usb_vaccine non-empty");
            Assert(!string.IsNullOrEmpty(Localizer.T("quick_setup_step_usb_vaccine")), "lo: quick_setup_step_usb_vaccine non-empty");
            Assert(!string.IsNullOrEmpty(Localizer.T("usb_rescue_title")), "lo: usb_rescue_title non-empty");
            Assert(!string.IsNullOrEmpty(Localizer.T("usb_cleaner_dialog_title")), "lo: usb_cleaner_dialog_title non-empty");
            Assert(!string.IsNullOrEmpty(Localizer.T("usb_cleaner_btn_start")), "lo: usb_cleaner_btn_start non-empty");

            Localizer.SetLanguage("en");
            Assert(!string.IsNullOrEmpty(Localizer.T("module_usb_vaccine")), "en: module_usb_vaccine non-empty");
            Assert(!string.IsNullOrEmpty(Localizer.T("quick_setup_step_usb_vaccine")), "en: quick_setup_step_usb_vaccine non-empty");
            Assert(!string.IsNullOrEmpty(Localizer.T("usb_rescue_title")), "en: usb_rescue_title non-empty");
            Assert(!string.IsNullOrEmpty(Localizer.T("usb_cleaner_dialog_title")), "en: usb_cleaner_dialog_title non-empty");
            Assert(!string.IsNullOrEmpty(Localizer.T("usb_cleaner_btn_start")), "en: usb_cleaner_btn_start non-empty");

            Localizer.SetLanguage("lo"); // restore Lao
        }

        private static void ComputerNameService_Contract()
        {
            // Generation
            string name1 = ComputerNameService.GenerateDefaultName("ຫ້ອງຄອມພິວເຕີ 1", 1);
            Assert(name1 == "COM1-01", "room 1 -> COM1-01 (got " + name1 + ")");

            string name2 = ComputerNameService.GenerateDefaultName("ຫ້ອງຄອມພິວເຕີ 2", 9);
            Assert(name2 == "COM2-09", "room 2 -> COM2-09 (got " + name2 + ")");

            string name10 = ComputerNameService.GenerateDefaultName("ຫ້ອງຄອມພິວເຕີ 1", 10);
            Assert(name10 == "COM1-10", "pc 10 -> COM1-10 (got " + name10 + ")");

            // Sanitization
            string clean1 = ComputerNameService.SanitizeName("lab-pc 01!");
            Assert(clean1 == "LAB-PC-01", "Sanitize removes special chars and spaces (got " + clean1 + ")");

            string clean2 = ComputerNameService.SanitizeName("---pc---01---");
            Assert(clean2 == "PC-01", "Sanitize trims hyphens (got " + clean2 + ")");

            // Validation
            string err;
            Assert(ComputerNameService.ValidateName("COM1-01", out err), "COM1-01 is valid");
            Assert(ComputerNameService.ValidateName("LAB-PC-05", out err), "LAB-PC-05 is valid");
            Assert(!ComputerNameService.ValidateName("", out err), "empty name invalid");
            Assert(!ComputerNameService.ValidateName("TOOLONGNAMEEXCEEDING15CHARS", out err), ">15 chars invalid");
            Assert(!ComputerNameService.ValidateName("PC 01", out err), "space invalid");
            Assert(!ComputerNameService.ValidateName("12345", out err), "numbers only invalid");
            Assert(!ComputerNameService.ValidateName("-PC01", out err), "starting with hyphen invalid");
        }

        private static void ComputerNameModule_MetadataAndContract()
        {
            var mod = new ComputerNameModule();
            Assert(mod.Id == "computer_name", "Id is computer_name");
            Assert(mod.DisplayNameKey == "module_computer_name", "DisplayNameKey is module_computer_name");
            Assert(mod.CanFix, "CanFix is true");

            var res = mod.Check();
            Assert(res != null, "CheckResult non-null");
            Assert(res.Status == CheckStatus.Pass || res.Status == CheckStatus.Warning, "Valid status");
            Assert(res.Details != null && res.Details.Count >= 2, "Details contains hostname info");
        }

        private static void LogonScreen_ConfigAndNotice()
        {
            var cfg = new AppConfig();
            Assert(cfg.logonScreen != null, "logonScreen section exists by default");
            Assert(cfg.logonScreen.setLockScreen && cfg.logonScreen.disableLogonBlur && cfg.logonScreen.preventWallpaperChange,
                "Lock screen, no-blur and wallpaper lock on by default");
            Assert(!cfg.logonScreen.showLogonNotice, "Logon notice off by default (needs teacher's text)");
            Assert(!string.IsNullOrEmpty(cfg.logonScreen.noticeText), "Default notice text provided");

            Assert(LogonScreenService.NormalizeNoticeText("a\nb\r\nc\rd") == "a\r\nb\r\nc\r\nd", "Line breaks normalized to CRLF");
            Assert(LogonScreenService.NormalizeNoticeText(null) == "", "Null text -> empty");
            Assert(LogonScreenService.NormalizeNoticeText("  ກົດລະບຽບ \n") == "ກົດລະບຽບ", "Lao text kept and trimmed");
        }

        private static void UserAccount_ValidationAndDefaults()
        {
            string err;
            Assert(UserAccountService.ValidateUserName("COM1-01", out err), "COM1-01 is a valid user name");
            Assert(!UserAccountService.ValidateUserName("", out err), "Empty rejected");
            Assert(!UserAccountService.ValidateUserName("ABCDEFGHIJKLMNOPQRSTU", out err), "21 chars rejected");
            Assert(!UserAccountService.ValidateUserName("a/b", out err), "Slash rejected");
            Assert(!UserAccountService.ValidateUserName("a@b", out err), "@ rejected");
            Assert(!UserAccountService.ValidateUserName("...", out err), "Dots only rejected");

            string domain, user;
            UserAccountService.SplitAccount(@"PC01\kitti", out domain, out user);
            Assert(domain == "PC01" && user == "kitti", "Split DOMAIN\\user");
            UserAccountService.SplitAccount("kitti", out domain, out user);
            Assert(domain == "" && user == "kitti", "Split bare user");

            Assert(new AppConfig().renameUserWithComputer, "Rename user with computer on by default");
            Assert(new AppConfig().userNamePrefix == "USER", "Default user prefix is USER");
            Assert(UserAccountService.BuildUserName("USER", "COM1-01") == "USER-COM1-01", "USER prefix");
            Assert(UserAccountService.BuildUserName("LTVC", "COM1-01") == "LTVC-COM1-01", "Custom prefix LTVC");
            Assert(UserAccountService.BuildUserName("LTVC-", "COM1-01") == "LTVC-COM1-01", "Trailing dash not doubled");
            Assert(UserAccountService.BuildUserName("", "COM1-01") == "COM1-01", "Empty prefix -> same as PC name");
            Assert(UserAccountService.BuildUserName(null, "COM1-01") == "COM1-01", "Null prefix -> same as PC name");
            Localizer.SetLanguage("en");
            Assert(Localizer.T("settings_rename_user_with_computer") != "settings_rename_user_with_computer", "en label exists");
            Localizer.SetLanguage("lo");
            Assert(Localizer.T("settings_rename_user_with_computer") != "settings_rename_user_with_computer", "lo label exists");
        }

        private static void DateTime_FormatsAndPreview()
        {
            var cfg = new AppConfig();
            Assert(cfg.dateTime != null && cfg.dateTime.dateFormat == "dd/MM/yyyy" && cfg.dateTime.use24Hour,
                "Default: dd/MM/yyyy and 24-hour");

            AreEqual("HH:mm", DateTimeService.GetShortTimePattern(true), "24h short time");
            AreEqual("h:mm tt", DateTimeService.GetShortTimePattern(false), "12h short time");

            var t = new DateTime(2026, 11, 16, 14, 5, 9);
            AreEqual("16/11/2026 14:05:09", DateTimeService.FormatPreview(t, "dd/MM/yyyy", true), "24h preview");
            AreEqual("16/11/2026 2:05:09 PM", DateTimeService.FormatPreview(t, "dd/MM/yyyy", false), "12h preview");
            AreEqual("2026-11-16 14:05:09", DateTimeService.FormatPreview(t, "yyyy-MM-dd", true), "ISO date preview");
            AreEqual("16/11/2026 14:05:09", DateTimeService.FormatPreview(t, "bogus", true), "Unsupported format falls back");

            AreEqual("/", DateTimeService.GetDateSeparator("dd/MM/yyyy"), "Separator /");
            AreEqual("-", DateTimeService.GetDateSeparator("yyyy-MM-dd"), "Separator -");
            AreEqual("1", DateTimeService.GetDateOrder("d/M/yyyy"), "Order d/M/y = 1");
            AreEqual("2", DateTimeService.GetDateOrder("yyyy-MM-dd"), "Order y/M/d = 2");

            Assert(DateTimeService.IsSupportedDateFormat("dd-MM-yyyy"), "dd-MM-yyyy supported");
            Assert(!DateTimeService.IsSupportedDateFormat("MM/dd/yyyy"), "US format not offered");
        }

        private static void DesktopIcons_OfficeShortcutDetection()
        {
            var word = DesktopIconService.OfficeApps[0];
            var ppt = DesktopIconService.OfficeApps[2];
            Assert(DesktopIconService.IsOfficeShortcutName("Word", word), "'Word' matches");
            Assert(DesktopIconService.IsOfficeShortcutName("Word 2016", word), "'Word 2016' matches");
            Assert(DesktopIconService.IsOfficeShortcutName("Microsoft Word", word), "'Microsoft Word' matches");
            Assert(!DesktopIconService.IsOfficeShortcutName("WordPad", word), "'WordPad' does not match");
            Assert(!DesktopIconService.IsOfficeShortcutName("Password Manager", word), "'Password' does not match");
            Assert(DesktopIconService.IsOfficeShortcutName("PowerPoint", ppt), "'PowerPoint' matches");

            string dir = Path.Combine(Path.GetTempPath(), "cc_office_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var dirs = new List<string> { dir };
                Assert(DesktopIconService.FindExistingOfficeShortcut(word, dirs) == null, "Empty desktop -> no Word shortcut");

                // shortcut ຊື່ອື່ນ ແຕ່ປາຍທາງເປັນ WINWORD.EXE → ຕ້ອງນັບວ່າມີແລ້ວ (ບໍ່ສ້າງຊ້ຳ)
                string custom = Path.Combine(dir, "My Documents Tool.lnk");
                Assert(DesktopIconService.CreateShortcutInternal(custom, @"C:\Program Files\Microsoft Office\root\Office16\WINWORD.EXE", "test"),
                    "Test shortcut created");
                Assert(DesktopIconService.FindExistingOfficeShortcut(word, dirs) == custom, "Detected by target exe");
                Assert(DesktopIconService.FindExistingOfficeShortcut(ppt, dirs) == null, "Word shortcut is not PowerPoint");
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { }
            }
        }

        private static void Performance_RulesAndDefaults()
        {
            var cfg = new AppConfig();
            Assert(cfg.performance != null && cfg.performance.mode == "auto" && cfg.performance.autoMaxRamGb == 4,
                "Default: auto for RAM <= 4GB");

            Assert(PerformanceTuningService.IsApplicable("auto", 4, 3.9), "auto: 3.9 GB applies");
            Assert(PerformanceTuningService.IsApplicable("auto", 4, 4.4), "auto: 4.4 GB applies (reported RAM varies)");
            Assert(!PerformanceTuningService.IsApplicable("auto", 4, 7.9), "auto: 8 GB does not apply");
            Assert(PerformanceTuningService.IsApplicable("on", 4, 16), "on: always");
            Assert(!PerformanceTuningService.IsApplicable("off", 4, 2), "off: never");

            Assert(PerformanceTuningService.GetServicesToDisable(SystemDiskType.Hdd).Contains("SysMain"), "HDD disables SysMain");
            Assert(!PerformanceTuningService.GetServicesToDisable(SystemDiskType.Ssd).Contains("SysMain"), "SSD keeps SysMain");
            var svcs = PerformanceTuningService.GetServicesToDisable(SystemDiskType.Hdd);
            foreach (string keep in new[] { "wuauserv", "WinDefend", "Spooler", "AudioSrv", "Dhcp" })
            {
                Assert(!svcs.Contains(keep), "Never disables " + keep);
            }

            Assert(PerformanceTuningService.IsUnwantedStartup("OneDrive", @"C:\x\OneDrive.exe /background"), "OneDrive is unwanted");
            Assert(PerformanceTuningService.IsUnwantedStartup("MicrosoftEdgeAutoLaunch_ABC", "msedge.exe"), "Edge autolaunch is unwanted");
            Assert(!PerformanceTuningService.IsUnwantedStartup("SecurityHealth", @"%windir%\system32\SecurityHealthSystray.exe"), "Defender tray kept");
            Assert(!PerformanceTuningService.IsUnwantedStartup("TeamViewer", "TeamViewer.exe"), "TeamViewer kept");

            Assert(PerformanceTuningService.IsStartupApprovedEnabled(null), "No StartupApproved value = enabled");
            Assert(PerformanceTuningService.IsStartupApprovedEnabled(new byte[] { 0x02, 0, 0 }), "02 = enabled");
            Assert(!PerformanceTuningService.IsStartupApprovedEnabled(new byte[] { 0x03, 0, 0 }), "03 = disabled");

            AreEqual("kBIDgA==", PerformanceTuningService.EncodeValue(new byte[] { 0x90, 0x12, 0x03, 0x80 }), "Binary backup is base64");

            var mod = new PerformanceModule();
            AreEqual("low_spec_performance", mod.Id, "PerformanceModule Id");
            Localizer.SetLanguage("en");
            Assert(Localizer.T(mod.DisplayNameKey) != mod.DisplayNameKey, "en module name exists");
            Localizer.SetLanguage("lo");
            Assert(Localizer.T(mod.DisplayNameKey) != mod.DisplayNameKey, "lo module name exists");
        }

        private static void LogonScreen_LockScreenFiles()
        {
            string name = LogonScreenService.BuildLockScreenFileName(new DateTime(2026, 11, 16, 9, 5, 7));
            Assert(name == "lockscreen_20261116_090507.png", "File name contains timestamp: " + name);

            string dir = Path.Combine(Path.GetTempPath(), "cc_lock_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                string oldFile = Path.Combine(dir, "lockscreen_20200101_000000.png");
                string keep = Path.Combine(dir, name);
                string wallpaper = Path.Combine(dir, "wallpaper.png");
                File.WriteAllText(oldFile, "x");
                File.WriteAllText(keep, "x");
                File.WriteAllText(wallpaper, "x");

                LogonScreenService.DeleteOldLockScreenFiles(dir, keep);

                Assert(!File.Exists(oldFile), "Old lock screen file deleted");
                Assert(File.Exists(keep), "Current lock screen file kept");
                Assert(File.Exists(wallpaper), "Wallpaper file untouched");
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { }
            }
        }

        private static void I18n_LogonScreen_Localized()
        {
            string[] keys = { "settings_tab_logon", "settings_set_lock_screen", "settings_disable_logon_blur",
                "settings_prevent_wallpaper_change", "settings_show_logon_notice", "settings_validation_notice_empty" };
            foreach (string lang in new[] { "lo", "en" })
            {
                Localizer.SetLanguage(lang);
                foreach (string k in keys)
                {
                    Assert(Localizer.T(k) != k, lang + " has " + k);
                }
            }
            Localizer.SetLanguage("lo");
        }

        private static void EssentialSoftware_RequiredList()
        {
            var ids = SoftwareInstallerService.GetEssentialPackages().Select(p => p.Id).ToList();
            Assert(ids.SequenceEqual(new[] { "sumatrapdf", "laoscript8", "vlc", "7zip" }),
                "Essential packages resolve to catalog entries: " + string.Join(",", ids));

            var mod = new EssentialSoftwareCheckModule();
            Assert(mod.Id == "essential_software", "Id unchanged (used by Master Summary)");
            var res = mod.Check();
            Assert(res.Status == CheckStatus.Pass || res.Status == CheckStatus.Fail, "Pass when all present, Fail when any missing");
            foreach (string name in new[] { "SumatraPDF", "LaoScript 8", "VLC", "7-Zip" })
            {
                Assert(res.Details.Any(d => d.Contains(name)), "Details mention " + name);
            }
        }
    }
}
