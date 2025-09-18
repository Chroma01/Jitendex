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
using System.Text;
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
            return GetReadingTexts(kanjiFormSlice, characterReadings);
        }
        else
        {
            return _readingCache.GetSpecialExpressionReadings(kanjiFormSlice.Text());
        }
    }

    private static ImmutableArray<string> GetReadingTexts(KanjiFormSlice kanjiFormSlice, ImmutableArray<CharacterReading> characterReadings)
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
            KunReading kunReading => EnumerateKunReadingTexts(kanjiFormSlice, kunReading),
            OnReading onReading => EnumerateOnReadingTexts(kanjiFormSlice, onReading),
            _ => throw new NotImplementedException()
        };

    private static IEnumerable<string> EnumerateKunReadingTexts(KanjiFormSlice kanjiFormSlice, KunReading kunReading)
    {
        var stems = kanjiFormSlice.ContainsFirstRune ?
            [kunReading.Stem] :
            kunReading.RendakuStems.Add(kunReading.Stem);

        foreach (var stem in stems)
        {
            yield return stem;
        }

        if (kunReading.InflectionalSuffix is null)
        {
            yield break;
        }

        foreach (var stem in stems)
        {
            var sum = new StringBuilder(stem);
            foreach (var suffixChar in kunReading.InflectionalSuffix[..^1])
            {
                sum.Append(suffixChar);
                yield return sum.ToString();
            }

            if (kunReading.MasuFormSuffix is not null)
            {
                yield return stem + kunReading.MasuFormSuffix;
            }
            else
            {
                yield return stem + kunReading.InflectionalSuffix;
            }
        }
    }

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
}
