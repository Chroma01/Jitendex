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

using Jitendex.Kanjidic2.Import.Models;
using Jitendex.Kanjidic2.Import.Parsing;
using Jitendex.Kanjidic2.Import.SQLite;
using static Jitendex.EdrdgDictionaryArchive.DictionaryFile;
using static Jitendex.EdrdgDictionaryArchive.Service;

namespace Jitendex.Kanjidic2;

public static class Service
{
    public static async Task UpdateAsync(DirectoryInfo? archiveDirectory)
    {
        var previousDate = GetPreviousDate();
        var previousDocument = await GetPreviousDocumentAsync(previousDate, archiveDirectory);

        while (true)
        {
            var (nextFile, _) = GetNextEdrdgFile(kanjidic2, previousDocument.Header.Date, archiveDirectory);

            if (nextFile is null)
            {
                break;
            }

            var nextDocument = await ReadAsync(nextFile);
            var diff = new DocumentDiff(previousDocument, nextDocument);

            Database.Update(diff);

            previousDocument = nextDocument;
        }

        if (previousDate == default)
        {
            using var context = new Kanjidic2Context();
            context.ExecuteVacuum();
        }
    }

    private static DateOnly GetPreviousDate()
    {
        using var context = new Kanjidic2Context();
        context.Database.EnsureCreated();
        return context.FileHeaders
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
            var (file, _) = GetNextEdrdgFile(kanjidic2, previousDate, archiveDirectory);
            document = await ReadAsync(file!);
            Database.Initialize(document);
        }
        else
        {
            var file = GetEdrdgFile(examples, previousDate, archiveDirectory);
            document = await ReadAsync(file);
        }
        return document;
    }

    private static async Task<Document> ReadAsync(FileInfo file)
    {
        var reader = ReaderProvider.GetReader(file);
        var document = await reader.ReadAsync();
        return document;
    }
}
