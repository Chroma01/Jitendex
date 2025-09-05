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

using Jitendex.Furigana.Helpers;
using Jitendex.Furigana.Models;

namespace Jitendex.Furigana.Business;

/// <summary>
/// Provides a process that expands a given list of readings by adding rendaku versions and stuff like this.
/// </summary>
public static class SpecialReadingExpander
{
    /// <summary>
    /// Given a special reading expression, returns all potential kana readings the expression could use.
    /// </summary>
    /// <param name="expression">Target special reading expression.</param>
    /// <param name="isFirstChar">Set to true if the first character of the expression is the first
    /// character of the string that the expression is found in.</param>
    /// <param name="isLastChar">Set to true if the last character of the expression is the last
    /// character of the string that the expression is found in.</param>
    /// <returns>A list containing all potential readings the expression could assume.</returns>
    public static List<SpecialReading> GetPotentialSpecialReadings(SpecialExpression expression, bool isFirstChar, bool isLastChar)
    {
        var specialReadings = new List<SpecialReading>(expression.Readings);

        // Add final small tsu rendaku
        if (!isLastChar)
        {
            var newSpecialReadings = new List<SpecialReading>();
            foreach (var specialReading in specialReadings)
            {
                if (!KanaHelper.SmallTsuRendakus.Contains(specialReading.ReadingText.Last()))
                    continue;

                string newKanaReading = specialReading.ReadingText[..^1] + "っ";
                var newSolution = new FuriganaSolution
                (
                    specialReading.Solution.Vocab,
                    specialReading.Solution.FuriganaParts.Clone()
                );
                var newSpecialReading = new SpecialReading(newKanaReading, newSolution);

                var index = newSpecialReading.Solution.Vocab.KanjiFormText.Length - 1;
                var affectedParts = newSpecialReading.Solution.GetPartsForIndex(index);

                foreach (var part in affectedParts)
                {
                    part.Value = part.Value[..^1] + "っ";
                }
                newSpecialReadings.Add(newSpecialReading);
            }
            specialReadings.AddRange(newSpecialReadings);
        }

        // Rendaku
        if (!isFirstChar)
        {
            var newSpecialReadings = new List<SpecialReading>();
            foreach (var specialReading in specialReadings)
            {
                if (KanaHelper.HiraganaToDiacriticForms.TryGetValue(specialReading.ReadingText.First(), out char[]? rendakuChars))
                {
                    foreach (var renChar in rendakuChars)
                    {
                        var newKanaReading = renChar + specialReading.ReadingText[1..];
                        var newSolution = new FuriganaSolution
                        (
                            specialReading.Solution.Vocab,
                            specialReading.Solution.FuriganaParts.Clone()
                        );
                        var newReading = new SpecialReading(newKanaReading, newSolution);

                        var affectedParts = newReading.Solution.GetPartsForIndex(0);
                        foreach (var part in affectedParts)
                        {
                            part.Value = renChar + part.Value[1..];
                        }
                        newSpecialReadings.Add(newReading);
                    }
                }
            }
            specialReadings.AddRange(newSpecialReadings);
        }

        return specialReadings.Distinct().ToList();
    }
}
