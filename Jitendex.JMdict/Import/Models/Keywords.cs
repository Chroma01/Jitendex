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

using Jitendex.JMdict.Entities;

namespace Jitendex.JMdict.Import.Models;

internal interface IKeywordElement : IKeyword
{
    abstract static string EntityName { get; }
}

internal sealed record ReadingInfoTagElement : IKeywordElement
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
    public static string EntityName => nameof(ReadingInfoTag);
}

internal sealed record KanjiFormInfoTagElement : IKeywordElement
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
    public static string EntityName => nameof(KanjiFormInfoTag);
}

internal sealed record PartOfSpeechTagElement : IKeywordElement
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
    public static string EntityName => nameof(PartOfSpeechTag);
}

internal sealed record FieldTagElement : IKeywordElement
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
    public static string EntityName => nameof(FieldTag);
}

internal sealed record MiscTagElement : IKeywordElement
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
    public static string EntityName => nameof(MiscTag);
}

internal sealed record DialectTagElement : IKeywordElement
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
    public static string EntityName => nameof(DialectTag);
}

internal sealed record GlossTypeElement : IKeywordElement
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
    public static string EntityName => nameof(GlossType);
}

internal sealed record CrossReferenceTypeElement : IKeywordElement
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
    public static string EntityName => nameof(CrossReferenceType);
}

internal sealed record LanguageSourceTypeElement : IKeywordElement
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
    public static string EntityName => nameof(LanguageSourceType);
}

internal sealed record PriorityTagElement : IKeywordElement
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
    public static string EntityName => nameof(PriorityTag);
}

internal sealed record LanguageElement : IKeywordElement
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
    public static string EntityName => nameof(Language);
}
