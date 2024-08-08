namespace FreelyProgrammableControl.Logic
{
    /// <summary>
    /// Represents a device with a label and a boolean value.
    /// </summary>
    public interface IDevice
    {
        /// <summary>
        /// Gets the label associated with the property.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that represents the label.
        /// </value>
        string Label { get; }
        /// <summary>
        /// Gets a value indicating whether the condition is true or false.
        /// </summary>
        /// <value>
        /// <c>true</c> if the condition is met; otherwise, <c>false</c>.
        /// </value>
        bool Value { get; }
    }
}
