namespace FreelyProgrammableControl.Logic
{
    public interface IOutputDevice : IDevice
    {
        new bool Value { get;  set; }
    }
}
