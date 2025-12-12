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

        DiffHashSets<int>(docA, docB, nameof(Document.Sequences));
        DiffDictionaries<int, EnglishSequence>(docA, docB, nameof(Document.EnglishSequences));
        DiffDictionaries<int, JapaneseSequence>(docA, docB, nameof(Document.JapaneseSequences));
        DiffDictionaries<(int, int), TokenizedSentence>(docA, docB, nameof(Document.TokenizedSentences));
        DiffDictionaries<(int, int, int), Token>(docA, docB, nameof(Document.Tokens));
    }

    public HashSet<int> GetTouchedSequenceIds()
        => InsertDocument.GetTouchedSequenceIds()
        .Concat(UpdateDocument.GetTouchedSequenceIds())
        .Concat(DeleteDocument.GetTouchedSequenceIds())
        .ToHashSet();

    private void DiffHashSets<T>(Document docA, Document docB, string propertyName) where T : notnull
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

    private void DiffDictionaries<TKey, TValue>(Document docA, Document docB, string propertyName)
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
            if (!dictB.TryGetValue(key, out var valueB))
            {
                deletes.Add(key, valueA);
            }
            else if (!valueA.Equals(valueB))
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
