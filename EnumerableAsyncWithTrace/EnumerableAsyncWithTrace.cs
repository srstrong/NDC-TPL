using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EnumerableAsyncWithTrace
{
    class EnumerableAsync
    {
        static void Main()
        {
            UsingIEnumerableOfTask();
        }

        static void UsingIEnumerableOfTask()
        {
            var memoryStream = new MemoryStream();

            var wr = WebRequest.Create("http://www.google.com");

            var end = Task<WebResponse>.Factory
                        .FromAsync(wr.BeginGetResponse, wr.EndGetResponse, null)

                        .ContinueWith(previous => CopyStream(previous.Result.GetResponseStream(), memoryStream).ToTask())

                        .ContinueWith(previous => Console.WriteLine(Encoding.UTF8.GetString(memoryStream.ToArray())));

            end.Wait();
        }

        static IEnumerable<Task> CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[0x2000];

            while (true)
            {
                // Create task to read from the input stream
                var read = Task<int>.Factory.FromAsync((buf, offset, count, callback, state) =>
                                                           {
                                                               Console.WriteLine("Issuing BeginRead");
                                                               return input.BeginRead(buf, offset, count, callback, state);
                                                           },
                                                           ar =>
                                                               {
                                                                   Console.WriteLine("Read completed");
                                                                   return input.EndRead(ar);
                                                               }, 
                                                               buffer, 0, buffer.Length, null);

                yield return read;

                // If we read no data, return
                if (read.Result == 0) break;

                // Create task to write to the output stream
                yield return Task.Factory.FromAsync((buf, offset, count, callback, state) =>
                                                           {
                                                               Console.WriteLine("Issuing BeginWrite");
                                                               return output.BeginWrite(buf, offset, count, callback, state);
                                                           },
                                                           ar =>
                                                               {
                                                                   Console.WriteLine("Write completed");
                                                                   output.EndWrite(ar);
                                                               }, buffer, 0, read.Result, null);
            }
        }
    }

    public static class ExtensionMethods
    {
        public static Task ToTask(this IEnumerable<Task> asyncIterator)
        {
            // Get the enumerator
            var enumerator = asyncIterator.GetEnumerator();

            // Create a TaskCompletionSource
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.AttachedToParent);

            // And with the TCS tasks completes, dispose of the enumerate
            tcs.Task.ContinueWith(previous => enumerator.Dispose());

            Action<Task> recursiveBody = null;
            recursiveBody = previous =>
            {
                Console.WriteLine("recursiveBody started - calling MoveNext");
                if (enumerator.MoveNext())
                {
                    Console.WriteLine("Setting current task to continue with recursiveBody");
                    enumerator.Current.ContinueWith(recursiveBody);
                }
                else
                {
                    Console.WriteLine("No more tasks in enumeration. Set result on outer task");
                    tcs.TrySetResult(null);
                }
                Console.WriteLine("Recursive body completed");
            };

            // Start a new task
            Task.Factory.StartNew(() => recursiveBody(null));

            // Return the TCS task to the user.  If you recall, it doesn't complete until someone calls tcs.TrySetResult()
            return tcs.Task;
        }
    }
}
