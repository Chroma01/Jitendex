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
using Jitendex.Warehouse.Jmdict.Models.EntryElements.KanjiFormElements;

namespace Jitendex.Warehouse.Jmdict.Models.EntryElements;

[PrimaryKey(nameof(EntryId), nameof(Order))]
public class KanjiForm
{
    public required int EntryId { get; set; }
    public required int Order { get; set; }
    public required string Text { get; set; }
    public virtual List<Info> Infos { get; set; } = [];
    public virtual List<Priority> Priorities { get; set; } = [];
    public virtual List<ReadingKanjiFormBridge> ReadingBridges { get; set; } = [];

    [ForeignKey(nameof(EntryId))]
    public virtual Entry Entry { get; set; } = null!;

    internal const string XmlTagName = "k_ele";

    public bool IsHidden() => Infos.Any(x => x.TagName == "sK");
}

internal static class KanjiFormReader
{
    public async static Task<KanjiForm> ReadKanjiFormAsync(this XmlReader reader, Entry entry, KeywordFactory factory)
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
                    await reader.ReadChildElementAsync(kanjiForm, factory);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{KanjiForm.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == KanjiForm.XmlTagName;
                    break;
            }
        }
        return kanjiForm;
    }

    private async static Task ReadChildElementAsync(this XmlReader reader, KanjiForm kanjiForm, KeywordFactory factory)
    {
        switch (reader.Name)
        {
            case "keb":
                kanjiForm.Text = await reader.ReadElementContentAsStringAsync();
                break;
            case Info.XmlTagName:
                var info = await reader.ReadInfoAsync(kanjiForm, factory);
                kanjiForm.Infos.Add(info);
                break;
            case Priority.XmlTagName:
                var priority = await reader.ReadPriorityAsync(kanjiForm, factory);
                kanjiForm.Priorities.Add(priority);
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{KanjiForm.XmlTagName}`");
        }
    }
}
