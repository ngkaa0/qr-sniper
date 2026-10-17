using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using QRSniper.Models;

namespace QRSniper.Services
{
    public class ConfigService
    {
        private readonly string _configFilePath;

        public ConfigService()
        {
            _configFilePath = DetermineConfigPath();
        }

        public string ConfigFilePath
        {
            get { return _configFilePath; }
        }

        private static string DetermineConfigPath()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
            string localConfig = Path.Combine(appDir, "config.json");

            if (File.Exists(localConfig))
            {
                return localConfig;
            }

            string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory).TrimEnd('\\', '/');
            string downloadsDir = InstallationService.GetActualDownloadsFolder().TrimEnd('\\', '/');

            if (string.Equals(appDir, desktopDir, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(appDir, downloadsDir, StringComparison.OrdinalIgnoreCase))
            {
                string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QRSniper");
                if (!Directory.Exists(appDataDir))
                {
                    Directory.CreateDirectory(appDataDir);
                }
                return Path.Combine(appDataDir, "config.json");
            }

            try
            {
                string testFile = Path.Combine(appDir, ".write_test");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                return localConfig;
            }
            catch
            {
                string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QRSniper");
                if (!Directory.Exists(appDataDir))
                {
                    Directory.CreateDirectory(appDataDir);
                }
                return Path.Combine(appDataDir, "config.json");
            }
        }

        public AppConfig LoadConfig()
        {
            AppConfig config = new AppConfig();
            try
            {
                if (File.Exists(_configFilePath))
                {
                    string[] lines = File.ReadAllLines(_configFilePath);
                    foreach (string rawLine in lines)
                    {
                        string line = rawLine.Trim();
                        if (line.StartsWith("{") || line.StartsWith("}") || string.IsNullOrEmpty(line))
                            continue;

                        int colonIdx = line.IndexOf(':');
                        if (colonIdx > 0)
                        {
                            string key = line.Substring(0, colonIdx).Trim(' ', '"', '\t');
                            string val = line.Substring(colonIdx + 1).Trim(' ', '"', ',', '\t');

                            if (key.Equals("ModCtrl", StringComparison.OrdinalIgnoreCase))
                                config.ModCtrl = ParseBool(val, true);
                            else if (key.Equals("ModShift", StringComparison.OrdinalIgnoreCase))
                                config.ModShift = ParseBool(val, true);
                            else if (key.Equals("ModAlt", StringComparison.OrdinalIgnoreCase))
                                config.ModAlt = ParseBool(val, false);
                            else if (key.Equals("ModWin", StringComparison.OrdinalIgnoreCase))
                                config.ModWin = ParseBool(val, false);
                            else if (key.Equals("HotkeyKey", StringComparison.OrdinalIgnoreCase))
                            {
                                try { config.HotkeyKey = (Keys)Enum.Parse(typeof(Keys), val, true); }
                                catch { config.HotkeyKey = Keys.Q; }
                            }
                            else if (key.Equals("CopyToClipboard", StringComparison.OrdinalIgnoreCase) ||
                                     key.Equals("AutoCopyClipboard", StringComparison.OrdinalIgnoreCase))
                                config.CopyToClipboard = ParseBool(val, false);
                            else if (key.Equals("PlaySound", StringComparison.OrdinalIgnoreCase))
                                config.PlaySound = ParseBool(val, true);
                            else if (key.Equals("ShowNotification", StringComparison.OrdinalIgnoreCase))
                                config.ShowNotification = ParseBool(val, true);
                            else if (key.Equals("StartWithWindows", StringComparison.OrdinalIgnoreCase))
                                config.StartWithWindows = ParseBool(val, false);
                        }
                    }
                    config.Version = AppConfig.CurrentVersion;
                    return config;
                }
            }
            catch
            {
                // При ошибке возвращаем дефолтные настройки
            }

            SaveConfig(config);
            return config;
        }

        public void SaveConfig(AppConfig config)
        {
            try
            {
                string dir = Path.GetDirectoryName(_configFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("{");
                sb.AppendLine(string.Format("  \"Version\": \"{0}\",", AppConfig.CurrentVersion));
                sb.AppendLine(string.Format("  \"ModCtrl\": {0},", config.ModCtrl.ToString().ToLower()));
                sb.AppendLine(string.Format("  \"ModShift\": {0},", config.ModShift.ToString().ToLower()));
                sb.AppendLine(string.Format("  \"ModAlt\": {0},", config.ModAlt.ToString().ToLower()));
                sb.AppendLine(string.Format("  \"ModWin\": {0},", config.ModWin.ToString().ToLower()));
                sb.AppendLine(string.Format("  \"HotkeyKey\": \"{0}\",", config.HotkeyKey.ToString()));
                sb.AppendLine(string.Format("  \"CopyToClipboard\": {0},", config.CopyToClipboard.ToString().ToLower()));
                sb.AppendLine(string.Format("  \"PlaySound\": {0},", config.PlaySound.ToString().ToLower()));
                sb.AppendLine(string.Format("  \"ShowNotification\": {0},", config.ShowNotification.ToString().ToLower()));
                sb.AppendLine(string.Format("  \"StartWithWindows\": {0}", config.StartWithWindows.ToString().ToLower()));
                sb.AppendLine("}");

                File.WriteAllText(_configFilePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ConfigService] Ошибка сохранения: " + ex.Message);
            }
        }

        private static bool ParseBool(string val, bool def)
        {
            bool res;
            if (bool.TryParse(val, out res)) return res;
            return def;
        }
    }
}
