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

namespace Jitendex.Import.Jmdict.Models.EntryElements.SenseElements;

[PrimaryKey(nameof(EntryId), nameof(SenseOrder), nameof(Order))]
public class CrossReference
{
    public required int EntryId { get; set; }
    public required int SenseOrder { get; set; }
    public required int Order { get; set; }

    public required string TypeName { get; set; }

    public required int RefEntryId { get; set; }
    public required int RefSenseOrder { get; set; }
    public required int RefReadingOrder { get; set; }
    public int? RefKanjiFormOrder { get; set; }

    [ForeignKey(nameof(TypeName))]
    public virtual CrossReferenceType Type { get; set; } = null!;

    [ForeignKey($"{nameof(EntryId)}, {nameof(SenseOrder)}")]
    public virtual Sense Sense { get; set; } = null!;

    [ForeignKey($"{nameof(RefEntryId)}, {nameof(RefSenseOrder)}")]
    public virtual Sense RefSense { get; set; } = null!;

    [ForeignKey($"{nameof(RefEntryId)}, {nameof(RefReadingOrder)}")]
    public virtual Reading RefReading { get; set; } = null!;

    [ForeignKey($"{nameof(RefEntryId)}, {nameof(RefKanjiFormOrder)}")]
    public virtual KanjiForm? RefKanjiForm { get; set; }

    [NotMapped]
    internal string RefText1 { get; set; } = null!;
    [NotMapped]
    internal string? RefText2 { get; set; }

    /// <summary>
    /// Stable and unique identifier for this reference in the raw data.
    /// </summary>
    internal string RawKey()
        => RefText2 is null ?
        $"{EntryId}・{Sense.Order}・{RefText1}・{RefSenseOrder}" :
        $"{EntryId}・{Sense.Order}・{RefText1}【{RefText2}】・{RefSenseOrder}";

    internal const string XmlTagName = "xref";
    internal const string XmlTagName_Antonym = "ant";
}
