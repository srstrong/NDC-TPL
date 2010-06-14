using System;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace NestedTasks
{
    class NestedTasks
    {
        static void Main()
        {
           // TerminationOrder();

            ExceptionManagement();
        }

        static void TerminationOrder()
        {
            var outer = Task.Factory.StartNew(() =>
                {
                    var email = BuildEmail();

                    Task.Factory.StartNew(() => SendEmail(email));

                    Console.WriteLine("Email sender task has been started");
                });

            Console.WriteLine("Back in Main");

            outer.Wait();

            Console.WriteLine("Outer task completed");

            Thread.Sleep(2500);
        }

        static void ExceptionManagement()
        {
            var outer = Task.Factory.StartNew(() =>
                {
                    var email = BuildEmail();

                    Task.Factory.StartNew(() => FailEmail(email), TaskCreationOptions.AttachedToParent);

                    Console.WriteLine("Email sender task has been started");
                });

            try
            {
                outer.Wait();
            }
            catch (AggregateException e)
            {
                e = e.Flatten();

                Console.WriteLine("Outer threw exception {0}", e);
            }

            outer.DisplayState("Outer task");

            Thread.Sleep(2500);

            outer = null;
            GC.Collect();
        }

        private static string BuildEmail()
        {
            Console.WriteLine("Creating email");
            Thread.Sleep(1000);

            return "Hello, world";
        }

        private static void SendEmail(string email)
        {
            Console.WriteLine("Sending email {0}", email);
            Thread.Sleep(1000);
            Console.WriteLine("Email sent");
        }

        private static void FailEmail(string email)
        {
            throw new SmtpFailedRecipientException();
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
