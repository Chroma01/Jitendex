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
using Jitendex.Furigana.Models;
using Jitendex.Furigana.TextExtensions;

namespace Jitendex.Furigana.Solver;

internal class DefaultSolutionParts : ISolutionPartsGenerator
{
    private readonly ResourceCache _resourceCache;

    public DefaultSolutionParts(ResourceCache resourceCache)
    {
        _resourceCache = resourceCache;
    }

    public IEnumerable<List<SolutionPart>> Enumerate(in Entry _, in KanjiFormSlice kanjiFormSlice, in ReadingState readingState) =>
        kanjiFormSlice.Runes switch
        {
            { Length: 1 } => DefaultSingleCharacterParts(kanjiFormSlice, readingState),
            { Length: 2 } => DefaultRepeatedKanjiParts(kanjiFormSlice, readingState),
            _ => []
        };

    private ImmutableArray<List<SolutionPart>> DefaultSingleCharacterParts(in KanjiFormSlice kanjiFormSlice, in ReadingState readingState)
    {
        var baseText = kanjiFormSlice.RawText();
        var readings = DefaultSingleCharacterReadings(kanjiFormSlice, readingState);
        var partsBuilder = ImmutableArray.CreateBuilder<List<SolutionPart>>(readings.Length);
        foreach (var reading in readings)
        {
            if (baseText.IsKanaEquivalent(reading))
            {
                partsBuilder.Add([new SolutionPart
                {
                    BaseText = baseText
                }]);
            }
            else
            {
                partsBuilder.Add([new SolutionPart
                {
                    BaseText = baseText,
                    Furigana = readingState.RemainingText[..reading.Length].ToString(),
                    Readings = [_resourceCache.NewReading(kanjiFormSlice.Runes[0], reading)],
                }]);
            }
        }
        return partsBuilder.ToImmutableArray();
    }

    private static ImmutableArray<string> DefaultSingleCharacterReadings(in KanjiFormSlice kanjiFormSlice, in ReadingState readingState)
    {
        var currentRune = kanjiFormSlice.Runes[0];

        if (currentRune.IsKana())
        {
            if (readingState.FirstRemainingChar.IsKanaEquivalent((char)currentRune.Value))
            {
                return [currentRune.ToString()];
            }
        }

        if (kanjiFormSlice.PreviousRune.IsKanaOrDefault() && kanjiFormSlice.NextRune.IsKanaOrDefault())
        {
            var regexReading = RegexReading(kanjiFormSlice, readingState);
            if (regexReading is not null)
            {
                return [regexReading];
            }
        }

        if (currentRune.IsKanji())
        {
            if (IsImpossibleKanjiReadingFirst(readingState.FirstRemainingNormalizedChar))
            {
                return [];
            }
            var remainingText = readingState.RemainingText;
            var readingsBuilder = ImmutableArray.CreateBuilder<string>(remainingText.Length);
            for (int i = 1; i <= remainingText.Length; i++)
            {
                readingsBuilder.Add(remainingText[..i].ToString());
            }
            return readingsBuilder.ToImmutableArray();
        }

        if (readingState.FirstRemainingChar == currentRune.Value)
        {
            return [readingState.FirstRemainingChar.ToString()];
        }

        return [];
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

    private ImmutableArray<List<SolutionPart>> DefaultRepeatedKanjiParts(in KanjiFormSlice kanjiFormSlice, in ReadingState readingState)
    {
        var currentRune1 = kanjiFormSlice.Runes[0];
        var currentRune2 = kanjiFormSlice.Runes[1];

        if (!currentRune1.IsKanji() || currentRune1 != currentRune2)
        {
            return [];
        }

        if (!kanjiFormSlice.PreviousRune.IsKanaOrDefault() || !kanjiFormSlice.NextRune.IsKanaOrDefault())
        {
            return [];
        }

        var reading = RegexReading(kanjiFormSlice, readingState);

        if (reading is null || reading.Length % 2 != 0)
        {
            return [];
        }

        int halfLength = reading.Length / 2;

        return
        [[
            new SolutionPart
            {
                BaseText = kanjiFormSlice.RawRunes[0].ToString(),
                Furigana = reading[..halfLength],
                Readings = [_resourceCache.NewReading(currentRune1, reading[..halfLength])],
            },
            new SolutionPart
            {
                BaseText = kanjiFormSlice.RawRunes[1].ToString(),
                Furigana = reading[halfLength..],
                Readings = [_resourceCache.NewReading(currentRune2, reading[halfLength..])],
            }
        ]];
    }

    private static string? RegexReading(in KanjiFormSlice kanjiFormSlice, in ReadingState readingState)
    {
        var remainingKanjiFormText = kanjiFormSlice.RemainingText().KatakanaToHiragana();
        var remainingReadingText = readingState.RemainingTextNormalized.ToString();

        var greedyMatch = Match("(.+)", remainingKanjiFormText, remainingReadingText);
        var lazyMatch = Match("(.+?)", remainingKanjiFormText, remainingReadingText);

        if (!greedyMatch.Success || !lazyMatch.Success)
        {
            return null;
        }

        var greedyValue = greedyMatch.Groups[1].Value;
        var lazyValue = lazyMatch.Groups[1].Value;

        if (greedyValue != string.Empty && string.Equals(greedyValue, lazyValue, StringComparison.Ordinal))
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
