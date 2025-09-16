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

using Jitendex.Furigana.Solver;

namespace Jitendex.Furigana.Test.SolverTests;

[TestClass]
public class RepeatedKanji
{
    private static readonly IterationSolver _solver = new([], []);

    private static readonly (string, string, string)[] _data =
    [
        // One kana per kanji
        ("唖々", "ああ", "[唖|あ][々|あ]"),
        ("唖唖", "ああ", "[唖|あ][唖|あ]"),

        // Two kana per kanji
        ("抑抑", "そもそも", "[抑|そも][抑|そも]"),
        ("抑々", "そもそも", "[抑|そも][々|そも]"),
        ("犇犇", "ひしひし", "[犇|ひし][犇|ひし]"),
        ("犇々", "ひしひし", "[犇|ひし][々|ひし]"),
        ("愈愈", "いよいよ", "[愈|いよ][愈|いよ]"),
        ("愈々", "いよいよ", "[愈|いよ][々|いよ]"),
        ("偶偶", "たまたま", "[偶|たま][偶|たま]"),
        ("偶々", "たまたま", "[偶|たま][々|たま]"),
        ("益益", "ますます", "[益|ます][益|ます]"),
        ("益々", "ますます", "[益|ます][々|ます]"),

        // Three kana per kanji
        ("州州", "しゅうしゅう", "[州|しゅう][州|しゅう]"),
        ("州々", "しゅうしゅう", "[州|しゅう][々|しゅう]"),

        // With a different reading for each kanji
        ("日日", "ひび", "[日|ひ][日|び]"),
        ("日々", "ひび", "[日|ひ][々|び]"),
        ("時時", "ときどき", "[時|とき][時|どき]"),
        ("時々", "ときどき", "[時|とき][々|どき]"),

        // With bordering kana
        ("捗捗しい", "はかばかしい", "[捗|はか][捗|ばか]しい"),
        ("捗々しい", "はかばかしい", "[捗|はか][々|ばか]しい"),
        ("かなしい時時", "かなしいときどき", "かなしい[時|とき][時|どき]"),
        ("かなしい時々", "かなしいときどき", "かなしい[時|とき][々|どき]"),
        ("捗捗しい時々", "はかばかしいときどき", "[捗|はか][捗|ばか]しい[時|とき][々|どき]"),
        ("捗々しい時時", "はかばかしいときどき", "[捗|はか][々|ばか]しい[時|とき][時|どき]"),
    ];

    private static readonly (string, string, int)[] _unsolvableData =
    [
        // Non-kanji
        ("々々", "ときどき", 0),
        ("〇〇", "まるまる", 0),
        ("ＡＡ", "アー", 0),
        ("ＸＸ", "ダブル・エックス", 0),
    ];

    [TestMethod]
    public void Test()
    {
        SolverTestMethods.TestSolvable(_solver, _data);
    }

    [TestMethod]
    public void TestUnsolvable()
    {
        SolverTestMethods.TestUnsolvable(_solver, _unsolvableData);
    }
}
