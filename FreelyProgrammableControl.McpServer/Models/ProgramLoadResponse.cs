namespace FreelyProgrammableControl.McpServer.Models
{
    /// <summary>
    /// Response model for the /api/program POST endpoint.
    /// Returned after attempting to load a FPC program into the desktop application.
    /// </summary>
    public class ProgramLoadResponse
    {
        /// <summary>
        /// Indicates whether the program was loaded successfully without parse errors.
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// Indicates whether the loaded program contains parse errors.
        /// If true the program cannot be started.
        /// </summary>
        public bool hasParseError { get; set; }

        /// <summary>
        /// The first parse error message if <see cref="hasParseError"/> is true; otherwise null.
        /// Kept for backwards compatibility — prefer <see cref="parseErrors"/> for the full list.
        /// </summary>
        public string? parseErrorMessage { get; set; }

        /// <summary>
        /// All parse errors found in the program.
        /// Each entry is formatted as "Line N: &lt;message&gt;".
        /// Empty when the program is valid.
        /// </summary>
        public List<string> parseErrors { get; set; } = new();

        /// <summary>
        /// Number of source code lines in the loaded program.
        /// </summary>
        public int sourceLines { get; set; }
    }
}
