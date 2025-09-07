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
using Jitendex.Furigana.InputModels;
using Jitendex.Furigana.OutputModels;

namespace Jitendex.Furigana.Solvers;

internal class SingleCharacterSolver : FuriganaSolver
{
    /// <summary>
    /// Attempts to solve furigana when the kanji form only has one character.
    /// </summary>
    protected override IEnumerable<IndexedSolution> DoSolve(VocabEntry v)
    {
        var runes = v.KanjiFormRunes;
        if (runes.Count != 1)
        {
            yield break;
        }
        if (runes[0].IsKana())
        {
            yield break;
        }

        yield return new IndexedSolution(v, new IndexedFurigana(v.ReadingText, 0));
    }
}
