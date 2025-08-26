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

internal static class MiscGroupReader
{
    public async static Task<MiscGroup> ReadMiscGroupAsync(this XmlReader reader, Entry entry)
    {
        var miscGroup = new MiscGroup
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
                    await reader.ReadChildElementAsync(miscGroup);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{MiscGroup.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == MiscGroup.XmlTagName;
                    break;
            }
        }
        return miscGroup;
    }

    private async static Task ReadChildElementAsync(this XmlReader reader, MiscGroup group)
    {
        switch (reader.Name)
        {
            case "grade":
                group.Grade = int.Parse(await reader.ReadElementContentAsStringAsync());
                break;
            case "freq":
                group.Frequency = int.Parse(await reader.ReadElementContentAsStringAsync());
                break;
            case "jlpt":
                group.JlptLevel = int.Parse(await reader.ReadElementContentAsStringAsync());
                break;
            case StrokeCount.XmlTagName:
                group.StrokeCounts.Add(new StrokeCount
                {
                    Character = group.Character,
                    Order = group.StrokeCounts.Count + 1,
                    Value = int.Parse(await reader.ReadElementContentAsStringAsync()),
                    Entry = group.Entry,
                });
                break;
            case Variant.XmlTagName:
                group.Variants.Add(new Variant
                {
                    Character = group.Character,
                    Order = group.Variants.Count + 1,
                    Type = reader.GetAttribute("var_type") ?? throw new Exception($"Character `{group.Character}` missing variant type"),
                    Text = await reader.ReadElementContentAsStringAsync(),
                    Entry = group.Entry,
                });
                break;
            case RadicalName.XmlTagName:
                group.RadicalNames.Add(new RadicalName
                {
                    Character = group.Character,
                    Order = group.RadicalNames.Count + 1,
                    Text = await reader.ReadElementContentAsStringAsync(),
                    Entry = group.Entry,
                });
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{MiscGroup.XmlTagName}`");
        }
    }
}
