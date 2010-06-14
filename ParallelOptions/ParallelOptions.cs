using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parallel_Options
{
    class Program
    {
        static void Main()
        {
            MaxParallelism();

            DynamicSchedulingAndWorkStealing();
        }

        static void MaxParallelism()
        {
            const int iterations = 10000;
            var threadsUsed = new ConcurrentDictionary<int, int>();

            var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };

            Parallel.ForEach(Enumerable.Range(1, iterations),
                             options,
                             val => threadsUsed.TryAdd(Thread.CurrentThread.ManagedThreadId, 0));

            Console.WriteLine("Used {0} threads", threadsUsed.Count);

            foreach (var threadId in threadsUsed.Keys)
            {
                Console.WriteLine("Used thread {0}", threadId);
            }
        }

        static void DynamicSchedulingAndWorkStealing()
        {
            const int iterations = 10000;

            var options = new ParallelOptions();// { MaxDegreeOfParallelism = -1 };
            var threadsUsed = new ConcurrentDictionary<int, int>();

            var sw = new Stopwatch();
            sw.Start();

            Parallel.ForEach(Enumerable.Range(1, iterations),
                             options,
                             () => 0L,
                             (val, pls, local) => 
                             {
                                 threadsUsed.TryAdd(Thread.CurrentThread.ManagedThreadId, 0);
                                 if (val < (iterations / 2))
                                 {
                                    //for (int i = 0; i < 1000000; i++) ;
                                    Thread.Sleep(1);
                                 }
                                 return local + 1;
                             },
                             local => Console.WriteLine("Thread {0} processed {1} items", Thread.CurrentThread.ManagedThreadId, local));

            sw.Stop();

            Console.WriteLine("Used {0} different threads and took {1}ms", threadsUsed.Count, sw.ElapsedMilliseconds);

        }
    }
}
