/*
Copyright (c) 2025-2026 Stephen Kraus
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
using Jitendex.JMdict.Import.Models;
using Jitendex.JMdict.Import.Models.EntryElements;
using Jitendex.JMdict.Import.Models.EntryElements.ReadingElements;
using Jitendex.JMdict.Import.Parsing.EntryElementReaders.ReadingElementReaders;

namespace Jitendex.JMdict.Import.Parsing.EntryElementReaders;

internal partial class ReadingReader : BaseReader<ReadingReader>
{
    private readonly RestrictionReader _restrictionReader;
    private readonly RInfoReader _infoReader;
    private readonly RPriorityReader _priorityReader;

    public ReadingReader(ILogger<ReadingReader> logger, XmlReader xmlReader, RestrictionReader restrictionReader, RInfoReader infoReader, RPriorityReader priorityReader) : base(logger, xmlReader)
    {
        _restrictionReader = restrictionReader;
        _infoReader = infoReader;
        _priorityReader = priorityReader;
    }

    public async Task ReadAsync(Document document, EntryElement entry)
    {
        var reading = new ReadingElement
        {
            EntryId = entry.Id,
            Order = document.Readings.NextOrder(entry.Id),
            Text = null!,
            NoKanji = false,
        };

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(document, reading);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    LogUnexpectedTextNode(ReadingElement.XmlTagName, text);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == ReadingElement.XmlTagName;
                    break;
            }
        }

        if (reading.Text is not null)
        {
            document.Readings.Add(reading.Key(), reading);
        }
        else
        {
            LogMissingElement(reading.EntryId, reading.Order, ReadingElement.Text_XmlTagName);
        }
    }

    private async Task ReadChildElementAsync(Document document, ReadingElement reading)
    {
        switch (_xmlReader.Name)
        {
            case ReadingElement.Text_XmlTagName:
                await ReadReadingText(reading);
                break;
            case ReadingElement.NoKanji_XmlTagName:
                reading.NoKanji = true;
                break;
            case RestrictionElement.XmlTagName:
                await _restrictionReader.ReadAsync(document, reading);
                break;
            case ReadingInfoElement.XmlTagName:
                await _infoReader.ReadAsync(document, reading);
                break;
            case ReadingPriorityElement.XmlTagName:
                await _priorityReader.ReadAsync(document, reading);
                break;
            default:
                LogUnexpectedChildElement(_xmlReader.Name, ReadingElement.XmlTagName);
                break;
        }
    }

    private async Task ReadReadingText(ReadingElement reading)
    {
        if (reading.Text is not null)
        {
            LogMultipleElements(reading.EntryId, reading.Order, ReadingElement.Text_XmlTagName);
        }

        reading.Text = await _xmlReader.ReadElementContentAsStringAsync();

        if (string.IsNullOrWhiteSpace(reading.Text))
        {
            LogEmptyTextForm(reading.EntryId, reading.Order);
        }
    }

    [LoggerMessage(LogLevel.Error,
    "Entry `{EntryId}` reading #{Order} does not contain a <{XmlTagName}> element")]
    partial void LogMissingElement(int entryId, int order, string xmlTagName);

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` reading #{Order} contains no text")]
    partial void LogEmptyTextForm(int entryId, int order);

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` reading #{Order} contains multiple <{XmlTagName}> elements")]
    partial void LogMultipleElements(int entryId, int order, string xmlTagName);
}
