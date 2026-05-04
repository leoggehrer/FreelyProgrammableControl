namespace FreelyProgrammableControl.McpServer.Models
{
    public class ProgramRetrieveResult
    {
        public bool Success { get; set; }
        public string ProgramCode { get; set; } = string.Empty;
        public int SourceLines { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}