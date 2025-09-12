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

using System.Collections.Frozen;
using System.Text;
using Jitendex.Furigana.InputModels;
using Jitendex.Furigana.OutputModels;
using Jitendex.Furigana.Helpers;

namespace Jitendex.Furigana.Solvers;

internal class KanaReadingSolver : FuriganaSolver
{
    private static readonly FrozenSet<char> _impossibleCutStart = ['っ', 'ょ', 'ゃ', 'ゅ', 'ん'];
    private readonly ResourceSet _resourceSet;

    public KanaReadingSolver(ResourceSet resourceSet)
    {
        _resourceSet = resourceSet;
    }

    /// <summary>
    /// Attempts to solve furigana by reading the kana string and attributing kanji a reading based
    /// not on the readings of the kanji, but on the kana characters that come up.
    /// </summary>
    /// <remarks>
    /// Basically, we are reading the kanji reading character by character, eating the kana from
    /// the kana reading and associating each kanji the piece of kana that comes next.
    /// The thing is, we are taking advantage that kanji readings cannot start with certain
    /// kana (ん and the small characters).
    /// If we just stumbled upon a kanji and the next characters of the kana string are of these
    /// impossible start kana, we can automatically associate them with the kanji.
    /// Now this will work only for a number of vocab, but it does significantly improve the results.
    /// It is especially good for 2-characters compounds that use unusual readings.
    /// <para>Example: 阿呆陀羅 (あほんだら)</para>
    /// <para>Read the あ for 阿;</para>
    /// <para>Read the ほ for 呆;</para>
    /// <para>Read the ん: it's an impossible start character, so it goes with 呆 as well;</para>
    /// <para>Read the だ for 陀;</para>
    /// <para>Read the ら for 羅.</para>
    /// </remarks>
    public override IEnumerable<IndexedSolution> Solve(Entry entry)
    {
        var builder = new SolutionBuilder();

        for (int sliceStart = 0; sliceStart < entry.KanjiFormRunes.Length; sliceStart++)
        {
            var specialPart = SpecialExpressionPart(entry, builder, ref sliceStart);
            if (specialPart is not null)
            {
                builder.Add(specialPart);
                continue;
            }

            var normalPart = NormalPart(entry, builder, sliceStart);
            if (normalPart is not null)
            {
                builder.Add(normalPart);
            }
            else
            {
                yield break;
            }
        }

        var solution = builder.ToIndexedSolution(entry);
        if (solution is not null)
        {
            yield return solution;
        }
    }

    private Solution.Part? SpecialExpressionPart(Entry entry, SolutionBuilder builder, ref int sliceStart)
    {
        var runes = entry.KanjiFormRunes;
        var oldReadings = builder.NormalizedReadingText();

        for (int sliceEnd = runes.Length; sliceStart < sliceEnd; sliceEnd--)
        {
            var runesSlice = runes[sliceStart..sliceEnd];
            string baseText = string.Join(string.Empty, runesSlice);
            var potentialReadings = _resourceSet.GetPotentialReadings(baseText);

            foreach (var baseReading in potentialReadings)
            {
                if (entry.NormalizedReadingText.StartsWith(oldReadings + baseReading))
                {
                    int i = oldReadings.Length;
                    int j = i + baseReading.Length;
                    var furigana = entry.ReadingText[i..j];
                    var part = new Solution.Part(baseText, furigana);

                    sliceStart += sliceEnd - sliceStart - 1;
                    return part;
                }
            }
        }
        return null;
    }

    private Solution.Part? NormalPart(Entry entry, SolutionBuilder builder, int sliceStart)
    {
        var oldReadings = builder.NormalizedReadingText();
        var remainingReadings = entry.ReadingText[oldReadings.Length..];

        if (remainingReadings.Length == 0)
        {
            return null;
        }

        var readingChar = remainingReadings[0].ToString();
        var remainingRunes = entry.KanjiFormRunes[sliceStart..];
        var rune = remainingRunes[0];
        var runeString = rune.ToString();

        if (rune.IsKana())
        {
            if (readingChar.IsKanaEquivalent(runeString))
            {
                return new Solution.Part(runeString, null);
            }
            else
            {
                return null;
            }
        }

        var newReadingBuilder = new StringBuilder(readingChar);

        char? nextRune = remainingRunes.Count() > 1 && remainingRunes[1].IsKana() ? (char)remainingRunes[1].Value : null;
        foreach (var nextReadingChar in remainingReadings[1..])
        {
            if (nextRune.HasValue && ((char)nextRune).IsKanaEquivalent(nextReadingChar))
            {
                break;
            }
            if (_impossibleCutStart.Contains(nextReadingChar.KatakanaToHiragana()))
            {
                newReadingBuilder.Append(nextReadingChar);
            }
            else
            {
                break;
            }
        }

        return new Solution.Part(runeString, newReadingBuilder.ToString());
    }
}
