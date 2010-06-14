using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Partitions
{
    class Partitions
    {
        private const int DataSize = 10000000;

        static void Main()
        {
            WithoutPartition();
            WithPartition();
        }

        static void WithoutPartition()
        {
            var source = Enumerable.Range(0, DataSize).ToArray();
            var results = new double[source.Length];

            var sw = new Stopwatch();
            sw.Start();

            Parallel.ForEach(source, (val, loopState, index) =>
            {
                results[index] = source[index] * Math.PI;
            });

            sw.Stop();
            Console.WriteLine("Non-partitioned data took {0}ms", sw.ElapsedMilliseconds);
        }

        static void WithPartition()
        {
            var source = Enumerable.Range(0, DataSize).ToArray();
            var results = new double[source.Length];

            var rangePartitioner = Partitioner.Create(0, source.Length);

            var sw = new Stopwatch();
            sw.Start();

            Parallel.ForEach(rangePartitioner, (range, loopState) =>
            {
                //Console.WriteLine("{0} to {1} on thread {2}", range.Item1, range.Item2, Thread.CurrentThread.ManagedThreadId);
                for (var i = range.Item1; i < range.Item2; i++)
                {
                    results[i] = source[i] * Math.PI;
                }
            });

            sw.Stop();
            Console.WriteLine("Partitioned data took {0}ms", sw.ElapsedMilliseconds);
        }
    }
}
