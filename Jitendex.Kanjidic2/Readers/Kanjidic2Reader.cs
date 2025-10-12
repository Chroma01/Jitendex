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

    public async Task<Kanjidic2Document> ReadAsync()
    {
        var header = await _headerReader.ReadAsync();
        var entries = await _entriesReader.ReadAsync();

        var kanjidic2 = new Kanjidic2Document
        {
            Date = header.DateOfCreation,
            Entries = entries,
            CodepointTypes = [.. _docTypes.CodepointTypes()],
            DictionaryTypes = [.. _docTypes.DictionaryTypes()],
            QueryCodeTypes = [.. _docTypes.QueryCodeTypes()],
            MisclassificationTypes = [.. _docTypes.MisclassificationTypes()],
            RadicalTypes = [.. _docTypes.RadicalTypes()],
            ReadingType = [.. _docTypes.ReadingType()],
            VariantTypes = [.. _docTypes.VariantTypes()],
        };

        return kanjidic2;
    }
}
