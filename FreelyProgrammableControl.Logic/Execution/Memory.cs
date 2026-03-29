using System.Text;
using FreelyProgrammableControl.Logic.Common;

namespace FreelyProgrammableControl.Logic.Execution
{
    /// <summary>
    /// Represents a memory structure that holds a fixed number of values of type <typeparamref name="T"/>.
    /// Inherits from the <see cref="Subject"/> class.
    /// </summary>
    /// <typeparam name="T">The type of values stored in the memory.</typeparam>
    /// <param name="length">The length of the memory, which determines the number of values it can hold.</param>
    internal class Memory<T>(int length) : Subject, IMemory<T>
    {
        #region  fields
        private readonly T[] values = new T[Math.Max(length, 0)];
        #endregion fields

        #region  properties
        /// <summary>
        /// Gets the length of the values array.
        /// </summary>
        /// <value>
        /// The number of elements in the values array.
        /// </value>
        public int Length => values.Length;
        #endregion properties

        #region constructors
        #endregion constructors

        #region  methods
        /// <summary>
        /// Resets the values in the array to their default values.
        /// </summary>
        /// <remarks>
        /// This method iterates through the <c>values</c> array and sets each element
        /// to its default value. After resetting the values, it calls the <c>Notify</c>
        /// method to signal that the state has changed.
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
        /// <param name="position">The zero-based index of the position where the value should be set.</param>
        /// <param name="value">The value to be set at the specified position.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the position is outside the bounds of the collection.</exception>
        /// <remarks>
        /// This method updates the value at the given position and triggers a notification only if the value has changed.
        /// </remarks>
        public void SetValue(int position, T value)
        {
            if (position < 0 || position >= values.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position is out of the bounds of the values array.");
            }
            
            var oldValue = values[position];
            values[position] = value;
            
            // Only notify if the value actually changed
            if (!EqualityComparer<T>.Default.Equals(oldValue, value))
            {
                Notify();
            }
        }
        /// <summary>
        /// Retrieves the value at the specified position in the collection.
        /// </summary>
        /// <typeparam name="T">The type of the value to be retrieved.</typeparam>
        /// <param name="position">The zero-based index of the value to retrieve.</param>
        /// <returns>The value at the specified position in the collection.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified position is outside the bounds of the collection.</exception>
        public T GetValue(int position)
        {
            if (position < 0 || position >= values.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position is out of the bounds of the values array.");
            }
            return values[position];
        }

        /// <summary>
        /// Returns a string representation of the memory contents.
        /// </summary>
        /// <returns>A string that represents the contents of the memory.</returns>
        public override string ToString()
        {
            var result = new StringBuilder();

            for (int i = 0; i < values.Length && i < 20; i++)
            {
                if (i > 0)
                    result.AppendLine();

                result.Append($"{i:d2} {values[i]:d3}");
            }
            return result.ToString();
        }
        #endregion methods
    }
}
