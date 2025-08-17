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
    public List<ReadingKanjiFormBridge>? KanjiFormBridges { get; set; }
    public List<ReadingInfoTag>? InfoTags { get; set; }

    [ForeignKey(nameof(EntryId))]
    public virtual Entry Entry { get; set; } = null!;

    [NotMapped]
    public required bool NoKanji { get; set; }
    [NotMapped]
    public List<string>? ConstraintKanjiFormTexts { get; set; }

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
                    ProcessElement(currentTagName, reading);
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

    private static void ProcessElement(string tagName, Reading reading)
    {
        switch (tagName)
        {
            case "re_nokanji":
                reading.NoKanji = true;
                break;
            default:
                // TODO: Log warning.
                break;
        }
    }

    private async static Task ProcessTextAsync(XmlReader reader, DocumentMetadata docMeta, string tagName, Reading reading)
    {
        switch (tagName)
        {
            case "reb":
                reading.Text = await reader.GetValueAsync();
                break;
            case "re_inf":
                var entityValue = await reader.GetValueAsync();
                var tagDescription = docMeta.GetTagDescription<ReadingInfoTagDescription>(entityValue);
                var infoTag = new ReadingInfoTag
                {
                    EntryId = reading.EntryId,
                    ReadingOrder = reading.Order,
                    TagId = tagDescription.Id,
                    Reading = reading,
                    Description = tagDescription,
                };
                reading.InfoTags ??= [];
                reading.InfoTags.Add(infoTag);
                break;
            case "re_restr":
                var kanjiFormText = await reader.GetValueAsync();
                reading.ConstraintKanjiFormTexts ??= [];
                reading.ConstraintKanjiFormTexts.Add(kanjiFormText);
                break;
            default:
                // TODO: Log warning.
                break;
        }
    }

    #endregion
}
