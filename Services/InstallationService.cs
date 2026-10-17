using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace QRSniper.Services
{
    public class InstallationService
    {
        private const string AppName = "QRSniper";
        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static string GetInstalledDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs",
                AppName
            );
        }

        public static string GetInstalledExePath()
        {
            return Path.Combine(GetInstalledDirectory(), "QRSniper.exe");
        }

        public static string GetActualDownloadsFolder()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("{374DE290-123F-4565-9164-39C4925E467B}") ?? key.GetValue("{7D83EE9B-2244-4E70-B1F5-539B82A6027C}");
                        if (val != null)
                        {
                            string path = Environment.ExpandEnvironmentVariables(val.ToString());
                            if (!string.IsNullOrEmpty(path))
                            {
                                return path;
                            }
                        }
                    }
                }
            }
            catch { }

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("{374DE290-123F-4565-9164-39C4925E467B}");
                        if (val != null)
                        {
                            string path = Environment.ExpandEnvironmentVariables(val.ToString());
                            if (!string.IsNullOrEmpty(path))
                            {
                                return path;
                            }
                        }
                    }
                }
            }
            catch { }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }

        public static bool IsInstalled()
        {
            try
            {
                Process currentProc = Process.GetCurrentProcess();
                string currentExe = currentProc.MainModule != null ? currentProc.MainModule.FileName : string.Empty;
                if (string.IsNullOrEmpty(currentExe)) return false;

                string installedDir = GetInstalledDirectory().TrimEnd('\\', '/');
                return currentExe.StartsWith(installedDir, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsRunningFromDownloadsOrTemp()
        {
            try
            {
                Process currentProc = Process.GetCurrentProcess();
                string currentExe = currentProc.MainModule != null ? currentProc.MainModule.FileName : string.Empty;
                if (string.IsNullOrEmpty(currentExe)) return false;

                string downloadsDir = GetActualDownloadsFolder().TrimEnd('\\', '/');
                string tempDir = Path.GetTempPath().TrimEnd('\\', '/');
                string defaultDownloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads").TrimEnd('\\', '/');

                if (currentExe.StartsWith(downloadsDir, StringComparison.OrdinalIgnoreCase) ||
                    currentExe.StartsWith(tempDir, StringComparison.OrdinalIgnoreCase) ||
                    currentExe.StartsWith(defaultDownloads, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Проверяем наличие папки Downloads, Загрузки или Temp в любом звене пути
                string currentDir = Path.GetDirectoryName(currentExe) ?? string.Empty;
                string[] dirParts = currentDir.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                foreach (string part in dirParts)
                {
                    if (part.Equals("Downloads", StringComparison.OrdinalIgnoreCase) ||
                        part.Equals("Загрузки", StringComparison.OrdinalIgnoreCase) ||
                        part.Equals("Temp", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool SetAutostart(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true))
                {
                    if (key == null) return false;

                    if (enable)
                    {
                        Process currentProc = Process.GetCurrentProcess();
                        string exePath = currentProc.MainModule != null ? currentProc.MainModule.FileName : string.Empty;
                        if (!string.IsNullOrEmpty(exePath))
                        {
                            key.SetValue(AppName, "\"" + exePath + "\" --autostart");
                            return true;
                        }
                    }
                    else
                    {
                        key.DeleteValue(AppName, false);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[InstallationService] Ошибка автозапуска: " + ex.Message);
            }
            return false;
        }

        public static bool IsAutostartEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false))
                {
                    return key != null && key.GetValue(AppName) != null;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void InstallAndRelaunch()
        {
            try
            {
                Process currentProc = Process.GetCurrentProcess();
                string currentExe = currentProc.MainModule != null ? currentProc.MainModule.FileName : string.Empty;
                if (string.IsNullOrEmpty(currentExe)) return;

                string targetDir = GetInstalledDirectory();
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                string targetExe = Path.Combine(targetDir, "QRSniper.exe");
                File.Copy(currentExe, targetExe, true);

                string currentDir = Path.GetDirectoryName(currentExe);
                if (!string.IsNullOrEmpty(currentDir))
                {
                    string localConfig = Path.Combine(currentDir, "config.json");
                    string targetConfig = Path.Combine(targetDir, "config.json");
                    if (File.Exists(localConfig))
                    {
                        File.Copy(localConfig, targetConfig, true);
                    }

                    string localIco = Path.Combine(currentDir, "app.ico");
                    string targetIco = Path.Combine(targetDir, "app.ico");
                    if (File.Exists(localIco))
                    {
                        File.Copy(localIco, targetIco, true);
                    }
                }

                CreateStartMenuShortcut(targetExe);

                // Регистрируем AUMID для Windows уведомлений
                RegisterAumid(targetExe);

                if (IsAutostartEnabled())
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true))
                    {
                        if (key != null)
                        {
                            key.SetValue(AppName, "\"" + targetExe + "\" --autostart");
                        }
                    }
                }

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = targetExe;
                psi.UseShellExecute = true;
                Process.Start(psi);

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[InstallationService] Ошибка установки: " + ex.Message);
            }
        }

        public static void RegisterAumid(string exePath)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\AppUserModelId\QR.Sniper"))
                {
                    if (key != null)
                    {
                        key.SetValue("DisplayName", "QR Sniper");
                        if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                        {
                            key.SetValue("IconUri", exePath);
                        }
                        key.SetValue("IconBackgroundColor", "0");
                    }
                }

                using (RegistryKey key2 = Registry.CurrentUser.CreateSubKey(@"Software\Classes\AppUserModelId\QRSniper"))
                {
                    if (key2 != null)
                    {
                        key2.SetValue("DisplayName", "QR Sniper");
                        if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                        {
                            key2.SetValue("IconUri", exePath);
                        }
                        key2.SetValue("IconBackgroundColor", "0");
                    }
                }
            }
            catch { }
        }

        private static void CreateStartMenuShortcut(string targetExe)
        {
            try
            {
                string programsDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                    "Programs"
                );
                string shortcutPath = Path.Combine(programsDir, "QR Sniper.lnk");

                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType != null)
                {
                    object shell = Activator.CreateInstance(shellType);
                    if (shell != null)
                    {
                        object shortcut = shellType.InvokeMember(
                            "CreateShortcut",
                            System.Reflection.BindingFlags.InvokeMethod,
                            null,
                            shell,
                            new object[] { shortcutPath }
                        );

                        if (shortcut != null)
                        {
                            Type scType = shortcut.GetType();
                            scType.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { targetExe });
                            scType.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { Path.GetDirectoryName(targetExe) });
                            scType.InvokeMember("Description", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { "Screen QR code scanner" });
                            scType.InvokeMember("IconLocation", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { targetExe + ",0" });
                            scType.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, shortcut, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[InstallationService] Ошибка создания ярлыка: " + ex.Message);
            }
        }
    }
}
