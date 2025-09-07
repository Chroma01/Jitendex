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

namespace Jitendex.Furigana.OutputModels;

internal static class IndexedSolutionChecker
{
    /// <summary>
    /// Checks if the solution is correctly solved for the given coupling of vocab and furigana.
    /// </summary>
    /// <returns>True if the furigana covers all characters of the vocab reading without
    /// overlapping.</returns>
    public static bool Check(IndexedSolution solution)
    {
        // There are three conditions to check:
        // 1. Furigana parts are not overlapping: for any given index in the kanji reading string,
        // there is between 0 and 1 matching furigana parts.
        // 2. All non-kana characters are covered by a furigana part.
        // 3. Reconstituting the kana reading from the kanji reading using the furigana parts when
        // available will give the kana reading of the vocab entry.

        // Keep in mind things like 真っ青 admit a correct "0-2:まっさお" solution. There can be
        // furigana parts covering kana.

        var runes = solution.Vocab.KanjiFormRunes;

        // Check condition 1.
        if (Enumerable.Range(0, runes.Count).Any(i => solution.Parts.Count(f => i >= f.StartIndex && i <= f.EndIndex) > 1))
        {
            // There are multiple furigana parts that are appliable for a given index.
            // This constitutes an overlap and results in the check being negative.
            // Condition 1 failed.
            return false;
        }

        // Now try to reconstitute the reading using the furigana parts.
        // This will allow us to test both 2 and 3.
        var reconstitutedReading = new StringBuilder();
        for (int i = 0; i < runes.Count; i++)
        {
            // Try to find a matching part.
            var matchingPart = solution.Parts.FirstOrDefault(f => i >= f.StartIndex && i <= f.EndIndex);
            if (matchingPart != null)
            {
                // We have a matching part. Add the furigana string to the reconstituted reading.
                reconstitutedReading.Append(matchingPart.Value);

                // Advance i to the end index and continue.
                i = matchingPart.EndIndex;
                continue;
            }

            // Characters that are not covered by a furigana part should be kana.
            var c = runes[i];
            if (c.IsKana())
            {
                // It is kana. Add the character to the reconstituted reading.
                char kana = (char)c.Value;
                reconstitutedReading.Append(kana);
            }
            else
            {
                // This is not kana and this is not covered by any furigana part.
                // The solution is not complete and is therefore not valid.
                // Condition 2 failed.
                return false;
            }
        }

        // Our reconstituted reading should be the same as the kana reading of the vocab.
        if (!solution.Vocab.ReadingText.IsKanaEquivalent(reconstitutedReading.ToString()))
        {
            // It is different. Something is not correct in the furigana reading values.
            // Condition 3 failed.
            return false;
        }

        // Nothing has failed. Everything is good.
        return true;
    }
}
