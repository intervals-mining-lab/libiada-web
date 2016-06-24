namespace LibiadaWeb.Models
{
    /// <summary>
    /// The fragment data of local characteristics (sliding window).
    /// </summary>
    public class FragmentData
    {
        /// <summary>
        /// The characteristics values.
        /// </summary>
        public readonly double[] Characteristics;

        /// <summary>
        /// The name of the fragment.
        /// </summary>
        public string Name;

        /// <summary>
        /// The starting position of the fragment in full sequence.
        /// </summary>
        public int Start;

        /// <summary>
        /// The length of the fragment.
        /// </summary>
        public int Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="FragmentData"/> class.
        /// </summary>
        /// <param name="characteristics">
        /// The characteristics values.
        /// </param>
        /// <param name="name">
        /// The name of the fragment.
        /// </param>
        /// <param name="start">
        /// The starting position of the fragment in full sequence.
        /// </param>
        /// <param name="length">
        /// The length of the fragment.
        /// </param>
        public FragmentData(double[] characteristics, string name, int start, int length)
        {
            Characteristics = characteristics;
            Name = name;
            Start = start;
            Length = length;
        }
    }
}
