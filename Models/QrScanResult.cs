using System;

namespace QRSniper.Models
{
    public class QrScanResult
    {
        public string Text { get; set; }
        public string Format { get; set; }
        public DateTime Timestamp { get; set; }

        public QrScanResult()
        {
            Text = string.Empty;
            Format = "QR_CODE";
            Timestamp = DateTime.Now;
        }

        public bool IsUrl
        {
            get
            {
                if (string.IsNullOrEmpty(Text)) return false;
                Uri uriResult;
                return Uri.TryCreate(Text.Trim(), UriKind.Absolute, out uriResult)
                       && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            }
        }

        public bool IsWifi
        {
            get
            {
                return !string.IsNullOrEmpty(Text) && Text.Trim().StartsWith("WIFI:", StringComparison.OrdinalIgnoreCase);
            }
        }

        public string WifiSsid
        {
            get { return GetWifiField("S:"); }
        }

        public string WifiPassword
        {
            get { return GetWifiField("P:"); }
        }

        public string WifiType
        {
            get { return GetWifiField("T:"); }
        }

        public bool WifiHidden
        {
            get
            {
                string h = GetWifiField("H:");
                return string.Equals(h, "true", StringComparison.OrdinalIgnoreCase);
            }
        }

        private string GetWifiField(string prefix)
        {
            if (!IsWifi) return string.Empty;
            string raw = Text.Trim().Substring(5);
            int idx = raw.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return string.Empty;
            int start = idx + prefix.Length;
            int end = start;
            while (end < raw.Length)
            {
                if (raw[end] == ';')
                {
                    int backslashes = 0;
                    int b = end - 1;
                    while (b >= start && raw[b] == '\\')
                    {
                        backslashes++;
                        b--;
                    }
                    if (backslashes % 2 == 0)
                    {
                        break;
                    }
                }
                end++;
            }
            string val = raw.Substring(start, end - start);
            return val.Replace("\\;", ";")
                      .Replace("\\:", ":")
                      .Replace("\\\\", "\\")
                      .Replace("\\,", ",");
        }
    }
}
