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
                        Machine = new MachineSettings(),
                        N8N = new N8nSettings()
                    };
                }

                var json = File.ReadAllText(settingsPath);
                _settings = JsonSerializer.Deserialize<AppSettings>(json);

                return _settings ?? new AppSettings
                {
                    Api = new ApiSettings { Port = 5555 },
                    Machine = new MachineSettings(),
                    N8N = new N8nSettings()
                };
            }
            catch
            {
                // Bei Fehler Default-Werte verwenden
                return new AppSettings
                {
                    Api = new ApiSettings { Port = 5555 },
                    Machine = new MachineSettings(),
                    N8N = new N8nSettings()
                };
            }
        }
    }

    public class AppSettings
    {
        public ApiSettings Api { get; set; } = new();
        public MachineSettings Machine { get; set; } = new();
        public N8nSettings N8N { get; set; } = new();
    }

    public class ApiSettings
    {
        public int Port { get; set; } = 5555;
    }

    public class MachineSettings
    {
        public int InputCount { get; set; } = 20;
        public int OutputCount { get; set; } = 20;
    }

    public class N8nSettings
    {
        public string SaveToGoogleDriveWebhookUrl { get; set; } = string.Empty;
        public string GetFPCSampleListWebhookUrl { get; set; } = string.Empty;
        public string LoadFPCSampleWebhookUrl { get; set; } = string.Empty;
        public string FPCSampleListFolderName { get; set; } = string.Empty;
        public string SaveToVectorWebhookUrl { get; set; } = string.Empty;
    }
}
