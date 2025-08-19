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

[PrimaryKey(nameof(EntryId), nameof(SenseOrder), nameof(TagId))]
public class MiscTag
{
    public required int EntryId { get; set; }
    public required int SenseOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(SenseOrder)}")]
    public virtual Sense Sense { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public virtual MiscTagDescription Description { get; set; } = null!;

    #region Static XML Factory

    public const string XmlTagName = "misc";

    public async static Task<MiscTag> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta, Sense sense)
    {
        var text = await reader.ReadAndGetTextValueAsync();
        var desc = docMeta.GetTagDescription<MiscTagDescription>(text);
        return new MiscTag
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            TagId = desc.Id,
            Sense = sense,
            Description = desc,
        };
    }

    #endregion
}
