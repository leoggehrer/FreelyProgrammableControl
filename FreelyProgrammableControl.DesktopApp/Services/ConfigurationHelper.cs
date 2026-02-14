using System;
using System.IO;
using System.Text.Json;

namespace FreelyProgrammableControl.DesktopApp.Services
{
    /// <summary>
    /// Helper-Klasse zum Lesen der Anwendungskonfiguration
    /// </summary>
    public static class ConfigurationHelper
    {
        private static AppSettings? _settings;

        /// <summary>
        /// Lädt die Konfiguration aus appsettings.json
        /// </summary>
        public static AppSettings GetSettings()
        {
            if (_settings != null)
                return _settings;

            try
            {
                var appDirectory = AppContext.BaseDirectory;
                var settingsPath = Path.Combine(appDirectory, "appsettings.json");

                if (!File.Exists(settingsPath))
                {
                    // Fallback auf Default-Werte
                    return new AppSettings
                    {
                        Api = new ApiSettings { Port = 5555 },
                        N8n = new N8nSettings()
                    };
                }

                var json = File.ReadAllText(settingsPath);
                _settings = JsonSerializer.Deserialize<AppSettings>(json);

                return _settings ?? new AppSettings
                {
                    Api = new ApiSettings { Port = 5555 },
                    N8n = new N8nSettings()
                };
            }
            catch
            {
                // Bei Fehler Default-Werte verwenden
                return new AppSettings
                {
                    Api = new ApiSettings { Port = 5555 },
                    N8n = new N8nSettings()
                };
            }
        }
    }

    public class AppSettings
    {
        public ApiSettings Api { get; set; } = new();
        public N8nSettings N8n { get; set; } = new();
    }

    public class ApiSettings
    {
        public int Port { get; set; } = 5555;
    }

    public class N8nSettings
    {
        public string SaveToGoogleDriveWebhookUrl { get; set; } = string.Empty;
        public string GetFpcSampleListWebhookUrl { get; set; } = string.Empty;
        public string LoadFpcSampleWebhookUrl { get; set; } = string.Empty;
    }
}
