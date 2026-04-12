namespace FreelyProgrammableControl.McpServer.Models
{
    /// <summary>
    /// Response model for the /api/step endpoint.
    /// Returned after a single debug step is executed on the controller.
    /// </summary>
    public class StepResponse
    {
        /// <summary>
        /// Indicates whether the step was executed successfully.
        /// False if the controller is not running or debug mode is disabled.
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// Human-readable summary of the execution state after the step,
        /// including the next instruction to be executed.
        /// </summary>
        public string? executionState { get; set; }
    }
}
