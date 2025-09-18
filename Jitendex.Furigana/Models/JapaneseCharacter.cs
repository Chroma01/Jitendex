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
using System.Text;
using Jitendex.Furigana.TextExtensions;

namespace Jitendex.Furigana.Models;

public abstract class JapaneseCharacter(Rune rune, IEnumerable<string> readings)
{
    public Rune Rune { get; } = rune;
    public ImmutableArray<CharacterReading> Readings { get; } = readings
        .Select(ReadingFactory)
        .Distinct()
        .ToImmutableArray();

    private static CharacterReading ReadingFactory(string text)
    {
        var normalText = text.Replace("-", string.Empty).Replace(".", string.Empty);

        if (normalText.IsAllHiragana())
        {
            return new KunReading(text);
        }
        else if (normalText.IsAllKatakana())
        {
            return new OnReading(text);
        }
        else
        {
            throw new ArgumentException(
                $"Reading `{text}` must either be all hiragana or all katakana.",
                nameof(text));
        }
    }
}
