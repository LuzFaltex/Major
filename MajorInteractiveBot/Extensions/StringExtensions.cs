using System;

namespace MajorInteractiveBot.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Checks to see if the string is null or empty, and if it is, returns the string provided in <paramref name="value"/>
        /// </summary>
        /// <param name="value">The value to provide if the string is null or empty</param>
        /// <param name="whiteSpaceIsEmpty">If true, checks using IsNullOrWhiteSpace; otherwise, checks IsNullOrEmpty.</param>
        public static string OrElse(this string left, string value, bool whiteSpaceIsEmpty = true)
        {
            if (whiteSpaceIsEmpty)
                return (string.IsNullOrWhiteSpace(left) ? value : left);
            else
                return (string.IsNullOrEmpty(left) ? value : left);
        }

        /// <summary>
        /// Checks to see if the string is null or empty, and if it is, throws the provided exception.
        /// </summary>
        /// <param name="ex">The exception to throw.</param>
        /// <param name="whiteSpaceIsEmpty">If true, checks using IsNullOrWhiteSpace; otherwise, checks IsNullOrEmpty.</param>
        public static string OrElse(this string left, Exception ex, bool whiteSpaceIsEmpty = true)
        {
            if (whiteSpaceIsEmpty)
                return (string.IsNullOrWhiteSpace(left) ? throw ex : left);
            else
                return (string.IsNullOrEmpty(left) ? throw ex : left);
        }
    }
}
