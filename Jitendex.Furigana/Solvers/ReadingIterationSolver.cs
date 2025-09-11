/*
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

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Jitendex.Furigana.Helpers;
using Jitendex.Furigana.InputModels;
using Jitendex.Furigana.OutputModels;

namespace Jitendex.Furigana.Solvers;

internal class ReadingIterationSolver : FuriganaSolver
{
    private readonly ResourceSet _resourceSet;

    public ReadingIterationSolver(ResourceSet resourceSet)
    {
        _resourceSet = resourceSet;
    }

    public override IEnumerable<IndexedSolution> Solve(Entry entry)
    {
        var builders = new List<SolutionBuilder>() { new() };

        for (int sliceStart = 0; sliceStart < entry.KanjiFormRunes.Length; sliceStart++)
        {
            var newBuilders = new List<SolutionBuilder>();
            for (int sliceEnd = entry.KanjiFormRunes.Length; sliceStart < sliceEnd; sliceEnd--)
            {
                newBuilders = IterateBuilders
                (
                    entry: entry,
                    oldBuilders: builders,
                    potentialReadings: GetPotentialReadings(entry, sliceStart, sliceEnd),
                    baseText: string.Join(string.Empty, entry.RawKanjiFormRunes[sliceStart..sliceEnd])
                );
                if (newBuilders.Count > 0)
                {
                    sliceStart += sliceEnd - sliceStart - 1;
                    builders = newBuilders;
                    break;
                }
            }
            if (newBuilders.Count == 0)
                yield break;
        }

        foreach (var builder in builders)
        {
            var solution = builder.ToIndexedSolution(entry);
            if (solution is not null)
                yield return solution;
        }
    }

    private ImmutableArray<string> GetPotentialReadings(Entry entry, int sliceStart, int sliceEnd)
    {
        var runesSlice = entry.KanjiFormRunes[sliceStart..sliceEnd];
        if (runesSlice.Length == 1)
        {
            bool isFirstRune = sliceStart == 0;
            bool isLastRune = sliceStart == entry.KanjiFormRunes.Length - 1;
            return _resourceSet.GetPotentialReadings(runesSlice[0], entry, isFirstRune, isLastRune);
        }
        else
        {
            var textSlice = string.Join(string.Empty, runesSlice);
            return _resourceSet.GetPotentialReadings(textSlice);
        }
    }

    private static List<SolutionBuilder> IterateBuilders(Entry entry, List<SolutionBuilder> oldBuilders, ImmutableArray<string> potentialReadings, string baseText)
    {
        var newBuilders = new List<SolutionBuilder>();
        foreach (var oldBuilder in oldBuilders)
        {
            var oldParts = oldBuilder.ToParts();
            var oldReadings = oldBuilder.NormalizedReadingText();
            foreach (var baseReading in potentialReadings)
            {
                if (TryGetNewPart(entry, baseText, oldReadings, baseReading, out var newPart))
                {
                    newBuilders.Add(new(oldParts.Append(newPart)));
                }
            }
        }
        return newBuilders;
    }

    private static bool TryGetNewPart(Entry entry, string baseText, string oldReadings, string? baseReading, [NotNullWhen(returnValue: true)] out Solution.Part? part)
    {
        if (baseReading is null)
        {
            part = new(baseText, null);
            return true;
        }
        else if (!entry.NormalizedReadingText.StartsWith(oldReadings + baseReading))
        {
            part = null;
            return false;
        }
        else if (baseText.IsKanaEquivalent(baseReading))
        {
            part = new(baseText, null);
            return true;
        }
        else
        {
            // Use the raw, non-normalized reading for the furigana text.
            int i = oldReadings.Length;
            int j = i + baseReading.Length;
            var furigana = entry.ReadingText[i..j];
            part = new(baseText, furigana);
            return true;
        }
    }
}
