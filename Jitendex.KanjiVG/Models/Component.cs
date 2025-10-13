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

[PrimaryKey(nameof(UnicodeScalarValue), nameof(VariantTypeId), nameof(GlobalOrder))]
public class Component
{
    public required int UnicodeScalarValue { get; set; }
    public required int VariantTypeId { get; set; }
    public required int GlobalOrder { get; set; }

    public required int? ParentGlobalOrder { get; set; }
    public required int LocalOrder { get; set; }

    public required string? Text { get; set; }
    public required bool IsVariant { get; set; }
    public required bool IsPartial { get; set; }
    public required string? Original { get; set; }
    public required int? Part { get; set; }
    public required int? Number { get; set; }
    public required bool IsTradForm { get; set; }
    public required bool IsRadicalForm { get; set; }
    public required int PositionId { get; set; }
    public required int RadicalId { get; set; }
    public required int PhonId { get; set; }

    [ForeignKey($"{nameof(UnicodeScalarValue)}, {nameof(VariantTypeId)}")]
    public required ComponentGroup Group { get; set; }

    [ForeignKey($"{nameof(UnicodeScalarValue)}, {nameof(VariantTypeId)}, {nameof(ParentGlobalOrder)}")]
    public required Component? Parent { get; set; }

    [ForeignKey(nameof(PositionId))]
    public required ComponentPosition Position { get; set; }

    [ForeignKey(nameof(RadicalId))]
    public required ComponentRadical Radical { get; set; }

    [ForeignKey(nameof(PhonId))]
    public required ComponentPhon Phon { get; set; }

    public List<Component> Children { get; set; } = [];

    public List<Stroke> Strokes { get; set; } = [];

    public string XmlIdAttribute() => "kvg:"
        + Group.Entry.FileNameFormat()
        + (GlobalOrder == 1 ? "" : $"-g{GlobalOrder - 1}");

    public int ChildComponentCount()
    {
        int count = 0;
        foreach (var child in Children)
        {
            count++;
            count += child.ChildComponentCount();
        }
        return count;
    }

    public int StrokeCount()
    {
        int count = 0;
        foreach (var stroke in Strokes)
        {
            count++;
        }
        foreach (var child in Children)
        {
            count += child.StrokeCount();
        }
        return count;
    }
}
