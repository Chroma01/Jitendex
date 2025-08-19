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

namespace Jitendex.Warehouse.Jmdict.Models.EntryElements.KanjiFormElements;

[Table($"{nameof(KanjiForm)}{nameof(InfoTag)}")]
[PrimaryKey(nameof(EntryId), nameof(KanjiFormOrder), nameof(TagId))]
public class InfoTag
{
    public required int EntryId { get; set; }
    public required int KanjiFormOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(KanjiFormOrder)}")]
    public virtual KanjiForm KanjiForm { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public virtual KanjiFormInfoTagDescription Description { get; set; } = null!;

    internal const string XmlTagName = "ke_inf";
}

internal static class InfoTagReader
{
    public async static Task<InfoTag> ReadElementContentAsInfoTagAsync(this XmlReader reader, DocumentMetadata docMeta, KanjiForm kanjiForm)
    {
        var text = await reader.ReadElementContentAsStringAsync();
        var desc = docMeta.GetTagDescription<KanjiFormInfoTagDescription>(text);
        return new InfoTag
        {
            EntryId = kanjiForm.EntryId,
            KanjiFormOrder = kanjiForm.Order,
            TagId = desc.Id,
            KanjiForm = kanjiForm,
            Description = desc,
        };
    }
}