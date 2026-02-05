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

internal partial class RadicalGroupReader : BaseReader<RadicalGroupReader>
{
    public RadicalGroupReader(ILogger<RadicalGroupReader> logger, XmlReader xmlReader) : base(logger, xmlReader) { }

    public async Task ReadAsync(Document document, EntryElement entry)
    {
        var group = new RadicalGroupElement
        (
            EntryId: entry.Id,
            Order: document.RadicalGroups.NextOrder(entry.Id)
        );

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(document, group);
                    break;
                case XmlNodeType.Text:
                    await LogUnexpectedTextNodeAsync(entry.Id, XmlTagName.RadicalGroup);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == XmlTagName.RadicalGroup;
                    break;
            }
        }

        document.RadicalGroups.Add(group.Key(), group);
    }

    private async Task ReadChildElementAsync(Document document, RadicalGroupElement group)
    {
        switch (_xmlReader.Name)
        {
            case XmlTagName.Radical:
                await ReadRadical(document, group);
                break;
            default:
                LogUnexpectedChildElement(group.ToRune(), _xmlReader.Name, XmlTagName.RadicalGroup);
                break;
        }
    }

    private async Task ReadRadical(Document document, RadicalGroupElement group)
    {
        var radical = new RadicalElement
        (
            EntryId: group.EntryId,
            GroupOrder: group.Order,
            Order: document.Radicals.NextOrder(group.Key()),
            TypeName: GetTypeName(document, group),
            Number: await GetNumber(group)
        );
        document.Radicals.Add(radical.Key(), radical);
    }

    private string GetTypeName(Document document, RadicalGroupElement group)
    {
        string typeName;
        var attribute = _xmlReader.GetAttribute("rad_type");
        if (string.IsNullOrWhiteSpace(attribute))
        {
            LogMissingTypeName(group.ToRune());
            typeName = string.Empty;
        }
        else
        {
            typeName = attribute;
        }
        if (!document.RadicalTypes.ContainsKey(typeName))
        {
            var type = new RadicalTypeElement(typeName, document.Header.Date);
            document.RadicalTypes.Add(typeName, type);
        }
        return typeName;
    }

    private async Task<int> GetNumber(RadicalGroupElement group)
    {
        var text = await _xmlReader.ReadElementContentAsStringAsync();
        if (int.TryParse(text, out int value))
        {
            return value;
        }
        else
        {
            LogNonNumericRadicalNumber(group.ToRune(), text);
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
