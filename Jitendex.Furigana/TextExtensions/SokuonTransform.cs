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

public static class SokuonTransform
{
    public static string? ToSokuonForm(this string text) => LastCanGeminate(text) switch
    {
        true => text[..^1] + "っ",
        false => null,
    };

    private static bool LastCanGeminate(string text) => text.LastOrDefault() switch
    {
        'つ' or
        'く' or
        'き' or
        'ち' => true,
        _ => false
    };
}
