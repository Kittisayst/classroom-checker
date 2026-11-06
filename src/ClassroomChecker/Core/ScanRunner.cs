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

        public async Task<Dictionary<ICheckModule, CheckResult>> RunCheckAsync(IList<ICheckModule> modules)
        {
            if (_isRunning) throw new InvalidOperationException("Scan is already in progress.");
            _isRunning = true;

            var results = new Dictionary<ICheckModule, CheckResult>();
            int total = modules != null ? modules.Count : 0;

            await Task.Run(() =>
            {
                for (int i = 0; i < total; i++)
                {
                    var mod = modules[i];
                    if (ModuleStarted != null)
                    {
                        ModuleStarted.Invoke(mod, i + 1, total);
                    }

                    CheckResult res;
                    try
                    {
                        res = mod.Check();
                    }
                    catch (Exception ex)
                    {
                        res = new CheckResult(CheckStatus.Unknown, "Module exception: " + ex.Message, new List<string> { ex.ToString() });
                    }

                    results[mod] = res;

                    if (ModuleCompleted != null)
                    {
                        ModuleCompleted.Invoke(mod, res, i + 1, total);
                    }
                }
            });

            _isRunning = false;

            if (AllCompleted != null)
            {
                AllCompleted.Invoke(results);
            }

            return results;
        }

        public async Task<Dictionary<ICheckModule, CheckResult>> RunFixAsync(IList<ICheckModule> modules)
        {
            if (_isRunning) throw new InvalidOperationException("Fix operation is already in progress.");
            _isRunning = true;

            var results = new Dictionary<ICheckModule, CheckResult>();
            int total = modules != null ? modules.Count : 0;

            await Task.Run(() =>
            {
                for (int i = 0; i < total; i++)
                {
                    var mod = modules[i];
                    if (!mod.CanFix) continue;

                    if (ModuleStarted != null)
                    {
                        ModuleStarted.Invoke(mod, i + 1, total);
                    }

                    CheckResult res;
                    try
                    {
                        res = mod.Fix();
                    }
                    catch (Exception ex)
                    {
                        res = new CheckResult(CheckStatus.Fail, "Fix exception: " + ex.Message, new List<string> { ex.ToString() });
                    }

                    results[mod] = res;

                    if (ModuleCompleted != null)
                    {
                        ModuleCompleted.Invoke(mod, res, i + 1, total);
                    }
                }
            });

            _isRunning = false;

            if (AllCompleted != null)
            {
                AllCompleted.Invoke(results);
            }

            return results;
        }
    }
}
