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

using Jitendex.Kanjidic2.Database.EntryElements;
using Jitendex.Kanjidic2.Models;

namespace Jitendex.Kanjidic2.Database;

internal static class DatabaseInitializer
{
    #region Keyword tables
    private static readonly KeywordTable<CodepointType> CodepointTypeTable = new();
    private static readonly KeywordTable<DictionaryType> DictionaryTypeTable = new();
    private static readonly KeywordTable<QueryCodeType> QueryCodeTypeTable = new();
    private static readonly KeywordTable<MisclassificationType> MisclassificationTypeTable = new();
    private static readonly KeywordTable<RadicalType> RadicalTypeTable = new();
    private static readonly KeywordTable<ReadingType> ReadingTypeTable = new();
    private static readonly KeywordTable<VariantType> VariantTypeTable = new();
    #endregion

    #region Entry data tables
    private static readonly EntryTable EntryTable = new();
    private static readonly CodepointTable CodepointTable = new();
    private static readonly DictionaryTable DictionaryTable = new();
    private static readonly MeaningTable MeaningTable = new();
    private static readonly NanoriTable NanoriTable = new();
    private static readonly QueryCodeTable QueryCodeTable = new();
    private static readonly RadicalTable RadicalTable = new();
    private static readonly RadicalNameTable RadicalNameTable = new();
    private static readonly ReadingTable ReadingTable = new();
    private static readonly StrokeCountTable StrokeCountTable = new();
    private static readonly VariantTable VariantTable = new();
    #endregion

    public static async Task WriteAsync(Document document)
    {
        await using var context = new Context();

        // Delete and recreate the database file.
        await context.InitializeDatabaseAsync();

        // For faster importing, write data to memory rather than to the disk.
        await context.ExecuteFastNewDatabasePragmaAsync();

        // Begin inserting data.
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            await CodepointTypeTable.InsertItemsAsync(context, document.CodepointTypes.Values);
            await DictionaryTypeTable.InsertItemsAsync(context, document.DictionaryTypes.Values);
            await QueryCodeTypeTable.InsertItemsAsync(context, document.QueryCodeTypes.Values);
            await MisclassificationTypeTable.InsertItemsAsync(context, document.MisclassificationTypes.Values);
            await RadicalTypeTable.InsertItemsAsync(context, document.RadicalTypes.Values);
            await ReadingTypeTable.InsertItemsAsync(context, document.ReadingTypes.Values);
            await VariantTypeTable.InsertItemsAsync(context, document.VariantTypes.Values);

            await EntryTable.InsertItemsAsync(context, document.Entries.Values);
            await CodepointTable.InsertItemsAsync(context, document.Codepoints.Values);
            await DictionaryTable.InsertItemsAsync(context, document.Dictionaries.Values);
            await MeaningTable.InsertItemsAsync(context, document.Meanings.Values);
            await NanoriTable.InsertItemsAsync(context, document.Nanoris.Values);
            await QueryCodeTable.InsertItemsAsync(context, document.QueryCodes.Values);
            await RadicalTable.InsertItemsAsync(context, document.Radicals.Values);
            await RadicalNameTable.InsertItemsAsync(context, document.RadicalNames.Values);
            await ReadingTable.InsertItemsAsync(context, document.Readings.Values);
            await StrokeCountTable.InsertItemsAsync(context, document.StrokeCounts.Values);
            await VariantTable.InsertItemsAsync(context, document.Variants.Values);

            await transaction.CommitAsync();
        }

        // Write database to the disk.
        await context.SaveChangesAsync();

        // Rebuild the database compactly.
        await context.ExecuteVacuumAsync();
    }
}
