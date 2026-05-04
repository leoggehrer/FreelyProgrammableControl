namespace FreelyProgrammableControl.McpServer.Models
{
    public class ControllerResetResult
    {
        public bool Success { get; set; }
        public bool IsRunning { get; set; }
        public bool DebugEnabled { get; set; }
        public int SourceLines { get; set; }
        public int InputsReset { get; set; }
        public int OutputsReset { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}