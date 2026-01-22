/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

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

namespace Jitendex.JMdict.Entities;

[Table(nameof(ReadingInfoTag))]
public class ReadingInfoTag
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

[Table(nameof(KanjiFormInfoTag))]
public class KanjiFormInfoTag
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

[Table(nameof(PartOfSpeechTag))]
public class PartOfSpeechTag
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

[Table(nameof(FieldTag))]
public class FieldTag
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

[Table(nameof(MiscTag))]
public class MiscTag
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

[Table(nameof(DialectTag))]
public class DialectTag
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

[Table(nameof(GlossType))]
public class GlossType
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

[Table(nameof(CrossReferenceType))]
public class CrossReferenceType
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

[Table(nameof(LanguageSourceType))]
public class LanguageSourceType
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

[Table(nameof(PriorityTag))]
public class PriorityTag
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
    public bool IsHighPriority() => Name switch
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
public class Language
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}
