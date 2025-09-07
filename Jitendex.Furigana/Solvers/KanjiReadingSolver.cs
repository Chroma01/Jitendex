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

internal class KanjiReadingSolver : FuriganaSolver
{
    /// <summary>
    /// Defines the maximal number of kana that can be attributed to a single kanji (performance trick).
    /// </summary>
    private static readonly int MaxKanaPerKanji = 4;
    private readonly ResourceSet _resourceSet;

    public KanjiReadingSolver(ResourceSet resourceSet)
    {
        _resourceSet = resourceSet;
    }

    /// <summary>
    /// Attempts to solve furigana by reading the kanji reading string and finding matching kanji
    /// kanji readings.
    /// </summary>
    protected override IEnumerable<IndexedSolution> DoSolve(VocabEntry v)
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
    /// <param name="v">Vocab to solve.</param>
    /// <param name="currentIndexKanji">Current position in the kanji string. Used for recursion.</param>
    /// <param name="currentIndexKana">Current position in the kana string. Used for recursion.</param>
    /// <param name="furiganaParts">Current furigana parts. Used for recursion.</param>
    private IEnumerable<IndexedSolution> TryReading
    (
        VocabEntry v,
        int currentIndexKanji,
        int currentIndexKana,
        List<IndexedFurigana> furiganaParts
    )
    {
        var runes = v.KanjiFormRunes();

        if (currentIndexKanji == runes.Count && currentIndexKana == v.ReadingText.Length)
        {
            // We successfuly read the word and stopped at the last character in both kanji and kana readings.
            // Our current cut is valid. Return it.
            yield return new IndexedSolution(v, furiganaParts);
            yield break;
        }
        else if (currentIndexKanji >= runes.Count || currentIndexKana >= v.ReadingText.Length)
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
        var rune = runes[currentIndexKanji];
        Kanji? kanji = _resourceSet.GetKanji(rune);

        if (kanji is not null)
        {
            // Read as kanji subpart.
            foreach (var solution in ReadAsKanji(v, currentIndexKanji, currentIndexKana, furiganaParts, kanji))
            {
                yield return solution;
            }
        }
        else if (rune.IsKana())
        {
            // Read as kana subpart.
            char kana = (char)rune.Value;
            foreach (var solution in ReadAsKana(v, currentIndexKanji, currentIndexKana, furiganaParts, kana))
            {
                yield return solution;
            }
        }
        else
        {
            // TODO
        }
    }

    /// <summary>
    /// Subpart of TryReading. Attempts to find a matching special expression.
    /// If found, iterates on TryReading.
    /// </summary>
    private IEnumerable<IndexedSolution> FindSpecialExpressions
    (
        VocabEntry v,
        int currentIndexKanji,
        int currentIndexKana,
        List<IndexedFurigana> furiganaParts
    )
    {
        var runes = v.KanjiFormRunes();
        for (int i = runes.Count - 1; i >= currentIndexKanji; i--)
        {
            var subRunes = runes.GetRange(currentIndexKanji, i - currentIndexKanji + 1);
            string lookup = string.Join("", subRunes);
            var expression = _resourceSet.GetExpression(lookup);

            if (expression is null) continue;

            foreach (var reading in expression.Readings)
            {
                if (v.ReadingText.Length < currentIndexKana + reading.Length)
                    continue;
                if (v.ReadingText.Substring(currentIndexKana, reading.Length) != reading)
                    continue;

                // The reading matches. Iterate with this possibility.
                var newFuriganaParts = furiganaParts.Clone();

                newFuriganaParts.Add(new IndexedFurigana(
                    value: reading,
                    startIndex: currentIndexKanji,
                    endIndex: currentIndexKanji + subRunes.Count - 1));

                var results = TryReading(v: v,
                    currentIndexKanji: i + 1,
                    currentIndexKana: currentIndexKana + reading.Length,
                    furiganaParts: newFuriganaParts);

                foreach (var result in results)
                {
                    yield return result;
                }
            }
        }
    }

    /// <summary>
    /// Subpart of TryReading. Finds all matching kanji readings for the current situation,
    /// and iterates on TryReading when found.
    /// </summary>
    private IEnumerable<IndexedSolution> ReadAsKanji
    (
        VocabEntry v,
        int currentIndexKanji,
        int currentIndexKana,
        List<IndexedFurigana> furiganaParts,
        Kanji kanji
    )
    {
        var runes = v.KanjiFormRunes();

        // Our character is a kanji. Try to consume kana strings that match that kanji.
        int remainingKanjiLength = runes.Count - currentIndexKanji - 1;
        var kanjiReadings = kanji.GetPotentialReadings(
            isFirstChar: currentIndexKanji == 0,
            isLastChar: currentIndexKanji == runes.Count - 1,
            isUsedInName: v.IsName);

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
            string testedString = v.ReadingText.Substring(currentIndexKana, i - currentIndexKana + 1);

            // Now try to match that string against one of the potential readings of our kanji.
            foreach (string reading in kanjiReadings)
            {
                if (reading != testedString)
                    continue;

                // We have a match. Create our new cut and iterate with it.
                var newFuriganaParts = furiganaParts.Clone();
                newFuriganaParts.Add(new IndexedFurigana(reading, currentIndexKanji));

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
    private IEnumerable<IndexedSolution> ReadAsKana
    (
        VocabEntry v,
        int currentIndexKanji,
        int currentIndexKana,
        List<IndexedFurigana> furiganaParts,
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
