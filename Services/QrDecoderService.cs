using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using QRSniper.Models;
using ZXingCpp;

namespace QRSniper.Services
{
    public class QrDecoderService
    {
        private readonly ReaderOptions _options;

        public QrDecoderService()
        {
            _options = new ReaderOptions
            {
                TryHarder = true,
                TryRotate = true,
                TryInvert = true,
                TryDownscale = true,
                Binarizer = Binarizer.LocalAverage,
                Formats = BarcodeFormat.QRCode,
                MaxNumberOfSymbols = 20
            };
        }

        public List<QrScanResult> Decode(Bitmap bitmap)
        {
            List<QrScanResult> results = new List<QrScanResult>();
            if (bitmap == null) return results;

            try
            {
                BitmapData data = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb
                );

                try
                {
                    ImageView iv = new ImageView(data.Scan0, bitmap.Width, bitmap.Height, ZXingCpp.ImageFormat.BGRA, data.Stride, 4);
                    Barcode[] barcodes = BarcodeReader.Read(iv, _options);

                    if (barcodes != null && barcodes.Length > 0)
                    {
                        foreach (Barcode b in barcodes)
                        {
                            if (b != null && !string.IsNullOrEmpty(b.Text))
                            {
                                bool alreadyExists = false;
                                foreach (QrScanResult existing in results)
                                {
                                    if (existing.Text == b.Text)
                                    {
                                        alreadyExists = true;
                                        break;
                                    }
                                }

                                if (!alreadyExists)
                                {
                                    QrScanResult item = new QrScanResult
                                    {
                                        Text = b.Text,
                                        Format = b.Format.ToString(),
                                        Timestamp = DateTime.Now
                                    };
                                    results.Add(item);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    bitmap.UnlockBits(data);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[QrDecoderService] Scan error: " + ex.Message);
            }

            return results;
        }
    }
}
