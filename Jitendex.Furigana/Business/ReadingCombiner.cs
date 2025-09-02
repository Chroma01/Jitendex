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

namespace Jitendex.Furigana.Business;

/// <summary>
/// Provides a method that combines all potential readings between them
/// and returns the list of possible combinations.
/// </summary>
public static class ReadingCombiner
{
    /// <summary>
    /// Finds and returns all possible combinations of the given list of strings that respect
    /// the order.
    /// </summary>
    /// <param name="readings">List containing string lists that can be combined with others.</param>
    /// <param name="solutions">List containing all possible solutions that should be searched.</param>
    /// <returns>All possible combinations.</returns>
    public static List<string> CombineReadings(List<List<string>> readings, IEnumerable<string> solutions)
    {
        return CombineReadings(readings, string.Empty, solutions);
    }

    private static List<string> CombineReadings(List<List<string>> readings, string prefix, IEnumerable<string> solutions)
    {
        // Recursion exit conditions.
        if (!solutions.Where(s => s.StartsWith(prefix)).Any())
        {
            return [];
        }
        if (readings.Count == 1)
        {
            // If we only have one list of readings, just return these readings prepended
            // with the prefix.
            return readings.First().Select(s => prefix + s).Intersect(solutions).ToList();
        }

        // Recursion general behavior.
        var output = new List<string>();
        // Take the first reading list.
        var firstReadingList = readings.First();
        foreach (string reading in firstReadingList)
        {
            // For each reading of that first list, create a matching prefix and
            // make a recursive call without the first reading list.
            output.AddRange(CombineReadings(readings.Skip(1).ToList(), prefix + reading + SeparatorHelper.FuriganaSeparator, solutions));
        }

        return output;
    }
}
