/*
Copyright (c) 2025 Doublevil
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

using System.Text;
using Jitendex.Furigana.Models;

namespace Jitendex.Furigana.Test;

[TestClass]
public class FuriganaTest
{
    [TestMethod]
    public void TestFuriganaIkkagetsu()
    {
        var resourceSet = new FuriganaResourceSet
        ([
            new Kanji(new Rune('一'), ["イチ", "イツ", "ひと-", "ひと.つ"]),
            new Kanji(new Rune('月'), ["ゲツ", "ガツ", "つき"]),
        ],
        [
            new SpecialExpression { Expression = "ヶ", Readings = ["ヶ", "か", "が"] }
        ]);
        TestFurigana("一ヶ月", "いっかげつ", "0:いっ;1:か;2:げつ", resourceSet);
    }

    [TestMethod]
    public void TestFuriganaGanbaru()
    {
        // Readings cannot begin with 'ん', so there is 1 possible solution.
        // No need to supply any character readings.
        var resourceSet = new FuriganaResourceSet();
        TestFurigana("頑張る", "がんばる", "0:がん;1:ば", resourceSet);
    }

    [TestMethod]
    public void TestFuriganaObocchan()
    {
        var resourceSet = new FuriganaResourceSet();
        TestFurigana("御坊っちゃん", "おぼっちゃん", "0:お;1:ぼ", resourceSet);
    }

    [TestMethod]
    public void TestFuriganaAra()
    {
        // This kanji is represented by a UTF-16 "Surrogate Pair."
        // The string has Length == 2.
        var resourceSet = new FuriganaResourceSet();
        TestFurigana("𩺊", "あら", "0:あら", resourceSet);
    }

    [TestMethod]
    public void TestFuriganaIjirimawasu()
    {
        // 1 possible solution. No need to supply any character readings.
        var resourceSet = new FuriganaResourceSet();
        TestFurigana("弄り回す", "いじりまわす", "0:いじ;2:まわ", resourceSet);
    }

    [TestMethod]
    public void TestFuriganaKassarau()
    {
        // 1 possible solution. No need to supply any character readings.
        var resourceSet = new FuriganaResourceSet();
        TestFurigana("掻っ攫う", "かっさらう", "0:か;2:さら", resourceSet);
    }

    [TestMethod]
    public void TestFuriganaOneesan()
    {
        var resourceSet = new FuriganaResourceSet
        ([
            new Kanji(new Rune('御'), ["ギョ", "ゴ", "おん-", "お-", "み-"]),
            new Kanji(new Rune('姉'), ["シ", "あね", "はは", "ねえ"]),
        ]);
        TestFurigana("御姉さん", "おねえさん", "0:お;1:ねえ", resourceSet);
    }

    [TestMethod]
    public void TestFuriganaHakabakashii()
    {
        // Rendaku is applied to the second instance of 捗.
        var resourceSet = new FuriganaResourceSet
        ([
            new Kanji(new Rune('捗'), ["チョク", "ホ", "はかど.る", "はか"])
        ]);
        TestFurigana("捗捗しい", "はかばかしい", "0:はか;1:ばか", resourceSet);
        TestFurigana("捗々しい", "はかばかしい", "0:はか;1:ばか", resourceSet);
    }

    [Ignore]
    [TestMethod]
    public void TestFuriganaIssue5()
    {
        var resourceSet = new FuriganaResourceSet();
        var testData = new[]
        {
            ("御兄さん", "おにいさん", "0:お;1:にい"),
            ("御姉さん", "おねえさん", "0:お;1:ねえ"),
            ("御母さん", "おかあさん", "0:お;1:かあ"),
            ("抑抑", "そもそも", "0:そも;1:そも"),
            ("犇犇", "ひしひし", "0:ひし;1:ひし"),
            ("険しい路", "けわしいみち", "0:けわ;3:みち"),
            ("芝生", "しばふ", "0-1:しばふ"),
            ("純日本風", "じゅんにほんふう", "0:じゅん;1-2:にほん;3:ふう"),
            ("真珠湾", "しんじゅわん", "0:しん;1:じゅ;2:わん"),
            ("草履", "ぞうり", "0-1:ぞうり"),
            ("大和魂", "やまとだましい", "0-1:やまと;2:だましい"),
            ("竹刀", "しない", "0-1:しない"),
            ("東京湾", "とうきょうわん", "0:とう;1:きょう;2:わん"),
            ("日本学者", "にほんがくしゃ", "0-1:にほん;2:がく;3:しゃ"),
            ("日本製", "にほんせい", "0-1:にほん;2:せい"),
            ("日本側", "にほんがわ", "0-1:にほん;2:がわ"),
            ("日本刀", "にほんとう", "0-1:にほん;2:とう"),
            ("日本風", "にほんふう", "0-1:にほん;2:ふう"),
            ("木ノ葉", "このは", "0:こ;2:は"),
            ("木ノ葉", "きのは", "0:き;2:は"),
            ("余所見", "よそみ", "0:よ;1:そ;2:み"),
            ("嗹", "れん", "0:れん"),
            ("愈愈", "いよいよ", "0:いよ;1:いよ"),
            ("偶偶", "たまたま", "0:たま;1:たま"),
            ("益益", "ますます", "0:ます;1:ます"),
            ("風邪薬", "かぜぐすり", "0-1:かぜ;2:ぐすり"),
            ("日独協会", "にちどくきょうかい", "0:にち;1:どく;2:きょう;3:かい"),
        };
        foreach (var x in testData)
        {
            TestFurigana(x.Item1, x.Item2, x.Item3, resourceSet);
        }
    }

    private static void TestFurigana(string kanjiForm, string reading, string expectedFurigana, FuriganaResourceSet resourceSet)
    {
        var v = new VocabEntry(kanjiForm, reading);
        var business = new FuriganaBusiness(resourceSet);
        var result = business.Solve(v);
        var solution = result.GetSingleSolution();
        Assert.IsNotNull(solution);

        var expectedSolution = FuriganaSolutionParser.Parse(expectedFurigana, v);
        Assert.AreEqual(expectedSolution, solution);
    }

    [TestMethod]
    public void TestBreakIntoPartsAkagaeruka()
    {
        var vocab = new VocabEntry("アカガエル科", "アカガエルか");
        var solution = new FuriganaSolution(vocab, new FuriganaPart("か", 5));

        var parts = solution.BreakIntoParts().ToList();

        Assert.HasCount(2, parts);
        Assert.AreEqual("アカガエル", parts[0].Text);
        Assert.IsNull(parts[0].Furigana);
        Assert.AreEqual("科", parts[1].Text);
        Assert.AreEqual("か", parts[1].Furigana);
    }

    [TestMethod]
    public void TestBreakIntoPartsOtonagai()
    {
        var vocab = new VocabEntry("大人買い", "おとながい");
        var solution = new FuriganaSolution(vocab, new FuriganaPart("おとな", 0, 1), new FuriganaPart("が", 2));

        var parts = solution.BreakIntoParts().ToList();

        Assert.HasCount(3, parts);
        Assert.AreEqual("大人", parts[0].Text);
        Assert.AreEqual("おとな", parts[0].Furigana);
        Assert.AreEqual("買", parts[1].Text);
        Assert.AreEqual("が", parts[1].Furigana);
        Assert.AreEqual("い", parts[2].Text);
        Assert.IsNull(parts[2].Furigana);
    }

    [TestMethod]
    public void TestBreakIntoPartsHakabakashii()
    {
        var vocab = new VocabEntry("捗々しい", "はかばかしい");
        var solution = new FuriganaSolution(vocab, new FuriganaPart("はか", 0), new FuriganaPart("ばか", 1));

        var parts = solution.BreakIntoParts().ToList();

        Assert.HasCount(3, parts);
        Assert.AreEqual("捗", parts[0].Text);
        Assert.AreEqual("はか", parts[0].Furigana);
        Assert.AreEqual("々", parts[1].Text);
        Assert.AreEqual("ばか", parts[1].Furigana);
        Assert.AreEqual("しい", parts[2].Text);
        Assert.IsNull(parts[2].Furigana);
    }
}
