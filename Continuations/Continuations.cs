using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Continuations
{
    class Continuations
    {
        static void Main()
        {
            SimpleContinuation();

            MultiTaskContinuation();

            ContinueWithExceptions();
        }

        static void SimpleContinuation()
        {
            var task = new Task<WebRequest>(() =>
            {
                Console.WriteLine("Creating web request");
                return WebRequest.Create("http://www.google.com");
            });

            var task2 = task.ContinueWith<WebResponse>(request =>
            {
                Console.WriteLine("Getting response");
                return request.Result.GetResponse();
            });

            var task3 = task2.ContinueWith<Stream>(response =>
            {
                Console.WriteLine("Getting response stream");
                return response.Result.GetResponseStream();
            });

            var task4 = task3.ContinueWith<string>(stream =>
            {
                Console.WriteLine("Decoding response stream");
                return new StreamReader(stream.Result).ReadToEnd();
            });

            var task5 = task4.ContinueWith(text => Console.WriteLine("Website content: {0}", text.Result));

            // Start the first task...
            task.Start();

            while (!task5.IsCompleted)
            {
                Console.WriteLine("Working...");
                Thread.Sleep(50);
            }
        }

        static void SimpleContinuation2()
        {
            var task = Task<WebRequest>.Factory
                .StartNew(() => WebRequest.Create("http://www.google.com"))

                .ContinueWith(request => request.Result.GetResponse())

                .ContinueWith(response => response.Result.GetResponseStream())

                .ContinueWith(stream => new StreamReader(stream.Result).ReadToEnd())

                .ContinueWith(text => Console.WriteLine("Website content: {0}", text.Result));

            // Could use .Wait(), but this shows that the initiating thread can do other work
            while (!task.IsCompleted)
            {
                Console.WriteLine("Working...");
                Thread.Sleep(50);
            }
        }


        private static void MultiTaskContinuation()
        {
            var tasks = new Task<Stream>[2];

            tasks[0] = new Task<Stream>(() => WebRequest.Create("http://www.google.com").GetResponse().GetResponseStream());
            tasks[1] = new Task<Stream>(() => WebRequest.Create("http://www.bing.com").GetResponse().GetResponseStream());

            tasks[1].Start();
            tasks[0].Start();

            var task3 = Task.Factory.ContinueWhenAny(tasks, winner => winner == tasks[0] ? "Google" : "Bing");

            // Blocks until task3 completes
            Console.WriteLine("{0} wins!", task3.Result);
        }

        private static void ContinueWithExceptions()
        {
            var task = new Task<Stream>(() =>
                                        WebRequest.Create("http://www.google.com")
                                                  .GetResponse()
                                                  .GetResponseStream()
                );

            var successTask = task.ContinueWith(t => Console.WriteLine("Request successful"),
                                          TaskContinuationOptions.OnlyOnRanToCompletion);

            var errorTask = task.ContinueWith(t => Console.WriteLine("Task failed: {0}", t.Exception), 
                                          TaskContinuationOptions.OnlyOnFaulted);

            task.Start();

            try
            {
                Task.WaitAll(successTask, errorTask);
            }
            catch
            {
            }

            task.DisplayState("web request");
            successTask.DisplayState("success");
            errorTask.DisplayState("error");
        }
    }

    public static class Extensions
    {
        public static void DisplayState(this Task task, string name)
        {
            Console.WriteLine("{3} state: {{ Completed: {0}, Cancelled: {1}, Faulted: {2} }}", task.IsCompleted, task.IsCanceled, task.IsFaulted, name);
        }
    }

}
