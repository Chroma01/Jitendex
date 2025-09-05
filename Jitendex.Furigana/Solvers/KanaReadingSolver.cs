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

using System.Collections.Frozen;
using Jitendex.Furigana.Business;
using Jitendex.Furigana.Models;
using Jitendex.Furigana.Helpers;

namespace Jitendex.Furigana.Solvers;

public class KanaReadingSolver : FuriganaSolver
{
    private static readonly FrozenSet<char> ImpossibleCutStart = ['っ', 'ょ', 'ゃ', 'ゅ', 'ん'];
    private readonly FuriganaResourceSet _resourceSet;

    public KanaReadingSolver(FuriganaResourceSet resourceSet)
    {
        _resourceSet = resourceSet;
    }

    /// <summary>
    /// Attempts to solve furigana by reading the kana string and attributing kanji a reading based
    /// not on the readings of the kanji, but on the kana characters that come up.
    /// </summary>
    protected override IEnumerable<FuriganaSolution> DoSolve(VocabEntry v)
    {
        // Basically, we are reading the kanji reading character by character, eating the kana from
        // the kana reading and associating each kanji the piece of kana that comes next.
        // The thing is, we are taking advantage that kanji readings cannot start with certain
        // kana (ん and the small characters).
        // If we just stumbled upon a kanji and the next characters of the kana string are of these
        // impossible start kana, we can automatically associate them with the kanji.
        // Now this will work only for a number of vocab, but it does significantly improve the results.
        // It is especially good for 2-characters compounds that use unusual readings.

        /// Example: 阿呆陀羅 (あほんだら)
        /// Read the あ for 阿;
        /// Read the ほ for 呆;
        /// Read the ん: it's an impossible start character, so it goes with 呆 as well;
        /// Read the だ for 陀;
        /// Read the ら for 羅.

        string kana = v.ReadingText;
        var furiganaParts = new List<FuriganaPart>();

        for (int i = 0; i < v.KanjiFormText.Length; i++)
        {
            if (kana.Length == 0)
            {
                // We still have characters to browse in our kanji reading, but
                // there are no more kana to consume. Cannot solve.
                yield break;
            }

            bool foundExpression = false;

            // Check for special expressions
            for (int j = v.KanjiFormText.Length - 1; j >= i; j--)
            {
                string lookup = v.KanjiFormText.Substring(i, j - i + 1);
                var expression = _resourceSet.GetExpression(lookup);

                if (expression is null) continue;

                // We found an expression.
                var expressionReadings = SpecialReadingExpander.GetPotentialSpecialReadings(
                    expression: expression,
                    isFirstChar: i == 0,
                    isLastChar: j == v.KanjiFormText.Length - 1);

                foreach (var expressionReading in expressionReadings)
                {
                    if (!kana.StartsWith(expressionReading.ReadingText))
                        continue;

                    // The reading matches. Eat the kana chain.
                    furiganaParts.AddRange(expressionReading.Solution.FuriganaParts
                        .Select(fp => new FuriganaPart(fp.Value, fp.StartIndex + i, fp.EndIndex + i)));

                    kana = kana[expressionReading.ReadingText.Length..];
                    i = j;
                    foundExpression = true;
                    break;
                }

                if (foundExpression)
                {
                    break;
                }
            }
            // End check for special expression

            if (foundExpression)
            {
                continue;
            }

            // Normal process: eat the first character of our kana string.
            string eaten = kana.First().ToString();
            kana = kana[1..];

            char c = v.KanjiFormText[i];
            Kanji? k = _resourceSet.GetKanji(c);

            if (k is not null)
            {
                // On a kanji case, also eat consecutive "impossible start characters"
                while (kana.Length > 0 && ImpossibleCutStart.Contains(kana.First()))
                {
                    eaten += kana.First();
                    kana = kana[1..];
                }
                furiganaParts.Add(new FuriganaPart(eaten, i));
            }
            else if (!c.IsKana())
            {
                // The character is neither a kanji or a kana.
                // Cannot solve.
                yield break;
            }
            else if (eaten != c.ToString())
            {
                // The character browsed is a kana but is not the
                // character that we just ate. We made a mistake
                // in one of the kanji readings, meaning that we...
                // Cannot solve.
                yield break;
            }
        }

        if (kana.Length == 0)
        {
            // We consumed the whole kana string.
            // The case is solved.
            yield return new FuriganaSolution(v, furiganaParts);
        }
    }
}
