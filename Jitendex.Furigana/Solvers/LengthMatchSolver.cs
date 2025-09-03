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

using Jitendex.Furigana.Business;
using Jitendex.Furigana.Models;
using Jitendex.Furigana.Helpers;

namespace Jitendex.Furigana.Solvers;

public class LengthMatchSolver : FuriganaSolver
{
    public LengthMatchSolver()
    {
        // Priority down because it's not good with special expressions.
        Priority = -1;
    }

    /// <summary>
    /// Attempts to solve cases where the length of the kanji reading matches the length of the
    /// kana reading.
    /// </summary>
    protected override IEnumerable<FuriganaSolution> DoSolve(FuriganaResourceSet r, VocabEntry v)
    {
        if (v.KanjiFormText.Length != v.ReadingText.Length)
        {
            yield break;
        }

        var parts = new List<FuriganaPart>();
        for (int i = 0; i < v.KanjiFormText.Length; i++)
        {
            if (r.GetKanji(v.KanjiFormText[i]) != null)
            {
                parts.Add(new FuriganaPart(v.ReadingText[i].ToString(), i));
            }
            else if (!KanaHelper.IsAllKana(v.KanjiFormText[i].ToString()))
            {
                // Our character is not a kanji and apparently not a kana either.
                // Stop right there. It's probably a trap.
                yield break;
            }
            else if (!KanaHelper.AreEquivalent(v.KanjiFormText[i].ToString(), v.ReadingText[i].ToString()))
            {
                // We are reading kana characters that are not equivalent. Stop.
                yield break;
            }
        }

        if (parts.Count > 0)
        {
            yield return new FuriganaSolution(v, parts);
        }
    }
}
