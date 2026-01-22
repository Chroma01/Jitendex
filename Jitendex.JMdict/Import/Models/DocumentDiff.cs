/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms
of the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

using Jitendex.JMdict.Import.Models.EntryElements;
using Jitendex.JMdict.Import.Models.EntryElements.KanjiFormElements;
using Jitendex.JMdict.Import.Models.EntryElements.ReadingElements;
using Jitendex.JMdict.Import.Models.EntryElements.SenseElements;

namespace Jitendex.JMdict.Import.Models;

internal sealed class DocumentDiff
{
    public FileHeader FileHeader { get; init; }
    public Document InsertDocument { get; init; }
    public Document UpdateDocument { get; init; }
    public Document DeleteDocument { get; init; }
    public IReadOnlySet<int> EntryIds { get; init; }

    public DocumentDiff(Document docA, Document docB)
    {
        FileHeader = docB.FileHeader;
        InsertDocument = new Document(0) { FileHeader = docB.FileHeader };
        UpdateDocument = new Document(0) { FileHeader = docB.FileHeader };
        DeleteDocument = new Document(0) { FileHeader = docB.FileHeader };

        FindNew<string, ReadingInfoTag>(docA, docB, propertyName: nameof(Document.ReadingInfoTags));
        FindNew<string, KanjiFormInfoTag>(docA, docB, propertyName: nameof(Document.KanjiFormInfoTags));
        FindNew<string, PartOfSpeechTag>(docA, docB, propertyName: nameof(Document.PartOfSpeechTags));
        FindNew<string, FieldTag>(docA, docB, propertyName: nameof(Document.FieldTags));
        FindNew<string, MiscTag>(docA, docB, propertyName: nameof(Document.MiscTags));
        FindNew<string, DialectTag>(docA, docB, propertyName: nameof(Document.DialectTags));
        FindNew<string, GlossType>(docA, docB, propertyName: nameof(Document.GlossTypes));
        FindNew<string, CrossReferenceType>(docA, docB, propertyName: nameof(Document.CrossReferenceTypes));
        FindNew<string, LanguageSourceType>(docA, docB, propertyName: nameof(Document.LanguageSourceTypes));
        FindNew<string, PriorityTag>(docA, docB, propertyName: nameof(Document.PriorityTags));
        FindNew<string, Language>(docA, docB, propertyName: nameof(Document.Languages));

        FindNew<int, Entry>(docA, docB, propertyName: nameof(Document.Entries));

        DiffDictionaryProperties<(int, int), KanjiForm>(docA, docB, propertyName: nameof(Document.KanjiForms));
        DiffDictionaryProperties<(int, int), Reading>(docA, docB, propertyName: nameof(Document.Readings));
        DiffDictionaryProperties<(int, int), Sense>(docA, docB, propertyName: nameof(Document.Senses));

        DiffDictionaryProperties<(int, int, int), KanjiFormInfo>(docA, docB, propertyName: nameof(Document.KanjiFormInfos));
        DiffDictionaryProperties<(int, int, int), KanjiFormPriority>(docA, docB, propertyName: nameof(Document.KanjiFormPriorities));

        DiffDictionaryProperties<(int, int, int), ReadingInfo>(docA, docB, propertyName: nameof(Document.ReadingInfos));
        DiffDictionaryProperties<(int, int, int), ReadingPriority>(docA, docB, propertyName: nameof(Document.ReadingPriorities));
        DiffDictionaryProperties<(int, int, int), Restriction>(docA, docB, propertyName: nameof(Document.Restrictions));

        DiffDictionaryProperties<(int, int, int), CrossReference>(docA, docB, propertyName: nameof(Document.CrossReferences));
        DiffDictionaryProperties<(int, int, int), Dialect>(docA, docB, propertyName: nameof(Document.Dialects));
        DiffDictionaryProperties<(int, int, int), Field>(docA, docB, propertyName: nameof(Document.Fields));
        DiffDictionaryProperties<(int, int, int), Gloss>(docA, docB, propertyName: nameof(Document.Glosses));
        DiffDictionaryProperties<(int, int, int), KanjiFormRestriction>(docA, docB, propertyName: nameof(Document.KanjiFormRestrictions));
        DiffDictionaryProperties<(int, int, int), LanguageSource>(docA, docB, propertyName: nameof(Document.LanguageSources));
        DiffDictionaryProperties<(int, int, int), Misc>(docA, docB, propertyName: nameof(Document.Miscs));
        DiffDictionaryProperties<(int, int, int), PartOfSpeech>(docA, docB, propertyName: nameof(Document.PartsOfSpeech));
        DiffDictionaryProperties<(int, int, int), ReadingRestriction>(docA, docB, propertyName: nameof(Document.ReadingRestrictions));

        EntryIds = InsertDocument.ConcatAllEntryIds()
            .Concat(UpdateDocument.ConcatAllEntryIds())
            .Concat(DeleteDocument.ConcatAllEntryIds())
            .ToHashSet();
    }

    private void FindNew<TKey, TValue>(Document docA, Document docB, string propertyName) where TKey : notnull
    {
        var prop = typeof(Document).GetProperty(propertyName)!;
        var dictA = (Dictionary<TKey, TValue>)prop.GetValue(docA)!;
        var dictB = (Dictionary<TKey, TValue>)prop.GetValue(docB)!;
        var inserts = (Dictionary<TKey, TValue>)prop.GetValue(InsertDocument)!;

        foreach (var (key, entry) in dictB)
        {
            if (!dictA.ContainsKey(key))
            {
                inserts.Add(key, entry);
            }
        }
    }

    private void DiffDictionaryProperties<TKey, TValue>(Document docA, Document docB, string propertyName)
        where TKey : notnull
        where TValue : notnull
    {
        var prop = typeof(Document).GetProperty(propertyName)!;
        var dictA = (Dictionary<TKey, TValue>)prop.GetValue(docA)!;
        var dictB = (Dictionary<TKey, TValue>)prop.GetValue(docB)!;
        var inserts = (Dictionary<TKey, TValue>)prop.GetValue(InsertDocument)!;
        var updates = (Dictionary<TKey, TValue>)prop.GetValue(UpdateDocument)!;
        var deletes = (Dictionary<TKey, TValue>)prop.GetValue(DeleteDocument)!;

        foreach (var (key, valueA) in dictA)
        {
            if (!dictB.TryGetValue(key, out TValue? valueB))
            {
                deletes.Add(key, valueA);
            }
            else if (!valueA.Equals(valueB))  // Hot spot!!!
            {
                updates.Add(key, valueB);
            }
        }
        foreach (var (key, valueB) in dictB)
        {
            if (!dictA.ContainsKey(key))
            {
                inserts.Add(key, valueB);
            }
        }
    }
}
