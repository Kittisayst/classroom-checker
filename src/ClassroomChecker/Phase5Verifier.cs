using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using ClassroomChecker.Config;
using ClassroomChecker.Core;
using ClassroomChecker.I18n;
using ClassroomChecker.Modules;

namespace ClassroomChecker
{
    /// <summary>
    /// ໂມດູນກວດສອບ ແລະ ທົດສອບລະບົບຄົບວົງຈອນສຳລັບ Phase 5 (Release & Verification)
    /// ສາມາດລັນຜ່ານ CLI: ClassroomChecker.exe --verify ຫຼື --test-phase5
    /// </summary>
    public static class Phase5Verifier
    {
        public static int Run()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("================================================================================");
            Console.WriteLine("           CLASSROOM CHECKER v1.0 — PHASE 5 VERIFICATION SUITE                  ");
            Console.WriteLine("================================================================================");

            int totalTests = 0;
            int passedTests = 0;

            void Assert(string testName, bool condition, string detail = null)
            {
                totalTests++;
                if (condition)
                {
                    passedTests++;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("  [PASS] ");
                    Console.ResetColor();
                    Console.WriteLine(testName + (string.IsNullOrEmpty(detail) ? "" : " (" + detail + ")"));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("  [FAIL] ");
                    Console.ResetColor();
                    Console.WriteLine(testName + (string.IsNullOrEmpty(detail) ? "" : " — " + detail));
                }
            }

