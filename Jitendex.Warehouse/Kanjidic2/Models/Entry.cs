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
using System.Xml;

namespace Jitendex.Warehouse.Kanjidic2.Models;

public class Entry
{
    [Key]
    public required string Character { get; set; }
    public ReadingMeaning? ReadingMeaning { get; set; }

    #region Static XML Factory

    public const string XmlTagName = "character";

    public async static Task<Entry> FromXmlAsync(XmlReader reader)
    {
        var entry = new Entry
        {
            Character = string.Empty,
        };
        var exit = false;
        string currentTagName = XmlTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    if (currentTagName == ReadingMeaning.XmlTagName)
                    {
                        entry.ReadingMeaning = await ReadingMeaning.FromXmlAsync(reader, entry);
                    }
                    break;
                case XmlNodeType.Text:
                    if (currentTagName == "literal")
                    {
                        entry.Character = await reader.GetValueAsync();
                    }
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return entry;
    }

    #endregion
}