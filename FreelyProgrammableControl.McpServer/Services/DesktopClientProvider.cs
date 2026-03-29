namespace FreelyProgrammableControl.McpServer.Services
{
    /// <summary>
    /// Zentraler Provider für den DesktopClientService.
    /// Die Konfiguration erfolgt einmalig beim Start aus der appsettings.json.
    /// Alle Tool-Klassen verwenden diesen gemeinsamen Client.
    /// </summary>
    public static class DesktopClientProvider
    {
        private static DesktopClientService? _client;

        /// <summary>
        /// Initialisiert den Provider mit der URL aus der Konfiguration.
        /// Wird einmalig in Program.cs aufgerufen.
        /// </summary>
        /// <param name="baseUrl">Die Base-URL der Desktop-Anwendung (z.B. aus DesktopApp:BaseUrl).</param>
        public static void Initialize(string baseUrl)
        {
            _client = new DesktopClientService(baseUrl);
        }

        /// <summary>
        /// Gibt den konfigurierten DesktopClientService zurück.
        /// </summary>
        /// <exception cref="InvalidOperationException">Wenn der Provider nicht initialisiert wurde.</exception>
        public static DesktopClientService Client =>
            _client ?? throw new InvalidOperationException(
                "DesktopClientProvider wurde nicht initialisiert. " +
                "Bitte 'DesktopApp:BaseUrl' in der appsettings.json konfigurieren.");
    }
}
