using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassroomChecker.Config;
using ClassroomChecker.I18n;
using ClassroomChecker.Modules;

namespace ClassroomChecker.Core
{
    public class QuickSetupStepEventArgs : EventArgs
    {
        public int StepIndex { get; set; }
        public int TotalSteps { get; set; }
        public string StepName { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsSuccess { get; set; }
        public string Detail { get; set; }

        public QuickSetupStepEventArgs(int index, int total, string name, bool completed, bool success, string detail = null)
        {
            StepIndex = index;
            TotalSteps = total;
            StepName = name;
            IsCompleted = completed;
            IsSuccess = success;
            Detail = detail;
        }
    }

    /// <summary>
    /// ຕົວປະມວນຜົນການຕັ້ງຄ່າດ່ວນຄລິກດຽວ (One-Click Lab PC Onboarding)
    /// ເຮັດວຽກ 6 ຂັ້ນຕອນໃນ background thread ພ້ອມລາຍງານ Progress
    /// </summary>
    public class QuickSetupRunner
    {
        public event EventHandler<QuickSetupStepEventArgs> StepProgress;

        public int NextNumber { get; private set; }
        public bool IsMaxReached { get; private set; }

        internal static int CalculateNextNumber(int currentNumber, int maxNumber, out bool isMaxReached)
        {
            isMaxReached = currentNumber >= maxNumber;
            int next = currentNumber + 1;
            if (next > maxNumber) next = maxNumber;
            return next;
        }

