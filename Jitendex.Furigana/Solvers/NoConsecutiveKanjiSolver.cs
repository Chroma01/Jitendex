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

using System.Text.RegularExpressions;
using Jitendex.Furigana.Business;
using Jitendex.Furigana.Models;

namespace Jitendex.Furigana.Solvers;

public class NoConsecutiveKanjiSolver : FuriganaSolver
{
    private readonly FuriganaResourceSet _resourceSet;

    public NoConsecutiveKanjiSolver(FuriganaResourceSet resourceSet)
    {
        _resourceSet = resourceSet;
    }

    /// <summary>
    /// Attempts to solve furigana in cases where there are no consecutive kanji in the kanji string,
    /// using regular expressions.
    /// </summary>
    protected override IEnumerable<FuriganaSolution> DoSolve(VocabEntry v)
    {
        // We are using both a greedy expression and a lazy expression because we want to make sure
        // there is only one way to read them. If the result differs with a greedy or a lazy expression,
        // it means that we have no idea how to read the damn thing.
        string regGreedy = "^";
        string regLazy = "^";
        bool consecutiveMarker = false;
        var kanjiIndexes = new List<int>(4);
        for (int i = 0; i < v.KanjiFormText.Length; i++)
        {
            char c = v.KanjiFormText[i];
            var kanji = _resourceSet.GetKanji(c);
            if (kanji == null)
            {
                // Add the characters to the string. No capture group for kana.
                regGreedy += string.Format(c.ToString());
                regLazy += string.Format(c.ToString());
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
                regGreedy += "(.+)";
                regLazy += "(.+?)";
                consecutiveMarker = true;
                kanjiIndexes.Add(i);
            }
        }
        regGreedy += "$";
        regLazy += "$";

        // Example regex:
        // For 持ち運ぶ (もちはこぶ)
        // The regexes would be:
        // ^(.+)ち(.+)ぶ$
        // ^(.+?)ち(.+?)ぶ$

        var regexGreedy = new Regex(regGreedy);
        var regexLazy = new Regex(regLazy);
        var matchGreedy = regexGreedy.Match(v.ReadingText);
        var matchLazy = regexLazy.Match(v.ReadingText);

        if (matchGreedy.Success && matchLazy.Success)
        {
            // Obtain both solutions.
            var greedySolution = MakeSolutionFromMatch(v, matchGreedy, kanjiIndexes);
            var lazySolution = MakeSolutionFromMatch(v, matchLazy, kanjiIndexes);

            // Are both solutions non-null and equivalent?
            if (greedySolution != null && lazySolution != null && greedySolution.Equals(lazySolution))
            {
                // Yes they are! Return only one of them of course.
                // Greedy wins obviously.
                yield return greedySolution;
            }
        }
    }

    /// <summary>
    /// Creates a furigana solution from a regex match computed in the DoSolve method.
    /// </summary>
    private static FuriganaSolution? MakeSolutionFromMatch(VocabEntry v, Match match, List<int> kanjiIndexes)
    {
        if (match.Groups.Count != kanjiIndexes.Count + 1)
        {
            return null;
        }

        var parts = new List<FuriganaPart>(match.Groups.Count - 1);
        for (int i = 1; i < match.Groups.Count; i++)
        {
            var group = match.Groups[i];
            parts.Add(new FuriganaPart(group.Value, kanjiIndexes[i - 1]));
        }

        return new FuriganaSolution(v, parts);
    }
}
