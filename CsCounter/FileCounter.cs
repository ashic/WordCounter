using System.Collections.Generic;
using System.IO;

namespace CsCounter
{
    public class FileCounter
    {
        private readonly LinesCounter _linesCounter;

        public FileCounter(LinesCounter linesCounter = null)
        {
            _linesCounter = linesCounter ?? new LinesCounter(LineCounter.Count);
        }

        public Dictionary<string, int> Count(IEnumerable<string> lines)
        {
            return _linesCounter.Count(lines);
        }

        public Dictionary<string, int> Count(string filePath)
        {
            return Count(File.ReadLines(filePath));
        }
    }
}