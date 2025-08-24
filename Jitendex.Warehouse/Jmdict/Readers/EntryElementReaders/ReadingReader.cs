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
using Jitendex.Warehouse.Jmdict.Models.EntryElements;
using Jitendex.Warehouse.Jmdict.Models.EntryElements.ReadingElements;
using Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders.ReadingElementReaders;

namespace Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders;

internal class ReadingReader
{
    private readonly XmlReader Reader;
    private readonly EntityFactory Factory;
    private readonly InfoReader InfoReader;
    private readonly PriorityReader PriorityReader;

    public ReadingReader(XmlReader reader, EntityFactory factory)
    {
        Reader = reader;
        Factory = factory;
        InfoReader = new InfoReader(reader, factory);
        PriorityReader = new PriorityReader(reader, factory);
    }

    public async Task<Reading> ReadAsync(Entry entry)
    {
        var reading = new Reading
        {
            EntryId = entry.Id,
            Order = entry.Readings.Count + 1,
            Text = string.Empty,
            NoKanji = false,
            Entry = entry,
        };

        var exit = false;
        while (!exit && await Reader.ReadAsync())
        {
            switch (Reader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(reading);
                    break;
                case XmlNodeType.Text:
                    var text = await Reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{Reading.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = Reader.Name == Reading.XmlTagName;
                    break;
            }
        }
        return reading;
    }

    private async Task ReadChildElementAsync(Reading reading)
    {
        switch (Reader.Name)
        {
            case "reb":
                reading.Text = await Reader.ReadElementContentAsStringAsync();
                break;
            case "re_nokanji":
                reading.NoKanji = true;
                break;
            case "re_restr":
                var kanjiFormText = await Reader.ReadElementContentAsStringAsync();
                reading.ConstraintKanjiFormTexts.Add(kanjiFormText);
                break;
            case Info.XmlTagName:
                var readingInfo = await InfoReader.ReadAsync(reading);
                reading.Infos.Add(readingInfo);
                break;
            case Priority.XmlTagName:
                var priority = await PriorityReader.ReadAsync(reading);
                reading.Priorities.Add(priority);
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{Reader.Name}` found in element `{Reading.XmlTagName}`");
        }
    }
}
