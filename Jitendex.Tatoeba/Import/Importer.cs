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

using Jitendex.Tatoeba.Import.Parsing;
using Jitendex.Tatoeba.Import.Models;
using static Jitendex.EdrdgDictionaryArchive.DictionaryFile;
using static Jitendex.EdrdgDictionaryArchive.Service;

namespace Jitendex.Tatoeba.Import;

internal sealed class Importer
{
    private readonly TatoebaReader _reader;
    private readonly TatoebaContext _context;
    private readonly Database _database;

    public Importer(TatoebaReader reader, TatoebaContext context, Database database) =>
        (_reader, _context, _database) =
        (@reader, @context, @database);

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
        var (file, date) = GetNextEdrdgFile(examples, default, archiveDirectory);
        var document = await _reader.ReadAsync(file!, date);
        _database.Initialize(document);
        _context.ExecuteVacuum();
        return document;
    }

    private async Task<Document> GetPreviousDocumentAsync(DirectoryInfo? archiveDirectory, DateOnly previousDate)
    {
        var file = GetEdrdgFile(examples, previousDate, archiveDirectory);
        var document = await _reader.ReadAsync(file, previousDate);
        return document;
    }

    private async Task UpdateDatabaseAsync(DirectoryInfo? archiveDirectory, Document previousDocument)
    {
        while (true)
        {
            var (nextFile, nextDate) = GetNextEdrdgFile(examples, previousDocument.Header.Date, archiveDirectory);

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
