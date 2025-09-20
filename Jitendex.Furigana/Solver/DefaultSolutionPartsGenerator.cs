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

using System.Text;
using System.Text.RegularExpressions;
using Jitendex.Furigana.Models;
using Jitendex.Furigana.TextExtensions;

namespace Jitendex.Furigana.Solver;

internal class DefaultSolutionParts : ISolutionPartsGenerator
{
    public IEnumerable<List<SolutionPart>> Enumerate(Entry _, KanjiFormSlice kanjiFormSlice, ReadingState readingState) =>
        kanjiFormSlice.Runes switch
        {
            { Length: 1 } => DefaultSingleCharacterParts(kanjiFormSlice, readingState),
            { Length: 2 } => DefaultRepeatedKanjiParts(kanjiFormSlice, readingState),
            _ => []
        };

    private static IEnumerable<List<SolutionPart>> DefaultSingleCharacterParts(KanjiFormSlice kanjiFormSlice, ReadingState readingState)
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
            if (readingState.RemainingText.FirstOrDefault().IsKanaEquivalent((char)currentRune.Value))
            {
                yield return currentRune.ToString();
            }
            yield break;
        }

        var previousRune = kanjiFormSlice.PreviousRune();
        var nextRune = kanjiFormSlice.NextRune();
        if (previousRune.IsKanaOrDefault() && nextRune.IsKanaOrDefault())
        {
            var regexReading = RegexReading(kanjiFormSlice, readingState);
            if (regexReading is not null)
            {
                yield return regexReading;
                yield break;
            }
        }

        if (currentRune.IsKanji())
        {
            var readingFirst = readingState.RemainingTextNormalized.FirstOrDefault();
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

        var minimumReading = readingState.RemainingText.FirstOrDefault();
        if (minimumReading == currentRune.Value)
        {
            yield return minimumReading.ToString();
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

    private static IEnumerable<List<SolutionPart>> DefaultRepeatedKanjiParts(KanjiFormSlice kanjiFormSlice, ReadingState readingState)
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

        var reading = RegexReading(kanjiFormSlice, readingState);

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

    private static string? RegexReading(KanjiFormSlice kanjiFormSlice, ReadingState readingState)
    {
        var remainingKanjiFormText = kanjiFormSlice.RemainingText().KatakanaToHiragana();
        var remainingReadingText = readingState.RemainingTextNormalized;

        var greedyMatch = Match("(.+)", remainingKanjiFormText, remainingReadingText);
        var lazyMatch = Match("(.+?)", remainingKanjiFormText, remainingReadingText);

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

    private static Match Match(string groupPattern, string kanjiFormText, string readingText)
    {
        var pattern = new StringBuilder($"^{groupPattern}");
        bool newGroup = false;
        foreach (var character in kanjiFormText)
        {
            if (character.IsKana())
            {
                pattern.Append(character);
                newGroup = true;
            }
            else if (newGroup)
            {
                pattern.Append(groupPattern);
                newGroup = false;
            }
        }
        pattern.Append('$');
        var regex = new Regex(pattern.ToString());
        return regex.Match(readingText);
    }
}
