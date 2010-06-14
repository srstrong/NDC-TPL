using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cancellation
{
    class Cancellation
    {
        static void Main()
        {
            var tokenSource = new CancellationTokenSource();

            var options = new ParallelOptions {CancellationToken = tokenSource.Token};

            ThreadPool.QueueUserWorkItem(obj =>
                    {
                        try
                        {
                            Parallel.For(0, 1000, options, i =>
                                        {
                                            Console.WriteLine("Iteration {0}", i);
                                            Thread.Sleep(10);
                                        });
                        }
                        catch (OperationCanceledException)
                        {
                            Console.WriteLine("Loop operation was cancelled");
                        }
                    });

            Thread.Sleep(100);

            Console.WriteLine("Cancelling work...");
            tokenSource.Cancel();

            Thread.Sleep(1000);
        }
    }
}
