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

using Jitendex.Furigana.Solvers;

namespace Jitendex.Furigana.Test.Solvers;

[TestClass]
internal class SingleKanjiSolverTest : SolverTest
{
    private static readonly SingleKanjiSolver _solver = new();

    [TestMethod]
    public void SingleCorrectSolution()
    {
        var data = new List<(string, string, string)>()
        {
            // Single kanji
            ("腹", "はら", "[腹|はら]"),

            // Single non-kanji
            ("◯", "おおきなまる", "[◯|おおきなまる]"),

            // Suffixed kanji
            ("難しい", "むずかしい", "[難|むずか]しい"),

            // Prefixed kanji
            ("ばね秤", "ばねばかり", "ばね[秤|ばかり]"),

            // Prefixed and suffixed kanji with one furigana character
            ("ぜんまい仕かけ", "ぜんまいじかけ", "ぜんまい[仕|じ]かけ"),

            // Prefixed and suffixed kanji with two furigana characters
            ("ありがたい事に", "ありがたいことに", "ありがたい[事|こと]に"),

            // Non-normalized text (all have the same reading)
            ("アリガタイ事ニ", "ありがたいことに", "アリガタイ[事|こと]ニ"),
            ("ありがたい事ニ", "ありがたいことに", "ありがたい[事|こと]ニ"),
            ("アリガタイ事に", "ありがたいことに", "アリガタイ[事|こと]に"),
            ("アリガタイ事ニ", "ありがたいことに", "アリガタイ[事|こと]ニ"),
            ("アりガたイ事ニ", "ありがたいことに", "アりガたイ[事|こと]ニ"),

            // Furigana written in katakana
            ("ありがたい事に", "ありがたいコトに", "ありがたい[事|コト]に"),
        };

        TestSolutions(_solver, data);
    }

    [TestMethod]
    public void TestUtf16SurrogatePair()
    {
        var data = new Dictionary<string, (string, string, string)>()
        {
            ["𩺊"] = ("𩺊", "あら", "[𩺊|あら]"),
            ["𠮟"] = ("𠮟かり", "しかり", "[𠮟|し]かり"),
            ["𤸎"] = ("しょう𤸎", "しょうかち", "しょう[𤸎|かち]")
        };

        foreach(var item in data)
        {
            var kanji = item.Key;
            Assert.AreEqual(2, kanji.Length);
            Assert.Contains(kanji, item.Value.Item1);
            Assert.Contains(kanji, item.Value.Item3);
        }
        
        TestSolutions(_solver, data.Values);
    }
}
