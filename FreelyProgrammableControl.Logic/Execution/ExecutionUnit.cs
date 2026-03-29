using System.Text;
using FreelyProgrammableControl.Logic.Common;
using FreelyProgrammableControl.Logic.Contracts;
using FreelyProgrammableControl.Logic.Counter;
using FreelyProgrammableControl.Logic.Extensions;
using FreelyProgrammableControl.Logic.Input;
using FreelyProgrammableControl.Logic.Output;

namespace FreelyProgrammableControl.Logic.Execution
{
    /// <summary>
    /// Represents an execution unit that processes parsed lines of instructions.
    /// </summary>
    /// <remarks>
    /// This class inherits from <see cref="Subject"/> and manages the execution of a set of instructions,
    /// handling inputs, outputs, memory, timers, and counters.
    /// </remarks>
    public class ExecutionUnit(int inputs, int outputs) : Subject, IExecutionUnit
    {
        #region constants
        /// <summary>
        /// Default size for memory in boolean values.
        /// </summary>
        private const int DefaultMemorySize = 1024;

        /// <summary>
        /// Default number of available timers.
        /// </summary>
        private const int DefaultTimerCount = 128;

        /// <summary>
        /// Default number of available counters.
        /// </summary>
        private const int DefaultCounterCount = 128;

        /// <summary>
        /// Default cycle time in milliseconds for the execution loop.
        /// </summary>
        private const int DefaultCycleTimeMs = 50;

        /// <summary>
        /// Minimum allowed cycle time in milliseconds to prevent busy loops.
        /// </summary>
        private const int MinCycleTimeMs = 1;
        #endregion constants

        #region  fields
        private volatile bool running = false;
        private bool debugEnabled = false;
        private int cycleTimeMs = DefaultCycleTimeMs;
        private int currentLineNumber = -1;
        private ParsedLine? currentExecutionLine = null;
        private readonly List<ParsedLine> parsedLines = [];

        private readonly Common.Stack<bool> stack = new();

        private readonly Outputs outputs = new(outputs);
        private readonly Inputs inputs = new(inputs);

        private readonly Memory<bool> memory = new(DefaultMemorySize);
        private readonly Timers timers = new(DefaultTimerCount);
        private readonly Counters counters = new(DefaultCounterCount);
        #endregion fields

