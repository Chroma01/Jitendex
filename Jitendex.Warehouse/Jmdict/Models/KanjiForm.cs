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
public class KanjiForm
{
    public required int EntryId { get; set; }
    public required int Order { get; set; }
    public required string Text { get; set; }
    public List<KanjiFormInfoTag> InfoTags { get; set; } = [];
    public List<KanjiFormPriorityTag> PriorityTags { get; set; } = [];
    public List<ReadingKanjiFormBridge> ReadingBridges { get; set; } = [];

    [ForeignKey(nameof(EntryId))]
    public virtual Entry Entry { get; set; } = null!;

    #region Static XML Factory

    public const string XmlTagName = "k_ele";

    public async static Task<KanjiForm> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta, Entry entry)
    {
        var kanjiForm = new KanjiForm
        {
            EntryId = entry.Id,
            Order = entry.KanjiForms.Count + 1,
            Text = string.Empty,
            Entry = entry,
        };

        var exit = false;
        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    await ProcessElementAsync(reader, docMeta, kanjiForm);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return kanjiForm;
    }

    private async static Task ProcessElementAsync(XmlReader reader, DocumentMetadata docMeta, KanjiForm kanjiForm)
    {
        switch (reader.Name)
        {
            case "keb":
                kanjiForm.Text = await reader.ReadAndGetTextValueAsync();
                break;
            case KanjiFormInfoTag.XmlTagName:
                var infoTag = await KanjiFormInfoTag.FromXmlAsync(reader, docMeta, kanjiForm);
                kanjiForm.InfoTags.Add(infoTag);
                break;
            case KanjiFormPriorityTag.XmlTagName:
                var priorityTag = await KanjiFormPriorityTag.FromXmlAsync(reader, kanjiForm);
                kanjiForm.PriorityTags.Add(priorityTag);
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{XmlTagName}`");
        }
    }

    #endregion
}
