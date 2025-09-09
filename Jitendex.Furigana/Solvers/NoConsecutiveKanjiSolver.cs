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

using System.Text;
using System.Text.RegularExpressions;
using Jitendex.Furigana.Helpers;
using Jitendex.Furigana.InputModels;
using Jitendex.Furigana.OutputModels;

namespace Jitendex.Furigana.Solvers;

internal class NoConsecutiveKanjiSolver : FuriganaSolver
{
    /// <summary>
    /// Attempts to solve furigana in cases where there are no consecutive kanji in the kanji string,
    /// using regular expressions.
    /// </summary>
    public override IEnumerable<IndexedSolution> Solve(Entry entry)
    {
        var runes = entry.KanjiFormRunes;
        var greedyPattern = new StringBuilder("^");
        var lazyPattern = new StringBuilder("^");
        var kanjiIndexes = new List<int>();
        bool consecutiveMarker = false;

        for (int i = 0; i < runes.Length; i++)
        {
            var c = runes[i];
            if (!c.IsKanji())
            {
                // Add the characters to the string. No capture group for kana.
                greedyPattern.Append(c);
                lazyPattern.Append(c);
                consecutiveMarker = false;
            }
            else if (consecutiveMarker)
            {
                // Consecutive kanji. The vocab entry is not eligible for this solution.
                yield break;
            }
            else
            {
                // Add the characters inside a capture group for kanji.
                greedyPattern.Append("(.+)");
                lazyPattern.Append("(.+?)");
                consecutiveMarker = true;
                kanjiIndexes.Add(i);
            }
        }
        greedyPattern.Append('$');
        lazyPattern.Append('$');

        // E.g., for 持ち運ぶ (もちはこぶ) the regexes would be
        // greedy: ^(.+)ち(.+)ぶ$
        // lazy: ^(.+?)ち(.+?)ぶ$

        var regexGreedy = new Regex(greedyPattern.ToString());
        var regexLazy = new Regex(lazyPattern.ToString());

        var matchGreedy = regexGreedy.Match(entry.ReadingText);
        var matchLazy = regexLazy.Match(entry.ReadingText);

        if (matchGreedy.Success && matchLazy.Success)
        {
            var greedySolution = MakeSolutionFromMatch(entry, matchGreedy, kanjiIndexes);
            var lazySolution = MakeSolutionFromMatch(entry, matchLazy, kanjiIndexes);

            // If solutions are not equivalent, then we don't know which is correct.
            if (greedySolution is not null && lazySolution is not null && greedySolution.Equals(lazySolution))
            {
                yield return greedySolution;
            }
        }
    }

    /// <summary>
    /// Creates a furigana solution from a regex match computed in the DoSolve method.
    /// </summary>
    private static IndexedSolution? MakeSolutionFromMatch(Entry entry, Match match, List<int> kanjiIndexes)
    {
        if (match.Groups.Count != kanjiIndexes.Count + 1)
        {
            return null;
        }

        var parts = new List<IndexedFurigana>();
        for (int i = 1; i < match.Groups.Count; i++)
        {
            var group = match.Groups[i];
            parts.Add(new IndexedFurigana(group.Value, kanjiIndexes[i - 1]));
        }

        return new IndexedSolution(entry, parts);
    }
}
