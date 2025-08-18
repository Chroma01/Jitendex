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

[PrimaryKey(nameof(EntryId), nameof(ReadingOrder), nameof(TagId))]
public class ReadingPriorityTag
{
    public required int EntryId { get; set; }
    public required int ReadingOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(ReadingOrder)}")]
    public virtual Reading Reading { get; set; } = null!;
}

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

[PrimaryKey(nameof(EntryId), nameof(KanjiFormOrder), nameof(TagId))]
public class KanjiFormPriorityTag
{
    public required int EntryId { get; set; }
    public required int KanjiFormOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(KanjiFormOrder)}")]
    public virtual KanjiForm KanjiForm { get; set; } = null!;
}

[PrimaryKey(nameof(EntryId), nameof(SenseOrder), nameof(TagId))]
public class PartOfSpeechTag
{
    public required int EntryId { get; set; }
    public required int SenseOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(SenseOrder)}")]
    public virtual Sense Sense { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public virtual PartOfSpeechTagDescription Description { get; set; } = null!;
}

[PrimaryKey(nameof(EntryId), nameof(SenseOrder), nameof(TagId))]
public class FieldTag
{
    public required int EntryId { get; set; }
    public required int SenseOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(SenseOrder)}")]
    public virtual Sense Sense { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public virtual FieldTagDescription Description { get; set; } = null!;
}

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
}

[PrimaryKey(nameof(EntryId), nameof(SenseOrder), nameof(TagId))]
public class DialectTag
{
    public required int EntryId { get; set; }
    public required int SenseOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(SenseOrder)}")]
    public virtual Sense Sense { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public virtual DialectTagDescription Description { get; set; } = null!;
}