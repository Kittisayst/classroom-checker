using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ClassroomChecker.Config;
using ClassroomChecker.I18n;
using ClassroomChecker.Modules;

namespace ClassroomChecker.UI
{
    public partial class WallpaperPanel : UserControl
    {
        private bool _isInitializing = true;
        private Bitmap _currentPreviewBitmap;

        public WallpaperPanel()
        {
            InitializeComponent();
        }

        private void WallpaperPanel_Load(object sender, EventArgs e)
        {
            Localizer.LanguageChanged += OnLanguageChanged;

            InitializeFromConfig();
            ApplyLocalization();

            _isInitializing = false;
            UpdatePreview();
        }

        private void InitializeFromConfig()
        {
            AppConfig cfg = ConfigService.Instance.Current;

            // 1. ຕັ້ງຄ່າເລກເຄື່ອງ
            int min = cfg.computerNumberRange != null ? cfg.computerNumberRange.min : 1;
            int max = cfg.computerNumberRange != null ? cfg.computerNumberRange.max : 45;
            numComputerNumber.Minimum = min;
            numComputerNumber.Maximum = max;

            int selectedNum = cfg.selected != null ? cfg.selected.number : 1;
            if (selectedNum < min) selectedNum = min;
            if (selectedNum > max) selectedNum = max;
            numComputerNumber.Value = selectedNum;

            // 2. ໂຫຼດລາຍຊື່ຫ້ອງ
            ReloadRoomList(cfg.selected != null ? cfg.selected.room : null);

            // 3. ຕັ້ງຄ່າຮູບພື້ນຫຼັງ
            txtBgPath.Text = cfg.background != null && !string.IsNullOrEmpty(cfg.background.imagePath)
                ? cfg.background.imagePath
                : "bg.png";
        }

        public void ApplyLocalization()
        {
            grpWallpaperSettings.Text = Localizer.T("tab_wallpaper");
            lblRoom.Text = Localizer.T("room");
            btnAddRoom.Text = Localizer.T("btn_add_room");
            btnDeleteRoom.Text = Localizer.T("btn_delete_room");
            lblNumber.Text = Localizer.T("computer_number");
            lblBgImage.Text = Localizer.T("bg_image");
            btnBrowseBg.Text = Localizer.T("btn_browse");
            btnDefaultBg.Text = Localizer.T("btn_default_bg");
            btnQuickSetup.Text = Localizer.T("btn_quick_setup");
            btnApplyWallpaper.Text = Localizer.T("btn_apply_wallpaper");
            lblPreviewTitle.Text = Localizer.T("preview") + ":";

            lblUserNote.Text = Localizer.T("set_for_user", Environment.UserName);

            if (ConfigService.Instance.IsUsingFallback)
            {
                lblStatus.Text = Localizer.T("usb_readonly_warning");
                lblStatus.ForeColor = Color.FromArgb(180, 83, 9); // Amber
            }
            else
            {
                lblStatus.Text = Localizer.T("status_ready");
                lblStatus.ForeColor = Color.FromArgb(46, 125, 50); // Green
            }
        }

        private void OnLanguageChanged()
        {
            ApplyLocalization();
            UpdatePreview();
        }

        private void ReloadRoomList(string selectRoomName = null)
        {
            cmbRooms.Items.Clear();
            var rooms = ConfigService.Instance.Current.rooms;

            if (rooms == null || rooms.Count == 0)
            {
                rooms = new System.Collections.Generic.List<string> { "ຫ້ອງ A" };
                ConfigService.Instance.Current.rooms = rooms;
            }

            foreach (string r in rooms)
            {
                cmbRooms.Items.Add(r);
            }

            if (!string.IsNullOrEmpty(selectRoomName) && cmbRooms.Items.Contains(selectRoomName))
            {
                cmbRooms.SelectedItem = selectRoomName;
            }
            else if (cmbRooms.Items.Count > 0)
            {
                cmbRooms.SelectedIndex = 0;
            }
        }

        private void cmbRooms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing || cmbRooms.SelectedItem == null) return;

            string selectedRoom = cmbRooms.SelectedItem.ToString();
            ConfigService.Instance.Current.selected.room = selectedRoom;
            ConfigService.Instance.Save();

