/*
Copyright (c) 2025 Stephen Kraus

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the
terms of the GNU Affero General Public License as published by the Free
Software Foundation, either version 3 of the License, or (at your option) any
later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along
with Jitendex. If not, see <https://www.gnu.org/licenses/>.
*/

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