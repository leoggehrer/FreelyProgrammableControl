namespace FreelyProgrammableControl.Logic.Contracts
{
    /// <summary>
    /// Defines read access to a fixed-size memory segment.
    /// </summary>
    /// <typeparam name="T">Type stored in the memory segment.</typeparam>
    internal interface IMemory<T>
    {
        /// <summary>
        /// Gets the number of available memory slots.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Gets the value at the specified position.
        /// </summary>
        /// <param name="position">Zero-based memory index.</param>
        /// <returns>The value at the given index.</returns>
        T GetValue(int position);
    }
}
