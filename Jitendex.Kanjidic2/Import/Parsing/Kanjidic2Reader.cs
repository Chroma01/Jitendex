/*
Copyright (c) 2025-2026 Stephen Kraus
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

using System.IO.Compression;
using System.Xml;
using Jitendex.Kanjidic2.Import.Models;

namespace Jitendex.Kanjidic2.Import.Parsing;

internal class Kanjidic2Reader
{
    private readonly HeaderReader _headerReader;
    private readonly EntriesReader _entriesReader;

    public Kanjidic2Reader(HeaderReader headerReader, EntriesReader entriesReader) =>
        (_headerReader, _entriesReader) =
        (@headerReader, @entriesReader);

   private static readonly XmlReaderSettings XmlReaderSettings = new()
    {
        Async = true,
        DtdProcessing = DtdProcessing.Parse,
        MaxCharactersFromEntities = long.MaxValue,
        MaxCharactersInDocument = long.MaxValue,
    };

    public async Task<Document> ReadAsync(FileInfo file, DateOnly fileDate)
    {
        await using FileStream f = new(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
        await using BrotliStream b = new(f, CompressionMode.Decompress);
        using var xmlReader = XmlReader.Create(b, XmlReaderSettings);

        var document = new Document
        {
            Header = await _headerReader.ReadAsync(xmlReader)
        };

        await _entriesReader.ReadAsync(xmlReader, document);

        return document;
    }
}
