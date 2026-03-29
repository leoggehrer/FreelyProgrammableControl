namespace FreelyProgrammableControl.McpServer.Models
{
    public class ProgramLoadResponse
    {
        public bool success { get; set; }
        public bool hasParseError { get; set; }
        public string? parseErrorMessage { get; set; }
        public int sourceLines { get; set; }
    }
}
