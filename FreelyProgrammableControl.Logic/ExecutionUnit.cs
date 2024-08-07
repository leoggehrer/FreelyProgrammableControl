namespace FreelyProgrammableControl.Logic
{
    public class ExecutionUnit(int inputs, int outputs) : Subject
    {
        #region  fields
        private volatile bool running = false;
        private readonly List<ParsedLine> parsedLines = [];

        private readonly Stack<bool> stack = new();

        private readonly Memory<bool> memory = new(1024);
        private readonly Timers timers = new(264);
        private readonly Counters counters = new(264);
        private readonly Inputs inputs = new(inputs);
        private readonly Outputs outputs = new(outputs);
        #endregion fields

        #region  properties
        public bool HasParseError { get; private set; }
        public string? ParseErrorMessage { get; private set; }
        public bool HasExecutionError { get; private set; }
        public string? ExecutionErrorMessage { get; private set; }
        public bool IsRunning => running;
        public int MemoryLength => memory.Length;
        public int TimerLength => timers.Length;
        public string[] Source => parsedLines.Select(e => e.Source).ToArray();

        public IInputs Inputs => inputs;
        public IOutputs Outputs => outputs;
        public ICounters Counters => counters;
        #endregion properties

        #region constructors
        public ExecutionUnit()
            : this(64, 64)
        {
        }
        #endregion constructors

        #region  methods
        public static ParsedLine[] Parse(IEnumerable<string> source)
        {
            var result = new List<ParsedLine>();
            var lineNumber = 0;

            foreach (var item in source.Where(l => string.IsNullOrEmpty(l) ==false))
            {
                result.Add(new ParsedLine(lineNumber++, item));
            }
            return [..result];
        }
        public void LoadSource(IEnumerable<string> source)
        {
            if (running) throw new Exception("Cannot set source while running");

            HasParseError = false;
            ParseErrorMessage = null;
            parsedLines.Clear();
            parsedLines.AddRange(Parse(source));

            HasParseError = parsedLines.Any(e => e.HasError);
            ParseErrorMessage = parsedLines.FirstOrDefault(e => e.HasError)?.ErrorMessage;
        }
        public void Start()
        {
            if (running == false && HasParseError == false && parsedLines.Any(e => e.IsComment == false))
            {
                var thread = new Thread(Run) { IsBackground = true };

                HasExecutionError = false;
                ExecutionErrorMessage = null;

                Reset();
                
                running = true;
                thread.Start();
            }
        }
        public void Stop()
        {
            running = false;
            Reset();
            NotifyAsync();
        }
        private void Run()
        {
            running = true;
            while (running)
            {
                foreach (var parsedLine in parsedLines)
                {
                    Execute(parsedLine);
                }
                NotifyAsync();

                if (running)
                {
                    Thread.Sleep(100);
                }
            }
        }
        private void Reset()
        {
            memory.Reset();
            timers.Reset();
            outputs.Reset();
            counters.Reset();
        }
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
                                    counters.SetValue(parsedLine.Address,parsedLine.Value);
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
                        case "INC":
                            opd_A = stack.Pop();
                            if (opd_A)
                            {
                                counters.SetValue(parsedLine.Address, counters.GetValue(parsedLine.Address) + 1);
                            }
                            break;
                        case "DEC":
                            opd_A = stack.Pop();
                            if (opd_A)
                            {
                                counters.SetValue(parsedLine.Address, counters.GetValue(parsedLine.Address) - 1);
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
