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
                    return new AppSettings { Api = new ApiSettings { Port = 5555 } };
                }

                var json = File.ReadAllText(settingsPath);
                _settings = JsonSerializer.Deserialize<AppSettings>(json);

                return _settings ?? new AppSettings { Api = new ApiSettings { Port = 5555 } };
            }
            catch
            {
                // Bei Fehler Default-Werte verwenden
                return new AppSettings { Api = new ApiSettings { Port = 5555 } };
            }
        }
    }

    public class AppSettings
    {
        public ApiSettings Api { get; set; } = new();
    }

    public class ApiSettings
    {
        public int Port { get; set; } = 5555;
    }
}
