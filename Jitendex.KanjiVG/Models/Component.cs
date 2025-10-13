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

[PrimaryKey(nameof(UnicodeScalarValue), nameof(VariantTypeName), nameof(Id))]
public class Component
{
    public required int UnicodeScalarValue { get; set; }
    public required string VariantTypeName { get; set; }
    public required string Id { get; set; }

    public required string? ParentId { get; set; }
    public required int Order { get; set; }

    public required string? Text { get; set; }
    public required bool Variant { get; set; }
    public required bool Partial { get; set; }
    public required string? Original { get; set; }
    public required int? Part { get; set; }
    public required int? Number { get; set; }
    public required bool TradForm { get; set; }
    public required bool RadicalForm { get; set; }
    public required string? Position { get; set; }
    public required string? Radical { get; set; }
    public required string? Phon { get; set; }

    [ForeignKey($"{nameof(UnicodeScalarValue)}, {nameof(VariantTypeName)}")]
    public required ComponentGroup Group { get; set; }

    [ForeignKey($"{nameof(UnicodeScalarValue)}, {nameof(VariantTypeName)}, {nameof(ParentId)}")]
    public required Component? Parent { get; set; }

    public List<Component> Children { get; set; } = [];

    public List<Stroke> Strokes { get; set; } = [];

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
