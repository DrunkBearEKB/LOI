using System;
using System.Collections.Generic;

namespace Common.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<int> IndexesOf(this string str, string subStr)
        {
            if (string.IsNullOrEmpty(subStr))
            {
                throw new ArgumentException("The string to find may not be empty");
            }
            
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += subStr.Length)
            {
                index = str.IndexOf(subStr, index, StringComparison.Ordinal);
                if (index == -1)
                {
                    return indexes;
                }
                indexes.Add(index);
            }
        }
    }
}