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

using Jitendex.Import.Jmdict.Models;
using Jitendex.Import.Jmdict.Readers.DocumentTypes;

namespace Jitendex.Import.Jmdict.Readers;

internal class JmdictReader
{
    private readonly DocumentTypesReader _documentTypesReader;
    private readonly EntriesReader _entriesReader;
    private readonly CorpusCache _corpusCache;
    private readonly KeywordCache _keywordCache;
    private readonly ExampleCache _exampleCache;

    public JmdictReader(DocumentTypesReader documentTypesReader, EntriesReader entriesReader, CorpusCache corpusCache, KeywordCache keywordCache, ExampleCache exampleCache) =>
        (_documentTypesReader, _entriesReader, _corpusCache, _keywordCache, _exampleCache) =
        (@documentTypesReader, @entriesReader, @corpusCache, @keywordCache, @exampleCache);

    public async Task<JmdictDocument> ReadJmdictAsync()
    {
        await _documentTypesReader.ReadAsync();
        var entries = await _entriesReader.ReadAsync();

        var jmdict = new JmdictDocument
        {
            Corpora = [.. _corpusCache.Corpora],
            Entries = entries,
            PriorityTags = [.. _keywordCache.PriorityTags()],
            ReadingInfoTags = [.. _keywordCache.ReadingInfoTags()],
            KanjiFormInfoTags = [.. _keywordCache.KanjiFormInfoTags()],
            PartOfSpeechTags = [.. _keywordCache.PartOfSpeechTags()],
            FieldTags = [.. _keywordCache.FieldTags()],
            MiscTags = [.. _keywordCache.MiscTags()],
            DialectTags = [.. _keywordCache.DialectTags()],
            GlossTypes = [.. _keywordCache.GlossTypes()],
            CrossReferenceTypes = [.. _keywordCache.CrossReferenceTypes()],
            LanguageSourceTypes = [.. _keywordCache.LanguageSourceTypes()],
            Languages = [.. _keywordCache.Languages()],
            ExampleSourceTypes = [.. _keywordCache.ExampleSourceTypes()],
            ExampleSources = [.. _exampleCache.ExampleSources],
        };

        return jmdict;
    }
}
