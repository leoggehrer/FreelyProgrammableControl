namespace FreelyProgrammableControl.McpServer.Models
{
    /// <summary>
    /// Result returned by the <c>delay_ms</c> MCP tool
    /// after a controlled pause has completed or been rejected.
    /// </summary>
    public class DelayResult
    {
        /// <summary>
        /// Indicates whether the delay completed successfully.
        /// False if the requested duration was negative or exceeded 60 000 ms.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Human-readable message describing the outcome of the delay operation.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
