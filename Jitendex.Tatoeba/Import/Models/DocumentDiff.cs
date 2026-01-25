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

namespace Jitendex.Tatoeba.Import.Models;

internal sealed class DocumentDiff
{
    public DateOnly Date { get; init; }
    public Document InsertDocument { get; init; }
    public Document UpdateDocument { get; init; }
    public Document DeleteDocument { get; init; }
    public IReadOnlySet<int> SequenceIds { get; init; }

    public DocumentDiff(Document docA, Document docB)
    {
        Date = docB.Header.Date;
        InsertDocument = new Document(Date, 0);
        UpdateDocument = new Document(Date, 0);
        DeleteDocument = new Document(Date, 0);

        DiffDictionaryProperties<int, ExampleElement>(docA, docB, propertyName: nameof(Document.Examples));
        DiffDictionaryProperties<int, TranslationElement>(docA, docB, propertyName: nameof(Document.Translations));
        DiffDictionaryProperties<(int, int), SegmentationElement>(docA, docB, propertyName: nameof(Document.Segmentations));
        DiffDictionaryProperties<(int, int, int), TokenElement>(docA, docB, propertyName: nameof(Document.Tokens));

        SequenceIds = InsertDocument.ConcatAllExampleIds()
            .Concat(UpdateDocument.ConcatAllExampleIds())
            .Concat(DeleteDocument.ConcatAllExampleIds())
            .ToHashSet();
    }

    private void DiffDictionaryProperties<TKey, TValue>(Document docA, Document docB, string propertyName)
        where TKey : struct
        where TValue : notnull
    {
        var prop = typeof(Document).GetProperty(propertyName)!;
        var dictA = (Dictionary<TKey, TValue>)prop.GetValue(docA)!;
        var dictB = (Dictionary<TKey, TValue>)prop.GetValue(docB)!;
        var inserts = (Dictionary<TKey, TValue>)prop.GetValue(InsertDocument)!;
        var updates = (Dictionary<TKey, TValue>)prop.GetValue(UpdateDocument)!;
        var deletes = (Dictionary<TKey, TValue>)prop.GetValue(DeleteDocument)!;
        var comparer = EqualityComparer<TValue>.Default;

        foreach (var (key, valueA) in dictA)
        {
            if (!dictB.TryGetValue(key, out TValue? valueB))
            {
                deletes.Add(key, valueA);
            }
            else if (!comparer.Equals(valueA, valueB))  // Hot spot!!!
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
