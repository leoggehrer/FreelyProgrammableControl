namespace FreelyProgrammableControl.Logic.Contracts
{
    /// <summary>
    /// Defines read access to a counter collection.
    /// </summary>
    public interface ICounters : ISubject
    {
        /// <summary>
        /// Gets the number of counters.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Gets the counter value at the specified position.
        /// </summary>
        /// <param name="position">Zero-based counter index.</param>
        /// <returns>Counter value at the given index.</returns>
        int GetValue(int position);
    }
}
