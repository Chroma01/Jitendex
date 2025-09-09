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

internal class SingleKanjiSolver : FuriganaSolver
{
    public SingleKanjiSolver()
    {
        Priority = 1;  // Priority up because it's quick and guarantees the only correct solution when appliable.
    }

    public override IEnumerable<IndexedSolution> Solve(VocabEntry v)
    {
        if (!EligibleForThisSolution(v)) yield break;

        var kanjiFormRunes = v.KanjiFormRunes;
        int kanjiIndex = 0;
        string kanaReading = v.ReadingText;

        // See if there are only obvious characters around.
        // Browse the kanji reading and eat characters until we get to
        // the kanji character.
        for (int i = 0; i < kanjiFormRunes.Length; i++)
        {
            var rune = kanjiFormRunes[i];
            if (rune.IsKanji())
            {
                // We are on the kanji. Skip.
                kanjiIndex = i;
                break;
            }
            else if (kanaReading.First() == rune.Value)
            {
                // Remove the first character of the reading.
                kanaReading = kanaReading[1..];
            }
            else
            {
                // There is something wrong. Readings don't add up.
                // Can't solve.
                yield break;
            }
        }

        // Now browse in reverse and eat characters until we get back to
        // the kanji character.
        for (int i = kanjiFormRunes.Length - 1; i >= 0; i--)
        {
            var rune = kanjiFormRunes[i];

            if (rune.IsKanji())
            {
                // We are on the kanji. Skip.
                break;
            }
            else if (kanaReading.Last() == rune.Value)
            {
                // Eat the last character of the reading.
                kanaReading = kanaReading[..^1];
            }
            else
            {
                // There is something wrong. Readings don't add up.
                // Can't solve.
                yield break;
            }
        }

        // We are done. Our kanaReading contains only what's left when eating the kana
        // before and after the kanji. It's the reading of our kanji.
        yield return new IndexedSolution(v, new IndexedFurigana(kanaReading, kanjiIndex));
    }

    private static bool EligibleForThisSolution(VocabEntry v)
    {
        int kanjiCount = 0;
        foreach (var rune in v.KanjiFormRunes)
        {
            if (rune.IsKanji() && ++kanjiCount > 1)
                return false;
        }
        return true;
    }
}
