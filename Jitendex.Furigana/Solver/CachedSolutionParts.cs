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

using System.Collections.Frozen;
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
            return GetReadings(characterReadings, kanjiFormSlice);
        }
        else
        {
            return _readingCache.GetSpecialExpressionReadings(kanjiFormSlice.Text());
        }
    }

    private ImmutableArray<string> GetReadings(ImmutableArray<CharacterReading> characterReadings, KanjiFormSlice kanjiFormSlice)
    {
        var readingSet = new HashSet<string>();

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

            readingSet.Add(reading.Stem);

            if (reading.Okurigana is null)
            {
                continue;
            }

            var sum = new StringBuilder(reading.Stem);
            foreach (var okuriganaChar in reading.Okurigana[..^1])
            {
                sum.Append(okuriganaChar);
                readingSet.Add(sum.ToString());
            }

            var verbEnding = reading.Okurigana.Last();
            char? newEnding = GodanVerbEndingToMasuInflection(verbEnding);
            if (newEnding is not null)
            {
                var newReading = reading.Text[..^1] + newEnding;
                readingSet.Add(newReading);
            }
            else
            {
                readingSet.Add(reading.Text);
            }
        }

        // Rendaku
        if (!kanjiFormSlice.ContainsFirstRune)
        {
            foreach (var reading in GetAllRendaku([.. readingSet]))
            {
                readingSet.Add(reading);
            }
        }

        // Add final small tsu rendaku
        if (!kanjiFormSlice.ContainsFinalRune)
        {
            foreach (var reading in GetSmallTsuRendaku([.. readingSet]))
            {
                readingSet.Add(reading);
            }
        }

        return [.. readingSet];
    }

    private static IEnumerable<string> GetSmallTsuRendaku(IEnumerable<string> readings)
    {
        foreach (var reading in readings)
        {
            if (IsConvertibleToSmallTsu(reading.Last()))
            {
                yield return reading[..^1] + "っ";
            }
        }
    }

    private static IEnumerable<string> GetAllRendaku(IEnumerable<string> readings)
    {
        foreach (var reading in readings)
        {
            if (HiraganaToDiacriticForms.TryGetValue(reading.First(), out char[]? rendakuChars))
            {
                foreach (var rendakuChar in rendakuChars)
                {
                    yield return rendakuChar + reading[1..];
                }
            }
        }
    }

    private static bool IsConvertibleToSmallTsu(char c) => c switch
    {
        'つ' or 'く' or 'き' or 'ち' => true,
        _ => false
    };

    private static char? GodanVerbEndingToMasuInflection(char c) => c switch
    {
        'く' => 'き',
        'ぐ' => 'ぎ',
        'す' => 'し',
        'ず' => 'じ',
        'む' => 'み',
        'る' => 'り',
        'ぶ' => 'び',
        'う' => 'い',
        _ => null
    };

    private static readonly FrozenDictionary<char, char[]> HiraganaToDiacriticForms = new Dictionary<char, char[]>
    {
        ['か'] = ['が'],
        ['き'] = ['ぎ'],
        ['く'] = ['ぐ'],
        ['け'] = ['げ'],
        ['こ'] = ['ご'],
        ['さ'] = ['ざ'],
        ['し'] = ['じ'],
        ['す'] = ['ず'],
        ['せ'] = ['ぜ'],
        ['そ'] = ['ぞ'],
        ['た'] = ['だ'],
        ['ち'] = ['ぢ', 'じ'],
        ['つ'] = ['づ', 'ず'],
        ['て'] = ['で'],
        ['と'] = ['ど'],
        ['は'] = ['ば', 'ぱ'],
        ['ひ'] = ['び', 'ぴ'],
        ['ふ'] = ['ぶ', 'ぷ'],
        ['へ'] = ['べ', 'ぺ'],
        ['ほ'] = ['ぼ', 'ぽ'],
    }.ToFrozenDictionary();
}
