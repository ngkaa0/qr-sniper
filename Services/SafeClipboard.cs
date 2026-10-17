using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace QRSniper.Services
{
    /// <summary>
    /// Thread-safe and retry-resilient clipboard utility.
    /// Handles temporary OLE clipboard locks by other Windows processes.
    /// </summary>
    public static class SafeClipboard
    {
        public static bool SetText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    Clipboard.SetText(text);
                    return true;
                }
                catch (ExternalException)
                {
                    Thread.Sleep(35);
                }
                catch (Exception)
                {
                    break;
                }
            }

            return false;
        }
    }
}
