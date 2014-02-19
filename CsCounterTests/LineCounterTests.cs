using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsCounter;
using NUnit.Framework;

namespace CsCounterTests
{
    [TestFixture]
    public class LineCounterTests
    {
        private Dictionary<string, int> _result;

        [SetUp]
        public void BeforeEach()
        {
            _result = new Dictionary<string, int>();
        }

        [Test]
        public void should_work_with_no_words()
        {
            LineCounter.Count(string.Empty, _result);
            Assert.AreEqual(0, _result.Count);
        }

        [Test]
        public void should_work_with_single_word()
        {
            LineCounter.Count("hello", _result);
            Assert.AreEqual(1, _result.Count);
            Assert.AreEqual(1, _result["hello"]);
        }

        [Test]
        public void should_work_with_two_different_words()
        {
            LineCounter.Count("hello world", _result);
            Assert.AreEqual(2, _result.Count);
            Assert.AreEqual(1, _result["hello"]);
            Assert.AreEqual(1, _result["world"]);
        }                      

        [Test]
        public void should_work_regardless_of_whitespace()
        {
            LineCounter.Count("hello  world", _result);
            Assert.AreEqual(2, _result.Count);
            Assert.AreEqual(1, _result["hello"]);
            Assert.AreEqual(1, _result["world"]);
        }


        [Test]
        public void should_work_with_multiple_instances_of_same_word()
        {
            LineCounter.Count("hello world hello", _result);
            Assert.AreEqual(2, _result.Count);
            Assert.AreEqual(2, _result["hello"]);
            Assert.AreEqual(1, _result["world"]);
        }

        [Test]
        public void should_ignore_punctuation()
        {
            LineCounter.Count("hello; world. hello", _result);
            Assert.AreEqual(2, _result.Count);
            Assert.AreEqual(2, _result["hello"]);
            Assert.AreEqual(1, _result["world"]);
        }

        [Test]
        public void should_ignore_case()
        {
            LineCounter.Count("hello; world. Hello", _result);
            Assert.AreEqual(2, _result.Count);
            Assert.AreEqual(2, _result["hello"]);
            Assert.AreEqual(1, _result["world"]);
        }

    }
}
