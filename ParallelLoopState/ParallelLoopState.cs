using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelLoopState
{
    class ParallelLoopState
    {
        static readonly Random Rand = new Random();

        static void Main()
        {
            //Run(Operation.Break);
            Run(Operation.Stop);
        }

        static void Run(Operation operation)
        {
            const int iterations = 1000;
            var completedFlags = new IterationStatus[iterations];
            var results = Parallel.ForEach(Enumerable.Range(1, iterations), 
                (val, pls, index) =>
            {
                if (pls.ShouldExitCurrentIteration)
                {
                    completedFlags[index] = IterationStatus.AskedToStop;
                    return;
                }
                                                                
                if (Rand.Next(50) == 0)
                {
                    if (operation == Operation.Break)
                    {
                        completedFlags[index] = IterationStatus.PerformedBreak;
                        pls.Break();
                    }
                    else
                    {
                        completedFlags[index] = IterationStatus.PerformedStop;
                        pls.Stop();
                    }
                    return;
                }

                Thread.Sleep(Rand.Next(10));
                completedFlags[index] = IterationStatus.Completed;
            });

            Console.WriteLine("Completed: {0}\nLowest Break Iteration: {1}", results.IsCompleted, results.LowestBreakIteration);

            for (int i = 0; i < iterations; i++)
            {
                if (completedFlags[i] != IterationStatus.NotRun)
                {
                    Console.WriteLine("Iteration {0} {1}", i, completedFlags[i]);
                }
            }
        }
    }
}
