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

using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.Kanjidic2.Import.Models;
using Jitendex.Kanjidic2.Import.Models.Groups;
using Jitendex.Kanjidic2.Import.Models.GroupElements;

namespace Jitendex.Kanjidic2.Import.Parsing.GroupReaders;

internal partial class DictionaryGroupReader
{
    private readonly ILogger<DictionaryGroupReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly Dictionary<int, int> _usedDictionaryGroupOrders = [];
    private readonly Dictionary<(int, int), int> _usedDictionaryOrders = [];

    public DictionaryGroupReader(ILogger<DictionaryGroupReader> logger, XmlReader xmlReader) =>
        (_logger, _xmlReader) =
        (@logger, @xmlReader);

    public async Task ReadAsync(Document document, Entry entry)
    {
        var group = new DictionaryGroup
        {
            UnicodeScalarValue = entry.UnicodeScalarValue,
            Order = _usedDictionaryGroupOrders.TryGetValue(entry.UnicodeScalarValue, out var order) ? order + 1 : 0,
        };

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
                    Log.UnexpectedTextNode(_logger, entry.ToRune(), DictionaryGroup.XmlTagName, text);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == DictionaryGroup.XmlTagName;
                    break;
            }
        }

        document.DictionaryGroups.Add(group.Key(), group);
    }

    private async Task ReadChildElementAsync(Document document, Entry entry, DictionaryGroup group)
    {
        switch (_xmlReader.Name)
        {
            case Dictionary.XmlTagName:
                await ReadDictionary(document, entry, group);
                break;
            default:
                Log.UnexpectedChildElement(_logger, entry.ToRune(), _xmlReader.Name, DictionaryGroup.XmlTagName);
                break;
        }
    }

    private async Task ReadDictionary(Document document, Entry entry, DictionaryGroup group)
    {
        var dictionary = new Dictionary
        {
            UnicodeScalarValue = group.UnicodeScalarValue,
            GroupOrder = group.Order,
            Order = _usedDictionaryOrders.TryGetValue(group.Key(), out var order) ? order + 1 : 0,
            TypeName = GetTypeName(document, entry),
            Volume = GetDictionaryVolume(entry),
            Page = GetDictionaryPage(entry),
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
        };

        _usedDictionaryOrders[group.Key()] = dictionary.Order;

        document.Dictionaries.Add(dictionary.Key(), dictionary);
    }

    private string GetTypeName(Document document, Entry entry)
    {
        string typeName;
        var attribute = _xmlReader.GetAttribute("dr_type");
        if (string.IsNullOrWhiteSpace(attribute))
        {
            LogMissingTypeName(entry.ToRune());
            typeName = string.Empty;
        }
        else
        {
            typeName = attribute;
        }
        if (!document.DictionaryTypes.ContainsKey(typeName))
        {
            var type = new DictionaryType
            {
                Name = typeName,
                CreatedDate = document.FileHeader.DateOfCreation,
            };
            document.DictionaryTypes.Add(typeName, type);
        }
        return typeName;
    }

    private int? GetDictionaryVolume(Entry entry)
    {
        var volume = _xmlReader.GetAttribute("m_vol");
        if (volume is null)
        {
            // Not an error; allowed to be null
            return null;
        }
        if (int.TryParse(volume, out int value))
        {
            return value;
        }
        else
        {
            LogNonNumericVolume(entry.ToRune(), volume);
            return null;
        }
    }

    private int? GetDictionaryPage(Entry entry)
    {
        var page = _xmlReader.GetAttribute("m_page");
        if (page is null)
        {
            // Not an error; allowed to be null
            return null;
        }
        if (int.TryParse(page, out int value))
        {
            return value;
        }
        else
        {
            LogNonNumericPage(entry.ToRune(), page);
            return null;
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` is missing a dictionary type attribute")]
    partial void LogMissingTypeName(Rune character);

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` has a dictionary volume attribute that is non-numeric: `{Volume}`")]
    partial void LogNonNumericVolume(Rune character, string volume);

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` has a dictionary page attribute that is non-numeric: `{Page}`")]
    partial void LogNonNumericPage(Rune character, string page);
}
