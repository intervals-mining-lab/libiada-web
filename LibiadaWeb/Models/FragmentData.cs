using System.Collections.Generic;

namespace LibiadaWeb.Models
{
    public class FragmentData
    {
        public readonly double[] Characteristics;
        public string Name;
        public int Start;
        public int Length;

        public FragmentData(double[] characteristics, string name, int start, int length)
        {
            Characteristics = characteristics;
            Name = name;
            Start = start;
            Length = length;
        }
    }
}