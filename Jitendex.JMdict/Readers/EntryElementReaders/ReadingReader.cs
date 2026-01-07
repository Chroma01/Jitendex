/*
Copyright (c) 2025 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

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
using Jitendex.JMdict.Models;
using Jitendex.JMdict.Models.EntryElements;
using Jitendex.JMdict.Models.EntryElements.ReadingElements;
using Jitendex.JMdict.Readers.EntryElementReaders.ReadingElementReaders;

namespace Jitendex.JMdict.Readers.EntryElementReaders;

internal partial class ReadingReader
{
    private readonly ILogger<ReadingReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly RestrictionReader _restrictionReader;
    private readonly RInfoReader _infoReader;
    private readonly RPriorityReader _priorityReader;

    public ReadingReader(ILogger<ReadingReader> logger, XmlReader xmlReader, RestrictionReader restrictionReader, RInfoReader infoReader, RPriorityReader priorityReader) =>
        (_logger, _xmlReader, _restrictionReader, _infoReader, _priorityReader) =
        (@logger, @xmlReader, @restrictionReader, @infoReader, @priorityReader);

    public async Task ReadAsync(Entry entry)
    {
        var reading = new Reading
        {
            EntryId = entry.Id,
            Order = entry.Readings.Count + 1,
            Text = null!,
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

        if (reading.Text is not null)
        {
            CheckForRedundancies(reading);
            entry.Readings.Add(reading);
        }
        else
        {
            LogMissingElement(reading.EntryId, reading.Order, Reading.Text_XmlTagName);
            entry.IsCorrupt = true;
        }
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
        if (reading.Text is not null)
        {
            LogMultipleElements(reading.EntryId, reading.Order, Reading.Text_XmlTagName);
            reading.Entry.IsCorrupt = true;
        }

        reading.Text = await _xmlReader.ReadElementContentAsStringAsync();

        if (string.IsNullOrWhiteSpace(reading.Text))
        {
            LogEmptyTextForm(reading.Entry.Id, reading.Order);
            reading.Entry.IsCorrupt = true;
        }
    }

    private void CheckForRedundancies(Reading reading)
    {
        int count = 0;
        if (reading.Restrictions.Count > 0) count++;
        if (reading.IsHidden()) count++;
        if (reading.NoKanji) count++;

        if (count > 1)
        {
            LogRedundantReadingConstraints(reading.Entry.Id, reading.Order);
            reading.Entry.IsCorrupt = true;
        }
    }

    [LoggerMessage(LogLevel.Error,
    "Entry `{EntryId}` reading #{Order} does not contain a <{XmlTagName}> element")]
    partial void LogMissingElement(int entryId, int order, string xmlTagName);

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` reading #{Order} contains no text")]
    partial void LogEmptyTextForm(int entryId, int order);

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` reading #{Order} has redundant constraint / NoKanji / hidden information")]
    partial void LogRedundantReadingConstraints(int entryId, int order);

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` reading #{Order} contains multiple <{XmlTagName}> elements")]
    partial void LogMultipleElements(int entryId, int order, string xmlTagName);
}
