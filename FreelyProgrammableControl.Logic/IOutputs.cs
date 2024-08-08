namespace FreelyProgrammableControl.Logic
{
    /// <summary>
    /// Represents a collection of output devices and provides methods to access their values.
    /// </summary>
    public interface IOutputs : ISubject
    {
        IOutputDevice this[int index] { get; set; }
        /// <summary>
        /// Gets the length of the object.
        /// </summary>
        /// <value>
        /// An integer representing the length.
        /// </value>
        int Length { get; }
        /// <summary>
        /// Retrieves a boolean value based on the specified position.
        /// </summary>
        /// <param name="position">The index of the value to retrieve.</param>
        /// <returns>
        /// <c>true</c> if the value at the specified position is true; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the <paramref name="position"/> is outside the valid range.
        /// </exception>
        bool GetValue(int position);
    }
}