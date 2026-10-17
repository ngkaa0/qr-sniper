using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QRSniper.Models;
using QRSniper.Services;

namespace QRSniper.UI
{
    public class AppTrayContext : ApplicationContext
    {
        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        private readonly AppConfig _config;
        private readonly ConfigService _configService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly ScreenCaptureService _captureService;
        private readonly QrDecoderService _decoderService;
        private readonly NotificationService _notificationService;

        private readonly Icon _formIcon;
        private readonly Icon _trayIconExact;
        private readonly NotifyIcon _trayIcon;
        private MainForm _settingsForm;

        public AppTrayContext(bool startSilently)
        {
            _configService = new ConfigService();
            _config = _configService.LoadConfig();

            _captureService = new ScreenCaptureService();
            _decoderService = new QrDecoderService();

            _formIcon = LoadFullFormIcon();
            _trayIconExact = LoadCrispTrayIcon();

            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = _trayIconExact;
            _trayIcon.Text = string.Format("QR Sniper v{0}", AppConfig.CurrentVersion);
            _trayIcon.Visible = true;

            _notificationService = new NotificationService(_trayIcon);

            _hotkeyManager = new HotkeyManager();
            _hotkeyManager.HotkeyPressed += OnHotkeyPressed;

            InitializeContextMenu();

            bool registered = _hotkeyManager.Register(_config);
            if (!registered)
            {
                _notificationService.NotifyInfo(
                    "QR Sniper",
                    string.Format("Failed to register shortcut {0}. Open Settings to change it.", _config.GetHotkeyDisplayString())
                );
            }

            _trayIcon.DoubleClick += delegate { ShowSettings(); };

            if (CheckDownloadsFolderPrompt())
            {
                return;
            }

            // Если запуск ручной — сразу открываем окно настроек
            if (!startSilently)
            {
                ShowSettings();
            }

            TrimMemory();
        }

