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

using Jitendex.Furigana.Models;
using Jitendex.Furigana.Solvers;

namespace Jitendex.Furigana.Business;

/// <summary>
/// Works with kanji and dictionary entries to attach each entry a furigana string.
/// </summary>
public class FuriganaBusiness
{
    private readonly List<IFuriganaSolver> _solvers;

    public FuriganaBusiness(FuriganaResourceSet resourceSet)
    {
        _solvers =
        [
            new KanaReadingSolver(resourceSet),
            new KanjiReadingSolver(resourceSet),
            new LengthMatchSolver(resourceSet),
            new NoConsecutiveKanjiSolver(resourceSet),
            new RepeatedKanjiSolver(),
            new SingleCharacterSolver(),
            new SingleKanjiSolver(resourceSet),
        ];
        _solvers.Sort();
        _solvers.Reverse();
    }

    /// <summary>
    /// Starts the process of associating a furigana string to vocab.
    /// </summary>
    /// <returns>The furigana vocab entries.</returns>
    public async IAsyncEnumerable<FuriganaSolutionSet> SolveRangeAsync(IEnumerable<VocabEntry> vocab)
    {
        var processingTasks = new List<Task<FuriganaSolutionSet>>();
        foreach (var v in vocab)
        {
            var task = Task.Run(() => Solve(v));
            processingTasks.Add(task);
        }
        await foreach (var task in Task.WhenEach(processingTasks))
        {
            yield return await task;
        }
    }

    public FuriganaSolutionSet Solve(VocabEntry v)
    {
        if (string.IsNullOrWhiteSpace(v.KanjiFormText) || string.IsNullOrWhiteSpace(v.ReadingText))
        {
            // Cannot solve when we do not have a kanji form or reading.
            return new FuriganaSolutionSet(v);
        }
        return Process(v);
    }

    private FuriganaSolutionSet Process(VocabEntry v)
    {
        var solutionSet = new FuriganaSolutionSet(v);
        int priority = _solvers.First().Priority;

        foreach (var solver in _solvers)
        {
            if (solver.Priority < priority)
            {
                if (solutionSet.Any())
                {
                    // Priority goes down and we already have solutions.
                    // Stop solving.
                    break;
                }

                // No solutions yet. Continue with the next level of priority.
                priority = solver.Priority;
            }

            // Add all solutions if they are correct and unique.
            var solution = solver.Solve(v);
            solutionSet.AddRange(solution);
        }

        return solutionSet;
    }
}
