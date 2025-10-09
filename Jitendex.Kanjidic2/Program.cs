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
using System.CommandLine;
using Microsoft.EntityFrameworkCore;
using Jitendex.Kanjidic2.Readers;

namespace Jitendex.Kanjidic2;

public class Program
{
    public static async Task Main(string[] args)
    {
        var sw = new Stopwatch();
        sw.Start();

        var kanjidic2FileArgument = new Argument<FileInfo>("kanjidic2-file")
        {
            Description = "Path to Kanjidic2 XML file",
        };

        var description = $"{nameof(Jitendex)}.{nameof(Kanjidic2)}: Import a Kanjidic2 XML document";
        var rootCommand = new RootCommand(description) { kanjidic2FileArgument };

        var parseResult = rootCommand.Parse(args);
        if (parseResult.Errors.Count == 0)
        {
            var jmdictFile = parseResult.GetValue(kanjidic2FileArgument)!;
            await RunKanjidic2(jmdictFile);
        }
        else
        {
            foreach (var parseError in parseResult.Errors)
            {
                Console.Error.WriteLine(parseError.Message);
            }
        }

        Console.WriteLine($"Finished in {double.Round(sw.Elapsed.TotalSeconds, 1)} seconds.");
    }

    private static async Task RunKanjidic2(FileInfo kanjidic2File)
    {
        var kanjidic2Paths = new FilePaths
        {
            XmlFile = kanjidic2File.FullName,
        };

        var reader = Kanjidic2ReaderProvider.GetReader(kanjidic2Paths);
        var kanjidic2 = await reader.ReadKanjidic2Async();

        var db = new Kanjidic2Context();
        await InitializeAsync(db);
        await db.Entries.AddRangeAsync(kanjidic2.Entries);
        await db.SaveChangesAsync();
    }

    private async static Task InitializeAsync(DbContext db)
    {
        // Delete and recreate database file.
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        // For faster importing, write data to memory
        // rather than to the disk during initial load.
        await using var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            PRAGMA synchronous = OFF;
            PRAGMA journal_mode = MEMORY;
            PRAGMA temp_store = MEMORY;
            PRAGMA cache_size = -200000;";
        await command.ExecuteNonQueryAsync();
    }
}

