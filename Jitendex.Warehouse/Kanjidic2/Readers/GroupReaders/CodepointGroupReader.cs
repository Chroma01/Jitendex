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
using Jitendex.Warehouse.Kanjidic2.Models;
using Jitendex.Warehouse.Kanjidic2.Models.Groups;
using Jitendex.Warehouse.Kanjidic2.Models.EntryElements;

namespace Jitendex.Warehouse.Kanjidic2.Readers.GroupReaders;

internal class CodepointGroupReader
{
    private readonly ILogger<CodepointGroupReader> _logger;
    private readonly XmlReader _xmlReader;

    public CodepointGroupReader(ILogger<CodepointGroupReader> logger, XmlReader xmlReader) =>
        (_logger, _xmlReader) =
        (@logger, @xmlReader);

    public async Task<CodepointGroup> ReadAsync(Entry entry)
    {
        var codepointGroup = new CodepointGroup
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
                    await ReadChildElementAsync(codepointGroup);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    Log.UnexpectedTextNode(_logger, CodepointGroup.XmlTagName, text);
                    entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == CodepointGroup.XmlTagName;
                    break;
            }
        }
        return codepointGroup;
    }

    private async Task ReadChildElementAsync(CodepointGroup group)
    {
        switch (_xmlReader.Name)
        {
            case Codepoint.XmlTagName:
                group.Codepoints.Add(new Codepoint
                {
                    Character = group.Character,
                    Order = group.Codepoints.Count + 1,
                    Type = _xmlReader.GetAttribute("cp_type") ?? throw new Exception($"Character `{group.Character}` missing codepoint type"),
                    Text = await _xmlReader.ReadElementContentAsStringAsync(),
                    Entry = group.Entry,
                });
                break;
            default:
                Log.UnexpectedChildElement(_logger, _xmlReader.Name, CodepointGroup.XmlTagName);
                group.Entry.IsCorrupt = true;
                break;
        }
    }
}
