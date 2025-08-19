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

namespace Jitendex.Warehouse.Jmdict.Models;

[PrimaryKey(nameof(EntryId), nameof(Order))]
public class Reading
{
    public required int EntryId { get; set; }
    public required int Order { get; set; }
    public required string Text { get; set; }
    public List<ReadingInfoTag> InfoTags { get; set; } = [];
    public List<ReadingPriorityTag> PriorityTags { get; set; } = [];
    public List<ReadingKanjiFormBridge> KanjiFormBridges { get; set; } = [];

    [ForeignKey(nameof(EntryId))]
    public virtual Entry Entry { get; set; } = null!;

    // TODO: Derive these [NotMapped] properties from mapped properties.
    [NotMapped]
    public bool NoKanji { get; set; } = false;
    [NotMapped]
    public List<string> ConstraintKanjiFormTexts { get; set; } = [];

    #region Static XML Factory

    public const string XmlTagName = "r_ele";

    public async static Task<Reading> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta, Entry entry)
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
                    await ProcessElementAsync(reader, docMeta, reading);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return reading;
    }

    private async static Task ProcessElementAsync(XmlReader reader, DocumentMetadata docMeta, Reading reading)
    {
        switch (reader.Name)
        {
            case "reb":
                reading.Text = await reader.ReadAndGetTextValueAsync();
                break;
            case "re_nokanji":
                reading.NoKanji = true;
                break;
            case "re_restr":
                var kanjiFormText = await reader.ReadAndGetTextValueAsync();
                reading.ConstraintKanjiFormTexts.Add(kanjiFormText);
                break;
            case ReadingInfoTag.XmlTagName:
                var readingInfoTag = await ReadingInfoTag.FromXmlAsync(reader, docMeta, reading);
                reading.InfoTags.Add(readingInfoTag);
                break;
            case "re_pri":
                reading.PriorityTags.Add(new ReadingPriorityTag
                {
                    EntryId = reading.EntryId,
                    ReadingOrder = reading.Order,
                    TagId = await reader.ReadAndGetTextValueAsync(),
                    Reading = reading,
                });
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{XmlTagName}`");
        }
    }

    #endregion
}
