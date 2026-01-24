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

public sealed record SenseDto(string? Note)
{
    public List<string> KanjiFormRestrictions { get; init; } = [];
    public List<string> ReadingRestrictions { get; init; } = [];
    public List<string> PartsOfSpeech { get; init; } = [];
    public List<string> Fields { get; init; } = [];
    public List<string> Miscs { get; init; } = [];
    public List<string> Dialects { get; init; } = [];
    public List<LanguageSourceDto> LanguageSources { get; init; } = [];
    public List<GlossDto> Glosses { get; init; } = [];
    public List<CrossReferenceDto> CrossReferences { get; init; } = [];

    public override string ToString() =>
          (PartsOfSpeech.Count > 0 ? $" [{string.Join(", ", PartsOfSpeech)}]" : string.Empty)
        + (Miscs.Count > 0 ? $" [{string.Join(", ", Miscs)}]" : string.Empty)
        + (Fields.Count > 0 ? $" {{{string.Join(", ", Fields)}}}" : string.Empty)
        + (KanjiFormRestrictions.Count > 0 ? $" [{string.Join("；", KanjiFormRestrictions)}]" : string.Empty)
        + (ReadingRestrictions.Count > 0 ? $" [{string.Join("；", ReadingRestrictions)}]" : string.Empty)
        + (Dialects.Count > 0 ? $"\n\t\tDialect: {string.Join(", ", Dialects)}" : string.Empty)
        + (LanguageSources.Count > 0 ? $"\n\t\tSource lang: {string.Join("; ", LanguageSources.Select(static l => l.ToString()))}" : string.Empty)
        + (Note is not null ? $"\n\t\t《{Note}》" : string.Empty)
        + (Glosses.Count > 0 ? string.Join(string.Empty, Glosses.Select(static g => $"\n\t\t▶ {g}")) : string.Empty)
        + (CrossReferences.Count > 0 ? "\n\t\tCross references:" + string.Join(string.Empty, CrossReferences.Select(static x => $"\n\t\t{x}")) : string.Empty);
}

public sealed record LanguageSourceDto(string? Text, string LanguageCode, string TypeName, bool IsWasei)
{
    public override string ToString() => LanguageCode
        + (TypeName == "full" ? string.Empty : $" ({TypeName})")
        + (IsWasei ? " [wasei]" : string.Empty)
        + (Text is not null ? $": {Text}" : string.Empty);
}

public sealed record GlossDto(string TypeName, string Text)
{
    public override string ToString() => TypeName == string.Empty ? Text : $"[{TypeName}] {Text}";
}

public sealed record CrossReferenceDto(string TypeName, string RefText1, string? RefText2, int RefSenseOrder)
{
    public override string ToString() => RefText2 is not null
        ? $"⇒{TypeName}: {RefText1}・{RefText2}・{RefSenseOrder + 1}"
        : $"⇒{TypeName}: {RefText1}・{RefSenseOrder + 1}";
}
