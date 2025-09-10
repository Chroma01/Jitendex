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

using Jitendex.Furigana.InputModels;
using Jitendex.Furigana.OutputModels;

internal class ReadingIterationSolver
{
    private readonly ResourceSet _resourceSet;

    public ReadingIterationSolver(ResourceSet resourceSet)
    {
        _resourceSet = resourceSet;
    }

    public Solution? Solve(Entry entry)
    {
        var partialSolutions = new List<PartialSolution>()
            { new() { Parts = [new("", null)], IsInitial = true }};

        for (int sliceStart = 0; sliceStart < entry.KanjiFormRunes.Length; sliceStart++)
        {
            partialSolutions = IterateSolutions(entry, partialSolutions, ref sliceStart);
            if (partialSolutions is null)
                return null;
        }

        var solutions = partialSolutions
            .Where(s => s.ReadingText() == entry.ReadingText);

        if (solutions.Count() != 1)
        {
            // Failed to find a unique solution.
            return null;
        }

        return new Solution
        {
            Entry = entry,
            Parts = [.. solutions.First().Parts]
        };
    }

    private List<PartialSolution>? IterateSolutions(Entry entry, List<PartialSolution> solutions, ref int sliceStart)
    {
        var newSolutions = new List<PartialSolution>();
        var runes = entry.KanjiFormRunes;

        bool isFirstRune = sliceStart == 0;
        bool isLastRune = sliceStart == runes.Length - 1;

        for (int sliceEnd = runes.Length; sliceStart < sliceEnd; sliceEnd--)
        {
            var runesSlice = runes[sliceStart..sliceEnd];
            var textSlice = string.Join(string.Empty, runesSlice);

            var potentialReadings = runesSlice.Length == 1 ?
                _resourceSet.GetPotentialReadings(runesSlice[0], entry, isFirstRune, isLastRune) :
                _resourceSet.GetPotentialReadings(textSlice);

            foreach (var solution in solutions)
            {
                var permutationReading = solution.ReadingText();
                foreach (var potentialReading in potentialReadings)
                {
                    if (potentialReading is null || entry.ReadingText.StartsWith(permutationReading + potentialReading))
                    {
                        Solution.Part newPart =
                            textSlice == potentialReading ?
                            new(textSlice, null) :
                            new(textSlice, potentialReading);

                        var newPremutation = new PartialSolution
                        {
                            Parts = solution.IsInitial ?
                                [newPart] :
                                [.. solution.Parts.Concat([newPart])]
                        };
                        newSolutions.Add(newPremutation);
                    }
                }
            }
            if (newSolutions.Count > 0)
            {
                sliceStart += textSlice.Length - 1;
                return newSolutions;
            }
        }
        // Exhausted all valid permutation branches
        return null;
    }

    private class PartialSolution
    {
        public required List<Solution.Part> Parts;
        public bool IsInitial;
        public string KanjiFormText() =>
            new(Parts.SelectMany(static x => x.BaseText).ToArray());
        public string ReadingText() =>
            new(Parts.SelectMany(static x => x.Furigana ?? x.BaseText).ToArray());
    }
}
