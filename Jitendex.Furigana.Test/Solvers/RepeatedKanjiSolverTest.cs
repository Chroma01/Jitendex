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
internal class RepeatedKanjiSolverTest : SolverTest
{
    private static readonly RepeatedKanjiSolver _solver = new();

    #region Tests expected to return a single correct solution

    [TestMethod]
    public void TestOneKanaPerKanji()
    {
        TestSolution("唖々", "ああ", "0:あ;1:あ");
        TestSolution("唖唖", "ああ", "0:あ;1:あ");
    }

    [TestMethod]
    public void TestTwoKanaPerKanji()
    {
        TestSolution("抑抑", "そもそも", "0:そも;1:そも");
        TestSolution("抑々", "そもそも", "0:そも;1:そも");
        TestSolution("犇犇", "ひしひし", "0:ひし;1:ひし");
        TestSolution("犇々", "ひしひし", "0:ひし;1:ひし");
        TestSolution("愈愈", "いよいよ", "0:いよ;1:いよ");
        TestSolution("愈々", "いよいよ", "0:いよ;1:いよ");
        TestSolution("偶偶", "たまたま", "0:たま;1:たま");
        TestSolution("偶々", "たまたま", "0:たま;1:たま");
        TestSolution("益益", "ますます", "0:ます;1:ます");
        TestSolution("益々", "ますます", "0:ます;1:ます");
    }

    [TestMethod]
    public void TestRendaku()
    {
        TestSolution("日日", "ひび", "0:ひ;1:び");
        TestSolution("日々", "ひび", "0:ひ;1:び");
        TestSolution("時時", "ときどき", "0:とき;1:どき");
        TestSolution("時々", "ときどき", "0:とき;1:どき");
    }

    [TestMethod]
    public void TestUtf16SurrogatePair()
    {
        // "𩺊" is represented by a UTF-16 "Surrogate Pair"
        // with string Length == 2.
        TestSolution("𩺊𩺊", "あらあら", "0:あら;1:あら");
        TestSolution("𩺊々", "あらあら", "0:あら;1:あら");
    }

    [TestMethod]
    public void TestThreeKanaPerKanji()
    {
        TestSolution("州州", "しゅうしゅう", "0:しゅう;1:しゅう");
        TestSolution("州々", "しゅうしゅう", "0:しゅう;1:しゅう");
    }

    [TestMethod]
    public void TestNonKanji()
    {
        TestSolution("々々", "ときどき", "0:とき;1:どき");
        TestSolution("〇〇", "まるまる", "0:まる;1:まる");
    }

    private static void TestSolution(string kanjiForm, string reading, string expectedFurigana)
    {
        var solver = new RepeatedKanjiSolver();
        var vocab = new VocabEntry(kanjiForm, reading);
        var solutions = solver.Solve(vocab).ToList();
        Assert.HasCount(1, solutions);

        var solution = solutions.First().ToTextSolution();
        var expectedSolution = Parser.Index(expectedFurigana, vocab);
        CollectionAssert.AreEqual(expectedSolution.Parts, solution.Parts);
    }

    #endregion

    #region Tests expected to return no solutions

    [TestMethod]
    public void TestKana()
    {
        TestNullSolution(_solver, new VocabEntry("あ", "あ"));
        TestNullSolution(_solver, new VocabEntry("あゝ", "ああ"));
        TestNullSolution(_solver, new VocabEntry("ああ", "ああ"));
    }

    [TestMethod]
    public void TestNonRepeatKanji()
    {
        TestNullSolution(_solver, new VocabEntry("可能", "かのう"));
        TestNullSolution(_solver, new VocabEntry("津波", "つなみ"));
        TestNullSolution(_solver, new VocabEntry("問題", "もんだい"));
        TestNullSolution(_solver, new VocabEntry("質問", "しつもん"));
    }

    [TestMethod]
    public void TestOddLengthKana()
    {
        TestNullSolution(_solver, new VocabEntry("主主", "しゅしゅう"));
        TestNullSolution(_solver, new VocabEntry("主主", "ししゅ"));
    }

    [TestMethod]
    public void TestWrongLengthKanji()
    {
        TestNullSolution(_solver, new VocabEntry("々", "とき"));
        TestNullSolution(_solver, new VocabEntry("〇", "まる"));
        TestNullSolution(_solver, new VocabEntry("抑", "そも"));
        TestNullSolution(_solver, new VocabEntry("捗捗し", "はかばかし"));
        TestNullSolution(_solver, new VocabEntry("捗々し", "はかばかし"));
        TestNullSolution(_solver, new VocabEntry("捗捗しい", "はかばかしい"));
        TestNullSolution(_solver, new VocabEntry("捗々しい", "はかばかしい"));
    }

    #endregion
}
