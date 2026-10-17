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
    }
}
