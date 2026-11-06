using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ClassroomChecker.Core;
using ClassroomChecker.Native;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ໂມດູນກວດສອບ ແລະ ຕັ້ງຄ່າ Power Scheme ໃຫ້ເປັນ Never Sleep (AC & DC = 0) ຕາມ cm-spec.md ຂໍ້ 5.3
    /// </summary>
    public class PowerModule : ICheckModule
    {
        public string Id
        {
            get { return "power"; }
        }

        public string DisplayNameKey
        {
            get { return "module_power"; }
        }

        public bool CanFix
        {
            get { return true; }
        }

        public CheckResult Check()
        {
            var details = new List<string>();
            bool isAllNever = true;

            try
            {
                IntPtr pSchemeGuid;
                uint ret = NativeMethods.PowerGetActiveScheme(IntPtr.Zero, out pSchemeGuid);
                if (ret != 0 || pSchemeGuid == IntPtr.Zero)
                {
                    return new CheckResult(CheckStatus.Unknown, "ບໍ່ສາມາດອ່ານ Active Power Scheme ໄດ້", details);
                }

                Guid activeScheme = (Guid)Marshal.PtrToStructure(pSchemeGuid, typeof(Guid));
                NativeMethods.LocalFree(pSchemeGuid);

                details.Add("Active Scheme GUID: " + activeScheme.ToString());

                // 1. ກວດ Monitor Timeout
                uint monitorAc = GetSettingValue(activeScheme, NativeMethods.GUID_VIDEO_SUBGROUP, NativeMethods.GUID_VIDEO_POWERDOWN_TIMEOUT, true);
                uint monitorDc = GetSettingValue(activeScheme, NativeMethods.GUID_VIDEO_SUBGROUP, NativeMethods.GUID_VIDEO_POWERDOWN_TIMEOUT, false);
                details.Add(string.Format("Monitor Timeout: AC={0}s, DC={1}s", monitorAc, monitorDc));
                if (monitorAc != 0 || monitorDc != 0) isAllNever = false;

                // 2. ກວດ Standby (Sleep) Timeout
                uint standbyAc = GetSettingValue(activeScheme, NativeMethods.GUID_SLEEP_SUBGROUP, NativeMethods.GUID_STANDBY_TIMEOUT, true);
                uint standbyDc = GetSettingValue(activeScheme, NativeMethods.GUID_SLEEP_SUBGROUP, NativeMethods.GUID_STANDBY_TIMEOUT, false);
                details.Add(string.Format("Sleep (Standby) Timeout: AC={0}s, DC={1}s", standbyAc, standbyDc));
                if (standbyAc != 0 || standbyDc != 0) isAllNever = false;

                // 3. ກວດ Hibernate Timeout
                uint hibAc = GetSettingValue(activeScheme, NativeMethods.GUID_SLEEP_SUBGROUP, NativeMethods.GUID_HIBERNATE_TIMEOUT, true);
                uint hibDc = GetSettingValue(activeScheme, NativeMethods.GUID_SLEEP_SUBGROUP, NativeMethods.GUID_HIBERNATE_TIMEOUT, false);
                details.Add(string.Format("Hibernate Timeout: AC={0}s, DC={1}s", hibAc, hibDc));
                if (hibAc != 0 || hibDc != 0) isAllNever = false;

                // 4. ກວດ Disk Timeout
                uint diskAc = GetSettingValue(activeScheme, NativeMethods.GUID_DISK_SUBGROUP, NativeMethods.GUID_DISK_POWERDOWN_TIMEOUT, true);
                uint diskDc = GetSettingValue(activeScheme, NativeMethods.GUID_DISK_SUBGROUP, NativeMethods.GUID_DISK_POWERDOWN_TIMEOUT, false);
                details.Add(string.Format("Disk Timeout: AC={0}s, DC={1}s", diskAc, diskDc));
                if (diskAc != 0 || diskDc != 0) isAllNever = false;

                if (isAllNever)
                {
                    return new CheckResult(CheckStatus.Pass, "ຕັ້ງຄ່າ Never Sleep ຮຽບຮ້ອຍແລ້ວ (AC & DC = Never)", details);
                }
                else
                {
                    return new CheckResult(CheckStatus.Fail, "ພົບການຕັ້ງຄ່າ Sleep/Timeout ທີ່ຍັງບໍ່ແມ່ນ Never", details);
                }
            }
            catch (Exception ex)
            {
                details.Add("Error: " + ex.Message);
                return new CheckResult(CheckStatus.Unknown, "ເກີດຂໍ້ຜິດພາດໃນການກວດ Power: " + ex.Message, details);
            }
        }

        public CheckResult Fix()
        {
            var details = new List<string>();

            try
            {
                string[] commands = {
                    "/change monitor-timeout-ac 0",
                    "/change monitor-timeout-dc 0",
                    "/change standby-timeout-ac 0",
                    "/change standby-timeout-dc 0",
                    "/change hibernate-timeout-ac 0",
                    "/change hibernate-timeout-dc 0",
                    "/change disk-timeout-ac 0",
                    "/change disk-timeout-dc 0"
                };

                foreach (string cmd in commands)
                {
                    RunPowerCfg(cmd);
                    details.Add("powercfg " + cmd + " [OK]");
                }

                // ສະແກນຊ້ຳເພື່ອຢືນຢັນ
                var recheck = Check();
                if (recheck.Status == CheckStatus.Pass)
                {
                    return new CheckResult(CheckStatus.Pass, "ຕັ້ງຄ່າ Never Sleep ສຳເລັດແລ້ວ!", details);
                }
                else
                {
                    return new CheckResult(CheckStatus.Warning, "ດຳເນີນການແລ້ວ ແຕ່ບາງຄ່າຍັງບໍ່ປ່ຽນເປັນ 0", details);
                }
            }
            catch (Exception ex)
            {
                details.Add("Error: " + ex.Message);
                return new CheckResult(CheckStatus.Fail, "ເກີດຂໍ້ຜິດພາດໃນການຕັ້ງ Power: " + ex.Message, details);
            }
        }

        private uint GetSettingValue(Guid scheme, Guid subGroup, Guid setting, bool isAc)
        {
            uint val = 0;
            if (isAc)
            {
                NativeMethods.PowerReadACValueIndex(IntPtr.Zero, ref scheme, ref subGroup, ref setting, out val);
            }
            else
            {
                NativeMethods.PowerReadDCValueIndex(IntPtr.Zero, ref scheme, ref subGroup, ref setting, out val);
            }
            return val;
        }

        private void RunPowerCfg(string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powercfg.exe",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process p = Process.Start(psi))
            {
                p.WaitForExit(5000);
            }
        }
    }
}
