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

namespace Jitendex.Furigana.TextExtensions;

public static class VerbTransform
{
    public static string? VerbToMasuStem(this string text) =>
        LastToMasuStemLast(text) switch
        {
            default(char) => null,
            char masuStemLast => text[..^1] + masuStemLast,
        };

    private static char LastToMasuStemLast(string text) =>
        text.LastOrDefault() switch
        {
            'く' => 'き',
            'ぐ' => 'ぎ',
            'す' => 'し',
            'ず' => 'じ',
            'む' => 'み',
            'る' => 'り',
            'ぶ' => 'び',
            'う' => 'い',
            _ => default
        };
}
