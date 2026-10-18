using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QRSniper.UI
{
    internal static class NativeCursors
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        private const int IDC_HAND = 32649;
        private static Cursor _hand;

        public static Cursor Hand
        {
            get
            {
                if (_hand == null)
                {
                    try
                    {
                        IntPtr handle = LoadCursor(IntPtr.Zero, IDC_HAND);
                        if (handle != IntPtr.Zero)
                        {
                            _hand = new Cursor(handle);
                        }
                    }
                    catch
                    {
                    }

                    if (_hand == null)
                    {
                        _hand = Cursors.Hand;
                    }
                }
                return _hand;
            }
        }
    }
}
