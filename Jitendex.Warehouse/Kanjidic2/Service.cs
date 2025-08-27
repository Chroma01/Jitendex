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
using Microsoft.Extensions.Logging;
using Jitendex.Warehouse.Kanjidic2.Models;
using Jitendex.Warehouse.Kanjidic2.Readers;

namespace Jitendex.Warehouse.Kanjidic2;

internal class Service
{
    private readonly XmlReader _xmlReader;
    private readonly EntryReader _entryReader;
    private readonly ILogger<Service> _logger;

    public Service(XmlReader @xmlReader, EntryReader @entryReader, ILogger<Service> @logger) =>
        (_xmlReader, _entryReader, _logger) =
        (@xmlReader, @entryReader, @logger);

    public async Task<List<Entry>> CreateEntriesAsync()
    {
        var entries = new List<Entry>();
        await foreach (var entry in EnumerateEntriesAsync())
        {
            entries.Add(entry);
        }
        return entries;
    }

    private async IAsyncEnumerable<Entry> EnumerateEntriesAsync()
    {
        // var path = Path.Combine("Resources", "edrdg", "kanjidic2.xml");
        // await using var stream = File.OpenRead(path);

        // var readerSettings = new XmlReaderSettings
        // {
        //     Async = true,
        //     DtdProcessing = DtdProcessing.Parse,
        //     MaxCharactersFromEntities = long.MaxValue,
        //     MaxCharactersInDocument = long.MaxValue,
        // };

        // using var reader = XmlReader.Create(stream, readerSettings);

        while (await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    if (_xmlReader.Name == Entry.XmlTagName)
                    {
                        var entry = await _entryReader.ReadAsync();
                        yield return entry;
                    }
                    break;
            }
        }
    }
}
