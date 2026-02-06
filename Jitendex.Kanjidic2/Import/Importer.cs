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

using Microsoft.Extensions.Logging;
using Jitendex.EdrdgDictionaryArchive;
using Jitendex.Kanjidic2.Import.Models;
using Jitendex.Kanjidic2.Import.Parsing;
using static Jitendex.EdrdgDictionaryArchive.DictionaryFile;

namespace Jitendex.Kanjidic2.Import;

internal sealed class Importer
{
    private readonly ILogger<Importer> _logger;
    private readonly IEdrdgArchiveService _fileArchive;
    private readonly Kanjidic2Context _context;
    private readonly Kanjidic2Reader _reader;
    private readonly Database _database;

    public Importer(ILogger<Importer> logger, IEdrdgArchiveService fileArchive, Kanjidic2Context context, Kanjidic2Reader reader, Database database) =>
        (_logger, _fileArchive, _context, _reader, _database) =
        (@logger, @fileArchive, @context, @reader, @database);

    public async Task ImportAsync(DirectoryInfo? archiveDirectory)
    {
        _context.Database.EnsureCreated();
        var previousDate = GetPreviousDate();

        var previousDocument = previousDate == default
            ? await InitializeDatabaseAsync(archiveDirectory)
            : await GetPreviousDocumentAsync(archiveDirectory, previousDate);

        await UpdateDatabaseAsync(archiveDirectory, previousDocument);
    }

    private DateOnly GetPreviousDate() => _context.FileHeaders
        .OrderByDescending(static x => x.Id)
        .Take(1)
        .Select(static x => x.Date)
        .FirstOrDefault();

    private async Task<Document> InitializeDatabaseAsync(DirectoryInfo? archiveDirectory)
    {
        var (file, date) = _fileArchive.GetNextFile(kanjidic2, default, archiveDirectory);
        var document = await _reader.ReadAsync(file!, date);
        _database.Initialize(document);
        _context.ExecuteVacuum();
        return document;
    }

    private async Task<Document> GetPreviousDocumentAsync(DirectoryInfo? archiveDirectory, DateOnly previousDate)
    {
        var file = _fileArchive.GetFile(kanjidic2, previousDate, archiveDirectory);
        var document = await _reader.ReadAsync(file, previousDate);
        return document;
    }

    private async Task UpdateDatabaseAsync(DirectoryInfo? archiveDirectory, Document previousDocument)
    {
        while (true)
        {
            var (nextFile, nextDate) = _fileArchive.GetNextFile(kanjidic2, previousDocument.Header.Date, archiveDirectory);

            if (nextFile is null)
            {
                break;
            }

            var nextDocument = await _reader.ReadAsync(nextFile, nextDate);
            var diff = new DocumentDiff(previousDocument, nextDocument);

            _database.Update(diff);

            previousDocument = nextDocument;
        }
    }
}
