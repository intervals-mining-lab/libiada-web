namespace LibiadaWeb.Math
{
    /// <summary>
    /// The math logic.
    /// </summary>
    public static class MathLogic
    {
        /// <summary>
        /// The nullable compare.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool NullableCompare(object first, object second)
        {
            if (first == null && second == null)
            {
                return true;
            }

            return first != null && first.Equals(second);
        }
    }
}