            UpdatePreview();
        }

        private void numComputerNumber_ValueChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return;

            int selectedNum = (int)numComputerNumber.Value;
            ConfigService.Instance.Current.selected.number = selectedNum;
            ConfigService.Instance.Save();

            UpdatePreview();
        }

        private void btnAddRoom_Click(object sender, EventArgs e)
        {
            using (InputDialog dlg = new InputDialog(
                Localizer.T("prompt_add_room_title"),
                Localizer.T("prompt_add_room"),
                ""))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    string newName = dlg.InputText;
                    if (string.IsNullOrEmpty(newName))
                    {
                        MessageBox.Show(Localizer.T("room_name_empty"), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var rooms = ConfigService.Instance.Current.rooms;
                    if (rooms.Contains(newName))
                    {
                        MessageBox.Show(Localizer.T("room_exists"), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    rooms.Add(newName);
                    ConfigService.Instance.Current.selected.room = newName;
                    ConfigService.Instance.Save();

                    ReloadRoomList(newName);
                    UpdatePreview();
                }
            }
        }

        private void btnDeleteRoom_Click(object sender, EventArgs e)
        {
            if (cmbRooms.SelectedItem == null) return;
            string roomToDelete = cmbRooms.SelectedItem.ToString();

            var rooms = ConfigService.Instance.Current.rooms;
            if (rooms.Count <= 1)
            {
                MessageBox.Show(Localizer.T("cannot_delete_last_room"), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                Localizer.T("confirm_delete_room", roomToDelete),
                Localizer.T("confirm_delete_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                rooms.Remove(roomToDelete);
                ConfigService.Instance.Current.selected.room = rooms[0];
                ConfigService.Instance.Save();

                ReloadRoomList(rooms[0]);
                UpdatePreview();
            }
        }

        private void btnBrowseBg_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All Files (*.*)|*.*";
                ofd.Title = Localizer.T("btn_browse");

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        string sourceFile = ofd.FileName;
                        string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                        string backgroundsDir = Path.Combine(exeDir, "backgrounds");

                        if (!Directory.Exists(backgroundsDir))
                        {
                            Directory.CreateDirectory(backgroundsDir);
                        }

                        string fileName = Path.GetFileName(sourceFile);
                        string destFile = Path.Combine(backgroundsDir, fileName);

                        // ສຳເນົາໄຟລ໌ລົງ backgrounds/
                        if (!sourceFile.Equals(destFile, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Copy(sourceFile, destFile, true);
                        }

                        string relativePath = Path.Combine("backgrounds", fileName);
                        txtBgPath.Text = relativePath;
                        ConfigService.Instance.Current.background.imagePath = relativePath;
                        ConfigService.Instance.Save();

                        UpdatePreview();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ເກີດຂໍ້ຜິດພາດໃນການເລືອກຮູບ: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnDefaultBg_Click(object sender, EventArgs e)
        {
            txtBgPath.Text = "bg.png";
            ConfigService.Instance.Current.background.imagePath = "bg.png";
            ConfigService.Instance.Save();

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            try
            {
                string room = cmbRooms.SelectedItem != null ? cmbRooms.SelectedItem.ToString() : "ຫ້ອງ A";
                int number = (int)numComputerNumber.Value;

                Rectangle screen = Screen.PrimaryScreen.Bounds;
                int w = screen.Width > 0 ? screen.Width : 1920;
                int h = screen.Height > 0 ? screen.Height : 1080;

                Bitmap bmp = WallpaperModule.Instance.GenerateWallpaperBitmap(room, number, w, h);

                if (picPreview.Image != null)
                {
                    picPreview.Image.Dispose();
                }
                _currentPreviewBitmap = bmp;
                picPreview.Image = new Bitmap(_currentPreviewBitmap);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Preview error: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void btnApplyWallpaper_Click(object sender, EventArgs e)
        {
            try
            {
                string room = cmbRooms.SelectedItem != null ? cmbRooms.SelectedItem.ToString() : "ຫ້ອງ A";
                int number = (int)numComputerNumber.Value;

                string appliedPath;
                string error;
                bool ok = WallpaperModule.Instance.ApplyWallpaper(room, number, out appliedPath, out error);

                if (ok)
                {
                    lblStatus.Text = "✓ " + Localizer.T("wallpaper_applied_success");
                    lblStatus.ForeColor = Color.FromArgb(46, 125, 50);

                    MessageBox.Show(
                        string.Format("{0}\n\n{1}: {2}\n{3}",
                            Localizer.T("wallpaper_applied_success"),
                            Localizer.T("room"), room + " - " + number.ToString("D2"),
                            Localizer.T("set_for_user", Environment.UserName) + WallpaperModule.Instance.LastAllUsersNote),
                        Localizer.T("app_title"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    lblStatus.Text = "✗ Failed: " + error;
                    lblStatus.ForeColor = Color.Red;
                    MessageBox.Show("ເກີດຂໍ້ຜິດພາດ: " + error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RefreshFromConfig()
        {
            InitializeFromConfig();
            UpdatePreview();
        }

        private void btnQuickSetup_Click(object sender, EventArgs e)
        {
            string room = cmbRooms.SelectedItem != null ? cmbRooms.SelectedItem.ToString() : "ຫ້ອງ A";
            int number = (int)numComputerNumber.Value;

            using (var dlg = new QuickSetupDialog(room, number))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshFromConfig();
                }
            }
        }
    }
}
