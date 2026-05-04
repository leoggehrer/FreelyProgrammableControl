namespace FreelyProgrammableControl.McpServer.Models
{
    public class ExecutionControlResult
    {
        public bool Success { get; set; }
        public bool IsRunning { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}