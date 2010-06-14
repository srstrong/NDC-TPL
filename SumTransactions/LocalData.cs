using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SumTransactions
{
    class LocalData
    {
        private static long _transactionTotal;

        static void Main()
        {
            Parallel.ForEach(GetTransactions(),                // Data source
                           () => 0L,                        // Initialisation function, called once per thread
                           (trx, pls, currentThreadState) =>  // Called for each data item
                               currentThreadState + trx.Amount, 
                           total => UpdateGrandTotal(total)   // Finalisation function, called once per thread
                );

            Console.WriteLine("Transaction total is {0}", _transactionTotal);
        }

        static IEnumerable<Transaction> GetTransactions()
        {
            var rand = new Random();

            return Enumerable.Range(1, 10000000)
                           .Select(i => new Transaction { AccountId = i, Amount = rand.Next(101) });
        }

        static void UpdateGrandTotal(long threadTotal)
        {
            // Add the thread's total to the grand total.  But need to be threadsafe...
            Interlocked.Add(ref _transactionTotal, threadTotal);

            // Or, if you need something more general but still "lock free"...
            /*
                long oldTotal;
                long newTotal;
                do
                {
                    oldTotal = _transactionTotal;
                    newTotal = oldTotal + threadTotal;

                } while (Interlocked.CompareExchange(ref _transactionTotal, newTotal, oldTotal) != oldTotal);
              */

            // Or, just use a lock.  There is a certain simplicity about it...
            /*
                lock (_someSharedLock)
                {
                    _transactionTotal += threadTotal;
                }
              */
        }
    }
}
