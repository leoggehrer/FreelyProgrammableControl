using System.Text;
using FreelyProgrammableControl.McpServer.Models;

namespace FreelyProgrammableControl.McpServer.Services
{
    /// <summary>
    /// Client zum Kommunizieren mit der FPC Desktop-Anwendung über HTTP
    /// </summary>
    public class DesktopClientService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public DesktopClientService(string baseUrl)
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
                // KEIN EnsureSuccessStatusCode — bei HTTP 400 (Parse-Fehler) wollen wir
                // trotzdem die Antwort lesen um die Fehlermeldung zu erhalten
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
                // KEIN EnsureSuccessStatusCode — Antwort immer lesen
                return await response.Content.ReadFromJsonAsync<ExecutionControlResponse>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Stoppen der Ausführung: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Setzt den Debug-Modus
        /// </summary>
        public async Task<DebugModeResponse?> SetDebugModeAsync(bool enable)
        {
            try
            {
                var content = new StringContent(enable.ToString(), Encoding.UTF8, "text/plain");
                var response = await _httpClient.PostAsync("/api/debug", content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<DebugModeResponse>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Setzen des Debug-Modus: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Führt einen einzelnen Programmschritt im Debug-Modus aus
        /// </summary>
        public async Task<StepResponse?> StepAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("/api/step", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<StepResponse>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Ausführen des Schritts: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ruft den detaillierten Ausführungszustand ab
        /// </summary>
        public async Task<ExecutionStateResponse?> GetExecutionStateAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/state");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ExecutionStateResponse>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Abrufen des Zustands: {ex.Message}", ex);
            }
        }

        // ====================================================
        // Debug & Inspection Endpoints
        // ====================================================

        /// <summary>
        /// Ruft einen vollständigen Debug-Snapshot ab (Stack, Memory, Timer, Counter, aktuelle Zeile)
        /// </summary>
        public async Task<string> GetDebugSnapshotAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/debug/snapshot");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Abrufen des Debug-Snapshots: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ruft alle Input-Zustände ab
        /// </summary>
        public async Task<string> GetInputsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/inputs");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Abrufen der Inputs: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Toggelt einen einzelnen Input
        /// </summary>
        public async Task<string> ToggleInputAsync(int index)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/api/inputs/{index}/toggle", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Toggeln von Input {index}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ruft alle Output-Zustände ab
        /// </summary>
        public async Task<string> GetOutputsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/outputs");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Abrufen der Outputs: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ruft Memory-Werte in einem Bereich ab
        /// </summary>
        public async Task<string> GetMemoryAsync(int from = 0, int to = 63)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/memory?from={from}&to={to}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Abrufen des Memory: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ruft Timer-Zustände in einem Bereich ab
        /// </summary>
        public async Task<string> GetTimersAsync(int from = 0, int to = 15)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/timers?from={from}&to={to}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Abrufen der Timer: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ruft Counter-Werte in einem Bereich ab
        /// </summary>
        public async Task<string> GetCountersAsync(int from = 0, int to = 15)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/counters?from={from}&to={to}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Abrufen der Counter: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ruft die aktuelle Zykluszeit ab
        /// </summary>
        public async Task<string> GetCycleTimeAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/cycletime");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Abrufen der Zykluszeit: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Setzt die Zykluszeit in Millisekunden
        /// </summary>
        public async Task<string> SetCycleTimeAsync(int cycleTimeMs)
        {
            try
            {
                var content = new StringContent(cycleTimeMs.ToString(), Encoding.UTF8, "text/plain");
                var response = await _httpClient.PostAsync("/api/cycletime", content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Setzen der Zykluszeit: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Setzt alle Outputs zurück
        /// </summary>
        public async Task<string> ResetOutputsAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("/api/reset/outputs", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Zurücksetzen der Outputs: {ex.Message}", ex);
            }
        }
    }
}