        public async Task<bool> RunSetupAsync(string room, int number, string computerName = null, bool renameComputer = true)
        {
            return await Task.Run(() =>
            {
                int totalSteps = 11;
                bool allSuccessful = true;

                // -----------------------------------------------------------------
                // ຂັ້ນຕອນທີ 1: ຕັ້ງ Wallpaper Desktop (ພ້ອມຂໍ້ຄວາມລາວ Phetsarath OT)
                // -----------------------------------------------------------------
                string step1Name = Localizer.T("quick_setup_step_wallpaper", room, number.ToString("D2"));
                Notify(1, totalSteps, step1Name, false, false);

                try
                {
                    string appliedPath, errMsg;
                    bool wpOk = WallpaperModule.Instance.ApplyWallpaper(room, number, out appliedPath, out errMsg);
                    if (wpOk)
                    {
                        Notify(1, totalSteps, step1Name, true, true, "Wallpaper applied: " + Path.GetFileName(appliedPath)
                            + WallpaperModule.Instance.LastAllUsersNote);
                    }
                    else
                    {
                        allSuccessful = false;
                        Notify(1, totalSteps, step1Name, true, false, "Error: " + errMsg);
                    }
                }
                catch (Exception ex)
                {
                    allSuccessful = false;
                    Notify(1, totalSteps, step1Name, true, false, ex.Message);
                }

                // -----------------------------------------------------------------
                // ຂັ້ນຕອນທີ 2: ປັບ Power Scheme ເປັນ Never Sleep (AC & DC = 0)
                // -----------------------------------------------------------------
                string step2Name = Localizer.T("quick_setup_step_power");
                Notify(2, totalSteps, step2Name, false, false);

                try
                {
                    var pwrMod = new PowerModule();
                    var pwrRes = pwrMod.Fix();
                    bool pwrOk = pwrRes != null && pwrRes.Status == CheckStatus.Pass;
                    Notify(2, totalSteps, step2Name, true, pwrOk, pwrRes != null ? pwrRes.Summary : "");
                    if (!pwrOk) allSuccessful = false;
                }
                catch (Exception ex)
                {
                    allSuccessful = false;
                    Notify(2, totalSteps, step2Name, true, false, ex.Message);
                }

                // -----------------------------------------------------------------
                // ຂັ້ນຕອນທີ 3: ກວດສອບ ແລະ ຕິດຕັ້ງ Font Phetsarath OT (ຖ້າຂາດ)
                // -----------------------------------------------------------------
                string step3Name = Localizer.T("quick_setup_step_font");
                Notify(3, totalSteps, step3Name, false, false);

                try
                {
                    var fontMod = new FontCheckModule();
                    var fontCheck = fontMod.Check();
                    if (fontCheck.Status == CheckStatus.Pass)
                    {
                        Notify(3, totalSteps, step3Name, true, true, "Font Phetsarath OT installed already");
                    }
                    else
                    {
                        var fontFix = fontMod.Fix();
                        bool fontOk = fontFix != null && fontFix.Status == CheckStatus.Pass;
                        Notify(3, totalSteps, step3Name, true, fontOk, fontFix != null ? fontFix.Summary : "");
                        if (!fontOk) allSuccessful = false;
                    }
                }
                catch (Exception ex)
                {
                    allSuccessful = false;
                    Notify(3, totalSteps, step3Name, true, false, ex.Message);
                }

                // -----------------------------------------------------------------
                // ຂັ້ນຕອນທີ 4: ກວດສອບ ແລະ ຕິດຕັ້ງ ແປ້ນພິມພາສາລາວ (0454:00000454)
                // -----------------------------------------------------------------
                string step4Name = Localizer.T("quick_setup_step_keyboard");
                Notify(4, totalSteps, step4Name, false, false);

                try
                {
                    var kbdMod = new KeyboardCheckModule();
                    var kbdCheck = kbdMod.Check();
                    if (kbdCheck.Status == CheckStatus.Pass)
                    {
                        Notify(4, totalSteps, step4Name, true, true, "Lao keyboard layout installed already");
                    }
                    else
                    {
                        var kbdFix = kbdMod.Fix();
                        bool kbdOk = kbdFix != null && kbdFix.Status == CheckStatus.Pass;
                        Notify(4, totalSteps, step4Name, true, kbdOk, kbdFix != null ? kbdFix.Summary : "");
                        if (!kbdOk) allSuccessful = false;
                    }
                }
                catch (Exception ex)
                {
                    allSuccessful = false;
                    Notify(4, totalSteps, step4Name, true, false, ex.Message);
                }

                // -----------------------------------------------------------------
                // ຂັ້ນຕອນທີ 5: ກຳນົດ Timezone (+7) ແລະ ຮູບແບບວັນທີ່ dd/MM/yyyy
                // -----------------------------------------------------------------
                string step5Name = Localizer.T("quick_setup_step_datetime");
                Notify(5, totalSteps, step5Name, false, false);

                try
                {
                    var dtMod = new DateTimeCheckModule();
                    var dtRes = dtMod.Fix();
                    bool dtOk = dtRes != null && dtRes.Status == CheckStatus.Pass;
                    Notify(5, totalSteps, step5Name, true, dtOk, dtRes != null ? dtRes.Summary : "");
                    if (!dtOk) allSuccessful = false;
                }
                catch (Exception ex)
                {
                    allSuccessful = false;
                    Notify(5, totalSteps, step5Name, true, false, ex.Message);
                }

                // -----------------------------------------------------------------
                // ຂັ້ນຕອນທີ 6: ສ້າງໄອຄອນໜ້າຈໍ This PC, Control Panel ແລະ MS Office
                // -----------------------------------------------------------------
                string step6Name = Localizer.T("quick_setup_step_desktop_icons");
                Notify(6, totalSteps, step6Name, false, false);

                try
                {
                    var iconMod = new DesktopIconsModule();
                    var iconRes = iconMod.Fix();
                    bool iconOk = iconRes != null && iconRes.Status == CheckStatus.Pass;
                    Notify(6, totalSteps, step6Name, true, iconOk, iconRes != null ? iconRes.Summary : "");
                    if (!iconOk) allSuccessful = false;
                }
                catch (Exception ex)
                {
                    allSuccessful = false;
                    Notify(6, totalSteps, step6Name, true, false, ex.Message);
                }

                // -----------------------------------------------------------------
                // ຂັ້ນຕອນທີ 7: ລ້າງໄຟລ໌ Temp/Cache ປອດໄພ
                // -----------------------------------------------------------------
                string step7Name = Localizer.T("quick_setup_step_cleanup");
                Notify(7, totalSteps, step7Name, false, false);

                try
                {
                    var cleanMod = new CleanupModule();
                    var cleanRes = cleanMod.Fix();
                    bool cleanOk = cleanRes != null && cleanRes.Status == CheckStatus.Pass;
                    Notify(7, totalSteps, step7Name, true, cleanOk, cleanRes != null ? cleanRes.Summary : "");
                }
                catch (Exception ex)
                {
                    Notify(7, totalSteps, step7Name, true, false, ex.Message);
                }

                // -----------------------------------------------------------------
                // ຂັ້ນຕອນທີ 8: Export Machine Spec ເປັນ Excel (.xlsx)
                // -----------------------------------------------------------------
                string step8Name = Localizer.T("quick_setup_step_export", room, number.ToString("D2"));
                Notify(8, totalSteps, step8Name, false, false);

                try
                {
                    var specMod = new SpecExportModule();
                    var specRes = specMod.Fix();
                    bool specOk = specRes != null && specRes.Status == CheckStatus.Pass;
                    Notify(8, totalSteps, step8Name, true, specOk, specRes != null ? specRes.Summary : "");
                    if (!specOk) allSuccessful = false;
                }
                catch (Exception ex)
                {
                    allSuccessful = false;
                    Notify(8, totalSteps, step8Name, true, false, ex.Message);
                }

                // -----------------------------------------------------------------
                // ຂັ້ນຕອນທີ 9: ປິດ Windows AutoRun & ວັກຊີນປ້ອງກັນ USB Shortcut
                // -----------------------------------------------------------------
                string step9Name = Localizer.T("quick_setup_step_usb_vaccine");
                Notify(9, totalSteps, step9Name, false, false);

                try
                {
                    var usbMod = new UsbVaccineModule();
                    var usbRes = usbMod.Fix();
                    bool usbOk = usbRes != null && usbRes.Status == CheckStatus.Pass;
                    Notify(9, totalSteps, step9Name, true, usbOk, usbRes != null ? usbRes.Summary : "");
                }
                catch (Exception ex)
                {
                    Notify(9, totalSteps, step9Name, true, false, ex.Message);
                }

                // -----------------------------------------------------------------
                // ຂັ້ນຕອນທີ 10: ໂໝດເຄື່ອງສະເປັກຕ່ຳ (ສະເພາະເຄື່ອງ RAM ນ້ອຍ ຕາມຕັ້ງຄ່າ)
                // -----------------------------------------------------------------
                string stepPerfName = Localizer.T("quick_setup_step_performance");
                Notify(10, totalSteps, stepPerfName, false, false);

                try
                {
                    var perfRes = new PerformanceModule().Fix();
                    bool perfOk = perfRes != null && perfRes.Status == CheckStatus.Pass;
                    Notify(10, totalSteps, stepPerfName, true, perfOk, perfRes != null ? perfRes.Summary : "");
                    if (!perfOk) allSuccessful = false;
                }
                catch (Exception ex)
                {
                    allSuccessful = false;
                    Notify(10, totalSteps, stepPerfName, true, false, ex.Message);
                }

                // -----------------------------------------------------------------
                // ຂັ້ນຕອນທີ 11: ປ່ຽນຊື່ຄອມພິວເຕີ (Computer Name / Hostname)
                // -----------------------------------------------------------------
                string targetHost = !string.IsNullOrEmpty(computerName)
                    ? computerName.Trim().ToUpperInvariant()
                    : ComputerNameService.GenerateDefaultName(room, number);

                string step10Name = Localizer.T("quick_setup_step_rename", targetHost);
                Notify(11, totalSteps, step10Name, false, false);

                try
                {
                    if (renameComputer)
                    {
                        string renameMsg;
                        bool renameOk = ComputerNameService.RenameComputer(targetHost, out renameMsg);

                        // ປ່ຽນຊື່ user ທີ່ login ຢູ່ໃຫ້ກົງກັບຊື່ຄອມ (config renameUserWithComputer)
                        string targetUser = UserAccountService.BuildUserName(
                            ConfigService.Instance.Current.userNamePrefix, targetHost);
                        if (renameOk && ConfigService.Instance.Current.renameUserWithComputer
                            && !UserAccountService.IsInteractiveUserNamed(targetUser))
                        {
                            string userMsg;
                            bool userOk = UserAccountService.RenameInteractiveUser(targetUser, out userMsg);
                            renameMsg += " | " + userMsg;
                            renameOk = userOk;
                        }

                        Notify(11, totalSteps, step10Name, true, renameOk, renameMsg);
                        if (!renameOk) allSuccessful = false;
                    }
                    else
                    {
                        Notify(11, totalSteps, step10Name, true, true, "ຂ້າມການປ່ຽນຊື່ເຄື່ອງ (Skipped)");
                    }
                }
                catch (Exception ex)
                {
                    allSuccessful = false;
                    Notify(11, totalSteps, step10Name, true, false, ex.Message);
                }

                // -----------------------------------------------------------------
                // Auto-Increment PC Number ສຳລັບເຄື່ອງຕໍ່ໄປ
                // -----------------------------------------------------------------
                try
                {
                    var cfg = ConfigService.Instance.Current;
                    int maxNum = cfg.computerNumberRange != null ? cfg.computerNumberRange.max : 45;
                    bool isMax;
                    int nextNum = CalculateNextNumber(number, maxNum, out isMax);

                    IsMaxReached = isMax;
                    NextNumber = nextNum;

                    cfg.selected.room = room;
                    cfg.selected.number = nextNum;
                    cfg.selected.computerName = "";
                    ConfigService.Instance.Save();

                    if (isMax)
                    {
                        ReportService.Log(string.Format("QuickSetup: ຮອດເລກເຄື່ອງສູງສຸດແລ້ວ ({0:D2})", maxNum));
                    }
                    else
                    {
                        ReportService.Log(string.Format("QuickSetup: ປັບເລກເຄື່ອງຕໍ່ໄປເປັນ {0:D2}", nextNum));
                    }
                }
                catch { }

                return allSuccessful;
            });
        }

        private void Notify(int index, int total, string name, bool completed, bool success, string detail = null)
        {
            var handler = StepProgress;
            if (handler != null)
            {
                handler(this, new QuickSetupStepEventArgs(index, total, name, completed, success, detail));
            }
        }
    }
}
