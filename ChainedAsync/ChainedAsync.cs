using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChainedAsync
{
    class ChainedAsync
    {
        static void Main()
        {
            ChainingReadsAndWrites();
        }

        static void ChainingReadsAndWrites()
        {
            var memoryStream = new MemoryStream();

            var wr = WebRequest.Create("http://www.google.com");

            // Start a task to get the response
            var responseTask = Task<WebResponse>.Factory.FromAsync(wr.BeginGetResponse, wr.EndGetResponse, null);

            // When that completes, continue with a call to Read()
            var readerWriter = responseTask.ContinueWith(previous => Read(previous.Result.GetResponseStream(), memoryStream));

            // And when the readerWriter task completes, output the results
            var end = readerWriter.ContinueWith(previous => Console.WriteLine(Encoding.UTF8.GetString(memoryStream.ToArray())));

            // Finally, wait for everything.  Be a shame to exit the process too quickly :)
            end.Wait();
        }

        private static void Read(Stream from, Stream to)
        {
            Console.WriteLine("Doing read...");

            var buffer = new byte[1024];

            // Create task to read a block from the stream
            var readChunk = Task<int>.Factory.FromAsync(from.BeginRead, from.EndRead, buffer, 0, buffer.Length, null, TaskCreationOptions.AttachedToParent);

            // When that completes, call Write()
            readChunk.ContinueWith(ant => Write(ant, buffer, from, to), TaskContinuationOptions.AttachedToParent);
        }

        private static void Write(Task<int> previous, byte[] buffer, Stream from, Stream to)
        {
            Console.WriteLine("Doing write...");

            if (previous.Result == 0)
            {
                // No data
                return;
            }

            // Got some data.  Create task to write the data to the output stream
            var writeChunk = Task.Factory.FromAsync(to.BeginWrite, to.EndWrite, buffer, 0,
                                                    previous.Result, null, TaskCreationOptions.AttachedToParent);

            // And when that completes, call back into Read()
            writeChunk.ContinueWith(ant => Read(from, to), TaskContinuationOptions.AttachedToParent);
        }
    }
}
