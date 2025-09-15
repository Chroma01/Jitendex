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
using Jitendex.Furigana.Models;
using Jitendex.Furigana.TextExtensions;

namespace Jitendex.Furigana.Solver;

internal class SliceReadingCache
{
    private readonly KanjiFormSlice _kanjiFormSlice;
    private readonly ImmutableArray<string> _cachedReadings;

    public SliceReadingCache(Entry entry, KanjiFormSlice kanjiFormSlice, ReadingCache readingCache)
    {
        _kanjiFormSlice = kanjiFormSlice;
        _cachedReadings = readingCache.GetReadings(entry, kanjiFormSlice);
    }

    public IEnumerable<List<Solution.Part>> EnumerateParts(ReadingState readingState)
    {
        var baseText = _kanjiFormSlice.RawText();

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
        if (_kanjiFormSlice.Runes.Length == 1)
        {
            var reading = DefaultSingleCharacterReading(readingState);
            if (reading is null || _cachedReadings.Contains(reading))
            {
                return [];
            }
            else if (baseText.IsKanaEquivalent(reading))
            {
                return [new(baseText, null)];
            }
            else
            {
                var furigana = readingState.RemainingReadingText[..reading.Length];
                return [new(baseText, furigana)];
            }
        }
        else if (_kanjiFormSlice.Runes.Length == 2)
        {
            var parts = DefaultDoubleCharacterParts(readingState);
            if (parts.Count > 0)
            {
                return parts;
            }
        }
        return [];
    }

    private string? DefaultSingleCharacterReading(ReadingState readingState)
    {
        var currentRune = _kanjiFormSlice.Runes[0];
        var previousRune = _kanjiFormSlice.PreviousRune();
        var nextRune = _kanjiFormSlice.NextRune();

        if (currentRune.IsKana())
        {
            return currentRune.KatakanaToHiragana().ToString();
        }
        else if (previousRune.IsKanaOrDefault() && nextRune.IsKanaOrDefault())
        {
            return readingState.RegexReading(_kanjiFormSlice);
        }
        else if (!currentRune.IsKanji())
        {
            return null;
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
        var currentRune1 = _kanjiFormSlice.Runes[0];
        var currentRune2 = _kanjiFormSlice.Runes[1];

        if (currentRune1.IsKana() || currentRune1 != currentRune2)
        {
            return [];
        }

        var previousRune = _kanjiFormSlice.PreviousRune();
        var nextRune = _kanjiFormSlice.NextRune();

        if (!previousRune.IsKanaOrDefault() || !nextRune.IsKanaOrDefault())
        {
            return [];
        }

        var reading = readingState.RegexReading(_kanjiFormSlice);

        if (reading is null || reading.Length % 2 != 0)
        {
            return [];
        }

        int halfLength = reading.Length / 2;

        return [
            new(_kanjiFormSlice.RawRunes[0].ToString(), reading[..halfLength]),
            new(_kanjiFormSlice.RawRunes[1].ToString(), reading[halfLength..])
        ];
    }
}
