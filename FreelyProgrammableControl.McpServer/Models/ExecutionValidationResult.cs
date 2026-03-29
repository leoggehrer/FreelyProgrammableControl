namespace FreelyProgrammableControl.McpServer.Models;

   /// <summary>
    /// Result of validating a program for execution.
    /// </summary>
    public class ExecutionValidationResult
    {
        /// <summary>
        /// Indicates whether the program can be executed.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Number of input devices available.
        /// </summary>
        public int InputCount { get; set; }

        /// <summary>
        /// Number of output devices available.
        /// </summary>
        public int OutputCount { get; set; }

        /// <summary>
        /// Total number of source code lines.
        /// </summary>
        public int TotalSourceLines { get; set; }

        /// <summary>
        /// Size of the memory available.
        /// </summary>
        public int MemorySize { get; set; }

        /// <summary>
        /// Number of timers available.
        /// </summary>
        public int TimerCount { get; set; }

        /// <summary>
        /// Parse error message if validation failed.
        /// </summary>
        public string? ParseErrorMessage { get; set; }

        /// <summary>
        /// Indicates if the program can be started (no errors and has instructions).
        /// </summary>
        public bool CanStart { get; set; }

        /// <summary>
        /// Summary message about the validation result.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
