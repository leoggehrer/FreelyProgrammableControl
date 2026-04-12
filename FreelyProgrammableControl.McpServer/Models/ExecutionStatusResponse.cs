namespace FreelyProgrammableControl.McpServer.Models
{
    /// <summary>
    /// Response model for the /api/status endpoint.
    /// Represents the current execution state of the FPC desktop application.
    /// </summary>
    public class ExecutionStatusResponse
    {
        /// <summary>
        /// Indicates whether the FPC program is currently executing.
        /// </summary>
        public bool isRunning { get; set; }

        /// <summary>
        /// Indicates whether the loaded program contains parse errors.
        /// </summary>
        public bool hasParseError { get; set; }

        /// <summary>
        /// The parse error message if <see cref="hasParseError"/> is true; otherwise null.
        /// </summary>
        public string? parseErrorMessage { get; set; }

        /// <summary>
        /// Indicates whether debug mode is currently enabled.
        /// In debug mode the program pauses at each instruction until stepped manually.
        /// </summary>
        public bool debugEnabled { get; set; }

        /// <summary>
        /// Number of source code lines in the currently loaded program.
        /// </summary>
        public int sourceLines { get; set; }

        /// <summary>
        /// Human-readable summary of the current execution state
        /// including timestamp, running flag, debug flag, and current command.
        /// </summary>
        public string? executionState { get; set; }
    }
}
