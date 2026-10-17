using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QRSniper.Models;

namespace QRSniper.Services
{
    public class HotkeyManager : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_NOREPEAT = 0x4000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly HotkeyMessageWindow _window;
        private int _currentHotkeyId = 1;
        private bool _isRegistered = false;

        public event Action HotkeyPressed;

        public HotkeyManager()
        {
            _window = new HotkeyMessageWindow(this);
            _window.CreateHandle(new CreateParams());
        }

        public bool Register(AppConfig config)
        {
            Unregister();

            if (config == null || config.HotkeyKey == Keys.None)
            {
                return false;
            }

            uint modifiers = MOD_NOREPEAT;
            if (config.ModAlt) modifiers |= MOD_ALT;
            if (config.ModCtrl) modifiers |= MOD_CONTROL;
            if (config.ModShift) modifiers |= MOD_SHIFT;
            if (config.ModWin) modifiers |= MOD_WIN;

            uint vk = (uint)config.HotkeyKey;

            _currentHotkeyId++;
            _isRegistered = RegisterHotKey(_window.Handle, _currentHotkeyId, modifiers, vk);
            if (!_isRegistered && (modifiers & MOD_NOREPEAT) != 0)
            {
                _isRegistered = RegisterHotKey(_window.Handle, _currentHotkeyId, modifiers & ~MOD_NOREPEAT, vk);
            }
            return _isRegistered;
        }

        public void Unregister()
        {
            if (_isRegistered)
            {
                UnregisterHotKey(_window.Handle, _currentHotkeyId);
                _isRegistered = false;
            }
        }

        private void OnHotkeyPressed()
        {
            Action handler = HotkeyPressed;
            if (handler != null)
            {
                handler();
            }
        }

        public void Dispose()
        {
            Unregister();
            _window.DestroyHandle();
            GC.SuppressFinalize(this);
        }

        private class HotkeyMessageWindow : NativeWindow
        {
            private readonly HotkeyManager _manager;

            public HotkeyMessageWindow(HotkeyManager manager)
            {
                _manager = manager;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == _manager._currentHotkeyId)
                {
                    _manager.OnHotkeyPressed();
                }
                base.WndProc(ref m);
            }
        }
    }
}
