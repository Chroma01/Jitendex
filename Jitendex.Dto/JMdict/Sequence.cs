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

public sealed record SequenceDto(int Id, DateOnly Date)
{
    public EntryDto? Entry { get; init; } = null;
    public List<RevisionDto> Revisions { get; init; } = [];
}

public sealed record RevisionDto(int Number, DateOnly Date, string DiffJson);

public sealed record EntryDto
{
    public List<KanjiFormDto> KanjiForms { get; init; } = [];
    public List<ReadingDto> Readings { get; init; } = [];
    public List<SenseDto> Senses { get; init; } = [];
}

public sealed record KanjiFormDto(string Text)
{
    public List<KanjiFormInfoDto> Infos { get; init; } = [];
    public List<KanjiFormPriorityDto> Priorities { get; init; } = [];
}

public sealed record ReadingDto(string Text, bool NoKanji)
{
    public List<ReadingInfoDto> Infos { get; init; } = [];
    public List<ReadingPriorityDto> Priorities { get; init; } = [];
    public List<RestrictionDto> Restrictions { get; init; } = [];
}

public sealed record SenseDto(string? Note)
{
    public List<KanjiFormRestrictionDto> KanjiFormRestrictions { get; init; } = [];
    public List<ReadingRestrictionDto> ReadingRestrictions { get; init; } = [];
    public List<PartOfSpeechDto> PartsOfSpeech { get; init; } = [];
    public List<FieldDto> Fields { get; init; } = [];
    public List<MiscDto> Miscs { get; init; } = [];
    public List<DialectDto> Dialects { get; init; } = [];
    public List<GlossDto> Glosses { get; init; } = [];
    public List<LanguageSourceDto> LanguageSources { get; init; } = [];
    public List<CrossReferenceDto> CrossReferences { get; init; } = [];
}

public sealed record KanjiFormInfoDto(string TagName);
public sealed record KanjiFormPriorityDto(string TagName);
public sealed record ReadingInfoDto(string TagName);
public sealed record ReadingPriorityDto(string TagName);
public sealed record RestrictionDto(string KanjiFormText);
public sealed record CrossReferenceDto(string TypeName, string RefText1, string? RefText2, int RefSenseOrder);
public sealed record DialectDto(string TagName);
public sealed record FieldDto(string TagName);
public sealed record GlossDto(string TypeName, string Text);
public sealed record KanjiFormRestrictionDto(string KanjiFormText);
public sealed record LanguageSourceDto(string? Text, string LanguageCode, string TypeName, bool IsWasei);
public sealed record MiscDto(string TagName);
public sealed record PartOfSpeechDto(string TagName);
public sealed record ReadingRestrictionDto(string ReadingText);
