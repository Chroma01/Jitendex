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

using Jitendex.JMdict.Import.Models;
using Jitendex.JMdict.Import.SQLite.EntryElements;
using Jitendex.JMdict.Import.SQLite.EntryElements.KanjiFormElements;
using Jitendex.JMdict.Import.SQLite.EntryElements.ReadingElements;
using Jitendex.JMdict.Import.SQLite.EntryElements.SenseElements;

namespace Jitendex.JMdict.Import.SQLite;

internal static class Database
{
    private static readonly FileHeaderTable FileHeaderTable = new();
    private static readonly EntryTable EntryTable = new();

    #region Entry Element Tables
    private static readonly KanjiFormTable KanjiFormTable = new();
    private static readonly ReadingTable ReadingTable = new();
    private static readonly SenseTable SenseTable = new();
    #endregion

    #region Kanji Form Element Tables
    private static readonly KanjiFormInfoTable KanjiFormInfoTable = new();
    private static readonly KanjiFormPriorityTable KanjiFormPriorityTable = new();
    #endregion

    #region Reading Element Tables
    private static readonly ReadingInfoTable ReadingInfoTable = new();
    private static readonly ReadingPriorityTable ReadingPriorityTable = new();
    private static readonly RestrictionTable RestrictionTable = new();
    #endregion

    #region Sense Element Tables
    private static readonly CrossReferenceTable CrossReferenceTable = new();
    private static readonly DialectTable DialectTable = new();
    private static readonly FieldTable FieldTable = new();
    private static readonly GlossTable GlossTable = new();
    private static readonly KanjiFormRestrictionTable KanjiFormRestrictionTable = new();
    private static readonly LanguageSourceTable LanguageSourceTable = new();
    private static readonly MiscTable MiscTable = new();
    private static readonly PartOfSpeechTable PartOfSpeechTable = new();
    private static readonly ReadingRestrictionTable ReadingRestrictionTable = new();
    #endregion

    #region Keyword Tables
    private static readonly KeywordTable<ReadingInfoTag> ReadingInfoTagTable = new();
    private static readonly KeywordTable<KanjiFormInfoTag> KanjiFormInfoTagTable = new();
    private static readonly KeywordTable<PartOfSpeechTag> PartOfSpeechTagTable = new();
    private static readonly KeywordTable<FieldTag> FieldTagTable = new();
    private static readonly KeywordTable<MiscTag> MiscTagTable = new();
    private static readonly KeywordTable<DialectTag> DialectTagTable = new();
    private static readonly KeywordTable<GlossType> GlossTypeTable = new();
    private static readonly KeywordTable<CrossReferenceType> CrossReferenceTypeTable = new();
    private static readonly KeywordTable<LanguageSourceType> LanguageSourceTypeTable = new();
    private static readonly KeywordTable<PriorityTag> PriorityTagTable = new();
    private static readonly KeywordTable<Language> LanguageTable = new();
    #endregion

    public static void Initialize(Document document)
    {
        using var context = new Context();
        context.InitializeDatabase();
        context.ExecuteFastNewDatabasePragma();

        using (var transaction = context.Database.BeginTransaction())
        {
            FileHeaderTable.InsertItem(context, document.FileHeader);

            ReadingInfoTagTable.InsertItems(context, document.ReadingInfoTags.Values);
            KanjiFormInfoTagTable.InsertItems(context, document.KanjiFormInfoTags.Values);
            PartOfSpeechTagTable.InsertItems(context, document.PartOfSpeechTags.Values);
            FieldTagTable.InsertItems(context, document.FieldTags.Values);
            MiscTagTable.InsertItems(context, document.MiscTags.Values);
            DialectTagTable.InsertItems(context, document.DialectTags.Values);
            GlossTypeTable.InsertItems(context, document.GlossTypes.Values);
            CrossReferenceTypeTable.InsertItems(context, document.CrossReferenceTypes.Values);
            LanguageSourceTypeTable.InsertItems(context, document.LanguageSourceTypes.Values);
            PriorityTagTable.InsertItems(context, document.PriorityTags.Values);
            LanguageTable.InsertItems(context, document.Languages.Values);

            EntryTable.InsertItems(context, document.Entries.Values);

            KanjiFormTable.InsertItems(context, document.KanjiForms.Values);
            ReadingTable.InsertItems(context, document.Readings.Values);
            SenseTable.InsertItems(context, document.Senses.Values);

            KanjiFormInfoTable.InsertItems(context, document.KanjiFormInfos.Values);
            KanjiFormPriorityTable.InsertItems(context, document.KanjiFormPriorities.Values);

            ReadingInfoTable.InsertItems(context, document.ReadingInfos.Values);
            ReadingPriorityTable.InsertItems(context, document.ReadingPriorities.Values);
            RestrictionTable.InsertItems(context, document.Restrictions.Values);

            CrossReferenceTable.InsertItems(context, document.CrossReferences.Values);
            DialectTable.InsertItems(context, document.Dialects.Values);
            FieldTable.InsertItems(context, document.Fields.Values);
            GlossTable.InsertItems(context, document.Glosses.Values);
            KanjiFormRestrictionTable.InsertItems(context, document.KanjiFormRestrictions.Values);
            LanguageSourceTable.InsertItems(context, document.LanguageSources.Values);
            MiscTable.InsertItems(context, document.Miscs.Values);
            PartOfSpeechTable.InsertItems(context, document.PartsOfSpeech.Values);
            ReadingRestrictionTable.InsertItems(context, document.ReadingRestrictions.Values);

            transaction.Commit();
        }

        context.SaveChanges();
    }
}
