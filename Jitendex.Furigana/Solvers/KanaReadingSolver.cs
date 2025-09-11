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
        var runes = entry.KanjiFormRunes;
        string kana = entry.ReadingText;
        var furiganaParts = new List<IndexedFurigana>();

        for (int sliceStart = 0; sliceStart < runes.Length; sliceStart++)
        {
            if (kana.Length == 0)
            {
                // We still have characters to browse in our kanji reading, but
                // there are no more kana to consume. Cannot solve.
                yield break;
            }

            var specialPart = SpecialExpressionPart(entry, ref sliceStart, ref kana);
            if (specialPart is not null)
            {
                furiganaParts.Add(specialPart);
                continue;
            }

            var normalPart = NormalPart(entry, ref sliceStart, ref kana);
            if (normalPart is not null)
            {
                furiganaParts.Add(normalPart);
            }
        }

        if (kana.Length == 0)
        {
            // We consumed the whole kana string.
            // The case is solved.
            yield return new IndexedSolution(entry, furiganaParts);
        }
    }

    private IndexedFurigana? SpecialExpressionPart(Entry entry, ref int sliceStart, ref string kana)
    {
        var runes = entry.KanjiFormRunes;

        for (int sliceEnd = runes.Length; sliceStart < sliceEnd; sliceEnd--)
        {
            var runesSlice = runes[sliceStart..sliceEnd];
            string lookup = string.Join(string.Empty, runesSlice);
            var readings = _resourceSet.GetPotentialReadings(lookup);

            foreach (var reading in readings)
            {
                if (!kana.StartsWith(reading))
                    continue;

                // The reading matches. Eat the kana chain.
                var newPart = new IndexedFurigana(reading, sliceStart, sliceStart + runesSlice.Length - 1);
                kana = kana[reading.Length..];
                sliceStart = sliceEnd - 1;
                return newPart;
            }
        }
        return null;
    }

    private IndexedFurigana? NormalPart(Entry entry, ref int sliceStart, ref string kana)
    {
        // Normal process: eat the first character of our kana string.
        string eaten = kana.First().ToString();
        kana = kana[1..];

        var runes = entry.KanjiFormRunes;
        var rune = runes[sliceStart];

        if (rune.IsKanji())
        {
            // On a kanji case, also eat consecutive "impossible start characters"
            while (kana.Length > 0 && _impossibleCutStart.Contains(kana.First()))
            {
                eaten += kana.First();
                kana = kana[1..];
            }
            return new IndexedFurigana(eaten, sliceStart);
        }
        else if (!rune.IsKana())
        {
            // The character is neither a kanji or a kana.
            // Cannot solve.
            sliceStart = runes.Length;
        }
        else if (eaten != rune.ToString())
        {
            // The character browsed is a kana but is not the
            // character that we just ate. We made a mistake
            // in one of the kanji readings, meaning that we...
            // Cannot solve.
            sliceStart = runes.Length;
        }
        return null;
    }
}
