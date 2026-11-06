using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ClassroomChecker.Native;

namespace ClassroomChecker.UI
{
    /// <summary>
    /// ລະບົບຈັດການ ແລະ ກຳນົດ Font ສຳລັບ UI ທົ່ວທັງລະບົບ (Phetsarath OT ມາດຕະຖານ)
    /// ຕາມ cm-spec.md ຂໍ້ 6: "Font ຂອງ UI: ໃຊ້ Phetsarath OT ຖ້າມີ, ບໍ່ມີກໍໃຊ້ Leelawadee UI, ສຸດທ້າຍ Segoe UI"
    /// </summary>
    public static class UiFontManager
    {
        private static string _preferredFontFamily;
        private static readonly object _lock = new object();
        private static PrivateFontCollection _pfc;

        public static string PreferredFontFamily
        {
            get
            {
                if (_preferredFontFamily == null)
                {
                    lock (_lock)
                    {
                        if (_preferredFontFamily == null)
                        {
                            ResolveFont();
                        }
                    }
                }
                return _preferredFontFamily;
            }
        }

        private static void ResolveFont()
        {
            // 1. ກວດສອບວ່າ Phetsarath OT ຕິດຕັ້ງໃນ Windows ແລ້ວຫຼືບໍ່
            try
            {
                using (InstalledFontCollection installed = new InstalledFontCollection())
                {
                    foreach (var family in installed.Families)
                    {
                        if (family.Name.Equals("Phetsarath OT", StringComparison.OrdinalIgnoreCase))
                        {
                            _preferredFontFamily = family.Name;
                            return;
                        }
                    }
                }
            }
            catch { }

            // 2. ຖ້າບໍ່ມີໃນ Windows, ລອງໂຫຼດ Font ຈາກ Embedded Resource ຜ່ານ AddFontMemResourceEx
            try
            {
                Assembly asm = typeof(UiFontManager).Assembly;
                using (Stream stream = asm.GetManifestResourceStream("ClassroomChecker.Assets.Fonts.PhetsarathOT.ttf"))
                {
                    if (stream != null)
                    {
                        byte[] fontData = new byte[stream.Length];
                        stream.Read(fontData, 0, (int)stream.Length);

                        _pfc = new PrivateFontCollection();
                        IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
                        try
                        {
                            Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
                            _pfc.AddMemoryFont(fontPtr, fontData.Length);

                            uint cFonts = 0;
                            NativeMethods.AddFontMemResourceEx(fontPtr, (uint)fontData.Length, IntPtr.Zero, ref cFonts);
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(fontPtr);
                        }

                        if (_pfc.Families.Length > 0)
                        {
                            _preferredFontFamily = _pfc.Families[0].Name;
                            return;
                        }
                    }
                }
            }
            catch { }

            // 3. Fallback: Leelawadee UI (ມີໃນ Windows 10/11 ຢູ່ແລ້ວ ແລະ ຮອງຮັບພາສາລາວ)
            try
            {
                using (InstalledFontCollection installed = new InstalledFontCollection())
                {
                    foreach (var family in installed.Families)
                    {
                        if (family.Name.Equals("Leelawadee UI", StringComparison.OrdinalIgnoreCase))
                        {
                            _preferredFontFamily = "Leelawadee UI";
                            return;
                        }
                    }
                }
            }
            catch { }

            // 4. Fallback ສຸດທ້າຍ: Segoe UI
            _preferredFontFamily = "Segoe UI";
        }

        /// <summary>
        /// ສ້າງ Font object ດ້ວຍ PreferredFontFamily ຕາມຂະໜາດ ແລະ style ທີ່ກຳນົດ
        /// </summary>
        public static Font GetFont(float size, FontStyle style = FontStyle.Regular)
        {
            return new Font(PreferredFontFamily, size, style, GraphicsUnit.Point);
        }

        /// <summary>
        /// ປັບ Font ຂອງ Control ແລະ Controls ລູກທັງໝົດໃຫ້ໃຊ້ Phetsarath OT ໂດຍຮັກສາຂະໜາດ ແລະ style ເດີມ
        /// </summary>
        public static void ApplyUiFont(Control control)
        {
            if (control == null) return;
            string family = PreferredFontFamily;

            try
            {
                if (control.Font != null && !control.Font.FontFamily.Name.Equals(family, StringComparison.OrdinalIgnoreCase))
                {
                    control.Font = new Font(family, control.Font.Size, control.Font.Style, control.Font.Unit);
                }
            }
            catch { }

            // ສຳລັບ DataGridView
            if (control is DataGridView dgv)
            {
                try
                {
                    if (dgv.ColumnHeadersDefaultCellStyle != null && dgv.ColumnHeadersDefaultCellStyle.Font != null)
                    {
                        dgv.ColumnHeadersDefaultCellStyle.Font = new Font(family, dgv.ColumnHeadersDefaultCellStyle.Font.Size, dgv.ColumnHeadersDefaultCellStyle.Font.Style);
                    }
                    if (dgv.DefaultCellStyle != null && dgv.DefaultCellStyle.Font != null)
                    {
                        dgv.DefaultCellStyle.Font = new Font(family, dgv.DefaultCellStyle.Font.Size, dgv.DefaultCellStyle.Font.Style);
                    }
                    if (dgv.RowsDefaultCellStyle != null && dgv.RowsDefaultCellStyle.Font != null)
                    {
                        dgv.RowsDefaultCellStyle.Font = new Font(family, dgv.RowsDefaultCellStyle.Font.Size, dgv.RowsDefaultCellStyle.Font.Style);
                    }
                }
                catch { }
            }

            // ສຳລັບ TabControl
            if (control is TabControl tab)
            {
                try
                {
                    tab.Font = new Font(family, tab.Font.Size, tab.Font.Style, tab.Font.Unit);
                }
                catch { }
            }

            // ເຮັດວຽກແບບ Recursive ສຳລັບ control ລູກທຸກອັນ
            foreach (Control child in control.Controls)
            {
                ApplyUiFont(child);
            }
        }
    }
}
