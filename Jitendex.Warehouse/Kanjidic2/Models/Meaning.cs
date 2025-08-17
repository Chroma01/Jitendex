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

[PrimaryKey(nameof(Character), nameof(GroupOrder), nameof(Order))]
public class Meaning
{
    public required string Character { get; set; }
    public required int GroupOrder { get; set; }
    public required int Order { get; set; }
    public required string Text { get; set; }
    public required string Language { get; set; }

    [ForeignKey($"{nameof(Character)}, {nameof(GroupOrder)}")]
    public virtual ReadingMeaningGroup Group { get; set; } = null!;

    #region Static XML Factory

    public const string XmlTagName = "meaning";

    public async static Task<Meaning> FromXmlAsync(XmlReader reader, ReadingMeaningGroup group)
    {
        var meaning = new Meaning
        {
            Character = group.Character,
            GroupOrder = group.Order,
            Order = (group.Meanings?.Count ?? 0) + 1,
            Text = string.Empty,
            Language = reader.GetAttribute("m_lang") ?? "en",
            Group = group,
        };
        var exit = false;
        string currentTagName = XmlTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    break;
                case XmlNodeType.Text:
                    if (currentTagName == XmlTagName)
                    {
                        meaning.Text = await reader.GetValueAsync();
                    }
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return meaning;
    }

    #endregion
}
