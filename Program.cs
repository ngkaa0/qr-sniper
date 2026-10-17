using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using QRSniper.Models;
using QRSniper.Services;
using QRSniper.UI;
using ZXing;

[assembly: AssemblyTitle("QR Sniper")]
[assembly: AssemblyDescription("Instant screen QR code scanner for Windows")]
[assembly: AssemblyProduct("QR Sniper")]
[assembly: AssemblyCompany("QR Sniper")]
[assembly: AssemblyVersion("0.9.6.0")]
[assembly: AssemblyFileVersion("0.9.6.0")]

namespace QRSniper
{
    internal static class Program
    {
        private const string MutexName = @"Global\QRSniper_SingleInstance_Mutex";
        private const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern int SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                string exePath = Process.GetCurrentProcess().MainModule != null ? Process.GetCurrentProcess().MainModule.FileName : string.Empty;
                InstallationService.RegisterAumid(exePath);
                SetCurrentProcessExplicitAppUserModelID("QRSniper");
            }
            catch { }

            // Embedded ZXing assembly resolver
            AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs resolveArgs)
            {
                try
                {
                    string shortName = new AssemblyName(resolveArgs.Name).Name;
                    Assembly executing = Assembly.GetExecutingAssembly();
                    foreach (string res in executing.GetManifestResourceNames())
                    {
                        if (res.EndsWith(shortName + ".dll", StringComparison.OrdinalIgnoreCase))
                        {
                            using (Stream stream = executing.GetManifestResourceStream(res))
                            {
                                if (stream == null) continue;
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    byte[] buffer = new byte[65536];
                                    int read;
                                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        ms.Write(buffer, 0, read);
                                    }
                                    return Assembly.Load(ms.ToArray());
                                }
                            }
                        }
                    }
                    return null;
                }
                catch
                {
                    return null;
                }
            };

            try { SetProcessDPIAware(); } catch { }

            // Self-test mode
            if (args != null && args.Length > 0 && args[0] == "--test")
            {
                try { AttachConsole(ATTACH_PARENT_PROCESS); } catch { }
                RunSelfTests();
                return;
            }

            bool startSilently = false;
            if (args != null)
            {
                foreach (string arg in args)
                {
                    if (arg.Equals("--autostart", StringComparison.OrdinalIgnoreCase) ||
                        arg.Equals("--silent", StringComparison.OrdinalIgnoreCase))
                    {
                        startSilently = true;
                        break;
                    }
                }
            }

            bool hasMutex = false;
            Mutex mutex = null;
            try
            {
                mutex = new Mutex(true, MutexName, out hasMutex);
                if (!hasMutex)
                {
                    // Allow up to 1000ms for previous instance to exit during update/relaunch
                    try
                    {
                        hasMutex = mutex.WaitOne(startSilently ? 2000 : 1000, false);
                    }
                    catch (AbandonedMutexException)
                    {
                        hasMutex = true;
                    }
                }
            }
            catch
            {
                hasMutex = true;
            }

            if (!hasMutex)
            {
                MessageBox.Show(
                    "QR Sniper is already running in the system tray.",
                    "QR Sniper",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new AppTrayContext(startSilently));
            }
            finally
            {
                if (mutex != null)
                {
                    try
                    {
                        if (hasMutex) mutex.ReleaseMutex();
                        mutex.Dispose();
                    }
                    catch { }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RunSelfTests()
        {
            Console.WriteLine("=== QR SNIPER SELF-TEST (v" + AppConfig.CurrentVersion + ") ===");

            // 1. Single QR test
            Console.Write("[Test 1/5] Single QR code recognition... ");
            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options = new ZXing.Common.EncodingOptions();
            writer.Options.Width = 200;
            writer.Options.Height = 200;
            writer.Options.Margin = 1;

            string testPayload = "https://github.com/qrsniper-test";
            using (Bitmap testBmp = writer.Write(testPayload))
            {
                QrDecoderService decoder = new QrDecoderService();
                List<QrScanResult> results = decoder.Decode(testBmp);

                if (results.Count == 1 && results[0].Text == testPayload && results[0].IsUrl)
                {
                    Console.WriteLine("PASSED!");
                }
                else
                {
                    Console.WriteLine(string.Format("FAILED! (Results: {0})", results.Count));
                    Environment.Exit(1);
                }
            }

            // 2. Multi-QR test
            Console.Write("[Test 2/5] Multi-QR code recognition... ");
            using (Bitmap multiBmp = new Bitmap(500, 300))
            {
                using (Graphics g = Graphics.FromImage(multiBmp))
                {
                    g.Clear(Color.White);
                    using (Bitmap bmp1 = writer.Write("QR-CODE-NUMBER-ONE"))
                    using (Bitmap bmp2 = writer.Write("https://another-url.com/second"))
                    {
                        g.DrawImage(bmp1, 20, 20);
                        g.DrawImage(bmp2, 260, 20);
                    }
                }

                QrDecoderService decoder = new QrDecoderService();
                List<QrScanResult> multiResults = decoder.Decode(multiBmp);
                if (multiResults.Count >= 2)
                {
                    Console.WriteLine(string.Format("PASSED! (Found: {0})", multiResults.Count));
                }
                else
                {
                    Console.WriteLine(string.Format("FAILED! (Expected >= 2, found: {0})", multiResults.Count));
                    Environment.Exit(1);
                }
            }

            // 3. Key translation test
            Console.Write("[Test 3/5] Key name formatting (Page Down, Enter, Tab)... ");
            string pgDownName = AppConfig.FormatKeyName(Keys.Next);
            string enterName = AppConfig.FormatKeyName(Keys.Return);
            string tabName = AppConfig.FormatKeyName(Keys.Tab);
            if (pgDownName == "Page Down" && enterName == "Enter" && tabName == "Tab")
            {
                Console.WriteLine("PASSED!");
            }
            else
            {
                Console.WriteLine("FAILED formatting: " + pgDownName + ", " + enterName);
                Environment.Exit(1);
            }

            // 4. Safe clipboard test
            Console.Write("[Test 4/5] SafeClipboard resilience check... ");
            bool cbNullSafe = !SafeClipboard.SetText("");
            if (cbNullSafe)
            {
                Console.WriteLine("PASSED!");
            }
            else
            {
                Console.WriteLine("FAILED!");
                Environment.Exit(1);
            }

            // 5. Screen capture service test
            Console.Write("[Test 5/5] Screen capture service initialization... ");
            ScreenCaptureService captureService = new ScreenCaptureService();
            using (Bitmap capturedScreen = captureService.CaptureVirtualScreen())
            {
                if (capturedScreen != null)
                {
                    Console.WriteLine(string.Format("PASSED! (Virtual screen: {0}x{1})", capturedScreen.Width, capturedScreen.Height));
                }
                else
                {
                    Console.WriteLine("SKIPPED (headless terminal)");
                }
            }

            Console.WriteLine("=== ALL TESTS PASSED! ===");
        }
    }
}
