/*
Copyright (c) 2025 Stephen Kraus

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

using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.Import.Kanjidic2.Models;
using Jitendex.Import.Kanjidic2.Models.Groups;
using Jitendex.Import.Kanjidic2.Models.EntryElements;

namespace Jitendex.Import.Kanjidic2.Readers.GroupReaders;

internal partial class CodepointGroupReader
{
    private readonly ILogger<CodepointGroupReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly DocumentTypes _docTypes;

    public CodepointGroupReader(ILogger<CodepointGroupReader> logger, XmlReader xmlReader, DocumentTypes docTypes) =>
        (_logger, _xmlReader, _docTypes) =
        (@logger, @xmlReader, @docTypes);

    public async Task<CodepointGroup> ReadAsync(Entry entry)
    {
        var group = new CodepointGroup
        {
            Character = entry.Character,
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
                    Log.UnexpectedTextNode(_logger, entry.Character, CodepointGroup.XmlTagName, text);
                    entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == CodepointGroup.XmlTagName;
                    break;
            }
        }
        return group;
    }

    private async Task ReadChildElementAsync(CodepointGroup group)
    {
        switch (_xmlReader.Name)
        {
            case Codepoint.XmlTagName:
                await ReadCodepoint(group);
                break;
            default:
                Log.UnexpectedChildElement(_logger, group.Entry.Character, _xmlReader.Name, CodepointGroup.XmlTagName);
                group.Entry.IsCorrupt = true;
                break;
        }
    }

    private async Task ReadCodepoint(CodepointGroup group)
    {
        var typeName = GetTypeName(group);
        var type = _docTypes.GetByName<CodepointType>(typeName);

        if (group.Codepoints.Any(c => c.TypeName == type.Name))
        {
            Log.Duplicate(_logger, group.Character, CodepointGroup.XmlTagName, type.Name, Codepoint.XmlTagName);
            group.Entry.IsCorrupt = true;
        }

        var codepoint = new Codepoint
        {
            Character = group.Character,
            Order = group.Codepoints.Count + 1,
            TypeName = type.Name,
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
            Entry = group.Entry,
            Type = type,
        };

        group.Codepoints.Add(codepoint);
    }

    private string? GetTypeName(CodepointGroup group)
    {
        var typeName = _xmlReader.GetAttribute("cp_type");
        if (string.IsNullOrWhiteSpace(typeName))
        {
            LogMissingTypeName(group.Character);
            group.Entry.IsCorrupt = true;
        }
        return typeName;
    }

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` is missing a codepoint type attribute")]
    private partial void LogMissingTypeName(string character);
}
