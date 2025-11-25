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

using Jitendex.Kanjidic2.Models;

namespace Jitendex.Kanjidic2.Readers;

internal class Kanjidic2Reader
{
    private readonly HeaderReader _headerReader;
    private readonly EntriesReader _entriesReader;
    private readonly DocumentTypes _docTypes;

    public Kanjidic2Reader(HeaderReader headerReader, EntriesReader entriesReader, DocumentTypes docTypes) =>
        (_headerReader, _entriesReader, _docTypes) =
        (@headerReader, @entriesReader, @docTypes);

    public async Task<Document> ReadAsync()
    {
        var header = await _headerReader.ReadAsync();
        var entries = await _entriesReader.ReadAsync();

        var document = new Document
        {
            Date = header.DateOfCreation,
            CodepointTypes = _docTypes.CodepointTypes(),
            DictionaryTypes = _docTypes.DictionaryTypes(),
            QueryCodeTypes = _docTypes.QueryCodeTypes(),
            MisclassificationTypes = _docTypes.MisclassificationTypes(),
            RadicalTypes = _docTypes.RadicalTypes(),
            ReadingTypes = _docTypes.ReadingTypes(),
            VariantTypes = _docTypes.VariantTypes(),
        };

        foreach (var entry in entries)
        {
            document.Entries.Add(entry.UnicodeScalarValue, entry);
            foreach (var codepoint in entry.Codepoints)
                document.Codepoints.Add(codepoint.Key, codepoint);
            foreach (var dictionary in entry.Dictionaries)
                document.Dictionaries.Add(dictionary.Key, dictionary);
            foreach(var meaning in entry.Meanings)
                document.Meanings.Add(meaning.Key, meaning);
            foreach(var nanori in entry.Nanoris)
                document.Nanoris.Add(nanori.Key, nanori);
            foreach(var queryCode in entry.QueryCodes)
                document.QueryCodes.Add(queryCode.Key, queryCode);
            foreach(var radical in entry.Radicals)
                document.Radicals.Add(radical.Key, radical);
            foreach(var radicalName in entry.RadicalNames)
                document.RadicalNames.Add(radicalName.Key, radicalName);
            foreach(var reading in entry.Readings)
                document.Readings.Add(reading.Key, reading);
            foreach(var strokeCount in entry.StrokeCounts)
                document.StrokeCounts.Add(strokeCount.Key, strokeCount);
            foreach(var variant in entry.Variants)
                document.Variants.Add(variant.Key, variant);
        }

        return document;
    }
}
