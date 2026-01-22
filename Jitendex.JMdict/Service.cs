/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

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

using Microsoft.EntityFrameworkCore;
using Jitendex.JMdict.Import.Models;
using Jitendex.JMdict.Import.Parsing;
using Jitendex.JMdict.Import.SQLite;
using static Jitendex.EdrdgDictionaryArchive.DictionaryFile;
using static Jitendex.EdrdgDictionaryArchive.Service;

namespace Jitendex.JMdict;

public static class Service
{
    public static async Task UpdateAsync(DirectoryInfo? archiveDirectory)
    {
        var previousDate = (DateOnly)default; // GetPreviousDate();
        var previousDocument = await GetPreviousDocumentAsync(previousDate, archiveDirectory);

        // while (true)
        // {
        //     var (nextFile, _) = GetNextEdrdgFile(kanjidic2, previousDocument.FileHeader.Date, archiveDirectory);

        //     if (nextFile is null)
        //     {
        //         break;
        //     }

        //     var nextDocument = await ReadAsync(nextFile);
        //     var diff = new DocumentDiff(previousDocument, nextDocument);

        //     Database.Update(diff);

        //     previousDocument = nextDocument;
        // }

        if (previousDate == default)
        {
            using var context = new Context();
            context.ExecuteVacuum();
        }
    }

    private static DateOnly GetPreviousDate()
    {
        using var context = new Context();
        context.Database.EnsureCreated();
        return context.FileHeaders
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Take(1)
            .Select(x => x.Date)
            .FirstOrDefault();
    }

    private static async Task<Document> GetPreviousDocumentAsync(DateOnly previousDate, DirectoryInfo? archiveDirectory)
    {
        Document document;
        if (previousDate == default)
        {
            var (file, date) = GetNextEdrdgFile(JMdict_e_examp, previousDate, archiveDirectory);
            document = await ReadAsync(file!, date);
            Database.Initialize(document);
        }
        else
        {
            var file = GetEdrdgFile(JMdict_e_examp, previousDate, archiveDirectory);
            document = await ReadAsync(file, previousDate);
        }
        return document;
    }

    private static async Task<Document> ReadAsync(FileInfo file, DateOnly date)
    {
        var reader = ReaderProvider.GetReader(file);
        var document = await reader.ReadAsync(date);
        return document;
    }
}
