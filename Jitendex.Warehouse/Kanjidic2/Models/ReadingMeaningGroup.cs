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

using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;
using Microsoft.EntityFrameworkCore;

namespace Jitendex.Warehouse.Kanjidic2.Models;

[PrimaryKey(nameof(Character), nameof(Order))]
public class ReadingMeaningGroup
{
    public required string Character { get; set; }
    public required int Order { get; set; }
    public List<Reading> Readings { get; set; } = [];
    public List<Meaning> Meanings { get; set; } = [];

    [ForeignKey(nameof(Character))]
    public virtual ReadingMeaning ReadingMeaning { get; set; } = null!;

    internal const string XmlTagName = "rmgroup";
}

internal static class ReadingMeaningGroupReader
{
    public async static Task<ReadingMeaningGroup> ReadElementContentAsReadingMeaningGroupAsync(this XmlReader reader, ReadingMeaning readingMeaning)
    {
        var group = new ReadingMeaningGroup
        {
            Character = readingMeaning.Character,
            Order = readingMeaning.Groups.Count + 1,
            ReadingMeaning = readingMeaning,
        };

        var exit = false;
        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    await reader.ReadChildElementAsync(group);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{ReadingMeaningGroup.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == ReadingMeaningGroup.XmlTagName;
                    break;
            }
        }
        return group;
    }

    private async static Task ReadChildElementAsync(this XmlReader reader, ReadingMeaningGroup group)
    {
        switch (reader.Name)
        {
            case Reading.XmlTagName:
                var reading = await reader.ReadElementContentAsReadingAsync(group);
                group.Readings.Add(reading);
                break;
            case Meaning.XmlTagName:
                var meaning = await reader.ReadElementContentAsMeaningAsync(group);
                group.Meanings.Add(meaning);
                break;
        }
    }
}
