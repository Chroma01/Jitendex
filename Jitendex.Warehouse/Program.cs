using System.Diagnostics;

namespace Jitendex.Warehouse;

public class Program
{
    public static async Task Main()
    {
        var sw = new Stopwatch();
        sw.Start();

        var JmdictPath = Path.Combine("Resources", "edrdg", "JMdict");
        var entryNumbers = new List<int>();

        await foreach (var entry in Jmdict.Loader.Entries(JmdictPath))
        {
            entryNumbers.Add(entry.Sequence);
        }
        Console.WriteLine($"Read {entryNumbers.Count} entries.");

        sw.Stop();
        Console.WriteLine($"Finished in {double.Round(sw.Elapsed.TotalSeconds, 1)} seconds.");
    }
}