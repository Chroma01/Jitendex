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

using Jitendex.JMdict.Database;
using Jitendex.JMdict.Models;

namespace Jitendex.JMdict;

internal static class DatabaseInitializer
{
    public static async Task WriteAsync(JmdictDocument jmdict)
    {
        await using var context = new Context();

        // Delete and recreate the database file.
        await context.InitializeDatabaseAsync();

        // For faster importing, write data to memory rather than to the disk.
        await context.ExecuteFastNewDatabasePragmaAsync();

        // Using a transaction decreases the runtime by 10 seconds.
        // Using multiple smaller transactions doesn't seem to improve upon that.
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Begin inserting data.
        await context.InsertCorporaAsync(jmdict.Corpora);
        await context.InsertKeywordsAsync(jmdict.CrossReferenceTypes);
        await context.InsertKeywordsAsync(jmdict.DialectTags);
        await context.InsertKeywordsAsync(jmdict.ExampleSourceTypes);
        await context.InsertKeywordsAsync(jmdict.FieldTags);
        await context.InsertKeywordsAsync(jmdict.GlossTypes);
        await context.InsertKeywordsAsync(jmdict.KanjiFormInfoTags);
        await context.InsertKeywordsAsync(jmdict.Languages);
        await context.InsertKeywordsAsync(jmdict.LanguageSourceTypes);
        await context.InsertKeywordsAsync(jmdict.MiscTags);
        await context.InsertKeywordsAsync(jmdict.PartOfSpeechTags);
        await context.InsertKeywordsAsync(jmdict.PriorityTags);
        await context.InsertKeywordsAsync(jmdict.ReadingInfoTags);
        await context.InsertExampleSourcesAsync(jmdict.ExampleSources);
        await context.InsertEntries(jmdict.Entries);

        await transaction.CommitAsync();

        // Write database to the disk.
        await context.SaveChangesAsync();

        // Rebuild the database compactly.
        await context.ExecuteVacuumAsync();
    }
}
