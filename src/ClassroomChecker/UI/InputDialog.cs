using System;
using System.Drawing;
using System.Windows.Forms;
using ClassroomChecker.I18n;

namespace ClassroomChecker.UI
{
    /// <summary>
    /// Dialog ສຳລັບປ້ອນຂໍ້ຄວາມ (ເຊັ່ນ: ຊື່ຫ້ອງຮຽນໃໝ່)
    /// </summary>
    public class InputDialog : Form
    {
        private Label lblPrompt;
        private TextBox txtInput;
        private Button btnOk;
        private Button btnCancel;

        public string InputText
        {
            get { return txtInput.Text.Trim(); }
        }

        public InputDialog(string title, string prompt, string defaultValue = "")
        {
            InitializeComponent(title, prompt, defaultValue);
        }

        private void InitializeComponent(string title, string prompt, string defaultValue)
        {
            this.Text = title;
            this.Size = new Size(420, 190);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.Font = new Font("Leelawadee UI", 9.5F, FontStyle.Regular);

            lblPrompt = new Label
            {
                Text = prompt,
                Location = new Point(20, 18),
                Size = new Size(365, 38),
                Font = new Font("Leelawadee UI", 9.5F, FontStyle.Regular)
            };

            txtInput = new TextBox
            {
                Text = defaultValue,
                Location = new Point(20, 60),
                Size = new Size(365, 25),
                Font = new Font("Leelawadee UI", 10F, FontStyle.Regular)
            };
            txtInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    btnOk.PerformClick();
                    e.Handled = true;
                }
            };

            btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(210, 105),
                Size = new Size(85, 32),
                BackColor = Color.FromArgb(26, 115, 232),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(300, 105),
                Size = new Size(85, 32)
            };

            this.Controls.Add(lblPrompt);
            this.Controls.Add(txtInput);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }
}
