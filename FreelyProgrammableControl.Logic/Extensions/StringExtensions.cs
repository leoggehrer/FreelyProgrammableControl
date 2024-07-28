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
        ///<summary>
        /// Converts a string to an integer.
        ///</summary>
        ///<param name="text">The string to be converted.</param>
        ///<returns>An integer representation of the input string.</returns>
        public static int ToInt(this string text)
        {
            int result = 0;

            foreach (var item in text)
            {
                if (char.IsDigit(item))
                {
                    result *= 10;
                    result = result + item - '0';
                }
            }
            return result;
        }

        public static bool ContainsDigit(this string text)
        {
            var result = false;
            var idx = 0;

            while (idx < text.Length && result == false)
            {
                result = char.IsDigit(text[idx++]);
            }

            return result;
        }

    }
}
