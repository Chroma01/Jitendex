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

using Jitendex.Furigana.Business;
using Jitendex.Furigana.Models;
using Jitendex.Furigana.Solvers;

namespace Jitendex.Furigana.Test;

[TestClass]
public class RepeatedKanjiSolverTest
{
    [TestMethod]
    public void TestOneKanaPerKanji()
    {
        TestSuccess("唖々", "ああ", "0:あ;1:あ");
        TestSuccess("唖唖", "ああ", "0:あ;1:あ");
    }

    [TestMethod]
    public void TestTwoKanaPerKanji()
    {
        TestSuccess("抑抑", "そもそも", "0:そも;1:そも");
        TestSuccess("抑々", "そもそも", "0:そも;1:そも");
        TestSuccess("犇犇", "ひしひし", "0:ひし;1:ひし");
        TestSuccess("犇々", "ひしひし", "0:ひし;1:ひし");
        TestSuccess("愈愈", "いよいよ", "0:いよ;1:いよ");
        TestSuccess("愈々", "いよいよ", "0:いよ;1:いよ");
        TestSuccess("偶偶", "たまたま", "0:たま;1:たま");
        TestSuccess("偶々", "たまたま", "0:たま;1:たま");
        TestSuccess("益益", "ますます", "0:ます;1:ます");
        TestSuccess("益々", "ますます", "0:ます;1:ます");
    }

    [TestMethod]
    public void TestRendaku()
    {
        TestSuccess("日日", "ひび", "0:ひ;1:び");
        TestSuccess("日々", "ひび", "0:ひ;1:び");
        TestSuccess("時時", "ときどき", "0:とき;1:どき");
        TestSuccess("時々", "ときどき", "0:とき;1:どき");
    }

    [TestMethod]
    public void TestUtf16SurrogatePair()
    {
        // "𩺊" is represented by a UTF-16 "Surrogate Pair"
        // with string Length == 2.
        TestSuccess("𩺊𩺊", "あらあら", "0:あら;1:あら");
        TestSuccess("𩺊々", "あらあら", "0:あら;1:あら");
    }

    [TestMethod]
    public void TestThreeKanaPerKanji()
    {
        TestSuccess("州州", "しゅうしゅう", "0:しゅう;1:しゅう");
        TestSuccess("州々", "しゅうしゅう", "0:しゅう;1:しゅう");
    }

    [TestMethod]
    public void TestNonKanji()
    {
        TestFailure("あ", "あ");
        TestFailure("々", "とき");
        TestFailure("〇", "まる");
        TestFailure("あゝ", "ああ");
        TestFailure("ああ", "ああ");
        TestFailure("々々", "ときどき");
        TestFailure("〇〇", "まるまる");
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
        TestFailure("抑", "そも");
        TestFailure("捗捗し", "はかばかし");
        TestFailure("捗々し", "はかばかし");
        TestFailure("捗捗しい", "はかばかしい");
        TestFailure("捗々しい", "はかばかしい");
    }

    private static void TestSuccess(string kanjiForm, string reading, string expectedFurigana)
    {
        var solver = new RepeatedKanjiSolver();
        var vocab = new VocabEntry(kanjiForm, reading);
        var solutions = solver.Solve(vocab).ToList();
        Assert.HasCount(1, solutions);

        var solution = solutions.First();
        var expectedSolution = FuriganaSolutionParser.Parse(expectedFurigana, vocab);
        Assert.AreEqual(expectedSolution, solution);
    }

    private static void TestFailure(string kanjiForm, string reading)
    {
        var solver = new RepeatedKanjiSolver();
        var vocab = new VocabEntry(kanjiForm, reading);
        var solution = solver.Solve(vocab).FirstOrDefault();
        Assert.IsNull(solution);
    }
}
