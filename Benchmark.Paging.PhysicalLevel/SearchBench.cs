using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Benchmark.Paging.PhysicalLevel
{
    public class SearchBench
    {
        [Params(1,10,50,100,1000,10000)]
        public int Count;

        private Dictionary<int, Tuple<int,int>> _dict = new Dictionary<int, Tuple<int, int>>();
        private Tuple<int, int>[] _arr;
        private Random _rnd = new Random();

        [GlobalSetup]
        public void Init()
        {
            _arr = new Tuple<int, int>[Count];
            foreach (var i in Enumerable.Range(0,Count))
            {
                _dict.Add(i,Tuple.Create(i,i));
                _arr[i] = Tuple.Create(i, i);
            }
        }

        [Benchmark]
        public int InArray()
        {
            var t = _rnd.Next(Count);
            return _arr.First(k => k.Item1 == t).Item2;
        }

        [Benchmark]
        public int InDictionary()
        {
            var t = _rnd.Next(Count);
            return _dict[t].Item2;
        }
    }
}
