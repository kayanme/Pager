using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using File.Paging.PhysicalLevel.Classes;
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
    [Serializable]
    public class Ser
    {
        public long E;
    }

    public class A
    {
        private BinaryFormatter _formatter = new BinaryFormatter();
        [Benchmark]
        public object Bin()
        {
            var s = new Ser{E = 35};
            var d = new byte[8];
            using (var str = new MemoryStream(d))
            {
                _formatter.Serialize(str,s);
                str.Position = 0;
               return _formatter.Deserialize(str);
            }
        }
        [Benchmark]
        public object My()
        {
            var s = new Ser { E = 35 };
            var d = new byte[4];
            RecordUtils.ToBytes(ref s.E,d,0);
            RecordUtils.FromBytes(d,0,ref s.E);
            return s;
        }
    }

   

}
