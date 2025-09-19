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
        if (readingState.RemainingText.Length == 0)
        {
            // All "default" readings are based on the remaining text in the reading.
            // If there is no remaining text, then there's nothing to do.
            yield break;
        }
        if (kanjiFormSlice.Runes.Length == 1)
        {
            foreach (var parts in DefaultSingleKanjiParts(kanjiFormSlice, readingState))
            {
                yield return parts;
            }
        }
        else if (kanjiFormSlice.Runes.Length == 2)
        {
            foreach (var parts in DefaultRepeatedKanjiParts(kanjiFormSlice, readingState))
            {
                yield return parts;
            }
        }
    }

    private static IEnumerable<List<Solution.Part>> DefaultSingleKanjiParts(KanjiFormSlice kanjiFormSlice, ReadingState readingState)
    {
        var baseText = kanjiFormSlice.RawText();
        foreach (var reading in DefaultSingleCharacterReadings(kanjiFormSlice, readingState))
        {
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
    }

    private static IEnumerable<string> DefaultSingleCharacterReadings(KanjiFormSlice kanjiFormSlice, ReadingState readingState)
    {
        var currentRune = kanjiFormSlice.Runes[0];
        if (currentRune.IsKana())
        {
            yield return currentRune.KatakanaToHiragana().ToString();
            yield break;
        }

        var previousRune = kanjiFormSlice.PreviousRune();
        var nextRune = kanjiFormSlice.NextRune();
        if (previousRune.IsKanaOrDefault() && nextRune.IsKanaOrDefault())
        {
            var regexReading = readingState.RegexReading(kanjiFormSlice);
            if (regexReading is not null)
            {
                yield return regexReading;
                yield break;
            }
        }

        if (currentRune.IsKanji())
        {
            var readingFirst = readingState.RemainingTextNormalized.First();
            if (IsImpossibleKanjiReadingFirst(readingFirst))
            {
                yield break;
            }
            for (int i = readingState.RemainingText.Length; i > 0; i--)
            {
                yield return readingState.RemainingText[..i];
            }
            yield break;
        }

        var minimumReading = readingState.MinimumReading();
        if (currentRune.ToString() == minimumReading)
        {
            yield return minimumReading;
        }
    }

    private static bool IsImpossibleKanjiReadingFirst(char c) => c switch
    {
        'っ' or
        'ょ' or
        'ゃ' or
        'ゅ' or
        'ん' => true,
        _ => false
    };

    private static IEnumerable<List<Solution.Part>> DefaultRepeatedKanjiParts(KanjiFormSlice kanjiFormSlice, ReadingState readingState)
    {
        var currentRune1 = kanjiFormSlice.Runes[0];
        var currentRune2 = kanjiFormSlice.Runes[1];

        if (!currentRune1.IsKanji() || currentRune1 != currentRune2)
        {
            yield break;
        }

        var previousRune = kanjiFormSlice.PreviousRune();
        var nextRune = kanjiFormSlice.NextRune();

        if (!previousRune.IsKanaOrDefault() || !nextRune.IsKanaOrDefault())
        {
            yield break;
        }

        var reading = readingState.RegexReading(kanjiFormSlice);

        if (reading is null || reading.Length % 2 != 0)
        {
            yield break;
        }

        int halfLength = reading.Length / 2;

        yield return [
            new(kanjiFormSlice.RawRunes[0].ToString(), reading[..halfLength]),
            new(kanjiFormSlice.RawRunes[1].ToString(), reading[halfLength..])
        ];
    }
}
