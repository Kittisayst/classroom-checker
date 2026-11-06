using System;
using System.Windows.Forms;
using ClassroomChecker.UI;

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

                if (args[0].Equals("--spike", StringComparison.OrdinalIgnoreCase) ||
                    args[0].Equals("--test", StringComparison.OrdinalIgnoreCase))
                {
                    SpikeCli.Run();
                    return;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "ເກີດຂໍ້ຜິດພາດທີ່ບໍ່ຄາດຄິດ:\n\n" + ex.Message + "\n\n" + ex.StackTrace,
                    "Classroom Checker — Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
