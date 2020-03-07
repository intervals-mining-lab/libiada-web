using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibiadaWeb.Models.Repositories.Sequences
{
    public class MultisequenceRepository
    {
        private static readonly Dictionary<int, string> RomanDigits = new Dictionary<int, string>
        {
            { 1000, "M" },
            { 900, "CM" },
            { 500, "D" },
            { 400, "CD" },
            { 100, "C" },
            { 90 , "XC" },
            { 50 , "L" },
            { 40 , "XL" },
            { 10 , "X" },
            { 9  , "IX" },
            { 5  , "V" },
            { 4  , "IV" },
            { 1  , "I" }
        };

        /// <summary>
        /// Converts roman numbers to arabic.
        /// </summary>
        /// <param name="number">
        /// Number to convert.
        /// </param>
        /// <returns>
        /// Returns arabic number.
        /// </returns>
        public static int ToArabic(string number) => number.Length == 0 ? 0 :
            RomanDigits
                .Where(d => number.StartsWith(d.Value))
                .Select(d => d.Key + ToArabic(number.Substring(d.Value.Length)))
                .FirstOrDefault();

        /// <summary>
        /// Identifies chain number.
        /// </summary>
        /// <param name="matterName">
        /// The matter name.
        /// </param>
        /// <returns>
        /// Returns chain number.
        /// </returns>
        public static int GetSequenceNumber(string matterName)
        {
            var refSplitArray = matterName.Split(' ').ToList();
            int result = 0;
            if (matterName.Contains("chromosome"))
            {
                int chromosomeWordIndex = refSplitArray.IndexOf("chromosome");
                if (refSplitArray.IndexOf("chromosome") < refSplitArray.Count - 1)
                {
                    if (refSplitArray[chromosomeWordIndex + 1].Replace(".", string.Empty)
                        .All(Char.IsDigit))
                    {
                        result = Convert.ToInt32(refSplitArray[chromosomeWordIndex + 1].Replace(".", string.Empty));
                    }
                    else
                    {
                        result = ToArabic(refSplitArray[chromosomeWordIndex + 1].Replace(".", string.Empty));
                    }
                }
                else
                {
                    result = 0;
                }
            }
            else if (matterName.Contains("segment"))
            {
                int segmentWordIndex = refSplitArray.IndexOf("segment");
                if (refSplitArray[segmentWordIndex + 1].Contains("RNA"))
                {
                    result = Convert.ToInt32(refSplitArray[refSplitArray.IndexOf("RNA") + 1].Replace(".", string.Empty));
                }
                else if (refSplitArray[segmentWordIndex + 1].All(Char.IsDigit))
                {
                    result = Convert.ToInt32(refSplitArray[segmentWordIndex + 1]);
                }
                else
                {
                    result = Convert.ToChar(refSplitArray[segmentWordIndex + 1]) - 64;
                }
            }
            else if (matterName.Contains("plasmid"))
            {
                int plasmidWordIndex = refSplitArray.IndexOf("plasmid");
                if (refSplitArray[plasmidWordIndex + 1].Length > 1 && !refSplitArray[plasmidWordIndex + 1].All(Char.IsDigit))
                {
                    bool check = false;
                    foreach (var ch in refSplitArray[plasmidWordIndex + 1])
                    {
                        if (Char.IsNumber(ch))
                        {
                            check = true;
                        }
                    }
                    if (check)
                    {
                        result = Convert.ToInt32(Regex.Replace(refSplitArray[plasmidWordIndex + 1],
                            @"[^\d]+", ""));
                    }
                    else
                    {
                        result = refSplitArray[plasmidWordIndex + 1].ToCharArray()[0] - 64;
                    }
                }
                else if (refSplitArray[plasmidWordIndex + 1].All(Char.IsDigit))
                {
                    result = Convert.ToInt32(refSplitArray[plasmidWordIndex + 1]);
                }
                else
                {
                    result = refSplitArray[plasmidWordIndex + 1].ToCharArray()[0] - 64;
                }
            }

            return result;
        }

        /// <summary>
        /// Discards excess parts.
        /// </summary>
        /// <param name="matterName">
        /// The matter name.
        /// </param>
        /// <returns>
        /// Returns matter name without excess parts.
        /// </returns>
        public static string GetMatterNameSplit(string matterName)
        {
            if (matterName.Split('|').Length > 2)
            {
                return matterName.Split('|')[1].Trim();
            }
            else
            {
                return matterName.Split('|')[0].Trim();
            }
        }
    }
}