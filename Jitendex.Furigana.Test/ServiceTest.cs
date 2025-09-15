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
using Jitendex.Furigana.Models;

namespace Jitendex.Furigana.Test;

[TestClass]
public class ServiceTest
{
    [TestMethod]
    public void TestFuriganaIkkagetsu()
    {
        var service = new Service(MakeReadingCache(
        new()
        {
            ["一"] = ["イチ", "イツ", "ひと-", "ひと.つ"],
            ["ヶ"] = ["ヶ", "か", "が"],
            ["ヵ"] = ["ヵ", "か", "が"],
            ["ケ"] = ["ケ", "か", "が"],
            ["月"] = ["ゲツ", "ガツ", "つき"],
        }));
        TestFurigana("一ヶ月", "いっかげつ", "[一|いっ][ヶ|か][月|げつ]", service);
        TestFurigana("一ヵ月", "いっかげつ", "[一|いっ][ヵ|か][月|げつ]", service);
        TestFurigana("一ケ月", "いっかげつ", "[一|いっ][ケ|か][月|げつ]", service);

        TestFurigana("一ケ月", "いっけげつ", "[一|いっ]ケ[月|げつ]", service);
        TestFurigana("一ケ月", "いっケげつ", "[一|いっ]ケ[月|げつ]", service);
    }

    [TestMethod]
    public void TestNameKanji()
    {
        var service = new Service(new ReadingCache(
            kanji: [
                new VocabKanji(new Rune('佐'), ["あ"]),
                new VocabKanji(new Rune('藤'), ["あ"]),
                new NameKanji(new Rune('佐'), ["あ", "さ"]),
                new NameKanji(new Rune('藤'), ["あ", "とう"]),
            ],
            specialExpressions: []
        ));

        // Cannot solve it as a Vocab Entry.
        var vocab = new VocabEntry("佐藤", "さとう");
        var vocabSolution = service.Solve(vocab);
        Assert.IsNull(vocabSolution);

        // Can solve it as a Name Entry.
        var name = new NameEntry("佐藤", "さとう");
        var nameSolution = service.Solve(name);
        Assert.IsNotNull(nameSolution);

        var expectedSolution = Parser.Solution("[佐|さ][藤|とう]", name);
        CollectionAssert.AreEqual(expectedSolution.Parts, nameSolution.Parts);
    }

    [TestMethod]
    public void TestFurigana頑張る()
    {
        var service = new Service(MakeReadingCache(new()
        {
            ["頑"] = ["ガン", "かたく.な"],
            ["張"] = ["チョウ", "は.る", "-は.り", "-ば.り"],
        }));
        TestFurigana("頑張る", "がんばる", "[頑|がん][張|ば]る", service);
    }

    [TestMethod]
    public void TestFurigana御坊っちゃん()
    {
        var service = new Service(MakeReadingCache(new()
        {
            ["御"] = ["ギョ", "ゴ", "おん-", "お-", "み-"],
            ["坊"] = ["ボウ", "ボッ"],
        }));
        TestFurigana("御坊っちゃん", "おぼっちゃん", "[御|お][坊|ぼ]っちゃん", service);
    }

    [TestMethod]
    public void TestFuriganaあら()
    {
        // This kanji is represented by a UTF-16 "Surrogate Pair."
        // The string has Length == 2.
        var service = new Service(MakeReadingCache([]));
        TestFurigana("𩺊", "あら", "[𩺊|あら]", service);
    }

    [TestMethod]
    public void TestFurigana弄り回す()
    {
        var service = new Service(MakeReadingCache(new()
        {
            ["弄"] = ["ロウ", "ル", "いじく.る", "ろう.する", "いじ.る", "ひねく.る", "たわむ.れる", "もてあそ.ぶ"],
            ["回"] = ["カイ", "エ", "まわ.る", "-まわ.る", "-まわ.り", "まわ.す", "-まわ.す", "まわ.し-", "-まわ.し", "もとお.る", "か.える"],
        }));
        TestFurigana("弄り回す", "いじりまわす", "[弄|いじ]り[回|まわ]す", service);
    }

    [TestMethod]
    public void TestFurigana掻っ攫う()
    {
        var service = new Service(MakeReadingCache(new()
        {
            ["掻"] = ["ソウ", "か.く"],
            ["攫"] = ["カク", "さら.う", "つか.む"],
        }));
        TestFurigana("掻っ攫う", "かっさらう", "[掻|か]っ[攫|さら]う", service);
    }

