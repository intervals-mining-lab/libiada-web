namespace LibiadaWeb.Helpers
{
    /// <summary>
    /// Class containing extension methods for string class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Trims given substring from the end of current string if any.
        /// </summary>
        /// <param name="source">
        /// Current string.
        /// </param>
        /// <param name="value">
        /// Substring to be removed.
        /// </param>
        /// <returns>
        /// Trimmed from the end string as <see cref="string"/>.
        /// </returns>
        public static string TrimEnd(this string source, string value)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value) || !source.EndsWith(value))
            {
                return source;
            }

            return source.Remove(source.LastIndexOf(value));
        }
    }
}
