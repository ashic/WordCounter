using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CsCounter
{
    public static class LineCounter
    {
        public static void Count(string line, Dictionary<string, int> map)
        {
            if (string.IsNullOrWhiteSpace(line))
                return;

            line = Regex.Replace(line, @"[^\w\s]", String.Empty).ToLowerInvariant();

            var parts = line.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
                map[part] = map.ContainsKey(part)?map[part] + 1 : 1;
        }
    }
}