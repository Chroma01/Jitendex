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
internal class KanaReadingSolverTest : SolverTest
{
    [TestMethod]
    public void TestUnequalLength()
    {
        var solver = new KanaReadingSolver(new ResourceSet([], []));
        TestVocabSuccess(solver, "阿呆陀羅", "あほんだら", "[阿|あ][呆|ほん][陀|だ][羅|ら]");
    }

    [TestMethod]
    public void TestSpecialExpression()
    {
        var solver = new KanaReadingSolver(new ResourceSet
        (
            [], [new SpecialExpression("発条", ["ばね"])]
        ));
        TestVocabSuccess(solver, "発条仕掛け", "ばねじかけ", "[発条|ばね][仕|じ][掛|か]け");
    }
}
