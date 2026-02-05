/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms of
the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.Kanjidic2.Import.Models;

namespace Jitendex.Kanjidic2.Import.Parsing.GroupReaders;

internal partial class ReadingMeaningGroupReader : BaseReader<ReadingMeaningGroupReader>
{
    private readonly ReadingMeaningReader _readingMeaningReader;

    public ReadingMeaningGroupReader(ILogger<ReadingMeaningGroupReader> logger, XmlReader xmlReader, ReadingMeaningReader readingMeaningReader)
        : base(logger, xmlReader)
    {
        _readingMeaningReader = readingMeaningReader;
    }

    public async Task ReadAsync(Document document, EntryElement entry)
    {
        var group = new ReadingMeaningGroupElement
        (
            EntryId: entry.Id,
            Order: document.ReadingMeaningGroups.NextOrder(entry.Id)
        );

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(document, entry, group);
                    break;
                case XmlNodeType.Text:
                    await LogUnexpectedTextNodeAsync(entry.Id, XmlTagName.ReadingMeaningGroup);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == XmlTagName.ReadingMeaningGroup;
                    break;
            }
        }

        document.ReadingMeaningGroups.Add(group.Key(), group);
    }

    private async Task ReadChildElementAsync(Document document, EntryElement entry, ReadingMeaningGroupElement group)
    {
        switch (_xmlReader.Name)
        {
            case XmlTagName.ReadingMeaning:
                await _readingMeaningReader.ReadAsync(document, entry, group);
                break;
            case XmlTagName.Nanori:
                await ReadNanori(document, entry, group);
                break;
            default:
                LogUnexpectedChildElement(entry.ToRune(), _xmlReader.Name, XmlTagName.ReadingMeaningGroup);
                break;
        }
    }

    private async Task ReadNanori(Document document, EntryElement entry, ReadingMeaningGroupElement group)
    {
        var nanori = new NanoriElement
        (
            EntryId: entry.Id,
            GroupOrder: group.Order,
            Order: document.Nanoris.NextOrder(group.Key()),
            Text: await _xmlReader.ReadElementContentAsStringAsync()
        );
        document.Nanoris.Add(nanori.Key(), nanori);
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry for character `{Character}` has more than one <{XmlTagName}> child element.")]
    partial void LogUnexpectedGroup(Rune character, string xmlTagName);
}
