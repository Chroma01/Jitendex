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

internal partial class RadicalGroupReader
{
    private readonly ILogger<RadicalGroupReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly Dictionary<int, int> _usedGroupOrders = [];
    private readonly Dictionary<(int, int), int> _usedOrders = [];

    public RadicalGroupReader(ILogger<RadicalGroupReader> logger, XmlReader xmlReader) =>
        (_logger, _xmlReader) =
        (@logger, @xmlReader);

    public async Task ReadAsync(Document document, Entry entry)
    {
        var group = new RadicalGroup
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
                    Log.UnexpectedTextNode(_logger, entry.ToRune(), RadicalGroup.XmlTagName, text);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == RadicalGroup.XmlTagName;
                    break;
            }
        }

        document.RadicalGroups.Add(group.Key(), group);
    }

    private async Task ReadChildElementAsync(Document document, Entry entry, RadicalGroup group)
    {
        switch (_xmlReader.Name)
        {
            case Radical.XmlTagName:
                await ReadRadical(document, entry, group);
                break;
            default:
                Log.UnexpectedChildElement(_logger, entry.ToRune(), _xmlReader.Name, RadicalGroup.XmlTagName);
                break;
        }
    }

    private async Task ReadRadical(Document document, Entry entry, RadicalGroup group)
    {
        var radical = new Radical
        {
            UnicodeScalarValue = group.UnicodeScalarValue,
            GroupOrder = group.Order,
            Order = _usedOrders.TryGetValue(group.Key(), out var order) ? order + 1 : 0,
            TypeName = GetTypeName(document, entry),
            Number = await GetNumber(entry),
        };
        _usedOrders[group.Key()] = radical.Order;
        document.Radicals.Add(radical.Key(), radical);
    }

    private string GetTypeName(Document document, Entry entry)
    {
        string typeName;
        var attribute = _xmlReader.GetAttribute("rad_type");
        if (string.IsNullOrWhiteSpace(attribute))
        {
            LogMissingTypeName(entry.ToRune());
            typeName = string.Empty;
        }
        else
        {
            typeName = attribute;
        }
        if (!document.RadicalTypes.ContainsKey(typeName))
        {
            var type = new RadicalType
            {
                Name = typeName,
                CreatedDate = document.FileHeader.DateOfCreation,
            };
            document.RadicalTypes.Add(typeName, type);
        }
        return typeName;
    }

    private async Task<int> GetNumber(Entry entry)
    {
        var text = await _xmlReader.ReadElementContentAsStringAsync();
        if (int.TryParse(text, out int value))
        {
            return value;
        }
        else
        {
            LogNonNumericRadicalNumber(entry.ToRune(), text);
            return default;
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` is missing a radical type attribute")]
    partial void LogMissingTypeName(Rune character);

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` has a radical number that is non-numeric: `{Text}`")]
    partial void LogNonNumericRadicalNumber(Rune character, string text);
}
