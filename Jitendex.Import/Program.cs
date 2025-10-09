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
using Jitendex.Import.Kanjidic2;
using Jitendex.Import.KanjiVG;

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
            RunKanjiVG(),
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

        var service = Kanjidic2ServiceProvider.GetService(kanjidic2Paths);
        var kanjidic2 = await service.ReadKanjidic2Async();

        var db = new Kanjidic2Context();
        await BuildDb.InitializeAsync(db);
        await db.Entries.AddRangeAsync(kanjidic2.Entries);
        await db.SaveChangesAsync();
    }

    private static async Task RunKanjiVG()
    {
        var kanjiVGPaths = new KanjiVG.FilePaths
        (
            SvgArchive: Path.Combine("Resources", "kanjivg", "kanji.tar.br")
        );

        var service = KanjiVGServiceProvider.GetService(kanjiVGPaths);
        var kanjiVG = await service.ReadKanjiVGAsync();
    }
}
