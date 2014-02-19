using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsCounter;

namespace CsCounterRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileCounter = new FileCounter();
            var results = fileCounter.Count(@".\..\..\..\TextFile1.txt");

            foreach (var key in results.Keys)
                Console.WriteLine("{0}:{1}", key, results[key]);
        }
    }
}
