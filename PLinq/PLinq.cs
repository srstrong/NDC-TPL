using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace PLinq
{
    class PLinq
    {
        static void Main()
        {
        }

        static void Example1()
        {
            var sw = new Stopwatch();

            sw.Start();

            var query = from item in Enumerable.Range(1, 10000).AsParallel()
                       where ExpensiveFiler(item)
                       select item;

            foreach (var item in query)
            {
                ExpensiveOp();
            }

            sw.Stop();

            Console.WriteLine("Completed in {0}ms", sw.ElapsedMilliseconds);
        }

        static void Example2()
        {
            var sw = new Stopwatch();

            sw.Start();

            var query = from item in Enumerable.Range(1, 10000).AsParallel() 
                       where ExpensiveFiler(item)
                       select item;

            query.ForAll(item =>  ExpensiveOp());

            sw.Stop();

            Console.WriteLine("Completed in {0}ms", sw.ElapsedMilliseconds);
        }

        private static void NoOp()
        {
        }

        static void IterateWithForEach()
        {
            var query = Enumerable.Range(1, 100)
                                .AsParallel()
                                .Where(item =>
                                           {
                                               Console.WriteLine("Processing where() for {0} on thread {1}", item, Thread.CurrentThread.ManagedThreadId);
                                               return item%2 == 0;
                                           });

            var enumerator = query.GetEnumerator();

            Console.WriteLine("About to call MoveNext on thread {0}", Thread.CurrentThread.ManagedThreadId);
            while (enumerator.MoveNext())
            {
                Console.WriteLine(enumerator.Current);
            }
        }

        static void IterateWithForAll()
        {
            var query = Enumerable.Range(1, 100)
                                .AsParallel()
                                .Where(item =>
                                {
                                    Console.WriteLine("Processing where() for {0} on thread {1}", item, Thread.CurrentThread.ManagedThreadId);
                                    return item % 2 == 0;
                                });

            query.ForAll(item => Console.WriteLine("item {0} on thread {1}", item, Thread.CurrentThread.ManagedThreadId));
        }

        static void IterateWithForEachUsingMerge()
        {
            var query = Enumerable.Range(1, 100)
                                .AsParallel()
                                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                                .Where(item =>
                                {
                                    Console.WriteLine("Processing where() for {0} on thread {1}", item, Thread.CurrentThread.ManagedThreadId);
                                    Thread.SpinWait(2000000);
                                    return item % 2 == 0;
                                });

            var enumerator = query.GetEnumerator();

            Console.WriteLine("About to call MoveNext on thread {0}", Thread.CurrentThread.ManagedThreadId);
            while (enumerator.MoveNext())
            {
                Console.WriteLine("Item {0} on thread {1}", enumerator.Current, Thread.CurrentThread.ManagedThreadId);
            }
        }

        static void OrderedResults()
        {
            var query = Enumerable.Range(1, 1000)
                                .AsParallel()
                                //.AsOrdered()
                                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                                .Where(item => item % 2 == 0 );

            foreach (var item in query)
            {
                Console.WriteLine("item {0} on thread {1}", item, Thread.CurrentThread.ManagedThreadId);
            }
        }

        private static void ExpensiveOp()
        {
            for (var i = 0; i < 100000; i++) ;
        }

        private static bool ExpensiveFiler(int item)
        {
            for (var i = 0; i < 100000; i++) ;

            return item % 2 == 0;
        }
    }
}