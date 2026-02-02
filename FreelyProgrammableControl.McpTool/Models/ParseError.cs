using System;

namespace FreelyProgrammableControl.McpTool.Models;

   /// <summary>
    /// Details of a single parse error.
    /// </summary>
    public class ParseError
    {
        /// <summary>
        /// The line number where the error occurred (0-based).
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// The source code of the line with the error.
        /// </summary>
        public string? SourceCode { get; set; }

        /// <summary>
        /// Detailed error message explaining what went wrong.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }
