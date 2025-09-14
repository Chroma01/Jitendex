/*
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

using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text;
using Jitendex.Furigana.Helpers;
using Jitendex.Furigana.InputModels;
using Jitendex.Furigana.OutputModels;

namespace Jitendex.Furigana.Solvers;

internal class IterationSolver : FuriganaSolver
{
    private readonly ReadingCache _readingCache;

    public IterationSolver(ReadingCache readingCache)
    {
        _readingCache = readingCache;
    }

    public override IEnumerable<Solution> Solve(Entry entry)
    {
        var builders = new List<SolutionBuilder>() { new() };
        for (int sliceStart = 0; sliceStart < entry.KanjiFormRunes.Length; sliceStart++)
        {
            var newBuilders = new List<SolutionBuilder>();
            for (int sliceEnd = entry.KanjiFormRunes.Length; sliceStart < sliceEnd; sliceEnd--)
            {
                var iterationSlice = new IterationSlice(entry, sliceStart, sliceEnd);
                newBuilders = IterateBuilders(builders, iterationSlice);
                if (newBuilders.Count > 0)
                {
                    sliceStart += sliceEnd - sliceStart - 1;
                    builders = newBuilders;
                    break;
                }
            }
            if (newBuilders.Count == 0)
            {
                yield break;
            }
        }
        foreach (var builder in builders)
        {
            var solution = builder.ToSolution(entry);
            if (solution is not null)
            {
                yield return solution;
            }
        }
    }

    private List<SolutionBuilder> IterateBuilders(List<SolutionBuilder> oldBuilders, IterationSlice iterationSlice)
    {
        var newBuilders = new List<SolutionBuilder>();
        var iterationSliceReadings = new IterationSliceReadings(iterationSlice, _readingCache);

        foreach (var oldBuilder in oldBuilders)
        {
            var priorReadings = oldBuilder.NormalizedReadingText();
            int readingIndex = priorReadings.Length;
            var potentialReadings = iterationSliceReadings.GetPotentialReadings(readingIndex);
            var oldParts = oldBuilder.ToParts();

            foreach (var potentialReading in potentialReadings)
            {
                if (TryGetNewPart(iterationSlice, priorReadings, potentialReading, out var newPart))
                {
                    newBuilders.Add
                    (
                        new SolutionBuilder(oldParts.Add(newPart))
                    );
                }
            }
        }
        return newBuilders;
    }

    private static bool TryGetNewPart(IterationSlice iterationSlice, string priorReadings, string potentialReading, out Solution.Part part)
    {
        if (!iterationSlice.Entry.NormalizedReadingText.StartsWith(priorReadings + potentialReading))
        {
            part = new(string.Empty, null);
            return false;
        }
        var sliceText = iterationSlice.RawKanjiFormText;
        if (sliceText.IsKanaEquivalent(potentialReading))
        {
            part = new(sliceText, null);
            return true;
        }
        else
        {
            // Use the raw, non-normalized reading for the furigana text.
            int i = priorReadings.Length;
            int j = i + potentialReading.Length;
            var sliceReading = iterationSlice.Entry.ReadingText[i..j];
            part = new(sliceText, sliceReading);
            return true;
        }
    }
}

internal class IterationSlice
{
    public Entry Entry { get; }

    private readonly int _sliceStart;
    private readonly int _sliceEnd;

    public ImmutableArray<Rune> PriorKanjiFormRunes { get => Entry.KanjiFormRunes[.._sliceStart]; }
    public ImmutableArray<Rune> KanjiFormRunes { get => Entry.KanjiFormRunes[_sliceStart.._sliceEnd]; }
    public ImmutableArray<Rune> RemainingKanjiFormRunes { get => Entry.KanjiFormRunes[_sliceEnd..]; }

    public ImmutableArray<Rune> PriorRawKanjiFormRunes { get => Entry.RawKanjiFormRunes[.._sliceStart]; }
    public ImmutableArray<Rune> RawKanjiFormRunes { get => Entry.RawKanjiFormRunes[_sliceStart.._sliceEnd]; }
    public ImmutableArray<Rune> RemainingRawKanjiFormRunes { get => Entry.RawKanjiFormRunes[_sliceEnd..]; }

    public bool ContainsFirstRune { get => _sliceStart == 0; }
    public bool ContainsFinalRune { get => _sliceEnd == Entry.KanjiFormRunes.Length; }

    public string KanjiFormText { get; }
    public string RawKanjiFormText { get; }

    public IterationSlice(Entry entry, int sliceStart, int sliceEnd)
    {
        Entry = entry;
        _sliceStart = sliceStart;
        _sliceEnd = sliceEnd;

        KanjiFormText = string.Join(string.Empty, KanjiFormRunes);
        RawKanjiFormText = string.Join(string.Empty, RawKanjiFormRunes);
    }
}

internal class IterationSliceReadings
{
    private readonly IterationSlice _iterationSlice;
    private readonly ImmutableArray<string> _cachedReadings;
    private readonly bool _sliceIsSingleKanji;
    private static readonly FrozenSet<char> _impossibleKanjiReadingStart = ['っ', 'ょ', 'ゃ', 'ゅ', 'ん'];

    public IterationSliceReadings(IterationSlice iterationSlice, ReadingCache readingCache)
    {
        _iterationSlice = iterationSlice;
        _cachedReadings = readingCache.GetPotentialReadings(iterationSlice);

        _sliceIsSingleKanji =
            iterationSlice.KanjiFormRunes.Length == 1 &&
            iterationSlice.KanjiFormRunes[0].IsKanji();
    }

    public ImmutableArray<string> GetPotentialReadings(int readingIndex)
    {
        if (_sliceIsSingleKanji)
        {
            return CachedReadingsWithDefaultReading(readingIndex);
        }
        else
        {
            return _cachedReadings;
        }
    }

    private ImmutableArray<string> CachedReadingsWithDefaultReading(int readingIndex)
    {
        var remainingReadingText = _iterationSlice.Entry.NormalizedReadingText[readingIndex..];

        if (remainingReadingText == string.Empty)
        {
            return _cachedReadings;
        }

        var defaultReading = DefaultReading(remainingReadingText);

        if (_cachedReadings.Contains(defaultReading))
        {
            return _cachedReadings;
        }
        else
        {
            return _cachedReadings.Add(defaultReading);
        }
    }

    private string DefaultReading(string remainingReadingText)
    {
        var defaultReadingBuilder = new StringBuilder(remainingReadingText[..1]);
        var nextRune = NextRune();
        if (remainingReadingText.Length > 1)
        {
            foreach (var readingCharacter in remainingReadingText[1..])
            {
                if (nextRune.IsKana() && readingCharacter.IsKanaEquivalent((char)nextRune.Value))
                {
                    break;
                }
                else if (!nextRune.IsKana() && !nextRune.IsKanji() && nextRune.Value != 0)
                {
                    // Next rune must be punctuation or a foreign writing system. All bets are off.
                    break;
                }
                else if (_impossibleKanjiReadingStart.Contains(readingCharacter))
                {
                    defaultReadingBuilder.Append(readingCharacter);
                }
                else
                {
                    break;
                }
            }
        }
        return defaultReadingBuilder.ToString();
    }

    private Rune NextRune()
    {
        var remainingKanjiFormRunes = _iterationSlice.RemainingKanjiFormRunes;
        if (remainingKanjiFormRunes.Length > 0)
        {
            return remainingKanjiFormRunes[0];
        }
        else
        {
            return new Rune(0);
        }
    }
}
