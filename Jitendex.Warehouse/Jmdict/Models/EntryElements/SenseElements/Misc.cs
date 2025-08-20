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

namespace Jitendex.Warehouse.Jmdict.Models.EntryElements.SenseElements;

[PrimaryKey(nameof(EntryId), nameof(SenseOrder), nameof(TagId))]
public class Misc
{
    public required int EntryId { get; set; }
    public required int SenseOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(SenseOrder)}")]
    public virtual Sense Sense { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public virtual MiscTag Tag { get; set; } = null!;

    internal const string XmlTagName = "misc";
}

internal static class MiscReader
{
    public async static Task<Misc> ReadMiscAsync(this XmlReader reader, Sense sense, DocumentMetadata docMeta)
    {
        var text = await reader.ReadElementContentAsStringAsync();
        var tag = docMeta.GetTag<MiscTag>(text);
        return new Misc
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            TagId = tag.Id,
            Sense = sense,
            Tag = tag,
        };
    }
}
