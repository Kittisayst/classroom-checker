using System;
using System.Collections.Generic;
using ClassroomChecker.Config;
using ClassroomChecker.Core;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ໂມດູນກວດສອບ ແລະ ຕັ້ງຄ່າ Timezone (+7 SE Asia), ຮູບແບບວັນທີ ແລະ ຮູບແບບເວລາ (12/24 ຊົ່ວໂມງ)
    /// ຕາມທີ່ກຳນົດໃນ ຕັ້ງຄ່າ → ວັນທີ & ເວລາ
    /// </summary>
    public class DateTimeCheckModule : ICheckModule
    {
        public string Id
        {
            get { return "datetime_format"; }
        }

        public string DisplayNameKey
        {
            get { return "module_datetime_format"; }
        }

        public bool CanFix
        {
            get { return true; }
        }

        private static DateTimeConfig CurrentConfig()
        {
            var cfg = ConfigService.Instance.Current;
            return (cfg != null && cfg.dateTime != null) ? cfg.dateTime : new DateTimeConfig();
        }

        public CheckResult Check()
        {
            var cfg = CurrentConfig();
            var details = new List<string>();

            string currentTzId;
            double offsetHours;
            bool tzOk = DateTimeService.IsTimezoneUtc7(out currentTzId, out offsetHours);

            bool dateOk, timeOk;
            string currentDate, currentTime;
            DateTimeService.IsDateTimeFormatOk(cfg, out dateOk, out timeOk, out currentDate, out currentTime);

            string wantTime = DateTimeService.GetShortTimePattern(cfg.use24Hour);
            string hourLabel = cfg.use24Hour ? "24 ຊົ່ວໂມງ" : "12 ຊົ່ວໂມງ";

            details.Add(string.Format("Timezone: {0} (Offset: UTC{1:+00.00;-00.00}) -> {2}",
                currentTzId, offsetHours, tzOk ? "PASS (✓)" : "NOT UTC+7 (✗)"));
            details.Add(string.Format("ວັນທີ: '{0}' (ຕ້ອງການ '{1}') -> {2}",
                currentDate, cfg.dateFormat, dateOk ? "PASS (✓)" : "✗"));
            details.Add(string.Format("ເວລາ: '{0}' (ຕ້ອງການ '{1}', {2}) -> {3}",
                currentTime, wantTime, hourLabel, timeOk ? "PASS (✓)" : "✗"));
            details.Add("ຕົວຢ່າງ: " + DateTimeService.FormatPreview(DateTime.Now, cfg.dateFormat, cfg.use24Hour));

            if (tzOk && dateOk && timeOk)
            {
                return new CheckResult(
                    CheckStatus.Pass,
                    string.Format("Timezone UTC+7, ວັນທີ {0}, ເວລາ {1} ຖືກຕ້ອງ", cfg.dateFormat, hourLabel),
                    details);
            }

            var missing = new List<string>();
            if (!tzOk) missing.Add("Timezone ຍັງບໍ່ແມ່ນ UTC+7");
            if (!dateOk) missing.Add("ຮູບແບບວັນທີຍັງບໍ່ແມ່ນ " + cfg.dateFormat);
            if (!timeOk) missing.Add("ຮູບແບບເວລາຍັງບໍ່ແມ່ນ " + hourLabel);

            return new CheckResult(CheckStatus.Warning, string.Join(", ", missing), details);
        }

        public CheckResult Fix()
        {
            var cfg = CurrentConfig();
            List<string> logs;
            bool ok = DateTimeService.ApplyAll(cfg, out logs);
            string hourLabel = cfg.use24Hour ? "24 ຊົ່ວໂມງ" : "12 ຊົ່ວໂມງ";

            if (ok)
            {
                return new CheckResult(
                    CheckStatus.Pass,
                    string.Format("ຕັ້ງ Timezone UTC+7, ວັນທີ {0} ແລະ ເວລາ {1} ສຳເລັດແລ້ວ", cfg.dateFormat, hourLabel),
                    logs);
            }

            return new CheckResult(
                CheckStatus.Warning,
                "ຕັ້ງຄ່າ Timezone ຫຼື ຮູບແບບວັນທີ/ເວລາ ບາງສ່ວນບໍ່ສຳເລັດ",
                logs);
        }
    }
}
