using System;
using System.Diagnostics;

namespace CacheDemo
{
    class CacheDemo
    {
        private const int RowSize = 4096;
        private const int NumRows = 50000;
        private const int Iterations = 1;

        static void Main()
        {
            var data = new byte[NumRows, RowSize];

            var sw = new Stopwatch();
            sw.Start();

            for (var loops = 0; loops < Iterations; loops++)
            {
                for (var i = 0; i < NumRows; i++)
                {
                    for (var j = 0; j < RowSize; j++)
                    {
                        int value = data[i, j];
                    }
                }
            }

            sw.Stop();

            Console.WriteLine("Elapsed: {0}ms", sw.ElapsedMilliseconds);
            Console.WriteLine("Press enter to continue");
            Console.ReadLine();

            sw.Start();

            for (var loops = 0; loops < Iterations; loops++)
            {
                for (var i = 0; i < RowSize; i++)
                {
                    for (var j = 0; j < NumRows; j++)
                    {
                        int value = data[j, i];
                    }
                }
            }

            sw.Stop();

            Console.WriteLine("Elapsed: {0}ms", sw.ElapsedMilliseconds);
            Console.WriteLine("Press enter to continue");
            Console.ReadLine();
        }
    }
}
