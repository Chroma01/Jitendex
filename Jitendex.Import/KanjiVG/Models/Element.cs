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
    public int UnicodeScalarValue { get; set; }
    public string? VariantTypeName { get; set; }
    public required string Id { get; set; }

    public required string GroupId { get; set; }
    public string? ParentId { get; set; }
    public required int Order { get; set; }

    public string? Text { get; set; }
    public string? Variant { get; set; } // Boolean?
    public string? Partial { get; set; } // Boolean?
    public string? Original { get; set; }
    public string? Part { get; set; } // Int?
    public string? Number { get; set; } // Int?
    public string? TradForm { get; set; }
    public string? RadicalForm { get; set; }
    public string? Position { get; set; }
    public string? Radical { get; set; }
    public string? Phon { get; set; }


    [ForeignKey($"{nameof(UnicodeScalarValue)}, {nameof(VariantTypeName)}, {nameof(GroupId)}")]
    public required ElementGroup ElementGroup { get; set; }

    [ForeignKey($"{nameof(UnicodeScalarValue)}, {nameof(VariantTypeName)}, {nameof(ParentId)}")]
    public Element? ParentElement { get; set; }

    public List<Element> ChildElements { get; set; } = [];

    public List<Stroke> Strokes { get; set; } = [];
}
