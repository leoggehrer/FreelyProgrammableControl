namespace FreelyProgrammableControl.Logic.Contracts
{
    /// <summary>
    /// Defines a readable input device.
    /// </summary>
    public interface IInputDevice : IDevice, ISubject
    {
        /// <summary>
        /// Gets a value indicating whether the input can be modified manually.
        /// </summary>
        bool Modifiable { get; }
    }
}
