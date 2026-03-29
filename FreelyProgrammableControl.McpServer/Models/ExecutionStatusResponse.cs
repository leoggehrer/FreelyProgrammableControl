namespace FreelyProgrammableControl.McpServer.Models
{
    public class ExecutionStatusResponse
    {
        public bool isRunning { get; set; }
        public bool hasParseError { get; set; }
        public string? parseErrorMessage { get; set; }
        public bool debugEnabled { get; set; }
        public int sourceLines { get; set; }
        public string? executionState { get; set; }
    }
}
