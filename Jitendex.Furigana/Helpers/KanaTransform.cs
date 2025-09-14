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

using System.Collections.Frozen;

namespace Jitendex.Furigana.Helpers;

public static class KanaTransform
{
    #region Public

    public static char KatakanaToHiragana(this char c) => Transform(c, _katakanaToHiragana);
    public static char HiraganaToKatakana(this char c) => Transform(c, _hiraganaToKatakana);

    public static string KatakanaToHiragana(this string text) => Transform(text, _katakanaToHiragana);
    public static string HiraganaToKatakana(this string text) => Transform(text, _hiraganaToKatakana);

    #endregion

    #region Private

    private static readonly FrozenDictionary<char, char> _hiraganaToKatakana =
        Enumerable.Range(0x30A1, 86).Concat(Enumerable.Range(0x30FD, 2))
        .Select(x => new KeyValuePair<char, char>((char)(x - 96), (char)x))
        .ToFrozenDictionary();

    private static readonly FrozenDictionary<char, char> _katakanaToHiragana =
        _hiraganaToKatakana.ToFrozenDictionary(x => x.Value, x => x.Key);

    private static char Transform(char character, FrozenDictionary<char, char> transformer)
    {
        if (transformer.TryGetValue(character, out char transformed))
        {
            return transformed;
        }
        else
        {
            return character;
        }
    }

    private static string Transform(string text, FrozenDictionary<char, char> transformer)
    {
        char[] transformedText = new char[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            transformedText[i] = Transform(text[i], transformer);
        }
        return new string(transformedText);
    }

    #endregion
}
