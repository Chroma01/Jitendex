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
using Jitendex.Furigana.Helpers;
using Jitendex.Furigana.Models;

namespace Jitendex.Furigana.Business;

/// <summary>
/// Provides a process that expands a given list of readings by adding rendaku versions and stuff like this.
/// </summary>
public static class ReadingExpander
{
    private static readonly FrozenDictionary<char, char[]> HiraToRendakus = new Dictionary<char, char[]>
    {
        ['か'] = ['が'],
        ['き'] = ['ぎ'],
        ['く'] = ['ぐ'],
        ['け'] = ['げ'],
        ['こ'] = ['ご'],
        ['さ'] = ['ざ'],
        ['し'] = ['じ'],
        ['す'] = ['ず'],
        ['せ'] = ['ぜ'],
        ['そ'] = ['ぞ'],
        ['た'] = ['だ'],
        ['ち'] = ['ぢ','じ'],
        ['つ'] = ['づ','ず'],
        ['て'] = ['で'],
        ['と'] = ['ど'],
        ['は'] = ['ば','ぱ'],
        ['ひ'] = ['び','ぴ'],
        ['ふ'] = ['ぶ','ぷ'],
        ['へ'] = ['べ','ぺ'],
        ['ほ'] = ['ぼ','ぽ'],
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, string> GodanVerbEndingToMasuInflection = new Dictionary<string, string>
    {
        ["く"] = "き",
        ["ぐ"] = "ぎ",
        ["す"] = "し",
        ["ず"] = "じ",
        ["む"] = "み",
        ["る"] = "り",
        ["ぶ"] = "び",
        ["う"] = "い",
    }.ToFrozenDictionary();

    private static readonly FrozenSet<char> SmallTsuRendakuList = ['つ', 'く', 'き', 'ち'];

    /// <summary>
    /// Given a kanji, finds and returns all potential readings that it could take in a string.
    /// </summary>
    /// <param name="kanji">Kanji to evaluate.</param>
    /// <param name="isFirstChar">Set to true if this kanji is the first character of the string
    /// that the kanji is found in.</param>
    /// <param name="isLastChar">Set to true if this kanji is the last character of the string
    /// that the kanji is found in.</param>
    /// <param name="useNanori">Set to true to use nanori readings as well.</param>
    /// <returns>A list containing all potential readings that the kanji could take.</returns>
    public static List<string> GetPotentialKanjiReadings(Kanji kanji, bool isFirstChar, bool isLastChar, bool useNanori)
    {
        var output = new List<string>();
        foreach (string reading in useNanori ? kanji.ReadingsWithNanori : kanji.Readings)
        {
            string r = reading.Replace("-", string.Empty);
            if (!KanaHelper.IsAllKatakana(r))
            {
                r = r.Replace("ー", string.Empty);
            }

            string[] dotSplit = r.Split('.');
            if (dotSplit.Length == 1)
            {
                output.Add(r);
            }
            else if (dotSplit.Length == 2)
            {
                output.Add(dotSplit[0]);
                output.Add(r.Replace(".", string.Empty));

                if (GodanVerbEndingToMasuInflection.TryGetValue(dotSplit[1], out string? newTerm))
                {
                    string newReading = r.Replace(".", string.Empty);
                    newReading = newReading[..^dotSplit[1].Length];
                    newReading += newTerm;
                    output.Add(newReading);
                }

                if (dotSplit[1].Length >= 2 && dotSplit[1][1] == 'る')
                {
                    // Add variant without the ending る.
                    string newReading = r.Replace(".", string.Empty);
                    newReading = newReading[..^1];
                    output.Add(newReading);
                }
            }
            else
            {
                throw new Exception(string.Format("Weird reading: {0} for kanji {1}.", reading, kanji.Character));
            }
        }

        // Add final small tsu rendaku
        if (!isLastChar)
        {
            output.AddRange(GetSmallTsuRendaku(output));
        }

        // Rendaku
        if (!isFirstChar)
        {
            output.AddRange(GetAllRendaku(output));
        }

        return output.Distinct().ToList();
    }

    /// <summary>
    /// Given a special reading expression, returns all potential kana readings the expression could use.
    /// </summary>
    /// <param name="expression">Target special reading expression.</param>
    /// <param name="isFirstChar">Set to true if the first character of the expression is the first
    /// character of the string that the expression is found in.</param>
    /// <param name="isLastChar">Set to true if the last character of the expression is the last
    /// character of the string that the expression is found in.</param>
    /// <returns>A list containing all potential readings the expression could assume.</returns>
    public static List<SpecialReading> GetPotentialSpecialReadings(SpecialExpression expression, bool isFirstChar, bool isLastChar)
    {
        var output = new List<SpecialReading>(expression.Readings);

        // Add final small tsu rendaku
        if (!isLastChar)
        {
            var add = new List<SpecialReading>();
            foreach (var r in output)
            {
                if (SmallTsuRendakuList.Contains(r.ReadingText.Last()))
                {
                    string newKanaReading = r.ReadingText[..^1] + "っ";
                    var newSolution = new FuriganaSolution(r.Solution.Vocab, r.Solution.FuriganaParts.Clone());
                    var newReading = new SpecialReading(newKanaReading, newSolution);

                    var affectedParts = newReading.Solution.GetPartsForIndex(newReading.Solution.Vocab.KanjiFormText.Length - 1);
                    foreach (var part in affectedParts)
                    {
                        part.Value = part.Value.Remove(part.Value.Length - 1) + "っ";
                    }
                    add.Add(newReading);
                }
            }
            output.AddRange(add);
        }

        // Rendaku
        if (!isFirstChar)
        {
            var add = new List<SpecialReading>();
            foreach (var r in output)
            {
                if (HiraToRendakus.TryGetValue(r.ReadingText.First(), out char[]? rendakuChars))
                {
                    foreach (var renChar in rendakuChars)
                    {
                        var newKanaReading = renChar + r.ReadingText[1..];
                        var newSolution = new FuriganaSolution(r.Solution.Vocab, r.Solution.FuriganaParts.Clone());
                        var newReading = new SpecialReading(newKanaReading, newSolution);

                        var affectedParts = newReading.Solution.GetPartsForIndex(0);
                        foreach (var part in affectedParts)
                        {
                            part.Value = renChar + part.Value[1..];
                        }
                        add.Add(newReading);
                    }
                }
            }
            output.AddRange(add);
        }
        return output.Distinct().ToList();
    }

    private static List<string> GetSmallTsuRendaku(List<string> readings)
    {
        var addedOutput = new List<string>();
        foreach (var reading in readings)
        {
            if (SmallTsuRendakuList.Contains(reading.Last()))
            {
                addedOutput.Add(reading[..^1] + "っ");
            }
        }
        return addedOutput;
    }

    private static List<string> GetAllRendaku(List<string> readings)
    {
        var rendakuOutput = new List<string>();
        foreach (var reading in readings)
        {
            if (HiraToRendakus.TryGetValue(reading.First(), out char[]? rendakuChars))
            {
                foreach (var renChar in rendakuChars)
                {
                    rendakuOutput.Add(renChar + reading[1..]);
                }
            }
        }
        return rendakuOutput;
    }
}
