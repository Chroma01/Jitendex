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

namespace Jitendex.Furigana.Solvers;

public class RepeatedKanjiSolver : FuriganaSolver
{
    /// <summary>
    /// Solves cases where the kanji reading consists in a repeated kanji.
    /// </summary>
    protected override IEnumerable<FuriganaSolution> DoSolve(FuriganaResourceSet r, VocabEntry v)
    {
        if (v.KanjiFormText.Length == 2 && v.ReadingText.Length % 2 == 0
            && (v.KanjiFormText[1] == '々' || v.KanjiFormText[1] == v.KanjiFormText[0]))
        {
            // We have a case where the kanji string is composed of kanji repeated (e.g. 中々),
            // and our kana string can be cut in two. Just do that.

            yield return new FuriganaSolution(v,
                new FuriganaPart(v.ReadingText[..(v.ReadingText.Length / 2)], 0),
                new FuriganaPart(v.ReadingText[(v.ReadingText.Length / 2)..], 1));
        }
    }
}
