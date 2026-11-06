using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Windows.Forms;
using ClassroomChecker.Config;
using ClassroomChecker.Native;

namespace ClassroomChecker.Modules
{
    /// <summary>
    /// ໂມດູນສ້າງຮູບພື້ນຫຼັງ (Wallpaper) ພ້ອມແທນ placeholder ແລະ ຕັ້ງຄ່າ Desktop
    /// </summary>
    public class WallpaperModule
    {
        private static WallpaperModule _instance;
        private static readonly object _lock = new object();

        public static WallpaperModule Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new WallpaperModule();
                        }
                    }
                }
                return _instance;
            }
        }

        private WallpaperModule() { }

        /// <summary>
        /// ສ້າງ Bitmap ຮູບ Wallpaper ຕາມຂະໜາດຈໍທີ່ລະບຸ ພ້ອມແຕ້ມຂໍ້ຄວາມກຳກັບ
        /// </summary>
        public Bitmap GenerateWallpaperBitmap(string room, int number, int width, int height)
        {
            if (width <= 0) width = 1920;
            if (height <= 0) height = 1080;

            AppConfig cfg = ConfigService.Instance.Current;
            Bitmap canvas = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                // 1. ໂຫຼດຮູບພື້ນຫຼັງ (ຈາກ config ຫຼື ຈາກ embedded asset)
                using (Image baseImage = LoadBaseBackgroundImage(cfg.background.imagePath))
                {
                    if (baseImage != null)
                    {
                        DrawBackgroundImage(g, baseImage, width, height, cfg.background.style);
                    }
                    else
                    {
                        // Fallback Gradient ຖ້າໂຫຼດຮູບບໍ່ໄດ້
                        using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                            new Point(0, 0),
                            new Point(width, height),
                            Color.FromArgb(15, 23, 42),
                            Color.FromArgb(30, 41, 59)))
                        {
                            g.FillRectangle(bgBrush, 0, 0, width, height);
                        }
                    }
                }

                // 2. ປະມວນຜົນຂໍ້ຄວາມ Placeholder
                string template = !string.IsNullOrEmpty(cfg.label.template) ? cfg.label.template : "{room} - {number}";
                string labelText = ReplacePlaceholders(template, room, number);

                // 3. ເລືອກ Font (Phetsarath OT -> Leelawadee UI -> Lao UI -> Segoe UI)
                string fontName = ResolveFontFamily(cfg.label.fontFamily);
                int fontSize = cfg.label.fontSizePx > 0 ? cfg.label.fontSizePx : 72;
                Color textColor = ParseColor(cfg.label.color, Color.White);
                int margin = cfg.label.marginPx > 0 ? cfg.label.marginPx : 40;

                // 4. ແຕ້ມຂໍ້ຄວາມລົງຕຳແໜ່ງ (default: top-right)
                DrawLabelText(g, labelText, fontName, fontSize, textColor, cfg.label.shadow, cfg.label.position, width, height, margin);

                // 5. ວາດຂໍ້ມູນເຄື່ອງຂະໜາດນ້ອຍຢູ່ມຸມລຸ່ມຊ້າຍ (Audit Info)
                DrawWatermarkInfo(g, fontName, width, height, margin);
            }

            return canvas;
        }

        /// <summary>
        /// ບັນທຶກຮູບລົງ C:\ProgramData\ClassroomChecker\wallpaper.png ແລະ ສັ່ງຕັ້ງ Wallpaper ຈິງ
        /// </summary>
        public bool ApplyWallpaper(string room, int number, out string appliedPath, out string errorMessage)
        {
            appliedPath = string.Empty;
            errorMessage = string.Empty;

            try
            {
                Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
                using (Bitmap bmp = GenerateWallpaperBitmap(room, number, screenBounds.Width, screenBounds.Height))
                {
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
                        targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ClassroomChecker");
                        if (!Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }
                    }

                    appliedPath = Path.Combine(targetDir, "wallpaper.png");
                    bmp.Save(appliedPath, System.Drawing.Imaging.ImageFormat.Png);

                    // ແປງ style
                    NativeMethods.WallpaperStyle style = ParseWallpaperStyle(ConfigService.Instance.Current.background.style);

                    bool success = NativeMethods.SetDesktopWallpaper(appliedPath, style);
                    if (!success)
                    {
                        errorMessage = "SystemParametersInfo returned false.";
                        return false;
                    }

                    // ອັບເດດ selected ລົງ config
                    ConfigService.Instance.Current.selected.room = room;
                    ConfigService.Instance.Current.selected.number = number;
                    ConfigService.Instance.Save();

                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private Image LoadBaseBackgroundImage(string configImagePath)
        {
            // 1. ລອງຫາຕາມ path ທີ່ກຳນົດໃນ config (relative ຈາກ exe ຫຼື absolute)
            if (!string.IsNullOrEmpty(configImagePath))
            {
                string fullPath = configImagePath;
                if (!Path.IsPathRooted(fullPath))
                {
                    string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                    fullPath = Path.Combine(exeDir, configImagePath);
                }

                if (File.Exists(fullPath))
                {
                    try
                    {
                        // ໂຫຼດຜ່ານ MemoryStream ເພື່ອບໍ່ໃຫ້ໄຟລ໌ຖືກລັອກ
                        byte[] bytes = File.ReadAllBytes(fullPath);
                        using (MemoryStream ms = new MemoryStream(bytes))
                        {
                            return Image.FromStream(ms);
                        }
                    }
                    catch { }
                }
            }

            // 2. ໂຫຼດຈາກ EmbeddedResource (Assets/bg.png)
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                using (Stream stream = asm.GetManifestResourceStream("ClassroomChecker.Assets.bg.png"))
                {
                    if (stream != null)
                    {
                        return Image.FromStream(stream);
                    }
                }
            }
            catch { }

            return null;
        }

        private void DrawBackgroundImage(Graphics g, Image img, int canvasWidth, int canvasHeight, string style)
        {
            if (string.IsNullOrEmpty(style)) style = "fill";
            style = style.ToLowerInvariant();

            if (style == "fit" || style == "center")
            {
                // Fit
                float ratio = Math.Min((float)canvasWidth / img.Width, (float)canvasHeight / img.Height);
                int w = (int)(img.Width * ratio);
                int h = (int)(img.Height * ratio);
                int x = (canvasWidth - w) / 2;
                int y = (canvasHeight - h) / 2;
                g.Clear(Color.Black);
                g.DrawImage(img, x, y, w, h);
            }
            else
            {
                // Fill / Stretch (default)
                g.DrawImage(img, 0, 0, canvasWidth, canvasHeight);
            }
        }

        private string ReplacePlaceholders(string template, string room, int number)
        {
            string result = template;
            result = result.Replace("{room}", room);
            result = result.Replace("{number}", number.ToString("D2"));
            result = result.Replace("{hostname}", Environment.MachineName);
            result = result.Replace("{ip}", GetLocalIPAddress());
            result = result.Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd"));
            return result;
        }

        private string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }

        private string ResolveFontFamily(string requestedFont)
        {
            using (InstalledFontCollection installed = new InstalledFontCollection())
            {
                var families = installed.Families;

                // ຖ້າ requestedFont ມີໃນເຄື່ອງ ໃຊ້ຕົວນັ້ນເລີຍ
                if (!string.IsNullOrEmpty(requestedFont))
                {
                    foreach (var f in families)
                    {
                        if (f.Name.Equals(requestedFont, StringComparison.OrdinalIgnoreCase))
                        {
                            return f.Name;
                        }
                    }
                }

                // Fallback ລຳດັບ
                string[] fallbacks = { "Phetsarath OT", "Leelawadee UI", "Lao UI", "DokChampa", "Segoe UI" };
                foreach (string fallback in fallbacks)
                {
                    foreach (var f in families)
                    {
                        if (f.Name.Equals(fallback, StringComparison.OrdinalIgnoreCase))
                        {
                            return f.Name;
                        }
                    }
                }
            }

            return "Segoe UI";
        }

        private Color ParseColor(string hex, Color defaultColor)
        {
            if (string.IsNullOrEmpty(hex)) return defaultColor;
            try
            {
                if (hex.StartsWith("#")) hex = hex.Substring(1);
                if (hex.Length == 6)
                {
                    int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    return Color.FromArgb(r, g, b);
                }
            }
            catch { }
            return defaultColor;
        }

        private NativeMethods.WallpaperStyle ParseWallpaperStyle(string style)
        {
            if (string.IsNullOrEmpty(style)) return NativeMethods.WallpaperStyle.Fill;
            switch (style.ToLowerInvariant())
            {
                case "fit": return NativeMethods.WallpaperStyle.Fit;
                case "stretch": return NativeMethods.WallpaperStyle.Stretch;
                case "center": return NativeMethods.WallpaperStyle.Center;
                case "tile": return NativeMethods.WallpaperStyle.Tile;
                default: return NativeMethods.WallpaperStyle.Fill;
            }
        }

        private void DrawLabelText(Graphics g, string text, string fontName, int fontSize, Color textColor, bool shadow, string position, int canvasWidth, int canvasHeight, int margin)
        {
            using (Font font = new Font(fontName, fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                Size textSize = TextRenderer.MeasureText(text, font);

                int x = canvasWidth - margin - textSize.Width;
                int y = margin;

                if (!string.IsNullOrEmpty(position) && position.Equals("top-left", StringComparison.OrdinalIgnoreCase))
                {
                    x = margin;
                    y = margin;
                }
                else if (!string.IsNullOrEmpty(position) && position.Equals("center", StringComparison.OrdinalIgnoreCase))
                {
                    x = (canvasWidth - textSize.Width) / 2;
                    y = (canvasHeight - textSize.Height) / 2;
                }

                // Shadow
                if (shadow)
                {
                    TextRenderer.DrawText(g, text, font, new Point(x + 3, y + 3), Color.FromArgb(0, 0, 0));
                }

                // Text
                TextRenderer.DrawText(g, text, font, new Point(x, y), textColor);
            }
        }

        private void DrawWatermarkInfo(Graphics g, string fontName, int canvasWidth, int canvasHeight, int margin)
        {
            using (Font font = new Font(fontName, 13, FontStyle.Regular, GraphicsUnit.Pixel))
            using (Brush brush = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                string info = string.Format("Classroom Checker | Host: {0} | User: {1} | {2}",
                    Environment.MachineName, Environment.UserName, DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                g.DrawString(info, font, brush, margin, canvasHeight - margin);
            }
        }
    }
}
