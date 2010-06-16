using System;
using System.Threading;
using System.Threading.Tasks;

namespace Exceptions
{
    class Exceptions
    {
        static void Main()
        {
            var random = new Random();

            try
            {
                Parallel.For(0, 100, (index, pls) =>
                    {
                        if (index == 0)
                        {
                            while (true)
                            {
                                if (pls.IsExceptional)
                                {
                                    Console.WriteLine("Noticed that we'd gone bad (I'm on thread {0})", Thread.CurrentThread.ManagedThreadId);
                                    return;
                                }
                                Thread.Sleep(1);
                            }
                        }

                        if (random.Next(50) == 0)
                        {
                            Console.WriteLine("Iteration {0}, thread {1} is going to spoil the party", index, Thread.CurrentThread.ManagedThreadId);
                            throw new Exception(string.Format("Iteration {0} threw on thread {1}", index, Thread.CurrentThread.ManagedThreadId));
                        }
                        Console.WriteLine("Completing iteration {0} on thread {1}", index, Thread.CurrentThread.ManagedThreadId);
                    }
                );
            }
            catch (AggregateException e)
            {
                foreach (var inner in e.InnerExceptions)
                {
                    Console.WriteLine("\nException!: {0}", inner);
                }
            }
        }
    }
}
