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

using System.Text.Json;
using System.Xml;
using Jitendex.Warehouse.Jmdict.Models;
using Microsoft.EntityFrameworkCore;

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
        await FixCrossReferencesAsync(entries);
        // Anticipating more operations here later.
    }

    private async static Task FixCrossReferencesAsync(List<Entry> entries)
    {
        var headwordToEntry = MapHeadwordCombinationsToEntries(entries);

        var crossReferences = entries
            .SelectMany(e => e.Senses)
            .SelectMany(s => s.CrossReferences);

        var seqpath = Path.Combine("Resources", "jmdict", "cross_reference_sequences.json");
        Dictionary<string, int> cachedSequences;
        await using (var stream = File.OpenRead(seqpath))
            cachedSequences = await JsonSerializer.DeserializeAsync<Dictionary<string, int>>(stream) ?? [];

        foreach (var xref in crossReferences)
        {
            var key = (xref.RefText1, xref.RefText2);
            var possibleTargetEntries = headwordToEntry[key]
                .Where(e =>
                    e.Id != xref.EntryId &&  // Entries cannot reference themselves.
                    e.CorpusId == xref.Sense.Entry.CorpusId &&  // Assume references are within same corpus.
                    e.Senses.Count >= xref.RefSenseOrder)  // Referenced entry must contain the referenced sense ID.
                .ToList();

            Entry? targetEntry;
            if (possibleTargetEntries.Count == 0)
            {
                throw new Exception($"No entries found for cross reference `{xref.RawKey()}`");
            }
            else if (possibleTargetEntries.Count == 1)
            {
                targetEntry = possibleTargetEntries.First();
            }
            else
            {
                string cacheKey = xref.RawKey();
                if (cachedSequences.TryGetValue(cacheKey, out int targetEntryId))
                {
                    targetEntry = possibleTargetEntries
                        .Where(e => e.Id == targetEntryId)
                        .FirstOrDefault();
                    if (targetEntry is null)
                    {
                        Console.WriteLine($"Cached ID `{targetEntryId}` is invalid for reference `{cacheKey}`");
                        targetEntry = possibleTargetEntries.First();
                    }
                }
                else
                {
                    Console.WriteLine($"Reference could refer to {possibleTargetEntries.Count} possible entries:");
                    Console.WriteLine($"\t\"{cacheKey}\": {string.Join(" || ", possibleTargetEntries.Select(e => e.Id.ToString()).ToList())},");
                    targetEntry = possibleTargetEntries.First();
                }
            }

            xref.RefEntryId = targetEntry.Id;
            xref.RefSense = targetEntry.Senses
                .Where(s => s.Order == xref.RefSenseOrder).First();
            xref.RefSense.ReverseCrossReferences.Add(xref);

            if (targetEntry.KanjiForms.All(k => k.IsHidden()))
            {
                string searchText;
                if (targetEntry.KanjiForms.Any(k => k.Text == xref.RefText1))
                {
                    searchText = targetEntry.Readings
                        .Where(r => !r.IsHidden())
                        .First().Text;
                    Console.WriteLine($"Entry {xref.EntryId} has a reference to hidden form {xref.RefText1} in entry {targetEntry.Id}");
                }
                else
                {
                    searchText = xref.RefText1;
                }

                var refReading = targetEntry.Readings
                    .Where(b => b.Text == searchText).First();

                if (refReading.IsHidden())
                {
                    Console.WriteLine($"Entry {xref.EntryId} has a reference to hidden form {xref.RefText1} in entry {targetEntry.Id}");
                    refReading = targetEntry.Readings
                        .Where(r => !r.IsHidden()).First();
                }

                xref.RefReading = refReading;
                xref.RefReadingOrder = refReading.Order;
            }
            else if (targetEntry.KanjiForms.Any(k => k.Text == xref.RefText1))
            {
                var refKanjiForm = targetEntry.KanjiForms
                    .Where(k => k.Text == xref.RefText1).First();

                if (refKanjiForm.IsHidden())
                {
                    Console.WriteLine($"Entry {xref.EntryId} has a reference to hidden form {xref.RefText1} in entry {targetEntry.Id}");
                    refKanjiForm = targetEntry.KanjiForms
                        .Where(k => !k.IsHidden()).First();
                }

                xref.RefKanjiForm = refKanjiForm;
                xref.RefKanjiFormOrder = refKanjiForm.Order;

                var refReading = xref.RefText2 is null ?
                    refKanjiForm.ReadingBridges.First().Reading :
                    refKanjiForm.ReadingBridges
                        .Where(b => b.Reading.Text == xref.RefText2)
                        .First().Reading;

                xref.RefReading = refReading;
                xref.RefReadingOrder = refReading.Order;
            }
            else
            {
                var refReading = targetEntry.Readings
                    .Where(b => b.Text == xref.RefText1).First();
                if (refReading.IsHidden())
                {
                    Console.WriteLine($"Entry {xref.EntryId} has a reference to hidden form {xref.RefText1} in entry {targetEntry.Id}");
                    refReading = targetEntry.Readings
                        .Where(r => !r.IsHidden()).First();
                }
                xref.RefReading = refReading;
                xref.RefReadingOrder = refReading.Order;
            }
        }
    }

    private static Dictionary<(string, string?), List<Entry>> MapHeadwordCombinationsToEntries(List<Entry> entries)
    {
        var map = new Dictionary<(string, string?), List<Entry>>();
        foreach (var entry in entries)
        {
            var keys = entry.HeadwordPermutations();
            foreach (var key in keys)
            {
                if (map.TryGetValue(key, out List<Entry>? values))
                {
                    values.Add(entry);
                }
                else
                {
                    map[key] = [entry];
                }
            }
        }
        return map;
    }

    private static List<(string, string?)> HeadwordPermutations(this Entry entry)
    {
        var keys = new List<(string, string?)>();
        foreach (var reading in entry.Readings)
        {
            keys.Add((reading.Text, null));
        }
        foreach (var kanjiForm in entry.KanjiForms)
        {
            keys.Add((kanjiForm.Text, null));
            foreach (var reading in entry.Readings)
            {
                keys.Add((kanjiForm.Text, reading.Text));
            }
        }
        return keys;
    }
}
