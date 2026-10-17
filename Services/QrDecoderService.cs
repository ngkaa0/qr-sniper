using System;
using System.Collections.Generic;
using System.Drawing;
using QRSniper.Models;
using ZXing;
using ZXing.Common;

namespace QRSniper.Services
{
    public class QrDecoderService
    {
        private readonly BarcodeReader _reader;

        public QrDecoderService()
        {
            _reader = new BarcodeReader();
            _reader.AutoRotate = true;
            _reader.Options = new DecodingOptions();
            _reader.Options.TryHarder = true;
            _reader.Options.TryInverted = true;
            _reader.Options.PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE };
        }

        public List<QrScanResult> Decode(Bitmap bitmap)
        {
            List<QrScanResult> results = new List<QrScanResult>();
            if (bitmap == null) return results;

            try
            {
                // Множественное распознавание
                Result[] multipleResults = _reader.DecodeMultiple(bitmap);
                if (multipleResults != null && multipleResults.Length > 0)
                {
                    foreach (Result res in multipleResults)
                    {
                        if (res != null && !string.IsNullOrEmpty(res.Text))
                        {
                            bool alreadyExists = false;
                            foreach (QrScanResult existing in results)
                            {
                                if (existing.Text == res.Text)
                                {
                                    alreadyExists = true;
                                    break;
                                }
                            }

                            if (!alreadyExists)
                            {
                                QrScanResult item = new QrScanResult();
                                item.Text = res.Text;
                                item.Format = res.BarcodeFormat.ToString();
                                item.Timestamp = DateTime.Now;
                                results.Add(item);
                            }
                        }
                    }
                }

                // Одиночный декод как резервный вариант
                if (results.Count == 0)
                {
                    Result singleResult = _reader.Decode(bitmap);
                    if (singleResult != null && !string.IsNullOrEmpty(singleResult.Text))
                    {
                        QrScanResult item = new QrScanResult();
                        item.Text = singleResult.Text;
                        item.Format = singleResult.BarcodeFormat.ToString();
                        item.Timestamp = DateTime.Now;
                        results.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[QrDecoderService] Ошибка распознавания: " + ex.Message);
            }

            return results;
        }
    }
}
