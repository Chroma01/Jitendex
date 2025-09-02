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

using Jitendex.Furigana.Business;
using Jitendex.Furigana.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jitendex.Furigana.Test;

[TestClass]
public class FuriganaTest
{
    private static readonly FuriganaResourceSet IkkagetsuResourceSet = new
    (
        new()
        {
            ['一'] = new Kanji { Character = '一', Readings = [] },
        },
        [],
        new()
        {
            ["ヶ月"] = new SpecialExpression
            {
                KanjiReading = "ヶ月",
                Readings =
                [
                    new SpecialReading
                    (
                        "かげつ",
                        new FuriganaSolution
                        (
                            new VocabEntry { KanjiReading = "ヶ月", KanaReading = "かげつ"},
                            new List<FuriganaPart>() { new("か", 0), new("げつ", 1) }
                        )
                    )
                ]
            },
        }
    );

    [TestMethod]
    public void TestFuriganaGanbaru()
    {
        var resourceSet = new FuriganaResourceSet(new()
        {
            ['頑'] = new Kanji { Character = '頑', Readings = [] },
            ['張'] = new Kanji { Character = '張', Readings = [] },
        }, [], []);
        // Readings cannot begin with 'ん', so there is 1 possible solution.
        // No need to supply any character readings.
        TestFurigana("頑張る", "がんばる", "0:がん;1:ば", resourceSet);
    }

    [TestMethod]
    public void TestFuriganaIkkagetsu()
    {
        TestFurigana("一ヶ月", "いっかげつ", "0:いっ;1:か;2:げつ", IkkagetsuResourceSet);
    }

    [TestMethod]
    public void TestFuriganaObocchan()
    {
        // TestFurigana("御坊っちゃん", "おぼっちゃん", "0:お;1:ぼ");
    }

    [TestMethod]
    [Ignore]
    public void TestFuriganaAra()
    {
        // Will fail. This is a weird kanji. The string containing only the kanji is Length == 2.
        // Would be cool to find a solution but don't worry too much about it.
        // TestFurigana("𩺊", "あら", "0:あら");
    }

    [TestMethod]
    public void TestFuriganaIjirimawasu()
    {
        var resourceSet = new FuriganaResourceSet(new()
        {
            ['弄'] = new Kanji { Character = '弄', Readings = [] },
            ['回'] = new Kanji { Character = '回', Readings = [] },
        }, [], []);
        // 1 possible solution. No need to supply any character readings.
        TestFurigana("弄り回す", "いじりまわす", "0:いじ;2:まわ", resourceSet);
    }

    [TestMethod]
    public void TestFuriganaKassarau()
    {
        var resourceSet = new FuriganaResourceSet(new()
        {
            ['掻'] = new Kanji { Character = '掻', Readings = [] },
            ['攫'] = new Kanji { Character = '攫', Readings = [] },
        }, [], []);
        // 1 possible solution. No need to supply any character readings.
        TestFurigana("掻っ攫う", "かっさらう", "0:か;2:さら", resourceSet);
    }

    [TestMethod]
    public void TestFuriganaOneesan()
    {
        // TestFurigana("御姉さん", "おねえさん", "0:お;1:ねえ");
    }

    [TestMethod]
    public void TestFuriganaHakabakashii()
    {
        var resourceSet = new FuriganaResourceSet(new()
        { 
            ['捗'] = new Kanji { Character = '捗', Readings = ["はか"] }
        }, [], []);
        // Rendaku is applied to the second instance of 捗.
        TestFurigana("捗捗しい", "はかばかしい", "0:はか;1:ばか", resourceSet);
        TestFurigana("捗々しい", "はかばかしい", "0:はか;1:ばか", resourceSet);
    }

    [TestMethod]
    public void TestFuriganaIssue5()
    {
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
            // TestFurigana(x.Item1, x.Item2, x.Item3);
        }
    }

    private static void TestFurigana(string kanjiForm, string reading, string expectedFurigana, FuriganaResourceSet resourceSet)
    {
        var v = new VocabEntry(kanjiForm, reading);
        var business = new FuriganaBusiness(resourceSet);
        var result = business.Execute(v);
        var solution = result.GetSingleSolution();
        Assert.IsNotNull(solution);

        var expectedSolution = FuriganaSolution.Parse(expectedFurigana, v);
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
}
