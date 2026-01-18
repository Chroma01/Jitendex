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

internal partial class CodepointGroupReader
{
    private readonly ILogger<CodepointGroupReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly Dictionary<int, int> _usedGroupOrders = [];
    private readonly Dictionary<(int, int), int> _usedOrders = [];

    public CodepointGroupReader(ILogger<CodepointGroupReader> logger, XmlReader xmlReader) =>
        (_logger, _xmlReader) =
        (@logger, @xmlReader);

    public async Task ReadAsync(Document document, Entry entry)
    {
        var group = new CodepointGroup
        {
            UnicodeScalarValue = entry.UnicodeScalarValue,
            Order = _usedGroupOrders.TryGetValue(entry.UnicodeScalarValue, out var order) ? order + 1 : 0,
        };

        _usedGroupOrders[entry.UnicodeScalarValue] = group.Order;
        document.CodepointGroups.Add(group.Key(), group);

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
                    Log.UnexpectedTextNode(_logger, entry.ToRune(), CodepointGroup.XmlTagName, text);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == CodepointGroup.XmlTagName;
                    break;
            }
        }
    }

    private async Task ReadChildElementAsync(Document document, Entry entry, CodepointGroup group)
    {
        switch (_xmlReader.Name)
        {
            case Codepoint.XmlTagName:
                await ReadCodepoint(document, entry, group);
                break;
            default:
                Log.UnexpectedChildElement(_logger, entry.ToRune(), _xmlReader.Name, CodepointGroup.XmlTagName);
                break;
        }
    }

    private async Task ReadCodepoint(Document document, Entry entry, CodepointGroup group)
    {
        var codepoint = new Codepoint
        {
            UnicodeScalarValue = entry.UnicodeScalarValue,
            GroupOrder = group.Order,
            Order = _usedOrders.TryGetValue(group.Key(), out var order) ? order + 1 : 0,
            TypeName = GetTypeName(document, entry),
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
        };
        _usedOrders[group.Key()] = codepoint.Order;
        document.Codepoints.Add(codepoint.Key(), codepoint);
    }

    private string GetTypeName(Document document, Entry entry)
    {
        string typeName;
        var attribute = _xmlReader.GetAttribute(Codepoint.TypeName_XmlAttrName);
        if (string.IsNullOrWhiteSpace(attribute))
        {
            LogMissingTypeName(entry.ToRune());
            typeName = string.Empty;
        }
        else
        {
            typeName = attribute;
        }
        if (!document.CodepointTypes.ContainsKey(typeName))
        {
            var type = new CodepointType
            {
                Name = typeName,
                CreatedDate = document.Header.DateOfCreation,
            };
            document.CodepointTypes.Add(typeName, type);
        }
        return typeName;
    }

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` is missing a codepoint type attribute")]
    partial void LogMissingTypeName(Rune character);
}
