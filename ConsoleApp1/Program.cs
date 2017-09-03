using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis.Semantics;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkDotNet.Running.BenchmarkRunner.Run<A>();
        }
    }

    public class A
    {
        private static int _i=0;

        private static int E()
        {
            return _i++;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T E2<T>()
        {
            if (typeof(T) == typeof(int))
                return (T)(object) (_i++);
            return default(T);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T E3<T>()
        {
            if (typeof(T) == typeof(int))
                return (T)(object)(_i++);
            return default(T);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static T E4<T>()
        {
            if (typeof(T) == typeof(int))
                return (T)(object)(_i++);
            return default(T);
        }

        [Benchmark]
        public object Native()
        {
            return E();
        }

        [Benchmark]
        public object NoInl()
        {
            return E2<int>();
        }

        [Benchmark]
        public object Aggr()
        {
            return E3<int>();
        }

        [Benchmark]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public object NoOpt()
        {
            return E4<int>();
        }
    }

   

}
