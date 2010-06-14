using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BasicTaskFunctionality
{
    internal class BasicTaskFunctionality
    {
        private static int zero;

        private static void Main()
        {
            //UsingParallelInvoke();

            //UsingTaskDirectly();

            //UsingTaskFactory();

            //GettingReturnValues();

            //AccessingLocalVariables();

            //WaitingWithTimeout();

            //Cancellation();

            //ExceptionsWhenWaiting();

            //ExceptionsWhenAccessingResult();

            //ExceptionsProperty();

            NonHandledExceptions();

            //NonHandledExceptionsWithHandler();
        }

        private static void UsingParallelInvoke()
        {
            Console.WriteLine("\n{0}()\n", MethodBase.GetCurrentMethod().Name);

            Parallel.Invoke(
                () => Console.WriteLine("First task"),
                () => Console.WriteLine("Second tast"));
        }

        private static void UsingTaskDirectly()
        {
            Console.WriteLine("\n{0}()\n", MethodBase.GetCurrentMethod().Name);

            var t1 = new Task(() => Console.WriteLine("First task"));
            var t2 = new Task(() => Console.WriteLine("Second task"));

            t1.Start();
            t2.Start();

            t1.Wait();
            t2.Wait();
        }

        private static void UsingTaskFactory()
        {
            Console.WriteLine("\n{0}()\n", MethodBase.GetCurrentMethod().Name);

            var t1 = Task.Factory.StartNew(() => Console.WriteLine("First task"));
            var t2 = Task.Factory.StartNew(() => Console.WriteLine("Second task"));

            t1.Wait();
            t2.Wait();
        }

        private static void GettingReturnValues()
        {
            Console.WriteLine("\n{0}()\n", MethodBase.GetCurrentMethod().Name);

            var t1 = Task<int>.Factory.StartNew(() => new Random().Next());
            
            Console.WriteLine("Result was {0}", t1.Result);
        }

        private static void AccessingLocalVariables()
        {
            Console.WriteLine("\n{0}()\n", MethodBase.GetCurrentMethod().Name);

            // Bad - captures loop variable
            var tasks = new Task[5];

            for (var i = 0; i < 5; i++)
            {
                tasks[i] = Task.Factory.StartNew(() => Console.WriteLine(i));
            }

            // Can wait for multiple tasks...
            Task.WaitAll(tasks);

            Console.WriteLine();

            // Or capture loop into temporate
            for (var i = 0; i < 5; i++)
            {
                var x = i;
                tasks[i] = Task.Factory.StartNew(() => Console.WriteLine(x));
            }

            Task.WaitAll(tasks);

            Console.WriteLine();

            // Or even better - pass loop variable into StartNew
            for (var i = 0; i < 5; i++)
            {
                tasks[i] = Task.Factory.StartNew(data => Console.WriteLine(data), i);
            }

            Task.WaitAll(tasks);
        }

        private static void WaitingWithTimeout()
        {
            Console.WriteLine("\n{0}()\n", MethodBase.GetCurrentMethod().Name);

            var t1 = Task.Factory.StartNew(() => Thread.Sleep(2000));

            t1.Wait(1000);

            t1.DisplayState();

            t1.Wait();

            t1.DisplayState();
        }

        private static void Cancellation()
        {
            Console.WriteLine("\n{0}()\n", MethodBase.GetCurrentMethod().Name);

            var tokenSource = new CancellationTokenSource();

            var t1 = Task.Factory.StartNew(() =>
                                  {
                                      while (true)
                                      {
                                          Console.WriteLine("Working...");
                                          Thread.Sleep(10);
                                          tokenSource.Token.ThrowIfCancellationRequested();
                                      }
                                  }, tokenSource.Token);

            Thread.Sleep(100);

            tokenSource.Cancel();

            try
            {
                t1.Wait();
            }
            catch (AggregateException e)
            {
                Console.WriteLine("Exeption! {0}", e);
            }

            t1.DisplayState();
        }

        private static void ExceptionsWhenWaiting()
        {
            Console.WriteLine("\n{0}()\n", MethodBase.GetCurrentMethod().Name);

            // Exception thrown when waiting...
            var t1 = Task.Factory.StartNew(() => 1/zero);

            try
            {
                t1.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception during task execution: {0}", e);
            }
        }

        private static void ExceptionsWhenAccessingResult()
        {
            Console.WriteLine("\n{0}()\n", MethodBase.GetCurrentMethod().Name);

            // Exception thrown when accessing results...
            var t1 = Task.Factory.StartNew(() => 1/zero);

            try
            {
                Console.WriteLine("Task returned {0}", t1.Result);
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception during task execution: {0}", e);
            }
        }

        private static void ExceptionsProperty()
        {
            Console.WriteLine("\n{0}()\n", MethodBase.GetCurrentMethod().Name);

            // Exception thrown when accessing results...
            var t1 = Task.Factory.StartNew(() => 1/zero);

            Thread.Sleep(1000);

            if (t1.Exception != null)
            {
                Console.WriteLine("Exception occurred during task execution: {0}", t1.Exception);
            }
        }

        private static void NonHandledExceptions()
        {
            Console.WriteLine("\n{0}()\n", MethodBase.GetCurrentMethod().Name);

            // Exception thrown when accessing results...
            var t1 = Task.Factory.StartNew(() => 1/zero);

            Thread.Sleep(1000);

            t1.DisplayState();

            t1 = null;

            GC.Collect();
            Thread.Sleep(1000);
        }

        private static void NonHandledExceptionsWithHandler()
        {
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Console.WriteLine("Caught exception from handler: {0}", e.Exception);
                e.SetObserved();
            };

            NonHandledExceptions();
        }
    }

    public static class Extensions
    {
        public static void DisplayState(this Task task)
        {
            Console.WriteLine("Task state: {{ Completed: {0}, Cancelled: {1}, Faulted: {2} }}", task.IsCompleted, task.IsCanceled, task.IsFaulted);
        }
    }
}