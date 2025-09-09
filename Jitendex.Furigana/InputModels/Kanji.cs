/*
Copyright (c) 2025 Doublevil
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
using Jitendex.Furigana.Helpers;

namespace Jitendex.Furigana.InputModels;

public class Kanji
{
    public Rune Character { get; }
    public ImmutableArray<string> Readings { get; }
    public ImmutableArray<string> NameReadings { get; }
    private readonly ImmutableArray<string> _readingsWithNameReadings;

    public Kanji(Rune character, IEnumerable<string> readings)
    {
        Character = character;
        Readings = [.. readings];
        NameReadings = [];
        _readingsWithNameReadings = [.. Readings];
    }

    public Kanji(Rune character, IEnumerable<string> readings, IEnumerable<string> nameReadings)
    {
        Character = character;
        Readings = [.. readings];
        NameReadings = [.. nameReadings];
        _readingsWithNameReadings = [.. readings.Union(nameReadings)];
    }

    internal ImmutableArray<string> GetPotentialReadings(bool isFirstChar, bool isLastChar, bool isUsedInName)
    {
        var output = new HashSet<string>();
        var readings = isUsedInName ? _readingsWithNameReadings : Readings;

        foreach (string reading in readings)
        {
            // No suffix readings for the first char.
            if (isFirstChar && reading.StartsWith('-'))
                continue;

            // No prefix readings for the last char.
            if (isLastChar && reading.EndsWith('-'))
                continue;

            // Ensure all readings are in hiragana
            string r = reading.Replace("-", string.Empty).KatakanaToHiragana();

            var dotSplit = r.Split('.');
            if (dotSplit.Length == 1)
            {
                output.Add(r);
            }
            else if (dotSplit.Length == 2)
            {
                var stemChars = dotSplit[0];
                var suffixChars = dotSplit[1];

                output.Add(stemChars);

                var sum = new StringBuilder(stemChars);
                foreach (var suffixChar in suffixChars)
                {
                    sum.Append(suffixChar);
                    output.Add(sum.ToString());
                }

                var verbEnding = suffixChars.Last();
                if (GodanVerbEndingToMasuInflection.TryGetValue(verbEnding, out char newEnding))
                {
                    var newReading = stemChars + suffixChars[..^1] + newEnding;
                    output.Add(newReading);
                }
            }
            else
            {
                // throw new Exception($"Reading `{reading}` for kanji `{kanji.Character}` should only have one dot separator");
            }
        }

        // Rendaku
        if (!isFirstChar)
        {
            foreach (var reading in GetAllRendaku([.. output]))
                output.Add(reading);
        }

        // Add final small tsu rendaku
        if (!isLastChar)
        {
            foreach (var reading in GetSmallTsuRendaku([.. output]))
                output.Add(reading);
        }

        return [.. output];
    }

    private static IEnumerable<string> GetSmallTsuRendaku(IEnumerable<string> readings)
    {
        foreach (var reading in readings)
        {
            if (SmallTsuRendakus.Contains(reading.Last()))
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

    private static readonly FrozenSet<char> SmallTsuRendakus = ['つ', 'く', 'き', 'ち'];

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
