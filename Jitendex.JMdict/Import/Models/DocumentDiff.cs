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
    public DocumentHeader FileHeader { get; init; }
    public Document InsertDocument { get; init; }
    public Document UpdateDocument { get; init; }
    public Document DeleteDocument { get; init; }
    public Dictionary<int, DocumentSequence> Sequences { get; init; }

    public DocumentDiff(Document docA, Document docB)
    {
        FileHeader = docB.Header;
        InsertDocument = new Document(0) { Header = docB.Header };
        UpdateDocument = new Document(0) { Header = docB.Header };
        DeleteDocument = new Document(0) { Header = docB.Header };

        FindNew<string, ReadingInfoTagElement>(docA, docB, propertyName: nameof(Document.ReadingInfoTags));
        FindNew<string, KanjiFormInfoTagElement>(docA, docB, propertyName: nameof(Document.KanjiFormInfoTags));
        FindNew<string, PartOfSpeechTagElement>(docA, docB, propertyName: nameof(Document.PartOfSpeechTags));
        FindNew<string, FieldTagElement>(docA, docB, propertyName: nameof(Document.FieldTags));
        FindNew<string, MiscTagElement>(docA, docB, propertyName: nameof(Document.MiscTags));
        FindNew<string, DialectTagElement>(docA, docB, propertyName: nameof(Document.DialectTags));
        FindNew<string, GlossTypeElement>(docA, docB, propertyName: nameof(Document.GlossTypes));
        FindNew<string, CrossReferenceTypeElement>(docA, docB, propertyName: nameof(Document.CrossReferenceTypes));
        FindNew<string, LanguageSourceTypeElement>(docA, docB, propertyName: nameof(Document.LanguageSourceTypes));
        FindNew<string, PriorityTagElement>(docA, docB, propertyName: nameof(Document.PriorityTags));
        FindNew<string, LanguageElement>(docA, docB, propertyName: nameof(Document.Languages));

        DiffDictionaryProperties<int, EntryElement>(docA, docB, propertyName: nameof(Document.Entries));

        DiffDictionaryProperties<(int, int), KanjiFormElement>(docA, docB, propertyName: nameof(Document.KanjiForms));
        DiffDictionaryProperties<(int, int), ReadingElement>(docA, docB, propertyName: nameof(Document.Readings));
        DiffDictionaryProperties<(int, int), SenseElement>(docA, docB, propertyName: nameof(Document.Senses));

        DiffDictionaryProperties<(int, int, int), KanjiFormInfoElement>(docA, docB, propertyName: nameof(Document.KanjiFormInfos));
        DiffDictionaryProperties<(int, int, int), KanjiFormPriorityElement>(docA, docB, propertyName: nameof(Document.KanjiFormPriorities));

        DiffDictionaryProperties<(int, int, int), ReadingInfoElement>(docA, docB, propertyName: nameof(Document.ReadingInfos));
        DiffDictionaryProperties<(int, int, int), ReadingPriorityElement>(docA, docB, propertyName: nameof(Document.ReadingPriorities));
        DiffDictionaryProperties<(int, int, int), RestrictionElement>(docA, docB, propertyName: nameof(Document.Restrictions));

        DiffDictionaryProperties<(int, int, int), CrossReferenceElement>(docA, docB, propertyName: nameof(Document.CrossReferences));
        DiffDictionaryProperties<(int, int, int), DialectElement>(docA, docB, propertyName: nameof(Document.Dialects));
        DiffDictionaryProperties<(int, int, int), FieldElement>(docA, docB, propertyName: nameof(Document.Fields));
        DiffDictionaryProperties<(int, int, int), GlossElement>(docA, docB, propertyName: nameof(Document.Glosses));
        DiffDictionaryProperties<(int, int, int), KanjiFormRestrictionElement>(docA, docB, propertyName: nameof(Document.KanjiFormRestrictions));
        DiffDictionaryProperties<(int, int, int), LanguageSourceElement>(docA, docB, propertyName: nameof(Document.LanguageSources));
        DiffDictionaryProperties<(int, int, int), MiscElement>(docA, docB, propertyName: nameof(Document.Miscs));
        DiffDictionaryProperties<(int, int, int), PartOfSpeechElement>(docA, docB, propertyName: nameof(Document.PartsOfSpeech));
        DiffDictionaryProperties<(int, int, int), ReadingRestrictionElement>(docA, docB, propertyName: nameof(Document.ReadingRestrictions));

        Sequences = InsertDocument.ConcatAllEntryIds()
            .Concat(UpdateDocument.ConcatAllEntryIds())
            .Concat(DeleteDocument.ConcatAllEntryIds())
            .Distinct()
            .Select(id => new DocumentSequence
            {
                Id = id,
                CreatedDate = FileHeader.Date,
            })
            .ToDictionary(s => s.Id);
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
