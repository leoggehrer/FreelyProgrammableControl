using System.ComponentModel;
using FreelyProgrammableControl.McpServer.Services;
using ModelContextProtocol.Server;

namespace FreelyProgrammableControl.McpServer.Tools
{
    /// <summary>
    /// MCP-Tools zur Inspektion und Steuerung von I/O-Ressourcen der FPC-Steuerung.
    /// Bietet Zugriff auf Inputs, Outputs, Memory, Timer, Counter und Zykluszeit.
    /// </summary>
    [McpServerToolType]
    public static partial class IOControlTool
    {
        private static DesktopClientService Client => DesktopClientProvider.Client;

        // ====================================================
        // Input Tools
        // ====================================================

        /// <summary>
        /// Ruft die Zustände aller Inputs ab.
        /// </summary>
        [McpServerTool(Name = "get_input_states")]
        [Description("Ruft die aktuellen Zustände aller Inputs der Steuerung ab. Gibt für jeden Input den Index, den booleschen Wert und das Label zurück.")]
        public static async Task<IOResult> GetInputStates()
        {
            System.Diagnostics.Debug.WriteLine($"[Tool] Rufe Input-Zustände ab...");
            try
            {
                var isConnected = await Client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new IOResult { Success = false, Message = "Desktop-Anwendung ist nicht erreichbar." };
                }

                var result = await Client.GetInputsAsync();
                return new IOResult { Success = true, DataJson = result, Message = "Input-Zustände abgerufen" };
            }
            catch (Exception ex)
            {
                return new IOResult { Success = false, Message = $"Fehler: {ex.Message}" };
            }
        }

