using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using ClassroomChecker.UI;

[assembly: InternalsVisibleTo("ClassroomChecker.Tests")]

namespace ClassroomChecker
{
    static class Program
    {
        /// <summary>
        /// Entry point ຫຼັກຂອງໂປຣແກຣມ Classroom Checker
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // ຮອງຮັບ CLI test mode ສຳລັບການທົດສອບອັດຕະໂນມັດ
            if (args != null && args.Length > 0)
            {
                if (args[0].Equals("--verify", StringComparison.OrdinalIgnoreCase) ||
                    args[0].Equals("--test-phase5", StringComparison.OrdinalIgnoreCase))
                {
                    int exitCode = Phase5Verifier.Run();
                    Environment.Exit(exitCode);
                    return;
                }

#if DEBUG
                // Spike CLI ມີສະເພາະ Debug build (ບໍ່ຕິດໄປກັບ exe ທີ່ແຈກຢາຍ)
                if (args[0].Equals("--spike", StringComparison.OrdinalIgnoreCase) ||
                    args[0].Equals("--test", StringComparison.OrdinalIgnoreCase))
                {
                    SpikeCli.Run();
                    return;
                }
#endif
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ກັນແອັບ crash ເງິຍໆ: ຈັບ exception ຂອງ UI thread ແລະ background thread
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ReportFatal(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ReportFatal(e.ExceptionObject as Exception);

            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                ReportFatal(ex);
            }
        }

        private static void ReportFatal(Exception ex)
        {
            try
            {
                string msg = ex != null ? ex.Message + "\n\n" + ex.StackTrace : "Unknown error";
                MessageBox.Show(
                    "ເກີດຂໍ້ຜິດພາດທີ່ບໍ່ຄາດຄິດ:\n\n" + msg,
                    "Classroom Checker — Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch { }
        }
    }
}
