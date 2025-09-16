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

using Jitendex.Furigana.Models;
using Jitendex.Furigana.Solver;

namespace Jitendex.Furigana.Test.SolverTests;

[TestClass]
public class RequiresKanjiReadings
{
    private static readonly IEnumerable<JapaneseCharacter> _kanji = ResourceMethods.VocabKanji(new()
    {
        ["御"] = ["ギョ", "ゴ", "おん-", "お-", "み-"],
        ["姉"] = ["シ", "あね", "はは", "ねえ"],
        ["母"] = ["ボ", "はは", "も", "かあ"],
        ["兄"] = ["ケイ", "キョウ", "あに", "にい"],
        ["東"] = ["トウ", "ひがし"],
        ["京"] = ["キョウ", "ケイ", "キン", "みやこ"],
        ["湾"] = ["ワン", "いりえ"],
        ["日"] = ["ニチ", "ジツ", "ひ", "-び", "-か"],
        ["独"] = ["ドク", "トク", "ひと.り"],
        ["協"] = ["キョウ"],
        ["会"] = ["カイ", "エ", "あ.う", "あ.わせる", "あつ.まる"],
        ["可"] = ["カ", "コク", "-べ.き", "-べ.し"],
        ["能"] = ["ノウ", "よ.く", "あた.う"],
        ["津"] = ["シン", "つ"],
        ["波"] = ["ハ", "なみ"],
        ["問"] = ["モン", "と.う", "と.い", "とん"],
        ["題"] = ["ダイ"],
        ["質"] = ["シツ", "シチ", "チ", "たち", "ただ.す", "もと", "わりふ"],
        ["蝶"] = ["チョウ"],
        ["夫"] = ["フ", "フウ", "ブ", "おっと", "それ"],
        ["乱"] = ["ラン", "ロン", "みだ.れる", "みだ.る", "みだ.す", "みだ", "おさ.める", "わた.る"],
        ["脈"] = ["ミャク", "すじ",],
    });

    private static readonly IterationSolver _solver = new(_kanji, []);
    private static readonly IterationSolver _resourcelessSolver = new([], []);

    private static readonly (string, string, string)[] _data = new[]
    {
        ("御姉さん", "おねえさん", "[御|お][姉|ねえ]さん"),
        ("御母さん", "おかあさん", "[御|お][母|かあ]さん"),
        ("御兄さん", "おにいさん", "[御|お][兄|にい]さん"),
        ("東京湾", "とうきょうわん", "[東|とう][京|きょう][湾|わん]"),
        ("日独協会", "にちどくきょうかい", "[日|にち][独|どく][協|きょう][会|かい]"),

        // Two characters, but no kanji repeats
        ("可能", "かのう", "[可|か][能|のう]"),
        ("津波", "つなみ", "[津|つ][波|なみ]"),
        ("問題", "もんだい", "[問|もん][題|だい]"),
        ("質問", "しつもん", "[質|しつ][問|もん]"),
        ("乱脈", "らんみゃく", "[乱|らん][脈|みゃく]"),

        // Despite being repeated kanji, the reading length is odd
        ("蝶蝶", "ちょうちょ", "[蝶|ちょう][蝶|ちょ]"),
        ("夫夫", "ふうふ", "[夫|ふう][夫|ふ]"),
    };

    private static readonly (string, string, int)[] _dataWithNoSolutions = _data
        .Select(static x => (x.Item1, x.Item2, 0))
        .ToArray();

    [TestMethod]
    public void Test()
    {
        SolverTestMethods.TestSolvable(_solver, _data);
    }

    [TestMethod]
    public void TestUnsolvable()
    {
        SolverTestMethods.TestUnsolvable(_resourcelessSolver, _dataWithNoSolutions);
    }
}
