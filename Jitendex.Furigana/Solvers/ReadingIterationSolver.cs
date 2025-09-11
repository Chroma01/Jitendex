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
        foreach(var solution in IterateSolutions(entry))
        {
            if (solution is not null)
                yield return solution;
        }
    }

    private IEnumerable<IndexedSolution?> IterateSolutions(Entry entry)
    {
        var builders = new List<SolutionBuilder>() { new() };

        for (int sliceStart = 0; sliceStart < entry.KanjiFormRunes.Length; sliceStart++)
        {
            var newBuilders = new List<SolutionBuilder>();
            for (int sliceEnd = entry.KanjiFormRunes.Length; sliceStart < sliceEnd; sliceEnd--)
            {
                var potentialReadings = GetPotentialReadings(entry, sliceStart, sliceEnd);
                var rawTextSlice = string.Join(string.Empty, entry.RawKanjiFormRunes[sliceStart..sliceEnd]);
                newBuilders = SolutionsForSlice(entry, builders, potentialReadings, rawTextSlice);
                if (newBuilders.Count > 0)
                {
                    sliceStart += rawTextSlice.Length - 1;
                    builders = newBuilders;
                    break;
                }
            }
            if (newBuilders.Count == 0)
            {
                yield break;
            }
        }
        foreach (var builder in builders)
        {
            yield return builder.ToIndexedSolution(entry);
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

    private static List<SolutionBuilder> SolutionsForSlice(Entry entry, List<SolutionBuilder> builders, ImmutableArray<string> potentialReadings, string textSlice)
    {
        var newBuilders = new List<SolutionBuilder>();
        foreach (var builder in builders)
        {
            var previousPartsReadingNormalized = builder.NormalizedReadingText();
            foreach (var potentialReading in potentialReadings)
            {
                var newPart = NewPart(entry, textSlice, previousPartsReadingNormalized, potentialReading);
                if (newPart is null)
                    continue;

                var newBuilder = new SolutionBuilder(builder.Parts);
                newBuilder.Add(newPart);
                newBuilders.Add(newBuilder);
            }
        }
        return newBuilders;
    }

    private static Solution.Part? NewPart(Entry entry, string textSlice, string previousPartsReading, string? potentialReading)
    {
        if (potentialReading is null)
        {
            return new(textSlice, null);
        }
        else if (!entry.NormalizedReadingText.StartsWith(previousPartsReading + potentialReading))
        {
            return null;
        }
        else if (textSlice.IsKanaEquivalent(potentialReading))
        {
            return new(textSlice, null);
        }
        else
        {
            // Use the non-normalized reading for the furigana text.
            int i = previousPartsReading.Length;
            int j = i + potentialReading.Length;
            var newPartReading = entry.ReadingText[i..j];
            return new(textSlice, newPartReading);
        }
    }
}
