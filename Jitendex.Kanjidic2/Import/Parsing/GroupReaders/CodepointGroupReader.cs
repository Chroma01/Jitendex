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
using Jitendex.Kanjidic2.Import.Models.Groups;
using Jitendex.Kanjidic2.Import.Models.GroupElements;

namespace Jitendex.Kanjidic2.Import.Parsing.GroupReaders;

internal partial class CodepointGroupReader : BaseReader<CodepointGroupReader>
{
    public CodepointGroupReader(ILogger<CodepointGroupReader> logger, XmlReader xmlReader) : base(logger, xmlReader) { }

    public async Task ReadAsync(Document document, EntryElement entry)
    {
        var group = new CodepointGroupElement
        {
            EntryId = entry.Id,
            Order = document.CodepointGroups.NextOrder(entry.Id),
        };

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
                    await LogUnexpectedTextNodeAsync(entry.Id, CodepointGroupElement.XmlTagName);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == CodepointGroupElement.XmlTagName;
                    break;
            }
        }
    }

    private async Task ReadChildElementAsync(Document document, EntryElement entry, CodepointGroupElement group)
    {
        switch (_xmlReader.Name)
        {
            case CodepointElement.XmlTagName:
                await ReadCodepoint(document, entry, group);
                break;
            default:
                LogUnexpectedChildElement(entry.ToRune(), _xmlReader.Name, CodepointGroupElement.XmlTagName);
                break;
        }
    }

    private async Task ReadCodepoint(Document document, EntryElement entry, CodepointGroupElement group)
    {
        var codepoint = new CodepointElement
        {
            EntryId = entry.Id,
            GroupOrder = group.Order,
            Order = document.Codepoints.NextOrder(group.Key()),
            TypeName = GetTypeName(document, entry),
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
        };
        document.Codepoints.Add(codepoint.Key(), codepoint);
    }

    private string GetTypeName(Document document, EntryElement entry)
    {
        string typeName;
        var attribute = _xmlReader.GetAttribute(CodepointElement.TypeName_XmlAttrName);

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
            var type = new CodepointTypeElement(typeName, document.Header.Date);
            document.CodepointTypes.Add(typeName, type);
        }

        return typeName;
    }

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` is missing a codepoint type attribute")]
    partial void LogMissingTypeName(Rune character);
}