        #region  properties
        /// <summary>
        /// Gets a string representation of the current state of the execution unit.
        /// </summary>
        public string State
        {
            get
            {
                var result = new StringBuilder();

                result.AppendLine($"Timestamp: {DateTime.Now:HH:mm:ss.fff}");
                result.AppendLine($"Running:   {running}");
                result.AppendLine($"Debug:     {debugEnabled}");
                if (debugEnabled)
                {
                    if (currentExecutionLine != null)
                    {
                        if (currentExecutionLine.IsComment)
                        {
                            result.AppendLine($"Comment: {currentExecutionLine.Source}");
                        }
                        else
                        {
                            var lineInfo = $"Command: {currentExecutionLine.LineNumber:d4} - {(currentExecutionLine.Source.HasContent() ? currentExecutionLine.Source : currentExecutionLine.Instruction)}";

                            if (currentExecutionLine.HasError)
                            {
                                lineInfo += $"  !!! ERROR: {currentExecutionLine.ErrorMessage}";
                            }
                            result.AppendLine(lineInfo);
                        }
                    }
                }
                else
                {
                    result.AppendLine($"Cycle Time (ms): {cycleTimeMs}");
                }
                return result.ToString();
            }
        }
        /// <summary>
        /// Gets a string containing debug information about the currently executing line of code, if any.
        /// </summary>
        public string DebugInfo
        {
            get
            {
                var result = new StringBuilder();

                if (currentExecutionLine != null)
                {
                    var lineInfo = $"Executed Line: {currentExecutionLine.LineNumber:d4} - {(currentExecutionLine.Source.HasContent() ? currentExecutionLine.Source : currentExecutionLine.Instruction)}";

                    if (currentExecutionLine.HasError)
                    {
                        lineInfo += $"  !!! ERROR: {currentExecutionLine.ErrorMessage}";
                    }
                    result.AppendLine($"Timestamp: {DateTime.Now:HH:mm:ss.fff}");
                    result.AppendLine(lineInfo);
                }
                return result.ToString();
            }
        }
        /// <summary>
        /// Gets a string representation of the current state of the stack, including a timestamp.
        /// </summary>
        public string StackInfo
        {
            get
            {
                var result = new StringBuilder();

                result.AppendLine($"Timestamp: {DateTime.Now:HH:mm:ss.fff}");
                result.AppendLine("Stack contents:");
                result.AppendLine(stack.ToString());

                return result.ToString();
            }
        }
        /// <summary>
        /// Gets a string representation of the current state of the inputs, including a timestamp.
        /// </summary>
        public string InputsInfo
        {
            get
            {
                var result = new StringBuilder();

                result.AppendLine($"Timestamp: {DateTime.Now:HH:mm:ss.fff}");
                result.AppendLine("Input values:");
                result.AppendLine(inputs.ToString());

                return result.ToString();
            }
        }
        /// <summary>
        /// Gets a string representation of the current state of the outputs, including a timestamp.
        /// </summary>
        public string OutputsInfo
        {
            get
            {
                var result = new StringBuilder();

                result.AppendLine($"Timestamp: {DateTime.Now:HH:mm:ss.fff}");
                result.AppendLine("Output values:");
                result.AppendLine(outputs.ToString());

                return result.ToString();
            }
        }
        /// <summary>
        /// Gets a string representation of the current state of the memory, including a timestamp.
        /// </summary>
        public string MemoryInfo
        {
            get
            {
                var result = new StringBuilder();

                result.AppendLine($"Timestamp: {DateTime.Now:HH:mm:ss.fff}");
                result.AppendLine("Memory contents:");
                result.AppendLine(memory.ToString());

                return result.ToString();
            }
        }
        /// <summary>
        /// Gets a string representation of the current state of the timers, including a timestamp.
        /// </summary>
        public string CountersInfo
        {
            get
            {
                var result = new StringBuilder();

                result.AppendLine($"Timestamp: {DateTime.Now:HH:mm:ss.fff}");
                result.AppendLine("Counter contents:");
                result.AppendLine(counters.ToString());

                return result.ToString();
            }
        }
        /// <summary>
        /// Gets a string representation of the current state of the timers, including a timestamp.
        /// </summary>
        public string TimersInfo
        {
            get
            {
                var result = new StringBuilder();

                result.AppendLine($"Timestamp: {DateTime.Now:HH:mm:ss.fff}");
                result.AppendLine("Timer contents:");
                result.AppendLine(timers.ToString());

                return result.ToString();
            }
        }

        /// <summary>
        ///  Gets the currently executing line of code, if any.
        /// </summary>
        public ParsedLine? CurrentExecutionLine => currentExecutionLine;

