using System.Collections.Generic;

namespace LibiadaWeb.Models
{
    public class FragmentData
    {
        public List<double> Characteristics;
        public string Name;
        public int Start;
        public int Length;

        public FragmentData(List<double> characteristics, string name, int start, int length)
        {
            Characteristics = characteristics;
            Name = name;
            Start = start;
            Length = length;
        }
    }
}