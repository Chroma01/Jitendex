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
    private static readonly IterationSolver _solver = new(new ReadingCache([], []));

    [TestMethod]
    public void SingleCorrectSolution()
    {
        var data = new List<(string, string, string)>()
        {
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

            // With rendaku
            ("日日", "ひび", "[日|ひ][日|び]"),
            ("日々", "ひび", "[日|ひ][々|び]"),
            ("時時", "ときどき", "[時|とき][時|どき]"),
            ("時々", "ときどき", "[時|とき][々|どき]"),

            // "𩺊" is represented by a UTF-16 "Surrogate Pair"
            // with string Length == 2.
            ("𩺊𩺊", "あらあら", "[𩺊|あら][𩺊|あら]"),
            ("𩺊々", "あらあら", "[𩺊|あら][々|あら]"),

            // Three kana per kanji
            ("州州", "しゅうしゅう", "[州|しゅう][州|しゅう]"),
            ("州々", "しゅうしゅう", "[州|しゅう][々|しゅう]"),

            // Non-kanji
            ("々々", "ときどき", "[々|とき][々|どき]"),
            ("〇〇", "まるまる", "[〇|まる][〇|まる]"),
        };

        TestSolutions(_solver, data);
    }

    [TestMethod]
    public void NullSolution()
    {
        var data = new List<(string, string)>()
        {
            // Kana
            // ("あ", "あ"),
            // ("あゝ", "ああ"),
            // ("ああ", "ああ"),

            // No kanji repeats
            ("可能", "かのう"),
            ("津波", "つなみ"),
            ("問題", "もんだい"),
            ("質問", "しつもん"),

            // Odd reading length
            ("主主", "しゅしゅう"),
            ("主主", "ししう"),

            // Wrong kanji form length
            // ("々", "とき"),
            // ("〇", "まる"),
            // ("抑", "そも"),
            // ("捗捗し", "はかばかし"),
            // ("捗々し", "はかばかし"),
            // ("捗捗しい", "はかばかしい"),
            // ("捗々しい", "はかばかしい"),
        };

        TestNullSolutions(_solver, data);
    }
}
