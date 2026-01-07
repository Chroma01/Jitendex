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

internal partial class RadicalGroupReader
{
    private readonly ILogger<RadicalGroupReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly DocumentTypes _docTypes;

    public RadicalGroupReader(ILogger<RadicalGroupReader> logger, XmlReader xmlReader, DocumentTypes docTypes) =>
        (_logger, _xmlReader, _docTypes) =
        (@logger, @xmlReader, @docTypes);

    public async Task<RadicalGroup> ReadAsync(Entry entry)
    {
        var group = new RadicalGroup
        {
            UnicodeScalarValue = entry.UnicodeScalarValue,
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
                    Log.UnexpectedTextNode(_logger, entry.ToRune(), RadicalGroup.XmlTagName, text);
                    entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == RadicalGroup.XmlTagName;
                    break;
            }
        }
        return group;
    }

    private async Task ReadChildElementAsync(RadicalGroup group)
    {
        switch (_xmlReader.Name)
        {
            case Radical.XmlTagName:
                await ReadRadical(group);
                break;
            default:
                Log.UnexpectedChildElement(_logger, group.Entry.ToRune(), _xmlReader.Name, RadicalGroup.XmlTagName);
                group.Entry.IsCorrupt = true;
                break;
        }
    }

    private async Task ReadRadical(RadicalGroup group)
    {
        var typeName = GetTypeName(group);
        var type = _docTypes.GetByName<RadicalType>(typeName);

        if (group.Radicals.Any(c => c.TypeName == type.Name))
        {
            Log.Duplicate(_logger, group.Entry.ToRune(), RadicalGroup.XmlTagName, type.Name, Radical.XmlTagName);
            group.Entry.IsCorrupt = true;
        }

        var number = await GetNumber(group);

        var radical = new Radical
        {
            UnicodeScalarValue = group.UnicodeScalarValue,
            Order = group.Radicals.Count + 1,
            TypeName = type.Name,
            Number = number,
            Entry = group.Entry,
            Type = type,
        };

        group.Radicals.Add(radical);
    }

    private string? GetTypeName(RadicalGroup group)
    {
        var typeName = _xmlReader.GetAttribute("rad_type");
        if (string.IsNullOrWhiteSpace(typeName))
        {
            LogMissingTypeName(group.Entry.ToRune());
            group.Entry.IsCorrupt = true;
        }
        return typeName;
    }

    private async Task<int> GetNumber(RadicalGroup group)
    {
        var text = await _xmlReader.ReadElementContentAsStringAsync();
        if (int.TryParse(text, out int value))
        {
            return value;
        }
        else
        {
            LogNonNumericRadicalNumber(group.Entry.ToRune(), text);
            group.Entry.IsCorrupt = true;
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
