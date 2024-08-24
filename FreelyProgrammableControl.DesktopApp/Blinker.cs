using System;
using System.Threading.Tasks;

namespace FreelyProgrammableControl.DesktopApp;

public class Blinker : Logic.Switch
{
    public Blinker(TimeSpan interval)
    {
        Interval = interval;
        Task.Run(() => {
            while (true)
            {
                Toggle();
                Task.Delay(Interval).Wait();
            }
        });
    }

    public TimeSpan Interval { get; }

}
