using System.ComponentModel;
using FreelyProgrammableControl.McpServer.Models;
using ModelContextProtocol.Server;

namespace FreelyProgrammableControl.McpServer.Tools
{
    /// <summary>
    /// MCP-Tool für kontrollierte Verzögerungen in Millisekunden.
    /// </summary>
    [McpServerToolType]
    public static class DelayTool
    {
        [McpServerTool(Name = "delay_ms")]
        [Description("Waits for the specified duration in milliseconds. Useful for intentionally adding a pause in tool chains.")]
        public static async Task<DelayResult> DelayMs(
            [Description("Delay duration in milliseconds (0-60000).")]
            int milliseconds)
        {
            try
            {
                if (milliseconds < 0)
                {
                    return new DelayResult
                    {
                        Success = false,
                        Message = "Die Wartezeit darf nicht negativ sein."
                    };
                }

                if (milliseconds > 60_000)
                {
                    return new DelayResult
                    {
                        Success = false,
                        Message = "Die maximale Wartezeit beträgt 60000 ms."
                    };
                }

                await Task.Delay(milliseconds);

                return new DelayResult
                {
                    Success = true,
                    Message = $"Verzögerung von {milliseconds} ms abgeschlossen."
                };
            }
            catch (Exception ex)
            {
                return new DelayResult
                {
                    Success = false,
                    Message = $"Fehler bei der Verzögerung: {ex.Message}"
                };
            }
        }
    }
}
