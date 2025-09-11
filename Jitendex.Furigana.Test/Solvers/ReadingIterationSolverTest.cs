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
internal class ReadingIterationSolverTest : SolverTest
{
    private static readonly ResourceSet _resourceSet = ServiceTest.MakeResourceSet(
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

    private static readonly ReadingIterationSolver _solver = new(_resourceSet);

    [TestMethod]
    public void Testぜんまい()
    {
        TestVocabSuccess(_solver, "発条仕掛け", "ぜんまいじかけ", "[発条|ぜんまい][仕|じ][掛|か]け");
    }

    [TestMethod]
    public void Testばね()
    {
        TestVocabSuccess(_solver, "発条仕掛け", "ばねじかけ", "[発条|ばね][仕|じ][掛|か]け");
    }

    [TestMethod]
    public void Testはつじょう()
    {
        TestVocabSuccess(_solver, "発条仕掛け", "はつじょうじかけ", "[発|はつ][条|じょう][仕|じ][掛|か]け");
    }

    [TestMethod]
    public void Testああ()
    {
        var entry = new VocabEntry("発条仕掛け", "ああああけ");
        var solution = _solver.Solve(entry).FirstOrDefault();
        Assert.IsNull(solution);
    }
}
