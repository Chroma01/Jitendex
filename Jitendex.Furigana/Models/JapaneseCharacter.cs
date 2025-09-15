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
using Jitendex.Furigana.TextExtensions;

namespace Jitendex.Furigana.Models;

public abstract class JapaneseCharacter(Rune rune, IEnumerable<string> readings)
{
    public Rune Rune { get; } = rune;
    public ImmutableArray<string> Readings { get; } = readings
        .Select(KanaTransform.KatakanaToHiragana)
        .Distinct()
        .ToImmutableArray();

    internal ImmutableArray<string> GetReadings(bool isFirstChar, bool isLastChar)
    {
        var readingSet = new HashSet<string>();

        foreach (var reading in Readings)
        {
            // No suffix readings for the first char.
            if (isFirstChar && reading.StartsWith('-'))
                continue;

            // No prefix readings for the last char.
            if (isLastChar && reading.EndsWith('-'))
                continue;

            var r = reading.Replace("-", string.Empty);

            var dotSplit = r.Split('.');
            if (dotSplit.Length == 1)
            {
                readingSet.Add(r);
            }
            else if (dotSplit.Length == 2)
            {
                var stemChars = dotSplit[0];
                var suffixChars = dotSplit[1];

                readingSet.Add(stemChars);

                var sum = new StringBuilder(stemChars);
                foreach (var suffixChar in suffixChars)
                {
                    sum.Append(suffixChar);
                    readingSet.Add(sum.ToString());
                }

                var verbEnding = suffixChars.Last();
                if (GodanVerbEndingToMasuInflection.TryGetValue(verbEnding, out char newEnding))
                {
                    var newReading = stemChars + suffixChars[..^1] + newEnding;
                    readingSet.Add(newReading);
                }
            }
            else
            {
                Console.WriteLine($"Reading `{reading}` for character `{Rune}` has more than one dot separator");
            }
        }

        // Rendaku
        if (!isFirstChar)
        {
            foreach (var reading in GetAllRendaku([.. readingSet]))
            {
                readingSet.Add(reading);
            }
        }

        // Add final small tsu rendaku
        if (!isLastChar)
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

    private static readonly FrozenDictionary<char, char> GodanVerbEndingToMasuInflection = new Dictionary<char, char>
    {
        ['く'] = 'き',
        ['ぐ'] = 'ぎ',
        ['す'] = 'し',
        ['ず'] = 'じ',
        ['む'] = 'み',
        ['る'] = 'り',
        ['ぶ'] = 'び',
        ['う'] = 'い',
    }.ToFrozenDictionary();
}
