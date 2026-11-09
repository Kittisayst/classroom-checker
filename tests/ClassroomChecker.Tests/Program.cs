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
    }
}
