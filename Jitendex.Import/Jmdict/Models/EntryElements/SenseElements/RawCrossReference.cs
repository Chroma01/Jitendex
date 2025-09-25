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

namespace Jitendex.Import.Jmdict.Models.EntryElements.SenseElements;

[NotMapped]
internal class RawCrossReference
{
    public required int EntryId { get; set; }
    public required int SenseOrder { get; set; }
    public required int Order { get; set; }
    public required string TypeName { get; set; }

    public required string RefText1 { get; set; }
    public required string? RefText2 { get; set; }
    public required int RefSenseOrder { get; set; }

    public required Sense Sense { get; set; }
    public required CrossReferenceType Type { get; set; }

    /// <summary>
    /// Stable and unique identifier for this reference in the raw data.
    /// </summary>
    public string RawKey() => RefText2 is null ?
        $"{EntryId}・{Sense.Order}・{RefText1}・{RefSenseOrder}" :
        $"{EntryId}・{Sense.Order}・{RefText1}【{RefText2}】・{RefSenseOrder}";

    public const string XmlTagName = "xref";
    public const string XmlTagName_Antonym = "ant";
}
