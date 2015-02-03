namespace LibiadaWeb.Models
{
    /// <summary>
    /// The int pair.
    /// </summary>
    public struct IntPair
    {
        /// <summary>
        /// The first.
        /// </summary>
        public readonly int First;

        /// <summary>
        /// The second.
        /// </summary>
        public readonly int Second;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntPair"/> struct.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        public IntPair(int first, int second)
        {
            First = first;
            Second = second;
        }
    }
}
