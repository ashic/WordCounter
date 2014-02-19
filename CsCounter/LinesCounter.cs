using System;
using System.Collections.Generic;

namespace CsCounter
{
    public class LinesCounter
    {
        private readonly Action<string, Dictionary<string, int>> _countingFunction;

        public LinesCounter(Action<string, Dictionary<string, int>> countingFunction)
        {
            _countingFunction = countingFunction;
        }


        public Dictionary<string, int> Count(IEnumerable<string> lines)
        {
            var map = new Dictionary<string, int>();
            
            foreach (var line in lines)
                _countingFunction(line, map);

            return map;
        }
    }
}