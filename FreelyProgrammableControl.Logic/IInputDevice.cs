namespace FreelyProgrammableControl.Logic
{
    /// <summary>
    /// Represents an input device that can receive input from users or other sources.
    /// </summary>
    /// <remarks>
    /// This interface extends the <see cref="IDevice"/> and <see cref="ISubject"/> interfaces,
    /// indicating that an input device is also a device and can notify observers of state changes.
    /// </remarks>
    public interface IInputDevice : IDevice, ISubject
    {
    }
}
