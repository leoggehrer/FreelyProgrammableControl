namespace FreelyProgrammableControl.McpServer.Models
{
    public class ExecutionStateResult
    {
        public bool Success { get; set; }
        public string? ExecutionState { get; set; }
        public bool IsRunning { get; set; }
        public bool DebugEnabled { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}