namespace FreelyProgrammableControl.McpServer.Models
{
    /// <summary>
    /// Response model for the /api/program GET endpoint.
    /// Contains the FPC source code currently loaded in the desktop application.
    /// </summary>
    public class ProgramResponse
    {
        /// <summary>
        /// The full FPC source code as a multi-line string.
        /// Each line contains one instruction or comment.
        /// Null if no program is loaded.
        /// </summary>
        public string? programCode { get; set; }

        /// <summary>
        /// Number of source code lines in the loaded program.
        /// </summary>
        public int sourceLines { get; set; }
    }
}
