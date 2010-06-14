using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncSupport
{
    class Async
    {
        static void Main()
        {
            AsyncUsingTaskCompletionSource();

            AsyncUsingFromAsync();

            AsyncUsingTaskCompletionSource2();
        }

        // Doing it "by hand"
        private static void AsyncUsingTaskCompletionSource()
        {
            var task = AsyncWebRequestTask("http://www.google.com");

            Console.WriteLine("Waiting for task to complete");
            task.Wait();
            Console.WriteLine(task.Result.ContentType);
            task.Result.Close();
        }

        private static Task<WebResponse> AsyncWebRequestTask(string url)
        {
            var tcs = new TaskCompletionSource<WebResponse>();

            var wr = WebRequest.Create(url);

            Console.WriteLine("About to issue GetResponse");

            wr.BeginGetResponse(ar =>
                                    {
                                        Console.WriteLine("Response received");
                                        tcs.SetResult(wr.EndGetResponse(ar));
                                    }, null);

            Console.WriteLine("GetResponse issued");

            return tcs.Task;
        }

        // And now the easy way...
        private static void AsyncUsingFromAsync()
        {
            var task = AsyncUsingFromAsyncTask("http://www.google.com");

            Console.WriteLine(task.Result.ContentType);
            task.Result.Close();
        }

        private static Task<WebResponse> AsyncUsingFromAsyncTask(string url)
        {
            var wr = WebRequest.Create(url);

            return Task<WebResponse>.Factory.FromAsync(wr.BeginGetResponse, wr.EndGetResponse, null);
        }

        // Doing it the hard way when you have to...
        private static void AsyncUsingTaskCompletionSource2()
        {
            var task = AsyncWebClientTask("http://www.google.com", CancellationToken.None);

            Console.WriteLine("Waiting for results...");
            Console.WriteLine(task.Result);
        }

        private static Task<string> AsyncWebClientTask(string url, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<string>();

            var webClient = new WebClient();

            webClient.DownloadStringCompleted += (sender, args) =>
                                                     {
                                                         Console.WriteLine("Download completed");

                                                         tcs.SetResult(args.Result);
                                                         webClient.Dispose();
                                                     };

            cancellationToken.Register(webClient.CancelAsync);

            Console.WriteLine("About to start download");

            webClient.DownloadStringAsync(new Uri(url));

            Console.WriteLine("Download started");

            return tcs.Task;
        }
    }
}
