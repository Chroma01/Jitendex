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

using System.Text.RegularExpressions;
using Jitendex.JMdict.Models;
using Jitendex.JMdict.Readers.DocumentTypes;

namespace Jitendex.JMdict.Readers;

internal partial class JmdictReader
{
    private readonly DocumentTypesReader _documentTypesReader;
    private readonly EntriesReader _entriesReader;
    private readonly CorpusCache _corpusCache;
    private readonly KeywordCache _keywordCache;
    private readonly ExampleCache _exampleCache;

    public JmdictReader(
        DocumentTypesReader documentTypesReader,
        EntriesReader entriesReader,
        CorpusCache corpusCache,
        KeywordCache keywordCache,
        ExampleCache exampleCache) =>
        (_documentTypesReader, _entriesReader, _corpusCache, _keywordCache, _exampleCache) =
        (@documentTypesReader, @entriesReader, @corpusCache, @keywordCache, @exampleCache);

    public async Task<JmdictDocument> ReadAsync()
    {
        await _documentTypesReader.ReadAsync();
        var entries = await _entriesReader.ReadAsync();

        var jmdict = new JmdictDocument
        {
            Date = GetDate(_corpusCache.Corpora),
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

    private static DateOnly GetDate(IEnumerable<Corpus> corpora)
    {
        var dateGlossText = corpora
            .Where(static corpus => corpus.Id == CorpusId.Metadata).FirstOrDefault()?
            .Entries.FirstOrDefault()?
            .Senses.FirstOrDefault()?
            .Glosses.FirstOrDefault()?
            .Text;

        if (dateGlossText is null)
        {
            return default;
        }

        Match match = DateOnlyRegex().Match(dateGlossText);

        if (!match.Success)
        {
            return default;
        }

        int year = int.Parse(match.Groups[1].Value);
        int month = int.Parse(match.Groups[2].Value);
        int day = int.Parse(match.Groups[3].Value);

        return new DateOnly(year, month, day);
    }


    [GeneratedRegex(@"(\d{4})-(\d{2})-(\d{2})", RegexOptions.None)]
    private static partial Regex DateOnlyRegex();
}
