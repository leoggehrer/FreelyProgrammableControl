using System.Text;
using FreelyProgrammableControl.McpServer.Models;

namespace FreelyProgrammableControl.McpServer.Services
{
    /// <summary>
    /// HTTP client that communicates with the FPC Desktop Application API
    /// running at <c>http://localhost:5555</c> (or a configured base URL).
    /// Each method maps to one REST endpoint exposed by the desktop app.
    /// </summary>
    public class DesktopClientService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initialises the client with the given base URL.
        /// </summary>
        /// <param name="baseUrl">Base URL of the FPC Desktop Application API (e.g. "http://localhost:5555").</param>
        public DesktopClientService(string baseUrl)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        // ----------------------------------------------------------------
        // Connectivity
        // ----------------------------------------------------------------

        /// <summary>
        /// Returns <c>true</c> when the desktop application is reachable.
        /// Never throws — returns <c>false</c> on any network error.
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

        // ----------------------------------------------------------------
        // Execution control
        // ----------------------------------------------------------------

        /// <summary>Returns the current execution status of the controller.</summary>
        public Task<ExecutionStatusResponse?> GetStatusAsync()
            => GetJsonAsync<ExecutionStatusResponse>("/api/status");

        /// <summary>
        /// Loads <paramref name="programCode"/> into the desktop application.
        /// The controller is stopped automatically before loading.
        /// </summary>
        /// <exception cref="Exception">Thrown when the program contains parse errors or the request fails.</exception>
        public async Task<ProgramLoadResponse?> LoadProgramAsync(string programCode)
        {
            try
            {
                var content = new StringContent(programCode, Encoding.UTF8, "text/plain");
                var response = await _httpClient.PostAsync("/api/program", content);
                var result = await response.Content.ReadFromJsonAsync<ProgramLoadResponse>();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Programm konnte nicht geladen werden: {result?.parseErrorMessage ?? "Unbekannter Fehler"}");

                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Verbindungsfehler beim Laden des Programms: {ex.Message}", ex);
            }
        }

        /// <summary>Returns the FPC source code currently loaded in the desktop application.</summary>
        public Task<ProgramResponse?> GetProgramAsync()
            => GetJsonAsync<ProgramResponse>("/api/program");

        /// <summary>
        /// Clears the currently loaded source code and unloads program lines.
        /// </summary>
        public Task<string> ClearProgramAsync()
            => PostStringAsync("/api/program/clear");

        /// <summary>
        /// Starts program execution.
        /// Does not throw on HTTP 400 — the caller should inspect <c>success</c> in the response.
        /// </summary>
        public Task<ExecutionControlResponse?> StartExecutionAsync()
            => PostJsonAsync<ExecutionControlResponse>("/api/start", ensureSuccess: false);

        /// <summary>
        /// Stops program execution.
        /// Does not throw on HTTP error — the caller should inspect <c>success</c> in the response.
        /// </summary>
        public Task<ExecutionControlResponse?> StopExecutionAsync()
            => PostJsonAsync<ExecutionControlResponse>("/api/stop", ensureSuccess: false);

        // ----------------------------------------------------------------
        // Debug control
        // ----------------------------------------------------------------

        /// <summary>
        /// Enables or disables debug mode.
        /// The controller must be stopped before calling this method.
        /// </summary>
        public Task<DebugModeResponse?> SetDebugModeAsync(bool enable)
        {
            var body = new StringContent(enable.ToString(), Encoding.UTF8, "text/plain");
            return PostJsonAsync<DebugModeResponse>("/api/debug", body);
        }

        /// <summary>
        /// Executes one instruction step in debug mode.
        /// Requires the controller to be running with debug mode enabled.
        /// </summary>
        public Task<StepResponse?> StepAsync()
            => PostJsonAsync<StepResponse>("/api/step");

        /// <summary>Returns the detailed runtime state of the controller.</summary>
        public Task<ExecutionStateResponse?> GetExecutionStateAsync()
            => GetJsonAsync<ExecutionStateResponse>("/api/state");

        // ----------------------------------------------------------------
        // Inspection endpoints
        // ----------------------------------------------------------------

        /// <summary>
        /// Returns a complete debug snapshot as a raw JSON string.
        /// Includes stack, memory, timers, counters, I/O states and the current execution line.
        /// </summary>
        public Task<string> GetDebugSnapshotAsync()
            => GetStringAsync("/api/debug/snapshot");

        /// <summary>Returns the current state of all input channels as a raw JSON string.</summary>
        public Task<string> GetInputsAsync()
            => GetStringAsync("/api/inputs");

