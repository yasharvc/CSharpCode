using System;

namespace Helper
{
    public static class StringExtensions
    {
        public static string GetFromTo(this string exp, string from, string to = null, int startPos = 0, bool searchToCharFromEnd = false)
        {
            var toChar = to ?? from;
            var start = exp.IndexOf(from, startPos);
            if (start < 0)
                return "";
            var end = searchToCharFromEnd ?
                exp.LastIndexOf(toChar)
                : exp.IndexOf(toChar, start + 1);
            if (end < 0)
                return "";
            return exp[start..end];
        }

        public static bool Like(this string value, string searchString)
        {
            bool result = false;

            var likeParts = searchString.Split(new char[] { '%' });

            for (int i = 0; i < likeParts.Length; i++)
            {
                if (likeParts[i] == string.Empty)
                {
                    continue;   // "a%"
                }

                if (i == 0)
                {
                    if (likeParts.Length == 1) // "a"
                    {
                        result = value.Equals(likeParts[i], StringComparison.OrdinalIgnoreCase);
                    }
                    else // "a%" or "a%b"
                    {
                        result = value.StartsWith(likeParts[i], StringComparison.OrdinalIgnoreCase);
                    }
                }
                else if (i == likeParts.Length - 1) // "a%b" or "%b"
                {
                    result &= value.EndsWith(likeParts[i], StringComparison.OrdinalIgnoreCase);
                }
                else // "a%b%c"
                {
                    int current = value.IndexOf(likeParts[i], StringComparison.OrdinalIgnoreCase);
                    int previous = value.IndexOf(likeParts[i - 1], StringComparison.OrdinalIgnoreCase);
                    result &= previous < current;
                }
            }

            return result;
        }

        /// <summary>
        /// Tests a string containing SQL Like style wildcards to be ReverseLike another string 
        /// </summary>
        /// <param name="value">search string containing wildcards</param>
        /// <param name="compareString">string to be compared</param>
        /// <returns>value.ReverseLike(compareString)</returns>
        /// <example>value.ReverseLike("a")</example>
        /// <example>value.ReverseLike("abc")</example>
        /// <example>value.ReverseLike("ab")</example>
        /// <example>value.ReverseLike("axb")</example>
        /// <example>value.ReverseLike("axbyc")</example>
        /// <remarks>reversed logic of Like string extension</remarks>
        public static bool ReverseLike(this string value, string compareString)
        {
            bool result = false;

            var likeParts = value.Split(new char[] { '%' });

            for (int i = 0; i < likeParts.Length; i++)
            {
                if (likeParts[i] == string.Empty)
                {
                    continue;   // "a%"
                }

                if (i == 0)
                {
                    if (likeParts.Length == 1) // "a"
                    {
                        result = compareString.Equals(likeParts[i], StringComparison.OrdinalIgnoreCase);
                    }
                    else // "a%" or "a%b"
                    {
                        result = compareString.StartsWith(likeParts[i], StringComparison.OrdinalIgnoreCase);
                    }
                }
                else if (i == likeParts.Length - 1) // "a%b" or "%b"
                {
                    result &= compareString.EndsWith(likeParts[i], StringComparison.OrdinalIgnoreCase);
                }
                else // "a%b%c"
                {
                    int current = compareString.IndexOf(likeParts[i], StringComparison.OrdinalIgnoreCase);
                    int previous = compareString.IndexOf(likeParts[i - 1], StringComparison.OrdinalIgnoreCase);
                    result &= previous < current;
                }
            }

            return result;
        }
        }
}