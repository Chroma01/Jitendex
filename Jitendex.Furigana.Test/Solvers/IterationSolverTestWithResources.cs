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

[TestClass]
internal class IterationSolverTestWithResources : SolverTest
{
    [TestMethod]
    public void SingleCorrectSolution()
    {
        var solver = new IterationSolver(ServiceTest.MakeResourceSet(
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
            }));

        var data = new List<(string, string, string)>()
        {
            // 発条 uses a special reading
            ("発条仕掛け", "ぜんまいじかけ", "[発条|ぜんまい][仕|じ][掛|か]け"),
            ("発条仕掛け", "ばねじかけ", "[発条|ばね][仕|じ][掛|か]け"),

            // 発条 uses regular, kanji dictionary readings
            ("発条仕掛け", "はつじょうじかけ", "[発|はつ][条|じょう][仕|じ][掛|か]け"),

            // Bogus data for laughs
            ("発条仕掛け", "ああああけ", "[発|あ][条|あ][仕|あ][掛|あ]け"),

            // Repeat the above tests with non-normalized readings
            ("発条仕掛け", "ゼンマイじかけ", "[発条|ゼンマイ][仕|じ][掛|か]け"),
            ("発条仕掛け", "バねジカけ", "[発条|バね][仕|ジ][掛|カ]け"),
            ("発条仕掛け", "ハつじョうじカケ", "[発|ハつ][条|じョう][仕|じ][掛|カ]け"),
            ("発条仕掛け", "あアあアけ", "[発|あ][条|ア][仕|あ][掛|ア]け"),
        };

        TestSolutions(solver, data);
    }

    [TestMethod]
    public void OnlySpecialExpressionData()
    {
        var solver = new IterationSolver(ServiceTest.MakeResourceSet
        (
            [], new() { ["発条"] = ["ぜんまい", "ばね"] }
        ));

        var data = new List<(string, string, string)>()
        {
            // 発条 uses a special reading
            ("発条仕掛け", "ぜんまいじかけ", "[発条|ぜんまい][仕|じ][掛|か]け"),
            ("発条仕掛け", "ばねじかけ", "[発条|ばね][仕|じ][掛|か]け"),

            // This is bogus data but it will solve because it's the correct length.
            // With this resource set, the ReadingIteration solver won't solve this.
            ("発条仕掛け", "ああああけ", "[発|あ][条|あ][仕|あ][掛|あ]け"),
        };

        TestSolutions(solver, data);

        // Unsolvable without kanji reading reasource data.
        var unsolvableEntry = new VocabEntry("発条仕掛け", "はつじょうじかけ");
        TestNullSolution(solver, unsolvableEntry);
    }

    /// <summary>
    /// Tests a situation in which there is no unique correct solution in principle.
    /// </summary>
    /// <remarks>
    /// [好|すき][嫌|きら] and [好|す][嫌|ききら] are both valid solutions according to the parameters of the problem.
    /// </remarks>
    [TestMethod]
    public void TestUnsolvable()
    {
        var entry = new VocabEntry("好嫌", "すききら");
        var solver = new IterationSolver(ServiceTest.MakeResourceSet(new()
        {
            ["好"] = ["すき", "す"],
            ["嫌"] = ["きら", "ききら"],
        }));
        var solutions = solver.Solve(entry).ToList();
        Assert.HasCount(2, solutions);
    }
}
