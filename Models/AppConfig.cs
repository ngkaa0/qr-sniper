using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QRSniper.Models
{
    public class AppConfig
    {
        public const string CurrentVersion = "0.9.6";

        public string Version { get; set; }

        public bool ModCtrl { get; set; }
        public bool ModShift { get; set; }
        public bool ModAlt { get; set; }
        public bool ModWin { get; set; }
        public Keys HotkeyKey { get; set; }

        public bool CopyToClipboard { get; set; }
        public bool AutoCopyClipboard { get { return CopyToClipboard; } set { CopyToClipboard = value; } }
        public bool PlaySound { get; set; }
        public bool ShowNotification { get; set; }
        public bool StartWithWindows { get; set; }

        public AppConfig()
        {
            Version = CurrentVersion;
            ModCtrl = true;
            ModShift = true;
            ModAlt = false;
            ModWin = false;
            HotkeyKey = Keys.Q;

            CopyToClipboard = false;
            PlaySound = true;
            ShowNotification = true;
            StartWithWindows = false;
        }

        public string GetHotkeyDisplayString()
        {
            List<string> parts = new List<string>();
            if (ModCtrl) parts.Add("Ctrl");
            if (ModAlt) parts.Add("Alt");
            if (ModShift) parts.Add("Shift");
            if (ModWin) parts.Add("Win");
            parts.Add(FormatKeyName(HotkeyKey));
            return string.Join(" + ", parts.ToArray());
        }

        public static string FormatKeyName(Keys key)
        {
            switch (key)
            {
                case Keys.Next: return "Page Down";
                case Keys.Prior: return "Page Up";
                case Keys.Return: return "Enter";
                case Keys.Back: return "Backspace";
                case Keys.Capital: return "Caps Lock";
                case Keys.Escape: return "Esc";
                case Keys.Space: return "Space";
                case Keys.Tab: return "Tab";
                case Keys.Up: return "Up";
                case Keys.Down: return "Down";
                case Keys.Left: return "Left";
                case Keys.Right: return "Right";
                case Keys.Delete: return "Delete";
                case Keys.Insert: return "Insert";
                case Keys.Home: return "Home";
                case Keys.End: return "End";
                case Keys.D0: return "0";
                case Keys.D1: return "1";
                case Keys.D2: return "2";
                case Keys.D3: return "3";
                case Keys.D4: return "4";
                case Keys.D5: return "5";
                case Keys.D6: return "6";
                case Keys.D7: return "7";
                case Keys.D8: return "8";
                case Keys.D9: return "9";
                case Keys.NumPad0: return "Num 0";
                case Keys.NumPad1: return "Num 1";
                case Keys.NumPad2: return "Num 2";
                case Keys.NumPad3: return "Num 3";
                case Keys.NumPad4: return "Num 4";
                case Keys.NumPad5: return "Num 5";
                case Keys.NumPad6: return "Num 6";
                case Keys.NumPad7: return "Num 7";
                case Keys.NumPad8: return "Num 8";
                case Keys.NumPad9: return "Num 9";
                case Keys.OemQuestion: return "/";
                case Keys.OemMinus: return "-";
                case Keys.Oemplus: return "+";
                case Keys.Oemtilde: return "~";
                case Keys.OemOpenBrackets: return "[";
                case Keys.OemCloseBrackets: return "]";
                case Keys.Oem1: return ";";
                case Keys.Oem7: return "'";
                case Keys.Oemcomma: return ",";
                case Keys.OemPeriod: return ".";
                case Keys.Snapshot: return "Print Screen";
                case Keys.Scroll: return "Scroll Lock";
                case Keys.Pause: return "Pause";
                default:
                    return key.ToString();
            }
        }
    }
}
