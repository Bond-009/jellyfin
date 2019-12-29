using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MediaBrowser.Common.Extensions;

namespace Jellyfin.Common.Benches
{
    [MemoryDiagnoser]
    public class ScrambledEqualsBenches
    {
        private byte[] _data;
        private byte[] _dataCopy;
        private byte[] _dataShuffled;

        private IEnumerable<byte> _dataEnumerable;
        private IEnumerable<byte> _dataCopyEnumerable;
        private IEnumerable<byte> _dataShuffledEnumerable;

        [Params(0, 10, 100, 1000, 10000)]
        public int N { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _data = new byte[N];
            new Random(42).NextBytes(_data);
            _dataCopy = (byte[])_data.Clone();
            _dataShuffled = (byte[])_data.Clone();
            _dataShuffled.Shuffle();

            _dataEnumerable = _data.Select(x => x);
            _dataCopyEnumerable = _dataCopy.Select(x => x);
            _dataShuffledEnumerable = _dataShuffled.Select(x => x);
        }

        [Benchmark]
        public bool SequenceEqual() => _data.SequenceEqual(_dataCopy);*/

        [Benchmark]
        public bool ScrambledEquals() => _data.ScrambledEquals(_dataCopy);

        [Benchmark]
        public bool SequenceEqualShuffledOrdered() => _data.OrderBy(x => x).SequenceEqual(_dataCopy.OrderBy(x => x));

        [Benchmark]
        public bool ScrambledEqualsShuffled() => _data.ScrambledEquals(_dataShuffled);

        [Benchmark]
        public bool SequenceEqualEnumerableEnumerable() => _dataEnumerable.SequenceEqual(_dataCopyEnumerable);*/

        [Benchmark]
        public bool ScrambledEqualsEnumerable() => _dataEnumerable.ScrambledEquals(_dataCopyEnumerable);

        [Benchmark]
        public bool ScrambledEqualsShuffledEnumerable() => _dataEnumerable.ScrambledEquals(_dataShuffledEnumerable);
    }
}
