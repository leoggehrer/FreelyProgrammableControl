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
        [Description("Retrieves the current states of all controller inputs. Returns index, boolean value, and label for each input.")]
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
        [Description("Toggles a single controller input (true to false or false to true). Useful for simulating input signals and testing program behavior.")]
        public static async Task<IOResult> ToggleInput(
            [Description("Index of the input to toggle (e.g., 0 for I0, 1 for I1).")]
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

        /// <summary>
        /// Setzt das Label eines einzelnen Inputs.
        /// </summary>
        [McpServerTool(Name = "set_input_label")]
        [Description("Sets the display label of a single controller input channel (e.g. 'Start button' for I0).")]
        public static async Task<IOResult> SetInputLabel(
            [Description("Index of the input channel (e.g. 0 for I0, 1 for I1).")]
            int index,
            [Description("New label text for the input channel.")]
            string label)
        {
            System.Diagnostics.Debug.WriteLine($"[Tool] Setze Input-Label {index}: \"{label}\"...");
            try
            {
                var isConnected = await Client.IsConnectedAsync();
                if (!isConnected)
                    return new IOResult { Success = false, Message = "Desktop-Anwendung ist nicht erreichbar." };

                var result = await Client.SetInputLabelAsync(index, label);
                return new IOResult { Success = true, DataJson = result, Message = $"Label von Input {index} auf \"{label}\" gesetzt" };
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
        [Description("Retrieves the current states of all controller outputs. Returns index, boolean value, and label for each output.")]
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
        /// Setzt das Label eines einzelnen Outputs.
        /// </summary>
        [McpServerTool(Name = "set_output_label")]
        [Description("Sets the display label of a single controller output channel (e.g. 'Green lamp' for Q0).")]
        public static async Task<IOResult> SetOutputLabel(
            [Description("Index of the output channel (e.g. 0 for Q0, 1 for Q1).")]
            int index,
            [Description("New label text for the output channel.")]
            string label)
        {
            System.Diagnostics.Debug.WriteLine($"[Tool] Setze Output-Label {index}: \"{label}\"...");
            try
            {
                var isConnected = await Client.IsConnectedAsync();
                if (!isConnected)
                    return new IOResult { Success = false, Message = "Desktop-Anwendung ist nicht erreichbar." };

                var result = await Client.SetOutputLabelAsync(index, label);
                return new IOResult { Success = true, DataJson = result, Message = $"Label von Output {index} auf \"{label}\" gesetzt" };
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
        [Description("Resets all controller outputs to false. Useful for restoring a defined initial output state.")]
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
        [Description("Retrieves the current boolean values of the controller memory area. Returns index and value for each memory cell. Default: M0-M63.")]
        public static async Task<IOResult> GetMemoryValues(
            [Description("Start index of the memory range (default: 0).")]
            int from = 0,
            [Description("End index of the memory range (default: 63).")]
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
        [Description("Retrieves the current boolean states of controller timers. Timers are pulsing (alternating between true and false). Default: T0-T15.")]
        public static async Task<IOResult> GetTimerStates(
            [Description("Start index of the timer range (default: 0).")]
            int from = 0,
            [Description("End index of the timer range (default: 15).")]
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
        [Description("Retrieves the current integer values of controller counters. Default: C0-C15.")]
        public static async Task<IOResult> GetCounterValues(
            [Description("Start index of the counter range (default: 0).")]
            int from = 0,
            [Description("End index of the counter range (default: 15).")]
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
        [Description("Retrieves the current controller cycle time in milliseconds. The cycle time determines how fast the program is repeatedly executed.")]
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
        [Description("Sets the controller cycle time in milliseconds. Smaller values mean faster execution. Minimum: 1ms, recommended: 50-100ms.")]
        public static async Task<IOResult> SetCycleTime(
            [Description("Desired cycle time in milliseconds (min. 1ms).")]
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
