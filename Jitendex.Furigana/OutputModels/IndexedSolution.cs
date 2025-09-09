/*
Copyright (c) 2025 Doublevil
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

using System.Collections.Immutable;
using Jitendex.Furigana.InputModels;

namespace Jitendex.Furigana.OutputModels;

/// <summary>
/// A vocab entry with a furigana reading string.
/// </summary>
internal class IndexedSolution
{
    public VocabEntry Vocab { get; }
    public ImmutableArray<IndexedFurigana> Parts { get; }

    public IndexedSolution(VocabEntry vocab, params IndexedFurigana[] parts) : this(vocab, parts.ToList()) { }

    public IndexedSolution(VocabEntry vocab, IEnumerable<IndexedFurigana> parts)
    {
        Vocab = vocab;
        Parts = [.. parts];

        if (!IndexedSolutionChecker.Check(this))
        {
            // TODO: More details
            throw new ArgumentException("Invalid solution parts for this vocab entry");
        }
    }

    public Solution ToTextSolution()
    {
        return new Solution
        {
            Vocab = Vocab,
            Parts = [.. EnumerateTextSolutionParts()],
        };
    }

    private IEnumerable<Solution.Part> EnumerateTextSolutionParts()
    {
        foreach (var (value, start, end) in EnumerateAllRanges())
        {
            var baseRunes = Vocab.RawKanjiFormRunes[start..(end + 1)];
            var baseText = string.Join(string.Empty, baseRunes);
            yield return new Solution.Part(baseText, value);
        }
    }

    private IEnumerable<(string? value, int start, int end)> EnumerateAllRanges()
    {
        int runeCount = Vocab.KanjiFormRunes.Length;
        int? emptyRangeStart = null;
        for (int i = 0; i < runeCount; i++)
        {
            var indexedFurigana = Parts.FirstOrDefault(x => x.StartIndex == i);
            if (indexedFurigana is null)
            {
                emptyRangeStart ??= i;
                continue;
            }
            if (emptyRangeStart is not null)
            {
                yield return new(null, (int)emptyRangeStart, i - 1);
                emptyRangeStart = null;
            }
            yield return new(indexedFurigana.Value, indexedFurigana.StartIndex, indexedFurigana.EndIndex);
            i = indexedFurigana.EndIndex;
        }
        if (emptyRangeStart is not null)
        {
            yield return new(null, (int)emptyRangeStart, runeCount - 1);
        }
    }

    public override bool Equals(object? obj) =>
        obj is IndexedSolution other &&
        (ReferenceEquals(Vocab, other.Vocab) || Vocab.Equals(other.Vocab)) &&
        Parts.SequenceEqual(other.Parts);

    public override int GetHashCode() =>
        Parts.Aggregate
        (
            seed: Vocab.GetHashCode(),
            func: (hashcode, part) => HashCode.Combine(hashcode, part.GetHashCode())
        );
}
