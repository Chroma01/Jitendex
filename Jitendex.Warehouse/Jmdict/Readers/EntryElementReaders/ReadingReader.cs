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
using Jitendex.Warehouse.Jmdict.Models;
using Jitendex.Warehouse.Jmdict.Models.EntryElements;
using Jitendex.Warehouse.Jmdict.Models.EntryElements.ReadingElements;

namespace Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders;

internal class ReadingReader : IJmdictReader<Entry, Reading>
{
    private readonly ILogger<ReadingReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly IJmdictReader<Reading, ReadingInfo> _infoReader;
    private readonly IJmdictReader<Reading, ReadingPriority> _priorityReader;

    public ReadingReader(ILogger<ReadingReader> logger, XmlReader xmlReader, IJmdictReader<Reading, ReadingInfo> infoReader, IJmdictReader<Reading, ReadingPriority> priorityReader) =>
        (_logger, _xmlReader, _infoReader, _priorityReader) =
        (@logger, @xmlReader, @infoReader, @priorityReader);

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
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(reading);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    Log.UnexpectedTextNode(_logger, Reading.XmlTagName, text);
                    reading.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == Reading.XmlTagName;
                    break;
            }
        }
        return reading;
    }

    private async Task ReadChildElementAsync(Reading reading)
    {
        switch (_xmlReader.Name)
        {
            case "reb":
                reading.Text = await _xmlReader.ReadElementContentAsStringAsync();
                break;
            case "re_nokanji":
                reading.NoKanji = true;
                break;
            case "re_restr":
                var kanjiFormText = await _xmlReader.ReadElementContentAsStringAsync();
                reading.ConstraintKanjiFormTexts.Add(kanjiFormText);
                break;
            case ReadingInfo.XmlTagName:
                var readingInfo = await _infoReader.ReadAsync(reading);
                reading.Infos.Add(readingInfo);
                break;
            case ReadingPriority.XmlTagName:
                var priority = await _priorityReader.ReadAsync(reading);
                reading.Priorities.Add(priority);
                break;
            default:
                Log.UnexpectedChildElement(_logger, _xmlReader.Name, Reading.XmlTagName);
                reading.IsCorrupt = true;
                break;
        }
    }
}
