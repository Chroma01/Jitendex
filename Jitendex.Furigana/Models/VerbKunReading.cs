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

public class VerbKunReading : SuffixedKunReading
{
    public string MasuFormSuffix { get; }

    public VerbKunReading(string text) : base(text)
    {
        MasuFormSuffix = Suffix.VerbToMasuStem() ??
            throw new ArgumentException
            (
                $"Reading `{text}` is not a verb",
                nameof(text)
            );
    }

    public override bool Equals(object? obj) =>
        obj is VerbKunReading reading &&
        base.Equals(obj) &&
        MasuFormSuffix == reading.MasuFormSuffix;

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), MasuFormSuffix);
    }
}
