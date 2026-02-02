using System;

namespace FreelyProgrammableControl.McpTool.Models;

    /// <summary>
    /// Result of parsing a FPC program.
    /// </summary>
    public class ParseResult
    {
        /// <summary>
        /// Indicates whether the program was parsed successfully without errors.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Total number of parse errors found.
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// List of all parse errors with line numbers and messages.
        /// </summary>
        public List<ParseError> Errors { get; set; } = [];

        /// <summary>
        /// Total number of lines in the program.
        /// </summary>
        public int TotalLines { get; set; }

        /// <summary>
        /// Number of non-comment instruction lines.
        /// </summary>
        public int NonCommentLines { get; set; }

        /// <summary>
        /// Summary message about the parse result.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
