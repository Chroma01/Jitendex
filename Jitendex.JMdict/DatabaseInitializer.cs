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
using Jitendex.JMdict.Database;
using Jitendex.JMdict.Models;

namespace Jitendex.JMdict;

internal static class DatabaseInitializer
{
    public static async Task WriteAsync(JmdictDocument jmdict)
    {
        await using var db = new Context();

        // Delete and recreate the database file.
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        // For faster importing, write data to memory rather than to the disk.
        await db.Database.ExecuteSqlRawAsync
        (
            """
            PRAGMA synchronous = FALSE;
            PRAGMA journal_mode = FALSE;
            PRAGMA temp_store = MEMORY;
            PRAGMA cache_size = -200000;
            PRAGMA locking_mode = EXCLUSIVE;
            """
        );

        // Using a transaction decreases the runtime by 10 seconds.
        // Using multiple smaller transactions doesn't seem to improve upon that.
        await using var transaction = await db.Database.BeginTransactionAsync();

        // Begin inserting data.
        await db.InsertCorporaAsync(jmdict.Corpora);
        await db.InsertKeywordsAsync(jmdict.CrossReferenceTypes);
        await db.InsertKeywordsAsync(jmdict.DialectTags);
        await db.InsertKeywordsAsync(jmdict.ExampleSourceTypes);
        await db.InsertKeywordsAsync(jmdict.FieldTags);
        await db.InsertKeywordsAsync(jmdict.GlossTypes);
        await db.InsertKeywordsAsync(jmdict.KanjiFormInfoTags);
        await db.InsertKeywordsAsync(jmdict.Languages);
        await db.InsertKeywordsAsync(jmdict.LanguageSourceTypes);
        await db.InsertKeywordsAsync(jmdict.MiscTags);
        await db.InsertKeywordsAsync(jmdict.PartOfSpeechTags);
        await db.InsertKeywordsAsync(jmdict.PriorityTags);
        await db.InsertKeywordsAsync(jmdict.ReadingInfoTags);
        await db.InsertExampleSourcesAsync(jmdict.ExampleSources);
        await db.InsertEntries(jmdict.Entries);

        await transaction.CommitAsync();

        // Write database to the disk.
        await db.SaveChangesAsync();

        // Rebuild the database compactly.
        await db.Database.ExecuteSqlRawAsync("VACUUM;");
    }
}
