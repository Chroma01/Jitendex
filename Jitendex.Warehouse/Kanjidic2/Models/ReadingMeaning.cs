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

namespace Jitendex.Warehouse.Kanjidic2.Models;

public class ReadingMeaning
{
    [Key]
    public required string Character { get; set; }
    public List<ReadingMeaningGroup> Groups { get; set; } = [];
    public List<Nanori> Nanoris { get; set; } = [];

    [ForeignKey(nameof(Character))]
    public virtual Entry Entry { get; set; } = null!;

    internal const string XmlTagName = "reading_meaning";
}

internal static class ReadingMeaningReader
{
    public async static Task<ReadingMeaning> ReadElementContentAsReadingMeaningAsync(this XmlReader reader, Entry entry)
    {
        var readingMeaning = new ReadingMeaning
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
                    await reader.ReadChildElementAsync(readingMeaning);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{ReadingMeaning.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == ReadingMeaning.XmlTagName;
                    break;
            }
        }
        return readingMeaning;
    }

    private async static Task ReadChildElementAsync(this XmlReader reader, ReadingMeaning readingMeaning)
    {
        switch (reader.Name)
        {
            case ReadingMeaningGroup.XmlTagName:
                var group = await reader.ReadElementContentAsReadingMeaningGroupAsync(readingMeaning);
                readingMeaning.Groups.Add(group);
                break;
            case Nanori.XmlTagName:
                readingMeaning.Nanoris.Add(new Nanori
                {
                    Character = readingMeaning.Character,
                    Order = readingMeaning.Nanoris.Count + 1,
                    Text = await reader.ReadElementContentAsStringAsync(),
                });
                break;
            default:
                // throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{XmlTagName}`");
                break;
        }
    }
}
