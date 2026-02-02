using System.ComponentModel;
using FreelyProgrammableControl.McpTool.Services;
using ModelContextProtocol.Server;

namespace FreelyProgrammableControl.McpTool.Tools
{
    /// <summary>
    /// MCP-Tools zur Kommunikation mit der FPC Desktop-Anwendung
    /// </summary>
    [McpServerToolType]
    public static partial class DesktopControlTool
    {
        private static readonly DesktopClientService _client = new("http://localhost:5555");

        /// <summary>
        /// Prüft die Verbindung zur Desktop-Anwendung und gibt den aktuellen Status zurück.
        /// </summary>
        /// <returns>Status-Informationen der Steuerung.</returns>
        [McpServerTool(Name = "check_desktop_status")]
        [Description("Prüft die Verbindung zur FPC Desktop-Anwendung und gibt den aktuellen Ausführungsstatus zurück.")]
        public static async Task<DesktopStatusResult> CheckDesktopStatus()
        {
            try
            {
                var isConnected = await _client.IsConnectedAsync();
                
                if (!isConnected)
                {
                    return new DesktopStatusResult
                    {
                        IsConnected = false,
                        Message = "Desktop-Anwendung ist nicht erreichbar. Bitte stellen Sie sicher, dass die Anwendung läuft."
                    };
                }

                var status = await _client.GetStatusAsync();
                
                return new DesktopStatusResult
                {
                    IsConnected = true,
                    IsRunning = status?.isRunning ?? false,
                    HasParseError = status?.hasParseError ?? false,
                    ParseErrorMessage = status?.parseErrorMessage,
                    DebugEnabled = status?.debugEnabled ?? false,
                    SourceLines = status?.sourceLines ?? 0,
                    ExecutionState = status?.executionState,
                    Message = status?.isRunning == true 
                        ? "Programm wird ausgeführt" 
                        : "Programm ist gestoppt"
                };
            }
            catch (Exception ex)
            {
                return new DesktopStatusResult
                {
                    IsConnected = false,
                    Message = $"Fehler beim Abrufen des Status: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Lädt ein FPC-Programm in die Desktop-Anwendung.
        /// </summary>
        /// <param name="programCode">Der FPC-Programm-Code.</param>
        /// <returns>Ergebnis des Ladevorgangs.</returns>
        [McpServerTool(Name = "load_program_to_desktop")]
        [Description("Lädt ein FPC-Programm in die Desktop-Anwendung. Das Programm wird geparst und auf Fehler überprüft.")]
        public static async Task<ProgramLoadResult> LoadProgramToDesktop(
            [Description("Der vollständige FPC-Programm-Code, der in die Desktop-Anwendung geladen werden soll.")]
            string programCode)
        {
            try
            {
                var isConnected = await _client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new ProgramLoadResult
                    {
                        Success = false,
                        Message = "Desktop-Anwendung ist nicht erreichbar. Bitte starten Sie die Desktop-Anwendung zuerst."
                    };
                }

                var result = await _client.LoadProgramAsync(programCode);
                
                return new ProgramLoadResult
                {
                    Success = result?.success ?? false,
                    HasParseError = result?.hasParseError ?? false,
                    ParseErrorMessage = result?.parseErrorMessage,
                    SourceLines = result?.sourceLines ?? 0,
                    Message = result?.success == true
                        ? $"Programm erfolgreich geladen ({result.sourceLines} Zeilen)"
                        : $"Fehler beim Laden: {result?.parseErrorMessage ?? "Unbekannter Fehler"}"
                };
            }
            catch (Exception ex)
            {
                return new ProgramLoadResult
                {
                    Success = false,
                    Message = $"Fehler beim Laden des Programms: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Startet die Ausführung des geladenen Programms in der Desktop-Anwendung.
        /// </summary>
        /// <returns>Ergebnis des Start-Befehls.</returns>
        [McpServerTool(Name = "start_program_execution")]
        [Description("Startet die Ausführung des aktuell geladenen FPC-Programms in der Desktop-Anwendung.")]
        public static async Task<ExecutionControlResult> StartProgramExecution()
        {
            try
            {
                var isConnected = await _client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new ExecutionControlResult
                    {
                        Success = false,
                        Message = "Desktop-Anwendung ist nicht erreichbar."
                    };
                }

                // Erst Status prüfen
                var status = await _client.GetStatusAsync();
                if (status?.hasParseError == true)
                {
                    return new ExecutionControlResult
                    {
                        Success = false,
                        IsRunning = false,
                        Message = $"Programm kann nicht gestartet werden. Parse-Fehler: {status.parseErrorMessage}"
                    };
                }

                var result = await _client.StartExecutionAsync();
                
                return new ExecutionControlResult
                {
                    Success = result?.success ?? false,
                    IsRunning = result?.isRunning ?? false,
                    Message = result?.isRunning == true
                        ? "Programm wurde erfolgreich gestartet"
                        : "Programm konnte nicht gestartet werden"
                };
            }
            catch (Exception ex)
            {
                return new ExecutionControlResult
                {
                    Success = false,
                    Message = $"Fehler beim Starten: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Stoppt die Ausführung des Programms in der Desktop-Anwendung.
        /// </summary>
        /// <returns>Ergebnis des Stopp-Befehls.</returns>
        [McpServerTool(Name = "stop_program_execution")]
        [Description("Stoppt die Ausführung des FPC-Programms in der Desktop-Anwendung.")]
        public static async Task<ExecutionControlResult> StopProgramExecution()
        {
            try
            {
                var isConnected = await _client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new ExecutionControlResult
                    {
                        Success = false,
                        Message = "Desktop-Anwendung ist nicht erreichbar."
                    };
                }

                var result = await _client.StopExecutionAsync();
                
                return new ExecutionControlResult
                {
                    Success = result?.success ?? false,
                    IsRunning = result?.isRunning ?? false,
                    Message = result?.isRunning == false
                        ? "Programm wurde erfolgreich gestoppt"
                        : "Programm läuft noch"
                };
            }
            catch (Exception ex)
            {
                return new ExecutionControlResult
                {
                    Success = false,
                    Message = $"Fehler beim Stoppen: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Ruft das aktuell in der Desktop-Anwendung geladene Programm ab.
        /// </summary>
        /// <returns>Das aktuelle Programm.</returns>
        [McpServerTool(Name = "get_current_program")]
        [Description("Ruft das aktuell in der Desktop-Anwendung geladene FPC-Programm ab.")]
        public static async Task<ProgramRetrieveResult> GetCurrentProgram()
        {
            try
            {
                var isConnected = await _client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new ProgramRetrieveResult
                    {
                        Success = false,
                        Message = "Desktop-Anwendung ist nicht erreichbar."
                    };
                }

                var result = await _client.GetProgramAsync();
                
                return new ProgramRetrieveResult
                {
                    Success = true,
                    ProgramCode = result?.programCode ?? string.Empty,
                    SourceLines = result?.sourceLines ?? 0,
                    Message = $"Programm abgerufen ({result?.sourceLines ?? 0} Zeilen)"
                };
            }
            catch (Exception ex)
            {
                return new ProgramRetrieveResult
                {
                    Success = false,
                    Message = $"Fehler beim Abrufen des Programms: {ex.Message}"
                };
            }
        }
    }

    #region Result Classes

    public class DesktopStatusResult
    {
        public bool IsConnected { get; set; }
        public bool IsRunning { get; set; }
        public bool HasParseError { get; set; }
        public string? ParseErrorMessage { get; set; }
        public bool DebugEnabled { get; set; }
        public int SourceLines { get; set; }
        public string? ExecutionState { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ProgramLoadResult
    {
        public bool Success { get; set; }
        public bool HasParseError { get; set; }
        public string? ParseErrorMessage { get; set; }
        public int SourceLines { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ExecutionControlResult
    {
        public bool Success { get; set; }
        public bool IsRunning { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ProgramRetrieveResult
    {
        public bool Success { get; set; }
        public string ProgramCode { get; set; } = string.Empty;
        public int SourceLines { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    #endregion
}
