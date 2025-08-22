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
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Jitendex.Warehouse.Jmdict.Models;

namespace Jitendex.Warehouse.Jmdict;

public static class Loader
{
    public async static Task ImportAsync()
    {
        var parseTask = ParseJmdictAsync();
        await using var db = new JmdictContext();
        await InitializeDatabaseAsync(db);

        var entries = await parseTask;

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

    private async static Task<List<Entry>> ParseJmdictAsync()
    {
        var entries = new List<Entry>();
        var jmdictPath = Path.Combine("Resources", "edrdg", "JMdict");
        await foreach (var entry in EnumerateEntriesAsync(jmdictPath))
        {
            entries.Add(entry);
        }
        await PostProcessAsync(entries);
        return entries;
    }

    private async static IAsyncEnumerable<Entry> EnumerateEntriesAsync(string path)
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

        while (await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.DocumentType:
                    var entityNameToDescription = await reader
                        .GetDocumentEntityNameToDescriptionAsync();
                    ITag.DescriptionToName = entityNameToDescription
                        .ToDictionary(x => x.Value, x => x.Key);
                    break;
                case XmlNodeType.Element:
                    if (reader.Name == Entry.XmlTagName)
                    {
                        var entry = await reader.ReadEntryAsync();
                        yield return entry;
                    }
                    break;
            }
        }
    }

    private async static Task PostProcessAsync(List<Entry> entries)
    {
        var seqpath = Path.Combine("Resources", "jmdict", "cross_reference_sequences.json");
        Dictionary<string, int> cachedSequences;
        await using (var stream = File.OpenRead(seqpath))
            cachedSequences = await JsonSerializer.DeserializeAsync<Dictionary<string, int>>(stream) ?? [];

        ReferenceSequencer.FixCrossReferences(entries, cachedSequences);
        // Anticipating more operations here later.
    }
}
