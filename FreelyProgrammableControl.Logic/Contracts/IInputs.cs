namespace FreelyProgrammableControl.Logic.Contracts
{
    /// <summary>
    /// Defines indexed access to all configured input devices.
    /// </summary>
    public interface IInputs : ISubject
    {
        /// <summary>
        /// Gets or sets an input device by index.
        /// </summary>
        /// <param name="index">Zero-based device index.</param>
        /// <returns>The input device at the given index.</returns>
        IInputDevice this[int index] { get; set; }

        /// <summary>
        /// Gets the number of input devices.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Gets the current value of the input at the specified position.
        /// </summary>
        /// <param name="position">Zero-based input index.</param>
        /// <returns>The input value at the given index.</returns>
        bool GetValue(int position);
    }
}
