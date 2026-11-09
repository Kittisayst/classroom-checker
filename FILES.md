# FILES.md — ລາຍການໄຟລ໌ໃນໂຄງການ Classroom Checker

ລາຍການໄຟລ໌ທັງໝົດທີ່ຖືກສ້າງຂຶ້ນໃນໂຄງການ ພ້ອມໜ້າທີ່ ແລະ ສະຖານະ:

| ໄຟລ໌ | ໜ້າທີ່ / ຄຳອະທິບາຍ |
|---|---|
| `cm-spec.md` | ເອກະສານ Specification ຫຼັກຂອງລະບົບ Classroom Checker |
| `FILES.md` | ບັນຊີລາຍຊື່ໄຟລ໌ທັງໝົດໃນໂຄງການ (ອັບເດດທຸກຄັ້ງທີ່ມີການສ້າງ/ແກ້ໄຂ) |
| `build.bat` | Script build ໂຄງການຜ່ານ MSBuild (Release x64) ໂດຍຄົ້ນຫາ MSBuild ແບບ dynamic ຜ່ານ vswhere ພ້ອມ fallback paths |
| `package.bat` | Script build ແລະ ລວມໄຟລ໌ລົງໂຟນເດີ `dist/` ສຳລັບ USB ໂດຍຄົ້ນຫາ MSBuild ແບບ dynamic ຜ່ານ vswhere |
| `dist/ClassroomChecker.exe` | ໄຟລ໌ Portable Executable ຫຼັກດ່ຽວ (Zero-dependency, 385 KB) ສຳລັບລັນຈາກ USB |
| `dist/README.txt` | ຄູ່ມືການໃຊ້ງານໂປຣແກຣມສອງພາສາ (ລາວ - English) ສຳລັບ Admin ຫ້ອງຮຽນ |
| `dist/backgrounds/README.txt` | ຄຳແນະນຳການນຳຮູບພາບພື້ນຫຼັງມາໃສ່ເພີ່ມເຕີມໃນໂຟນເດີ backgrounds |
| `dist/specs/.gitkeep` | ໂຟນເດີເປົ້າໝາຍສຳລັບບັນທຶກໄຟລ໌ Export Spec Excel (.xlsx) |
| `ClassroomChecker.sln` | Visual Studio Solution ຫຼັກຂອງໂຄງການ |
| `src/ClassroomChecker/ClassroomChecker.csproj` | C# Project file (.NET Framework 4.8, x64, WinForms, requireAdministrator) |
| `src/ClassroomChecker/app.manifest` | Application Manifest ກຳນົດສິດ `requireAdministrator` ແລະ PerMonitorV2 DPI |
| `src/ClassroomChecker/app.ico` | Application Icon ຫຼາຍຂະໜາດ (256, 64, 48, 32, 16) ຝັງເຂົ້າ Win32 PE Resource ແລະ MainForm |
| `src/ClassroomChecker/Program.cs` | Main Entry Point ເລີ່ມຕົ້ນ MainForm (ມີ global exception handler, ຮອງຮັບ CLI flags `--verify`, `--test-phase5` ແລະ `--spike` (ສະເພາະ Debug)) |
| `src/ClassroomChecker/Phase5Verifier.cs` | Suite ກວດສອບ ແລະ ທົດສອບລະບົບຄົບວົງຈອນສຳລັບ Phase 5 (22/22 Tests Passed) (ກວດທຸກ 11 module) |
| `test_phase5.ps1` | Script PowerShell ສຳລັບລັນ Phase5Verifier ຜ່ານ CLI ແບບ headless ໄດ້ທັນທີ |
| `src/ClassroomChecker/Config/AppConfig.cs` | Data Model ສຳລັບ config.json ຄົບທຸກ field ຕາມ spec ຂໍ້ 4 |
| `src/ClassroomChecker/Config/ConfigService.cs` | ບໍລິການອ່ານ/ບັນທຶກ config.json, fallback ໄປ AppData, auto-repair ຖ້າ json ເສຍ |
| `src/ClassroomChecker/Core/ICheckModule.cs` | Interface `ICheckModule`, Enum `CheckStatus`, Class `CheckResult` ສຳລັບລະບົບສະແກນ |
| `src/ClassroomChecker/Core/ScanRunner.cs` | ຕົວປະມວນຜົນການສະແກນ ແລະ ແກ້ໄຂໃນ Background Task ພ້ອມ Progress Event (reset `_isRunning` ໃນ finally, ນັບ progress ສະເພາະ module ທີ່ແກ້ໄດ້) |
| `src/ClassroomChecker/Core/ReportService.cs` | Log file (logs/{ຫ້ອງ}-{ເລກ}.log) ແລະ ບັນທຶກລາຍງານອັດຕະໂນມັດ (reports/) ຂ້າງ exe, fallback ໄປ AppData ຖ້າ USB read-only (log ຖືກ rotate ເປັນ .old ເມື່ອເກີນ 1 MB) |
| `tests/ClassroomChecker.Tests/Program.cs` | Unit tests 18 ກໍລະນີ (config ເສຍ, ScanRunner, xlsx round-trip, ຊື່ໄຟລ໌, ລາຍງານ, disk space threshold, duplicate spec detection, OEM serial filters, quick setup boundaries) ແບບບໍ່ຕ້ອງໃຊ້ NuGet |
| `tests/ClassroomChecker.Tests/ClassroomChecker.Tests.csproj` | Project ຂອງ unit tests (ບໍ່ຢູ່ໃນ .sln ເພື່ອບໍ່ໃຫ້ກະທົບ build/package) |
| `tests/run_tests.bat` | Build ແລະ ຣັນ unit tests (exit code 0 = ຜ່ານທັງໝົດ) |
| `src/ClassroomChecker/Core/SimpleXlsxWriter.cs` | Lightweight, zero-dependency OpenXML (.xlsx) Multi-sheet Generator ຜ່ານ System.IO.Compression |
| `src/ClassroomChecker/Modules/OfficeCheckModule.cs` | ໂມດູນກວດສອບ MS Office (Word, Excel, PowerPoint, FileVersion) ແລະ License (ospp.vbs) |
| `src/ClassroomChecker/Modules/WindowsLicenseCheckModule.cs` | ໂມດູນກວດສອບສະຖານະລິຂະສິດ Windows Activation ຜ່ານ WMI SoftwareLicensingProduct |
| `src/ClassroomChecker/Modules/EssentialSoftwareCheckModule.cs` | ໂມດູນກວດສອບຊອບແວພື້ນຖານການຮຽນ (Chrome, Edge, 7-Zip, WinRAR, PDF, VS Code, Python) ພ້ອມ Fix ຮອງຮັບ installers/ |
| `src/ClassroomChecker/Modules/FontCheckModule.cs` | ໂມດູນກວດສອບ ແລະ ຕິດຕັ້ງ Font Phetsarath OT ພ້ອມ Broadcast WM_FONTCHANGE |
| `src/ClassroomChecker/Modules/KeyboardCheckModule.cs` | ໂມດູນກວດສອບ ແລະ ຕິດຕັ້ງແປ້ນພິມລາວ (0454:00000454) ຜ່ານ InstallLayoutOrTip & PowerShell Append |
| `src/ClassroomChecker/Modules/PowerModule.cs` | ໂມດູນກວດສອບ ແລະ ຕັ້ງຄ່າ Never Sleep ສຳລັບ AC & DC (Monitor, Standby, Hibernate, Disk = 0) |
| `src/ClassroomChecker/Modules/CleanupModule.cs` | ໂມດູນກວດສອບ ແລະ ລ້າງໄຟລ໌ Temp (User, Windows, Update Cache, Recycle Bin) ປອດໄພ |
| `src/ClassroomChecker/Modules/DriverCheckModule.cs` | ໂມດູນກວດສອບ Driver ທີ່ຂາດ (Code 28) ຫຼື ຜິດປົກກະຕິ ຜ່ານ WMI Win32_PnPEntity (Fix: ຕິດຕັ້ງ driver ຈາກໂຟນເດີ drivers/ ຂ້າງ exe ດ້ວຍ pnputil, offline) |
| `src/ClassroomChecker/Modules/DiskSpaceCheckModule.cs` | ກວດພື້ນທີ່ວ່າງ drive ລະບົບ (<10% Warning, <5% Fail) |
| `src/ClassroomChecker/Modules/DefenderCheckModule.cs` | ກວດ Windows Defender (real-time + definitions ອາຍຸ) ຜ່ານ WMI offline |
| `src/ClassroomChecker/Modules/SpecExportModule.cs` | ໂມດູນດຶງຂໍ້ມູນ Hardware/OS ຜ່ານ WMI ແລະ Export ເປັນ Excel .xlsx ລົງໂຟນເດີ specs/ (ກວດເລກເຄື່ອງຊ້ຳ: ບໍ່ຂຽນທັບໄຟລ໌ຂອງເຄື່ອງອື່ນ, ບັນທຶກເປັນໄຟລ໌ໃໝ່ + Warning) |
| `src/ClassroomChecker/Modules/WallpaperModule.cs` | ໂມດູນສ້າງຮູບ Wallpaper ແທນ placeholders `{room}`, `{number}`, `{hostname}`, `{ip}` ແລະ ຕັ້ງ Desktop |
| `src/ClassroomChecker/Modules/AllUsersWallpaper.cs` | (ທາງເລືອກ, config background.applyToAllUsers) ຕັ້ງ wallpaper ໃຫ້ profile ຂອງ user ອື່ນ + Default profile ຜ່ານ reg load |
| `sign.bat` | ເຊັນ digital signature ໃຫ້ dist exe ດ້ວຍ signtool (ຕ້ອງຕັ້ງ SIGN_PFX / SIGN_PASSWORD) |
| `docs/code-signing.md` | ຄູ່ມື code signing (self-signed vs OV/EV, ວິທີເຊັນ, trust cert) |
| `docs/font-license.md` | Checklist ກວດ license ຂອງ font Phetsarath OT ກ່ອນແຈກຢາຍ |
| `src/ClassroomChecker/Native/NativeMethods.cs` | Win32 P/Invoke (`SystemParametersInfo`, `SHEmptyRecycleBin`, `Power*`, `AddFontResourceW`, Registry) |
| `src/ClassroomChecker/I18n/lo.json` | ໄຟລ໌ແປພາສາລາວ (ຝັງເປັນ EmbeddedResource) |
| `src/ClassroomChecker/I18n/en.json` | ໄຟລ໌ແປພາສາອັງກິດ (ຝັງເປັນ EmbeddedResource) |
| `src/ClassroomChecker/I18n/Localizer.cs` | ລະບົບສອງພາສາ `Localizer.T(...)` ພ້ອມ Event ແຈ້ງ UI refresh ທັນທີ |
| `src/ClassroomChecker/Assets/bg.png` | ຮູບພື້ນຫຼັງເລີ່ມຕົ້ນ default (ຝັງເປັນ EmbeddedResource 1920x1080) |
| `src/ClassroomChecker/Assets/Fonts/PhetsarathOT.ttf` | ໄຟລ໌ Font Phetsarath OT ທີ່ຝັງເປັນ EmbeddedResource ສຳລັບຕິດຕັ້ງ offline |
| `src/ClassroomChecker/UI/UiFontManager.cs` | ລະບົບຈັດການ Font UI ທົ່ວທັງແອັບ (Phetsarath OT ມາດຕະຖານ ພ້ອມ Memory font fallback) |
| `src/ClassroomChecker/UI/InputDialog.cs` | Modal Dialog ສຳລັບປ້ອນຊື່ຫ້ອງຮຽນໃໝ່ ພ້ອມກວດສອບຊື່ຊ້ຳ |
| `src/ClassroomChecker/UI/DetailsDialog.cs` | Modal Dialog ສະແດງລາຍລະອຽດຜົນການກວດສອບແຕ່ລະ Module ຢ່າງລະອຽດ |
| `src/ClassroomChecker/UI/WallpaperPanel.cs` | UI ແຖບພື້ນຫຼັງ (ເລືອກຫ້ອງ, ເພີ່ມ/ລຶບຫ້ອງ, ເລກເຄື່ອງ, ເລືອກຮູບ, Live Preview, ປຸ່ມກຳນົດພື້ນຫຼັງ) |
| `src/ClassroomChecker/UI/WallpaperPanel.Designer.cs` | ສ່ວນອອກແບບ Controls ຂອງ WallpaperPanel |
| `src/ClassroomChecker/UI/ScanPanel.cs` | UI ແຖບກວດສອບ (Checkboxes, ປຸ່ມສະແກນ, ຕາຕະລາງຜົນ ✓/✗/⚠/?, Progress, ແລະ Export ລາຍງານ) |
| `src/ClassroomChecker/UI/ScanPanel.Designer.cs` | ສ່ວນອອກແບບ Controls ຂອງ ScanPanel |
| `src/ClassroomChecker/UI/MainForm.cs` | ໜ້າຕ່າງຫຼັກ 2 ແຖບ (ພື້ນຫຼັງ \| ກວດສອບ) ພ້ອມ Dropdown ປ່ຽນພາສາ ລາວ/EN |
| `src/ClassroomChecker/UI/MainForm.Designer.cs` | ສ່ວນອອກແບບ Controls ຂອງ MainForm |
| `src/ClassroomChecker/UI/SpikeForm.cs` | [Debug ເທົ່ານັ້ນ] UI ທົດສອບ Phase 0 Spike: ແຕ້ມຂໍ້ຄວາມລາວ (GDI+ vs Uniscribe), Preview, ແລະ ຕັ້ງ Wallpaper ຈິງ |
| `src/ClassroomChecker/UI/SpikeForm.Designer.cs` | ສ່ວນອອກແບບ Controls ຂອງ SpikeForm |
| `src/ClassroomChecker/SpikeCli.cs` | [Debug ເທົ່ານັ້ນ] Automated CLI Spike Runner ສຳລັບທົດສອບແຕ້ມຟອນລາວ ແລະ ຕັ້ງ wallpaper ແບບ console |
| `docs/glossary.md` | ຄຳສັບສະເພາະ (Glossary) & Domain Model ສອງພາສາ (ລາວ - English) |
| `docs/adr/0001-architecture-and-runtime.md` | ADR-001: ການເລືອກ Runtime (.NET 4.8), ການ Build, ແລະ ບັນຈຸ Single-Exe |
| `docs/adr/0002-wallpaper-rendering-and-resolution.md` | ADR-002: ການແຕ້ມຂໍ້ຄວາມພາສາລາວ ແລະ ການປັບຂະໜາດຮູບຕາມ Screen Resolution |
| `docs/adr/0003-keyboard-and-font-installation.md` | ADR-003: ການຕິດຕັ້ງແປ້ນພິມ ແລະ ຟອນລາວ ແບບປອດໄພ (Non-destructive Append) |
| `src/ClassroomChecker/Core/MasterSummaryService.cs` | ບໍລິການລວມໄຟລ໌ Spec Excel ແຕ່ລະເຄື່ອງໃນ `specs/` ເຂົ້າເປັນ Master Summary Excel 3 Sheets (Overview, Hardware, Checks) ພ້ອມ KPI Cards |
| `src/ClassroomChecker/Core/QuickSetupRunner.cs` | ຕົວປະມວນຜົນການຕັ້ງຄ່າດ່ວນ 6 ຂັ້ນຕອນ (Wallpaper, Never Sleep, Font, Keyboard, Clean Temp, Spec Export) ພ້ອມ Auto-Increment PC Number ແລະ ກວດຈັບການຮອດເລກສູງສຸດ (Max PC limit) |
| `src/ClassroomChecker/UI/QuickSetupDialog.cs` | Modal Dialog ສະແດງຂະບວນການ Quick Setup 6 ຂັ້ນຕອນແບບ Live Progress, ສຽງເຕືອນ ແລະ ປິດອັດຕະໂນມັດ |
| `test_features_1_2.ps1` | Script PowerShell ທົດສອບການເຮັດວຽກຂອງ Master Summary Excel (Feature 1) ແລະ Quick Setup (Feature 2) ຄົບ 25/25 Tests |
| `docs/adr/0004-single-file-and-usb-deployment.md` | ADR-004: ການແພັກ Single Portable Executable ແລະ ການນຳໃຊ້ຜ່ານ USB Offline |
