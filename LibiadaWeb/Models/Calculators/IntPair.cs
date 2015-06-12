namespace LibiadaWeb.Models.Calculators
{
    /// <summary>
    /// The integer pair.
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
        /// Initializes a new instance of the <see cref="IntPair"/> structure.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        public IntPair(int first, int second)
        {
            this.First = first;
            this.Second = second;
        }
    }
}
