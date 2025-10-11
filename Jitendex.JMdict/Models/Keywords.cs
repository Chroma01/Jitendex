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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Jitendex.JMdict.Models;

[Table(nameof(ReadingInfoTag))]
public class ReadingInfoTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

[Table(nameof(KanjiFormInfoTag))]
public class KanjiFormInfoTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

[Table(nameof(PartOfSpeechTag))]
public class PartOfSpeechTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

[Table(nameof(FieldTag))]
public class FieldTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

[Table(nameof(MiscTag))]
public class MiscTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

[Table(nameof(DialectTag))]
public class DialectTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

[Table(nameof(GlossType))]
public class GlossType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

[Table(nameof(CrossReferenceType))]
public class CrossReferenceType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

[Table(nameof(LanguageSourceType))]
public class LanguageSourceType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

[Table(nameof(ExampleSourceType))]
public class ExampleSourceType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

[Table(nameof(PriorityTag))]
public class PriorityTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
    public bool IsHighPriority() => IsHighPriorityName(Name);
    private static bool IsHighPriorityName(string tagName) => tagName switch
    {
        "gai1" or
        "ichi1" or
        "news1" or
        "spec1" or
        "spec2" => true,
        _ => false
    };
}

[Table(nameof(Language))]
public class Language : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}
