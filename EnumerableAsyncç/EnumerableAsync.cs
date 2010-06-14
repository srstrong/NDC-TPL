using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EnumerableAsync
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
                var read = Task<int>.Factory.FromAsync(input.BeginRead, input.EndRead, buffer, 0, buffer.Length, null);

                yield return read;

                // If we read no data, return
                if (read.Result == 0) break;

                // Create task to write to the output stream
                yield return Task.Factory.FromAsync(output.BeginWrite, output.EndWrite, buffer, 0, read.Result, null);
            }
        }

        static IEnumerable<Task> CopyStreamUsingSyntacticSugar(Stream input, Stream output)
        {
            var buffer = new byte[0x2000];

            while (true)
            {
                // Create task to read from the input stream
                var read = input.Read(buffer);

                yield return read;

                // If we read no data, return
                if (read.Result == 0) break;

                // Create task to write to the output stream
                yield return output.Write(buffer, read.Result);
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
                if (enumerator.MoveNext())
                {
                    enumerator.Current.ContinueWith(recursiveBody);
                }
                else
                {
                    tcs.TrySetResult(null);
                }
            };

            // Start a new task
            Task.Factory.StartNew(() => recursiveBody(null));

            // Return the TCS task to the user.  If you recall, it doesn't complete until someone calls tcs.TrySetResult()
            return tcs.Task;
        }

        public static Task<int> Read(this Stream stream, byte[] buffer)
        {
            return Task<int>.Factory.FromAsync(stream.BeginRead, stream.EndRead, buffer, 0, buffer.Length, null);
        }

        public static Task  Write(this Stream stream, byte[] buffer, int numBytes)
        {
            return Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, buffer, 0, numBytes, null);
        }
    }
}
