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
using Jitendex.JMdict.Entities.EntryElements.KanjiFormElements;
using Jitendex.JMdict.Entities.EntryElements.ReadingElements;
using Jitendex.JMdict.Entities.EntryElements.SenseElements;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Jitendex.JMdict.Entities;

public interface IKeyword
{
    string Name { get; init; }
    DateOnly CreatedDate { get; init; }
}

[Table(nameof(ReadingInfoTag))]
public class ReadingInfoTag : IKeyword
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }

    [InverseProperty(nameof(ReadingInfo.Tag))]
    public List<ReadingInfo> Infos { get; init; } = [];
}

[Table(nameof(KanjiFormInfoTag))]
public class KanjiFormInfoTag : IKeyword
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }

    [InverseProperty(nameof(KanjiFormInfo.Tag))]
    public List<KanjiFormInfo> Infos { get; init; } = [];
}

[Table(nameof(PartOfSpeechTag))]
public class PartOfSpeechTag : IKeyword
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }

    [InverseProperty(nameof(PartOfSpeech.Tag))]
    public List<PartOfSpeech> PartsOfSpeech { get; init; } = [];
}

[Table(nameof(FieldTag))]
public class FieldTag : IKeyword
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }

    [InverseProperty(nameof(Field.Tag))]
    public List<Field> Fields { get; init; } = [];
}

[Table(nameof(MiscTag))]
public class MiscTag : IKeyword
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }

    [InverseProperty(nameof(Misc.Tag))]
    public List<Misc> Miscs { get; init; } = [];
}

[Table(nameof(DialectTag))]
public class DialectTag : IKeyword
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }

    [InverseProperty(nameof(Dialect.Tag))]
    public List<Dialect> Dialects { get; init; } = [];
}

[Table(nameof(GlossType))]
public class GlossType : IKeyword
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }

    [InverseProperty(nameof(Gloss.Type))]
    public List<Gloss> Glosses { get; init; } = [];
}

[Table(nameof(CrossReferenceType))]
public class CrossReferenceType : IKeyword
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }

    [InverseProperty(nameof(CrossReference.Type))]
    public List<CrossReference> CrossReferences { get; init; } = [];
}

[Table(nameof(LanguageSourceType))]
public class LanguageSourceType : IKeyword
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }

    [InverseProperty(nameof(LanguageSource.Type))]
    public List<LanguageSource> LanguageSources { get; init; } = [];
}

[Table(nameof(PriorityTag))]
public class PriorityTag : IKeyword
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }

    [InverseProperty(nameof(KanjiFormPriority.Tag))]
    public List<KanjiFormPriority> KanjiFormPriorities { get; init; } = [];

    [InverseProperty(nameof(ReadingPriority.Tag))]
    public List<ReadingPriority> ReadingPriorities { get; init; } = [];

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
public class Language : IKeyword
{
    [Key]
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }

    [InverseProperty(nameof(LanguageSource.Language))]
    public List<LanguageSource> LanguageSources { get; init; } = [];
}
