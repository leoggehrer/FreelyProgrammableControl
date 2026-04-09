using System.ComponentModel;
using FreelyProgrammableControl.McpServer.Services;
using ModelContextProtocol.Server;

namespace FreelyProgrammableControl.McpServer.Tools
{
    /// <summary>
    /// MCP-Tools zum Debuggen von FPC-Programmen.
    /// Bietet Zugriff auf Debug-Snapshots, Einzelschritt-Ausführung mit Zustandsinspektion
    /// und detaillierte Ressourcen-Überwachung.
    /// </summary>
    [McpServerToolType]
    public static partial class DebuggerTool
    {
        private static DesktopClientService Client => DesktopClientProvider.Client;

        /// <summary>
        /// Ruft einen vollständigen Debug-Snapshot ab mit allen Zustandsinformationen.
        /// </summary>
        [McpServerTool(Name = "debug_snapshot")]
        [Description("Retrieves a complete debug snapshot of the controller. Includes current state, stack contents, memory values, timer states, counter values, input/output states, current execution line, and parse/runtime errors. Ideal for troubleshooting and state analysis.")]
        public static async Task<DebugSnapshotResult> GetDebugSnapshot()
        {
            System.Diagnostics.Debug.WriteLine($"[Tool] Rufe Debug-Snapshot ab...");
            try
            {
                var isConnected = await Client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new DebugSnapshotResult
                    {
                        Success = false,
                        Message = "Desktop-Anwendung ist nicht erreichbar."
                    };
                }

                var snapshot = await Client.GetDebugSnapshotAsync();

                return new DebugSnapshotResult
                {
                    Success = true,
                    SnapshotJson = snapshot,
                    Message = "Debug-Snapshot erfolgreich abgerufen"
                };
            }
            catch (Exception ex)
            {
                return new DebugSnapshotResult
                {
                    Success = false,
                    Message = $"Fehler beim Abrufen des Debug-Snapshots: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Führt einen einzelnen Programmschritt aus und gibt den vollständigen Debug-Snapshot zurück.
        /// Kombiniert step_program + debug_snapshot in einem Aufruf.
        /// </summary>
        [McpServerTool(Name = "debug_step_and_inspect")]
        [Description("Executes a single program step in debug mode and then returns the full debug snapshot (stack, memory, timers, counters, I/O, current line). The controller must be running and debug mode must be enabled. Ideal for step-by-step debugging.")]
        public static async Task<DebugStepResult> StepAndInspect()
        {
            System.Diagnostics.Debug.WriteLine($"[Tool] Führe Debug-Schritt mit Inspektion aus...");
            try
            {
                var isConnected = await Client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new DebugStepResult
                    {
                        Success = false,
                        Message = "Desktop-Anwendung ist nicht erreichbar."
                    };
                }

                // Prüfe Status
                var status = await Client.GetStatusAsync();
                if (status?.isRunning == false)
                {
                    return new DebugStepResult
                    {
                        Success = false,
                        Message = "Die Steuerung muss gestartet sein. Verwende 'start_program_execution' zuerst."
                    };
                }

                if (status?.debugEnabled == false)
                {
                    return new DebugStepResult
                    {
                        Success = false,
                        Message = "Debug-Modus ist nicht aktiviert. Verwende 'enable_debug_mode' mit enable=true, bevor du das Programm startest."
                    };
                }

                // Step ausführen
                var stepResult = await Client.StepAsync();
                if (stepResult?.success != true)
                {
                    return new DebugStepResult
                    {
                        Success = false,
                        Message = "Fehler beim Ausführen des Schritts"
                    };
                }

                // Debug-Snapshot nach dem Step holen
                var snapshot = await Client.GetDebugSnapshotAsync();

                return new DebugStepResult
                {
                    Success = true,
                    SnapshotJson = snapshot,
                    Message = "Schritt ausgeführt, Snapshot abgerufen"
                };
            }
            catch (Exception ex)
            {
                return new DebugStepResult
                {
                    Success = false,
                    Message = $"Fehler beim Debug-Schritt: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Führt mehrere Programmschritte hintereinander aus und gibt den Zustand nach jedem Schritt zurück.
        /// </summary>
        [McpServerTool(Name = "debug_multi_step")]
        [Description("Executes multiple program steps in debug mode and returns the final state as a debug snapshot. The controller must be running and debug mode must be enabled. Useful for quickly advancing through several lines.")]
        public static async Task<DebugStepResult> MultiStep(
            [Description("Number of steps to execute (1-100).")]
            int steps = 1)
        {
            System.Diagnostics.Debug.WriteLine($"[Tool] Führe {steps} Debug-Schritte aus...");
            try
            {
                steps = Math.Clamp(steps, 1, 100);

                var isConnected = await Client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new DebugStepResult
                    {
                        Success = false,
                        Message = "Desktop-Anwendung ist nicht erreichbar."
                    };
                }

                var status = await Client.GetStatusAsync();
                if (status?.isRunning == false)
                {
                    return new DebugStepResult
                    {
                        Success = false,
                        Message = "Die Steuerung muss gestartet sein."
                    };
                }

                if (status?.debugEnabled == false)
                {
                    return new DebugStepResult
                    {
                        Success = false,
                        Message = "Debug-Modus ist nicht aktiviert."
                    };
                }

                // Mehrere Steps ausführen
                for (int i = 0; i < steps; i++)
                {
                    var stepResult = await Client.StepAsync();
                    if (stepResult?.success != true)
                    {
                        return new DebugStepResult
                        {
                            Success = false,
                            Message = $"Fehler bei Schritt {i + 1} von {steps}"
                        };
                    }
                }

                // Finalen Snapshot holen
                var snapshot = await Client.GetDebugSnapshotAsync();

                return new DebugStepResult
                {
                    Success = true,
                    SnapshotJson = snapshot,
                    Message = $"{steps} Schritte ausgeführt, Endzustand abgerufen"
                };
            }
            catch (Exception ex)
            {
                return new DebugStepResult
                {
                    Success = false,
                    Message = $"Fehler beim Multi-Step: {ex.Message}"
                };
            }
        }
    }

    #region Result Classes

    public class DebugSnapshotResult
    {
        public bool Success { get; set; }
        public string? SnapshotJson { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class DebugStepResult
    {
        public bool Success { get; set; }
        public string? SnapshotJson { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    #endregion
}
