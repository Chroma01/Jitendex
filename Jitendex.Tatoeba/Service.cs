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

using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Jitendex.Tatoeba.Readers;
using Jitendex.Tatoeba.SQLite;
using static Jitendex.EdrdgDictionaryArchive.DictionaryFile;
using static Jitendex.EdrdgDictionaryArchive.Service;

namespace Jitendex.Tatoeba;

public static class Service
{
    public static async Task UpdateAsync(DirectoryInfo? archiveDirectory)
    {
        var previousDocument = await GetPreviousDocumentAsync(archiveDirectory);
        var previousDate = previousDocument.Date;

        // while (true)
        // {
        //     var (nextFile, nextDate) = GetNextEdrdgFile(examples, previousDate, archiveDirectory);

        //     if (nextFile is null)
        //     {
        //         break;
        //     }

        //     var nextDocument = await ReadAsync(nextFile, nextDate);

        //     await Console.Error.WriteLineAsync($"Updating database with data from {nextDate:yyyy-MM-dd}");
        //     Database.Update(previousDocument, nextDocument);

        //     previousDocument = nextDocument;
        //     previousDate = nextDate;
        // }
    }

    private static async Task<Dto.Document> GetPreviousDocumentAsync(DirectoryInfo? archiveDirectory)
    {
        var previousDate = GetPreviousDate();
        if (previousDate == default)
        {
            var (file, date) = GetNextEdrdgFile(examples, previousDate, archiveDirectory);
            var document = await ReadAsync(file!, date);
            await Console.Error.WriteLineAsync($"Initializing database with data from {date:yyyy-MM-dd}");
            Database.Initialize(document);
            return document;
        }
        else
        {
            var file = GetEdrdgFile(examples, previousDate, archiveDirectory);
            var document = await ReadAsync(file, previousDate);
            return document;
        }
    }

    private static DateOnly GetPreviousDate()
    {
        using var db = new Context();
        db.Database.EnsureCreated();
        return db.Metadata
            .OrderBy(static x => x.Id)
            .Select(static x => x.Date)
            .LastOrDefault();
    }

    private static async Task<Dto.Document> ReadAsync(FileInfo file, DateOnly date)
    {
        using var loggerFactory = CreateLoggerFactory();
        var logger = loggerFactory.CreateLogger<TatoebaReader>();

        await using FileStream fs = new(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
        await using BrotliStream bs = new(fs, CompressionMode.Decompress);
        using var streamReader = new StreamReader(bs);

        var reader = new TatoebaReader(logger, streamReader, date);
        return await reader.ReadAsync();
    }

    private static ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(static builder =>
            builder.AddSimpleConsole(static options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = false;
                options.TimestampFormat = "HH:mm:ss ";
            }));
}
