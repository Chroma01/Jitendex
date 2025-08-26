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
using Jitendex.Warehouse.Kanjidic2.Models;
using Jitendex.Warehouse.Kanjidic2.Models.EntryElements;
using Jitendex.Warehouse.Kanjidic2.Models.Groups;

namespace Jitendex.Warehouse.Kanjidic2.Readers.GroupReaders;

internal static class ReadingMeaningGroupReader
{
    public async static Task<ReadingMeaningGroup> ReadReadingMeaningGroupAsync(this XmlReader reader, Entry entry)
    {
        var group = new ReadingMeaningGroup
        {
            Character = entry.Character,
            ReadingMeaning = null,
            Entry = entry,
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
            case ReadingMeaning.XmlTagName:
                if (group.ReadingMeaning != null)
                    throw new Exception($"Reading-meaning group for character `{group.Character}` has more than one reading-meaning set.");
                var readingMeaning = await reader.ReadReadingMeaningAsync(group);
                group.ReadingMeaning = readingMeaning;
                break;
            case Nanori.XmlTagName:
                group.Nanoris.Add(new Nanori
                {
                    Character = group.Character,
                    Order = group.Nanoris.Count + 1,
                    Text = await reader.ReadElementContentAsStringAsync(),
                    Entry = group.Entry,
                });
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{ReadingMeaningGroup.XmlTagName}`");
        }
    }
}
