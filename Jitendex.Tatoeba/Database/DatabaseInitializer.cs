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
    public static async Task WriteAsync(List<SentenceIndex> indices)
    {
        await using var context = new Context();

        // Delete and recreate the database file.
        await context.InitializeDatabaseAsync();

        // For faster importing, write data to memory rather than to the disk.
        await context.ExecuteFastNewDatabasePragmaAsync();

        // Wait until all data is imported before checking foreign key constraints.
        await context.ExecuteDeferForeignKeysPragmaAsync();

        // Initialize table objects.
        SentenceIndexTable siTable = new();
        EnglishSentenceTable esTable = new();
        JapaneseSentenceTable jsTable = new();
        IndexElementTable ieTable = new();

        // Begin inserting data.
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            await siTable.InsertItemsAsync(context, indices);
            await esTable.InsertItemsAsync(context, indices.GetAllEnglishSentences());
            await jsTable.InsertItemsAsync(context, indices.GetAllJapaneseSentences());
            await ieTable.InsertItemsAsync(context, indices.GetAllIndexElements());
            await transaction.CommitAsync();
        }

        // Write database to the disk.
        await context.SaveChangesAsync();

        // Rebuild the database compactly.
        await context.ExecuteVacuumAsync();
    }

    private static IEnumerable<EnglishSentence> GetAllEnglishSentences(this List<SentenceIndex> indices)
    {
        Dictionary<int, EnglishSentence> all = new(indices.Count);
        foreach (var index in indices)
        {
            if (!all.ContainsKey(index.MeaningId))
            {
                all[index.MeaningId] = index.Meaning;
            }
        }
        return all.Values;
    }

    private static IEnumerable<JapaneseSentence> GetAllJapaneseSentences(this List<SentenceIndex> indices)
    {
        Dictionary<int, JapaneseSentence> all = new(indices.Count);
        foreach (var index in indices)
        {
            if (!all.ContainsKey(index.SentenceId))
            {
                all[index.SentenceId] = index.Sentence;
            }
        }
        return all.Values;
    }

    private static IEnumerable<IndexElement> GetAllIndexElements(this List<SentenceIndex> indices)
        => indices.SelectMany(static i => i.Elements);
}
