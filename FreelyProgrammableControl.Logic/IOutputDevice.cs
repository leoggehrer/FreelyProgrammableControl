namespace FreelyProgrammableControl.Logic
{
    public interface IOutputDevice : IDevice, ISubject
    {
        new bool Value { get;  set; }
    }
}
