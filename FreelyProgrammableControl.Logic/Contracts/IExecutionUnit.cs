using FreelyProgrammableControl.Logic.Execution;

namespace FreelyProgrammableControl.Logic.Contracts
{
    /// <summary>
    /// Defines the public API of the execution engine for loading, parsing and running FPC programs.
    /// </summary>
    public interface IExecutionUnit
    {
        /// <summary>
        /// Gets a human-readable snapshot of the current runtime state.
        /// </summary>
        string State { get; }

        /// <summary>
        /// Gets detailed information about the most recently executed instruction.
        /// </summary>
        string DebugInfo { get; }

        /// <summary>
        /// Gets current stack details.
        /// </summary>
        string StackInfo { get; }

        /// <summary>
        /// Gets current input values as formatted text.
        /// </summary>
        string InputsInfo { get; }

        /// <summary>
        /// Gets current output values as formatted text.
        /// </summary>
        string OutputsInfo { get; }

        /// <summary>
        /// Gets memory contents as formatted text.
        /// </summary>
        string MemoryInfo { get; }

        /// <summary>
        /// Gets counter values as formatted text.
        /// </summary>
        string CountersInfo { get; }

        /// <summary>
        /// Gets timer values as formatted text.
        /// </summary>
        string TimersInfo { get; }

        /// <summary>
        /// Gets the line that is currently active in execution.
        /// </summary>
        ParsedLine? CurrentExecutionLine { get; }

        /// <summary>
        /// Gets or sets whether debug stepping is enabled.
        /// </summary>
        bool DebugEnabled { get; set; }

        /// <summary>
        /// Gets a value indicating whether parsing produced any error.
        /// </summary>
        bool HasParseError { get; }

        /// <summary>
        /// Gets the last parse error message, if available.
        /// </summary>
        string? ParseErrorMessage { get; }

        /// <summary>
        /// Gets a value indicating whether execution produced any runtime error.
        /// </summary>
        bool HasExecutionError { get; }

        /// <summary>
        /// Gets the last runtime error message, if available.
        /// </summary>
        string? ExecutionErrorMessage { get; }

        /// <summary>
        /// Gets a value indicating whether the execution loop is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the configured memory size.
        /// </summary>
        int MemoryLength { get; }

        /// <summary>
        /// Gets the configured timer count.
        /// </summary>
        int TimerLength { get; }

        /// <summary>
        /// Gets the currently loaded source code lines.
        /// </summary>
        string[] Source { get; }

        /// <summary>
        /// Gets or sets the runtime cycle time in milliseconds.
        /// </summary>
        int CycleTimeMs { get; set; }

        /// <summary>
        /// Gets available input devices.
        /// </summary>
        IInputs Inputs { get; }

        /// <summary>
        /// Gets available output devices.
        /// </summary>
        IOutputs Outputs { get; }

        /// <summary>
        /// Gets available counters.
        /// </summary>
        ICounters Counters { get; }

        /// <summary>
        /// Normalizes source lines for parser and runtime processing.
        /// </summary>
        /// <param name="source">Raw source lines.</param>
        /// <returns>Prepared source lines.</returns>
        static abstract IEnumerable<string> PrepareSource(IEnumerable<string> source);

        /// <summary>
        /// Loads a source program into the execution unit.
        /// </summary>
        /// <param name="source">Program source lines.</param>
        void LoadSource(IEnumerable<string> source);

        /// <summary>
        /// Parses source lines without starting execution.
        /// </summary>
        /// <param name="source">Program source lines.</param>
        /// <returns>Parsed line results including parse errors.</returns>
        ParsedLine[] Parse(IEnumerable<string> source);

        /// <summary>
        /// Starts continuous execution of the loaded program.
        /// </summary>
        void Start();

        /// <summary>
        /// Executes exactly one step when debug mode is enabled.
        /// </summary>
        void Step();

        /// <summary>
        /// Stops the execution loop.
        /// </summary>
        void Stop();
    }
}
