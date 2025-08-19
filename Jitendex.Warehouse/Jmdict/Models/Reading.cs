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

    public async static Task<Reading> FromXmlAsync(XmlReader reader, Entry entry, DocumentMetadata docMeta)
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
        string currentTagName = XmlTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    ProcessElement(reader, reading);
                    break;
                case XmlNodeType.Text:
                    await ProcessTextAsync(reader, docMeta, currentTagName, reading);
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return reading;
    }

    private static void ProcessElement(XmlReader reader, Reading reading)
    {
        switch (reader.Name)
        {
            case "re_nokanji":
                reading.NoKanji = true;
                break;
        }
    }

    private async static Task ProcessTextAsync(XmlReader reader, DocumentMetadata docMeta, string tagName, Reading reading)
    {
        var text = await reader.GetValueAsync();
        switch (tagName)
        {
            case "reb":
                reading.Text = text;
                break;
            case "re_inf":
                var tagDescription = docMeta.GetTagDescription<ReadingInfoTagDescription>(text);
                reading.InfoTags.Add(new ReadingInfoTag
                {
                    EntryId = reading.EntryId,
                    ReadingOrder = reading.Order,
                    TagId = tagDescription.Id,
                    Reading = reading,
                    Description = tagDescription,
                });
                break;
            case "re_pri":
                reading.PriorityTags.Add(new ReadingPriorityTag
                {
                    EntryId = reading.EntryId,
                    ReadingOrder = reading.Order,
                    TagId = text,
                    Reading = reading,
                });
                break;
            case "re_restr":
                reading.ConstraintKanjiFormTexts.Add(text);
                break;
            default:
                // TODO: Log warning.
                break;
        }
    }

    #endregion
}
