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
using Jitendex.Warehouse.Kanjidic2.Models.EntryElements;

namespace Jitendex.Warehouse.Kanjidic2.Models.Groups;

internal static class CodepointGroupReader
{
    public async static Task<CodepointGroup> ReadCodepointGroupAsync(this XmlReader reader, Entry entry)
    {
        var codepointGroup = new CodepointGroup
        {
            Character = entry.Character,
            Entry = entry,
        };

        var exit = false;
        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    await reader.ReadChildElementAsync(codepointGroup);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{CodepointGroup.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == CodepointGroup.XmlTagName;
                    break;
            }
        }
        return codepointGroup;
    }

    private async static Task ReadChildElementAsync(this XmlReader reader, CodepointGroup group)
    {
        switch (reader.Name)
        {
            case Codepoint.XmlTagName:
                group.Codepoints.Add(new Codepoint
                {
                    Character = group.Character,
                    Order = group.Codepoints.Count + 1,
                    Type = reader.GetAttribute("cp_type") ?? throw new Exception($"Character `{group.Character}` missing codepoint type"),
                    Text = await reader.ReadElementContentAsStringAsync(),
                    Entry = group.Entry,
                });
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{CodepointGroup.XmlTagName}`");
        }
    }
}