        /// <summary>
        /// Gets or sets a value indicating whether debug mode is enabled.
        /// </summary>
        public bool DebugEnabled
        {
            get => debugEnabled;
            set
            {
                if (IsRunning == false)
                {
                    debugEnabled = value;
                    NotifyAsync();
                }
            }
        }
        /// <summary>
        /// Gets a value indicating whether there was a parse error.
        /// </summary>
        /// <value>
        /// <c>true</c> if a parse error occurred; otherwise, <c>false</c>.
        /// </value>
        public bool HasParseError { get; private set; }
        /// <summary>
        /// Gets the error message that occurred during parsing, if any.
        /// </summary>
        /// <value>
        /// A string containing the error message, or <c>null</c> if there was no error.
        /// </value>
        public string? ParseErrorMessage { get; private set; }
        /// <summary>
        /// Gets a value indicating whether an execution error has occurred.
        /// </summary>
        /// <value>
        /// <c>true</c> if an execution error has occurred; otherwise, <c>false</c>.
        /// </value>
        public bool HasExecutionError { get; private set; }
        /// <summary>
        /// Gets the error message associated with the execution, if any.
        /// </summary>
        /// <value>
        /// A string representing the error message. This property can be null if there is no error.
        /// </value>
        public string? ExecutionErrorMessage { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the process is currently running.
        /// </summary>
        /// <value>
        /// <c>true</c> if the process is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning => running;
        /// <summary>
        /// Gets the length of the memory array.
        /// </summary>
        /// <value>
        /// The length of the memory array as an integer.
        /// </value>
        public int MemoryLength => memory.Length;
        /// <summary>
        /// Gets the length of the timers array.
        /// </summary>
        /// <value>
        /// The number of elements in the timers array.
        /// </value>
        public int TimerLength => timers.Length;
        /// <summary>
        /// Gets an array of source strings extracted from the parsed lines.
        /// </summary>
        /// <returns>
        /// An array of strings representing the source values from each parsed line.
        /// </returns>
        public string[] Source => parsedLines.Select(e => e.Source).ToArray();

        /// <summary>
        /// Gets or sets the cycle time (in milliseconds) for the execution loop.
        /// </summary>
        /// <remarks>
        /// Values &lt;= 0 werden auf <see cref="MinCycleTimeMs"/> ms begrenzt, um einen Busy-Loop zu vermeiden.
        /// </remarks>
        public int CycleTimeMs
        {
            get => cycleTimeMs;
            set => cycleTimeMs = Math.Max(MinCycleTimeMs, value);
        }

        /// <summary>
        /// Gets the inputs associated with this instance.
        /// </summary>
        /// <value>
        /// An <see cref="IInputs"/> instance representing the inputs.
        /// </value>
        public IInputs Inputs => inputs;
        /// <summary>
        /// Gets the outputs associated with this instance.
        /// </summary>
        /// <value>
        /// An <see cref="IOutputs"/> instance representing the outputs.
        /// </value>
        public IOutputs Outputs => outputs;
        /// <summary>
        /// Gets the counters associated with the current instance.
        /// </summary>
        /// <value>An instance of <see cref="ICounters"/> that provides access to the counters.</value>
        public ICounters Counters => counters;

        /// <summary>
        /// Gets the value of a memory cell at the specified position.
        /// </summary>
        /// <param name="position">The zero-based index of the memory cell.</param>
        /// <returns>The boolean value of the memory cell.</returns>
        public bool GetMemoryValue(int position) => memory.GetValue(position);

        /// <summary>
        /// Gets the current value of a timer at the specified position.
        /// </summary>
        /// <param name="position">The zero-based index of the timer.</param>
        /// <returns>The current boolean state of the timer.</returns>
        public bool GetTimerValue(int position) => timers.GetValue(position);
        #endregion properties

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionUnit"/> class with default dimensions.
        /// </summary>
        /// <remarks>
        /// This constructor sets the width and height to 64.
        /// </remarks>
        public ExecutionUnit()
            : this(64, 64)
        {
        }
        #endregion constructors

        #region  methods
        /// <summary>
        /// Prepares the source by removing leading empty lines.
        /// </summary>
        /// <param name="source">The source lines to prepare.</param>
        /// <returns>A collection of strings with leading empty lines removed.</returns>
        public static IEnumerable<string> PrepareSource(IEnumerable<string> source)
        {
            source = source.SkipWhile(i => string.IsNullOrWhiteSpace(i)); // skip leading empty lines
            source = source.Reverse(); // to avoid multiple enumerations
            source = source.SkipWhile(i => string.IsNullOrWhiteSpace(i)); // skip leading empty lines
            source = source.Reverse(); // restore original order

            return source;
        }

        /// <summary>
        /// Parses a collection of strings and converts each non-empty string into a <see cref="ParsedLine"/>.
        /// </summary>
        /// <param name="source">An <see cref="IEnumerable{String}"/> containing the lines to be parsed.</param>
        /// <returns>An array of <see cref="ParsedLine"/> containing the parsed lines with their corresponding line numbers.</returns>
        /// <remarks>
        /// Each non-empty line from the source will be assigned a line number starting from 0.
        /// Empty lines are ignored during parsing.
        /// </remarks>
        public ParsedLine[] Parse(IEnumerable<string> source)
        {
            var result = new List<ParsedLine>();
            var lineNumber = 0;

            foreach (var item in PrepareSource(source))
            {
                result.Add(new ParsedLine(lineNumber++, item));
            }

            foreach (var item in result)
            {
                if (item.HasError == false)
                {
                    switch (item.Subject)
                    {
                        case "I":
                            if (item.Address < 0 || item.Address >= Inputs.Length)
                            {
                                item.HasError = true;
                                item.ErrorMessage = $"Invalid input address '{item.Address}'";
                            }
                            break;
                        case "O":
                            if (item.Address < 0 || item.Address >= Outputs.Length)
                            {
                                item.HasError = true;
                                item.ErrorMessage = $"Invalid output address '{item.Address}'";
                            }
                            break;
                        case "M":
                            if (item.Address < 0 || item.Address >= MemoryLength)
                            {
                                item.HasError = true;
                                item.ErrorMessage = $"Invalid memory address '{item.Address}'";
                            }
                            break;
                        case "T":
                            if (item.Address < 0 || item.Address >= TimerLength)
                            {
                                item.HasError = true;
                                item.ErrorMessage = $"Invalid timer address '{item.Address}'";
                            }
                            break;
                    }
                }
            }
            return [.. result];
        }
        /// <summary>
        /// Executes a single step of the execution unit in debug mode.
        /// </summary>
        public void Step()
        {
            if (running && DebugEnabled)
            {
                currentExecutionLine = parsedLines[currentLineNumber++];
                try
                {
                    currentExecutionLine.HasError = false;
                    currentExecutionLine.ErrorMessage = null;
                    Execute(currentExecutionLine);
                }
                catch (Exception ex)
                {
                    // Fehlerbehandlung erfolgt in Execute-Methode
                    currentExecutionLine.HasError = true;
                    currentExecutionLine.ErrorMessage = ex.Message;
                }

                if (currentLineNumber >= parsedLines.Count)
                {
                    stack.Clear();
                    currentLineNumber = 0;
                }
                currentExecutionLine = parsedLines[currentLineNumber];
                NotifyAsync();
            }
        }
        /// <summary>
        /// Loads a source of strings for processing.
        /// </summary>
        /// <param name="source">An enumerable collection of strings representing the source data to be loaded.</param>
        /// <exception cref="Exception">Thrown when attempting to set the source while the process is running.</exception>
        /// <remarks>
        /// This method clears any previously parsed lines and attempts to parse the new source.
        /// It updates the <c>HasParseError</c> property to indicate if any errors occurred during parsing,
        /// and sets the <c>ParseErrorMessage</c> to the first error message encountered, if any.
        /// </remarks>
        public void LoadSource(IEnumerable<string> source)
        {
            if (running) throw new Exception("Cannot set source while running");

            HasParseError = false;
            ParseErrorMessage = null;
            parsedLines.Clear();

            parsedLines.AddRange(Parse(PrepareSource(source)));

            HasParseError = parsedLines.Any(e => e.HasError);
            ParseErrorMessage = parsedLines.FirstOrDefault(e => e.HasError)?.ErrorMessage;
            NotifyAsync();
        }
        /// <summary>
        /// Initiates the execution process if the current state allows it.
        /// </summary>
        /// <remarks>
        /// This method checks if the process is not already running, there are no parse errors,
        /// and at least one parsed line exists that is not a comment. If these conditions are met,
        /// it starts a new background thread to run the execution logic.
        /// </remarks>
        /// <exception cref="ThreadStartException">
        /// Thrown if the thread cannot be started due to an unexpected error.
        /// </exception>
        public void Start()
        {
            if (running == false
                && HasParseError == false
                && parsedLines.Count > 0
                && parsedLines.Any(e => e.IsComment == false))
            {
                Reset();

                running = true;
                if (DebugEnabled)
                {
                    currentLineNumber = 0;
                    currentExecutionLine = parsedLines.FirstOrDefault();
                }
                else
                {
                    var thread = new Thread(Run) { IsBackground = true };

                    thread.Start();
                }
                NotifyAsync();
            }
        }
        /// <summary>
        /// Stops the current operation, setting the running state to false.
        /// </summary>
        /// <remarks>
        /// This method also resets the necessary components and notifies any asynchronous listeners
        /// of the stop action. It is typically used to gracefully terminate ongoing processes.
        /// </remarks>
        public void Stop()
        {
            running = false;
            Reset();
            NotifyAsync();
        }
        /// <summary>
        /// Executes a loop that processes each parsed line and notifies asynchronously.
        /// </summary>
        /// <remarks>
        /// The method sets the <c>running</c> flag to <c>true</c> and enters a loop that continues until <c>running</c> is set to <c>false</c>.
        /// Within the loop, it iterates over the <c>parsedLines</c> collection, executing each line using the <c>Execute</c> method.
        /// After processing all lines, it calls <c>NotifyAsync</c> to perform any necessary notifications.
        /// If the <c>running</c> flag is still <c>true</c> after processing, it pauses the execution for
        /// </remarks>
        private void Run()
        {
            running = true;
            while (running)
            {
                if (DebugEnabled == false)
                {
                    stack.Clear();
                    foreach (var line in parsedLines)
                    {
                        var executionLine = line;

                        Execute(executionLine);
                    }
                    NotifyAsync();
                }

                if (running)
                {
                    Thread.Sleep(cycleTimeMs);
                }
            }
        }
        /// <summary>
        /// Resets the internal state of the system by invoking the reset methods
        /// on the memory, timers, outputs, and counters components.
        /// </summary>
        private void Reset()
        {
            currentExecutionLine = null;
            currentLineNumber = -1;
            HasExecutionError = false;
            ExecutionErrorMessage = null;

            stack.Clear();
            memory.Reset();
            timers.Reset();
            outputs.Reset();
            counters.Reset();
        }
        /// <summary>
        /// Executes a parsed line of instructions based on its instruction type and subject.
        /// The method processes various instructions such as GET, GETNOT, DUP, NOT, AND, OR, XOR, MOV,
        /// CMOV, SET, CSET, INC, DEC, CMP, GT, and LE. It manipulates a stack and interacts with
        /// inputs, outputs, memory, timers, and counters depending on the parsed line's attributes.
        /// </summary>
        /// <param name="parsedLine">The <see cref="ParsedLine"/> object containing the instruction,
        /// subject, value, address, and a flag indicating if it is a comment.</param>
        /// <exception cref="Exception">Thrown when an unknown instruction is encountered
        private void Execute(ParsedLine parsedLine)
        {
            int value;
            bool opd_A, opd_B;

            try
            {
                if (parsedLine.IsComment == false)
                {
                    switch (parsedLine.Instruction)
                    {
                        case "NOP":
                            // Do nothing
                            break;
                        case "GET":
                            switch (parsedLine.Subject)
                            {
                                case "C_OPD":
                                    stack.Push(parsedLine.Value == 1);
                                    break;
                                case "I":
                                    stack.Push(inputs.GetValue(parsedLine.Address));
                                    break;
                                case "O":
                                    stack.Push(outputs.GetValue(parsedLine.Address));
                                    break;
                                case "M":
                                    stack.Push(memory.GetValue(parsedLine.Address));
                                    break;
                                case "T":
                                    stack.Push(timers.GetValue(parsedLine.Address));
                                    break;
                            }
                            break;
                        case "GETNOT":
                            switch (parsedLine.Subject)
                            {
                                case "I":
                                    stack.Push(!inputs.GetValue(parsedLine.Address));
                                    break;
                                case "O":
                                    stack.Push(!outputs.GetValue(parsedLine.Address));
                                    break;
                                case "M":
                                    stack.Push(!memory.GetValue(parsedLine.Address));
                                    break;
                                case "T":
                                    stack.Push(!timers.GetValue(parsedLine.Address));
                                    break;
                            }
                            break;
                        case "DUP":
                            for (int i = 1; i < parsedLine.Value; i++)
                            {
                                stack.Push(stack.Top());
                            }
                            break;
                        case "NOT":
                            stack.Push(!stack.Pop());
                            break;
                        case "AND":
                            opd_A = stack.Pop();
                            opd_B = stack.Pop();

                            stack.Push(opd_A && opd_B);
                            break;
                        case "OR":
                            opd_A = stack.Pop();
                            opd_B = stack.Pop();

                            stack.Push(opd_A || opd_B);
                            break;
                        case "XOR":
                            opd_A = stack.Pop();
                            opd_B = stack.Pop();

                            stack.Push(opd_A ^ opd_B);
                            break;
                        case "MOV":
                            switch (parsedLine.Subject)
                            {
                                case "O":
                                    outputs.SetValue(parsedLine.Address, stack.Pop());
                                    break;
                                case "M":
                                    memory.SetValue(parsedLine.Address, stack.Pop());
                                    break;
                            }
                            break;
                        case "CMOV":
                            opd_A = stack.Pop();
                            if (opd_A)
                            {
                                switch (parsedLine.Subject)
                                {
                                    case "O":
                                        outputs.SetValue(parsedLine.Address, parsedLine.Value > 0);
                                        break;
                                    case "M":
                                        memory.SetValue(parsedLine.Address, parsedLine.Value > 0);
                                        break;
                                }
                            }
                            break;
                        case "SET":
                            switch (parsedLine.Subject)
                            {
                                case "T":
                                    timers.SetTimer(parsedLine.Address, parsedLine.Value);
                                    break;
                                case "C":
                                    counters.SetValue(parsedLine.Address, parsedLine.Value);
                                    break;
                            }
                            break;
                        case "CSET":
                            opd_A = stack.Pop();
                            if (opd_A)
                            {
                                switch (parsedLine.Subject)
                                {
                                    case "T":
                                        timers.SetTimer(parsedLine.Address, parsedLine.Value);
                                        break;
                                    case "C":
                                        counters.SetValue(parsedLine.Address, parsedLine.Value);
                                        break;
                                }
                            }
                            break;
                        case "CINC":
                            opd_A = stack.Pop();
                            if (opd_A)
                            {
                                counters.SetValue(parsedLine.Address, counters.GetValue(parsedLine.Address) + parsedLine.Value);
                            }
                            break;
                        case "CDEC":
                            opd_A = stack.Pop();
                            if (opd_A)
                            {
                                counters.SetValue(parsedLine.Address, counters.GetValue(parsedLine.Address) - parsedLine.Value);
                            }
                            break;
                        case "CMP":
                            value = counters.GetValue(parsedLine.Address);

                            stack.Push(parsedLine.Value == value);
                            break;
                        case "GT":
                            value = counters.GetValue(parsedLine.Address);

                            stack.Push(value > parsedLine.Value);
                            break;
                        case "LE":
                            value = counters.GetValue(parsedLine.Address);

                            stack.Push(value < parsedLine.Value);
                            break;
                        default:
                            throw new Exception($"Unknown instruction: {parsedLine.Instruction}");
                    }
                }
            }
            catch (Exception ex)
            {
                running = false;
                HasExecutionError = true;
                ExecutionErrorMessage = ex.Message;
                throw;
            }
        }
        #endregion methods
    }
}
