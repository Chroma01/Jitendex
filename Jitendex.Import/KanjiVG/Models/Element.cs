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

namespace Jitendex.Import.KanjiVG.Models;

[PrimaryKey(nameof(UnicodeScalarValue), nameof(VariantTypeName), nameof(Id))]
public class Element
{
    public required int UnicodeScalarValue { get; set; }
    public required string VariantTypeName { get; set; }
    public required string Id { get; set; }

    public required string GroupId { get; set; }
    public required string? ParentId { get; set; }
    public required int Order { get; set; }

    public required string? Text { get; set; }
    public required string? Variant { get; set; } // Boolean?
    public required string? Partial { get; set; } // Boolean?
    public required string? Original { get; set; }
    public required string? Part { get; set; } // Int?
    public required string? Number { get; set; } // Int?
    public required string? TradForm { get; set; }
    public required string? RadicalForm { get; set; }
    public required string? Position { get; set; }
    public required string? Radical { get; set; }
    public required string? Phon { get; set; }


    [ForeignKey($"{nameof(UnicodeScalarValue)}, {nameof(VariantTypeName)}, {nameof(GroupId)}")]
    public required ElementGroup Group { get; set; }

    [ForeignKey($"{nameof(UnicodeScalarValue)}, {nameof(VariantTypeName)}, {nameof(ParentId)}")]
    public Element? Parent { get; set; }

    public List<Element> Children { get; set; } = [];

    public List<Stroke> Strokes { get; set; } = [];
}
