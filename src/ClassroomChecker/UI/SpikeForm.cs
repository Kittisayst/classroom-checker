using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using ClassroomChecker.Native;

namespace ClassroomChecker.UI
{
    public partial class SpikeForm : Form
    {
        private Bitmap _currentWallpaperBitmap;

        public SpikeForm()
        {
            InitializeComponent();
        }

        private void SpikeForm_Load(object sender, EventArgs e)
        {
            // ໂຫຼດລາຍຊື່ Fonts ທີ່ຮອງຮັບພາສາລາວ
            PopulateAvailableFonts();

            // ສະແດງ Resolution ຂອງຈໍຫຼັກ
            Rectangle screen = Screen.PrimaryScreen.Bounds;
            lblScreenRes.Text = string.Format("Resolution ຈໍຫຼັກ: {0} x {1} px | ຜູ້ໃຊ້ປັດຈຸບັນ: {2}",
                screen.Width, screen.Height, Environment.UserName);

            // ສ້າງ Preview ຄັ້ງທຳອິດ
            GenerateWallpaperPreview();
        }

        private void PopulateAvailableFonts()
        {
            cmbFontFamily.Items.Clear();

            // ກວດສອບຟອນທີ່ມີໃນລະບົບ
            using (InstalledFontCollection installedFonts = new InstalledFontCollection())
            {
                var families = installedFonts.Families;

                // ລຳດັບຄວາມສຳຄັນ
                string[] candidates = { "Phetsarath OT", "Leelawadee UI", "Lao UI", "DokChampa", "Segoe UI" };

                foreach (string candidate in candidates)
                {
                    foreach (var font in families)
                    {
                        if (font.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase))
                        {
                            cmbFontFamily.Items.Add(font.Name);
                            break;
                        }
                    }
                }
            }

            if (cmbFontFamily.Items.Count == 0)
            {
                cmbFontFamily.Items.Add("Segoe UI");
            }

            cmbFontFamily.SelectedIndex = 0;
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            GenerateWallpaperPreview();
        }

