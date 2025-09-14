/*
Copyright (c) 2015 Doublevil
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
using Jitendex.Furigana.InputModels;
using Jitendex.Furigana.OutputModels;

namespace Jitendex.Furigana.Solvers;

internal class RepeatedKanjiSolver : FuriganaSolver
{
    /// <summary>
    /// Solves entries consisting of kanji forms with two repeated kanji and even-length readings.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>ひび【日日】</item>
    /// <item>たまたま【偶々】</item>
    /// <item>ときどき【時時】</item>
    /// </list>
    /// </remarks>
    public override IEnumerable<Solution> Solve(Entry entry)
    {
        if (TryGetBaseText(entry, out var baseText) && TryGetFurigana(entry, out var furigana))
        {
            var solutionBuilder = new SolutionBuilder
            ([
                new(baseText.First, furigana.First),
                new(baseText.Second, furigana.Second),
            ]);

            var solution = solutionBuilder.ToSolution(entry);
            if (solution is not null)
            {
                yield return solution;
            }
        }
    }

    private static bool TryGetBaseText(Entry entry, out (string First, string Second) baseText)
    {
        baseText = (string.Empty, string.Empty);
        var runes = entry.KanjiFormRunes;

        if (runes.Length != 2)
        {
            return false;
        }

        var (firstRune, secondRune) = (runes[0], runes[1]);

        if (firstRune != secondRune || firstRune.IsKana())
        {
            return false;
        }

        var rawRunes = entry.RawKanjiFormRunes;

        baseText =
        (
            rawRunes[0].ToString(),
            rawRunes[1].ToString()
        );

        return true;
    }

    private static bool TryGetFurigana(Entry entry, out (string First, string Second) furigana)
    {
        if (entry.ReadingText.Length % 2 != 0)
        {
            furigana = (string.Empty, string.Empty);
            return false;
        }

        int halfTextLength = entry.ReadingText.Length / 2;

        furigana =
        (
            entry.ReadingText[..halfTextLength],
            entry.ReadingText[halfTextLength..]
        );

        return true;
    }
}
