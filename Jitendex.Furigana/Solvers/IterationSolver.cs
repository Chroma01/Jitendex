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

using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
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
        var sliceReadingCache = new SliceReadingCache(iterationSlice, _readingCache);

        foreach (var oldBuilder in oldBuilders)
        {
            var readingState = new ReadingState(iterationSlice, oldBuilder);
            var potentialReadings = sliceReadingCache.GetPotentialReadings(readingState);
            var oldParts = oldBuilder.ToParts();

            foreach (var potentialReading in potentialReadings)
            {
                if (TryGetNewPart(iterationSlice, readingState, potentialReading, out var newPart))
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

    private static bool TryGetNewPart(IterationSlice iterationSlice, ReadingState readingState, string potentialReading, out Solution.Part part)
    {
        var priorReadingText = readingState.PriorReadingText;
        if (!iterationSlice.Entry.NormalizedReadingText.StartsWith(priorReadingText + potentialReading))
        {
            part = null!;
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
            int i = priorReadingText.Length;
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

internal class SliceReadingCache
{
    private readonly ImmutableArray<string> _cachedReadings;

    public SliceReadingCache(IterationSlice iterationSlice, ReadingCache readingCache)
    {
        _cachedReadings = readingCache.GetPotentialReadings(iterationSlice);
    }

    public ImmutableArray<string> GetPotentialReadings(ReadingState readingState)
    {
        var defaultReading = readingState.DefaultSliceReading();

        if (defaultReading is null || _cachedReadings.Contains(defaultReading))
        {
            return _cachedReadings;
        }
        else
        {
            return _cachedReadings.Add(defaultReading);
        }
    }
}

internal class ReadingState
{
    private readonly IterationSlice _iterationSlice;
    private readonly SolutionBuilder _solutionBuilder;
    private readonly string _normalizedRemainingKanjiFormText;

    public string PriorReadingText { get; }
    public string RemainingReadingText { get; }

    public ReadingState(IterationSlice iterationSlice, SolutionBuilder solutionBuilder)
    {
        _iterationSlice = iterationSlice;
        _solutionBuilder = solutionBuilder;
        _normalizedRemainingKanjiFormText = _iterationSlice.RemainingKanjiFormText().KatakanaToHiragana();

        PriorReadingText = _solutionBuilder.NormalizedReadingText();
        RemainingReadingText = _iterationSlice.Entry
            .NormalizedReadingText[PriorReadingText.Length..];
    }

    public string? DefaultSliceReading()
    {
        bool sliceIsSingleKanji = _iterationSlice.KanjiFormRunes.Length == 1 && _iterationSlice.KanjiFormRunes[0].IsKanji();
        if (!sliceIsSingleKanji)
        {
            // Default readings not yet supported for slices greater than length 1
            return null;
        }

        var previousRune = _iterationSlice.PreviousKanjiFormRune();
        var nextRune = _iterationSlice.NextKanjiFormRune();

        if ((previousRune.IsKana() || previousRune == default) && (nextRune.IsKana() || nextRune == default))
        {
            return RegexKanjiReading();
        }
        else if (nextRune.IsKanji() || nextRune == default)
        {
            return NormalKanjiReading();
        }
        else
        {
            // Next rune must be punctuation or from a foreign writing system.
            return MinimumReading();
        }
    }

    private string? MinimumReading() =>
        RemainingReadingText == string.Empty ? null : RemainingReadingText[..1];

    private string? NormalKanjiReading()
    {
        if (RemainingReadingText == string.Empty)
        {
            return null;
        }
        var reading = new StringBuilder(RemainingReadingText[..1]);
        foreach (var character in RemainingReadingText[1..])
        {
            if (IsImpossibleKanjiReadingStart(character))
            {
                reading.Append(character);
            }
            else
            {
                break;
            }
        }
        return reading.ToString();
    }

    private static bool IsImpossibleKanjiReadingStart(char c) => c switch
    {
        'っ' or 'ょ' or 'ゃ' or 'ゅ' or 'ん' => true, _ => false
    };

    private string? RegexKanjiReading()
    {
        var greedyMatch = Match("(.+)");
        var lazyMatch = Match("(.+?)");

        if (!greedyMatch.Success || !lazyMatch.Success)
        {
            return null;
        }

        var greedyValue = greedyMatch.Groups[1].Value;

        if (greedyValue != string.Empty && greedyValue == lazyMatch.Groups[1].Value)
        {
            return greedyValue;
        }
        else
        {
            return null;
        }
    }

    private Match Match(string groupPattern)
    {
        var pattern = new StringBuilder($"^{groupPattern}");
        foreach (var character in _normalizedRemainingKanjiFormText)
        {
            if (character.IsKana())
            {
                pattern.Append(character);
            }
            else
            {
                pattern.Append(groupPattern);
            }
        }
        pattern.Append('$');
        var regex = new Regex(pattern.ToString());
        return regex.Match(RemainingReadingText);
    }
}
