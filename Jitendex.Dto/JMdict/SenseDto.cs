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

public sealed class SenseDto
{
    public string? Note { get; init; }
    public List<string> KanjiFormRestrictions { get; init; } = [];
    public List<string> ReadingRestrictions { get; init; } = [];
    public List<string> PartsOfSpeech { get; init; } = [];
    public List<string> Fields { get; init; } = [];
    public List<string> Miscs { get; init; } = [];
    public List<string> Dialects { get; init; } = [];
    public List<LanguageSourceDto> LanguageSources { get; init; } = [];
    public List<GlossDto> Glosses { get; init; } = [];
    public List<CrossReferenceDto> CrossReferences { get; init; } = [];

    public override string ToString()
    {
        var sb = new StringBuilder();
        if (PartsOfSpeech.Count > 0)
        {
            sb.Append($"[{string.Join(", ", PartsOfSpeech)}]");
        }
        if (Miscs.Count > 0)
        {
            sb.Append($"[{string.Join(", ", Miscs)}]");
        }
        if (Fields.Count > 0)
        {
            sb.Append($"{{{string.Join(", ", Fields)}}}");
        }
        if (KanjiFormRestrictions.Count > 0)
        {
            sb.Append($"[{string.Join("；", KanjiFormRestrictions)}]");
        }
        if (ReadingRestrictions.Count > 0)
        {
            sb.Append($"[{string.Join("；", ReadingRestrictions)}]");
        }
        if (Dialects.Count > 0)
        {
            sb.AppendLine();
            sb.Append($"\t\tDialect: {string.Join(", ", Dialects)}");
        }
        if (LanguageSources.Count > 0)
        {
            sb.AppendLine();
            sb.Append($"\t\tSource lang: {string.Join("; ", LanguageSources.Select(static l => l.ToString()))}");
        }
        if (Note is not null)
        {
            sb.AppendLine();
            sb.Append($"\t\t《{Note}》");
        }
        if (Glosses.Count > 0)
        {
            foreach (var gloss in Glosses)
            {
                sb.AppendLine();
                sb.Append($"\t\t▶ {gloss}");
            }
        }
        if (CrossReferences.Count > 0)
        {
            sb.AppendLine();
            sb.Append("\t\tCross references:");
            foreach (var xref in CrossReferences)
            {
                sb.AppendLine();
                sb.Append($"\t\t{xref}");
            }
        }
        return sb.ToString();
    }
}
