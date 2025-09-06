/*
Copyright (c) 2025 Doublevil
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
using Jitendex.Furigana.Helpers;

namespace Jitendex.Furigana.Models;

public class Kanji
{
    public Rune Character { get => _character; }

    private readonly Rune _character;
    private readonly ImmutableList<string> _readings;
    private readonly ImmutableList<string> _readingsWithNameReadings;

    public Kanji(Rune character, List<string> readings)
    {
        _character = character;
        _readings = readings.ToImmutableList();
        _readingsWithNameReadings = _readings;
    }

    public Kanji(Rune character, List<string> readings, List<string> nameReadings)
    {
        _character = character;
        _readings = readings.ToImmutableList();
        _readingsWithNameReadings = readings.Union(nameReadings).ToImmutableList();
    }

    public List<string> GetPotentialReadings(bool isFirstChar, bool isLastChar, bool isUsedInName)
    {
        var output = new List<string>();
        var readings = isUsedInName ? _readingsWithNameReadings : _readings;

        foreach (string reading in readings)
        {
            if (isFirstChar && reading.StartsWith('-'))
                continue; // No suffix readings for the first char.

            if (isLastChar && reading.EndsWith('-'))
                continue; // No prefix readings for the last char.

            string r = reading.Replace("-", string.Empty).KatakanaToHiragana();

            var dotSplit = r.Split('.');
            if (dotSplit.Length == 1)
            {
                output.Add(r);
            }
            else if (dotSplit.Length == 2)
            {
                var stemChars = dotSplit[0];
                var suffixChars = dotSplit[1];

                output.Add(stemChars);

                var sum = new StringBuilder(stemChars);
                foreach (var suffixChar in suffixChars)
                {
                    sum.Append(suffixChar);
                    output.Add(sum.ToString());
                }

                var verbEnding = suffixChars.Last();
                if (KanaHelper.GodanVerbEndingToMasuInflection.TryGetValue(verbEnding, out char newEnding))
                {
                    var newReading = stemChars + suffixChars[..^1] + newEnding;
                    output.Add(newReading);
                }
            }
            else
            {
                continue;
                // throw new Exception($"Reading `{reading}` for kanji `{kanji.Character}` should only have one dot separator");
            }
        }

        // Add final small tsu rendaku
        if (!isLastChar)
        {
            output.AddRange(GetSmallTsuRendaku(output));
        }

        // Rendaku
        if (!isFirstChar)
        {
            output.AddRange(GetAllRendaku(output));
        }

        return output.Distinct().ToList();
    }

    private static List<string> GetSmallTsuRendaku(List<string> readings)
    {
        var newReadings = new List<string>();
        foreach (var reading in readings)
        {
            if (KanaHelper.SmallTsuRendakus.Contains(reading.Last()))
            {
                newReadings.Add(reading[..^1] + "っ");
            }
        }
        return newReadings;
    }

    private static List<string> GetAllRendaku(List<string> readings)
    {
        var newReadings = new List<string>();
        foreach (var reading in readings)
        {
            if (KanaHelper.HiraganaToDiacriticForms.TryGetValue(reading.First(), out char[]? rendakuChars))
            {
                foreach (var rendakuChar in rendakuChars)
                {
                    newReadings.Add(rendakuChar + reading[1..]);
                }
            }
        }
        return newReadings;
    }

    public override string ToString()
    {
        return Character.ToString();
    }
}