        private void GenerateWallpaperPreview()
        {
            try
            {
                Rectangle screen = Screen.PrimaryScreen.Bounds;
                int width = screen.Width > 0 ? screen.Width : 1920;
                int height = screen.Height > 0 ? screen.Height : 1080;

                if (_currentWallpaperBitmap != null)
                {
                    _currentWallpaperBitmap.Dispose();
                    _currentWallpaperBitmap = null;
                }

                _currentWallpaperBitmap = new Bitmap(width, height);

                using (Graphics g = Graphics.FromImage(_currentWallpaperBitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                    // 1. ວາດພື້ນຫຼັງ Gradient ທັນສະໄໝ (Dark Navy to Slate Blue)
                    using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                        new Point(0, 0),
                        new Point(width, height),
                        Color.FromArgb(15, 23, 42),      // Slate 900
                        Color.FromArgb(30, 41, 59)))     // Slate 800
                    {
                        g.FillRectangle(bgBrush, 0, 0, width, height);
                    }

                    // ວາດລວດລາຍ accent line ເບື້ອງເທິງ
                    using (Pen accentPen = new Pen(Color.FromArgb(59, 130, 246), 6))
                    {
                        g.DrawLine(accentPen, 0, 0, width, 0);
                    }

                    // 2. ກຽມຂໍ້ຄວາມທີ່ຈະແຕ້ມ
                    string room = txtRoomName.Text.Trim();
                    int num = (int)numComputerNumber.Value;
                    string mainLabel = string.Format("{0} - ເຄື່ອງ {1:D2}", room, num);
                    string customTest = txtCustomText.Text.Trim();
                    string fontName = cmbFontFamily.SelectedItem != null ? cmbFontFamily.SelectedItem.ToString() : "Leelawadee UI";

                    int margin = 50;

                    // 3. ແຕ້ມຂໍ້ຄວາມຕາມ Engine ທີ່ເລືອກ
                    if (rdoGdiPlus.Checked)
                    {
                        // ວິທີທີ 1: GDI+ Graphics.DrawString
                        DrawLaoTextWithGdiPlus(g, mainLabel, customTest, fontName, width, margin);
                    }
                    else
                    {
                        // ວິທີທີ 2: GDI / Uniscribe TextRenderer.DrawText
                        DrawLaoTextWithTextRenderer(g, _currentWallpaperBitmap, mainLabel, customTest, fontName, width, margin);
                    }

                    // ວາດ watermark ລຸ່ມຊ້າຍ
                    using (Font subFont = new Font(fontName, 14, FontStyle.Regular))
                    using (Brush subBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
                    {
                        string info = string.Format("Classroom Checker | Host: {0} | User: {1} | {2}",
                            Environment.MachineName, Environment.UserName, DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                        g.DrawString(info, subFont, subBrush, margin, height - margin);
                    }
                }

                // ສະແດງໃນ Preview PictureBox
                if (picPreview.Image != null)
                {
                    picPreview.Image.Dispose();
                }
                picPreview.Image = new Bitmap(_currentWallpaperBitmap);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "ເກີດຂໍ້ຜິດພາດໃນການສ້າງ Preview: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void DrawLaoTextWithGdiPlus(Graphics g, string mainText, string customText, string fontName, int screenWidth, int margin)
        {
            using (Font mainFont = new Font(fontName, 60, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Font detailFont = new Font(fontName, 26, FontStyle.Regular, GraphicsUnit.Pixel))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near })
            {
                // Shadow
                using (Brush shadowBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                {
                    g.DrawString(mainText, mainFont, shadowBrush, new PointF(screenWidth - margin + 3, margin + 3), sf);
                    g.DrawString(customText, detailFont, shadowBrush, new PointF(screenWidth - margin + 2, margin + 85 + 2), sf);
                }

                // Text
                using (Brush textBrush = new SolidBrush(Color.White))
                using (Brush accentBrush = new SolidBrush(Color.FromArgb(147, 197, 253)))
                {
                    g.DrawString(mainText, mainFont, textBrush, new PointF(screenWidth - margin, margin), sf);
                    g.DrawString(customText, detailFont, accentBrush, new PointF(screenWidth - margin, margin + 85), sf);
                }
            }
        }

        private void DrawLaoTextWithTextRenderer(Graphics g, Bitmap bmp, string mainText, string customText, string fontName, int screenWidth, int margin)
        {
            using (Font mainFont = new Font(fontName, 60, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Font detailFont = new Font(fontName, 26, FontStyle.Regular, GraphicsUnit.Pixel))
            {
                Size mainSize = TextRenderer.MeasureText(mainText, mainFont);
                Size detailSize = TextRenderer.MeasureText(customText, detailFont);

                int mainX = screenWidth - margin - mainSize.Width;
                int mainY = margin;

                int detailX = screenWidth - margin - detailSize.Width;
                int detailY = margin + mainSize.Height + 10;

                // Shadow
                TextRenderer.DrawText(g, mainText, mainFont, new Point(mainX + 3, mainY + 3), Color.FromArgb(0, 0, 0));
                TextRenderer.DrawText(g, customText, detailFont, new Point(detailX + 2, detailY + 2), Color.FromArgb(0, 0, 0));

                // ForeText
                TextRenderer.DrawText(g, mainText, mainFont, new Point(mainX, mainY), Color.White);
                TextRenderer.DrawText(g, customText, detailFont, new Point(detailX, detailY), Color.FromArgb(147, 197, 253));
            }
        }

        private void btnApplyWallpaper_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentWallpaperBitmap == null)
                {
                    GenerateWallpaperPreview();
                }

                // 1. ສ້າງໂຟນເດີ C:\ProgramData\ClassroomChecker (ຫຼື fallback ໄປ LocalAppData)
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

                string wallpaperPath = Path.Combine(targetDir, "wallpaper.png");

                // 2. ບັນທຶກ Bitmap ເປັນໄຟລ໌ PNG
                _currentWallpaperBitmap.Save(wallpaperPath, System.Drawing.Imaging.ImageFormat.Png);

                // 3. ເອີ້ນ Native P/Invoke ຕັ້ງ Wallpaper
                bool success = NativeMethods.SetDesktopWallpaper(wallpaperPath, NativeMethods.WallpaperStyle.Fill);

                if (success)
                {
                    lblStatus.Text = string.Format("✓ ຕັ້ງ Wallpaper ສຳເລັດແລ້ວ! ສຳລັບບັນຊີ: {0}", Environment.UserName);
                    lblStatus.ForeColor = Color.FromArgb(46, 125, 50);
                    MessageBox.Show(
                        string.Format("ຕັ້ງ Desktop Wallpaper ຮຽບຮ້ອຍແລ້ວ!\n\nໄຟລ໌: {0}\nບັນຊີ: {1}", wallpaperPath, Environment.UserName),
                        "Classroom Checker — ສຳເລັດ",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    lblStatus.Text = "✗ ຕັ້ງ Wallpaper ບໍ່ສຳເລັດ (SystemParametersInfo ສົ່ງຄ່າ false)";
                    lblStatus.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "ເກີດຂໍ້ຜິດພາດ: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show("ເກີດຂໍ້ຜິດພາດ: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "PNG Image (*.png)|*.png";
                    sfd.FileName = string.Format("{0}-{1}.png", txtRoomName.Text.Trim(), numComputerNumber.Value);
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        _currentWallpaperBitmap.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                        lblStatus.Text = "✓ ບັນທຶກຮູບສຳເລັດ: " + Path.GetFileName(sfd.FileName);
                        lblStatus.ForeColor = Color.FromArgb(46, 125, 50);
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "ເກີດຂໍ້ຜິດພາດໃນການບັນທຶກ: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
            }
        }
    }
}
