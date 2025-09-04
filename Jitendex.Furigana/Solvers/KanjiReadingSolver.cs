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

public class KanjiReadingSolver : FuriganaSolver
{
    /// <summary>
    /// Defines the maximal number of kana that can be attributed to a single kanji (performance trick).
    /// </summary>
    private static readonly int MaxKanaPerKanji = 4;
    private readonly FuriganaResourceSet _resourceSet;
    private readonly bool _useNanori;

    public KanjiReadingSolver(FuriganaResourceSet resourceSet, bool useNanori)
    {
        _resourceSet = resourceSet;
        _useNanori = useNanori;
    }

    /// <summary>
    /// Attempts to solve furigana by reading the kanji reading string and finding matching kanji
    /// kanji readings.
    /// </summary>
    protected override IEnumerable<FuriganaSolution> DoSolve(VocabEntry v)
    {
        foreach (var solution in TryReading(v, 0, 0, []))
        {
            yield return solution;
        }
    }

    /// <summary>
    /// Recursive method that reads the kanji reading string and attempts to find all the ways the
    /// kana reading could be cut by matching it with the potential kanji readings.
    /// </summary>
    /// <param name="r">Resource set.</param>
    /// <param name="v">Vocab to solve.</param>
    /// <param name="currentIndexKanji">Current position in the kanji string. Used for recursion.</param>
    /// <param name="currentIndexKana">Current position in the kana string. Used for recursion.</param>
    /// <param name="furiganaParts">Current furigana parts. Used for recursion.</param>
    private IEnumerable<FuriganaSolution> TryReading
    (
        VocabEntry v,
        int currentIndexKanji,
        int currentIndexKana,
        List<FuriganaPart> furiganaParts
    )
    {
        if (currentIndexKanji == v.KanjiFormText.Length && currentIndexKana == v.ReadingText.Length)
        {
            // We successfuly read the word and stopped at the last character in both kanji and kana readings.
            // Our current cut is valid. Return it.
            yield return new FuriganaSolution(v, furiganaParts);
            yield break;
        }
        else if (currentIndexKanji >= v.KanjiFormText.Length || currentIndexKana >= v.ReadingText.Length)
        {
            // Broken case. Do not return anything.
            yield break;
        }

        // Search for special expressions.
        bool foundSpecialExpressions = false;
        foreach (var solution in FindSpecialExpressions(v, currentIndexKanji, currentIndexKana, furiganaParts))
        {
            foundSpecialExpressions = true;
            yield return solution;
        }

        if (foundSpecialExpressions)
        {
            yield break;
        }

        // General case. Get the current character and see if it is a kanji.
        char c = v.KanjiFormText[currentIndexKanji];

        if (c == '々' && currentIndexKanji > 0)
        {
            // Special case: handle the repeater kanji by using the previous character instead.
            c = v.KanjiFormText[currentIndexKanji - 1];
        }

        Kanji? kanji = _resourceSet.GetKanji(c);

        if (kanji is not null)
        {
            // Read as kanji subpart.
            foreach (var solution in ReadAsKanji(v, currentIndexKanji, currentIndexKana, furiganaParts, kanji))
            {
                yield return solution;
            }
        }
        else
        {
            // Read as kana subpart.
            foreach (var solution in ReadAsKana(v, currentIndexKanji, currentIndexKana, furiganaParts, c))
            {
                yield return solution;
            }
        }
    }