        /// <summary>
        /// Toggles input channel <paramref name="index"/> (true→false or false→true).
        /// Returns the new state as a raw JSON string.
        /// </summary>
        public Task<string> ToggleInputAsync(int index)
            => PostStringAsync($"/api/inputs/{index}/toggle");

        /// <summary>Returns the current state of all output channels as a raw JSON string.</summary>
        public Task<string> GetOutputsAsync()
            => GetStringAsync("/api/outputs");

        /// <summary>Returns memory values in the range [<paramref name="from"/>, <paramref name="to"/>] as a raw JSON string.</summary>
        public Task<string> GetMemoryAsync(int from = 0, int to = 63)
            => GetStringAsync($"/api/memory?from={from}&to={to}");

        /// <summary>Returns timer states in the range [<paramref name="from"/>, <paramref name="to"/>] as a raw JSON string.</summary>
        public Task<string> GetTimersAsync(int from = 0, int to = 15)
            => GetStringAsync($"/api/timers?from={from}&to={to}");

        /// <summary>Returns counter values in the range [<paramref name="from"/>, <paramref name="to"/>] as a raw JSON string.</summary>
        public Task<string> GetCountersAsync(int from = 0, int to = 15)
            => GetStringAsync($"/api/counters?from={from}&to={to}");

        /// <summary>Returns the current cycle time in milliseconds as a raw JSON string.</summary>
        public Task<string> GetCycleTimeAsync()
            => GetStringAsync("/api/cycletime");

        /// <summary>Sets the cycle time to <paramref name="cycleTimeMs"/> milliseconds (minimum 1 ms).</summary>
        public Task<string> SetCycleTimeAsync(int cycleTimeMs)
        {
            var body = new StringContent(cycleTimeMs.ToString(), Encoding.UTF8, "text/plain");
            return PostStringAsync("/api/cycletime", body);
        }

        /// <summary>Resets all output channels to <c>false</c>.</summary>
        public Task<string> ResetOutputsAsync()
            => PostStringAsync("/api/reset/outputs");

        /// <summary>Sets the label of input channel <paramref name="index"/>.</summary>
        public Task<string> SetInputLabelAsync(int index, string label)
        {
            var body = new StringContent(label, Encoding.UTF8, "text/plain");
            return PutStringAsync($"/api/inputs/{index}/label", body);
        }

        /// <summary>Sets the label of output channel <paramref name="index"/>.</summary>
        public Task<string> SetOutputLabelAsync(int index, string label)
        {
            var body = new StringContent(label, Encoding.UTF8, "text/plain");
            return PutStringAsync($"/api/outputs/{index}/label", body);
        }

        // ----------------------------------------------------------------
        // Private HTTP helpers
        // ----------------------------------------------------------------

        /// <summary>
        /// Sends a GET request and deserialises the JSON response body as <typeparamref name="T"/>.
        /// </summary>
        private async Task<T?> GetJsonAsync<T>(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                throw new Exception($"GET {url} fehlgeschlagen: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sends a POST request and deserialises the JSON response body as <typeparamref name="T"/>.
        /// </summary>
        /// <param name="ensureSuccess">
        /// When <c>false</c> the response status code is not validated, so the caller can
        /// read error details from a non-2xx response body.
        /// </param>
        private async Task<T?> PostJsonAsync<T>(string url, HttpContent? body = null, bool ensureSuccess = true)
        {
            try
            {
                var response = await _httpClient.PostAsync(url, body);
                if (ensureSuccess) response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                throw new Exception($"POST {url} fehlgeschlagen: {ex.Message}", ex);
            }
        }

        /// <summary>Sends a GET request and returns the raw response body as a string.</summary>
        private async Task<string> GetStringAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"GET {url} fehlgeschlagen: {ex.Message}", ex);
            }
        }

        /// <summary>Sends a POST request and returns the raw response body as a string.</summary>
        private async Task<string> PostStringAsync(string url, HttpContent? body = null)
        {
            try
            {
                var response = await _httpClient.PostAsync(url, body);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"POST {url} fehlgeschlagen: {ex.Message}", ex);
            }
        }

        /// <summary>Sends a PUT request and returns the raw response body as a string.</summary>
        private async Task<string> PutStringAsync(string url, HttpContent? body = null)
        {
            try
            {
                var response = await _httpClient.PutAsync(url, body);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"PUT {url} fehlgeschlagen: {ex.Message}", ex);
            }
        }
    }
}
