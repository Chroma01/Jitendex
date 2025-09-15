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
    private readonly ReadingCache _readingCache;

    public IterationSolver(ReadingCache readingCache)
    {
        _readingCache = readingCache;
    }

    public IEnumerable<Solution> Solve(Entry entry)
    {
        var solutions = new List<SolutionBuilder>() { new() };
        for (int sliceStart = 0; sliceStart < entry.KanjiFormRunes.Length; sliceStart++)
        {
            var newSolutions = new List<SolutionBuilder>();
            for (int sliceEnd = entry.KanjiFormRunes.Length; sliceStart < sliceEnd; sliceEnd--)
            {
                var iterationSlice = new IterationSlice(entry, sliceStart, sliceEnd);
                newSolutions = IterateSolutions(entry, solutions, iterationSlice);
                if (newSolutions.Count > 0)
                {
                    sliceStart += sliceEnd - sliceStart - 1;
                    solutions = newSolutions;
                    break;
                }
            }
            if (newSolutions.Count == 0)
            {
                yield break;
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

    private List<SolutionBuilder> IterateSolutions(Entry entry, List<SolutionBuilder> partialSolutions, IterationSlice iterationSlice)
    {
        var solutions = new List<SolutionBuilder>();
        var sliceReadingCache = new SliceReadingCache(entry, iterationSlice, _readingCache);

        foreach (var partialSolution in partialSolutions)
        {
            var readingState = new ReadingState(entry, partialSolution.ReadingTextLength());
            var previousSolutionParts = partialSolution.ToParts();

            foreach (var nextSolutionParts in sliceReadingCache.EnumerateParts(readingState))
            {
                solutions.Add
                (
                    new SolutionBuilder(previousSolutionParts.AddRange(nextSolutionParts))
                );
            }
        }
        return solutions;
    }
}
