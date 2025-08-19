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

using System.Xml;
using Jitendex.Warehouse.Jmdict.Models;
using Microsoft.EntityFrameworkCore;

namespace Jitendex.Warehouse.Jmdict;

public static class Loader
{
    public async static Task ImportAsync()
    {
        var parseJmdictTask = ParseJmdict();
        await using var db = new JmdictContext();
        await InitializeDatabaseAsync(db);

        var entries = await parseJmdictTask;
        await db.Entries.AddRangeAsync(entries);
        await db.SaveChangesAsync();
    }

    private async static Task InitializeDatabaseAsync(JmdictContext db)
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

    private async static Task<List<Entry>> ParseJmdict()
    {
        var entries = new List<Entry>();
        var jmdictPath = Path.Combine("Resources", "edrdg", "JMdict");
        await foreach (var entry in EntriesAsync(jmdictPath))
        {
            entries.Add(entry);
        }
        return entries;
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
        DocumentMetadata? docMeta = null;

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
