namespace FreelyProgrammableControl.McpServer.Models
{
    public class ExecutionStateResponse
    {
        public bool isRunning { get; set; }
        public bool debugEnabled { get; set; }
        public string? executionState { get; set; }
    }
}
