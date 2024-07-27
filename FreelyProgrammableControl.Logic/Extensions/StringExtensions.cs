using System.Text;

namespace FreelyProgrammableControl.Logic.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Removes the specified characters from the left and right sides of a string.
        /// </summary>
        /// <param name="source">The string to remove characters from.</param>
        /// <param name="ignores">The characters to ignore and not remove.</param>
        /// <returns>A new string with the specified characters removed from the left and right sides.</returns>
        public static string RemoveLeftAndRight(this string source, params char[] ignores)
        {
            var result = new StringBuilder();
            var start = 0;
            var end = source.Length - 1;

            while (start < end && ignores.Contains(source[start]))
            {
                start++;
            }

            while (start < end && ignores.Contains(source[end]))
            {
                end--;
            }

            while (start <= end)
            {
                result.Append(source[start]);
                start++;
            }
            return result.ToString();
        }

    }
}
