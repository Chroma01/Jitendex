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
using Jitendex.Furigana.Models.TextUnits;
using Jitendex.Furigana.Models.TextUnits.Readings;

namespace Jitendex.Furigana.Solver;

internal class CachedSolutionPartsGenerator : ISolutionPartsGenerator
{
    private readonly ResourceCache _resourceCache;

    public CachedSolutionPartsGenerator(ResourceCache resourceCache)
    {
        _resourceCache = resourceCache;
    }

    public IEnumerable<List<SolutionPart>> Enumerate(Entry entry, KanjiFormSlice kanjiFormSlice, ReadingState readingState)
    {
        var textToReadings = GetReadings(entry, kanjiFormSlice);
        var baseText = kanjiFormSlice.RawText();

        foreach (var (text, readings) in textToReadings)
        {
            if (!readingState.RemainingTextNormalized.StartsWith(text))
            {
                continue;
            }
            else if (baseText.IsKanaEquivalent(text))
            {
                yield return [new SolutionPart { BaseText = baseText }];
            }
            else
            {
                yield return [new SolutionPart
                {
                    BaseText = baseText,
                    Furigana = readingState.RemainingText[..text.Length],
                    Readings = [.. readings],
                }];
            }
        }
    }

    private Dictionary<string, List<IReading>> GetReadings(Entry entry, KanjiFormSlice kanjiFormSlice)
    {
        if (kanjiFormSlice.Runes.Length == 1)
        {
            var rune = kanjiFormSlice.Runes[0];
            if (_resourceCache.Characters.TryGetValue(rune.Value, out JapaneseCharacter? character))
            {
                return GetCharacterReadingTexts(entry, kanjiFormSlice, character);
            }
        }
        else
        {
            var text = kanjiFormSlice.Text();
            if (_resourceCache.Compounds.TryGetValue(text, out JapaneseCompound? compound))
            {
                return compound.Readings
                    .Select(x => new KeyValuePair<string, List<IReading>>(x.Reading, [x]))
                    .ToDictionary();
            }
        }
        return [];
    }

    private static Dictionary<string, List<IReading>> GetCharacterReadingTexts(Entry entry, KanjiFormSlice kanjiFormSlice, JapaneseCharacter character)
    {
        var textToReadings = new Dictionary<string, List<IReading>>();

        foreach (var reading in character.Readings)
        {
            if (reading is NameReading && entry is not NameEntry)
            {
                continue;
            }
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
                if (textToReadings.TryGetValue(text, out List<IReading>? readings))
                {
                    readings.Add(reading);
                }
                else
                {
                    textToReadings[text] = [reading];
                }
            }
        }

        return textToReadings;
    }

    private static IEnumerable<string> EnumerateReadingTexts(KanjiFormSlice kanjiFormSlice, CharacterReading characterReading) =>
        characterReading switch
        {
            NonKanjiReading nonKanjiReading => EnumerateNonKanjiReadingTexts(nonKanjiReading),
            NameReading nameReading => EnumerateNameReadingTexts(nameReading),
            OnReading onReading => EnumerateOnReadingTexts(kanjiFormSlice, onReading),
            VerbKunReading verbKunReading => EnumerateVerbKunReadingTexts(kanjiFormSlice, verbKunReading),
            SuffixedKunReading suffixedKunReading => EnumerateSuffixedKunReadingTexts(kanjiFormSlice, suffixedKunReading),
            KunReading kunReading => EnumerateKunReadingTexts(kanjiFormSlice, kunReading),
            _ => throw new NotImplementedException()
        };

    private static IEnumerable<string> EnumerateNonKanjiReadingTexts(NonKanjiReading nonKanjiReading)
    {
        yield return nonKanjiReading.Reading;
    }

    private static IEnumerable<string> EnumerateNameReadingTexts(NameReading nameReading)
    {
        yield return nameReading.Reading;
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
                yield return stem + kunReading.Suffix[..(i + 1)]; ;
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
            }
        }

        foreach (var stem in stems)
        {
            yield return stem + kunReading.MasuFormSuffix;
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
}
