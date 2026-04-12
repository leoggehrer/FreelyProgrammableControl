namespace FreelyProgrammableControl.McpServer.Models
{
    /// <summary>
    /// Response model for the /api/start and /api/stop endpoints.
    /// Returned after a start or stop command is sent to the desktop application.
    /// </summary>
    public class ExecutionControlResponse
    {
        /// <summary>
        /// Indicates whether the start/stop command was executed successfully.
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// Reflects the current running state of the controller after the command.
        /// </summary>
        public bool isRunning { get; set; }

        /// <summary>
        /// Error description if <see cref="success"/> is false; otherwise null.
        /// </summary>
        public string? error { get; set; }
    }
}