    [TestMethod]
    public void TestFurigana御姉さん()
    {
        var service = new Service(MakeReadingCache(new()
        {
            ["御"] = ["ギョ", "ゴ", "おん-", "お-", "み-"],
            ["姉"] = ["シ", "あね", "はは", "ねえ"],
        }));
        TestFurigana("御姉さん", "おねえさん", "[御|お][姉|ねえ]さん", service);
    }

    [TestMethod]
    public void TestFurigana捗捗しい()
    {
        // Rendaku is applied to the second instance of 捗.
        var service = new Service(MakeReadingCache(new()
        {
            ["捗"] = ["チョク", "ホ", "はかど.る", "はか"],
        }));
        TestFurigana("捗捗しい", "はかばかしい", "[捗|はか][捗|ばか]しい", service);
        TestFurigana("捗々しい", "はかばかしい", "[捗|はか][々|ばか]しい", service);
    }

    [TestMethod]
    public void TestFuriganaIssue5()
    {
        // These kanji readings are all in kanjidic2 except for
        // 兄・にい, 姉・ねえ, and 母・かあ.
        var service = new Service(MakeReadingCache(
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
            ("御兄さん", "おにいさん", "[御|お][兄|にい]さん"),
            ("御姉さん", "おねえさん", "[御|お][姉|ねえ]さん"),
            ("御母さん", "おかあさん", "[御|お][母|かあ]さん"),
            ("抑抑", "そもそも", "[抑|そも][抑|そも]"),
            ("犇犇", "ひしひし", "[犇|ひし][犇|ひし]"),
            ("険しい路", "けわしいみち", "[険|けわ]しい[路|みち]"),
            ("芝生", "しばふ", "[芝生|しばふ]"),
            ("純日本風", "じゅんにほんふう", "[純|じゅん][日本|にほん][風|ふう]"),
            ("真珠湾", "しんじゅわん", "[真|しん][珠|じゅ][湾|わん]"),
            ("草履", "ぞうり", "[草履|ぞうり]"),
            ("大和魂", "やまとだましい", "[大和|やまと][魂|だましい]"),
            ("竹刀", "しない", "[竹刀|しない]"),
            ("東京湾", "とうきょうわん", "[東|とう][京|きょう][湾|わん]"),
            ("日本学者", "にほんがくしゃ", "[日本|にほん][学|がく][者|しゃ]"),
            ("日本製", "にほんせい", "[日本|にほん][製|せい]"),
            ("日本側", "にほんがわ", "[日本|にほん][側|がわ]"),
            ("日本刀", "にほんとう", "[日本|にほん][刀|とう]"),
            ("日本風", "にほんふう", "[日本|にほん][風|ふう]"),
            ("木ノ葉", "このは", "[木|こ]ノ[葉|は]"),
            ("木ノ葉", "きのは", "[木|き]ノ[葉|は]"),
            ("余所見", "よそみ", "[余|よ][所|そ][見|み]"),
            ("嗹", "れん", "[嗹|れん]"),
            ("愈愈", "いよいよ", "[愈|いよ][愈|いよ]"),
            ("偶偶", "たまたま", "[偶|たま][偶|たま]"),
            ("益益", "ますます", "[益|ます][益|ます]"),
            ("風邪薬", "かぜぐすり", "[風邪|かぜ][薬|ぐすり]"),
            ("日独協会", "にちどくきょうかい", "[日|にち][独|どく][協|きょう][会|かい]"),
            ("大人の人", "おとなのひと", "[大人|おとな]の[人|ひと]"),
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

        var expectedSolution = Parser.Solution(expectedFurigana, vocab);
        CollectionAssert.AreEqual(expectedSolution.Parts, solution.Parts);
    }

    public static ReadingCache MakeReadingCache(Dictionary<string, List<string>> kanjiInfo)
    {
        return MakeReadingCache(kanjiInfo, []);
    }

    public static ReadingCache MakeReadingCache(Dictionary<string, List<string>> kanjiInfo, Dictionary<string, List<string>> specialExpressionInfo)
    {
        return new ReadingCache
        (
            kanji: kanjiInfo.Select(x => new VocabKanji(x.Key.EnumerateRunes().First(), x.Value)),
            specialExpressions: specialExpressionInfo.Select(x => new SpecialExpression(x.Key, x.Value))
        );
    }
}
