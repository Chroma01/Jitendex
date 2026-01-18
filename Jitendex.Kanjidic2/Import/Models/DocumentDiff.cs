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

using Jitendex.Kanjidic2.Import.Models.Groups;
using Jitendex.Kanjidic2.Import.Models.GroupElements;
using Jitendex.Kanjidic2.Import.Models.SubgroupElements;

namespace Jitendex.Kanjidic2.Import.Models;

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

        FindNew<string, CodepointType>(docA, docB, propertyName: nameof(Document.CodepointTypes));
        FindNew<string, DictionaryType>(docA, docB, propertyName: nameof(Document.DictionaryTypes));
        FindNew<string, QueryCodeType>(docA, docB, propertyName: nameof(Document.QueryCodeTypes));
        FindNew<string, MisclassificationType>(docA, docB, propertyName: nameof(Document.MisclassificationTypes));
        FindNew<string, RadicalType>(docA, docB, propertyName: nameof(Document.RadicalTypes));
        FindNew<string, ReadingType>(docA, docB, propertyName: nameof(Document.ReadingTypes));
        FindNew<string, VariantType>(docA, docB, propertyName: nameof(Document.VariantTypes));

        FindNew<int, Entry>(docA, docB, propertyName: nameof(Document.Entries));

        DiffDictionaryProperties<(int, int), CodepointGroup>(docA, docB, propertyName: nameof(Document.CodepointGroups));
        DiffDictionaryProperties<(int, int), DictionaryGroup>(docA, docB, propertyName: nameof(Document.DictionaryGroups));
        DiffDictionaryProperties<(int, int), MiscGroup>(docA, docB, propertyName: nameof(Document.MiscGroups));
        DiffDictionaryProperties<(int, int), QueryCodeGroup>(docA, docB, propertyName: nameof(Document.QueryCodeGroups));
        DiffDictionaryProperties<(int, int), RadicalGroup>(docA, docB, propertyName: nameof(Document.RadicalGroups));
        DiffDictionaryProperties<(int, int), ReadingMeaningGroup>(docA, docB, propertyName: nameof(Document.ReadingMeaningGroups));

        DiffDictionaryProperties<(int, int, int), Codepoint>(docA, docB, propertyName: nameof(Document.Codepoints));
        DiffDictionaryProperties<(int, int, int), Dictionary>(docA, docB, propertyName: nameof(Document.Dictionaries));
        DiffDictionaryProperties<(int, int, int), Nanori>(docA, docB, propertyName: nameof(Document.Nanoris));
        DiffDictionaryProperties<(int, int, int), QueryCode>(docA, docB, propertyName: nameof(Document.QueryCodes));
        DiffDictionaryProperties<(int, int, int), Radical>(docA, docB, propertyName: nameof(Document.Radicals));
        DiffDictionaryProperties<(int, int, int), RadicalName>(docA, docB, propertyName: nameof(Document.RadicalNames));
        DiffDictionaryProperties<(int, int, int), ReadingMeaning>(docA, docB, propertyName: nameof(Document.ReadingMeanings));
        DiffDictionaryProperties<(int, int, int), StrokeCount>(docA, docB, propertyName: nameof(Document.StrokeCounts));
        DiffDictionaryProperties<(int, int, int), Variant>(docA, docB, propertyName: nameof(Document.Variants));

        DiffDictionaryProperties<(int, int, int, int), Meaning>(docA, docB, propertyName: nameof(Document.Meanings));
        DiffDictionaryProperties<(int, int, int, int), Reading>(docA, docB, propertyName: nameof(Document.Readings));

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
