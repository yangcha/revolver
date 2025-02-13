using Concurrent;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace revolver
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            int itemsToAdd = 500;
            var rev = new Revolver<Image>(1);

            Task.Run(() =>
            {
                while (true)
                {
                    var im = rev.Take();
                    if (im == null)
                    {
                        break;
                    }
                    Console.WriteLine("Take:{0} ", im);
                    im.Dispose();

                    // Simulate a slow consumer. This will cause
                    // the circular buffer to drop the old items.
                    Thread.SpinWait(100000);
                }
            });

            // A simple non-blocking producer with no cancellation.
            Task.Run(() =>
            {
                for (int i = 0; i < itemsToAdd; i++)
                {
                    rev.Add(new Bitmap(100, 100));
                    Console.WriteLine("Add:{0} Count={1}", i, rev.Count);
                }

                rev.Finish();
            });
            // Keep the console display open in debug mode.
            Console.ReadLine();
        }
    }
}
