using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using ClassroomChecker.Native;

namespace ClassroomChecker
{
    public class SpikeCli
    {
        public static int Run()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("==========================================================");
            Console.WriteLine("   Classroom Checker — Phase 0 Spike Test (Console Mode)  ");
            Console.WriteLine("==========================================================");

            try
            {
                // 1. ກວດສອບ Fonts
                Console.WriteLine("\n[1] ກວດສອບ Fonts ພາສາລາວໃນລະບົບ...");
                string resolvedFont = "Segoe UI";
                using (InstalledFontCollection installed = new InstalledFontCollection())
                {
                    string[] candidates = { "Phetsarath OT", "Leelawadee UI", "Lao UI", "DokChampa" };
                    foreach (string candidate in candidates)
                    {
                        foreach (var f in installed.Families)
                        {
                            if (f.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine("    [✓] ພົບ Font: " + f.Name);
                                if (resolvedFont == "Segoe UI")
                                {
                                    resolvedFont = f.Name;
                                }
                            }
                        }
                    }
                }
                Console.WriteLine("    => Font ຫຼັກທີ່ໃຊ້ທົດສອບ: " + resolvedFont);

                // 2. ກວດສອບ Resolution ຈໍ
                Rectangle screen = Screen.PrimaryScreen.Bounds;
                int width = screen.Width > 0 ? screen.Width : 1920;
                int height = screen.Height > 0 ? screen.Height : 1080;
                Console.WriteLine(string.Format("\n[2] Resolution ຈໍຫຼັກ: {0} x {1} px", width, height));

                // 3. ທົດສອບສ້າງຮູບ Bitmap ພ້ອມຂໍ້ຄວາມລາວ
                Console.WriteLine("\n[3] ກຳລັງແຕ້ມຮູບ Wallpaper ພ້ອມຂໍ້ຄວາມລາວ (ສະຣະ/ວັນນະຍຸດ)...");
                string room = "ຫ້ອງ A";
                int number = 1;
                string mainLabel = string.Format("{0} - ເຄື່ອງ {1:D2}", room, number);
                string testLaoText = "ທົດສອບສະຣະ/ວັນນະຍຸດ: ຕົ້ນໄມ້ ໝໍ້ ຜູ້ໃຫຍ່ ເສື້ອຜ້າ ໐໑໒";

                using (Bitmap bmp = new Bitmap(width, height))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                    // Gradient background
                    using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                        new Point(0, 0),
                        new Point(width, height),
                        Color.FromArgb(15, 23, 42),
                        Color.FromArgb(30, 41, 59)))
                    {
                        g.FillRectangle(bgBrush, 0, 0, width, height);
                    }

                    // Accent header line
                    using (Pen pen = new Pen(Color.FromArgb(59, 130, 246), 6))
                    {
                        g.DrawLine(pen, 0, 0, width, 0);
                    }

                    // ແຕ້ມຂໍ້ຄວາມດ້ວຍ Uniscribe (TextRenderer)
                    int margin = 50;
                    using (Font mainFont = new Font(resolvedFont, 60, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (Font detailFont = new Font(resolvedFont, 26, FontStyle.Regular, GraphicsUnit.Pixel))
                    {
                        Size mainSize = TextRenderer.MeasureText(mainLabel, mainFont);
                        Size detailSize = TextRenderer.MeasureText(testLaoText, detailFont);

                        int mainX = width - margin - mainSize.Width;
                        int mainY = margin;
                        int detailX = width - margin - detailSize.Width;
                        int detailY = margin + mainSize.Height + 10;

                        // Shadow
                        TextRenderer.DrawText(g, mainLabel, mainFont, new Point(mainX + 3, mainY + 3), Color.Black);
                        TextRenderer.DrawText(g, testLaoText, detailFont, new Point(detailX + 2, detailY + 2), Color.Black);

                        // ForeText
                        TextRenderer.DrawText(g, mainLabel, mainFont, new Point(mainX, mainY), Color.White);
                        TextRenderer.DrawText(g, testLaoText, detailFont, new Point(detailX, detailY), Color.FromArgb(147, 197, 253));
                    }

                    // Footer audit info
                    using (Font subFont = new Font(resolvedFont, 14, FontStyle.Regular, GraphicsUnit.Pixel))
                    using (Brush subBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
                    {
                        string info = string.Format("Classroom Checker Spike | Host: {0} | User: {1} | {2}",
                            Environment.MachineName, Environment.UserName, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        g.DrawString(info, subFont, subBrush, margin, height - margin);
                    }

                    // 4. ບັນທຶກຮູບ
                    string targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ClassroomChecker");
                    try
                    {
                        if (!Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }
                    }
                    catch
                    {
                        // Fallback ໄປ LocalApplicationData ຖ້າບໍ່ມີສິດຂຽນ ProgramData
                        targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ClassroomChecker");
                        if (!Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }
                    }

                    string wallpaperPath = Path.Combine(targetDir, "wallpaper.png");
                    bmp.Save(wallpaperPath, ImageFormat.Png);
                    Console.WriteLine("    [✓] ບັນທຶກຮູບສຳເລັດ: " + wallpaperPath);

                    // ບັນທຶກໃສ່ໂຟນເດີໂປຣເຈັກນຳ ເພື່ອໃຫ້ຜູ້ໃຊ້ກວດເບິ່ງໄດ້ງ່າຍ
                    string localPreview = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "spike_preview.png");
                    bmp.Save(localPreview, ImageFormat.Png);
                    Console.WriteLine("    [✓] ບັນທຶກ Preview ສຳເລັດ: " + localPreview);

                    // 5. ທົດສອບຕັ້ງ Desktop Wallpaper ຈິງ
                    Console.WriteLine("\n[4] ກຳລັງສັ່ງຕັ້ງ Wallpaper ເທິງ Desktop ຈິງ (P/Invoke SystemParametersInfo)...");
                    bool ok = NativeMethods.SetDesktopWallpaper(wallpaperPath, NativeMethods.WallpaperStyle.Fill);
                    if (ok)
                    {
                        Console.WriteLine("    [✓] ຕັ້ງ Wallpaper ສຳເລັດແລ້ວ! (Desktop Wallpaper Updated)");
                    }
                    else
                    {
                        Console.WriteLine("    [!] SystemParametersInfo returned false");
                    }
                }

                Console.WriteLine("\n==========================================================");
                Console.WriteLine("   ຜົນການທົດສອບ PHASE 0 SPIKE: ຜ່ານຮຽບຮ້ອຍ (SUCCESS!)      ");
                Console.WriteLine("==========================================================");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[ERROR] ເກີດຂໍ້ຜິດພາດ: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        }
    }
}
