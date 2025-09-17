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
global using UnsolvableData = System.Collections.Generic.IEnumerable<(string KanjiFormText, string ReadingText)>;

using Jitendex.Furigana.Models;

namespace Jitendex.Furigana.Test;

public class ServiceTest
{
    protected static Service DefaultService { get; } = new([], []);

    protected static void TestSolvable(Service service, SolvableData data)
    {
        foreach (var datum in data)
        {
            var entry = new VocabEntry(datum.KanjiFormText, datum.ReadingText);
            TestSingleSolvable(service, entry, datum.ExpectedSolutionText);
        }
    }

    protected static void TestUnsolvable(Service service, UnsolvableData data)
    {
        foreach (var datum in data)
        {
            var entry = new VocabEntry(datum.KanjiFormText, datum.ReadingText);
            TestSingleUnsolvable(service, entry);
        }
    }

    private static void TestSingleSolvable(Service service, Entry entry, string expectedSolutionText)
    {
        var solution = service.Solve(entry);
        Assert.IsNotNull(solution, $"\n\n{entry}\n");

        var expectedSolution = TextSolution.Parse(expectedSolutionText, entry);
        Assert.AreEqual(expectedSolution, solution);
    }

    private static void TestSingleUnsolvable(Service service, Entry entry)
    {
        var solution = service.Solve(entry);
        Assert.IsNull(solution, $"\n\n{entry}\n");
    }
}
