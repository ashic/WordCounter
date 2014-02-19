using System;
using System.Collections.Generic;
using CsCounter;
using NUnit.Framework;

namespace CsCounterTests.Properties
{
    [TestFixture]
    public class FileCounterTests
    {
        [Test]
        public void should_fetch_lines_and_delegate_to_lines_counter()
        {
            int called = 0;
            Action<string, Dictionary<string, int>> countAction = (_, __) => called++;
            var linesCounter = new LinesCounter(countAction);

            var fileCounter = new FileCounter(linesCounter);
            fileCounter.Count(new[] {"hello world"});
        }

        [Test]
        [Category("Slow")]
        public void should_work_with_real_file()
        {
            var fileCounter = new FileCounter();
            var results = fileCounter.Count(@".\..\..\..\TextFile1.txt");

            Assert.AreEqual(1, results["volutpat"]);
            Assert.AreEqual(2, results["typi"]);
        }
    }
}