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

using Jitendex.Furigana.Models;

namespace Jitendex.Furigana.Solver;

internal class IterationSolver
{
    private readonly List<ISolutionParts> _solutionPartsGenerators;

    public IterationSolver(List<ISolutionParts> solutionPartsGenerators)
    {
        _solutionPartsGenerators = solutionPartsGenerators;
    }

    public IEnumerable<Solution> Solve(Entry entry)
    {
        var solutions = new List<SolutionBuilder>() { new() };

        for (int sliceStart = 0; sliceStart < entry.KanjiFormRunes.Length; sliceStart++)
        {
        BeginGeneratorLoop:
            foreach (var solutionPartsGenerator in _solutionPartsGenerators)
            {
                for (int sliceEnd = entry.KanjiFormRunes.Length; sliceStart < sliceEnd; sliceEnd--)
                {
                    var kanjiFormSlice = new KanjiFormSlice(entry, sliceStart, sliceEnd);
                    var newSolutions = IterateSolutions(solutionPartsGenerator, entry, kanjiFormSlice, solutions);
                    if (newSolutions.Count > 0)
                    {
                        sliceStart += sliceEnd - sliceStart;
                        solutions = newSolutions;
                        goto BeginGeneratorLoop;
                    }
                }
            }
        }

        foreach (var solutionBuilder in solutions)
        {
            var solution = solutionBuilder.ToSolution(entry);
            if (solution is not null)
            {
                yield return solution;
            }
        }
    }

    private List<SolutionBuilder> IterateSolutions(ISolutionParts solutionPartsGenerator, Entry entry, KanjiFormSlice kanjiFormSlice, List<SolutionBuilder> solutions)
    {
        var newSolutions = new List<SolutionBuilder>();

        foreach (var solution in solutions)
        {
            var solutionParts = solution.ToParts();
            var readingState = new ReadingState(entry, solution.ReadingTextLength());

            foreach (var newParts in solutionPartsGenerator.Enumerate(entry, kanjiFormSlice, readingState))
            {
                newSolutions.Add
                (
                    new(solutionParts.AddRange(newParts))
                );
            }
        }

        return newSolutions;
    }
}
