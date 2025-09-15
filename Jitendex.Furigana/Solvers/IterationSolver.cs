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
                newBuilders = IterateBuilders(entry, builders, iterationSlice);
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

    private List<SolutionBuilder> IterateBuilders(Entry entry, List<SolutionBuilder> oldBuilders, IterationSlice iterationSlice)
    {
        var newBuilders = new List<SolutionBuilder>();
        var sliceReadingCache = new SliceReadingCache(entry, iterationSlice, _readingCache);

        foreach (var oldBuilder in oldBuilders)
        {
            var priorReadingText = oldBuilder.NormalizedReadingText();
            var readingState = new ReadingState
            {
                ReadingText = entry.ReadingText,
                PriorReadingTextNormalized = priorReadingText,
                RemainingReadingTextNormalized = entry.NormalizedReadingText[priorReadingText.Length..],
            };

            var oldParts = oldBuilder.ToParts();
            foreach (var newParts in sliceReadingCache.EnumerateParts(readingState))
            {
                newBuilders.Add
                (
                    new SolutionBuilder(oldParts.AddRange(newParts))
                );
            }
        }
        return newBuilders;
    }
}

internal class IterationSlice
{
    private readonly Entry _entry;
    private readonly int _sliceStart;
    private readonly int _sliceEnd;

    public ImmutableArray<Rune> PriorKanjiFormRunes { get => _entry.KanjiFormRunes[.._sliceStart]; }
    public ImmutableArray<Rune> KanjiFormRunes { get => _entry.KanjiFormRunes[_sliceStart.._sliceEnd]; }
    public ImmutableArray<Rune> RemainingKanjiFormRunes { get => _entry.KanjiFormRunes[_sliceEnd..]; }

    public Rune PreviousKanjiFormRune() => PriorKanjiFormRunes.LastOrDefault();
    public Rune NextKanjiFormRune() => RemainingKanjiFormRunes.FirstOrDefault();

    public ImmutableArray<Rune> RawKanjiFormRunes { get => _entry.RawKanjiFormRunes[_sliceStart.._sliceEnd]; }

    public bool ContainsFirstRune { get => _sliceStart == 0; }
    public bool ContainsFinalRune { get => _sliceEnd == _entry.KanjiFormRunes.Length; }

    public string KanjiFormText() => string.Join(string.Empty, KanjiFormRunes);
    public string RawKanjiFormText() => string.Join(string.Empty, RawKanjiFormRunes);
    public string RemainingKanjiFormText() => string.Join(string.Empty, RemainingKanjiFormRunes);

    public IterationSlice(Entry entry, int sliceStart, int sliceEnd)
    {
        _entry = entry;
        _sliceStart = sliceStart;
        _sliceEnd = sliceEnd;
    }
}

internal class SliceReadingCache
{
    private readonly IterationSlice _iterationSlice;
    private readonly ImmutableArray<string> _cachedReadings;

    public SliceReadingCache(Entry entry, IterationSlice iterationSlice, ReadingCache readingCache)
    {
        _iterationSlice = iterationSlice;
        _cachedReadings = readingCache.GetPotentialReadings(entry, iterationSlice);
    }

    public IEnumerable<List<Solution.Part>> EnumerateParts(ReadingState readingState)
    {
        var baseText = _iterationSlice.RawKanjiFormText();

        foreach (var parts in EnumerateCachedParts(readingState, baseText))
        {
            yield return parts;
        }

        var defaultParts = DefaultParts(readingState, baseText);

        if (defaultParts.Count > 0)
        {
            yield return defaultParts;
        }
    }

    private IEnumerable<List<Solution.Part>> EnumerateCachedParts(ReadingState readingState, string baseText)
    {
        foreach (var reading in _cachedReadings)
        {
            if (!readingState.RemainingReadingTextNormalized.StartsWith(reading))
            {
                continue;
            }
            else if (baseText.IsKanaEquivalent(reading))
            {
                yield return [new(baseText, null)];
            }
            else
            {
                var furigana = readingState.RemainingReadingText[..reading.Length];
                yield return [new(baseText, furigana)];
            }
        }
    }

