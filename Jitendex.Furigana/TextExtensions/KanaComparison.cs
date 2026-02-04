/*
Copyright (c) 2025 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms of
the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

using System.Text;

namespace Jitendex.Furigana.TextExtensions;

public static class KanaComparison
{
    public static bool IsKana(this char c) => IsKana((int)c);
    public static bool IsHiragana(this char c) => IsHiragana((int)c);
    public static bool IsKatakana(this char c) => IsKatakana((int)c);

    public static bool IsKana(this Rune c) => IsKana(c.Value);
    public static bool IsHiragana(this Rune c) => IsHiragana(c.Value);
    public static bool IsKatakana(this Rune c) => IsKatakana(c.Value);

    public static bool IsKanaOrDefault(this Rune c) => c.IsKana() || c == default;
    public static bool IsHiraganaOrDefault(this Rune c) => c.IsHiragana() || c == default;
    public static bool IsKatakanaOrDefault(this Rune c) => c.IsKatakana() || c == default;

    public static bool IsAllKana(this string text) => text.All(IsKana);
    public static bool IsAllHiragana(this string text) => text.All(IsHiragana);
    public static bool IsAllKatakana(this string text) => text.All(IsKatakana);

    public static bool AreAllKana(this IEnumerable<Rune> runes) => runes.All(IsKana);
    public static bool AreAllHiragana(this IEnumerable<Rune> runes) => runes.All(IsHiragana);
    public static bool AreAllKatakana(this IEnumerable<Rune> runes) => runes.All(IsKatakana);

    public static bool IsKanaEquivalent(this char c, char comparison) =>
        c.KatakanaToHiragana() == comparison.KatakanaToHiragana();
    public static bool IsKanaEquivalent(this Rune c, Rune comparison) =>
        c.KatakanaToHiragana() == comparison.KatakanaToHiragana();

    public static bool IsKanaEquivalent(this string text, string comparisonText) => string.Equals
    (
        text.KatakanaToHiragana(),
        comparisonText.KatakanaToHiragana(),
        StringComparison.Ordinal
    );

    private static bool IsKana(int c) => IsHiragana(c) || IsKatakana(c);

    private static bool IsHiragana(int c) => c switch
    {
        (>= 0x3041) and (<= 0x3096) => true,
        (>= 0x3099) and (<= 0x309F) => true,
                                  _ => false
    };

    private static bool IsKatakana(int c) => c switch
    {
        (>= 0x30A0) and (<= 0x30FF) => true,
                                  _ => false
    };
}
