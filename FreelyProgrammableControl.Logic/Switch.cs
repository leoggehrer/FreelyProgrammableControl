namespace FreelyProgrammableControl.Logic
{
    /// <summary>
    /// Represents a switch input device that can be toggled on or off.
    /// </summary>
    /// <remarks>
    /// This class inherits from the <see cref="Subject"/> class and implements the <see cref="IInputDevice"/> interface.
    /// It provides functionality to toggle its state and notify observers of state changes.
    /// </remarks>
    public class Switch : Subject, IInputDevice
    {
        #region fields
        private bool _value = false;
        #endregion fields

        #region  properties
        /// <summary>
        /// Gets or sets the label associated with the property.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the label. This property is required.
        /// </value>
        public required string Label { get; set; }
        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        /// <value>
        /// <c>true</c> if the value is true; otherwise, <c>false</c>.
        /// </value>
        public bool Value
        {
            get => _value;
        }
        #endregion  properties

        #region methods
        /// <summary>
        /// Toggles the current value between true and false.
        /// </summary>
        /// <remarks>
        /// This method inverts the current state of the <c>_value</c> field and then calls
        /// the <c>Notify</c> method to inform any subscribers of the change.
        /// </remarks>
        public void Toggle()
        {
            _value = !_value;
            Notify();
        }
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string in the format "Switch [Value]", where Value is the value of the current object.
        /// </returns>
        public override string ToString()
        {
            return $"Switch [{Value}]";
        }
        #endregion methods
    }
}
