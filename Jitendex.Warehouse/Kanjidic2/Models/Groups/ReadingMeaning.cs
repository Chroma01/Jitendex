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
using Jitendex.Warehouse.Kanjidic2.Models.EntryElements;

namespace Jitendex.Warehouse.Kanjidic2.Models.Groups;

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
                readingMeaning.Readings.Add(new Reading
                {
                    Character = readingMeaning.Character,
                    Order = readingMeaning.Readings.Count + 1,
                    Type = reader.GetAttribute("r_type") ?? string.Empty,
                    Text = await reader.ReadElementContentAsStringAsync(),
                    Entry = readingMeaning.Entry,
                });
                break;
            case Meaning.XmlTagName:
                readingMeaning.Meanings.Add(new Meaning
                {
                    Character = readingMeaning.Character,
                    Order = readingMeaning.Meanings.Count + 1,
                    Language = reader.GetAttribute("m_lang") ?? "en",
                    Text = await reader.ReadElementContentAsStringAsync(),
                    Entry = readingMeaning.Entry,
                });
                break;
        }
    }
}
