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
using Jitendex.Warehouse.Kanjidic2.Models.EntryElements;

namespace Jitendex.Warehouse.Kanjidic2.Models.Groups;

[NotMapped]
internal class DictionaryGroup
{
    [Key]
    public required string Character { get; set; }
    public List<Dictionary> Dictionaries { get; set; } = [];

    [ForeignKey(nameof(Character))]
    public virtual Entry Entry { get; set; } = null!;

    internal const string XmlTagName = "dic_number";
}

internal static class DictionaryGroupReader
{
    public async static Task<DictionaryGroup> ReadElementContentAsDictionaryGroupAsync(this XmlReader reader, Entry entry)
    {
        var dicNumberGroup = new DictionaryGroup
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
                    await reader.ReadChildElementAsync(dicNumberGroup);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{DictionaryGroup.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == DictionaryGroup.XmlTagName;
                    break;
            }
        }
        return dicNumberGroup;
    }

    private async static Task ReadChildElementAsync(this XmlReader reader, DictionaryGroup group)
    {
        switch (reader.Name)
        {
            case Dictionary.XmlTagName:
                var volume = reader.GetAttribute("m_vol");
                var page = reader.GetAttribute("m_page");
                group.Dictionaries.Add(new Dictionary
                {
                    Character = group.Character,
                    Order = group.Dictionaries.Count + 1,
                    Type = reader.GetAttribute("dr_type") ?? throw new Exception($"Character `{group.Character}` missing dictionary type"),
                    Volume = volume != null ? int.Parse(volume) : null,
                    Page = page != null ? int.Parse(page) : null,
                    Text = await reader.ReadElementContentAsStringAsync(),
                    Entry = group.Entry,
                });
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{DictionaryGroup.XmlTagName}`");
        }
    }
}
