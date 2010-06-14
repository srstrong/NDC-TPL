using System;
using System.Threading;
using System.Threading.Tasks;

namespace CooperativeCancellation
{
    class CooperativeCancellation
    {
        static void Main()
        {
            var tokenSource = new CancellationTokenSource();

            var options = new ParallelOptions { CancellationToken = tokenSource.Token };

            ThreadPool.QueueUserWorkItem(obj =>
            {
                try
                {
                    Parallel.For(0, 1000, options, i => DoWork(i, options.CancellationToken));
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Loop operation was cancelled");
                }
            });

            Thread.Sleep(250);

            Console.WriteLine("Cancelling work...");
            tokenSource.Cancel();

            Thread.Sleep(1000);
        }

        private static void DoWork(int iteration, CancellationToken cancellationToken)
        {
            // Simulate some long running task
            for (var i = 0; i < 5; i++)
            {
                Console.WriteLine("Working... (iteration {0}, loop {1})", iteration, i);
                Thread.Sleep(10);
                
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
