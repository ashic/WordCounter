using System;
using System.Collections.Generic;
using CsCounter;
using NUnit.Framework;

namespace CsCounterTests
{
    [TestFixture]
    public class LinesCounterTests
    {
        [Test]
        public void should_defer_to_line_counting_function_for_each_line()
        {
            int called = 0;
            Action<string, Dictionary<string, int>> countingFunction = (line, map) => called++;
            var counter = new LinesCounter(countingFunction);

            counter.Count(new[] { "hello", "hello world" });

            Assert.AreEqual(2, called);
        }

        [Test]
        public void should_not_defer_to_line_counting_function_for_zero_lines()
        {
            int called = 0;
            Action<string, Dictionary<string, int>> countingFunction = (line, map) => called++;
            var counter = new LinesCounter(countingFunction);

            counter.Count(new string[0]);

            Assert.AreEqual(0, called);
        }

        [Test]
        public void should_return_dictionary_mutated_by_counting_function()
        {
            Action<string, Dictionary<string, int>> countingFunction = (line, map) => map["hello"] = 5;
            var counter = new LinesCounter(countingFunction);

            var result = counter.Count(new []{"foo"});

            Assert.AreEqual(5, result["hello"]);
        }
         
    }
}