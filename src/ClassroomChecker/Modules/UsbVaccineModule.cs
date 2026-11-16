using System;
using System.Collections.Generic;
using System.Diagnostics;
using ClassroomChecker.Core;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ໂມດູນກວດສອບ ແລະ ຕັ້ງຄ່າວັກຊີນປ້ອງກັນ USB Shortcut & Windows AutoRun
    /// </summary>
    public class UsbVaccineModule : ICheckModule
    {
        public string Id
        {
            get { return "usb_vaccine"; }
        }

        public string DisplayNameKey
        {
            get { return "module_usb_vaccine"; }
        }

        public bool CanFix
        {
            get { return true; }
        }

        public CheckResult Check()
        {
            var details = new List<string>();

            // 1. ກວດສອບ Windows AutoRun/AutoPlay Policy
            bool autoRunDisabled = UsbVirusCleanerService.IsHostAutoRunDisabled();
            details.Add(string.Format("Windows AutoRun Policy (NoDriveTypeAutoRun): {0}",
                autoRunDisabled ? "Disabled (0xFF / ປິດ AutoRun ປອດໄພ ✓)" : "Enabled (ຍັງເປີດຢູ່ - ສ່ຽງຕິດໄວຣັສ USB ⚠️)"));

            // 2. ກວດສອບ Process ໄວຣັສ Script ທີ່ກຳລັງແລ່ນໃນ RAM
            Process[] wscripts = Process.GetProcessesByName("wscript");
            Process[] cscripts = Process.GetProcessesByName("cscript");
            int rogueProcesses = wscripts.Length + cscripts.Length;

            if (rogueProcesses > 0)
            {
                details.Add(string.Format("⚠️ ພົບ Process ໄວຣັສ Script ກຳລັງແລ່ນໃນ RAM: wscript ({0}), cscript ({1})",
                    wscripts.Length, cscripts.Length));
            }
            else
            {
                details.Add("Process ໄວຣັສ Script: ບໍ່ພົບ wscript/cscript ແລ່ນຄ້າງໃນ RAM (✓)");
            }

            // 3. ສະຫຼຸບຜົນ
            if (autoRunDisabled && rogueProcesses == 0)
            {
                return new CheckResult(
                    CheckStatus.Pass,
                    "ປິດ Windows AutoRun ແລະ ບໍ່ພົບ Process ໄວຣັສ USB ແລ່ນໃນເຄື່ອງ (ປອດໄພ)",
                    details);
            }
            else if (rogueProcesses > 0)
            {
                return new CheckResult(
                    CheckStatus.Fail,
                    string.Format("ພົບ Process ໄວຣັສ Script ({0} processes) ກຳລັງແລ່ນໃນ RAM ຕ້ອງປິດທັນທີ!", rogueProcesses),
                    details);
            }
            else
            {
                return new CheckResult(
                    CheckStatus.Warning,
                    "Windows AutoRun ຍັງເປີດຢູ່ (ສ່ຽງຕິດໄວຣັສ USB Shortcut ເມື່ອສຽບ Flash Drive)",
                    details);
            }
        }

        public CheckResult Fix()
        {
            var details = new List<string>();

            // 1. ປິດ Process ໄວຣັສ
            int killed = UsbVirusCleanerService.KillHostMalwareProcesses(msg => details.Add(msg));
            if (killed > 0)
            {
                details.Add(string.Format("✓ ປິດ Process ໄວຣັສ script ໄປ {0} processes", killed));
            }

            // 2. ລຶບຄ່າ Startup Persistence
            int persistenceCleaned = UsbVirusCleanerService.CleanHostStartupPersistence(msg => details.Add(msg));
            if (persistenceCleaned > 0)
            {
                details.Add(string.Format("✓ ລຶບຄ່າໄວຣັສ Startup Persistence ໄປ {0} ລາຍການ", persistenceCleaned));
            }

            // 3. ປິດ Windows AutoRun/AutoPlay
            bool immunized = UsbVirusCleanerService.ImmunizeHostMachine(msg => details.Add(msg));
            if (immunized)
            {
                details.Add("✓ ຕັ້ງຄ່າປິດ Windows AutoRun/AutoPlay (NoDriveTypeAutoRun = 0xFF) ສຳເລັດ");
            }

            var postCheck = Check();
            details.AddRange(postCheck.Details);

            return new CheckResult(
                postCheck.Status,
                postCheck.Status == CheckStatus.Pass
                    ? "ສ້າງວັກຊີນ ແລະ ປິດ Windows AutoRun ສຳເລັດຮຽບຮ້ອຍ (ປອດໄພ)"
                    : postCheck.Summary,
                details);
        }
    }
}
