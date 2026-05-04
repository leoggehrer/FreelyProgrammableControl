namespace FreelyProgrammableControl.McpServer.Models
{
    public class StepResult
    {
        public bool Success { get; set; }
        public string? ExecutionState { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}