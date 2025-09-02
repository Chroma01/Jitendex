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

using Jitendex.Furigana.Models;

namespace Jitendex.Furigana.Business.Solvers;

public class SingleKanjiSolver : FuriganaSolver
{
    public SingleKanjiSolver()
    {
        // Priority up because it's quick and guarantees the only correct solution when appliable.
        Priority = 1;
    }

    protected override IEnumerable<FuriganaSolution> DoSolve(FuriganaResourceSet r, VocabEntry v)
    {
        int kanjiCount = v.KanjiReading.Count(c => r.GetKanji(c) != null);
        if (kanjiCount == 1)
        {
            int kanjiIndex = 0;

            string kanaReading = v.KanaReading;
            // See if there are only obvious characters around.

            // Browse the kanji reading and eat characters until we get to
            // the kanji character.
            for (int i = 0; i < v.KanjiReading.Length; i++)
            {
                char c = v.KanjiReading[i];
                Kanji? k = r.GetKanji(c);
                if (k == null)
                {
                    if (kanaReading.First() == c)
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
                else
                {
                    // We are on the kanji. Skip.
                    kanjiIndex = i;
                    break;
                }
            }

            // Now browse in reverse and eat characters until we get back to
            // the kanji character.
            for (int i = v.KanjiReading.Length - 1; i >= 0; i--)
            {
                char c = v.KanjiReading[i];
                Kanji? k = r.GetKanji(c);
                if (k == null)
                {
                    if (kanaReading.Last() == c)
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
                else
                {
                    // We are on the kanji. Skip.
                    break;
                }
            }

            // We are done. Our kanaReading contains only what's left when eating the kana
            // before and after the kanji. It's the reading of our kanji.
            yield return new FuriganaSolution(v, new FuriganaPart(kanaReading, kanjiIndex));
        }
    }
}
