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
            Parts = [.. solutions.First().GetNormalizedParts()]
        };
    }

    private List<PartialSolution>? IterateSolutions(Entry entry, List<PartialSolution> solutions, ref int sliceStart)
    {
        var runes = entry.KanjiFormRunes;

        for (int sliceEnd = runes.Length; sliceStart < sliceEnd; sliceEnd--)
        {
            var potentialReadings = GetPotentialReadings(entry, sliceStart, sliceEnd);
            var rawTextSlice = string.Join(string.Empty, entry.RawKanjiFormRunes[sliceStart..sliceEnd]);
            var newSolutions = SolutionsForSlice(entry, solutions, potentialReadings, rawTextSlice);
            if (newSolutions.Count > 0)
            {
                sliceStart += rawTextSlice.Length - 1;
                return newSolutions;
            }
        }
        // No valid partial solutions found.
        return null;
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

    private List<PartialSolution> SolutionsForSlice(Entry entry, List<PartialSolution> solutions, ImmutableArray<string> potentialReadings, string textSlice)
    {
        var newSolutions = new List<PartialSolution>();
        foreach (var solution in solutions)
        {
            var previousPartsReading = solution.ReadingText();
            foreach (var newPartReading in potentialReadings)
            {
                if (newPartReading is null || entry.ReadingText.StartsWith(previousPartsReading + newPartReading))
                {
                    Solution.Part newPart =
                        textSlice == newPartReading ?
                        new(textSlice, null) :
                        new(textSlice, newPartReading);

                    var newSolution = new PartialSolution
                    {
                        Parts = solution.IsInitial ?
                            [newPart] :
                            [.. solution.Parts.Append(newPart)]
                    };
                    newSolutions.Add(newSolution);
                }
            }
        }
        return newSolutions;
    }

    private class PartialSolution
    {
        public required List<Solution.Part> Parts;
        public bool IsInitial;

        public string ReadingText() =>
            new(Parts.SelectMany(static x => x.Furigana ?? x.BaseText).ToArray());

        /// <summary>
        /// Merge consecutive parts together if they have null furigana.
        /// </summary>
        public List<Solution.Part> GetNormalizedParts()
        {
            var parts = new List<Solution.Part>();
            var mergedTexts = new List<string>();
            foreach (var part in Parts)
            {
                if (part.Furigana is null)
                {
                    mergedTexts.Add(part.BaseText);
                    continue;
                }
                if (mergedTexts.Count > 0)
                {
                    var baseText = string.Join(string.Empty, mergedTexts);
                    parts.Add(new Solution.Part(baseText, null));
                    mergedTexts = new List<string>();
                }
                parts.Add(part);
            }
            if (mergedTexts.Count > 0)
            {
                var baseText = string.Join(string.Empty, mergedTexts);
                parts.Add(new Solution.Part(baseText, null));
            }
            return parts;
        }
    }
}
