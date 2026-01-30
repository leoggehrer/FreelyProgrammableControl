using FreelyProgrammableControl.Logic.Common;

namespace FreelyProgrammableControl.Logic.Output
{
    /// <summary>
    /// Represents an output device that can hold a boolean value.
    /// </summary>
    /// <remarks>
    /// This interface extends the <see cref="IDevice"/> and <see cref="ISubject"/> interfaces,
    /// providing additional functionality specific to output devices.
    /// </remarks>
    public interface IOutputDevice : IDevice, ISubject
    {
        /// <summary>
        /// Gets or sets a value indicating whether the condition is true or false.
        /// </summary>
        /// <value>
        /// <c>true</c> if the condition is met; otherwise, <c>false</c>.
        /// </value>
        new bool Value { get; set; }
    }
}
