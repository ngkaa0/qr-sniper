using System;
using System.Media;
using System.Windows.Forms;

namespace QRSniper.Services
{
    public class NotificationService
    {
        private readonly NotifyIcon _trayIcon;

        public NotificationService(NotifyIcon trayIcon)
        {
            _trayIcon = trayIcon;
        }

        public void NotifyQrFound(string text, bool playSound, bool copiedToClipboard)
        {
            if (playSound)
            {
                try { SystemSounds.Asterisk.Play(); } catch { }
            }

            string safeText = text ?? string.Empty;
            string preview = safeText.Length > 80 ? safeText.Substring(0, 77) + "..." : safeText;
            if (string.IsNullOrEmpty(preview)) preview = "(Empty QR content)";

            string title = copiedToClipboard ? "QR Code Copied" : "QR Code Detected";

            ShowTipSafe(3000, title, preview, ToolTipIcon.Info);
        }

        public void NotifyMultipleFound(int count, bool playSound)
        {
            if (playSound)
            {
                try { SystemSounds.Asterisk.Play(); } catch { }
            }

            ShowTipSafe(
                3500,
                "Multiple QR Codes Found",
                string.Format("Found {0} QR codes on screen. Result window opened.", count),
                ToolTipIcon.Info
            );
        }

        public void NotifyNotFound(bool playSound)
        {
            if (playSound)
            {
                try { SystemSounds.Exclamation.Play(); } catch { }
            }

            ShowTipSafe(
                2000,
                "No QR Code Found",
                "No readable QR code was detected on the screen.",
                ToolTipIcon.None
            );
        }

        public void NotifyInfo(string title, string message)
        {
            ShowTipSafe(3000, title, message, ToolTipIcon.None);
        }

        private void ShowTipSafe(int timeout, string title, string message, ToolTipIcon icon)
        {
            if (_trayIcon == null) return;

            try
            {
                _trayIcon.Visible = true;
                _trayIcon.ShowBalloonTip(timeout, title, message, icon);
            }
            catch
            {
                try
                {
                    _trayIcon.ShowBalloonTip(timeout, title, message, ToolTipIcon.None);
                }
                catch { }
            }
        }
    }
}
