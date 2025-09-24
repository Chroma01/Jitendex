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
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Jitendex.Import.Jmdict.Models;

public class ReadingInfoTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

public class KanjiFormInfoTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

public class PartOfSpeechTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

public class FieldTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

public class MiscTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

public class DialectTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

public class GlossType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

public class CrossReferenceType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

public class LanguageSourceType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

public class ExampleSourceType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

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

public class Language : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}
