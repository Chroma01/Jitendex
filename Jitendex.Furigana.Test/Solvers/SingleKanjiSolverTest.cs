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
public class SingleKanjiSolverTest
{
    private static readonly SingleKanjiSolver _solver = new();

    [TestMethod]
    public void TestSingleKanji()
    {
        TestSuccess("腹", "はら", "[腹|はら]");
    }

    [TestMethod]
    public void TestSingleNonKanji()
    {
        TestSuccess("◯", "おおきなまる", "[◯|おおきなまる]");
    }

    [TestMethod]
    public void TestSuffixedKanji()
    {
        TestSuccess("難しい", "むずかしい", "[難|むずか]しい");
    }

    [TestMethod]
    public void TestPrefixedKanji()
    {
        TestSuccess("ばね秤", "ばねばかり", "ばね[秤|ばかり]");
    }

    [TestMethod]
    public void TestPrefixedAndSuffixedKanjiWithOneFuriganaCharacter()
    {
        TestSuccess("ぜんまい仕かけ", "ぜんまいじかけ", "ぜんまい[仕|じ]かけ");
    }

    [TestMethod]
    public void TestPrefixedAndSuffixedKanjiWithTwoFuriganaCharacters()
    {
        TestSuccess("ありがたい事に", "ありがたいことに", "ありがたい[事|こと]に");
    }

    [TestMethod]
    public void TestNormalization()
    {
        TestSuccess("アリガタイ事ニ", "ありがたいことに", "アリガタイ[事|こと]ニ");
    }

    [TestMethod]
    public void TestUtf16SurrogatePair()
    {
        var kanji = "𩺊";
        Assert.AreEqual(2, kanji.Length);
        TestSuccess(kanji, "あら", $"[{kanji}|あら]");
    }

    [TestMethod]
    public void TestSuffixedUtf16SurrogatePair()
    {
        var kanji = "𠮟";
        Assert.AreEqual(2, kanji.Length);
        TestSuccess($"{kanji}かり", "しかり", $"[{kanji}|し]かり");
    }

    [TestMethod]
    public void TestPrefixedAndSuffixedUtf16SurrogatePair()
    {
        var kanji = "𠮟";
        Assert.AreEqual(2, kanji.Length);
        TestSuccess($"れいせい{kanji}た", "れいせいしった", $"れいせい[{kanji}|しっ]た");
    }

    private static void TestSuccess(string KanjiFormText, string ReadingText, string expectedSolutionText)
    {
        var entry = new VocabEntry(KanjiFormText, ReadingText);
        var solution = _solver.Solve(entry).FirstOrDefault();
        Assert.IsNotNull(solution);

        var expectedSolution = Parser.Solution(expectedSolutionText, entry);
        CollectionAssert.AreEqual(expectedSolution.Parts, solution.ToTextSolution().Parts);
    }
}
