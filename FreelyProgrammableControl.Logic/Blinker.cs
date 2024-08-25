namespace FreelyProgrammableControl.Logic
{
    public class Blinker : Logic.Switch
    {
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

        public TimeSpan Interval { get; }
    }
}
