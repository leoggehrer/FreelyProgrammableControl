namespace FreelyProgrammableControl.Logic.Contracts
{
    /// <summary>
    /// Represents a generic named device that exposes a current boolean state.
    /// </summary>
    public partial interface IDevice
    {
        /// <summary>
        /// Gets or sets the display label of the device.
        /// </summary>
        string Label { get; set; }

        /// <summary>
        /// Gets the current state of the device.
        /// </summary>
        bool Value { get; }
    }
}