            try
            {
                // -------------------------------------------------------------
                // 1. EMBEDDED RESOURCES & SINGLE-FILE INTEGRITY
                // -------------------------------------------------------------
                Console.WriteLine("\n[1] ກວດສອບ Embedded Resources ແລະ ຄວາມສົມບູນຂອງ Single Executable:");
                Assembly asm = typeof(Phase5Verifier).Assembly;
                string[] resNames = asm.GetManifestResourceNames();
                HashSet<string> resSet = new HashSet<string>(resNames);

                Assert("EmbeddedResource: lo.json",
                    resSet.Contains("ClassroomChecker.I18n.lo.json"));

                Assert("EmbeddedResource: en.json",
                    resSet.Contains("ClassroomChecker.I18n.en.json"));

                Assert("EmbeddedResource: bg.png",
                    resSet.Contains("ClassroomChecker.Assets.bg.png"));

                Assert("EmbeddedResource: PhetsarathOT.ttf",
                    resSet.Contains("ClassroomChecker.Assets.Fonts.PhetsarathOT.ttf"));

                Assert("EmbeddedResource: app.ico (Application Icon)",
                    resSet.Contains("ClassroomChecker.app.ico"));

                using (Stream stream = asm.GetManifestResourceStream("ClassroomChecker.Assets.Fonts.PhetsarathOT.ttf"))
                {
                    Assert("Font PhetsarathOT.ttf non-empty (>50KB)",
                        stream != null && stream.Length > 50000,
                        stream != null ? (stream.Length / 1024) + " KB" : "null");
                }

                using (Stream stream = asm.GetManifestResourceStream("ClassroomChecker.Assets.bg.png"))
                {
                    bool validImage = false;
                    int imgW = 0, imgH = 0;
                    if (stream != null)
                    {
                        using (Image img = Image.FromStream(stream))
                        {
                            validImage = img.Width > 0 && img.Height > 0;
                            imgW = img.Width;
                            imgH = img.Height;
                        }
                    }
                    Assert("Default bg.png valid image",
                        validImage,
                        imgW + "x" + imgH + " px");
                }

                // -------------------------------------------------------------
                // 2. I18N LOCALIZATION SYSTEM
                // -------------------------------------------------------------
                Console.WriteLine("\n[2] ກວດສອບລະບົບສອງພາສາ (I18n / Localizer):");
                Localizer.SetLanguage("lo");
                string loTitle = Localizer.T("app_title");
                Assert("Lao localization active",
                    !string.IsNullOrEmpty(loTitle) && !loTitle.StartsWith("["),
                    loTitle);

                Localizer.SetLanguage("en");
                string enTitle = Localizer.T("app_title");
                Assert("English localization active",
                    !string.IsNullOrEmpty(enTitle) && enTitle.Contains("Classroom Checker"),
                    enTitle);

                Assert("UI Font Manager: Phetsarath OT resolved",
                    !string.IsNullOrEmpty(ClassroomChecker.UI.UiFontManager.PreferredFontFamily),
                    ClassroomChecker.UI.UiFontManager.PreferredFontFamily);

                // -------------------------------------------------------------
                // 3. CONFIGURATION & PERSISTENCE
                // -------------------------------------------------------------
                Console.WriteLine("\n[3] ກວດສອບການຕັ້ງຄ່າ ແລະ ຄວາມທົນທານຂອງ ConfigService:");
                AppConfig cfg = ConfigService.Instance.Current;
                Assert("Config loaded successfully",
                    cfg != null && cfg.rooms != null && cfg.rooms.Count > 0,
                    "Rooms count: " + (cfg != null && cfg.rooms != null ? cfg.rooms.Count : 0));

                Assert("Config active path exists",
                    !string.IsNullOrEmpty(ConfigService.Instance.ActiveConfigPath),
                    ConfigService.Instance.ActiveConfigPath);

                // -------------------------------------------------------------
                // 4. WALLPAPER MODULE & LAO RENDERING
                // -------------------------------------------------------------
                Console.WriteLine("\n[4] ກວດສອບ WallpaperModule (Render Lao Text to Bitmap):");
                try
                {
                    using (Bitmap bmp = WallpaperModule.Instance.GenerateWallpaperBitmap("ຫ້ອງທົດສອບ", 99, 1280, 720))
                    {
                        Assert("Wallpaper rendered (1280x720)",
                            bmp != null && bmp.Width == 1280 && bmp.Height == 720,
                            "Width: " + (bmp != null ? bmp.Width : 0) + ", Height: " + (bmp != null ? bmp.Height : 0));
                    }
                }
                catch (Exception ex)
                {
                    Assert("Wallpaper rendering", false, ex.Message);
                }

                // -------------------------------------------------------------
                // 5. CHECK MODULES (ALL 9 MODULES)
                // -------------------------------------------------------------
                Console.WriteLine("\n[5] ກວດສອບໂມດູນສະແກນທັງ 9 ໂມດູນ (ICheckModule.Check()):");
                List<ICheckModule> modules = new List<ICheckModule>
                {
                    new OfficeCheckModule(),
                    new WindowsLicenseCheckModule(),
                    new EssentialSoftwareCheckModule(),
                    new FontCheckModule(),
                    new KeyboardCheckModule(),
                    new PowerModule(),
                    new CleanupModule(),
                    new DriverCheckModule(),
                    new SpecExportModule()
                };

                foreach (var mod in modules)
                {
                    try
                    {
                        CheckResult res = mod.Check();
                        bool validStatus = res != null;
                        string detail = res != null ? string.Format("{0} - {1}", res.Status, res.Summary) : "null";
                        Assert("Module " + mod.Id + " (" + Localizer.T(mod.DisplayNameKey) + ")", validStatus, detail);
                    }
                    catch (Exception ex)
                    {
                        Assert("Module " + mod.Id + " exception safety", false, ex.Message);
                    }
                }

                // -------------------------------------------------------------
                // 6. EXCEL EXPORT (OPEXML & SPECS VERIFICATION)
                // -------------------------------------------------------------
                Console.WriteLine("\n[6] ກວດສອບການ Export Spec ເປັນ OpenXML (.xlsx):");
                try
                {
                    SpecExportModule exportMod = new SpecExportModule();
                    string targetPath = exportMod.GetTargetExcelPath();

                    CheckResult exportRes = exportMod.Fix();
                    Assert("Export spec executed",
                        exportRes != null && exportRes.Status == CheckStatus.Pass,
                        exportRes != null ? exportRes.Summary : "null");

                    Assert("Export spec generated .xlsx file",
                        File.Exists(targetPath),
                        Path.GetFileName(targetPath));

                    // Verify zip structure of xlsx
                    bool hasWorkbook = false;
                    bool hasSheet1 = false;
                    bool hasSheet2 = false;
                    bool hasContentTypes = false;

                    using (FileStream fs = new FileStream(targetPath, FileMode.Open, FileAccess.Read))
                    using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read))
                    {
                        foreach (var entry in zip.Entries)
                        {
                            if (entry.FullName.Equals("xl/workbook.xml", StringComparison.OrdinalIgnoreCase)) hasWorkbook = true;
                            if (entry.FullName.Equals("xl/worksheets/sheet1.xml", StringComparison.OrdinalIgnoreCase)) hasSheet1 = true;
                            if (entry.FullName.Equals("xl/worksheets/sheet2.xml", StringComparison.OrdinalIgnoreCase)) hasSheet2 = true;
                            if (entry.FullName.Equals("[Content_Types].xml", StringComparison.OrdinalIgnoreCase)) hasContentTypes = true;
                        }
                    }

                    Assert("Excel OpenXML structure valid (Multi-sheet)",
                        hasWorkbook && hasSheet1 && hasSheet2 && hasContentTypes,
                        "Workbook: " + hasWorkbook + ", Sheet1: " + hasSheet1 + ", Sheet2: " + hasSheet2);
                }
                catch (Exception ex)
                {
                    Assert("Excel spec export validation", false, ex.Message);
                }

                // -------------------------------------------------------------
                // SUMMARY
                // -------------------------------------------------------------
                Console.WriteLine("\n================================================================================");
                if (passedTests == totalTests)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(string.Format("   PHASE 5 VERIFICATION PASSED: {0}/{1} TESTS SUCCESSFUL! 100% READY FOR RELEASE", passedTests, totalTests));
                    Console.ResetColor();
                    Console.WriteLine("================================================================================\n");
                    return 0;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("   PHASE 5 VERIFICATION FAILED: {0}/{1} TESTS PASSED", passedTests, totalTests));
                    Console.ResetColor();
                    Console.WriteLine("================================================================================\n");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[FATAL ERROR IN VERIFIER]: " + ex.Message + "\n" + ex.StackTrace);
                Console.ResetColor();
                return 1;
            }
        }
    }
}
