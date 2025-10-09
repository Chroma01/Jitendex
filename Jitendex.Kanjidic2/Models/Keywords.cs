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

namespace Jitendex.Import.Kanjidic2.Models;

internal interface ICorruptable
{
    bool IsCorrupt { get; set; }
}

internal interface IKeyword : ICorruptable
{
    string Name { get; set; }
    string Description { get; set; }
}

public class CodepointType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

public class DictionaryType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

public class QueryCodeType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

public class MisclassificationType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

public class RadicalType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

public class ReadingType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}

public class VariantType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCorrupt { get; set; }
}
