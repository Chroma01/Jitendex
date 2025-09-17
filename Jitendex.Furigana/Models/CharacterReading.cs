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

using Jitendex.Furigana.TextExtensions;

namespace Jitendex.Furigana.Models;

public enum Yomi
{
    Unknown,
    Kun,
    On,
}

public class CharacterReading
{
    public bool IsPrefix { get; }
    public bool IsSuffix { get; }
    public Yomi Yomi { get; }
    public string Stem { get; }
    public string? Okurigana { get; }

    public CharacterReading(string text)
    {
        IsPrefix = text.EndsWith('-');
        IsSuffix = text.StartsWith('-');
        (Stem, Okurigana) = SplitReading(text);
        Yomi = GetYomi(text);
    }

    private static (string, string?) SplitReading(string text)
    {
        var textSplit = text.Replace("-", string.Empty).Split('.');
        if (textSplit.Length == 1)
        {
            return (
                textSplit[0].KatakanaToHiragana(),
                null
            );
        }
        else if (textSplit.Length == 2)
        {
            return (
                textSplit[0].KatakanaToHiragana(),
                textSplit[1].KatakanaToHiragana()
            );
        }
        else
        {
            throw new ArgumentException($"Reading `{text}` contains too many '.' splits", nameof(text));
        }
    }

    private static Yomi GetYomi(string text)
    {
        text = text.Replace("-", string.Empty).Replace(".", string.Empty);
        if (text.IsAllHiragana())
        {
            return Yomi.Kun;
        }
        else if (text.IsAllKatakana())
        {
            return Yomi.On;
        }
        else
        {
            return Yomi.Unknown;
        }
    }

    public override bool Equals(object? obj) =>
        obj is CharacterReading reading &&
        IsPrefix == reading.IsPrefix &&
        IsSuffix == reading.IsSuffix &&
        Yomi == reading.Yomi &&
        Stem == reading.Stem &&
        Okurigana == reading.Okurigana;

    public override int GetHashCode() =>
        HashCode.Combine(IsPrefix, IsSuffix, Yomi, Stem, Okurigana);
}
