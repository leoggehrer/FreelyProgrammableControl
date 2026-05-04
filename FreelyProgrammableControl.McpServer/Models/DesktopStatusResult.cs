namespace FreelyProgrammableControl.McpServer.Models
{
    public class DesktopStatusResult
    {
        public bool IsConnected { get; set; }
        public bool IsRunning { get; set; }
        public bool HasParseError { get; set; }
        public string? ParseErrorMessage { get; set; }
        public bool DebugEnabled { get; set; }
        public int SourceLines { get; set; }
        public string? ExecutionState { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}