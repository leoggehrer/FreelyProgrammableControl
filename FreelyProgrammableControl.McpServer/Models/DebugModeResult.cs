namespace FreelyProgrammableControl.McpServer.Models
{
    public class DebugModeResult
    {
        public bool Success { get; set; }
        public bool DebugEnabled { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}