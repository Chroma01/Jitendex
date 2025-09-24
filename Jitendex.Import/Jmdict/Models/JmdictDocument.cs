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

using System.ComponentModel.DataAnnotations.Schema;

namespace Jitendex.Import.Jmdict.Models;

[NotMapped]
public class JmdictDocument
{
    public required DateOnly Date { get; init; }

    public required List<Corpus> Corpora { get; init; }
    public required List<Entry> Entries { get; init; }

    public required List<PriorityTag> PriorityTags { get; init; }
    public required List<ReadingInfoTag> ReadingInfoTags { get; init; }
    public required List<KanjiFormInfoTag> KanjiFormInfoTags { get; init; }

    public required List<PartOfSpeechTag> PartOfSpeechTags { get; init; }
    public required List<FieldTag> FieldTags { get; init; }
    public required List<MiscTag> MiscTags { get; init; }
    public required List<DialectTag> DialectTags { get; init; }

    public required List<GlossType> GlossTypes { get; init; }
    public required List<CrossReferenceType> CrossReferenceTypes { get; init; }
    public required List<LanguageSourceType> LanguageSourceTypes { get; init; }
    public required List<Language> Languages { get; init; }

    public required List<ExampleSourceType> ExampleSourceTypes { get; init; }
    public required List<ExampleSource> ExampleSources { get; init; }
}
