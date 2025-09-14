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

using System.Text;
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

    public override IEnumerable<IndexedSolution> Solve(Entry entry)
    {
        var prefix = Prefix(entry);
        if (prefix is null) yield break;

        var suffix = Suffix(entry, prefix);
        if (suffix is null) yield break;

        var furigana = Furigana(entry, prefix, suffix);
        if (furigana is null) yield break;

        var baseText = entry.KanjiFormRunes[prefix.Length].ToString();

        var solutionBuilder = new SolutionBuilder
        ([
            new Solution.Part(prefix, null),
            new Solution.Part(baseText, furigana),
            new Solution.Part(suffix, null),
        ]);

        var solution = solutionBuilder.ToIndexedSolution(entry);
        if (solution is not null)
        {
            yield return solution;
        }
    }

    private static string? Prefix(Entry entry)
    {
        var prefixBuilder = new StringBuilder();
        foreach (var rune in entry.KanjiFormRunes)
        {
            if (rune.IsKana())
            {
                prefixBuilder.Append(rune);
            }
            else
            {
                return prefixBuilder.ToString();
            }
        }
        // Either the runes are all kana or there were no runes.
        return null;
    }

    private static string? Suffix(Entry entry, string prefix)
    {
        var suffixBuilder = new StringBuilder();
        foreach (var rune in entry.KanjiFormRunes[(prefix.Length + 1)..])
        {
            if (rune.IsKana())
            {
                suffixBuilder.Append(rune);
            }
            else
            {
                // Kanji form contains more than one kanji.
                return null;
            }
        }
        return suffixBuilder.ToString();
    }

    private static string? Furigana(Entry entry, string prefix, string suffix)
    {
        var normalizedPrefix = prefix.KatakanaToHiragana();
        if (!entry.NormalizedReadingText.StartsWith(normalizedPrefix))
        {
            return null;
        }

        var normalizedSuffix = suffix.KatakanaToHiragana();
        if (!entry.NormalizedReadingText.EndsWith(normalizedSuffix))
        {
            return null;
        }

        int i = prefix.Length;
        int j = entry.ReadingText.Length - suffix.Length;

        if (i == j)
        {
            return null;
        }

        return entry.ReadingText[i..j];
    }
}
