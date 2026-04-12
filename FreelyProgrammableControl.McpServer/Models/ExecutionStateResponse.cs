namespace FreelyProgrammableControl.McpServer.Models
{
    /// <summary>
    /// Response model for the /api/state endpoint.
    /// Provides the detailed runtime state of the FPC controller.
    /// </summary>
    public class ExecutionStateResponse
    {
        /// <summary>
        /// Indicates whether the FPC program is currently executing.
        /// </summary>
        public bool isRunning { get; set; }

        /// <summary>
        /// Indicates whether debug mode is currently active.
        /// </summary>
        public bool debugEnabled { get; set; }

        /// <summary>
        /// Human-readable summary of the current execution state
        /// including timestamp, running flag, debug flag, and current command.
        /// </summary>
        public string? executionState { get; set; }
    }
}
