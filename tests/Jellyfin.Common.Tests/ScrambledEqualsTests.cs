using System;
using MediaBrowser.Common.Extensions;
using Xunit;

namespace Jellyfin.Common.Tests
{
    public class ScrambledEqualsTests
    {
        private static Random _random = new Random(42);

        [Fact]
        public void PositiveTest()
        {
            var data = new byte[100];
            _random.NextBytes(data);
            var dataCopy = (byte[])data.Clone();
            Assert.True(data.ScrambledEquals(dataCopy));
        }

        [Fact]
        public void PositiveTestShuffled()
        {
            var data = new byte[100];
            _random.NextBytes(data);
            var dataCopy = (byte[])data.Clone();
            dataCopy.Shuffle();
            Assert.True(data.ScrambledEquals(dataCopy));
        }

        [Fact]
        public void NegativeTestShuffled()
        {
            var data = new byte[100];
            _random.NextBytes(data);
            var dataCopy = (byte[])data.Clone();
            dataCopy.Shuffle();
            data[_random.Next(0, 99)]--;
            Assert.False(data.ScrambledEquals(dataCopy));
        }
    }
}
