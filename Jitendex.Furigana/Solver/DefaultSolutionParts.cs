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

using Jitendex.Furigana.Models;
using Jitendex.Furigana.TextExtensions;

namespace Jitendex.Furigana.Solver;

internal class DefaultSolutionParts
{
    public IEnumerable<List<Solution.Part>> Enumerate(KanjiFormSlice kanjiFormSlice, ReadingState readingState)
    {
        if (kanjiFormSlice.Runes.Length == 1)
        {
            var reading = DefaultSingleCharacterReadings(kanjiFormSlice, readingState);
            if (reading is null)
            {
                yield break;
            }
            var baseText = kanjiFormSlice.RawText();
            if (baseText.IsKanaEquivalent(reading))
            {
                yield return [new(baseText, null)];
            }
            else
            {
                var furigana = readingState.RemainingText[..reading.Length];
                yield return [new(baseText, furigana)];
            }
        }
        else if (kanjiFormSlice.Runes.Length == 2)
        {
            var parts = DefaultRepeatedKanjiParts(kanjiFormSlice, readingState);
            if (parts.Count > 0)
            {
                yield return parts;
            }
        }
    }

    private string? DefaultSingleCharacterReadings(KanjiFormSlice kanjiFormSlice, ReadingState readingState)
    {
        var currentRune = kanjiFormSlice.Runes[0];
        if (currentRune.IsKana())
        {
            return currentRune.KatakanaToHiragana().ToString();
        }

        var previousRune = kanjiFormSlice.PreviousRune();
        var nextRune = kanjiFormSlice.NextRune();
        if (previousRune.IsKanaOrDefault() && nextRune.IsKanaOrDefault())
        {
            return readingState.RegexReading(kanjiFormSlice);
        }

        bool currentRuneIsKanji = currentRune.IsKanji();
        if (currentRuneIsKanji && nextRune.IsKanjiOrDefault())
        {
            return readingState.RegularKanjiReading();
        }

        var minimumReading = readingState.MinimumReading();
        if (currentRuneIsKanji)
        {
            return minimumReading;
        }
        else if (currentRune.ToString() == minimumReading)
        {
            return minimumReading;
        }

        return null;
    }

    private List<Solution.Part> DefaultRepeatedKanjiParts(KanjiFormSlice kanjiFormSlice, ReadingState readingState)
    {
        var currentRune1 = kanjiFormSlice.Runes[0];
        var currentRune2 = kanjiFormSlice.Runes[1];

        if (!currentRune1.IsKanji() || currentRune1 != currentRune2)
        {
            return [];
        }

        var previousRune = kanjiFormSlice.PreviousRune();
        var nextRune = kanjiFormSlice.NextRune();

        if (!previousRune.IsKanaOrDefault() || !nextRune.IsKanaOrDefault())
        {
            return [];
        }

        var reading = readingState.RegexReading(kanjiFormSlice);

        if (reading is null || reading.Length % 2 != 0)
        {
            return [];
        }

        int halfLength = reading.Length / 2;

        return [
            new(kanjiFormSlice.RawRunes[0].ToString(), reading[..halfLength]),
            new(kanjiFormSlice.RawRunes[1].ToString(), reading[halfLength..])
        ];
    }
}
