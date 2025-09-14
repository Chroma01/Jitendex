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
        var sliceText = iterationSlice.RawKanjiFormText();
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

    public Rune PreviousKanjiFormRune() => PriorKanjiFormRunes.LastOrDefault();
    public Rune NextKanjiFormRune() => RemainingKanjiFormRunes.FirstOrDefault();

    public ImmutableArray<Rune> RawKanjiFormRunes { get => Entry.RawKanjiFormRunes[_sliceStart.._sliceEnd]; }

    public bool ContainsFirstRune { get => _sliceStart == 0; }
    public bool ContainsFinalRune { get => _sliceEnd == Entry.KanjiFormRunes.Length; }

    public string KanjiFormText() => string.Join(string.Empty, KanjiFormRunes);
    public string RawKanjiFormText() => string.Join(string.Empty, RawKanjiFormRunes);
    public string RemainingKanjiFormText() => string.Join(string.Empty, RemainingKanjiFormRunes);

    public IterationSlice(Entry entry, int sliceStart, int sliceEnd)
    {
        Entry = entry;
        _sliceStart = sliceStart;
        _sliceEnd = sliceEnd;
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
            return CachedReadingsWithDefaultSingleKanjiReading(readingIndex);
        }
        else
        {
            return _cachedReadings;
        }
    }

    private ImmutableArray<string> CachedReadingsWithDefaultSingleKanjiReading(int readingIndex)
    {
        var nextReadingCharacter = _iterationSlice.Entry.NormalizedReadingText[readingIndex..].FirstOrDefault();

        if (nextReadingCharacter == default)
        {
            return _cachedReadings;
        }

        var remainingReadingText = _iterationSlice.Entry.NormalizedReadingText[(readingIndex + 1)..];
        var remainingKanjiFormText = _iterationSlice.RemainingKanjiFormText().KatakanaToHiragana();

        if (remainingReadingText.EndsWith(remainingKanjiFormText))
        {
            int i = remainingReadingText.Length - remainingKanjiFormText.Length;
            remainingReadingText = remainingReadingText[..i];
            remainingKanjiFormText = string.Empty;
        }

        var defaultReading = DefaultSingleKanjiReading(nextReadingCharacter, remainingReadingText, remainingKanjiFormText);

        if (defaultReading is null || _cachedReadings.Contains(defaultReading))
        {
            return _cachedReadings;
        }
        else
        {
            return _cachedReadings.Add(defaultReading);
        }
    }

    private string? DefaultSingleKanjiReading(char nextReadingCharacter, string remainingReadingText, string remainingKanjiFormText)
    {
        var defaultReadingBuilder = new StringBuilder();

        var remainingKanjiFormRunes = remainingKanjiFormText.EnumerateRunes();
        var nextRune = remainingKanjiFormRunes.FirstOrDefault();
        var previousRune = _iterationSlice.PreviousKanjiFormRune();

        if ((previousRune.IsKana() || previousRune == default) && (nextRune.IsKana() || nextRune == default))
        {
            var delimiter = (char)nextRune.Value;

            var remainingReadingDelimiterCount =
                remainingReadingText.Count(x => x == delimiter);

            var remainingKanjiFormDelimiterCount =
                remainingKanjiFormText.Count(x => x == delimiter);

            if (remainingReadingDelimiterCount != remainingKanjiFormDelimiterCount)
            {
                // Cannot determine the number of reading characters for this single kanji.
                return null;
            }
            defaultReadingBuilder.Append(nextReadingCharacter);
            foreach (var readingCharacter in remainingReadingText)
            {
                if (readingCharacter != delimiter)
                {
                    defaultReadingBuilder.Append(readingCharacter);
                }
                else
                {
                    break;
                }
            }
            return defaultReadingBuilder.ToString();
        }
        else if (nextRune.IsKanji() || nextRune == default)
        {
            defaultReadingBuilder.Append(nextReadingCharacter);
            foreach (var readingCharacter in remainingReadingText)
            {
                if (_impossibleKanjiReadingStart.Contains(readingCharacter))
                {
                    defaultReadingBuilder.Append(readingCharacter);
                }
                else
                {
                    break;
                }
            }
            return defaultReadingBuilder.ToString();
        }
        else
        {
            // Next rune must be punctuation or from a foreign writing system.
            return nextReadingCharacter.ToString();
        }
    }
}
