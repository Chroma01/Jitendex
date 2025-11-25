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

using Jitendex.Kanjidic2.Models;

namespace Jitendex.Kanjidic2.Database;

internal static class DatabaseInitializer
{
    public static async Task WriteAsync(Kanjidic2Document kanjidic2)
    {
        await using var context = new Context();

        // Delete and recreate the database file.
        await context.InitializeDatabaseAsync();

        // For faster importing, write data to memory rather than to the disk.
        await context.ExecuteFastNewDatabasePragmaAsync();

        // Initialize table objects.
        var table1 = new KeywordTable<CodepointType>();
        var table2 = new KeywordTable<DictionaryType>();
        var table3 = new KeywordTable<QueryCodeType>();
        var table4 = new KeywordTable<MisclassificationType>();
        var table5 = new KeywordTable<RadicalType>();
        var table6 = new KeywordTable<ReadingType>();
        var table7 = new KeywordTable<VariantType>();

        // Begin inserting data.
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            await table1.InsertItemsAsync(context, kanjidic2.CodepointTypes);
            await table2.InsertItemsAsync(context, kanjidic2.DictionaryTypes);
            await table3.InsertItemsAsync(context, kanjidic2.QueryCodeTypes);
            await table4.InsertItemsAsync(context, kanjidic2.MisclassificationTypes);
            await table5.InsertItemsAsync(context, kanjidic2.RadicalTypes);
            await table6.InsertItemsAsync(context, kanjidic2.ReadingTypes);
            await table7.InsertItemsAsync(context, kanjidic2.VariantTypes);
            await context.InsertEntriesAsync(kanjidic2.Entries);

            await transaction.CommitAsync();
        }

        // Write database to the disk.
        await context.SaveChangesAsync();

        // Rebuild the database compactly.
        await context.ExecuteVacuumAsync();
    }
}
