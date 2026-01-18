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
using Jitendex.Kanjidic2.Import.Models.SubgroupElements;

namespace Jitendex.Kanjidic2.Import.Parsing.GroupReaders;

internal partial class ReadingMeaningReader
{
    private readonly ILogger<ReadingMeaningReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly Dictionary<(int, int), int> _usedReadingMeaningOrders = [];
    private readonly Dictionary<(int, int, int), int> _usedReadingOrders = [];
    private readonly Dictionary<(int, int, int), int> _usedMeaningOrders = [];

    public ReadingMeaningReader(ILogger<ReadingMeaningReader> logger, XmlReader xmlReader) =>
        (_logger, _xmlReader) =
        (@logger, @xmlReader);

    public async Task ReadAsync(Document document, Entry entry, ReadingMeaningGroup group)
    {
        var readingMeaning = new ReadingMeaning
        {
            UnicodeScalarValue = entry.UnicodeScalarValue,
            GroupOrder = group.Order,
            Order = _usedReadingMeaningOrders.TryGetValue(group.Key(), out var order) ? order + 1 : 0,
        };

        _usedReadingMeaningOrders[group.Key()] = group.Order;

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(document, entry, group, readingMeaning);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    Log.UnexpectedTextNode(_logger, entry.ToRune(), ReadingMeaning.XmlTagName, text);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == ReadingMeaning.XmlTagName;
                    break;
            }
        }

        document.ReadingMeanings.Add(readingMeaning.Key(), readingMeaning);
    }

    private async Task ReadChildElementAsync(Document document, Entry entry, ReadingMeaningGroup group, ReadingMeaning readingMeaning)
    {
        switch (_xmlReader.Name)
        {
            case Reading.XmlTagName:
                await ReadReading(document, entry, group, readingMeaning);
                break;
            case Meaning.XmlTagName:
                await ReadMeaning(document, entry, group, readingMeaning);
                break;
            default:
                Log.UnexpectedChildElement(_logger, entry.ToRune(), _xmlReader.Name, ReadingMeaning.XmlTagName);
                break;
        }
    }

    private async Task ReadReading(Document document, Entry entry, ReadingMeaningGroup group, ReadingMeaning readingMeaning)
    {
        var reading = new Reading
        {
            UnicodeScalarValue = readingMeaning.UnicodeScalarValue,
            GroupOrder = group.Order,
            ReadingMeaningOrder = readingMeaning.Order,
            Order = _usedReadingOrders.TryGetValue(readingMeaning.Key(), out var order) ? order + 1 : 0,
            TypeName = GetReadingTypeName(document, entry),
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
        };
        _usedReadingOrders[readingMeaning.Key()] = reading.Order;
        document.Readings.Add(reading.Key(), reading);
    }

    private string GetReadingTypeName(Document document, Entry entry)
    {
        string typeName;
        var attribute = _xmlReader.GetAttribute("r_type");
        if (string.IsNullOrWhiteSpace(attribute))
        {
            LogMissingTypeName(entry.ToRune());
            typeName = string.Empty;
        }
        else
        {
            typeName = attribute;
        }
        if (!document.ReadingTypes.ContainsKey(typeName))
        {
            var type = new ReadingType
            {
                Name = typeName,
                CreatedDate = document.Header.DateOfCreation,
            };
            document.ReadingTypes.Add(typeName, type);
        }
        return typeName;
    }

    private async Task ReadMeaning(Document document, Entry entry, ReadingMeaningGroup group, ReadingMeaning readingMeaning)
    {
        var language = _xmlReader.GetAttribute("m_lang") ?? "en";
        var meaning = new Meaning
        {
            UnicodeScalarValue = entry.UnicodeScalarValue,
            GroupOrder = group.Order,
            ReadingMeaningOrder = readingMeaning.Order,
            Order = _usedMeaningOrders.TryGetValue(readingMeaning.Key(), out var order) ? order + 1 : 0,
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
        };

        if (language != "en")
        {
            return;
        }
        if (meaning.Text == "(kokuji)")
        {
            readingMeaning.IsKokuji = true;
            return;
        }
        if (meaning.Text == "(ghost kanji)")
        {
            readingMeaning.IsGhost = true;
            return;
        }

        _usedMeaningOrders[readingMeaning.Key()] = meaning.Order;
        document.Meanings.Add(meaning.Key(), meaning);
    }

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` is missing a reading type attribute")]
    partial void LogMissingTypeName(Rune character);
}
