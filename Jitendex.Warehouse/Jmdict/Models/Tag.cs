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
using Microsoft.EntityFrameworkCore;

namespace Jitendex.Warehouse.Jmdict.Models;

[Table("Jmdict.ReadingInfoTags")]
[PrimaryKey(nameof(EntryId), nameof(ReadingOrder), nameof(TagId))]
public class ReadingInfoTag
{
    public required int EntryId { get; set; }
    public required int ReadingOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(ReadingOrder)}")]
    public virtual Reading Reading { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public virtual ReadingInfoTagDescription Description { get; set; } = null!;
}

[Table("Jmdict.KanjiFormInfoTags")]
[PrimaryKey(nameof(EntryId), nameof(KanjiFormOrder), nameof(TagId))]
public class KanjiFormInfoTag
{
    public required int EntryId { get; set; }
    public required int KanjiFormOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(KanjiFormOrder)}")]
    public virtual KanjiForm KanjiForm { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public virtual KanjiFormInfoTagDescription Description { get; set; } = null!;
}