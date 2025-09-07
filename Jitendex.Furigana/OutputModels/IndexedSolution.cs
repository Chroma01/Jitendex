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
    public ImmutableList<IndexedFurigana> Parts { get; }

    public IndexedSolution(VocabEntry vocab, params IndexedFurigana[] parts) : this(vocab, parts.ToList()) { }

    public IndexedSolution(VocabEntry vocab, IEnumerable<IndexedFurigana> parts)
    {
        Vocab = vocab;
        Parts = [.. parts];

        if (!IndexedSolutionChecker.Check(this))
        {
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
            var baseRunes = Vocab.RawKanjiFormRunes.GetRange(start, end - start + 1);
            var baseText = string.Join("", baseRunes);
            yield return new Solution.Part(baseText, value);
        }
    }

    private IEnumerable<(string? value, int start, int end)> EnumerateAllRanges()
    {
        int runeCount = Vocab.KanjiFormRunes.Count;
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

    public override bool Equals(object? obj)
    {
        if (obj is IndexedSolution other)
        {
            // Compare both solutions.
            if (Vocab != other.Vocab || Parts.Count != other.Parts.Count)
            {
                // Not the same vocab or not the same count of furigana parts.
                return false;
            }

            // If there is at least one furigana part that has no equivalent in the other
            // furigana solution, then the readings differ.
            return Parts.All(f1 => other.Parts.Any(f2 => f1.Equals(f2)))
                && other.Parts.All(f2 => Parts.Any(f1 => f1.Equals(f2)));
        }
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
