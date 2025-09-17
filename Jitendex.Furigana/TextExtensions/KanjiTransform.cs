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

using System.Text;

namespace Jitendex.Furigana.TextExtensions;

public static class KanjiTransform
{
    public static Rune[] IterationMarksToKanji(this IList<Rune> runes)
    {
        var normalizedRunes = new Rune[runes.Count];
        var iterationMarkIndices = GetIterationMarkIndices(runes);

        // Replace iteration marks (々) and doubled iteration marks (々々) with their respective kanji.
        for (int i = 0; i < runes.Count; i++)
        {
            if (i > 1 && iterationMarkIndices.Contains(i) && iterationMarkIndices.Contains(i + 1))
            {
                // Double repeater
                normalizedRunes[i] = normalizedRunes[i - 2];
                i++;
                normalizedRunes[i] = normalizedRunes[i - 2];
            }
            else if (i > 0 && iterationMarkIndices.Contains(i))
            {
                // Single repeater
                normalizedRunes[i] = normalizedRunes[i - 1];
            }
            else
            {
                // No repeater
                normalizedRunes[i] = runes[i];
            }
        }

        return normalizedRunes;
    }

    private static HashSet<int> GetIterationMarkIndices(IEnumerable<Rune> rawRunes) => rawRunes
        .Select(static (rune, index) =>
        (
            IsIterationMark: IsIterationMark(rune.Value),
            Index: index
        ))
        .Where(static x => x.IsIterationMark)
        .Select(static x => x.Index)
        .ToHashSet();

    private static bool IsIterationMark(int c) => c switch
    {
        '々' or '〻' => true,
        _ => false
    };
}
