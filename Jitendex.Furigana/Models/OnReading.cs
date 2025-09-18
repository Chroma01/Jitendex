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

using System.Collections.Immutable;
using Jitendex.Furigana.TextExtensions;

namespace Jitendex.Furigana.Models;

public class OnReading : CharacterReading
{
    public string Reading { get; }
    public string? SokuonForm { get; }
    public ImmutableArray<string> RendakuReadings { get; }
    public ImmutableArray<string> RendakuSokuonReadings { get; }

    public OnReading(string text): base(text)
    {
        if (text.Contains('.'))
        {
            throw new ArgumentException(
                "Onyomi must not contain dot splits", nameof(text));
        }

        Reading = text.Replace("-", string.Empty).KatakanaToHiragana();
        SokuonForm = Reading.ToSokuonForm();
        RendakuReadings = Reading.ToRendakuForms();
        RendakuSokuonReadings = SokuonForm?.ToRendakuForms() ?? [];
    }

    public override bool Equals(object? obj) =>
        obj is OnReading reading &&
        IsPrefix == reading.IsPrefix &&
        IsSuffix == reading.IsSuffix &&
        Reading == reading.Reading;

    public override int GetHashCode() =>
        HashCode.Combine(IsPrefix, IsSuffix, Reading);
}
