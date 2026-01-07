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

using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.Kanjidic2.Models;
using Jitendex.Kanjidic2.Models.Groups;
using Jitendex.Kanjidic2.Models.EntryElements;

namespace Jitendex.Kanjidic2.Readers.GroupReaders;

internal partial class ReadingMeaningGroupReader
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
            UnicodeScalarValue = entry.UnicodeScalarValue,
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
                    Log.UnexpectedTextNode(_logger, entry.ToRune(), ReadingMeaningGroup.XmlTagName, text);
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
                {
                    group.Entry.IsCorrupt = true;
                    LogUnexpectedGroup(group.Entry.ToRune(), ReadingMeaning.XmlTagName);
                }
                group.ReadingMeaning = await _readingMeaningReader.ReadAsync(group);
                break;
            case Nanori.XmlTagName:
                await ReadNanori(group);
                break;
            default:
                Log.UnexpectedChildElement(_logger, group.Entry.ToRune(), _xmlReader.Name, ReadingMeaningGroup.XmlTagName);
                group.Entry.IsCorrupt = true;
                break;
        }
    }

    private async Task ReadNanori(ReadingMeaningGroup group)
    {
        var nanori = new Nanori
        {
            UnicodeScalarValue = group.UnicodeScalarValue,
            Order = group.Nanoris.Count + 1,
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
            Entry = group.Entry,
        };

        if (group.Nanoris.Any(n => n.Text == nanori.Text))
        {
            Log.Duplicate(_logger, group.Entry.ToRune(), ReadingMeaningGroup.XmlTagName, nanori.Text, Nanori.XmlTagName);
            group.Entry.IsCorrupt = true;
        }

        group.Nanoris.Add(nanori);
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry for character `{Character}` has more than one <{XmlTagName}> child element.")]
    partial void LogUnexpectedGroup(Rune character, string xmlTagName);
}
