using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using QRSniper.Models;
using QRSniper.Services;

namespace QRSniper.UI
{
    public class MainForm : Form
    {
        private readonly AppConfig _config;
        private readonly ConfigService _configService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly Action _triggerScanAction;

        private HotkeyControl _btnHotkeySelector;
        private CheckBox _chkCopyToClipboard;
        private CheckBox _chkPlaySound;
        private CheckBox _chkNotifications;
        private CheckBox _chkAutostart;
        private Label _lblLocationInfo;
        private Button _btnMigrate;
        private Button _btnOpenFolder;
        private Label _lblSaveStatus;

        private bool _tempModCtrl;
        private bool _tempModShift;
        private bool _tempModAlt;
        private bool _tempModWin;
        private Keys _tempKey;

        public MainForm(AppConfig config, ConfigService configService, HotkeyManager hotkeyManager, Action triggerScanAction, Icon appIcon)
        {
            _config = config;
            _configService = configService;
            _hotkeyManager = hotkeyManager;
            _triggerScanAction = triggerScanAction;

            if (appIcon != null)
            {
                Icon = appIcon;
            }

            _tempModCtrl = _config.ModCtrl;
            _tempModShift = _config.ModShift;
            _tempModAlt = _config.ModAlt;
            _tempModWin = _config.ModWin;
            _tempKey = _config.HotkeyKey;

            InitializeComponent();
            LoadConfigToUi();
        }

