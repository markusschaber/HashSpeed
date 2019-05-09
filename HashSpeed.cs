using System;
using System.Collections.Generic;
using System.Text;

namespace HashSpeed
{
    using BenchmarkDotNet.Running;

    public static class HashSpeed
    {
        public static void Main()
        {
            var summary = BenchmarkRunner.Run<HashBenchmark>();

            Console.ReadLine();
        }
    }
}
