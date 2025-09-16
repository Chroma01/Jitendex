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
using Jitendex.Furigana.Solver;

namespace Jitendex.Furigana.Test;

internal static class SolverTestMethods
{
    public static void TestSolvable(IterationSolver solver, IEnumerable<(string, string, string)> data)
    {
        foreach (var (kanjiFormText, readingText, expectedResultText) in data)
        {
            var entry = new VocabEntry(kanjiFormText, readingText);
            TestSingleSolvable(solver, entry, expectedResultText);
        }
    }

    public static void TestUnsolvable(IterationSolver solver, IEnumerable<(string, string, int)> data)
    {
        foreach (var (kanjiFormText, readingText, expectedSolutionCount) in data)
        {
            var entry = new VocabEntry(kanjiFormText, readingText);
            TestSingleUnsolvable(solver, entry, expectedSolutionCount);
        }
    }

    private static void TestSingleSolvable(IterationSolver solver, Entry entry, string expectedResultText)
    {
        var solutions = solver.Solve(entry).ToList();
        Assert.HasCount(1, solutions);

        var solution = solutions.First();
        var expectedSolution = TextSolution.Parse(expectedResultText, entry);
        CollectionAssert.AreEqual(expectedSolution.Parts, solution.Parts);
    }

    private static void TestSingleUnsolvable(IterationSolver solver, Entry entry, int expectedSolutionCount)
    {
        var solutions = solver.Solve(entry);
        Assert.AreEqual(expectedSolutionCount, solutions.Count());
    }
}
