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

using Jitendex.Tatoeba.Models;

namespace Jitendex.Tatoeba.Database;

internal static class DatabaseInitializer
{
    public static async Task WriteAsync(Document document)
    {
        await using var context = new Context();

        // Delete and recreate the database file.
        await context.InitializeDatabaseAsync();

        // For faster importing, write data to memory rather than to the disk.
        await context.ExecuteFastNewDatabasePragmaAsync();

        // Initialize table objects.
        var jTable = new JapaneseSentenceTable();
        var sTable = new EnglishSentenceTable();
        var iTable = new SentenceIndexTable();
        var eTable = new IndexElementTable();

        // Begin inserting data.
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            await jTable.InsertItemsAsync(context, document.JapaneseSentences.Values);
            await sTable.InsertItemsAsync(context, document.EnglishSentences.Values);
            await iTable.InsertItemsAsync(context, document.SentenceIndices.Values);
            await eTable.InsertItemsAsync(context, document.IndexElements.Values);
            await transaction.CommitAsync();
        }

        // Write database to the disk.
        await context.SaveChangesAsync();

        // Rebuild the database compactly.
        await context.ExecuteVacuumAsync();
    }
}
