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
using Jitendex.Furigana.Models.TextUnits.Readings;

namespace Jitendex.Furigana.Models.TextUnits;

public class Kanji : JapaneseCharacter
{
    public override ImmutableArray<CharacterReading> Readings { get; }

    public Kanji(Rune rune, IEnumerable<string> readings, IEnumerable<string> nanori) : base(rune)
    {
        Readings = readings
            .Select(ReadingFactory)
            .Concat(nanori.Select(static text => new NameReading(text)))
            .Distinct()
            .ToImmutableArray();
    }

    private static CharacterReading ReadingFactory(string text)
    {
        var hyphenlessText = text.Replace("-", string.Empty);
        var normalText = hyphenlessText.Replace(".", string.Empty);

        return (normalText.IsAllKatakana(), normalText.IsAllHiragana()) switch
        {
            (true, false) => new OnReading(text),

            (false, true) => (hyphenlessText.Split("."), normalText.VerbToMasuStem()) switch
            {
                ({ Length: 1 }, _) => new KunReading(text),
                ({ Length: 2 }, null) => new SuffixedKunReading(text),
                ({ Length: 2 }, not null) => new VerbKunReading(text),

                _ => throw new ArgumentException
                (
                    $"Reading `{text}` has too many '.' delimiters",
                    nameof(text)
                )
            },

            (true, true) => throw new ArgumentException
            (
                "Reading must not be empty",
                nameof(text)
            ),

            (false, false) => throw new ArgumentException
            (
                $"Reading `{text}` must either be all hiragana or all katakana",
                nameof(text)
            )
        };
    }
}