    private List<Solution.Part> DefaultParts(ReadingState readingState, string baseText)
    {
        if (_iterationSlice.KanjiFormRunes.Length == 1)
        {
            var reading = DefaultSingleKanjiReading(readingState);
            if (reading is null || _cachedReadings.Contains(reading))
            {
                return [];
            }
            else
            {
                var furigana = readingState.RemainingReadingText[..reading.Length];
                return [new(baseText, furigana)];
            }
        }
        else if (_iterationSlice.KanjiFormRunes.Length == 2)
        {
            var parts = DefaultDoubleCharacterParts(readingState);
            if (parts.Count > 0)
            {
                return parts;
            }
        }
        return [];
    }

    private string? DefaultSingleKanjiReading(ReadingState readingState)
    {
        var currentRune = _iterationSlice.KanjiFormRunes[0];
        if (!currentRune.IsKanji())
        {
            return null;
        }

        var previousRune = _iterationSlice.PreviousKanjiFormRune();
        var nextRune = _iterationSlice.NextKanjiFormRune();

        if (previousRune.IsKanaOrDefault() && nextRune.IsKanaOrDefault())
        {
            return readingState.RegexKanjiReading(RemainingKanjiFormTextNormalized());
        }
        else if (nextRune.IsKanjiOrDefault())
        {
            return readingState.RegularKanjiReading();
        }
        else
        {
            // Next rune must be punctuation or from a foreign writing system.
            return readingState.MinimumReading();
        }
    }

    private List<Solution.Part> DefaultDoubleCharacterParts(ReadingState readingState)
    {
        var currentRune1 = _iterationSlice.KanjiFormRunes[0];
        var currentRune2 = _iterationSlice.KanjiFormRunes[1];

        if (currentRune1.IsKana() || currentRune1 != currentRune2)
        {
            return [];
        }

        var previousRune = _iterationSlice.PreviousKanjiFormRune();
        var nextRune = _iterationSlice.NextKanjiFormRune();

        if (!previousRune.IsKanaOrDefault() || !nextRune.IsKanaOrDefault())
        {
            return [];
        }

        var reading = readingState.RegexKanjiReading(RemainingKanjiFormTextNormalized());

        if (reading is null || reading.Length % 2 != 0)
        {
            return [];
        }

        int halfLength = reading.Length / 2;

        return [
            new(_iterationSlice.RawKanjiFormRunes[0].ToString(), reading[..halfLength]),
            new(_iterationSlice.RawKanjiFormRunes[1].ToString(), reading[halfLength..])
        ];
    }

    private string RemainingKanjiFormTextNormalized() =>
        _iterationSlice.RemainingKanjiFormText().KatakanaToHiragana();
}

internal class ReadingState
{
    public required string ReadingText { get; init; }
    public string PriorReadingText { get => ReadingText[..PriorReadingTextNormalized.Length]; }
    public string RemainingReadingText { get => ReadingText[PriorReadingTextNormalized.Length..]; }

    public string ReadingTextNormalized { get => PriorReadingTextNormalized + RemainingReadingTextNormalized; }
    public required string PriorReadingTextNormalized { get; init; }
    public required string RemainingReadingTextNormalized { get; init; }

    public string? MinimumReading() =>
        RemainingReadingTextNormalized == string.Empty ? null : RemainingReadingTextNormalized[..1];

    public string? RegularKanjiReading()
    {
        if (RemainingReadingTextNormalized == string.Empty)
        {
            return null;
        }
        var reading = new StringBuilder(RemainingReadingTextNormalized[..1]);
        foreach (var character in RemainingReadingTextNormalized[1..])
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
        'っ' or 'ょ' or 'ゃ' or 'ゅ' or 'ん' => true,
        _ => false
    };

    public string? RegexKanjiReading(string remainingKanjiFormTextNormalized)
    {
        var greedyMatch = Match("(.+)", remainingKanjiFormTextNormalized);
        var lazyMatch = Match("(.+?)", remainingKanjiFormTextNormalized);

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

    private Match Match(string groupPattern, string remainingKanjiFormTextNormalized)
    {
        var pattern = new StringBuilder($"^{groupPattern}");
        foreach (var character in remainingKanjiFormTextNormalized)
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
        return regex.Match(RemainingReadingTextNormalized);
    }
}
