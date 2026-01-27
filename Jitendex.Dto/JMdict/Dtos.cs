/*
Copyright (c) 2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms of
the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

namespace Jitendex.Dto.JMdict;

public sealed class SequenceDto
{
    public required int Id { get; init; }
    public required DateOnly CreatedDate { get; init; }
    public EntryDto? Entry { get; init; }
    public IReadOnlyList<RevisionDto> Revisions { get; init; } = [];
}

public sealed class EntryDto
{
    public IReadOnlyList<KanjiFormDto> KanjiForms { get; init; } = [];
    public IReadOnlyList<ReadingDto> Readings { get; init; } = [];
    public IReadOnlyList<SenseDto> Senses { get; init; } = [];
}

public sealed class KanjiFormDto
{
    public required string Text { get; init; }
    public IReadOnlyList<string> Infos { get; init; } = [];
    public IReadOnlyList<string> Priorities { get; init; } = [];
}

public sealed class ReadingDto
{
    public required string Text { get; init; }
    public required bool NoKanji { get; init; }
    public IReadOnlyList<string> Infos { get; init; } = [];
    public IReadOnlyList<string> Priorities { get; init; } = [];
    public IReadOnlyList<string> Restrictions { get; init; } = [];
}

public sealed class SenseDto
{
    public string? Note { get; init; }
    public IReadOnlyList<string> KanjiFormRestrictions { get; init; } = [];
    public IReadOnlyList<string> ReadingRestrictions { get; init; } = [];
    public IReadOnlyList<string> PartsOfSpeech { get; init; } = [];
    public IReadOnlyList<string> Fields { get; init; } = [];
    public IReadOnlyList<string> Miscs { get; init; } = [];
    public IReadOnlyList<string> Dialects { get; init; } = [];
    public IReadOnlyList<LanguageSourceDto> LanguageSources { get; init; } = [];
    public IReadOnlyList<GlossDto> Glosses { get; init; } = [];
    public IReadOnlyList<CrossReferenceDto> CrossReferences { get; init; } = [];
}

public sealed record RevisionDto(int Number, DateOnly Date, string DiffJson);
public sealed record LanguageSourceDto(string? Text, string LanguageCode, string TypeName, bool IsWasei);
public sealed record GlossDto(string Text, string TypeName);
public sealed record CrossReferenceDto(string TypeName, string RefText1, string? RefText2, int RefSenseOrder);
