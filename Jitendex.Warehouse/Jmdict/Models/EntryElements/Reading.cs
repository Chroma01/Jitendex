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
using Jitendex.Warehouse.Jmdict.Models.EntryElements.ReadingElements;

namespace Jitendex.Warehouse.Jmdict.Models.EntryElements;

[PrimaryKey(nameof(EntryId), nameof(Order))]
public class Reading
{
    public required int EntryId { get; set; }
    public required int Order { get; set; }
    public required string Text { get; set; }

    public virtual List<Info> Infos { get; set; } = [];
    public virtual List<Priority> Priorities { get; set; } = [];
    public virtual List<ReadingKanjiFormBridge> KanjiFormBridges { get; set; } = [];

    [ForeignKey(nameof(EntryId))]
    public virtual Entry Entry { get; set; } = null!;

    [NotMapped]
    internal bool NoKanji { get; set; } = false;
    [NotMapped]
    internal List<string> ConstraintKanjiFormTexts { get; set; } = [];

    internal const string XmlTagName = "r_ele";

    public bool IsHidden() => Infos.Any(x => x.TagName == "sk");
}

internal static class ReadingReader
{
    public async static Task<Reading> ReadReadingAsync(this XmlReader reader, Entry entry, KeywordFactory factory)
    {
        var reading = new Reading
        {
            EntryId = entry.Id,
            Order = entry.Readings.Count + 1,
            Text = string.Empty,
            NoKanji = false,
            Entry = entry,
        };

        var exit = false;
        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    await reader.ReadChildElementAsync(reading, factory);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{Reading.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == Reading.XmlTagName;
                    break;
            }
        }
        return reading;
    }

    private async static Task ReadChildElementAsync(this XmlReader reader, Reading reading, KeywordFactory factory)
    {
        switch (reader.Name)
        {
            case "reb":
                reading.Text = await reader.ReadElementContentAsStringAsync();
                break;
            case "re_nokanji":
                reading.NoKanji = true;
                break;
            case "re_restr":
                var kanjiFormText = await reader.ReadElementContentAsStringAsync();
                reading.ConstraintKanjiFormTexts.Add(kanjiFormText);
                break;
            case Info.XmlTagName:
                var readingInfo = await reader.ReadInfoAsync(reading, factory);
                reading.Infos.Add(readingInfo);
                break;
            case Priority.XmlTagName:
                var priority = await reader.ReadPriorityAsync(reading, factory);
                reading.Priorities.Add(priority);
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{Reading.XmlTagName}`");
        }
    }
}
