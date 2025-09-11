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
        var solution = IterateSolutions(entry);

        if (solution is null)
            yield break;

        yield return new IndexedSolution
        (
            entry: entry,
            parts: [.. solution.GetIndexedParts()]
        );
    }

    private SolutionBuilder? IterateSolutions(Entry entry)
    {
        var solutions = new List<SolutionBuilder>()
            { new() { Parts = [new("", null)], IsInitial = true }};

        for (int sliceStart = 0; sliceStart < entry.KanjiFormRunes.Length; sliceStart++)
        {
            var newSolutions = new List<SolutionBuilder>();
            for (int sliceEnd = entry.KanjiFormRunes.Length; sliceStart < sliceEnd; sliceEnd--)
            {
                var potentialReadings = GetPotentialReadings(entry, sliceStart, sliceEnd);
                var rawTextSlice = string.Join(string.Empty, entry.RawKanjiFormRunes[sliceStart..sliceEnd]);
                newSolutions = SolutionsForSlice(entry, solutions, potentialReadings, rawTextSlice);
                if (newSolutions.Count > 0)
                {
                    sliceStart += rawTextSlice.Length - 1;
                    solutions = newSolutions;
                    break;
                }
            }
            if (newSolutions.Count == 0)
                return null;
        }
        return ValidSolution(entry, solutions);
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

    private List<SolutionBuilder> SolutionsForSlice(Entry entry, List<SolutionBuilder> solutions, ImmutableArray<string> potentialReadings, string textSlice)
    {
        var newSolutions = new List<SolutionBuilder>();
        foreach (var solution in solutions)
        {
            var previousPartsReadingNormalized = solution.NormalizedReadingText();
            foreach (var potentialReading in potentialReadings)
            {
                var newPart = NewPart(entry, textSlice, previousPartsReadingNormalized, potentialReading);
                if (newPart is null)
                    continue;
                var newSolution = new SolutionBuilder
                {
                    Parts = solution.IsInitial ?
                        [newPart] :
                        [.. solution.Parts.Append(newPart)]
                };
                newSolutions.Add(newSolution);
            }
        }
        return newSolutions;
    }

    private Solution.Part? NewPart(Entry entry, string textSlice, string previousPartsReading, string? potentialReading)
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

    private SolutionBuilder? ValidSolution(Entry entry, List<SolutionBuilder> solutions)
    {
        // It's necessary to check for length equality with the original reading here
        // because the iteration algorithm only checks `StartsWith()`
        var validSolutions = solutions
            .Where(s => s.ReadingTextLength() == entry.ReadingText.Length);
        if (validSolutions.Count() == 1)
        {
            return validSolutions.First();
        }
        else
        {
            return null;
        }
    }
}
