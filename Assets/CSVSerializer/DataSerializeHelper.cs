using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DungeonShooter
{
    public static class DataSerializeHelper
    {
        public static string ListToString<T>(IReadOnlyList<T> list, string separator = "/")
        {
            if (list == null || list.Count == 0)
                return string.Empty;

            return string.Join(separator, list);
        }
        
        public static List<int> StringToIntList(string text, string separator = "/")
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<int>();

            return text
                .Split(new[] { separator }, StringSplitOptions.None)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(p => int.Parse(p, CultureInfo.InvariantCulture))
                .ToList();
        }
    }
}

