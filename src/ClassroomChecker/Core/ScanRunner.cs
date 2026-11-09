using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClassroomChecker.Core
{
    /// <summary>
    /// ຕົວປະມວນຜົນການສະແກນ ແລະ ແກ້ໄຂໂມດູນຕ່າງໆ ໃນ Background Thread ໂດຍບໍ່ໃຫ້ UI ຄ້າງ
    /// </summary>
    public class ScanRunner
    {
        public event Action<ICheckModule, int, int> ModuleStarted;
        public event Action<ICheckModule, CheckResult, int, int> ModuleCompleted;
        public event Action<Dictionary<ICheckModule, CheckResult>> AllCompleted;

        private bool _isRunning;

        public bool IsRunning
        {
            get { return _isRunning; }
        }

        public Task<Dictionary<ICheckModule, CheckResult>> RunCheckAsync(IList<ICheckModule> modules)
        {
            var list = new List<ICheckModule>(modules ?? new List<ICheckModule>());
            return RunAsync(list, false);
        }

        public Task<Dictionary<ICheckModule, CheckResult>> RunFixAsync(IList<ICheckModule> modules)
        {
            // ກັ່ນຕອງສະເພາະ module ທີ່ແກ້ໄດ້ ກ່ອນນັບ progress (ເພື່ອບໍ່ໃຫ້ເລກຂ້າມ)
            var list = new List<ICheckModule>();
            if (modules != null)
            {
                foreach (var m in modules)
                {
                    if (m.CanFix) list.Add(m);
                }
            }
            return RunAsync(list, true);
        }

        private async Task<Dictionary<ICheckModule, CheckResult>> RunAsync(List<ICheckModule> modules, bool fix)
        {
            if (_isRunning)
            {
                throw new InvalidOperationException(fix
                    ? "Fix operation is already in progress."
                    : "Scan is already in progress.");
            }
            _isRunning = true;

            var results = new Dictionary<ICheckModule, CheckResult>();
            int total = modules.Count;

            try
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < total; i++)
                    {
                        var mod = modules[i];
                        SafeRaise(() => { if (ModuleStarted != null) ModuleStarted.Invoke(mod, i + 1, total); });

                        CheckResult res;
                        try
                        {
                            res = fix ? mod.Fix() : mod.Check();
                        }
                        catch (Exception ex)
                        {
                            res = new CheckResult(
                                fix ? CheckStatus.Fail : CheckStatus.Unknown,
                                (fix ? "Fix exception: " : "Module exception: ") + ex.Message,
                                new List<string> { ex.ToString() });
                        }

                        results[mod] = res;
                        int idx = i;
                        SafeRaise(() => { if (ModuleCompleted != null) ModuleCompleted.Invoke(mod, res, idx + 1, total); });
                    }
                });
            }
            finally
            {
                // ຕ້ອງ reset ສະເໝີ ເຖິງມີ exception ຖ້າບໍ່ດັ່ງນັ້ນປຸ່ມສະແກນຈະກົດບໍ່ໄດ້ອີກ
                _isRunning = false;
            }

            SafeRaise(() => { if (AllCompleted != null) AllCompleted.Invoke(results); });
            return results;
        }

        /// <summary>ກັນ exception ຈາກ event handler ບໍ່ໃຫ້ທຳລາຍການສະແກນທັງໝົດ</summary>
        private static void SafeRaise(Action raise)
        {
            try { raise(); }
            catch (Exception) { }
        }
    }
}
