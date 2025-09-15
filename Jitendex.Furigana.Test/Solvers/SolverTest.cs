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
using Jitendex.Furigana.Solvers;

namespace Jitendex.Furigana.Test.Solvers;

internal class SolverTest
{
    protected static void TestSolution(IterationSolver solver, Entry entry, string expectedResultText)
    {
        var solutions = solver.Solve(entry).ToList();
        Assert.HasCount(1, solutions, $"\n\n{entry} {solver.GetType()}\n");

        var solution = solutions.First();
        var expectedSolution = Parser.Solution(expectedResultText, entry);
        CollectionAssert.AreEqual(expectedSolution.Parts, solution.Parts);
    }

    protected static void TestSolutions(IterationSolver solver, IEnumerable<(string, string, string)> data)
    {
        foreach (var (kanjiFormText, readingText, expectedResultText) in data)
        {
            var entry = new VocabEntry(kanjiFormText, readingText);
            TestSolution(solver, entry, expectedResultText);
        }
    }

    protected static void TestSolutionsCount(int expectedSolutionCount, IterationSolver solver, Entry entry)
    {
        var solutions = solver.Solve(entry);
        Assert.AreEqual(expectedSolutionCount, solutions.Count(), $"\n\n{entry} {solver.GetType()}\n");
    }

    protected static void TestNullSolutions(IterationSolver solver, IEnumerable<(string, string)> data)
    {
        foreach (var (kanjiFormText, readingText) in data)
        {
            var entry = new VocabEntry(kanjiFormText, readingText);
            TestSolutionsCount(0, solver, entry);
        }
    }
}
