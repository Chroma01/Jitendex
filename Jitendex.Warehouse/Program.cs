using System.Diagnostics;

namespace Jitendex.Warehouse;

public class Program
{
    public static async Task Main()
    {
        var sw = new Stopwatch();
        sw.Start();

        var db = new WarehouseContext();
        Console.WriteLine($"Database path: {db.DbPath}.");

        var JmdictPath = Path.Combine("Resources", "edrdg", "JMdict");
        await foreach (var entry in Jmdict.Loader.Entries(JmdictPath))
        {
            await db.JmdictEntries.AddAsync(entry);
        }
        await db.SaveChangesAsync();

        var Kanjidic2Path = Path.Combine("Resources", "edrdg", "kanjidic2.xml");
        await foreach (var entry in Kanjidic2.Loader.Entries(Kanjidic2Path))
        {
            await db.Kanjidic2Entries.AddAsync(entry);
        }
        await db.SaveChangesAsync();

        sw.Stop();
        Console.WriteLine($"Finished in {double.Round(sw.Elapsed.TotalSeconds, 1)} seconds.");
    }
}