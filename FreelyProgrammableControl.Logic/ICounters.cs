namespace FreelyProgrammableControl.Logic
{
    /// <summary>
    /// Represents a collection of counters that can be observed.
    /// </summary>
    public interface ICounters : ISubject
    {
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
        /// <param name="position">The zero-based index of the position from which to retrieve the value.</param>
        /// <returns>The value at the specified position.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the position is outside the valid range.</exception>
        int GetValue(int position);
    }
}