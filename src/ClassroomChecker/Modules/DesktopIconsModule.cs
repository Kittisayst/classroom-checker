using System;
using System.Collections.Generic;
using ClassroomChecker.Core;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ໂມດູນກວດສອບ ແລະ ສ້າງໄອຄອນໜ້າຈໍ Desktop (This PC, Control Panel, Word, Excel, PowerPoint)
    /// </summary>
    public class DesktopIconsModule : ICheckModule
    {
        public string Id
        {
            get { return "desktop_icons"; }
        }

        public string DisplayNameKey
        {
            get { return "module_desktop_icons"; }
        }

        public bool CanFix
        {
            get { return true; }
        }

        public CheckResult Check()
        {
            var details = new List<string>();

            bool thisPc, controlPanel;
            bool sysOk = DesktopIconService.AreSystemIconsEnabled(out thisPc, out controlPanel);

            bool hasWord, hasExcel, hasPpt;
            bool officeLnkOk = DesktopIconService.AreOfficeShortcutsPresent(out hasWord, out hasExcel, out hasPpt);

            bool officeInstalled = DesktopIconService.FindOfficeExe("WINWORD.EXE") != null ||
                                  DesktopIconService.FindOfficeExe("EXCEL.EXE") != null;

            details.Add(string.Format("System Icons: This PC={0}, Control Panel={1} -> {2}",
                thisPc ? "SHOWN (✓)" : "HIDDEN (✗)",
                controlPanel ? "SHOWN (✓)" : "HIDDEN (✗)",
                sysOk ? "PASS (✓)" : "INCOMPLETE (✗)"));

            if (officeInstalled)
            {
                details.Add(string.Format("MS Office Desktop Shortcuts: Word={0}, Excel={1}, PowerPoint={2} -> {3}",
                    hasWord ? "✓" : "✗",
                    hasExcel ? "✓" : "✗",
                    hasPpt ? "✓" : "✗",
                    officeLnkOk ? "PASS (✓)" : "MISSING (✗)"));
            }
            else
            {
                details.Add("MS Office Executables: Not found on this machine");
            }

            // ຖ້າບໍ່ມີ Office ຢູ່ໃນເຄື່ອງ ໃຫ້ຖືວ່າຜ່ານຖ້າ system icons ຄົບ
            bool overallPass = sysOk && (!officeInstalled || officeLnkOk);

            if (overallPass)
            {
                return new CheckResult(
                    CheckStatus.Pass,
                    "ໄອຄອນໜ້າຈໍ This PC, Control Panel ແລະ MS Office ພ້ອມໃຊ້ງານແລ້ວ",
                    details);
            }
            else
            {
                var missing = new List<string>();
                if (!thisPc) missing.Add("This PC");
                if (!controlPanel) missing.Add("Control Panel");
                if (officeInstalled)
                {
                    if (!hasWord) missing.Add("Word");
                    if (!hasExcel) missing.Add("Excel");
                    if (!hasPpt) missing.Add("PowerPoint");
                }

                return new CheckResult(
                    CheckStatus.Warning,
                    "ຍັງຂາດໄອຄອນໜ້າຈໍ: " + string.Join(", ", missing),
                    details);
            }
        }

        public CheckResult Fix()
        {
            List<string> logs;
            bool ok = DesktopIconService.ApplyAll(out logs);

            if (ok)
            {
                return new CheckResult(
                    CheckStatus.Pass,
                    "ສ້າງ ແລະ ເປີດສະແດງໄອຄອນ This PC, Control Panel ແລະ MS Office ໄວ້ໜ້າຈໍຮຽບຮ້ອຍແລ້ວ",
                    logs);
            }
            else
            {
                return new CheckResult(
                    CheckStatus.Warning,
                    "ຕັ້ງຄ່າໄອຄອນໜ້າຈໍບາງລາຍການບໍ່ສຳເລັດ",
                    logs);
            }
        }
    }
}
