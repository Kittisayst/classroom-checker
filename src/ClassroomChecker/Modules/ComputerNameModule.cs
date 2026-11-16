using System;
using System.Collections.Generic;
using ClassroomChecker.Config;
using ClassroomChecker.Core;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ໂມດູນກວດສອບ ແລະ ປ່ຽນຊື່ຄອມພິວເຕີ (Computer Name / Hostname)
    /// ກວດສອບວ່າຊື່ເຄື່ອງໄດ້ມາດຕະຖານຕາມຫ້ອງຮຽນ ແລະ ເລກເຄື່ອງ (ເຊັ່ນ COM1-01) ຫຼືບໍ່
    /// </summary>
    public class ComputerNameModule : ICheckModule
    {
        public string Id
        {
            get { return "computer_name"; }
        }

        public string DisplayNameKey
        {
            get { return "module_computer_name"; }
        }

        public bool CanFix
        {
            get { return true; }
        }

        public CheckResult Check()
        {
            var details = new List<string>();

            string current = ComputerNameService.GetCurrentName();
            string pending = ComputerNameService.GetPendingName();

            var cfg = ConfigService.Instance.Current;
            string room = (cfg != null && cfg.selected != null && !string.IsNullOrEmpty(cfg.selected.room))
                ? cfg.selected.room
                : "ຫ້ອງຄອມພິວເຕີ 1";
            int number = (cfg != null && cfg.selected != null)
                ? cfg.selected.number
                : 1;

            string expected = (cfg != null && cfg.selected != null && !string.IsNullOrEmpty(cfg.selected.computerName))
                ? cfg.selected.computerName.Trim().ToUpperInvariant()
                : ComputerNameService.GenerateDefaultName(room, number);

            details.Add("ຊື່ເຄື່ອງປັດຈຸບັນ (Current Hostname): " + current);
            details.Add("ຊື່ມາດຕະຖານທີ່ກຳນົດ (Expected Name): " + expected);

            if (!string.IsNullOrEmpty(pending))
            {
                details.Add("ຊື່ໃໝ່ທີ່ລໍຖ້າ Restart (Pending Hostname): " + pending);
            }

            // 1. ກໍລະນີຊື່ປັດຈຸບັນກົງກັບຊື່ທີ່ກຳນົດແລ້ວ
            if (current.Equals(expected, StringComparison.OrdinalIgnoreCase))
            {
                return new CheckResult(
                    CheckStatus.Pass,
                    string.Format("ຊື່ເຄື່ອງໄດ້ມາດຕະຖານແລ້ວ ({0})", current),
                    details);
            }

            // 2. ກໍລະນີມີການປ່ຽນຊື່ແລ້ວ ແຕ່ກຳລັງລໍຖ້າ Restart
            if (!string.IsNullOrEmpty(pending) && pending.Equals(expected, StringComparison.OrdinalIgnoreCase))
            {
                return new CheckResult(
                    CheckStatus.Pass,
                    string.Format("ປ່ຽນຊື່ເຄື່ອງເປັນ {0} ແລ້ວ (ຈະມີຜົນຫຼັງຈາກ Restart)", expected),
                    details);
            }

            // 3. ກໍລະນີຊື່ຍັງເປັນຊື່ສຸ່ມເລີ່ມຕົ້ນຂອງ Windows (DESKTOP-XXXX, WIN-XXXX, LAPTOP-XXXX)
            bool isGeneric = current.StartsWith("DESKTOP-", StringComparison.OrdinalIgnoreCase) ||
                            current.StartsWith("WIN-", StringComparison.OrdinalIgnoreCase) ||
                            current.StartsWith("LAPTOP-", StringComparison.OrdinalIgnoreCase);

            string summary = isGeneric
                ? string.Format("ຊື່ເຄື່ອງຍັງເປັນຊື່ເລີ່ມຕົ້ນຂອງ Windows ({0}), ຄວນປ່ຽນເປັນ {1}", current, expected)
                : string.Format("ຊື່ເຄື່ອງປັດຈຸບັນ ({0}) ບໍ່ກົງກັບຮູບແບບຫ້ອງຮຽນ (ຄວນເປັນ {1})", current, expected);

            return new CheckResult(CheckStatus.Warning, summary, details);
        }

        public CheckResult Fix()
        {
            var cfg = ConfigService.Instance.Current;
            string room = (cfg != null && cfg.selected != null && !string.IsNullOrEmpty(cfg.selected.room))
                ? cfg.selected.room
                : "ຫ້ອງຄອມພິວເຕີ 1";
            int number = (cfg != null && cfg.selected != null)
                ? cfg.selected.number
                : 1;

            string target = (cfg != null && cfg.selected != null && !string.IsNullOrEmpty(cfg.selected.computerName))
                ? cfg.selected.computerName.Trim().ToUpperInvariant()
                : ComputerNameService.GenerateDefaultName(room, number);

            string statusMsg;
            bool ok = ComputerNameService.RenameComputer(target, out statusMsg);

            var details = new List<string>
            {
                "Target Hostname: " + target,
                "Status: " + statusMsg
            };

            string pending = ComputerNameService.GetPendingName();
            if (!string.IsNullOrEmpty(pending))
            {
                details.Add("Pending Name: " + pending);
            }

            if (ok)
            {
                return new CheckResult(CheckStatus.Pass, statusMsg, details);
            }
            else
            {
                return new CheckResult(CheckStatus.Fail, statusMsg, details);
            }
        }
    }
}
