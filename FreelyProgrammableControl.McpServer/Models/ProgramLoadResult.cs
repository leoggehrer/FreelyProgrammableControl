namespace FreelyProgrammableControl.McpServer.Models
{
    public class ProgramLoadResult
    {
        public bool Success { get; set; }
        public bool HasParseError { get; set; }
        public string? ParseErrorMessage { get; set; }
        public int SourceLines { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}