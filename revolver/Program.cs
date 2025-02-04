﻿using Concurrent;
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
            try
            {
                Task.Run(() =>
                {
                    while (!rev.IsCompleted)
                    {
                        var im = rev.Take();
                        Console.WriteLine("Take:{0} ", im);

                        // Simulate a slow consumer. This will cause
                        // the circular buffer to drop the old items.
                        Thread.SpinWait(100000);
                    }
                });

                // A simple blocking producer with no cancellation.
                Task.Run(() =>
                {
                    for (int i = 0; i < itemsToAdd; i++)
                    {
                        rev.Add(new Bitmap(100, 100));
                        Console.WriteLine("Add:{0} Count={1}", i, rev.Count);
                    }

                    rev.CompleteAdding();
                });
            }
            finally
            {
                rev?.Dispose();
            }
            // Keep the console display open in debug mode.
            Console.ReadLine();
        }
    }
}