    /// <summary>
    /// Subpart of TryReading. Attempts to find a matching special expression.
    /// If found, iterates on TryReading.
    /// </summary>
    private IEnumerable<FuriganaSolution> FindSpecialExpressions
    (
        VocabEntry v,
        int currentIndexKanji,
        int currentIndexKana,
        List<FuriganaPart> furiganaParts
    )
    {
        string lookup = string.Empty;
        for (int i = v.KanjiFormText.Length - 1; i >= currentIndexKanji; i--)
        {
            lookup = v.KanjiFormText.Substring(currentIndexKanji, (i - currentIndexKanji) + 1);
            var expression = _resourceSet.GetExpression(lookup);
            if (expression is null) continue;

            var potentialSpecialReadings = ReadingExpander.GetPotentialSpecialReadings
            (
                expression: expression,
                isFirstChar: currentIndexKanji == 0,
                isLastChar: i == v.KanjiFormText.Length - 1
            );

            foreach (var expressionReading in potentialSpecialReadings)
            {
                if (v.ReadingText.Length < currentIndexKana + expressionReading.ReadingText.Length)
                    continue;

                if (v.ReadingText.Substring(currentIndexKana, expressionReading.ReadingText.Length) == expressionReading.ReadingText)
                {
                    // The reading matches. Iterate with this possibility.
                    var newFuriganaParts = furiganaParts.Clone();
                    newFuriganaParts.AddRange(
                        expressionReading.Solution.FuriganaParts
                        .Select(fp => new FuriganaPart(
                            value: fp.Value,
                            startIndex: fp.StartIndex + currentIndexKanji,
                            endIndex: fp.EndIndex + currentIndexKanji)));

                    var results = TryReading(v: v,
                        currentIndexKanji: i + 1,
                        currentIndexKana: currentIndexKana + expressionReading.ReadingText.Length,
                        furiganaParts: newFuriganaParts);

                    foreach (var result in results)
                    {
                        yield return result;
                    }
                }
            }

        }
    }

    /// <summary>
    /// Subpart of TryReading. Finds all matching kanji readings for the current situation,
    /// and iterates on TryReading when found.
    /// </summary>
    private IEnumerable<FuriganaSolution> ReadAsKanji
    (
        VocabEntry v,
        int currentIndexKanji,
        int currentIndexKana,
        List<FuriganaPart> furiganaParts,
        Kanji kanji
    )
    {
        // Our character is a kanji. Try to consume kana strings that match that kanji.
        int remainingKanjiLength = v.KanjiFormText.Length - currentIndexKanji - 1;
        var kanjiReadings = ReadingExpander.GetPotentialKanjiReadings(
            kanji: kanji,
            isFirstChar: currentIndexKanji == 0,
            isLastChar: currentIndexKanji == v.KanjiFormText.Length - 1,
            useNanori: _useNanori);

        // Iterate on the kana reading.
        for (int i = currentIndexKana; i < v.ReadingText.Length && i < currentIndexKana + MaxKanaPerKanji; i++)
        {
            int remainingKanaLength = v.ReadingText.Length - i - 1;
            if (remainingKanaLength < remainingKanjiLength)
            {
                // We consumed too many characters: not enough kana remaining for the number of kanji.
                // Stop here. There are no more solutions.
                yield break;
            }

            // Get the kana string between currentIndexKana and i.
            string testedString = v.ReadingText.Substring(currentIndexKana, (i - currentIndexKana) + 1);

            // Now try to match that string against one of the potential readings of our kanji.
            foreach (string reading in kanjiReadings)
            {
                if (reading != testedString) continue;

                // We have a match. Create our new cut and iterate with it.
                var newFuriganaParts = furiganaParts.Clone();
                newFuriganaParts.Add(new FuriganaPart(reading, currentIndexKanji));

                var solutions = TryReading(v: v,
                    currentIndexKanji: currentIndexKanji + 1,
                    currentIndexKana: i + 1,
                    furiganaParts: newFuriganaParts);

                foreach (var solution in solutions)
                {
                    yield return solution;
                }
            }

            // Continue to expand our testedString to try and follow other potential reading paths.
        }
    }

    /// <summary>
    /// Subpart of TryReading. Attempts to find a match between the current kanji reading character
    /// and the current kana reading character. If found, iterates on TryReading.
    /// </summary>
    private IEnumerable<FuriganaSolution> ReadAsKana
    (
        VocabEntry v,
        int currentIndexKanji,
        int currentIndexKana,
        List<FuriganaPart> furiganaParts,
        char c
    )
    {
        char kc = v.ReadingText[currentIndexKana];
        if (c == kc || c.ToString().KatakanaToHiragana() == kc.ToString().KatakanaToHiragana())
        {
            // This kanji form substring matches the kana form substring.
            // We can iterate with the same cut (no added furigana) because we are reading kana.
            foreach (var result in TryReading(v, currentIndexKanji + 1, currentIndexKana + 1, furiganaParts))
            {
                yield return result;
            }
        }
    }
}
