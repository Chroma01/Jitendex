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

namespace Jitendex.Furigana.Helpers;

public static class KanaHelper
{
    public static readonly FrozenDictionary<char, char[]> HiraganaToDiacriticForms = new Dictionary<char, char[]>
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

    public static readonly FrozenDictionary<char, char> GodanVerbEndingToMasuInflection = new Dictionary<char, char>
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

    public static readonly FrozenSet<char> SmallTsuRendakus = ['つ', 'く', 'き', 'ち'];

    public static bool IsHiragana(this char c) => c switch
    {
        < '\u3041' => false,
        < '\u3097' => true,
        < '\u3099' => false,
        < '\u30A0' => true,
                 _ => false
    };

    public static bool IsKatakana(this char c) => c switch
    {
        < '\u30A0' => false,
        < '\u3100' => true,
                 _ => false
    };

    public static bool IsKana(this char c) => c switch
    {
        < '\u3041' => false,
        < '\u3097' => true,
        < '\u3099' => false,
        < '\u3100' => true,
                 _ => false
    };

    private static readonly FrozenDictionary<char, char> _hiraganaToKatakana =
        Enumerable.Range(0x30A1, 86)
        .Concat(Enumerable.Range(0x30FD, 2))
        .Select(x => new KeyValuePair<char, char>((char)(x - 96), (char)x))
        .ToFrozenDictionary();

    private static readonly FrozenDictionary<char, char> _katakanaToHiragana =
        _hiraganaToKatakana.ToFrozenDictionary(x => x.Value, x => x.Key);

    public static string KatakanaToHiragana(this string text) => new(text
        .Select(x => _katakanaToHiragana.TryGetValue(x, out char c) ? c : x)
        .ToArray());

    public static string HiraganaToKatakana(this string text) => new(text
        .Select(x => _hiraganaToKatakana.TryGetValue(x, out char c) ? c : x)
        .ToArray());

    public static bool IsAllHiragana(this string text)
    {
        foreach (char c in text)
            if (!c.IsHiragana())
                return false;
        return true;
    }

    public static bool IsAllKatakana(this string text)
    {
        foreach (char c in text)
            if (!c.IsKatakana())
                return false;
        return true;
    }

    public static bool IsAllKana(this string text)
    {
        foreach (char c in text)
            if (!c.IsKana())
                return false;
        return true;
    }

    public static bool AreEquivalent(string a, string b)
    {
        return ToCommonFormat(a) == ToCommonFormat(b);
    }

    private static string ToCommonFormat(string input)
    {
        return input.Replace("・", string.Empty).KatakanaToHiragana();
    }
}
