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
using Jitendex.Furigana.InputModels;

namespace Jitendex.Furigana.Test;

[TestClass]
public class ServiceTest
{
    [TestMethod]
    public void TestFuriganaIkkagetsu()
    {
        var resourceSet = new ResourceSet
        ([
            new Kanji(new Rune('一'), ["イチ", "イツ", "ひと-", "ひと.つ"]),
            new Kanji(new Rune('月'), ["ゲツ", "ガツ", "つき"]),
        ],
        [
            new SpecialExpression("ヶ", ["か", "が"]),
            new SpecialExpression("ヵ", ["か", "が"]),
            new SpecialExpression("ケ", ["か", "が"]),
        ]);
        var service = new Service(resourceSet);
        TestFurigana("一ヶ月", "いっかげつ", "0:いっ;1:か;2:げつ", service);
        TestFurigana("一ヵ月", "いっかげつ", "0:いっ;1:か;2:げつ", service);
        TestFurigana("一ケ月", "いっかげつ", "0:いっ;1:か;2:げつ", service);

        TestFurigana("一ケ月", "いっけげつ", "0:いっ;2:げつ", service);
        TestFurigana("一ケ月", "いっケげつ", "0:いっ;2:げつ", service);
    }

    [TestMethod]
    public void TestFuriganaGanbaru()
    {
        // Readings cannot begin with 'ん', so there is 1 possible solution.
        // No need to supply any character readings.
        var resourceSet = new ResourceSet();
        var service = new Service(resourceSet);
        TestFurigana("頑張る", "がんばる", "0:がん;1:ば", service);
    }

    [TestMethod]
    public void TestFuriganaObocchan()
    {
        var resourceSet = new ResourceSet();
        var service = new Service(resourceSet);
        TestFurigana("御坊っちゃん", "おぼっちゃん", "0:お;1:ぼ", service);
    }

    [TestMethod]
    public void TestFuriganaAra()
    {
        // This kanji is represented by a UTF-16 "Surrogate Pair."
        // The string has Length == 2.
        var resourceSet = new ResourceSet();
        var service = new Service(resourceSet);
        TestFurigana("𩺊", "あら", "0:あら", service);
    }

    [TestMethod]
    public void TestFuriganaIjirimawasu()
    {
        // 1 possible solution. No need to supply any character readings.
        var resourceSet = new ResourceSet();
        var service = new Service(resourceSet);
        TestFurigana("弄り回す", "いじりまわす", "0:いじ;2:まわ", service);
    }

    [TestMethod]
    public void TestFuriganaKassarau()
    {
        // 1 possible solution. No need to supply any character readings.
        var resourceSet = new ResourceSet();
        var service = new Service(resourceSet);
        TestFurigana("掻っ攫う", "かっさらう", "0:か;2:さら", service);
    }

    [TestMethod]
    public void TestFuriganaOneesan()
    {
        var resourceSet = new ResourceSet
        ([
            new Kanji(new Rune('御'), ["ギョ", "ゴ", "おん-", "お-", "み-"]),
            new Kanji(new Rune('姉'), ["シ", "あね", "はは", "ねえ"]),
        ]);
        var service = new Service(resourceSet);
        TestFurigana("御姉さん", "おねえさん", "0:お;1:ねえ", service);
    }

    [TestMethod]
    public void TestFuriganaHakabakashii()
    {
        // Rendaku is applied to the second instance of 捗.
        var resourceSet = new ResourceSet
        ([
            new Kanji(new Rune('捗'), ["チョク", "ホ", "はかど.る", "はか"])
        ]);
        var service = new Service(resourceSet);
        TestFurigana("捗捗しい", "はかばかしい", "0:はか;1:ばか", service);
        TestFurigana("捗々しい", "はかばかしい", "0:はか;1:ばか", service);
    }

