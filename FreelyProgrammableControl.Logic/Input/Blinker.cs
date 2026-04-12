namespace FreelyProgrammableControl.Logic.Input
{
    /// <summary>
    /// A <see cref="Switch"/> that automatically toggles its value at a fixed interval.
    /// Useful for simulating pulsing input signals in test scenarios.
    /// The toggle loop runs on a background task and cannot be stopped once started.
    /// </summary>
    public class Blinker : Switch
    {
        /// <summary>
        /// Initializes a new <see cref="Blinker"/> with the specified toggle interval
        /// and immediately starts the background toggle loop.
        /// </summary>
        /// <param name="interval">Time between each toggle (on→off or off→on).</param>
        public Blinker(TimeSpan interval)
        {
            _modifable = false;
            Interval = interval;
            Task.Run(() => {
                while (true)
                {
                    _value = !_value;
                    NotifyAsync();
                    Task.Delay(Interval).Wait();
                }
            });
        }

        /// <summary>
        /// The time between each automatic toggle of the input value.
        /// </summary>
        public TimeSpan Interval { get; }
    }
}