        public static void TrimMemory()
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            }
            catch { }
        }

        private void InitializeContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem scanItem = new ToolStripMenuItem(
                string.Format("Scan Screen ({0})", _config.GetHotkeyDisplayString()),
                null,
                delegate { ExecuteScan(); }
            );
            scanItem.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            contextMenu.Items.Add(scanItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem settingsItem = new ToolStripMenuItem("Settings...", null, delegate { ShowSettings(); });
            contextMenu.Items.Add(settingsItem);

            ToolStripMenuItem aboutItem = new ToolStripMenuItem(
                "About...",
                null,
                delegate
                {
                    MessageBox.Show(
                        string.Format("QR Sniper v{0}\n\n" +
                                      "Author: ngkaa (github.com/ngkaa0)\n" +
                                      "Built with Gemini in Google Antigravity\n\n" +
                                      "Shortcut: {1}\n" +
                                      "RAM usage: ~3-5 MB\n" +
                                      "Idle CPU: 0.0%",
                                      AppConfig.CurrentVersion, _config.GetHotkeyDisplayString()),
                        "About QR Sniper",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            );
            contextMenu.Items.Add(aboutItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit", null, delegate { ExitApplication(); });
            contextMenu.Items.Add(exitItem);

            _trayIcon.ContextMenuStrip = contextMenu;
        }

        private bool CheckDownloadsFolderPrompt()
        {
            if (InstallationService.IsRunningFromDownloadsOrTemp())
            {
                DialogResult result = MessageBox.Show(
                    "QR Sniper is running from your Downloads or temporary folder.\n\n" +
                    "It is recommended to install the app to your local Programs folder (%LOCALAPPDATA%\\Programs\\QRSniper) and create a Start Menu shortcut.\n\n" +
                    "Install to system now?",
                    "QR Sniper - Smart Portable",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    InstallationService.InstallAndRelaunch();
                    return true;
                }
            }
            return false;
        }

        private void OnHotkeyPressed()
        {
            ExecuteScan();
        }

        public void ExecuteScan()
        {
            try
            {
                using (Bitmap screenBitmap = _captureService.CaptureVirtualScreen())
                {
                    if (screenBitmap == null)
                    {
                        if (_config.ShowNotification)
                        {
                            _notificationService.NotifyInfo("QR Sniper", "Screen capture failed (screen may be locked).");
                        }
                        return;
                    }

                    List<QrScanResult> results = _decoderService.Decode(screenBitmap);

                    if (results.Count == 0)
                    {
                        if (_config.ShowNotification)
                        {
                            _notificationService.NotifyNotFound(_config.PlaySound);
                        }
                    }
                    else if (results.Count == 1)
                    {
                        QrScanResult result = results[0];

                        if (_config.CopyToClipboard)
                        {
                            SafeClipboard.SetText(result.Text);

                            if (_config.ShowNotification)
                            {
                                _notificationService.NotifyQrFound(result.Text, _config.PlaySound, true);
                            }
                            else if (_config.PlaySound)
                            {
                                try { System.Media.SystemSounds.Asterisk.Play(); } catch { }
                            }
                        }
                        else
                        {
                            if (_config.PlaySound)
                            {
                                try { System.Media.SystemSounds.Asterisk.Play(); } catch { }
                            }

                            QrResultDialog dialog = new QrResultDialog(results, _formIcon);
                            dialog.ShowDialog();
                        }
                    }
                    else
                    {
                        if (_config.ShowNotification)
                        {
                            _notificationService.NotifyMultipleFound(results.Count, _config.PlaySound);
                        }

                        QrResultDialog dialog = new QrResultDialog(results, _formIcon);
                        dialog.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[AppTrayContext] Scan error: " + ex.Message);
            }
            finally
            {
                TrimMemory();
            }
        }

        private void ShowSettings()
        {
            if (_settingsForm == null || _settingsForm.IsDisposed)
            {
                _settingsForm = new MainForm(_config, _configService, _hotkeyManager, ExecuteScan, _formIcon);
            }

            _settingsForm.Show();
            _settingsForm.BringToFront();
            _settingsForm.Activate();

            if (_trayIcon.ContextMenuStrip != null && _trayIcon.ContextMenuStrip.Items.Count > 0)
            {
                ToolStripMenuItem scanItem = _trayIcon.ContextMenuStrip.Items[0] as ToolStripMenuItem;
                if (scanItem != null)
                {
                    scanItem.Text = string.Format("Scan Screen ({0})", _config.GetHotkeyDisplayString());
                }
            }
        }

        private void ExitApplication()
        {
            _hotkeyManager.Dispose();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            ExitThread();
        }

        private static Stream GetEmbeddedIconStream(string filename)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream s = asm.GetManifestResourceStream("QRSniper.Resources." + filename);
            if (s != null) return s;
            return asm.GetManifestResourceStream(filename);
        }

        /// <summary>
        /// Загружает полноразмерную иконку (со всеми слоями 16, 24, 32, 48, 64) для окон и таскбара.
        /// Гарантирует четкость в панели задач Windows даже если .exe запущен отдельно на Рабочем столе.
        /// </summary>
        private static Icon LoadFullFormIcon()
        {
            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string icoPath = Path.Combine(appDir, "app.ico");
                if (File.Exists(icoPath))
                {
                    return new Icon(icoPath);
                }

                using (Stream stream = GetEmbeddedIconStream("app.ico"))
                {
                    if (stream != null)
                    {
                        return new Icon(stream);
                    }
                }
            }
            catch { }

            return SystemIcons.Application;
        }

        private static Icon LoadCrispTrayIcon()
        {
            try
            {
                Size sz = SystemInformation.SmallIconSize;
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string icoPath = Path.Combine(appDir, "app.ico");
                if (File.Exists(icoPath))
                {
                    return new Icon(icoPath, sz);
                }

                using (Stream stream = GetEmbeddedIconStream("app.ico"))
                {
                    if (stream != null)
                    {
                        return new Icon(stream, sz);
                    }
                }
            }
            catch { }

            return SystemIcons.Application;
        }
    }
}
