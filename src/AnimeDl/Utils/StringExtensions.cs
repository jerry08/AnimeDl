using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnimeDl
{
    static class StringExtensions
    {
        public static string FindBetween(this string value, string a, string b)
        {
            int start = value.IndexOf(a);
            if (start != -1)
            {
                start += a.Length;

                int end = value.IndexOf(b, start);
                if (end != -1)
                {
                    return value.Substring(start, end - start);
                }
            }

            return null;
        }

        public static string SubstringAfter(this string value, string a)
        {
            int start = value.IndexOf(a);
            if (start != -1)
            {
                start += a.Length;
                return value.Substring(start);
            }

            return null;
        }

        public static string SubstringBefore(this string text, string stopAt)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return string.Empty;
        }

        //? - any character (one and only one)
        //* - any characters (zero or more)
        public static string WildCardToRegular(string value)
        {
            // If you want to implement both "*" and "?"
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";

            // If you want to implement "*" only
            //return "^" + Regex.Escape(value).Replace("\\*", ".*") + "$";
        }

        static void Test()
        {
            string test = "Some Data X";

            bool endsWithEx = Regex.IsMatch(test, WildCardToRegular("*X"));
            bool startsWithS = Regex.IsMatch(test, WildCardToRegular("S*"));
            bool containsD = Regex.IsMatch(test, WildCardToRegular("*D*"));

            // Starts with S, ends with X, contains "me" and "a" (in that order) 
            bool complex = Regex.IsMatch(test, WildCardToRegular("S*me*a*X"));
        }

        public static IEnumerable<String> SplitInParts(this String s, Int32 partLength)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", nameof(partLength));

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }
    }
}