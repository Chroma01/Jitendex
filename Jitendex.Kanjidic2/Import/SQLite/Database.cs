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
using Jitendex.Kanjidic2.Import.SQLite.Groups;
using Jitendex.Kanjidic2.Import.SQLite.GroupElements;
using Jitendex.Kanjidic2.Import.SQLite.SubgroupElements;

namespace Jitendex.Kanjidic2.Import.SQLite;

internal static class Database
{
    private static readonly FileHeaderTable FileHeaderTable = new();
    private static readonly EntryTable EntryTable = new();

    #region Keyword tables
    private static readonly KeywordTable<CodepointType> CodepointTypeTable = new();
    private static readonly KeywordTable<DictionaryType> DictionaryTypeTable = new();
    private static readonly KeywordTable<QueryCodeType> QueryCodeTypeTable = new();
    private static readonly KeywordTable<MisclassificationType> MisclassificationTypeTable = new();
    private static readonly KeywordTable<RadicalType> RadicalTypeTable = new();
    private static readonly KeywordTable<ReadingType> ReadingTypeTable = new();
    private static readonly KeywordTable<VariantType> VariantTypeTable = new();
    #endregion

    #region Group Tables
    private static readonly CodepointGroupTable CodepointGroupTable = new();
    private static readonly DictionaryGroupTable DictionaryGroupTable = new();
    private static readonly MiscGroupTable MiscGroupTable = new();
    private static readonly QueryCodeGroupTable QueryCodeGroupTable = new();
    private static readonly RadicalGroupTable RadicalGroupTable = new();
    private static readonly ReadingMeaningGroupTable ReadingMeaningGroupTable = new();
    #endregion

    #region Group Element Tables
    private static readonly CodepointTable CodepointTable = new();
    private static readonly DictionaryTable DictionaryTable = new();
    private static readonly NanoriTable NanoriTable = new();
    private static readonly QueryCodeTable QueryCodeTable = new();
    private static readonly RadicalTable RadicalTable = new();
    private static readonly RadicalNameTable RadicalNameTable = new();
    private static readonly ReadingMeaningTable ReadingMeaningTable = new();
    private static readonly StrokeCountTable StrokeCountTable = new();
    private static readonly VariantTable VariantTable = new();
    #endregion

    #region Subgroup Element Tables
    private static readonly MeaningTable MeaningTable = new();
    private static readonly ReadingTable ReadingTable = new();
    #endregion

    public static void Initialize(Document document)
    {
        using var context = new Context();
        context.InitializeDatabase();
        context.ExecuteFastNewDatabasePragma();

        using (var transaction = context.Database.BeginTransaction())
        {
            FileHeaderTable.InsertItem(context, document.FileHeader);

            CodepointTypeTable.InsertItems(context, document.CodepointTypes.Values);
            DictionaryTypeTable.InsertItems(context, document.DictionaryTypes.Values);
            QueryCodeTypeTable.InsertItems(context, document.QueryCodeTypes.Values);
            MisclassificationTypeTable.InsertItems(context, document.MisclassificationTypes.Values);
            RadicalTypeTable.InsertItems(context, document.RadicalTypes.Values);
            ReadingTypeTable.InsertItems(context, document.ReadingTypes.Values);
            VariantTypeTable.InsertItems(context, document.VariantTypes.Values);

            EntryTable.InsertItems(context, document.Entries.Values);

            CodepointGroupTable.InsertItems(context, document.CodepointGroups.Values);
            DictionaryGroupTable.InsertItems(context, document.DictionaryGroups.Values);
            MiscGroupTable.InsertItems(context, document.MiscGroups.Values);
            QueryCodeGroupTable.InsertItems(context, document.QueryCodeGroups.Values);
            RadicalGroupTable.InsertItems(context, document.RadicalGroups.Values);
            ReadingMeaningGroupTable.InsertItems(context, document.ReadingMeaningGroups.Values);

            CodepointTable.InsertItems(context, document.Codepoints.Values);
            DictionaryTable.InsertItems(context, document.Dictionaries.Values);
            NanoriTable.InsertItems(context, document.Nanoris.Values);
            QueryCodeTable.InsertItems(context, document.QueryCodes.Values);
            RadicalTable.InsertItems(context, document.Radicals.Values);
            RadicalNameTable.InsertItems(context, document.RadicalNames.Values);
            ReadingMeaningTable.InsertItems(context, document.ReadingMeanings.Values);
            StrokeCountTable.InsertItems(context, document.StrokeCounts.Values);
            VariantTable.InsertItems(context, document.Variants.Values);

            MeaningTable.InsertItems(context, document.Meanings.Values);
            ReadingTable.InsertItems(context, document.Readings.Values);

            transaction.Commit();
        }

        context.SaveChanges();
    }
}
