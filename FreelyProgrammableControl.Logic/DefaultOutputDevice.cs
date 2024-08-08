namespace FreelyProgrammableControl.Logic
{
    /// <summary>
    /// Represents a default output device that implements the <see cref="IOutputDevice"/> interface.
    /// </summary>
    /// <remarks>
    /// This class derives from <see cref="Subject"/> and provides a property to manage the output device's state.
    /// </remarks>
    public class DefaultOutputDevice : Subject, IOutputDevice
    {
        private bool _value;

        /// <summary>
        /// Gets or sets the label for the property.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that represents the label. This property is required.
        /// </value>
        public required string Label { get; set; }
        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        /// <value>
        /// A <see cref="bool"/> indicating the current value.
        /// Returns <c>true</c> if the value is set to true; otherwise, <c>false</c>.
        /// </value>
        public bool Value
        {
            get => _value;
            set => _value = value;
        }
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string in the format "Output [Value]", where <c>Value</c> is the value of the object.
        /// </returns>
        public override string ToString()
        {
            return $"Output [{Value}]";
        }

    }
}
