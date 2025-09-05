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

using System.Text;
using Jitendex.Furigana.Helpers;
using Jitendex.Furigana.Models;

namespace Jitendex.Furigana.Business;

/// <summary>
/// Provides a process that expands a given list of readings by adding rendaku versions and stuff like this.
/// </summary>
public static class ReadingExpander
{
    /// <summary>
    /// Given a kanji, finds and returns all potential readings that it could take in a string.
    /// </summary>
    /// <param name="kanji">Kanji to evaluate.</param>
    /// <param name="isFirstChar">Set to true if this kanji is the first character of the string
    /// that the kanji is found in.</param>
    /// <param name="isLastChar">Set to true if this kanji is the last character of the string
    /// that the kanji is found in.</param>
    /// <param name="useNanori">Set to true to use nanori readings as well.</param>
    /// <returns>A list containing all potential readings that the kanji could take.</returns>
    public static List<string> GetPotentialKanjiReadings(Kanji kanji, bool isFirstChar, bool isLastChar, bool useNanori)
    {
        var output = new List<string>();
        foreach (string reading in useNanori ? kanji.ReadingsWithNanori : kanji.Readings)
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
                foreach (var suffixChar in suffixChars[..^1])
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
}
