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
using Jitendex.Warehouse.Kanjidic2.Models;
using Jitendex.Warehouse.Kanjidic2.Readers;

namespace Jitendex.Warehouse.Kanjidic2;

public class Loader
{
    public async static Task<List<Entry>> EntriesAsync(Resources resources, bool save)
    {
        var db = new Kanjidic2Context();
        var initializeDbTask = save ? BuildDb.InitializeAsync(db) : Task.CompletedTask;
        var path = Path.Combine("Resources", "edrdg", "kanjidic2.xml");

        var entries = new List<Entry>();
        await foreach (var entry in EnumerateEntriesAsync(path))
        {
            entries.Add(entry);
        }

        if (save)
        {
            await initializeDbTask;
            await db.Entries.AddRangeAsync(entries);
            await db.SaveChangesAsync();
        }

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
}