        private void InitializeComponent()
        {
            Text = string.Format("QR Sniper v{0} - Settings", AppConfig.CurrentVersion);
            ClientSize = new Size(470, 396);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = SystemColors.Window;
            Font = new Font("Segoe UI", 9f, FontStyle.Regular);

            // 1. Hotkey Group
            GroupBox gbHotkey = new GroupBox();
            gbHotkey.Text = " Shortcut ";
            gbHotkey.Location = new Point(16, 12);
            gbHotkey.Size = new Size(438, 78);
            gbHotkey.Font = new Font("Segoe UI", 9f);
            gbHotkey.ForeColor = SystemColors.ControlText;

            Label lblHotkeyHint = new Label();
            lblHotkeyHint.Text = "Click the field and press your key combination:";
            lblHotkeyHint.Location = new Point(14, 18);
            lblHotkeyHint.AutoSize = true;
            lblHotkeyHint.ForeColor = SystemColors.GrayText;
            lblHotkeyHint.Font = new Font("Segoe UI", 8.5f);
            gbHotkey.Controls.Add(lblHotkeyHint);

            _btnHotkeySelector = new HotkeyControl();
            _btnHotkeySelector.Location = new Point(14, 38);
            _btnHotkeySelector.Size = new Size(286, 28);
            _btnHotkeySelector.HotkeyChanged += OnHotkeyControlChanged;
            gbHotkey.Controls.Add(_btnHotkeySelector);

            Button btnResetHotkey = new Button();
            btnResetHotkey.Text = "Default";
            btnResetHotkey.Location = new Point(310, 38);
            btnResetHotkey.Size = new Size(114, 28);
            btnResetHotkey.UseVisualStyleBackColor = true;
            btnResetHotkey.Font = new Font("Segoe UI", 8.5f);
            btnResetHotkey.Cursor = NativeCursors.Hand;
            btnResetHotkey.Click += delegate
            {
                _tempModCtrl = true;
                _tempModShift = true;
                _tempModAlt = false;
                _tempModWin = false;
                _tempKey = Keys.Q;
                UpdateHotkeyDisplay();
            };
            gbHotkey.Controls.Add(btnResetHotkey);
            Controls.Add(gbHotkey);

            // 2. Options Group
            GroupBox gbOptions = new GroupBox();
            gbOptions.Text = " Options ";
            gbOptions.Location = new Point(16, 98);
            gbOptions.Size = new Size(438, 140);
            gbOptions.Font = new Font("Segoe UI", 9f);
            gbOptions.ForeColor = SystemColors.ControlText;

            _chkCopyToClipboard = CreateCheckBox("Copy to clipboard (skip result window)", 22);
            _chkPlaySound = CreateCheckBox("Play sound notification when a QR code is found", 50);
            _chkNotifications = CreateCheckBox("Show system notifications in tray", 78);
            _chkAutostart = CreateCheckBox("Start QR Sniper automatically on Windows logon", 106);

            gbOptions.Controls.Add(_chkCopyToClipboard);
            gbOptions.Controls.Add(_chkPlaySound);
            gbOptions.Controls.Add(_chkNotifications);
            gbOptions.Controls.Add(_chkAutostart);
            Controls.Add(gbOptions);

            // 3. Location & Installation Group
            GroupBox gbLocation = new GroupBox();
            gbLocation.Text = " Location && Installation ";
            gbLocation.Location = new Point(16, 246);
            gbLocation.Size = new Size(438, 86);
            gbLocation.Font = new Font("Segoe UI", 9f);
            gbLocation.ForeColor = SystemColors.ControlText;

            _lblLocationInfo = new Label();
            _lblLocationInfo.Location = new Point(14, 20);
            _lblLocationInfo.Size = new Size(410, 20);
            _lblLocationInfo.AutoEllipsis = true;
            _lblLocationInfo.ForeColor = SystemColors.GrayText;
            _lblLocationInfo.Font = new Font("Segoe UI", 8.5f);
            _lblLocationInfo.Text = "Path:";
            gbLocation.Controls.Add(_lblLocationInfo);

            _btnMigrate = new Button();
            _btnMigrate.Text = "Install to System (Start Menu)";
            _btnMigrate.Location = new Point(14, 46);
            _btnMigrate.Size = new Size(260, 28);
            _btnMigrate.UseVisualStyleBackColor = true;
            _btnMigrate.Font = new Font("Segoe UI", 8.5f);
            _btnMigrate.Cursor = NativeCursors.Hand;
            _btnMigrate.Click += delegate
            {
                if (MessageBox.Show(
                    "Install QR Sniper to your local programs folder (%LOCALAPPDATA%\\Programs\\QRSniper) and create a Start Menu shortcut?\n\nYou can then safely remove the downloaded file.",
                    "QR Sniper Installation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    InstallationService.InstallAndRelaunch();
                }
            };
            gbLocation.Controls.Add(_btnMigrate);

            _btnOpenFolder = new Button();
            _btnOpenFolder.Text = "Open Folder";
            _btnOpenFolder.Location = new Point(284, 46);
            _btnOpenFolder.Size = new Size(140, 28);
            _btnOpenFolder.UseVisualStyleBackColor = true;
            _btnOpenFolder.Cursor = NativeCursors.Hand;
            _btnOpenFolder.Font = new Font("Segoe UI", 8.5f);
            _btnOpenFolder.Click += delegate
            {
                string dir = AppDomain.CurrentDomain.BaseDirectory;
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = dir;
                psi.UseShellExecute = true;
                Process.Start(psi);
            };
            gbLocation.Controls.Add(_btnOpenFolder);
            Controls.Add(gbLocation);

            // 4. Bottom Action Row
            Panel pnlBottom = new Panel();
            pnlBottom.Location = new Point(16, 340);
            pnlBottom.Size = new Size(438, 48);

            _lblSaveStatus = new Label();
            _lblSaveStatus.Location = new Point(0, 14);
            _lblSaveStatus.AutoSize = true;
            _lblSaveStatus.ForeColor = Color.FromArgb(22, 130, 60);
            _lblSaveStatus.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _lblSaveStatus.Text = "";
            pnlBottom.Controls.Add(_lblSaveStatus);

            Button btnScanNow = new Button();
            btnScanNow.Text = "Test Scan";
            btnScanNow.Location = new Point(166, 8);
            btnScanNow.Size = new Size(120, 32);
            btnScanNow.UseVisualStyleBackColor = true;
            btnScanNow.Font = new Font("Segoe UI", 9f);
            btnScanNow.Cursor = NativeCursors.Hand;
            btnScanNow.Click += delegate
            {
                _triggerScanAction();
            };
            pnlBottom.Controls.Add(btnScanNow);

            Button btnSave = new Button();
            btnSave.Text = "Save to Tray";
            btnSave.Location = new Point(296, 8);
            btnSave.Size = new Size(142, 32);
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnSave.Cursor = NativeCursors.Hand;
            btnSave.Click += BtnSave_Click;
            pnlBottom.Controls.Add(btnSave);

            Controls.Add(pnlBottom);

            FormClosing += MainForm_FormClosing;
        }

        private CheckBox CreateCheckBox(string text, int top)
        {
            CheckBox cb = new CheckBox();
            cb.Text = text;
            cb.Location = new Point(14, top);
            cb.AutoSize = true;
            cb.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            cb.ForeColor = SystemColors.ControlText;
            cb.Cursor = NativeCursors.Hand;
            return cb;
        }

        private void LoadConfigToUi()
        {
            _chkCopyToClipboard.Checked = _config.CopyToClipboard;
            _chkPlaySound.Checked = _config.PlaySound;
            _chkNotifications.Checked = _config.ShowNotification;
            _chkAutostart.Checked = InstallationService.IsAutostartEnabled();

            UpdateHotkeyDisplay();

            Process currentProc = Process.GetCurrentProcess();
            string currentExe = currentProc.MainModule != null ? currentProc.MainModule.FileName : AppDomain.CurrentDomain.BaseDirectory;
            bool isInstalled = InstallationService.IsInstalled();
            bool isDownloads = InstallationService.IsRunningFromDownloadsOrTemp();

            if (isInstalled)
            {
                _lblLocationInfo.Text = "Installed in system (%LOCALAPPDATA%)";
                _lblLocationInfo.ForeColor = Color.FromArgb(20, 130, 40);
                _btnMigrate.Visible = false;

                _btnOpenFolder.Location = new Point(14, 46);
                _btnOpenFolder.Size = new Size(160, 28);
            }
            else
            {
                if (isDownloads)
                {
                    _lblLocationInfo.Text = "Running from Downloads (installing to system is recommended)";
                    _lblLocationInfo.ForeColor = Color.FromArgb(190, 80, 0);
                }
                else
                {
                    _lblLocationInfo.Text = "Portable mode: " + currentExe;
                    _lblLocationInfo.ForeColor = SystemColors.GrayText;
                }

                _btnMigrate.Text = "Install to System (Start Menu)";
                _btnMigrate.Location = new Point(14, 46);
                _btnMigrate.Size = new Size(260, 28);
                _btnMigrate.Visible = true;

                _btnOpenFolder.Location = new Point(284, 46);
                _btnOpenFolder.Size = new Size(140, 28);
            }
        }

        private void UpdateHotkeyDisplay()
        {
            _btnHotkeySelector.SetHotkey(_tempModCtrl, _tempModAlt, _tempModShift, _tempModWin, _tempKey);
        }

        private void OnHotkeyControlChanged(bool ctrl, bool alt, bool shift, bool win, Keys key)
        {
            _tempModCtrl = ctrl;
            _tempModAlt = alt;
            _tempModShift = shift;
            _tempModWin = win;
            _tempKey = key;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_tempKey == Keys.None)
            {
                MessageBox.Show("Please specify a valid shortcut key!", "Shortcut Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isTypingKey = (_tempKey >= Keys.A && _tempKey <= Keys.Z) ||
                               (_tempKey >= Keys.D0 && _tempKey <= Keys.D9) ||
                               _tempKey == Keys.Space || _tempKey == Keys.Enter ||
                               _tempKey == Keys.Back || _tempKey == Keys.Tab || _tempKey == Keys.Escape;
            bool hasModifier = _tempModCtrl || _tempModAlt || _tempModShift || _tempModWin;

            if (!hasModifier && isTypingKey)
            {
                MessageBox.Show(
                    "Alphanumeric typing keys (A-Z, 0-9, Space, Enter) require at least one modifier key (Ctrl, Alt, Shift, or Win) so standard typing is not blocked.\n\nNavigation and function keys (Page Down, Page Up, F1-F12, etc.) can be used freely without modifiers.",
                    "Shortcut Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            _config.ModCtrl = _tempModCtrl;
            _config.ModShift = _tempModShift;
            _config.ModAlt = _tempModAlt;
            _config.ModWin = _tempModWin;
            _config.HotkeyKey = _tempKey;

            _config.CopyToClipboard = _chkCopyToClipboard.Checked;
            _config.PlaySound = _chkPlaySound.Checked;
            _config.ShowNotification = _chkNotifications.Checked;
            _config.StartWithWindows = _chkAutostart.Checked;

            InstallationService.SetAutostart(_config.StartWithWindows);
            _configService.SaveConfig(_config);

            bool registered = _hotkeyManager.Register(_config);
            if (!registered)
            {
                MessageBox.Show(
                    string.Format("Could not register shortcut {0}.\nIt might already be in use by another application.", _config.GetHotkeyDisplayString()),
                    "Shortcut Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            Hide();
            AppTrayContext.TrimMemory();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                AppTrayContext.TrimMemory();
            }
        }
    }

    /// <summary>
    /// Clean, native-feeling hotkey selector without text carets or artificial colors.
    /// </summary>
    public class HotkeyControl : Control
    {
        private bool _isListening = false;
        private bool _modCtrl = false;
        private bool _modAlt = false;
        private bool _modShift = false;
        private bool _modWin = false;
        private Keys _key = Keys.None;

        public delegate void HotkeyChangedHandler(bool ctrl, bool alt, bool shift, bool win, Keys key);
        public event HotkeyChangedHandler HotkeyChanged;

        public HotkeyControl()
        {
            SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Cursor = NativeCursors.Hand;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            BackColor = SystemColors.Window;
        }

        public void SetHotkey(bool ctrl, bool alt, bool shift, bool win, Keys key)
        {
            _modCtrl = ctrl;
            _modAlt = alt;
            _modShift = shift;
            _modWin = win;
            _key = key;
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Focus();
            _isListening = true;
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            _isListening = false;
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!_isListening) return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            // Bare Escape cancels listening without changing hotkey
            if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt)
            {
                _isListening = false;
                Invalidate();
                return;
            }

            // Ignore bare modifier presses
            if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey ||
                e.KeyCode == Keys.Menu || e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
            {
                return;
            }

            _modCtrl = e.Control;
            _modShift = e.Shift;
            _modAlt = e.Alt;
            _modWin = (ModifierKeys & Keys.RWin) != 0 || (ModifierKeys & Keys.LWin) != 0;
            _key = e.KeyCode;

            _isListening = false;
            Invalidate();

            HotkeyChangedHandler handler = HotkeyChanged;
            if (handler != null)
            {
                handler(_modCtrl, _modAlt, _modShift, _modWin, _key);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.Clear(BackColor);

            Color borderColor = _isListening ? SystemColors.Highlight : SystemColors.ActiveBorder;
            using (Pen pen = new Pen(borderColor, 1f))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }

            string text;
            Color textColor;

            if (_isListening)
            {
                text = "Press shortcut keys...";
                textColor = SystemColors.Highlight;
            }
            else
            {
                List<string> parts = new List<string>();
                if (_modCtrl) parts.Add("Ctrl");
                if (_modAlt) parts.Add("Alt");
                if (_modShift) parts.Add("Shift");
                if (_modWin) parts.Add("Win");
                if (_key != Keys.None) parts.Add(AppConfig.FormatKeyName(_key));

                text = parts.Count > 0 ? string.Join(" + ", parts.ToArray()) : "None";
                textColor = SystemColors.WindowText;
            }

            TextRenderer.DrawText(
                g,
                text,
                _isListening ? new Font(Font, FontStyle.Italic) : new Font(Font, FontStyle.Bold),
                ClientRectangle,
                textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
            );
        }
    }
}
