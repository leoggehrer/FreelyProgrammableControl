using System.Text;

namespace FreelyProgrammableControl.Logic.Execution
{
    /// <summary>
    /// Represents a collection of timers, each with a specific duration and start time.
    /// </summary>
    /// <remarks>
    /// This class allows for setting, resetting, and retrieving the status of timers.
    /// The timers are stored in an array, and the maximum number of timers is determined by the length parameter provided at initialization.
    /// </remarks>
    internal class Timers(int length)
    {
        #region nested class
        private class Timer(DateTime dateTime, int durationInMs)
        {
            /// <summary>
            /// Gets or sets the date and time value.
            /// </summary>
            /// <value>
            /// A <see cref="DateTime"/> representing the date and time.
            /// </value>
            public DateTime DateTime { get; set; } = dateTime;
            /// <summary>
            /// Gets or sets the duration in milliseconds.
            /// </summary>
            /// <value>
            /// An integer representing the duration in milliseconds.
            /// </value>
            public long DurationInMs { get; set; } = durationInMs;
            /// <summary>
            /// Gets a value indicating whether the specified duration has not yet elapsed
            /// since the specified <see cref="DateTime"/>.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the total time in milliseconds since the specified <see cref="DateTime"/>
            /// is less than or equal to <see cref="DurationInMs"/>; otherwise, <c>false</c>.
            /// </returns>
            public bool Value
            {
                get
                {
                    var result = DurationInMs > 0;

                    if (result)
                    {
                        var totalInMs = (long)(DateTime.Now - DateTime).TotalMilliseconds;

                        result = totalInMs / DurationInMs % 2 == 0;
                    }

                    return result;
                }
            }
        }
        #endregion  nested class

        #region fields
        private readonly Timer?[] timers = new Timer[Math.Max(length, 0)];
        #endregion fields

        #region  properties
        /// <summary>
        /// Gets the number of timers in the collection.
        /// </summary>
        /// <value>
        /// The number of timers as an integer.
        /// </value>
        public int Length => timers.Length;
        #endregion properties

        #region constructors
        #endregion constructors

        #region methods
        /// <summary>
        /// Resets the timers by setting each timer in the array to null.
        /// </summary>
        /// <remarks>
        /// This method iterates through the <c>timers</c> array and assigns <c>null</c> to each element,
        /// effectively resetting all timers to their initial state.
        /// </remarks>
        public void Reset()
        {
            for (int i = 0; i < timers.Length; i++)
            {
                timers[i] = null;
            }
        }

        /// <summary>
        /// Sets a timer at the specified position with the given duration.
        /// </summary>
        /// <param name="position">The index at which to set the timer.</param>
        /// <param name="durationInMs">The duration of the timer in milliseconds.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the position is out of the bounds of the timers array.</exception>
        /// <remarks>
        /// This method initializes a new Timer instance with the current date and time and the specified duration,
        /// and assigns it to the timers array at the specified position.
        /// </remarks>
        public void SetTimer(int position, int durationInMs)
        {
            if (position < 0 || position >= timers.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position is out of the bounds of the timers array.");
            }
            timers[position] = new Timer(DateTime.Now, durationInMs);
        }

        /// <summary>
        /// Retrieves the boolean value from the <see cref="timers"/> collection at the specified position.
        /// </summary>
        /// <param name="position">The zero-based index of the timer in the <see cref="timers"/> collection.</param>
        /// <returns>
        /// <c>true</c> if the timer at the specified position has a value; otherwise, <c>false</c>.
        /// If the timer at the specified position is <c>default</c>, it will return <c>false</c>.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when the <paramref name="position"/> is outside the bounds of the <see cref="timers"/> collection.
        /// </exception
        public bool GetValue(int position)
        {
            return timers[position] == default ? false : timers[position]!.Value;
        }

        /// <summary>
        /// Returns a string representation of the timers collection.
        /// </summary>
        /// <returns>A string that represents the contents of the timers collection.</returns>
        public override string ToString()
        {
            var result = new StringBuilder(timers.Length == 0 ? "Timers are empty." : "Timers contents: ");
            var hasTimer = false;

            for (int i = 0; i < timers.Length && i < 20; i++)
            {
                if (timers[i] != null)
                {
                    if (hasTimer)
                        result.Append(" - ");

                    result.Append($"{i:d2} {timers[i]?.Value}");
                    hasTimer = true;
                }
            }
            return result.ToString();
        }
        #endregion methods
    }
}
