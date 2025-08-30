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

internal partial class ReadingReader : IJmdictReader<Entry, Reading>
{
    private readonly ILogger<ReadingReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly IJmdictReader<Reading, Restriction> _restrictionReader;
    private readonly IJmdictReader<Reading, ReadingInfo> _infoReader;
    private readonly IJmdictReader<Reading, ReadingPriority> _priorityReader;

    public ReadingReader(ILogger<ReadingReader> logger, XmlReader xmlReader, IJmdictReader<Reading, Restriction> restrictionReader, IJmdictReader<Reading, ReadingInfo> infoReader, IJmdictReader<Reading, ReadingPriority> priorityReader) =>
        (_logger, _xmlReader, _restrictionReader, _infoReader, _priorityReader) =
        (@logger, @xmlReader, @restrictionReader, @infoReader, @priorityReader);

    public async Task ReadAsync(Entry entry)
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
                    reading.Entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == Reading.XmlTagName;
                    break;
            }
        }

        CheckForRedundancies(reading);
        entry.Readings.Add(reading);
    }

    private async Task ReadChildElementAsync(Reading reading)
    {
        switch (_xmlReader.Name)
        {
            case Reading.Text_XmlTagName:
                await ReadReadingText(reading);
                break;
            case Reading.NoKanji_XmlTagName:
                reading.NoKanji = true;
                break;
            case Restriction.XmlTagName:
                await _restrictionReader.ReadAsync(reading);
                break;
            case ReadingInfo.XmlTagName:
                await _infoReader.ReadAsync(reading);
                break;
            case ReadingPriority.XmlTagName:
                await _priorityReader.ReadAsync(reading);
                break;
            default:
                Log.UnexpectedChildElement(_logger, _xmlReader.Name, Reading.XmlTagName);
                reading.Entry.IsCorrupt = true;
                break;
        }
    }

    private async Task ReadReadingText(Reading reading)
    {
        reading.Text = await _xmlReader.ReadElementContentAsStringAsync();
        if (string.IsNullOrWhiteSpace(reading.Text))
        {
            LogEmptyTextForm(reading.Entry.Id, reading.Order);
            reading.Entry.IsCorrupt = true;
        }
    }

    private void CheckForRedundancies(Reading reading)
    {
        var count = 0;
        if (reading.Restrictions.Count > 0) count++;
        if (reading.IsHidden()) count++;
        if (reading.NoKanji) count++;

        if (count > 1)
        {
            LogRedundantReadingConstraints(reading.Entry.Id, reading.Order);
            reading.Entry.IsCorrupt = true;
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` reading #{Order} contains no text")]
    private partial void LogEmptyTextForm(int entryId, int order);

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` reading #{Order} has redundant constraint / NoKanji / hidden information")]
    private partial void LogRedundantReadingConstraints(int entryId, int order);

}
