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
using Microsoft.EntityFrameworkCore;

namespace Jitendex.Warehouse.Kanjidic2;

public class Loader
{
    public async static Task ImportAsync(WarehouseContext db, string kanjidic2Path)
    {
        await db.Database.ExecuteSqlRawAsync(@"
            DELETE FROM 'Kanjidic2.Readings';
            DELETE FROM 'Kanjidic2.Meanings';
            DELETE FROM 'Kanjidic2.ReadingMeaningGroups';
            DELETE FROM 'Kanjidic2.ReadingMeanings';
            DELETE FROM 'Kanjidic2.Entries';");

        var Kanjidic2Path = Path.Combine("Resources", "edrdg", "kanjidic2.xml");
        await foreach (var entry in EntriesAsync(Kanjidic2Path))
        {
            await db.Kanjidic2Entries.AddAsync(entry);
        }
        await db.SaveChangesAsync();
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

        while (await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (reader.Name == Entry.XmlTagName)
                    {
                        var entry = await Entry.FromXmlAsync(reader);
                        yield return entry;
                    }
                    break;
                default:
                    break;
            }
        }
    }

}
