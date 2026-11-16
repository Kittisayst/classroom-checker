using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ClassroomChecker.Config;
using ClassroomChecker.Core;
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

            // 3. ຕັ້ງຄ່າຊື່ເຄື່ອງ
            if (cfg.selected != null && !string.IsNullOrEmpty(cfg.selected.computerName))
            {
                txtComputerName.Text = cfg.selected.computerName;
            }
            else
            {
                string r = cfg.selected != null ? cfg.selected.room : "ຫ້ອງຄອມພິວເຕີ 1";
                txtComputerName.Text = ComputerNameService.GenerateDefaultName(r, selectedNum);
            }

            // 4. ຕັ້ງຄ່າຮູບພື້ນຫຼັງ
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
            lblComputerName.Text = Localizer.T("lbl_computer_name", "ຊື່ເຄື່ອງ:");
            btnRenameComputer.Text = Localizer.T("btn_rename_computer", "ປ່ຽນຊື່ເຄື່ອງ");
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

            LayoutSettingsRows();
        }

        /// <summary>
        /// ຈັດຕຳແໜ່ງ control ໃນກ່ອງຕັ້ງຄ່າຕາມຄວາມຍາວຂໍ້ຄວາມຈິງ (ພາສາອັງກິດຍາວກວ່າລາວ
        /// ເຮັດໃຫ້ປຸ່ມ/label ທີ່ວາງແບບ fix pixel ຖືກຕັດ ຫຼື ທັບກັນ)
        /// </summary>
        private void LayoutSettingsRows()
        {
            const int gap = 6;
            const int groupGap = 16;

            int labelCol = lblRoom.Left + Math.Max(lblRoom.PreferredWidth, lblBgImage.PreferredWidth) + 8;
            cmbRooms.Left = labelCol;
            txtBgPath.Left = labelCol;

            // ແຖວ 1: ຫ້ອງ | ເພີ່ມ | ລຶບ | ເລກເຄື່ອງ | ຊື່ເຄື່ອງ | ປ່ຽນຊື່
            int x = cmbRooms.Right + gap;
            x = PlaceButton(btnAddRoom, x, 75) + gap;
            x = PlaceButton(btnDeleteRoom, x, 70) + groupGap;
            lblNumber.Left = x;
            x += lblNumber.PreferredWidth + gap;
            numComputerNumber.Left = x;
            x = numComputerNumber.Right + groupGap;
            lblComputerName.Left = x;
            x += lblComputerName.PreferredWidth + gap;
            txtComputerName.Left = x;
            x = txtComputerName.Right + gap;
            PlaceButton(btnRenameComputer, x, 85);

            // ແຖວ 2: ຮູບພື້ນຫຼັງ | ເລືອກຮູບ | ໃຊ້ຄ່າເລີ່ມຕົ້ນ
            x = txtBgPath.Right + 10;
            x = PlaceButton(btnBrowseBg, x, 90) + gap;
            PlaceButton(btnDefaultBg, x, 105);
        }

        private static int PlaceButton(Button btn, int x, int minWidth)
        {
            int textWidth = TextRenderer.MeasureText(btn.Text ?? string.Empty, btn.Font).Width + 18;
            btn.Width = Math.Max(minWidth, textWidth);
            btn.Left = x;
            return btn.Right;
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
                rooms = new System.Collections.Generic.List<string> {
                    "ຫ້ອງຄອມພິວເຕີ 1",
                    "ຫ້ອງຄອມພິວເຕີ 2",
                    "ຫ້ອງຄອມພິວເຕີ 3",
                    "ຫ້ອງຄອມພິວເຕີ 4",
                    "ຫ້ອງຄອມພິວເຕີ 5",
                    "ຫ້ອງຄອມພິວເຕີ 6",
                    "ຫ້ອງຄອມພິວເຕີ 7",
                    "ຫ້ອງຄອມພິວເຕີ 8"
                };
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

            UpdateSuggestedComputerName();
            UpdatePreview();
        }

        private void numComputerNumber_ValueChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return;

            int selectedNum = (int)numComputerNumber.Value;
            ConfigService.Instance.Current.selected.number = selectedNum;
            ConfigService.Instance.Save();

            UpdateSuggestedComputerName();
            UpdatePreview();
        }

        private void UpdateSuggestedComputerName()
        {
            if (cmbRooms.SelectedItem == null) return;
            string room = cmbRooms.SelectedItem.ToString();
            int number = (int)numComputerNumber.Value;
            string autoName = ComputerNameService.GenerateDefaultName(room, number);

            txtComputerName.Text = autoName;
        }

        private void btnRenameComputer_Click(object sender, EventArgs e)
        {
            string target = txtComputerName.Text.Trim().ToUpperInvariant();
            string valErr;
            if (!ComputerNameService.ValidateName(target, out valErr))
            {
                MessageBox.Show(valErr, Localizer.T("app_title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string current = ComputerNameService.GetCurrentName();
            string pending = ComputerNameService.GetPendingName();
            bool computerDone = current.Equals(target, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrEmpty(pending) && pending.Equals(target, StringComparison.OrdinalIgnoreCase));

            // ປ່ຽນຊື່ user ທີ່ login ຢູ່ໃຫ້ກົງກັບຊື່ຄອມ (ປິດໄດ້ໃນ ຕັ້ງຄ່າ → ທົ່ວໄປ)
            string currentUser = UserAccountService.GetInteractiveLocalUserName();
            string targetUser = UserAccountService.BuildUserName(ConfigService.Instance.Current.userNamePrefix, target);
            bool renameUser = ConfigService.Instance.Current.renameUserWithComputer
                && currentUser != null
                && !UserAccountService.IsInteractiveUserNamed(targetUser);

            if (computerDone && !renameUser)
            {
                MessageBox.Show(
                    string.Format("ຊື່ເຄື່ອງປັດຈຸບັນແມ່ນ {0} ຢູ່ແລ້ວ!", target),
                    Localizer.T("app_title"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var confirm = new System.Text.StringBuilder("ທ່ານຕ້ອງການປ່ຽນ:\n");
            if (!computerDone) confirm.AppendFormat("\n• ຊື່ເຄື່ອງ: '{0}' → '{1}' (ມີຜົນເມື່ອ Restart)", current, target);
            if (renameUser) confirm.AppendFormat("\n• ຊື່ user: '{0}' → '{1}' (ມີຜົນເມື່ອ Sign out)", currentUser, targetUser);
            confirm.Append("\n\nແທ້ບໍ່?");

            var dr = MessageBox.Show(confirm.ToString(), "ຢືນຢັນການປ່ຽນຊື່", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr != DialogResult.Yes) return;

            var result = new System.Text.StringBuilder();
            bool allOk = true;

            if (!computerDone)
            {
                string msg;
                bool ok = ComputerNameService.RenameComputer(target, out msg);
                result.AppendLine((ok ? "✓ " : "✗ ") + msg);
                allOk &= ok;
                if (ok && ConfigService.Instance.Current.selected != null)
                {
                    ConfigService.Instance.Current.selected.computerName = target;
                    ConfigService.Instance.Save();
                }
            }

            if (renameUser && allOk)
            {
                string userMsg;
                bool userOk = UserAccountService.RenameInteractiveUser(targetUser, out userMsg);
                result.AppendLine((userOk ? "✓ " : "✗ ") + userMsg);
                allOk &= userOk;
            }

            if (!computerDone && allOk)
            {
                var askRestart = MessageBox.Show(
                    result + "\nທ່ານຕ້ອງການ Restart ເຄື່ອງດຽວນີ້ເລີຍບໍ່?",
                    Localizer.T("app_title"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (askRestart == DialogResult.Yes)
                {
                    ComputerNameService.RestartComputer(3);
                }
            }
            else
            {
                MessageBox.Show(result.ToString(), Localizer.T("app_title"), MessageBoxButtons.OK,
                    allOk ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
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
                string room = cmbRooms.SelectedItem != null ? cmbRooms.SelectedItem.ToString() : "ຫ້ອງຄອມພິວເຕີ 1";
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
                string room = cmbRooms.SelectedItem != null ? cmbRooms.SelectedItem.ToString() : "ຫ້ອງຄອມພິວເຕີ 1";
                int number = (int)numComputerNumber.Value;

                string appliedPath;
                string error;
                bool ok = WallpaperModule.Instance.ApplyWallpaper(room, number, out appliedPath, out error);

                if (ok)
                {
                    if (ConfigService.Instance.Current.selected != null)
                    {
                        ConfigService.Instance.Current.selected.computerName = txtComputerName.Text.Trim().ToUpperInvariant();
                        ConfigService.Instance.Save();
                    }

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
            string room = cmbRooms.SelectedItem != null ? cmbRooms.SelectedItem.ToString() : "ຫ້ອງຄອມພິວເຕີ 1";
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
