# Revolver

A thread-safe circular buffer in C# is implementing a non-blocking producer-consumer pattern.

Sample usage:

```cs
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

        // A simple non-blocking producer with no cancellation.
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
```
