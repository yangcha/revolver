# Revolver

A thread-safe circular buffer in C# is implementing a non-blocking producer-consumer pattern.

Sample usage:

```cs
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
```

## References
* https://ericlippert.com/2015/11/16/monitor-madness-part-one/
* https://devblogs.microsoft.com/oldnewthing/20180201-00/?p=97946