    [TestMethod]
    public void TestFuriganaIssue5()
    {
        // These kanji readings are all in kanjidic2 except for
        // 兄・にい, 姉・ねえ, and 母・かあ.
        var resourceSet = new ResourceSet
        ([
            new Kanji(new Rune('御'), ["ギョ", "ゴ", "おん-", "お-", "み-"]),
            new Kanji(new Rune('兄'), ["ケイ", "キョウ", "あに", "にい"]),
            new Kanji(new Rune('姉'), ["シ", "あね", "はは", "ねえ"]),
            new Kanji(new Rune('母'), ["ボ", "はは", "も", "かあ"]),
            new Kanji(new Rune('抑'), ["ヨク", "おさ.える"]),
            new Kanji(new Rune('犇'), ["ホン", "ひし.めく", "ひしひし", "はし.る"]),
            new Kanji(new Rune('険'), ["ケン", "けわ.しい"]),
            new Kanji(new Rune('路'), ["ロ", "ル", "-じ", "みち"]),
            new Kanji(new Rune('芝'), ["シ", "しば"]),
            new Kanji(new Rune('生'), ["セイ", "ショウ", "い.きる", "い.かす", "い.ける", "う.まれる", "うま.れる", "う.まれ", "うまれ", "う.む", "お.う", "は.える", "は.やす", "き", "なま", "なま-", "な.る", "な.す", "む.す", "-う"]),
            new Kanji(new Rune('純'), ["ジュン"]),
            new Kanji(new Rune('日'), ["ニチ", "ジツ", "ひ", "-び", "-か"]),
            new Kanji(new Rune('本'), ["ホン", "もと"]),
            new Kanji(new Rune('風'), ["フウ", "フ", "かぜ", "かざ-"]),
            new Kanji(new Rune('真'), ["シン", "ま", "ま-", "まこと"]),
            new Kanji(new Rune('珠'), ["シュ", "たま"]),
            new Kanji(new Rune('湾'), ["ワン", "いりえ"]),
            new Kanji(new Rune('草'), ["ソウ", "くさ", "くさ-", "-ぐさ"]),
            new Kanji(new Rune('履'), ["リ", "は.く"]),
            new Kanji(new Rune('大'), ["ダイ", "タイ", "おお-", "おお.きい", "-おお.いに"]),
            new Kanji(new Rune('和'), ["ワ", "オ", "カ", "やわ.らぐ", "やわ.らげる", "なご.む", "なご.やか", "あ.える"]),
            new Kanji(new Rune('魂'), ["コン", "たましい", "たま"]),
            new Kanji(new Rune('竹'), ["チク", "たけ"]),
            new Kanji(new Rune('刀'), ["トウ", "かたな", "そり"]),
            new Kanji(new Rune('東'), ["トウ", "ひがし"]),
            new Kanji(new Rune('京'), ["キョウ", "ケイ", "キン", "みやこ"]),
            new Kanji(new Rune('学'), ["ガク", "まな.ぶ"]),
            new Kanji(new Rune('者'), ["シャ", "もの"]),
            new Kanji(new Rune('製'), ["セイ"]),
            new Kanji(new Rune('側'), ["ソク", "かわ", "がわ", "そば"]),
            new Kanji(new Rune('木'), ["ボク", "モク", "き", "こ-"]),
            new Kanji(new Rune('葉'), ["ヨウ", "は"]),
            new Kanji(new Rune('余'), ["ヨ", "あま.る", "あま.り", "あま.す", "あんま.り"]),
            new Kanji(new Rune('所'), ["ショ", "ところ", "-ところ", "どころ", "とこ"]),
            new Kanji(new Rune('見'), ["ケン", "み.る", "み.える", "み.せる"]),
            new Kanji(new Rune('嗹'), ["レン", "おしゃべり"]),
            new Kanji(new Rune('愈'), ["ユ", "いよいよ", "まさ.る"]),
            new Kanji(new Rune('偶'), ["グウ", "たま"]),
            new Kanji(new Rune('益'), ["エキ", "ヤク", "ま.す"]),
            new Kanji(new Rune('邪'), ["ジャ", "よこし.ま"]),
            new Kanji(new Rune('薬'), ["ヤク", "くすり"]),
            new Kanji(new Rune('独'), ["ドク", "トク", "ひと.り"]),
            new Kanji(new Rune('協'), ["キョウ"]),
            new Kanji(new Rune('会'), ["カイ", "エ", "あ.う", "あ.わせる", "あつ.まる"]),
        ],
        [
            new SpecialExpression("芝生", ["しばふ"]),
            new SpecialExpression("草履", ["ぞうり"]),
            new SpecialExpression("日本", ["にほん"]),
            new SpecialExpression("大和", ["やまと"]),
            new SpecialExpression("竹刀", ["しない"]),
            new SpecialExpression("風邪", ["かぜ"]),
        ]);
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
        var service = new Service(resourceSet);
        foreach (var (kanjiForm, reading, expectedFurigana) in testData)
        {
            TestFurigana(kanjiForm, reading, expectedFurigana, service);
        }
    }

    private static void TestFurigana(string kanjiForm, string reading, string expectedFurigana, Service service)
    {
        var v = new VocabEntry(kanjiForm, reading);
        var solution = service.Solve(v);
        Assert.IsNotNull(solution);

        var expectedSolution = FuriganaSolutionParser.Parse(expectedFurigana, v);
        CollectionAssert.AreEqual(expectedSolution.Parts, solution.Parts);
    }
}
