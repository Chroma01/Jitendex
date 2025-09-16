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

global using SolvableData = System.Collections.Generic.IEnumerable<(string KanjiFormText, string ReadingText, string ExpectedSolutionText)>;
global using UnsolvableData = System.Collections.Generic.IEnumerable<(string KanjiFormText, string ReadingText, int ExpectedSolutionCount)>;

using Jitendex.Furigana.Models;
using Jitendex.Furigana.Solver;

namespace Jitendex.Furigana.Test;

internal static class SolverTestMethods
{
    public static void TestSolvable(IterationSolver solver, SolvableData data)
    {
        foreach (var datum in data)
        {
            var entry = new VocabEntry(datum.KanjiFormText, datum.ReadingText);
            TestSingleSolvable(solver, entry, datum.ExpectedSolutionText);
        }
    }

    public static void TestUnsolvable(IterationSolver solver, UnsolvableData data)
    {
        foreach (var datum in data)
        {
            var entry = new VocabEntry(datum.KanjiFormText, datum.ReadingText);
            TestSingleUnsolvable(solver, entry, datum.ExpectedSolutionCount);
        }
    }

    private static void TestSingleSolvable(IterationSolver solver, Entry entry, string expectedSolutionText)
    {
        var solutions = solver.Solve(entry).ToList();
        Assert.HasCount(1, solutions, $"\n\n{entry}\n");

        var solution = solutions.First();
        var expectedSolution = TextSolution.Parse(expectedSolutionText, entry);
        CollectionAssert.AreEqual(expectedSolution.Parts, solution.Parts);
    }

    private static void TestSingleUnsolvable(IterationSolver solver, Entry entry, int expectedSolutionCount)
    {
        var solutions = solver.Solve(entry);
        Assert.AreEqual(expectedSolutionCount, solutions.Count(), $"\n\n{entry}\n");
    }
}
