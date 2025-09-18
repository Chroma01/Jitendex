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

public class KunReading : CharacterReading
{
    public string Stem { get; }
    public string? InflectionalSuffix { get; }
    public ImmutableArray<string> RendakuStems { get; }
    public string? MasuFormSuffix { get; }

    public KunReading(string text) : base(text)
    {
        (Stem, InflectionalSuffix) = SplitReading(text);
        RendakuStems = Stem.ToRendakuForms();
        MasuFormSuffix = InflectionalSuffix?.VerbToMasuStem();
    }

    private static (string, string?) SplitReading(string text)
    {
        var textSplit = text.Replace("-", string.Empty).Split('.');
        var stem = textSplit[0].KatakanaToHiragana();

        return textSplit switch
        {
            { Length: 1 } => (stem, null),
            { Length: 2 } => (stem, textSplit[1].KatakanaToHiragana()),
            _ => throw new ArgumentException($"Reading `{text}` contains too many '.' splits", nameof(text)),
        };
    }

    public override bool Equals(object? obj) =>
        obj is KunReading reading &&
        IsPrefix == reading.IsPrefix &&
        IsSuffix == reading.IsSuffix &&
        Stem == reading.Stem &&
        InflectionalSuffix == reading.InflectionalSuffix;

    public override int GetHashCode() =>
        HashCode.Combine(IsPrefix, IsSuffix, Stem, InflectionalSuffix);
}
