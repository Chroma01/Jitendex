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

namespace Jitendex.KanjiVG;

public class Program
{
    public static async Task Main(string[] args)
    {
        var sw = new Stopwatch();
        sw.Start();

        var kanjivgFileArgument = new Argument<FileInfo>("kanjivg-file")
        {
            Description = "Path to a compressed (Brotli) tar of KanjiVG files",
        };

        var rootCommand = new RootCommand("Jitendex.KanjiVG: Import KanjiVG data")
        {
            kanjivgFileArgument
        };

        var parseResult = rootCommand.Parse(args);
        if (parseResult.Errors.Count > 0)
        {
            foreach (var parseError in parseResult.Errors)
            {
                Console.Error.WriteLine(parseError.Message);
            }
            return;
        }

        var files = new Files
        {
            SvgArchive = parseResult.GetRequiredValue(kanjivgFileArgument)
        };

        var reader = ReaderProvider.GetReader(files);
        var kanjivg = await reader.ReadAsync();

        var db = new KanjiVGContext();
        await InitializeAsync(db);
        await db.Entries.AddRangeAsync(kanjivg.Entries);
        await db.SaveChangesAsync();

        Console.WriteLine($"Finished in {double.Round(sw.Elapsed.TotalSeconds, 1)} seconds.");
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

