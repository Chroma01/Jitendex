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

internal class RegexSolver : FuriganaSolver
{
    private readonly Service _service;

    public RegexSolver(ReadingCache readingCache)
    {
        _service = new Service
        ([
            new IterationSolver(readingCache),
            new RepeatedKanjiSolver(),
            new SingleKanjiSolver(),
        ]);
    }

    /// <summary>
    /// Attempts to solve furigana in cases where there are no consecutive kanji in the kanji string,
    /// using regular expressions.
    /// </summary>
    public override IEnumerable<Solution> Solve(Entry entry)
    {
        if (!entry.KanjiFormRunes.Any(KanaComparison.IsKana))
        {
            yield break;
        }

        var greedyMatch = Match(entry, "(.+)");
        var lazyMatch = Match(entry, "(.+?)");

        if (!greedyMatch.Success || !lazyMatch.Success)
        {
            yield break;
        }

        var greedySolution = SolveMatchGroups(entry, greedyMatch);
        var lazySolution = SolveMatchGroups(entry, lazyMatch);

        // If solutions are not equivalent, then we don't know which is correct.
        if (greedySolution is not null && lazySolution is not null && greedySolution.Equals(lazySolution))
        {
            yield return greedySolution;
        }
    }

    private Solution? SolveMatchGroups(Entry entry, Match match)
    {
        throw new NotImplementedException();
    }

    private static Match Match(Entry entry, string groupPattern)
    {
        var pattern = new StringBuilder("^");
        var normalizedKanjiFormText = entry.KanjiFormText.KatakanaToHiragana();
        foreach (var character in normalizedKanjiFormText)
        {
            if (character.IsKana())
            {
                pattern.Append(character);
            }
            else
            {
                pattern.Append(groupPattern);
            }
        }
        pattern.Append('$');
        var regex = new Regex(pattern.ToString());
        return regex.Match(entry.NormalizedReadingText);
    }
}
