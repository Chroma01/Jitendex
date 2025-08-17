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
    public List<ReadingMeaningGroup>? Groups { get; set; }
    public List<Nanori>? Nanoris { get; set; }

    [ForeignKey(nameof(Character))]
    public virtual Entry Entry { get; set; } = null!;

    #region Static XML Factory

    public const string XmlTagName = "reading_meaning";

    public async static Task<ReadingMeaning> FromXmlAsync(XmlReader reader, Entry entry)
    {
        var readingMeaning = new ReadingMeaning
        {
            Character = entry.Character,
            Entry = entry,
        };
        var exit = false;
        string currentTagName = XmlTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    if (currentTagName == "rmgroup")
                    {
                        var group = await ReadingMeaningGroup.FromXmlAsync(reader, readingMeaning);
                        readingMeaning.Groups ??= [];
                        readingMeaning.Groups.Add(group);
                    }
                    break;
                case XmlNodeType.Text:
                    if (currentTagName == "nanori")
                    {
                        var nanori = new Nanori
                        {
                            Character = readingMeaning.Character,
                            Order = (readingMeaning.Nanoris?.Count ?? 0) + 1,
                            Text = await reader.GetValueAsync(),
                        };
                        readingMeaning.Nanoris ??= [];
                        readingMeaning.Nanoris.Add(nanori);
                    }
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return readingMeaning;
    }

    #endregion
}
