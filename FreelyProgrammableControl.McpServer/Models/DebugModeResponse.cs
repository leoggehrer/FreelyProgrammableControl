namespace FreelyProgrammableControl.McpServer.Models
{
    /// <summary>
    /// Response model for the /api/debug endpoint.
    /// Returned after enabling or disabling debug mode on the controller.
    /// </summary>
    public class DebugModeResponse
    {
        /// <summary>
        /// Indicates whether the debug mode change was applied successfully.
        /// False if the controller is currently running (debug mode can only be changed while stopped).
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// Reflects the current debug mode state after the command.
        /// </summary>
        public bool debugEnabled { get; set; }
    }
}
