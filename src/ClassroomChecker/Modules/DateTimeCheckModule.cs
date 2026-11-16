using System;
using System.Collections.Generic;
using ClassroomChecker.Core;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ໂມດູນກວດສອບ ແລະ ຕັ້ງຄ່າ Timezone (+7 SE Asia) ແລະ ຮູບແບບວັນທີ່ (dd/MM/yyyy)
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

        public CheckResult Check()
        {
            var details = new List<string>();

            string currentTzId;
            double offsetHours;
            bool tzOk = DateTimeService.IsTimezoneUtc7(out currentTzId, out offsetHours);

            string currentFormat;
            string iDate;
            bool dateOk = DateTimeService.IsDateFormatDmy(out currentFormat, out iDate);

            details.Add(string.Format("Timezone: {0} (Offset: UTC{1:+00.00;-00.00}) -> {2}",
                currentTzId, offsetHours, tzOk ? "PASS (✓)" : "NOT UTC+7 (✗)"));
            details.Add(string.Format("Date Format: sShortDate='{0}', iDate='{1}' -> {2}",
                currentFormat, iDate, dateOk ? "PASS (✓)" : "NOT dd/MM/yyyy (✗)"));

            if (tzOk && dateOk)
            {
                return new CheckResult(
                    CheckStatus.Pass,
                    "ຕັ້ງ Timezone UTC+7 (SE Asia) ແລະ ຮູບແບບວັນທີ່ dd/MM/yyyy (ວ/ດ/ປ) ຖືກຕ້ອງ",
                    details);
            }
            else
            {
                var missing = new List<string>();
                if (!tzOk) missing.Add("Timezone ຍັງບໍ່ແມ່ນ UTC+7");
                if (!dateOk) missing.Add("ຮູບແບບວັນທີ່ ຍັງບໍ່ແມ່ນ ວ/ດ/ປ (dd/MM/yyyy)");

                return new CheckResult(
                    CheckStatus.Warning,
                    string.Join(", ", missing),
                    details);
            }
        }

        public CheckResult Fix()
        {
            List<string> logs;
            bool ok = DateTimeService.ApplyAll(out logs);

            if (ok)
            {
                return new CheckResult(
                    CheckStatus.Pass,
                    "ຕັ້ງ Timezone ເປັນ UTC+7 ແລະ ຮູບແບບວັນທີ່ເປັນ dd/MM/yyyy (ວ/ດ/ປ) ສຳເລັດແລ້ວ",
                    logs);
            }
            else
            {
                return new CheckResult(
                    CheckStatus.Warning,
                    "ຕັ້ງຄ່າ Timezone ຫຼື ຮູບແບບວັນທີ່ບາງສ່ວນບໍ່ສຳເລັດ",
                    logs);
            }
        }
    }
}
