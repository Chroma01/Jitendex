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
using System.Xml;
using Jitendex.Warehouse.Jmdict.Models;
using Microsoft.EntityFrameworkCore;

namespace Jitendex.Warehouse.Jmdict;

public static class Loader
{
    public async static Task ImportAsync()
    {
        await using var db = new JmdictContext();
        Console.WriteLine($"Jmdict database path: {db.DbPath}.");

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        await using (var connection = db.Database.GetDbConnection())
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = @"
                PRAGMA synchronous = OFF;
                PRAGMA journal_mode = MEMORY;
                PRAGMA temp_store = MEMORY;
                PRAGMA cache_size = -200000;";
            await command.ExecuteNonQueryAsync();
        }

        int i = 0;
        int batchSize = 5000;

        var importSw = new Stopwatch();
        importSw.Start();
        var batchSw = new Stopwatch();
        batchSw.Start();

        using var transaction = await db.Database.BeginTransactionAsync();

        var jmdictPath = Path.Combine("Resources", "edrdg", "JMdict");
        await foreach (var entry in EntriesAsync(jmdictPath))
        {
            await db.Entries.AddAsync(entry);
            if (++i % batchSize == 0)
            {
                await db.SaveChangesAsync();
                Console.WriteLine($"Imported batch of {batchSize} Jmdict entries in {double.Round(batchSw.Elapsed.TotalSeconds, 2):N2}s (avg {double.Round(1000 * batchSw.Elapsed.TotalSeconds / batchSize, 2):N2}ms / entry).");
                batchSw.Restart();
            }
        }
        await db.SaveChangesAsync();
        Console.WriteLine($"Imported batch of {i % batchSize} Jmdict entries in {double.Round(batchSw.Elapsed.TotalSeconds, 2):N2}s (avg {double.Round(1000 * batchSw.Elapsed.TotalSeconds / batchSize, 2):N2}ms / entry).");

        await transaction.CommitAsync();
        importSw.Stop();
        Console.WriteLine($"Imported {i} Jmdict entries in {double.Round(importSw.Elapsed.TotalSeconds, 2):N2}s (avg {double.Round(1000 * importSw.Elapsed.TotalSeconds / i, 2):N2}ms / entry).");
    }

    private async static IAsyncEnumerable<Entry> EntriesAsync(string path)
    {
        await using var stream = File.OpenRead(path);

        var readerSettings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Parse,
            MaxCharactersFromEntities = long.MaxValue,
            MaxCharactersInDocument = long.MaxValue,
        };

        using var reader = XmlReader.Create(stream, readerSettings);
        var docMeta = new DocumentMetadata
        {
            Name = string.Empty,
            EntityValueToName = [],
        };

        while (await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.DocumentType:
                    docMeta = await DocumentMetadata.FromXmlAsync(reader);
                    break;
                case XmlNodeType.Element:
                    if (reader.Name == Entry.XmlTagName)
                    {
                        var entry = await Entry.FromXmlAsync(reader, docMeta);
                        yield return entry;
                    }
                    break;
            }
        }
    }
}
