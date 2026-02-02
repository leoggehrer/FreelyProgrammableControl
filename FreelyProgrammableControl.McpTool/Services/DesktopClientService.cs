using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FreelyProgrammableControl.McpTool.Services
{
    /// <summary>
    /// Client zum Kommunizieren mit der FPC Desktop-Anwendung über HTTP
    /// </summary>
    public class DesktopClientService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public DesktopClientService(string baseUrl = "http://localhost:5555")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        /// <summary>
        /// Prüft, ob die Desktop-App erreichbar ist
        /// </summary>
        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/status");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ruft den aktuellen Status der Steuerung ab
        /// </summary>
        public async Task<ExecutionStatusResponse?> GetStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/status");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ExecutionStatusResponse>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Abrufen des Status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lädt ein Programm in die Desktop-Anwendung
        /// </summary>
        public async Task<ProgramLoadResponse?> LoadProgramAsync(string programCode)
        {
            try
            {
                var content = new StringContent(programCode, Encoding.UTF8, "text/plain");
                var response = await _httpClient.PostAsync("/api/program", content);
                
                var result = await response.Content.ReadFromJsonAsync<ProgramLoadResponse>();
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Programm konnte nicht geladen werden: {result?.parseErrorMessage ?? "Unbekannter Fehler"}");
                }
                
                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Verbindungsfehler beim Laden des Programms: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Laden des Programms: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ruft das aktuelle Programm von der Desktop-Anwendung ab
        /// </summary>
        public async Task<ProgramResponse?> GetProgramAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/program");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ProgramResponse>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Abrufen des Programms: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Startet die Programmausführung
        /// </summary>
        public async Task<ExecutionControlResponse?> StartExecutionAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("/api/start", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ExecutionControlResponse>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Starten der Ausführung: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Stoppt die Programmausführung
        /// </summary>
        public async Task<ExecutionControlResponse?> StopExecutionAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("/api/stop", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ExecutionControlResponse>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Stoppen der Ausführung: {ex.Message}", ex);
            }
        }
    }

    #region Response DTOs

    public class ExecutionStatusResponse
    {
        public bool isRunning { get; set; }
        public bool hasParseError { get; set; }
        public string? parseErrorMessage { get; set; }
        public bool debugEnabled { get; set; }
        public int sourceLines { get; set; }
        public string? executionState { get; set; }
    }

    public class ProgramLoadResponse
    {
        public bool success { get; set; }
        public bool hasParseError { get; set; }
        public string? parseErrorMessage { get; set; }
        public int sourceLines { get; set; }
    }

    public class ProgramResponse
    {
        public string? programCode { get; set; }
        public int sourceLines { get; set; }
    }

    public class ExecutionControlResponse
    {
        public bool success { get; set; }
        public bool isRunning { get; set; }
    }

    #endregion
}
