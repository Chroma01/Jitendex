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
    private static bool IsHiragana(char c) => c switch
    {
        < '\u3041' => false,
        < '\u3097' => true,
        < '\u3099' => false,
        < '\u30A0' => true,
                 _ => false
    };

    private static bool IsKatakana(char c) => c switch
    {
        < '\u30A0' => false,
        < '\u3100' => true,
                 _ => false
    };

    private static bool IsKana(char c) => c switch
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

    public static string KatakanaToHiragana(string text) => new(text
        .Select(x => _katakanaToHiragana.TryGetValue(x, out char c) ? c : x)
        .ToArray());

    public static string HiraganaToKatakana(string text) => new(text
        .Select(x => _hiraganaToKatakana.TryGetValue(x, out char c) ? c : x)
        .ToArray());

    public static bool IsAllHiragana(string text)
    {
        foreach (char c in text)
        {
            if (!IsHiragana(c))
                return false;
        }
        return true;
    }

    public static bool IsAllKatakana(string text)
    {
        foreach (char c in text)
        {
            if (!IsKatakana(c))
                return false;
        }
        return true;
    }

    public static bool IsAllKana(string text)
    {
        foreach (char c in text)
        {
            if (!IsKana(c))
                return false;
        }
        return true;
    }

    public static bool AreEquivalent(string a, string b)
    {
        return ToCommonFormat(a) == ToCommonFormat(b);
    }

    private static string ToCommonFormat(string input)
    {
        return KatakanaToHiragana(input.Replace("・", string.Empty));
    }
}
