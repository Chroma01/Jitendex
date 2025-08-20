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

namespace Jitendex.Warehouse.Kanjidic2.Models;

[NotMapped]
internal class ReadingMeaning
{
    public required string Character { get; set; }
    public List<Reading> Readings { get; set; } = [];
    public List<Meaning> Meanings { get; set; } = [];

    [ForeignKey(nameof(Character))]
    public virtual Entry Entry { get; set; } = null!;

    internal const string XmlTagName = "rmgroup";
}

internal static class ReadingMeaningReader
{
    public async static Task<ReadingMeaning> ReadElementContentAsReadingMeaningAsync(this XmlReader reader, ReadingMeaningGroup group)
    {
        var readingMeaning = new ReadingMeaning
        {
            Character = group.Character,
            Entry = group.Entry,
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
            case Reading.XmlTagName:
                var reading = await reader.ReadElementContentAsReadingAsync(readingMeaning);
                readingMeaning.Readings.Add(reading);
                break;
            case Meaning.XmlTagName:
                var meaning = await reader.ReadElementContentAsMeaningAsync(readingMeaning);
                readingMeaning.Meanings.Add(meaning);
                break;
        }
    }
}
