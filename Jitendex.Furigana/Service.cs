/*
Copyright (c) 2015, 2017 Doublevil
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
using Jitendex.Furigana.Solvers;

namespace Jitendex.Furigana;

/// <summary>
/// Works with kanji and dictionary entries to attach each entry a furigana string.
/// </summary>
public class Service
{
    private readonly List<IFuriganaSolver> _solvers;

    public Service(ResourceSet resourceSet)
    {
        _solvers =
        [
            new IterationSolver(resourceSet),
            new RegexSolver(),
            new RepeatedKanjiSolver(),
            new SingleKanjiSolver(),
        ];
        _solvers.Sort();
        _solvers.Reverse();
    }

    public Solution? Solve(Entry entry)
    {
        var solutionSet = new HashSet<IndexedSolution>();
        int priority = _solvers.First().Priority;

        foreach (var solver in _solvers)
        {
            if (solver.Priority < priority)
            {
                if (solutionSet.Count > 0)
                {
                    // Priority goes down and we already have solutions.
                    // Stop solving.
                    break;
                }
                // No solutions yet. Continue with the next level of priority.
                priority = solver.Priority;
            }
            // Add all solutions if they are correct and unique.
            foreach (var solution in solver.Solve(entry))
                solutionSet.Add(solution);
        }

        if (solutionSet.Count == 1)
            return solutionSet.First().ToTextSolution();
        else
            return null;
    }
}
