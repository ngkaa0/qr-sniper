using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using QRSniper.Models;
using QRSniper.Services;

namespace QRSniper.UI
{
    public class QrResultDialog : Form
    {
        private readonly List<QrScanResult> _results;
        private Label _lblStatus;

        public QrResultDialog(List<QrScanResult> results, Icon appIcon)
        {
            _results = results ?? new List<QrScanResult>();
            if (appIcon != null)
            {
                Icon = appIcon;
            }

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = string.Format("QR Sniper v{0} - Scan Result", AppConfig.CurrentVersion);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            TopMost = true;
            BackColor = SystemColors.Window;
            Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            KeyPreview = true;

            KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    Close();
                }
            };

            BuildUnifiedLayout();
        }

        private void BuildUnifiedLayout()
        {
            int count = _results.Count;

            // Sizing: compact for 1 item, comfortably sized for multiple
            if (count <= 1)
            {
                ClientSize = new Size(560, 215);
                MinimumSize = new Size(480, 200);
            }
            else if (count == 2)
            {
                ClientSize = new Size(580, 310);
                MinimumSize = new Size(500, 260);
            }
            else
            {
                ClientSize = new Size(580, Math.Min(560, 150 + count * 88));
                MinimumSize = new Size(500, 320);
            }

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.Padding = new Padding(16, 14, 16, 12);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            Controls.Add(layout);

            // 1. Heading
            Label lblTitle = new Label();
            lblTitle.Text = count == 1
                ? "Detected 1 QR code on screen:"
                : string.Format("Detected {0} QR codes on screen:", count);
            lblTitle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            lblTitle.ForeColor = SystemColors.ControlText;
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Margin = new Padding(0);
            layout.Controls.Add(lblTitle, 0, 0);

            // 2. Scrollable list of cards
            Panel scrollContainer = new Panel();
            scrollContainer.Dock = DockStyle.Fill;
            scrollContainer.AutoScroll = true;

            TableLayoutPanel cardsTable = new TableLayoutPanel();
            cardsTable.Dock = DockStyle.Top;
            cardsTable.AutoSize = true;
            cardsTable.ColumnCount = 1;
            cardsTable.Padding = new Padding(0, 0, count > 2 ? 8 : 0, 0);

            Button primaryDefaultButton = null;

            if (count == 0)
            {
                QrScanResult emptyResult = new QrScanResult { Text = "" };
                Button btn;
                cardsTable.Controls.Add(CreateQrCard(1, emptyResult, out btn));
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    QrScanResult item = _results[i];
                    Button itemPrimaryButton;
                    Panel card = CreateQrCard(i + 1, item, out itemPrimaryButton);
                    if (i == 0)
                    {
                        primaryDefaultButton = itemPrimaryButton;
                    }
                    cardsTable.Controls.Add(card);
                }
            }

            if (count == 1 && primaryDefaultButton != null)
            {
                AcceptButton = primaryDefaultButton;
            }

            scrollContainer.Controls.Add(cardsTable);
            layout.Controls.Add(scrollContainer, 0, 1);

            // 3. Bottom Actions Row
            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Fill;

            _lblStatus = new Label();
            _lblStatus.Location = new Point(0, 10);
            _lblStatus.AutoSize = true;
            _lblStatus.ForeColor = Color.FromArgb(20, 130, 40);
            _lblStatus.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _lblStatus.Text = "";
            bottomPanel.Controls.Add(_lblStatus);

            FlowLayoutPanel buttonFlow = new FlowLayoutPanel();
            buttonFlow.Dock = DockStyle.Right;
            buttonFlow.FlowDirection = FlowDirection.LeftToRight;
            buttonFlow.AutoSize = true;
            buttonFlow.WrapContents = false;

            if (count > 1)
            {
                Button btnCopyAll = CreateNativeButton("Copy All (" + count + ")", 115);
                btnCopyAll.Click += delegate
                {
                    StringBuilder sb = new StringBuilder();
                    for (int j = 0; j < _results.Count; j++)
                    {
                        sb.AppendLine(_results[j].Text);
                        if (j < _results.Count - 1) sb.AppendLine("---");
                    }
                    SafeClipboard.SetText(sb.ToString());
                    ShowStatus("All codes copied");
                };
                buttonFlow.Controls.Add(btnCopyAll);
            }

            Button btnClose = CreateNativeButton("Close", 80);
            btnClose.Click += delegate { Close(); };
            buttonFlow.Controls.Add(btnClose);
            CancelButton = btnClose;

            bottomPanel.Controls.Add(buttonFlow);
            layout.Controls.Add(bottomPanel, 0, 2);
        }

        private Panel CreateQrCard(int index, QrScanResult result, out Button primaryButton)
        {
            primaryButton = null;

            Panel card = new Panel();
            card.Dock = DockStyle.Top;
            card.AutoSize = true;
            card.BackColor = SystemColors.Control;
            card.Margin = new Padding(0, 0, 0, 8);
            card.Padding = new Padding(12, 10, 12, 10);

            card.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen pen = new Pen(Color.FromArgb(215, 215, 215), 1f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            TableLayoutPanel rowLayout = new TableLayoutPanel();
            rowLayout.Dock = DockStyle.Top;
            rowLayout.AutoSize = true;
            rowLayout.ColumnCount = 2;
            rowLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            rowLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            // Left: Index + Text
            TableLayoutPanel contentCol = new TableLayoutPanel();
            contentCol.Dock = DockStyle.Fill;
            contentCol.AutoSize = true;
            contentCol.ColumnCount = 1;
            contentCol.RowCount = 2;
            contentCol.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            contentCol.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            string typeName = "Text";
            if (result.IsWifi) typeName = "Wi-Fi";
            else if (result.IsUrl) typeName = "URL";

            Label lblIndex = new Label();
            lblIndex.Text = string.Format("#{0} ({1})", index, typeName);
            lblIndex.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblIndex.ForeColor = SystemColors.ControlText;
            lblIndex.AutoSize = true;
            lblIndex.Margin = new Padding(0, 0, 0, 4);
            contentCol.Controls.Add(lblIndex, 0, 0);

            Label lblText = new Label();
            lblText.Dock = DockStyle.Top;
            lblText.AutoSize = true;
            lblText.Margin = new Padding(0);

            // Right: Action buttons
            FlowLayoutPanel actionFlow = new FlowLayoutPanel();
            actionFlow.Dock = DockStyle.Right;
            actionFlow.FlowDirection = FlowDirection.LeftToRight;
            actionFlow.AutoSize = true;
            actionFlow.WrapContents = false;
            actionFlow.Margin = new Padding(8, 0, 0, 0);

            if (result.IsWifi)
            {
                string ssid = string.IsNullOrEmpty(result.WifiSsid) ? "(Hidden)" : result.WifiSsid;
                string sec = string.IsNullOrEmpty(result.WifiType) ? "None" : result.WifiType;
                string pass = string.IsNullOrEmpty(result.WifiPassword) ? "(None)" : result.WifiPassword;

                lblText.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
                lblText.ForeColor = SystemColors.WindowText;
                lblText.Text = string.Format("SSID: {0}   |   Security: {1}\nPassword: {2}", ssid, sec, pass);

                if (!string.IsNullOrEmpty(result.WifiPassword))
                {
                    Button btnCopyPass = CreateNativeButton("Copy Pass", 86);
                    btnCopyPass.Height = 28;
                    btnCopyPass.Font = new Font("Segoe UI", 8.5f);
                    btnCopyPass.Click += delegate
                    {
                        SafeClipboard.SetText(result.WifiPassword);
                        ShowStatus(_results.Count == 1 ? "Password copied" : string.Format("Password #{0} copied", index));
                    };
                    actionFlow.Controls.Add(btnCopyPass);
                    primaryButton = btnCopyPass;
                }

                Button btnCopyRaw = CreateNativeButton("Copy", 72);
                btnCopyRaw.Height = 28;
                btnCopyRaw.Font = new Font("Segoe UI", 8.5f);
                btnCopyRaw.Click += delegate
                {
                    SafeClipboard.SetText(result.Text);
                    ShowStatus(_results.Count == 1 ? "Wi-Fi config copied" : string.Format("Copied #{0}", index));
                };
                actionFlow.Controls.Add(btnCopyRaw);
                if (primaryButton == null) primaryButton = btnCopyRaw;
            }
            else
            {
                lblText.Font = new Font(result.IsUrl ? "Segoe UI" : "Consolas", 9f, FontStyle.Regular);
                lblText.ForeColor = SystemColors.WindowText;
                lblText.Text = string.IsNullOrEmpty(result.Text) ? "(Empty)" : result.Text;

                Button btnCopyItem = CreateNativeButton("Copy", 72);
                btnCopyItem.Height = 28;
                btnCopyItem.Font = new Font("Segoe UI", 8.5f);
                btnCopyItem.Click += delegate
                {
                    SafeClipboard.SetText(result.Text);
                    ShowStatus(_results.Count == 1 ? "Copied to clipboard" : string.Format("Copied #{0}", index));
                };
                actionFlow.Controls.Add(btnCopyItem);
                primaryButton = btnCopyItem;

                if (result.IsUrl)
                {
                    Button btnOpenItem = CreateNativeButton("Open", 72);
                    btnOpenItem.Height = 28;
                    btnOpenItem.Font = new Font("Segoe UI", 8.5f);
                    btnOpenItem.Click += delegate
                    {
                        OpenUrl(result.Text);
                    };
                    actionFlow.Controls.Add(btnOpenItem);
                }
            }

            contentCol.Controls.Add(lblText, 0, 1);
            rowLayout.Controls.Add(contentCol, 0, 0);
            rowLayout.Controls.Add(actionFlow, 1, 0);
            card.Controls.Add(rowLayout);

            // Dynamic text wrapping based on available width
            card.SizeChanged += delegate
            {
                int availableWidth = card.ClientSize.Width - actionFlow.Width - card.Padding.Horizontal - 16;
                if (availableWidth > 120)
                {
                    lblText.MaximumSize = new Size(availableWidth, 0);
                }
            };

            return card;
        }

        private Button CreateNativeButton(string text, int width)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.UseVisualStyleBackColor = true;
            btn.Width = width;
            btn.Height = 28;
            btn.Font = new Font("Segoe UI", 9f);
            btn.Margin = new Padding(4, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private void OpenUrl(string text)
        {
            string url = text != null ? text.Trim() : "";
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = uri.ToString();
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not open URL: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowStatus(string msg)
        {
            _lblStatus.Text = msg;
            Timer timer = new Timer();
            timer.Interval = 2500;
            timer.Tick += delegate
            {
                _lblStatus.Text = "";
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }
    }
}
