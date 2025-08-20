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

[Table($"{nameof(KanjiForm)}{nameof(PriorityTag)}")]
[PrimaryKey(nameof(EntryId), nameof(KanjiFormOrder), nameof(TagId))]
public class PriorityTag
{
    public required int EntryId { get; set; }
    public required int KanjiFormOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(KanjiFormOrder)}")]
    public virtual KanjiForm KanjiForm { get; set; } = null!;

    internal const string XmlTagName = "ke_pri";
}

internal static class PriorityTagReader
{
    public async static Task<PriorityTag> ReadPriorityTagAsync(this XmlReader reader, KanjiForm kanjiForm)
        => new PriorityTag
        {
            EntryId = kanjiForm.EntryId,
            KanjiFormOrder = kanjiForm.Order,
            TagId = await reader.ReadElementContentAsStringAsync(),
            KanjiForm = kanjiForm,
        };
}
