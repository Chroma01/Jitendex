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
    protected static readonly ResourceSet _resourceSet発条仕掛け = ServiceTest.MakeResourceSet(
        new()
        {
            ["発"] = ["ハツ", "ホツ", "た.つ", "あば.く", "おこ.る", "つか.わす", "はな.つ"],
            ["条"] = ["ジョウ", "チョウ", "デキ", "えだ", "すじ"],
            ["仕"] = ["シ", "ジ", "つか.える"],
            ["掛"] = ["カイ", "ケイ", "か.ける", "-か.ける", "か.け", "-か.け", "-が.け", "か.かる", "-か.かる", "-が.かる", "か.かり", "-が.かり", "かかり", "-がかり"],
        },
        new()
        {
            ["発条"] = ["ぜんまい", "ばね"],
        });

    protected static void TestSolution(IFuriganaSolver solver, Entry entry, string expectedResultText)
    {
        var solutions = solver.Solve(entry).ToList();
        Assert.HasCount(1, solutions);

        var solution = solutions.First();
        var expectedSolution = Parser.Solution(expectedResultText, entry);
        CollectionAssert.AreEqual(expectedSolution.Parts, solution.ToTextSolution().Parts);
    }

    protected static void TestSolutions(IFuriganaSolver solver, IEnumerable<(string, string, string)> data)
    {
        foreach (var (kanjiFormText, readingText, expectedResultText) in data)
        {
            var entry = new VocabEntry(kanjiFormText, readingText);
            TestSolution(solver, entry, expectedResultText);
        }
    }

    protected static void TestNullSolution(IFuriganaSolver solver, Entry entry)
    {
        var solution = solver.Solve(entry).FirstOrDefault();
        Assert.IsNull(solution);
    }

    protected static void TestNullSolutions(IFuriganaSolver solver, IEnumerable<(string, string)> data)
    {
        foreach (var (kanjiFormText, readingText) in data)
        {
            var entry = new VocabEntry(kanjiFormText, readingText);
            TestNullSolution(solver, entry);
        }
    }
}
