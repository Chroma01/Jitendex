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

using Microsoft.EntityFrameworkCore;
using Jitendex.MinimalJsonDiff;
using Jitendex.JMdict.Import.Models;
using Jitendex.JMdict.Import.SQLite.EntryElements;
using Jitendex.JMdict.Import.SQLite.EntryElements.KanjiFormElements;
using Jitendex.JMdict.Import.SQLite.EntryElements.ReadingElements;
using Jitendex.JMdict.Import.SQLite.EntryElements.SenseElements;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        Console.Error.WriteLine($"Initializing database with data from {document.FileHeader.Date:yyyy-MM-dd}");

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

    public static void Update(DocumentDiff diff)
    {
        Console.Error.WriteLine($"Updating {diff.EntryIds.Count} entries with data from {diff.FileHeader.Date:yyyy-MM-dd}");

        using var context = new Context();

        Console.Error.WriteLine($"Loading entries from database");
        var aEntries = LoadEntries(context, diff.EntryIds);

        Console.Error.WriteLine($"Beginning transaction");
        using (var transaction = context.Database.BeginTransaction())
        {
            context.ExecuteDeferForeignKeysPragma();

            FileHeaderTable.InsertItem(context, diff.InsertDocument.FileHeader);

            ReadingInfoTagTable.InsertOrIgnoreItems(context, diff.InsertDocument.ReadingInfoTags.Values);
            KanjiFormInfoTagTable.InsertOrIgnoreItems(context, diff.InsertDocument.KanjiFormInfoTags.Values);
            PartOfSpeechTagTable.InsertOrIgnoreItems(context, diff.InsertDocument.PartOfSpeechTags.Values);
            FieldTagTable.InsertOrIgnoreItems(context, diff.InsertDocument.FieldTags.Values);
            MiscTagTable.InsertOrIgnoreItems(context, diff.InsertDocument.MiscTags.Values);
            DialectTagTable.InsertOrIgnoreItems(context, diff.InsertDocument.DialectTags.Values);
            GlossTypeTable.InsertOrIgnoreItems(context, diff.InsertDocument.GlossTypes.Values);
            CrossReferenceTypeTable.InsertOrIgnoreItems(context, diff.InsertDocument.CrossReferenceTypes.Values);
            LanguageSourceTypeTable.InsertOrIgnoreItems(context, diff.InsertDocument.LanguageSourceTypes.Values);
            PriorityTagTable.InsertOrIgnoreItems(context, diff.InsertDocument.PriorityTags.Values);
            LanguageTable.InsertOrIgnoreItems(context, diff.InsertDocument.Languages.Values);

            EntryTable.InsertOrIgnoreItems(context, diff.InsertDocument.Entries.Values);

            KanjiFormTable.InsertItems(context, diff.InsertDocument.KanjiForms.Values);
            ReadingTable.InsertItems(context, diff.InsertDocument.Readings.Values);
            SenseTable.InsertItems(context, diff.InsertDocument.Senses.Values);
            KanjiFormInfoTable.InsertItems(context, diff.InsertDocument.KanjiFormInfos.Values);
            KanjiFormPriorityTable.InsertItems(context, diff.InsertDocument.KanjiFormPriorities.Values);
            ReadingInfoTable.InsertItems(context, diff.InsertDocument.ReadingInfos.Values);
            ReadingPriorityTable.InsertItems(context, diff.InsertDocument.ReadingPriorities.Values);
            RestrictionTable.InsertItems(context, diff.InsertDocument.Restrictions.Values);
            CrossReferenceTable.InsertItems(context, diff.InsertDocument.CrossReferences.Values);
            DialectTable.InsertItems(context, diff.InsertDocument.Dialects.Values);
            FieldTable.InsertItems(context, diff.InsertDocument.Fields.Values);
            GlossTable.InsertItems(context, diff.InsertDocument.Glosses.Values);
            KanjiFormRestrictionTable.InsertItems(context, diff.InsertDocument.KanjiFormRestrictions.Values);
            LanguageSourceTable.InsertItems(context, diff.InsertDocument.LanguageSources.Values);
            MiscTable.InsertItems(context, diff.InsertDocument.Miscs.Values);
            PartOfSpeechTable.InsertItems(context, diff.InsertDocument.PartsOfSpeech.Values);
            ReadingRestrictionTable.InsertItems(context, diff.InsertDocument.ReadingRestrictions.Values);

            KanjiFormTable.UpdateItems(context, diff.UpdateDocument.KanjiForms.Values);
            ReadingTable.UpdateItems(context, diff.UpdateDocument.Readings.Values);
            SenseTable.UpdateItems(context, diff.UpdateDocument.Senses.Values);
            KanjiFormInfoTable.UpdateItems(context, diff.UpdateDocument.KanjiFormInfos.Values);
            KanjiFormPriorityTable.UpdateItems(context, diff.UpdateDocument.KanjiFormPriorities.Values);
            ReadingInfoTable.UpdateItems(context, diff.UpdateDocument.ReadingInfos.Values);
            ReadingPriorityTable.UpdateItems(context, diff.UpdateDocument.ReadingPriorities.Values);
            RestrictionTable.UpdateItems(context, diff.UpdateDocument.Restrictions.Values);
            CrossReferenceTable.UpdateItems(context, diff.UpdateDocument.CrossReferences.Values);
            DialectTable.UpdateItems(context, diff.UpdateDocument.Dialects.Values);
            FieldTable.UpdateItems(context, diff.UpdateDocument.Fields.Values);
            GlossTable.UpdateItems(context, diff.UpdateDocument.Glosses.Values);
            KanjiFormRestrictionTable.UpdateItems(context, diff.UpdateDocument.KanjiFormRestrictions.Values);
            LanguageSourceTable.UpdateItems(context, diff.UpdateDocument.LanguageSources.Values);
            MiscTable.UpdateItems(context, diff.UpdateDocument.Miscs.Values);
            PartOfSpeechTable.UpdateItems(context, diff.UpdateDocument.PartsOfSpeech.Values);
            ReadingRestrictionTable.UpdateItems(context, diff.UpdateDocument.ReadingRestrictions.Values);

            ReadingRestrictionTable.DeleteItems(context, diff.DeleteDocument.ReadingRestrictions.Values);
            PartOfSpeechTable.DeleteItems(context, diff.DeleteDocument.PartsOfSpeech.Values);
            MiscTable.DeleteItems(context, diff.DeleteDocument.Miscs.Values);
            LanguageSourceTable.DeleteItems(context, diff.DeleteDocument.LanguageSources.Values);
            KanjiFormRestrictionTable.DeleteItems(context, diff.DeleteDocument.KanjiFormRestrictions.Values);
            GlossTable.DeleteItems(context, diff.DeleteDocument.Glosses.Values);
            FieldTable.DeleteItems(context, diff.DeleteDocument.Fields.Values);
            DialectTable.DeleteItems(context, diff.DeleteDocument.Dialects.Values);
            CrossReferenceTable.DeleteItems(context, diff.DeleteDocument.CrossReferences.Values);
            RestrictionTable.DeleteItems(context, diff.DeleteDocument.Restrictions.Values);
            ReadingPriorityTable.DeleteItems(context, diff.DeleteDocument.ReadingPriorities.Values);
            ReadingInfoTable.DeleteItems(context, diff.DeleteDocument.ReadingInfos.Values);
            KanjiFormPriorityTable.DeleteItems(context, diff.DeleteDocument.KanjiFormPriorities.Values);
            KanjiFormInfoTable.DeleteItems(context, diff.DeleteDocument.KanjiFormInfos.Values);
            SenseTable.DeleteItems(context, diff.DeleteDocument.Senses.Values);
            ReadingTable.DeleteItems(context, diff.DeleteDocument.Readings.Values);
            KanjiFormTable.DeleteItems(context, diff.DeleteDocument.KanjiForms.Values);

            transaction.Commit();
        }

        Console.Error.WriteLine($"Loading updated entries from database");
        var bEntries = LoadEntries(context, diff.EntryIds);

        var entries = context.Entries
            .Where(entry => diff.EntryIds.Contains(entry.Id))
            .Include(static entry => entry.Revisions)
            .ToList();

        foreach (var entry in entries)
        {
            if (aEntries.TryGetValue(entry.Id, out var aEntry))
            {
                var bEntry = bEntries[entry.Id];
                var baDiff = JsonDiffer.Diff(a: bEntry, b: aEntry);
                entry.Revisions.Add(new()
                {
                    EntryId = entry.Id,
                    Number = entry.Revisions.Count,
                    CreatedDate = diff.FileHeader.Date,
                    DiffJson = baDiff,
                    Entry = entry,
                });
            }
        }

        context.SaveChanges();
    }

    private static Dictionary<int, Entities.Entry> LoadEntries(Context context, IReadOnlySet<int> ids)
    {
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var entries = context.Entries
            .Where(entry => ids.Contains(entry.Id))
            .ToDictionary(static e => e.Id);

        var kanjiForms = context.KanjiForms
            .Where(k => ids.Contains(k.EntryId))
            .ToDictionary(static k => k.Key());

        var readings = context.Readings
            .Where(r => ids.Contains(r.EntryId))
            .ToDictionary(static r => r.Key());

        var senses = context.Senses
            .Where(s => ids.Contains(s.EntryId))
            .ToDictionary(static s => s.Key());

        // Entry Elements
        foreach (var kanjiForm in kanjiForms.Values)
            entries[kanjiForm.EntryId].KanjiForms.Add(kanjiForm);
        foreach (var reading in readings.Values)
            entries[reading.EntryId].Readings.Add(reading);
        foreach (var sense in senses.Values)
            entries[sense.EntryId].Senses.Add(sense);

        // Kanji Form Elements
        foreach (var x in context.KanjiFormInfos.Where(y => ids.Contains(y.EntryId)).ToList())
            kanjiForms[x.ParentKey()].Infos.Add(x);
        foreach (var x in context.KanjiFormPriorities.Where(y => ids.Contains(y.EntryId)).ToList())
            kanjiForms[x.ParentKey()].Priorities.Add(x);

        // Reading ELements
        foreach (var x in context.ReadingInfos.Where(y => ids.Contains(y.EntryId)).ToList())
            readings[x.ParentKey()].Infos.Add(x);
        foreach (var x in context.ReadingPriorities.Where(y => ids.Contains(y.EntryId)).ToList())
            readings[x.ParentKey()].Priorities.Add(x);
        foreach (var x in context.Restrictions.Where(y => ids.Contains(y.EntryId)).ToList())
            readings[x.ParentKey()].Restrictions.Add(x);

        // Sense Elements
        foreach (var x in context.CrossReferences.Where(y => ids.Contains(y.EntryId)).ToList())
            senses[x.ParentKey()].CrossReferences.Add(x);
        foreach (var x in context.Dialects.Where(y => ids.Contains(y.EntryId)).ToList())
            senses[x.ParentKey()].Dialects.Add(x);
        foreach (var x in context.Fields.Where(y => ids.Contains(y.EntryId)).ToList())
            senses[x.ParentKey()].Fields.Add(x);
        foreach (var x in context.Glosses.Where(y => ids.Contains(y.EntryId)).ToList())
            senses[x.ParentKey()].Glosses.Add(x);
        foreach (var x in context.KanjiFormRestrictions.Where(y => ids.Contains(y.EntryId)).ToList())
            senses[x.ParentKey()].KanjiFormRestrictions.Add(x);
        foreach (var x in context.LanguageSources.Where(y => ids.Contains(y.EntryId)).ToList())
            senses[x.ParentKey()].LanguageSources.Add(x);
        foreach (var x in context.Miscs.Where(y => ids.Contains(y.EntryId)).ToList())
            senses[x.ParentKey()].Miscs.Add(x);
        foreach (var x in context.PartsOfSpeech.Where(y => ids.Contains(y.EntryId)).ToList())
            senses[x.ParentKey()].PartsOfSpeech.Add(x);
        foreach (var x in context.ReadingRestrictions.Where(y => ids.Contains(y.EntryId)).ToList())
            senses[x.ParentKey()].ReadingRestrictions.Add(x);

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        return entries;
    }
}
