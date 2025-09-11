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
public class RepeatedKanjiSolverTest
{
    #region Tests expected to succeed

    [TestMethod]
    public void TestOneKanaPerKanji()
    {
        TestVocabSuccess("唖々", "ああ", "0:あ;1:あ");
        TestVocabSuccess("唖唖", "ああ", "0:あ;1:あ");
    }

    [TestMethod]
    public void TestTwoKanaPerKanji()
    {
        TestVocabSuccess("抑抑", "そもそも", "0:そも;1:そも");
        TestVocabSuccess("抑々", "そもそも", "0:そも;1:そも");
        TestVocabSuccess("犇犇", "ひしひし", "0:ひし;1:ひし");
        TestVocabSuccess("犇々", "ひしひし", "0:ひし;1:ひし");
        TestVocabSuccess("愈愈", "いよいよ", "0:いよ;1:いよ");
        TestVocabSuccess("愈々", "いよいよ", "0:いよ;1:いよ");
        TestVocabSuccess("偶偶", "たまたま", "0:たま;1:たま");
        TestVocabSuccess("偶々", "たまたま", "0:たま;1:たま");
        TestVocabSuccess("益益", "ますます", "0:ます;1:ます");
        TestVocabSuccess("益々", "ますます", "0:ます;1:ます");
    }

    [TestMethod]
    public void TestRendaku()
    {
        TestVocabSuccess("日日", "ひび", "0:ひ;1:び");
        TestVocabSuccess("日々", "ひび", "0:ひ;1:び");
        TestVocabSuccess("時時", "ときどき", "0:とき;1:どき");
        TestVocabSuccess("時々", "ときどき", "0:とき;1:どき");
    }

    [TestMethod]
    public void TestUtf16SurrogatePair()
    {
        // "𩺊" is represented by a UTF-16 "Surrogate Pair"
        // with string Length == 2.
        TestVocabSuccess("𩺊𩺊", "あらあら", "0:あら;1:あら");
        TestVocabSuccess("𩺊々", "あらあら", "0:あら;1:あら");
    }

    [TestMethod]
    public void TestThreeKanaPerKanji()
    {
        TestVocabSuccess("州州", "しゅうしゅう", "0:しゅう;1:しゅう");
        TestVocabSuccess("州々", "しゅうしゅう", "0:しゅう;1:しゅう");
    }

    [TestMethod]
    public void TestNonKanji()
    {
        TestVocabSuccess("々々", "ときどき", "0:とき;1:どき");
        TestVocabSuccess("〇〇", "まるまる", "0:まる;1:まる");
    }

    private static void TestVocabSuccess(string kanjiForm, string reading, string expectedFurigana)
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

    #region Tests expected to fail

    [TestMethod]
    public void TestKana()
    {
        TestFailure("あ", "あ");
        TestFailure("あゝ", "ああ");
        TestFailure("ああ", "ああ");
    }

    [TestMethod]
    public void TestNonRepeatKanji()
    {
        TestFailure("可能", "かのう");
        TestFailure("津波", "つなみ");
        TestFailure("問題", "もんだい");
        TestFailure("質問", "しつもん");
    }

    [TestMethod]
    public void TestOddLengthKana()
    {
        TestFailure("主主", "しゅしゅう");
        TestFailure("主主", "ししゅ");
    }

    [TestMethod]
    public void TestWrongLengthKanji()
    {
        TestFailure("々", "とき");
        TestFailure("〇", "まる");
        TestFailure("抑", "そも");
        TestFailure("捗捗し", "はかばかし");
        TestFailure("捗々し", "はかばかし");
        TestFailure("捗捗しい", "はかばかしい");
        TestFailure("捗々しい", "はかばかしい");
    }

    private static void TestFailure(string kanjiForm, string reading)
    {
        var solver = new RepeatedKanjiSolver();
        var vocab = new VocabEntry(kanjiForm, reading);
        var solution = solver.Solve(vocab).FirstOrDefault();
        Assert.IsNull(solution);
    }

    #endregion
}
