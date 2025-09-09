/*
Copyright (c) 2015 Doublevil
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
using System.Text;

namespace Jitendex.Furigana.Helpers;

public static class KanaHelper
{
    #region Public

    public static bool IsKana(this Rune c) => IsKana(c.Value);
    public static bool IsHiragana(this Rune c) => IsHiragana(c.Value);
    public static bool IsKatakana(this Rune c) => IsKatakana(c.Value);

    public static bool IsKana(this char c) => IsKana((int)c);
    public static bool IsHiragana(this char c) => IsHiragana((int)c);
    public static bool IsKatakana(this char c) => IsKatakana((int)c);

    public static bool IsAllKana(this string text) => text.All(c => c.IsKana());
    public static bool IsAllHiragana(this string text) => text.All(c => c.IsHiragana());
    public static bool IsAllKatakana(this string text) => text.All(c => c.IsKatakana());

    public static string KatakanaToHiragana(this string text) => TransformText(text, _katakanaToHiragana);
    public static string HiraganaToKatakana(this string text) => TransformText(text, _hiraganaToKatakana);

    public static bool IsKanaEquivalent(this string text, string comparisonText) =>
        text.KatakanaToHiragana() == comparisonText.KatakanaToHiragana();

    #endregion

    #region Private

    private static bool IsKana(int c) => IsHiragana(c) || IsKatakana(c);

    private static bool IsHiragana(int c) => c switch
    {
        < 0x3041 => false,
        < 0x3097 => true,
        < 0x3099 => false,
        < 0x30A0 => true,
               _ => false
    };

    private static bool IsKatakana(int c) => c switch
    {
        < 0x30A0 => false,
        < 0x3100 => true,
               _ => false
    };

    private static readonly FrozenDictionary<char, char> _hiraganaToKatakana =
        Enumerable.Range(0x30A1, 86).Concat(Enumerable.Range(0x30FD, 2))
        .Select(x => new KeyValuePair<char, char>((char)(x - 96), (char)x))
        .ToFrozenDictionary();

    private static readonly FrozenDictionary<char, char> _katakanaToHiragana =
        _hiraganaToKatakana.ToFrozenDictionary(x => x.Value, x => x.Key);

    private static string TransformText(string text, FrozenDictionary<char, char> transformer)
    {
        char[] transformedText = new char[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            char original = text[i];
            if (transformer.TryGetValue(original, out char transformed))
                transformedText[i] = transformed;
            else
                transformedText[i] = original;
        }
        return new string(transformedText);
    }

    #endregion
}
