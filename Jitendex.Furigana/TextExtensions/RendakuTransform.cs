/*
Copyright (c) 2025 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

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

namespace Jitendex.Furigana.TextExtensions;

public static class RendakuTransform
{
    public static ImmutableArray<string> ToRendakuForms(this string text) => FirstToRendakuChars(text) switch
    {
        [] => [],
        var rendakuChars => rendakuChars
            .Select(rendaku => rendaku + text[1..])
            .ToImmutableArray()
    };

    private static ImmutableArray<char> FirstToRendakuChars(string x) => x.FirstOrDefault() switch
    {
        'か' => ['が'],
        'き' => ['ぎ'],
        'く' => ['ぐ'],
        'け' => ['げ'],
        'こ' => ['ご'],
        'さ' => ['ざ'],
        'し' => ['じ'],
        'す' => ['ず'],
        'せ' => ['ぜ'],
        'そ' => ['ぞ'],
        'た' => ['だ'],
        'ち' => ['ぢ', 'じ'],
        'つ' => ['づ', 'ず'],
        'て' => ['で'],
        'と' => ['ど'],
        'は' => ['ば', 'ぱ'],
        'ひ' => ['び', 'ぴ'],
        'ふ' => ['ぶ', 'ぷ'],
        'へ' => ['べ', 'ぺ'],
        'ほ' => ['ぼ', 'ぽ'],
        _ => []
    };
}
