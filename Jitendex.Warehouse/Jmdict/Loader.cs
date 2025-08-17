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
using Microsoft.EntityFrameworkCore;
using Jitendex.Warehouse.Jmdict.Models;

namespace Jitendex.Warehouse.Jmdict;

public static class Loader
{
    public async static Task ImportAsync()
    {
        var db = new JmdictContext();
        Console.WriteLine($"Jmdict database path: {db.DbPath}.");

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        var jmdictPath = Path.Combine("Resources", "edrdg", "JMdict");
        await foreach (var entry in EntriesAsync(jmdictPath))
        {
            await db.Entries.AddAsync(entry);
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
