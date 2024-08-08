namespace FreelyProgrammableControl.Logic
{
    /// <summary>
    /// Represents a collection of counters that can be reset and updated.
    /// Implements the <see cref="ICounters"/> interface and inherits from <see cref="Subject"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="Counters"/> class maintains an internal array of integer values,
    /// allowing for manipulation of individual counter values and notifying observers
    /// of any changes made to the counters.
    /// </remarks>
    internal class Counters(int length) : Subject, ICounters
    {
        #region  fields
        private readonly int[] values = new int[Math.Max(length, 0)];
        #endregion fields

        #region  properties
        /// <summary>
        /// Gets the length of the <see cref="values"/> array.
        /// </summary>
        /// <value>
        /// The number of elements in the <see cref="values"/> array.
        /// </value>
        public int Length => values.Length;
        #endregion properties

        #region constructors
        #endregion constructors

        #region  methods
        /// <summary>
        /// Resets all elements in the <see cref="values"/> array to their default value.
        /// </summary>
        /// <remarks>
        /// This method iterates through each element of the <see cref="values"/> array and assigns the default value for its type.
        /// After resetting the values, it calls the <see cref="Notify"/> method to inform any listeners of the change.
        /// </remarks>
        public void Reset()
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = default!;
            }
            Notify();
        }
        /// <summary>
        /// Sets the value at the specified position in the collection.
        /// </summary>
        /// <param name="position">The zero-based index at which to set the value.</param>
        /// <param name="value">The value to set at the specified position.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown when the position is outside the bounds of the collection.</exception>
        /// <remarks>
        /// This method updates the value at the given position and then notifies any observers of the change.
        /// </remarks>
        public void SetValue(int position, int value)
        {
            values[position] = value;
            Notify();
        }
        /// <summary>
        /// Retrieves the value at the specified position in the values array.
        /// </summary>
        /// <param name="position">The zero-based index of the value to retrieve.</param>
        /// <returns>The value at the specified position.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the position is outside the bounds of the values array.</exception>
        public int GetValue(int position)
        {
            return values[position];
        }
        /// <summary>
        /// Calculates a hash code for the current instance by summing the values in the <c>values</c> array.
        /// </summary>
        /// <returns>
        /// An integer that represents the hash code for the current instance, which is the sum of all elements in the <c>values</c> array.
        /// </returns>
        /// <remarks>
        /// This method overrides the default implementation of <see cref="Object.GetHashCode"/>.
        /// It is recommended to ensure that the <c>values</c> array is not null before calling this method to avoid potential exceptions.
        /// </remarks>
        public override int GetHashCode()
        {
            int result = 0;

            for (int i = 0; i < values.Length; i++)
            {
                result += values[i];
            }
            return result;
        }
        #endregion methods
    }
}
