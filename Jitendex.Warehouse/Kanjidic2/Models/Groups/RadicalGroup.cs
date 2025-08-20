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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace Jitendex.Warehouse.Kanjidic2.Models.Groups;

[NotMapped]
internal class RadicalGroup
{
    [Key]
    public required string Character { get; set; }
    public List<Radical> Radicals { get; set; } = [];

    [ForeignKey(nameof(Character))]
    public virtual Entry Entry { get; set; } = null!;

    internal const string XmlTagName = "radical";
}

internal static class RadicalGroupReader
{
    public async static Task<RadicalGroup> ReadElementContentAsRadicalGroupAsync(this XmlReader reader, Entry entry)
    {
        var radicalGroup = new RadicalGroup
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
                    await reader.ReadChildElementAsync(radicalGroup);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{RadicalGroup.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == RadicalGroup.XmlTagName;
                    break;
            }
        }
        return radicalGroup;
    }

    private async static Task ReadChildElementAsync(this XmlReader reader, RadicalGroup group)
    {
        switch (reader.Name)
        {
            case Radical.XmlTagName:
                group.Radicals.Add(new Radical
                {
                    Character = group.Character,
                    Order = group.Radicals.Count + 1,
                    Type = reader.GetAttribute("rad_type") ?? throw new Exception($"Character `{group.Character}` missing radical type"),
                    Number = int.Parse(await reader.ReadElementContentAsStringAsync()),
                    Entry = group.Entry,
                });
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{RadicalGroup.XmlTagName}`");
        }
    }
}
