using System.Collections.Generic;

namespace ClassroomChecker.Core
{
    public enum CheckStatus
    {
        Pass,       // ✓ ຜ່ານ
        Fail,       // ✗ ບໍ່ຜ່ານ (ຕ້ອງແກ້ໄຂ)
        Warning,    // ⚠ ເຕືອນ (ມີຂໍ້ຜິດປົກກະຕິແຕ່ບໍ່ເຖິງຂັ້ນຂາດ)
        Unknown     // ? ກວດບໍ່ໄດ້ / ບໍ່ຮູ້ສະຖານະແນ່ນອນ
    }

    public class CheckResult
    {
        public CheckStatus Status { get; set; }
        public string Summary { get; set; }
        public List<string> Details { get; set; }

        public CheckResult()
        {
            Status = CheckStatus.Unknown;
            Summary = string.Empty;
            Details = new List<string>();
        }

        public CheckResult(CheckStatus status, string summary)
        {
            Status = status;
            Summary = summary;
            Details = new List<string>();
        }

        public CheckResult(CheckStatus status, string summary, List<string> details)
        {
            Status = status;
            Summary = summary;
            Details = details ?? new List<string>();
        }
    }

    /// <summary>
    /// Interface ຫຼັກສຳລັບທຸກໂມດູນກວດສອບໃນ Classroom Checker ຕາມ cm-spec.md ຂໍ້ 3
    /// </summary>
    public interface ICheckModule
    {
        string Id { get; }
        string DisplayNameKey { get; }
        bool CanFix { get; }

        /// <summary>
        /// ກວດສອບສະຖານະ (ຫ້າມປ່ຽນຄ່າລະບົບ)
        /// </summary>
        CheckResult Check();

        /// <summary>
        /// ແກ້ໄຂຂໍ້ທີ່ບໍ່ຜ່ານ (ສະເພາະເມື່ອ CanFix = true)
        /// </summary>
        CheckResult Fix();
    }
}
