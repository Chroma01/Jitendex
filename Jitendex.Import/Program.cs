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
using Jitendex.Import.Jmdict;
using Jitendex.Import.Kanjidic2;

namespace Jitendex.Import;

public class Program
{
    public static void Main()
    {
        var sw = new Stopwatch();
        sw.Start();

        var tasks = new Task[]
        {
            RunKanjidic2(),
            RunJmdict(),
        };
        Task.WaitAll(tasks);

        Console.WriteLine($"Finished in {double.Round(sw.Elapsed.TotalSeconds, 1)} seconds.");
    }

    private static async Task RunKanjidic2()
    {
        var kanjidic2Paths = new Kanjidic2.FilePaths
        (
            XmlFile: Path.Combine("Resources", "edrdg", "kanjidic2.xml")
        );

        var kanjidic2Service = Kanjidic2ServiceProvider.GetService(kanjidic2Paths);
        var kanjidic2Entries = await kanjidic2Service.CreateEntriesAsync();

        var db = new Kanjidic2Context();
        await BuildDb.InitializeAsync(db);
        await db.Entries.AddRangeAsync(kanjidic2Entries);
        await db.SaveChangesAsync();
    }

    private static async Task RunJmdict()
    {
        var jmdictPaths = new Jmdict.FilePaths
        (
            XmlFile: Path.Combine("Resources", "edrdg", "JMdict_e_examp"),
            XRefCache: Path.Combine("Resources", "jmdict", "cross_reference_sequences.json")
        );

        var jmdictService = JmdictServiceProvider.GetService(jmdictPaths);
        var jmdictEntries = await jmdictService.CreateEntriesAsync();

        var db = new JmdictContext();
        await BuildDb.InitializeAsync(db);
        await db.Entries.AddRangeAsync(jmdictEntries);
        await db.SaveChangesAsync();
    }
}