        /// <summary>
        /// Schaltet einen einzelnen Input um (Toggle).
        /// </summary>
        [McpServerTool(Name = "toggle_input")]
        [Description("Schaltet einen einzelnen Input der Steuerung um (true→false oder false→true). Nützlich um Eingangssignale zu simulieren und das Programmverhalten zu testen.")]
        public static async Task<IOResult> ToggleInput(
            [Description("Der Index des umzuschaltenden Inputs (z.B. 0 für I0, 1 für I1).")]
            int index)
        {
            System.Diagnostics.Debug.WriteLine($"[Tool] Toggle Input {index}...");
            try
            {
                var isConnected = await Client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new IOResult { Success = false, Message = "Desktop-Anwendung ist nicht erreichbar." };
                }

                var result = await Client.ToggleInputAsync(index);
                return new IOResult { Success = true, DataJson = result, Message = $"Input {index} umgeschaltet" };
            }
            catch (Exception ex)
            {
                return new IOResult { Success = false, Message = $"Fehler: {ex.Message}" };
            }
        }

        // ====================================================
        // Output Tools
        // ====================================================

        /// <summary>
        /// Ruft die Zustände aller Outputs ab.
        /// </summary>
        [McpServerTool(Name = "get_output_states")]
        [Description("Ruft die aktuellen Zustände aller Outputs der Steuerung ab. Gibt für jeden Output den Index, den booleschen Wert und das Label zurück.")]
        public static async Task<IOResult> GetOutputStates()
        {
            System.Diagnostics.Debug.WriteLine($"[Tool] Rufe Output-Zustände ab...");
            try
            {
                var isConnected = await Client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new IOResult { Success = false, Message = "Desktop-Anwendung ist nicht erreichbar." };
                }

                var result = await Client.GetOutputsAsync();
                return new IOResult { Success = true, DataJson = result, Message = "Output-Zustände abgerufen" };
            }
            catch (Exception ex)
            {
                return new IOResult { Success = false, Message = $"Fehler: {ex.Message}" };
            }
        }

        /// <summary>
        /// Setzt alle Outputs auf false zurück.
        /// </summary>
        [McpServerTool(Name = "reset_outputs")]
        [Description("Setzt alle Outputs der Steuerung auf false zurück. Nützlich um einen definierten Ausgangszustand herzustellen.")]
        public static async Task<IOResult> ResetOutputs()
        {
            System.Diagnostics.Debug.WriteLine($"[Tool] Setze Outputs zurück...");
            try
            {
                var isConnected = await Client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new IOResult { Success = false, Message = "Desktop-Anwendung ist nicht erreichbar." };
                }

                var result = await Client.ResetOutputsAsync();
                return new IOResult { Success = true, DataJson = result, Message = "Alle Outputs zurückgesetzt" };
            }
            catch (Exception ex)
            {
                return new IOResult { Success = false, Message = $"Fehler: {ex.Message}" };
            }
        }

        // ====================================================
        // Memory Tools
        // ====================================================

        /// <summary>
        /// Ruft Memory-Werte in einem bestimmten Bereich ab.
        /// </summary>
        [McpServerTool(Name = "get_memory_values")]
        [Description("Ruft die aktuellen booleschen Werte des Memory-Bereichs der Steuerung ab. Gibt für jede Speicherzelle den Index und den Wert zurück. Standard: M0-M63.")]
        public static async Task<IOResult> GetMemoryValues(
            [Description("Start-Index des Memory-Bereichs (Standard: 0).")]
            int from = 0,
            [Description("End-Index des Memory-Bereichs (Standard: 63).")]
            int to = 63)
        {
            System.Diagnostics.Debug.WriteLine($"[Tool] Rufe Memory ab: {from}-{to}...");
            try
            {
                var isConnected = await Client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new IOResult { Success = false, Message = "Desktop-Anwendung ist nicht erreichbar." };
                }

                var result = await Client.GetMemoryAsync(from, to);
                return new IOResult { Success = true, DataJson = result, Message = $"Memory M{from}-M{to} abgerufen" };
            }
            catch (Exception ex)
            {
                return new IOResult { Success = false, Message = $"Fehler: {ex.Message}" };
            }
        }

        // ====================================================
        // Timer Tools
        // ====================================================

        /// <summary>
        /// Ruft Timer-Zustände in einem bestimmten Bereich ab.
        /// </summary>
        [McpServerTool(Name = "get_timer_states")]
        [Description("Ruft die aktuellen booleschen Zustände der Timer der Steuerung ab. Timer sind pulsierend (alternieren zwischen true/false). Standard: T0-T15.")]
        public static async Task<IOResult> GetTimerStates(
            [Description("Start-Index des Timer-Bereichs (Standard: 0).")]
            int from = 0,
            [Description("End-Index des Timer-Bereichs (Standard: 15).")]
            int to = 15)
        {
            System.Diagnostics.Debug.WriteLine($"[Tool] Rufe Timer ab: {from}-{to}...");
            try
            {
                var isConnected = await Client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new IOResult { Success = false, Message = "Desktop-Anwendung ist nicht erreichbar." };
                }

                var result = await Client.GetTimersAsync(from, to);
                return new IOResult { Success = true, DataJson = result, Message = $"Timer T{from}-T{to} abgerufen" };
            }
            catch (Exception ex)
            {
                return new IOResult { Success = false, Message = $"Fehler: {ex.Message}" };
            }
        }

        // ====================================================
        // Counter Tools
        // ====================================================

        /// <summary>
        /// Ruft Counter-Werte in einem bestimmten Bereich ab.
        /// </summary>
        [McpServerTool(Name = "get_counter_values")]
        [Description("Ruft die aktuellen Integer-Werte der Counter der Steuerung ab. Standard: C0-C15.")]
        public static async Task<IOResult> GetCounterValues(
            [Description("Start-Index des Counter-Bereichs (Standard: 0).")]
            int from = 0,
            [Description("End-Index des Counter-Bereichs (Standard: 15).")]
            int to = 15)
        {
            System.Diagnostics.Debug.WriteLine($"[Tool] Rufe Counter ab: {from}-{to}...");
            try
            {
                var isConnected = await Client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new IOResult { Success = false, Message = "Desktop-Anwendung ist nicht erreichbar." };
                }

                var result = await Client.GetCountersAsync(from, to);
                return new IOResult { Success = true, DataJson = result, Message = $"Counter C{from}-C{to} abgerufen" };
            }
            catch (Exception ex)
            {
                return new IOResult { Success = false, Message = $"Fehler: {ex.Message}" };
            }
        }

        // ====================================================
        // Cycle Time Tools
        // ====================================================

        /// <summary>
        /// Ruft die aktuelle Zykluszeit der Steuerung ab.
        /// </summary>
        [McpServerTool(Name = "get_cycle_time")]
        [Description("Ruft die aktuelle Zykluszeit der Steuerung in Millisekunden ab. Die Zykluszeit bestimmt, wie schnell das Programm wiederholt ausgeführt wird.")]
        public static async Task<IOResult> GetCycleTime()
        {
            System.Diagnostics.Debug.WriteLine($"[Tool] Rufe Zykluszeit ab...");
            try
            {
                var isConnected = await Client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new IOResult { Success = false, Message = "Desktop-Anwendung ist nicht erreichbar." };
                }

                var result = await Client.GetCycleTimeAsync();
                return new IOResult { Success = true, DataJson = result, Message = "Zykluszeit abgerufen" };
            }
            catch (Exception ex)
            {
                return new IOResult { Success = false, Message = $"Fehler: {ex.Message}" };
            }
        }

        /// <summary>
        /// Setzt die Zykluszeit der Steuerung.
        /// </summary>
        [McpServerTool(Name = "set_cycle_time")]
        [Description("Setzt die Zykluszeit der Steuerung in Millisekunden. Kleinere Werte = schnellere Ausführung. Minimum: 1ms, empfohlen: 50-100ms.")]
        public static async Task<IOResult> SetCycleTime(
            [Description("Die gewünschte Zykluszeit in Millisekunden (min. 1ms).")]
            int cycleTimeMs)
        {
            System.Diagnostics.Debug.WriteLine($"[Tool] Setze Zykluszeit: {cycleTimeMs}ms...");
            try
            {
                var isConnected = await Client.IsConnectedAsync();
                if (!isConnected)
                {
                    return new IOResult { Success = false, Message = "Desktop-Anwendung ist nicht erreichbar." };
                }

                if (cycleTimeMs < 1)
                {
                    return new IOResult { Success = false, Message = "Zykluszeit muss mindestens 1ms betragen." };
                }

                var result = await Client.SetCycleTimeAsync(cycleTimeMs);
                return new IOResult { Success = true, DataJson = result, Message = $"Zykluszeit auf {cycleTimeMs}ms gesetzt" };
            }
            catch (Exception ex)
            {
                return new IOResult { Success = false, Message = $"Fehler: {ex.Message}" };
            }
        }
    }

    #region Result Classes

    public class IOResult
    {
        public bool Success { get; set; }
        public string? DataJson { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    #endregion
}
