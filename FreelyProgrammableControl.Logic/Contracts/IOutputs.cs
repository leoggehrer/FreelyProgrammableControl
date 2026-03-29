namespace FreelyProgrammableControl.Logic.Contracts
{
    /// <summary>
    /// Defines indexed access to all configured output devices.
    /// </summary>
    public interface IOutputs : ISubject
    {
        /// <summary>
        /// Gets or sets an output device by index.
        /// </summary>
        /// <param name="index">Zero-based device index.</param>
        /// <returns>The output device at the given index.</returns>
        IOutputDevice this[int index] { get; set; }

        /// <summary>
        /// Gets the number of output devices.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Gets the current value of the output at the specified position.
        /// </summary>
        /// <param name="position">Zero-based output index.</param>
        /// <returns>The output value at the given index.</returns>
        bool GetValue(int position);
    }
}
