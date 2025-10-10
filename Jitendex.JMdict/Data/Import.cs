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

using Microsoft.EntityFrameworkCore;
using Jitendex.JMdict.Models;

namespace Jitendex.JMdict.Data;

internal static class Import
{
    private const string ImportPragmaCommandText =
        """
        PRAGMA synchronous = FALSE;
        PRAGMA journal_mode = MEMORY;
        PRAGMA temp_store = MEMORY;
        PRAGMA cache_size = -200000;
        """;

    public static async Task ImportDocumentAsync(JmdictDocument jmdictDocument)
    {
        await using var db = new JmdictContext();

        // Delete and recreate the database file.
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        // Keep a single connection open for all following commands.
        await using var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();

        // For faster importing, write data to memory rather than to the disk.
        await db.Database.ExecuteSqlRawAsync(ImportPragmaCommandText);

        // Entries contain references to other entries, so the foreign key
        // constraints generally won't be satisfied until all entries are loaded.
        using var transaction = await connection.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("PRAGMA defer_foreign_keys = TRUE;");

        // Begin inserting data.
        await db.InsertCorporaAsync(jmdictDocument.Corpora);
        await db.InsertKeywordsAsync(jmdictDocument.CrossReferenceTypes);
        await db.InsertKeywordsAsync(jmdictDocument.DialectTags);
        await db.InsertKeywordsAsync(jmdictDocument.ExampleSourceTypes);
        await db.InsertKeywordsAsync(jmdictDocument.FieldTags);
        await db.InsertKeywordsAsync(jmdictDocument.GlossTypes);
        await db.InsertKeywordsAsync(jmdictDocument.KanjiFormInfoTags);
        await db.InsertKeywordsAsync(jmdictDocument.Languages);
        await db.InsertKeywordsAsync(jmdictDocument.LanguageSourceTypes);
        await db.InsertKeywordsAsync(jmdictDocument.MiscTags);
        await db.InsertKeywordsAsync(jmdictDocument.PartOfSpeechTags);
        await db.InsertKeywordsAsync(jmdictDocument.PriorityTags);
        await db.InsertKeywordsAsync(jmdictDocument.ReadingInfoTags);
        await db.InsertExampleSourcesAsync(jmdictDocument.ExampleSources);
        await db.InsertEntries(jmdictDocument.Entries);

        await transaction.CommitAsync();

        // Write database to the disk.
        await db.SaveChangesAsync();
    }
}
