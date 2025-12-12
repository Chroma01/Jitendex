/*
Copyright (c) 2025 Stephen Kraus

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

namespace Jitendex.Tatoeba.ImportDto;

internal sealed class DocumentDiff
{
    public DateOnly Date { get; init; }
    public Document InsertDocument { get; init; }
    public Document UpdateDocument { get; init; }
    public Document DeleteDocument { get; init; }

    public DocumentDiff(Document docA, Document docB)
    {
        Date = docB.Date;
        InsertDocument = new Document(Date);
        UpdateDocument = new Document(Date);
        DeleteDocument = new Document(Date);

        DiffHashSetProperties<int>(docA, docB, propertyName: nameof(Document.Sequences));
        DiffDictionaryProperties<int, EnglishSequence>(docA, docB, propertyName: nameof(Document.EnglishSequences));
        DiffDictionaryProperties<int, JapaneseSequence>(docA, docB, propertyName: nameof(Document.JapaneseSequences));
        DiffDictionaryProperties<(int, int), TokenizedSentence>(docA, docB, propertyName: nameof(Document.TokenizedSentences));
        DiffDictionaryProperties<(int, int, int), Token>(docA, docB, propertyName: nameof(Document.Tokens));
    }

    public HashSet<int> GetTouchedSequenceIds()
        => InsertDocument.GetTouchedSequenceIds()
        .Concat(UpdateDocument.GetTouchedSequenceIds())
        .Concat(DeleteDocument.GetTouchedSequenceIds())
        .ToHashSet();

    private void DiffHashSetProperties<T>(Document docA, Document docB, string propertyName) where T : struct
    {
        var prop = typeof(Document).GetProperty(propertyName)!;
        var setA = (HashSet<T>)prop.GetValue(docA)!;
        var setB = (HashSet<T>)prop.GetValue(docB)!;
        var inserts = (HashSet<T>)prop.GetValue(InsertDocument)!;
        var deletes = (HashSet<T>)prop.GetValue(DeleteDocument)!;

        foreach (var key in setA)
        {
            if (!setB.Contains(key))
            {
                deletes.Add(key);
            }
        }
        foreach (var key in setB)
        {
            if (!setA.Contains(key))
            {
                inserts.Add(key);
            }
        }
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

        foreach (var (key, valueA) in dictA)
        {
            if (!dictB.TryGetValue(key, out TValue? valueB))
            {
                deletes.Add(key, valueA);
            }
            else if (!Equals(valueA, valueB))  // Hot spot!!!
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
