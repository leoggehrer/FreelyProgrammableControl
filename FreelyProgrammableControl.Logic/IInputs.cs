namespace FreelyProgrammableControl.Logic
{
    /// <summary>
    /// Represents a collection of input devices that can be accessed by index.
    /// </summary>
    /// <remarks>
    /// This interface extends the <see cref="ISubject"/> interface, allowing for additional functionality related to input devices.
    /// </remarks>
    public interface IInputs : ISubject
    {
        IInputDevice this[int index] { get; set; }
        /// <summary>
        /// Gets the length of the object.
        /// </summary>
        /// <value>
        /// An integer representing the length.
        /// </value>
        int Length { get; }
        /// <summary>
        /// Retrieves the value at the specified position.
        /// </summary>
        /// <param name="position">The zero-based index of the position to retrieve the value from.</param>
        /// <returns>
        /// <c>true</c> if the value at the specified position is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the <paramref name="position"/> is less than zero or greater than the maximum allowable index.
        /// </exception>
        bool GetValue(int position);
    }
}