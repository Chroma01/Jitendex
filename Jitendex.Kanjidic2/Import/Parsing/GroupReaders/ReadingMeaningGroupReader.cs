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
using Jitendex.Kanjidic2.Import.Models;
using Jitendex.Kanjidic2.Import.Models.Groups;
using Jitendex.Kanjidic2.Import.Models.GroupElements;

namespace Jitendex.Kanjidic2.Import.Parsing.GroupReaders;

internal partial class ReadingMeaningGroupReader
{
    private readonly ILogger<ReadingMeaningGroupReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly ReadingMeaningReader _readingMeaningReader;
    private readonly Dictionary<int, int> _usedGroupOrders = [];
    private readonly Dictionary<(int, int), int> _usedOrders = [];

    public ReadingMeaningGroupReader(ILogger<ReadingMeaningGroupReader> logger, XmlReader xmlReader, ReadingMeaningReader readingMeaningReader) =>
        (_logger, _xmlReader, _readingMeaningReader) =
        (@logger, @xmlReader, @readingMeaningReader);

    public async Task ReadAsync(Document document, Entry entry)
    {
        var group = new ReadingMeaningGroup
        {
            UnicodeScalarValue = entry.UnicodeScalarValue,
            Order = _usedGroupOrders.TryGetValue(entry.UnicodeScalarValue, out var order) ? order + 1 : 0,
        };

        _usedGroupOrders[entry.UnicodeScalarValue] = group.Order;

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(document, entry, group);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    Log.UnexpectedTextNode(_logger, entry.ToRune(), ReadingMeaningGroup.XmlTagName, text);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == ReadingMeaningGroup.XmlTagName;
                    break;
            }
        }

        document.ReadingMeaningGroups.Add(group.Key(), group);
    }

    private async Task ReadChildElementAsync(Document document, Entry entry, ReadingMeaningGroup group)
    {
        switch (_xmlReader.Name)
        {
            case ReadingMeaning.XmlTagName:
                await _readingMeaningReader.ReadAsync(document, entry, group);
                break;
            case Nanori.XmlTagName:
                await ReadNanori(document, entry, group);
                break;
            default:
                Log.UnexpectedChildElement(_logger, entry.ToRune(), _xmlReader.Name, ReadingMeaningGroup.XmlTagName);
                break;
        }
    }

    private async Task ReadNanori(Document document, Entry entry, ReadingMeaningGroup group)
    {
        var nanori = new Nanori
        {
            UnicodeScalarValue = entry.UnicodeScalarValue,
            GroupOrder = group.Order,
            Order = _usedOrders.TryGetValue(group.Key(), out var order) ? order + 1 : 0,
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
        };
        _usedOrders[group.Key()] = nanori.Order;
        document.Nanoris.Add(nanori.Key(), nanori);
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry for character `{Character}` has more than one <{XmlTagName}> child element.")]
    partial void LogUnexpectedGroup(Rune character, string xmlTagName);
}
