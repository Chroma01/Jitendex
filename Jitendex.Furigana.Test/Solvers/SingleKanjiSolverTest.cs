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
internal class SingleKanjiSolverTest : SolverTest
{
    private static readonly SingleKanjiSolver _solver = new();

    [TestMethod]
    public void TestSingleKanji()
    {
        TestSolution(_solver, new VocabEntry("腹", "はら"), "[腹|はら]");
    }

    [TestMethod]
    public void TestSingleNonKanji()
    {
        TestSolution(_solver, new VocabEntry("◯", "おおきなまる"), "[◯|おおきなまる]");
    }

    [TestMethod]
    public void TestSuffixedKanji()
    {
        TestSolution(_solver, new VocabEntry("難しい", "むずかしい"), "[難|むずか]しい");
    }

    [TestMethod]
    public void TestPrefixedKanji()
    {
        TestSolution(_solver, new VocabEntry("ばね秤", "ばねばかり"), "ばね[秤|ばかり]");
    }

    [TestMethod]
    public void TestPrefixedAndSuffixedKanjiWithOneFuriganaCharacter()
    {
        TestSolution(_solver, new VocabEntry("ぜんまい仕かけ", "ぜんまいじかけ"), "ぜんまい[仕|じ]かけ");
    }

    [TestMethod]
    public void TestPrefixedAndSuffixedKanjiWithTwoFuriganaCharacters()
    {
        TestSolution(_solver, new VocabEntry("ありがたい事に", "ありがたいことに"), "ありがたい[事|こと]に");
    }

    [TestMethod]
    public void TestNormalization()
    {
        TestSolution(_solver, new VocabEntry("アリガタイ事ニ", "ありがたいことに"), "アリガタイ[事|こと]ニ");
    }

    [TestMethod]
    public void TestUtf16SurrogatePair()
    {
        var kanji = "𩺊";
        Assert.AreEqual(2, kanji.Length);
        TestSolution(_solver, new VocabEntry(kanji, "あら"), $"[{kanji}|あら]");
    }

    [TestMethod]
    public void TestSuffixedUtf16SurrogatePair()
    {
        var kanji = "𠮟";
        Assert.AreEqual(2, kanji.Length);
        TestSolution(_solver, new VocabEntry($"{kanji}かり", "しかり"), $"[{kanji}|し]かり");
    }

    [TestMethod]
    public void TestPrefixedAndSuffixedUtf16SurrogatePair()
    {
        var kanji = "𠮟";
        Assert.AreEqual(2, kanji.Length);
        TestSolution(_solver, new VocabEntry($"れいせい{kanji}た", "れいせいしった"), $"れいせい[{kanji}|しっ]た");
    }
}
