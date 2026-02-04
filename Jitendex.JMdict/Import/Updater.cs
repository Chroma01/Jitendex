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
using Jitendex.JMdict.Import.Models;
using Jitendex.JMdict.Import.Parsing;
using static Jitendex.EdrdgDictionaryArchive.DictionaryFile;
using static Jitendex.EdrdgDictionaryArchive.Service;
using Jitendex.JMdict.Import.Analysis;

namespace Jitendex.JMdict.Import;

internal sealed class Updater
{
    private readonly ILogger<Updater> _logger;
    private readonly JmdictContext _context;
    private readonly DocumentReader _reader;
    private readonly Database _database;
    private readonly Analyzer _analyzer;

    public Updater(ILogger<Updater> logger, JmdictContext context, DocumentReader reader, Database database, Analyzer analyzer) =>
        (_logger, _context, _reader, _database, _analyzer) =
        (@logger, @context, @reader, @database, @analyzer);

    public async Task UpdateAsync(DirectoryInfo? archiveDirectory)
    {
        _context.Database.EnsureCreated();
        await UpdateJmdictDatabaseAsync(archiveDirectory);
        _analyzer.Analyze();
    }

    private async Task UpdateJmdictDatabaseAsync(DirectoryInfo? archiveDirectory)
    {
        var previousDate = GetPreviousDate();
        var previousDocument = await GetPreviousDocumentAsync(previousDate, archiveDirectory);

        while (true)
        {
            var (nextFile, nextDate) = GetNextEdrdgFile(JMdict_e_examp, previousDocument.Header.Date, archiveDirectory);

            if (nextFile is null)
            {
                break;
            }

            var nextDocument = await _reader.ReadAsync(nextFile, nextDate);
            var diff = new DocumentDiff(previousDocument, nextDocument);

            _database.Update(diff);

            previousDocument = nextDocument;
        }

        if (previousDate == default)
        {
            using var context = new JmdictContext();
            context.ExecuteVacuum();
        }
    }

    private DateOnly GetPreviousDate() => _context.FileHeaders
        .OrderByDescending(static x => x.Id)
        .Take(1)
        .Select(static x => x.Date)
        .FirstOrDefault();

    private async Task<Document> GetPreviousDocumentAsync(DateOnly previousDate, DirectoryInfo? archiveDirectory)
    {
        Document document;
        if (previousDate == default)
        {
            var (file, date) = GetNextEdrdgFile(JMdict_e_examp, previousDate, archiveDirectory);
            document = await _reader.ReadAsync(file!, date);
            _database.Initialize(document);
        }
        else
        {
            var file = GetEdrdgFile(JMdict_e_examp, previousDate, archiveDirectory);
            document = await _reader.ReadAsync(file, previousDate);
        }
        return document;
    }
}
