/*
Copyright (c) 2015-2017 Doublevil
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
        var service = new Service(MakeResourceSet(
        new()
        {
            ["一"] = ["イチ", "イツ", "ひと-", "ひと.つ"],
            ["ヶ"] = ["ヶ", "か", "が"],
            ["ヵ"] = ["ヵ", "か", "が"],
            ["ケ"] = ["ケ", "か", "が"],
            ["月"] = ["ゲツ", "ガツ", "つき"],
        }));
        TestFurigana("一ヶ月", "いっかげつ", "0:いっ;1:か;2:げつ", service);
        TestFurigana("一ヵ月", "いっかげつ", "0:いっ;1:か;2:げつ", service);
        TestFurigana("一ケ月", "いっかげつ", "0:いっ;1:か;2:げつ", service);

        TestFurigana("一ケ月", "いっけげつ", "0:いっ;2:げつ", service);
        TestFurigana("一ケ月", "いっケげつ", "0:いっ;2:げつ", service);
    }

    [TestMethod]
    public void TestNameKanji()
    {
        var service = new Service(new ResourceSet(
            kanji: [
                new VocabKanji(new Rune('佐'), ["あ"]),
                new VocabKanji(new Rune('藤'), ["あ"]),
                new NameKanji(new Rune('佐'), ["あ", "さ"]),
                new NameKanji(new Rune('藤'), ["あ", "とう"]),
            ],
            specialExpressions: []
        ));

        var vocab = new VocabEntry("佐藤", "さとう");
        var vocabSolution = service.Solve(vocab);
        Assert.IsNull(vocabSolution);

        var name = new NameEntry("佐藤", "さとう");
        var nameSolution = service.Solve(name);
        Assert.IsNotNull(nameSolution);

        var expectedSolution = Parser.Index("0:さ;1:とう", name);
        CollectionAssert.AreEqual(expectedSolution.Parts, nameSolution.Parts);
    }

    [TestMethod]
    public void TestFurigana頑張る()
    {
        var service = new Service(MakeResourceSet(new()
        {
            ["頑"] = ["ガン", "かたく.な"],
            ["張"] = ["チョウ", "は.る", "-は.り", "-ば.り"],
        }));
        TestFurigana("頑張る", "がんばる", "0:がん;1:ば", service);
    }

    [TestMethod]
    public void TestFurigana御坊っちゃん()
    {
        var service = new Service(MakeResourceSet(new()
        {
            ["御"] = ["ギョ", "ゴ", "おん-", "お-", "み-"],
            ["坊"] = ["ボウ", "ボッ"],
        }));
        TestFurigana("御坊っちゃん", "おぼっちゃん", "0:お;1:ぼ", service);
    }

    [TestMethod]
    public void TestFuriganaあら()
    {
        // This kanji is represented by a UTF-16 "Surrogate Pair."
        // The string has Length == 2.
        var service = new Service(MakeResourceSet(new()
        {
            ["𩺊"] = []
        }));
        TestFurigana("𩺊", "あら", "0:あら", service);
    }

    [TestMethod]
    public void TestFurigana弄り回す()
    {
        var service = new Service(MakeResourceSet(new()
        {
            ["弄"] = ["ロウ", "ル", "いじく.る", "ろう.する", "いじ.る", "ひねく.る", "たわむ.れる", "もてあそ.ぶ"],
            ["回"] = ["カイ", "エ", "まわ.る", "-まわ.る", "-まわ.り", "まわ.す", "-まわ.す", "まわ.し-", "-まわ.し", "もとお.る", "か.える"],
        }));
        TestFurigana("弄り回す", "いじりまわす", "0:いじ;2:まわ", service);
    }

    [TestMethod]
    public void TestFurigana掻っ攫う()
    {
        var service = new Service(MakeResourceSet(new()
        {
            ["掻"] = ["ソウ", "か.く"],
            ["攫"] = ["カク", "さら.う", "つか.む"],
        }));
        TestFurigana("掻っ攫う", "かっさらう", "0:か;2:さら", service);
    }

    [TestMethod]
    public void TestFurigana御姉さん()
    {
        var service = new Service(MakeResourceSet(new()
        {
            ["御"] = ["ギョ", "ゴ", "おん-", "お-", "み-"],
            ["姉"] = ["シ", "あね", "はは", "ねえ"],
        }));
        TestFurigana("御姉さん", "おねえさん", "0:お;1:ねえ", service);
    }

    [TestMethod]
    public void TestFurigana捗捗しい()
    {
        // Rendaku is applied to the second instance of 捗.
        var service = new Service(MakeResourceSet(new()
        {
            ["捗"] = ["チョク", "ホ", "はかど.る", "はか"],
        }));
        TestFurigana("捗捗しい", "はかばかしい", "0:はか;1:ばか", service);
        TestFurigana("捗々しい", "はかばかしい", "0:はか;1:ばか", service);
    }

    [TestMethod]
    public void TestFuriganaIssue5()
    {
        // These kanji readings are all in kanjidic2 except for
        // 兄・にい, 姉・ねえ, and 母・かあ.
        var service = new Service(MakeResourceSet(
        new()
        {
            ["御"] = ["ギョ", "ゴ", "おん-", "お-", "み-"],
            ["兄"] = ["ケイ", "キョウ", "あに", "にい"],
            ["姉"] = ["シ", "あね", "はは", "ねえ"],
            ["母"] = ["ボ", "はは", "も", "かあ"],
            ["抑"] = ["ヨク", "おさ.える"],
            ["犇"] = ["ホン", "ひし.めく", "ひしひし", "はし.る"],
            ["険"] = ["ケン", "けわ.しい"],
            ["路"] = ["ロ", "ル", "-じ", "みち"],
            ["芝"] = ["シ", "しば"],
            ["生"] = ["セイ", "ショウ", "い.きる", "い.かす", "い.ける", "う.まれる", "うま.れる", "う.まれ", "うまれ", "う.む", "お.う", "は.える", "は.やす", "き", "なま", "なま-", "な.る", "な.す", "む.す", "-う"],
            ["純"] = ["ジュン"],
            ["日"] = ["ニチ", "ジツ", "ひ", "-び", "-か"],
            ["本"] = ["ホン", "もと"],
            ["風"] = ["フウ", "フ", "かぜ", "かざ-"],
            ["真"] = ["シン", "ま", "ま-", "まこと"],
            ["珠"] = ["シュ", "たま"],
            ["湾"] = ["ワン", "いりえ"],
            ["草"] = ["ソウ", "くさ", "くさ-", "-ぐさ"],
            ["履"] = ["リ", "は.く"],
            ["大"] = ["ダイ", "タイ", "おお-", "おお.きい", "-おお.いに"],
            ["和"] = ["ワ", "オ", "カ", "やわ.らぐ", "やわ.らげる", "なご.む", "なご.やか", "あ.える"],
            ["魂"] = ["コン", "たましい", "たま"],
            ["竹"] = ["チク", "たけ"],
            ["刀"] = ["トウ", "かたな", "そり"],
            ["東"] = ["トウ", "ひがし"],
            ["京"] = ["キョウ", "ケイ", "キン", "みやこ"],
            ["学"] = ["ガク", "まな.ぶ"],
            ["者"] = ["シャ", "もの"],
            ["製"] = ["セイ"],
            ["側"] = ["ソク", "かわ", "がわ", "そば"],
            ["木"] = ["ボク", "モク", "き", "こ-"],
            ["葉"] = ["ヨウ", "は"],
            ["余"] = ["ヨ", "あま.る", "あま.り", "あま.す", "あんま.り"],
            ["所"] = ["ショ", "ところ", "-ところ", "どころ", "とこ"],
            ["見"] = ["ケン", "み.る", "み.える", "み.せる"],
            ["嗹"] = ["レン", "おしゃべり"],
            ["愈"] = ["ユ", "いよいよ", "まさ.る"],
            ["偶"] = ["グウ", "たま"],
            ["益"] = ["エキ", "ヤク", "ま.す"],
            ["邪"] = ["ジャ", "よこし.ま"],
            ["薬"] = ["ヤク", "くすり"],
            ["独"] = ["ドク", "トク", "ひと.り"],
            ["協"] = ["キョウ"],
            ["会"] = ["カイ", "エ", "あ.う", "あ.わせる", "あつ.まる"],
            ["人"] = ["ジン", "ニン", "ひと", "-り", "-と"],
        },
        new()
        {
            ["芝生"] = ["しばふ"],
            ["草履"] = ["ぞうり"],
            ["日本"] = ["にほん"],
            ["大和"] = ["やまと"],
            ["竹刀"] = ["しない"],
            ["風邪"] = ["かぜ"],
            ["大人"] = ["おとな"],
        }));
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
            ("大人の人", "おとなのひと", "0-1:おとな;3:ひと"),
        };
        foreach (var (kanjiForm, reading, expectedFurigana) in testData)
        {
            TestFurigana(kanjiForm, reading, expectedFurigana, service);
        }
    }

    private static void TestFurigana(string kanjiForm, string reading, string expectedFurigana, Service service)
    {
        var vocab = new VocabEntry(kanjiForm, reading);
        var solution = service.Solve(vocab);
        Assert.IsNotNull(solution);

        var expectedSolution = Parser.Index(expectedFurigana, vocab);
        CollectionAssert.AreEqual(expectedSolution.Parts, solution.Parts);
    }

    public static ResourceSet MakeResourceSet(Dictionary<string, List<string>> kanjiInfo)
    {
        return MakeResourceSet(kanjiInfo, []);
    }

    public static ResourceSet MakeResourceSet(Dictionary<string, List<string>> kanjiInfo, Dictionary<string, List<string>> specialExpressionInfo)
    {
        return new ResourceSet
        (
            kanji: kanjiInfo.Select(x => new VocabKanji(x.Key.EnumerateRunes().First(), x.Value)),
            specialExpressions: specialExpressionInfo.Select(x => new SpecialExpression(x.Key, x.Value))
        );
    }
}
