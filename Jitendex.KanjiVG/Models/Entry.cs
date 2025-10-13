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

namespace Jitendex.KanjiVG.Models;

[Table("Entry")]
[PrimaryKey(nameof(UnicodeScalarValue), nameof(VariantTypeId))]
public class Entry
{
    public required int UnicodeScalarValue { get; set; }
    public required int VariantTypeId { get; set; }

    public required ComponentGroup ComponentGroup { get; set; }
    public required StrokeNumberGroup StrokeNumberGroup { get; set; }

    [ForeignKey(nameof(VariantTypeId))]
    public required VariantType VariantType { get; set; }

    public string FileNameFormat() =>
        UnicodeScalarValue.ToString("X").PadLeft(5, '0').ToLower()
        + VariantType.FileNameFormat();

    public string FileName() => FileNameFormat() + ".svg";
}
