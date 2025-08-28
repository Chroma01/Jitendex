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
using Jitendex.Warehouse.Kanjidic2.Models.Groups;
using Jitendex.Warehouse.Kanjidic2.Models.EntryElements;

namespace Jitendex.Warehouse.Kanjidic2.Readers.GroupReaders;

internal class ReadingMeaningGroupReader
{
    private readonly ILogger<ReadingMeaningGroupReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly ReadingMeaningReader _readingMeaningReader;

    public ReadingMeaningGroupReader(ILogger<ReadingMeaningGroupReader> logger, XmlReader xmlReader, ReadingMeaningReader readingMeaningReader) =>
        (_logger, _xmlReader, _readingMeaningReader) =
        (@logger, @xmlReader, @readingMeaningReader);

    public async Task<ReadingMeaningGroup> ReadAsync(Entry entry)
    {
        var group = new ReadingMeaningGroup
        {
            Character = entry.Character,
            ReadingMeaning = null,
            Entry = entry,
        };

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(group);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    Log.UnexpectedTextNode(_logger, entry.Character, ReadingMeaningGroup.XmlTagName, text);
                    entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == ReadingMeaningGroup.XmlTagName;
                    break;
            }
        }
        return group;
    }

    private async Task ReadChildElementAsync(ReadingMeaningGroup group)
    {
        switch (_xmlReader.Name)
        {
            case ReadingMeaning.XmlTagName:
                if (group.ReadingMeaning != null)
                    throw new Exception($"Reading-meaning group for character `{group.Character}` has more than one reading-meaning set.");
                var readingMeaning = await _readingMeaningReader.ReadAsync(group);
                group.ReadingMeaning = readingMeaning;
                break;
            case Nanori.XmlTagName:
                group.Nanoris.Add(new Nanori
                {
                    Character = group.Character,
                    Order = group.Nanoris.Count + 1,
                    Text = await _xmlReader.ReadElementContentAsStringAsync(),
                    Entry = group.Entry,
                });
                break;
            default:
                Log.UnexpectedChildElement(_logger, group.Entry.Character, _xmlReader.Name, ReadingMeaningGroup.XmlTagName);
                group.Entry.IsCorrupt = true;
                return;
        }
    }
}
