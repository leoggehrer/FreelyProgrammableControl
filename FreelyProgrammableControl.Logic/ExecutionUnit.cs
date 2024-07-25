namespace FreelyProgrammableControl.Logic
{
    public class ExecutionUnit(int inputs, int outputs)
    {
        #region  fields
        private volatile bool running = false;
        private readonly List<string> source = [];

        private readonly Stack<bool> stack = new();

        private readonly Memory<bool> memory = new(1024);
        private readonly Timers timers = new(264);
        private readonly Counters counters = new(264);
        private readonly Inputs inputs = new(inputs);
        private readonly Outputs outputs = new(outputs);
        #endregion fields

        #region  properties
        public int MemoryLength => memory.Length;
        public int TimerLength => timers.Length;
        public int CounterLength => counters.Length;
        public int InputLength => inputs.Length;
        public int OutputLength => outputs.Length;
        #endregion properties

        #region constructors
        public ExecutionUnit()
            : this(64, 64)
        {
        }
        #endregion constructors

        #region  methods
        public void SetSource(IEnumerable<string> source)
        {
            this.source.Clear();
            this.source.AddRange(source);
        }
        public void Start()
        {
            if (running == false)
            {
                var thread = new Thread(Run);

                thread.IsBackground = true;
                thread.Start();
            }
        }
        public void Stop()
        {
            running = false;
        }
        private void Run()
        {
            running = true;
            while (running)
            {
                Execute(source);
                if (running)
                {
                    Thread.Sleep(200);
                }
            }
        }
        private void Execute(List<string> source)
        {
        }
        #endregion methods
    }
}
