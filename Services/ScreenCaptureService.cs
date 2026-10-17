using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace QRSniper.Services
{
    public class ScreenCaptureService
    {
        /// <summary>
        /// Делает мгновенный снимок всего виртуального экрана (включая все подключенные мониторы).
        /// Возвращает null, если рабочий стол временно заблокирован или недоступен.
        /// </summary>
        public Bitmap CaptureVirtualScreen()
        {
            try
            {
                Rectangle bounds = SystemInformation.VirtualScreen;
                if (bounds.Width <= 0 || bounds.Height <= 0)
                {
                    Screen primary = Screen.PrimaryScreen;
                    bounds = primary != null ? primary.Bounds : new Rectangle(0, 0, 1920, 1080);
                }

                Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(
                        bounds.Left,
                        bounds.Top,
                        0,
                        0,
                        bounds.Size,
                        CopyPixelOperation.SourceCopy
                    );
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ScreenCaptureService] Ошибка захвата: " + ex.Message);
                return null;
            }
        }
    }
}
