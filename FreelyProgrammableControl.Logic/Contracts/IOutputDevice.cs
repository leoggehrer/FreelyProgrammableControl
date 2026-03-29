namespace FreelyProgrammableControl.Logic.Contracts
{
    /// <summary>
    /// Defines a writable output device.
    /// </summary>
    public interface IOutputDevice : IDevice, ISubject
    {
        /// <summary>
        /// Gets or sets the current output state.
        /// </summary>
        new bool Value { get; set; }
    }
}
