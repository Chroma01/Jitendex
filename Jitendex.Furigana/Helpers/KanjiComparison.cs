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

using System.Text;

namespace Jitendex.Furigana.Helpers;

public static class KanjiComparison
{
    public static bool IsKanji(this Rune c) => c.Value switch
    {
        (>= 0x4E00)  and (<= 0x9FFF)  => true,  // CJK Unified Ideographs
        (>= 0x3400)  and (<= 0x4DBF)  => true,  // CJK Unified Ideographs Extension A
        (>= 0x20000) and (<= 0x2A6DF) => true,  // CJK Unified Ideographs Extension B
        (>= 0x2A700) and (<= 0x2B73F) => true,  // CJK Unified Ideographs Extension C
        (>= 0x2B740) and (<= 0x2B81F) => true,  // CJK Unified Ideographs Extension D
        (>= 0x2B820) and (<= 0x2CEAF) => true,  // CJK Unified Ideographs Extension E
        (>= 0x2CEB0) and (<= 0x2EBEF) => true,  // CJK Unified Ideographs Extension F
        (>= 0x30000) and (<= 0x3134F) => true,  // CJK Unified Ideographs Extension G
        (>= 0x31350) and (<= 0x323AF) => true,  // CJK Unified Ideographs Extension H
        (>= 0x2EBF0) and (<= 0x2EE5F) => true,  // CJK Unified Ideographs Extension I
        (>= 0x2E80)  and (<= 0x2EFF)  => true,  // CJK Radicals Supplement
        (>= 0x2F00)  and (<= 0x2FDF)  => true,  // Kangxi Radicals
        (>= 0xF900)  and (<= 0xFAFF)  => true,  // CJK Compatibility Ideographs
        (>= 0x2F800) and (<= 0x2FA1F) => true,  // CJK Compatibility Ideographs Supplement
                                    _ => false
    };
}
