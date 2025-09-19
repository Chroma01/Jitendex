/*
Copyright (c) 2015, 2017 Doublevil
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

internal class CachedSolutionParts
{
    private readonly ReadingCache _readingCache;

    public CachedSolutionParts(ReadingCache readingCache)
    {
        _readingCache = readingCache;
    }

    public IEnumerable<List<Solution.Part>> Enumerate(Entry entry, KanjiFormSlice kanjiFormSlice, ReadingState readingState)
    {
        var readings = GetReadings(entry, kanjiFormSlice);
        var baseText = kanjiFormSlice.RawText();

        foreach (var reading in readings)
        {
            if (!readingState.RemainingTextNormalized.StartsWith(reading))
            {
                continue;
            }
            else if (baseText.IsKanaEquivalent(reading))
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

    private ImmutableArray<string> GetReadings(Entry entry, KanjiFormSlice kanjiFormSlice)
    {
        if (kanjiFormSlice.Runes.Length == 1)
        {
            var rune = kanjiFormSlice.Runes[0];
            var characterReadings = _readingCache.GetCharacterReadings(entry, rune);
            return GetCharacterReadingTexts(kanjiFormSlice, characterReadings);
        }
        else
        {
            return _readingCache.GetSpecialExpressionReadings(kanjiFormSlice.Text());
        }
    }

    private static ImmutableArray<string> GetCharacterReadingTexts(KanjiFormSlice kanjiFormSlice, ImmutableArray<CharacterReading> characterReadings)
    {
        var textSet = new HashSet<string>();

        foreach (var reading in characterReadings)
        {
            if (reading.IsSuffix && kanjiFormSlice.ContainsFirstRune)
            {
                continue;
            }
            if (reading.IsPrefix && kanjiFormSlice.ContainsFinalRune)
            {
                continue;
            }
            foreach (var text in EnumerateReadingTexts(kanjiFormSlice, reading))
            {
                textSet.Add(text);
            }
        }

        return [.. textSet];
    }

    private static IEnumerable<string> EnumerateReadingTexts(KanjiFormSlice kanjiFormSlice, CharacterReading characterReading) =>
        characterReading switch
        {
            OnReading onReading => EnumerateOnReadingTexts(kanjiFormSlice, onReading),
            VerbKunReading verbKunReading => EnumerateVerbKunReadingTexts(kanjiFormSlice, verbKunReading),
            SuffixedKunReading suffixedKunReading => EnumerateSuffixedKunReadingTexts(kanjiFormSlice, suffixedKunReading),
            KunReading kunReading => EnumerateKunReadingTexts(kanjiFormSlice, kunReading),
            _ => throw new NotImplementedException()
        };

    private static IEnumerable<string> EnumerateOnReadingTexts(KanjiFormSlice kanjiFormSlice, OnReading onReading)
    {
        yield return onReading.Reading;

        if (!kanjiFormSlice.ContainsFirstRune)
        {
            foreach (var text in onReading.RendakuReadings)
            {
                yield return text;
            }
        }

        if (!kanjiFormSlice.ContainsFinalRune && onReading.SokuonForm is not null)
        {
            yield return onReading.SokuonForm;
        }

        if (!kanjiFormSlice.ContainsFirstRune && !kanjiFormSlice.ContainsFinalRune)
        {
            foreach (var text in onReading.RendakuSokuonReadings)
            {
                yield return text;
            }
        }
    }

    private static ImmutableArray<string> GetStems(KanjiFormSlice kanjiFormSlice, KunReading kunReading)
    {
        if (kanjiFormSlice.ContainsFirstRune)
        {
            return [kunReading.Reading];
        }
        else
        {
            return kunReading.RendakuReadings.Add(kunReading.Reading);
        }
    }

    private static IEnumerable<string> EnumerateKunReadingTexts(KanjiFormSlice kanjiFormSlice, KunReading kunReading)
    {
        var readings = GetStems(kanjiFormSlice, kunReading);
        foreach (var reading in readings)
        {
            yield return reading;
        }
    }

    private static IEnumerable<string> EnumerateSuffixedKunReadingTexts(KanjiFormSlice kanjiFormSlice, SuffixedKunReading kunReading)
    {
        var stems = GetStems(kanjiFormSlice, kunReading);
        foreach (var stem in stems)
        {
            yield return stem;
        }

        var remainingKanjiFormText = kanjiFormSlice.RemainingText().KatakanaToHiragana();
        for (int i = 0; i < kunReading.Suffix.Length; i++)
        {
            var remainingSuffix = kunReading.Suffix[i..];
            if (remainingKanjiFormText.StartsWith(remainingSuffix))
            {
                yield break;
            }

            foreach (var stem in stems)
            {
                yield return stem + kunReading.Suffix[..(i + 1)];;
            }
        }
    }

    private static IEnumerable<string> EnumerateVerbKunReadingTexts(KanjiFormSlice kanjiFormSlice, VerbKunReading kunReading)
    {
        var stems = GetStems(kanjiFormSlice, kunReading);
        foreach (var stem in stems)
        {
            yield return stem;
        }

        var remainingKanjiFormText = kanjiFormSlice.RemainingText().KatakanaToHiragana();
        for (int i = 0; i < kunReading.Suffix.Length; i++)
        {
            var remainingSuffix = kunReading.Suffix[i..];
            if (remainingKanjiFormText.StartsWith(remainingSuffix))
            {
                yield break;
            }

            var remainingMasuSuffix = kunReading.MasuFormSuffix[i..];
            if (remainingKanjiFormText.StartsWith(remainingMasuSuffix))
            {
                yield break;
            }

            foreach (var stem in stems)
            {
                yield return stem + kunReading.Suffix[..(i + 1)];
                yield return stem + kunReading.MasuFormSuffix[..(i + 1)];
            }
        }
    }
}
