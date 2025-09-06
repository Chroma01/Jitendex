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

using System.Text;
using Jitendex.Furigana.Helpers;

namespace Jitendex.Furigana.Models;

/// <summary>
/// Represents an individual part of the reading of a word or expression.
/// </summary>
public record ReadingPart(string Text, string? Furigana);

/// <summary>
/// A vocab entry with a furigana reading string.
/// </summary>
public class FuriganaSolution
{
    public VocabEntry Vocab { get; set; }
    public List<FuriganaPart> FuriganaParts { get; set; }

    public FuriganaSolution(VocabEntry vocab, List<FuriganaPart> furiganaParts)
    {
        Vocab = vocab;
        FuriganaParts = furiganaParts;
    }

    public FuriganaSolution(VocabEntry vocab, params FuriganaPart[] furiganaParts) : this(vocab, furiganaParts.ToList()) { }

    #region Methods

    #region Static

    /// <summary>
    /// Checks if the solution is correctly solved for the given coupling of vocab and furigana.
    /// </summary>
    /// <param name="v">Vocab to check.</param>
    /// <param name="furiganaParts">Furigana to check.</param>
    /// <returns>True if the furigana covers all characters of the vocab reading without
    /// overlapping.</returns>
    public static bool Check(VocabEntry v, List<FuriganaPart> furiganaParts)
    {
        // There are three conditions to check:
        // 1. Furigana parts are not overlapping: for any given index in the kanji reading string,
        // there is between 0 and 1 matching furigana parts.
        // 2. All non-kana characters are covered by a furigana part.
        // 3. Reconstituting the kana reading from the kanji reading using the furigana parts when
        // available will give the kana reading of the vocab entry.

        // Keep in mind things like 真っ青 admit a correct "0-2:まっさお" solution. There can be
        // furigana parts covering kana.

        var runes = v.KanjiFormRunes();

        // Check condition 1.
        if (Enumerable.Range(0, runes.Count).Any(i => furiganaParts.Count(f => i >= f.StartIndex && i <= f.EndIndex) > 1))
        {
            // There are multiple furigana parts that are appliable for a given index.
            // This constitutes an overlap and results in the check being negative.
            // Condition 1 failed.
            return false;
        }

        // Now try to reconstitute the reading using the furigana parts.
        // This will allow us to test both 2 and 3.
        var reconstitutedReading = new StringBuilder();
        for (int i = 0; i < runes.Count; i++)
        {
            // Try to find a matching part.
            var matchingPart = furiganaParts.FirstOrDefault(f => i >= f.StartIndex && i <= f.EndIndex);
            if (matchingPart != null)
            {
                // We have a matching part. Add the furigana string to the reconstituted reading.
                reconstitutedReading.Append(matchingPart.Value);

                // Advance i to the end index and continue.
                i = matchingPart.EndIndex;
                continue;
            }

            // Characters that are not covered by a furigana part should be kana.
            var c = runes[i];
            if (c.IsKana())
            {
                // It is kana. Add the character to the reconstituted reading.
                char kana = (char)c.Value;
                reconstitutedReading.Append(kana);
            }
            else
            {
                // This is not kana and this is not covered by any furigana part.
                // The solution is not complete and is therefore not valid.
                // Condition 2 failed.
                return false;
            }
        }

        // Our reconstituted reading should be the same as the kana reading of the vocab.
        if (!KanaHelper.AreEquivalent(reconstitutedReading.ToString(), v.ReadingText))
        {
            // It is different. Something is not correct in the furigana reading values.
            // Condition 3 failed.
            return false;
        }

        // Nothing has failed. Everything is good.
        return true;
    }

    #endregion

    /// <summary>
    /// Gets the parts covering the given index.
    /// Remember that an invalid solution may have several parts for a given index.
    /// </summary>
    /// <param name="index">Target index.</param>
    /// <returns>All parts covering the given index.</returns>
    public List<FuriganaPart> GetPartsForIndex(int index)
    {
        return FuriganaParts
            .Where(f => f.StartIndex <= index && index <= f.EndIndex)
            .ToList();
    }

    /// <summary>
    /// Checks if the solution is correctly solved.
    /// </summary>
    /// <returns>True if the furigana covers all characters without overlapping.
    /// False otherwise.</returns>
    public bool Check()
    {
        return Check(Vocab, FuriganaParts);
    }

    /// <summary>
    /// Breaks down the solution to its individual reading parts.
    /// </summary>
    public IEnumerable<ReadingPart> BreakIntoParts()
    {
        var runes = Vocab.RawKanjiFormRunes();
        int? kanaStart = null;
        for (int i = 0; i < runes.Count; i++)
        {
            var matchingFurigana = FuriganaParts.FirstOrDefault(f => f.StartIndex == i);
            if (matchingFurigana is not null)
            {
                // We are on a furigana start index.
                // If there was any kana, output that part first
                if (kanaStart.HasValue)
                {
                    yield return new ReadingPart(
                        Text: string.Join("", runes.ToArray()[kanaStart.Value..i]),
                        Furigana: null);

                    kanaStart = null;
                }

                // Then output the furigana part
                yield return new ReadingPart(
                    Text: string.Join("", runes.GetRange(i, matchingFurigana.EndIndex - i + 1)),
                    Furigana: matchingFurigana.Value);

                i = matchingFurigana.EndIndex;
            }
            else
            {
                // We are not on a furigana-covered character, must be kana. Set kanaStart if not already set.
                kanaStart ??= i;
            }
        }

        // Output the final kana part if any
        if (kanaStart.HasValue)
        {
            yield return new ReadingPart(
                Text: string.Join("", runes.ToArray()[kanaStart.Value..]),
                Furigana: null);
        }
    }

    public override string ToString()
    {
        return string.Join
        (
            Separator.FileField,
            new string[]
            {
                Vocab.KanjiFormText,
                Vocab.ReadingText,
                string.Join
                (
                    Separator.MultiValue,
                    FuriganaParts.Select(x => x.ToString()).ToArray()
                )
            }
        );
    }

    public override bool Equals(object? obj)
    {
        if (obj is FuriganaSolution other)
        {
            // Compare both solutions.
            if (Vocab != other.Vocab || FuriganaParts.Count != other.FuriganaParts.Count)
            {
                // Not the same vocab or not the same count of furigana parts.
                return false;
            }

            // If there is at least one furigana part that has no equivalent in the other
            // furigana solution, then the readings differ.
            return FuriganaParts.All(f1 => other.FuriganaParts.Any(f2 => f1.Equals(f2)))
                && other.FuriganaParts.All(f2 => FuriganaParts.Any(f1 => f1.Equals(f2)));
        }
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    #endregion
}
