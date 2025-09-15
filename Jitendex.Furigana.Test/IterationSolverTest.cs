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

namespace Jitendex.Furigana.Test;

[TestClass]
public class IterationSolverTest
{
    private static readonly IterationSolver _resourcelessSolver = new([], []);

    [TestMethod]
    public void TestAllKana()
    {
        var data = new List<(string, string, string)>()
        {
            ("あ", "あ", "あ"),
            ("ア", "あ", "ア"),
            ("あいうえお", "あいうえお", "あいうえお"),
            ("アイウエオ", "あいうえお", "アイウエオ"),
            ("あいうえお", "アイウエオ", "あいうえお"),
            ("あイうエお", "あいうえお", "あイうエお"),
        };

        TestSolutions(_resourcelessSolver, data);
    }

    [TestMethod]
    public void SingleNonKana()
    {
        var data = new List<(string, string, string)>()
        {
            // Single kanji
            ("腹", "はら", "[腹|はら]"),
            ("嗹", "れん", "[嗹|れん]"),

            // Single non-kanji
            ("◯", "おおきなまる", "[◯|おおきなまる]"),
            ("々", "とき", "[々|とき]"),

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
            ("嗹", "レン", "[嗹|レン]"),
            ("ありがたい事に", "ありがたいコトに", "ありがたい[事|コト]に"),
        };

        TestSolutions(_resourcelessSolver, data);
    }

    [TestMethod]
    public void SingleNonKanaWithUtf16SurrogatePair()
    {
        var data = new Dictionary<string, (string KanjiFormText, string ReadingText, string ExpectedResultText)>()
        {
            ["𩺊"] = ("𩺊", "あら", "[𩺊|あら]"),
            ["𠮟"] = ("𠮟かり", "しかり", "[𠮟|し]かり"),
            ["𤸎"] = ("しょう𤸎", "しょうかち", "しょう[𤸎|かち]")
        };

        foreach (var item in data)
        {
            var kanji = item.Key;
            Assert.AreEqual(2, kanji.Length);
            Assert.Contains(kanji, item.Value.KanjiFormText);
            Assert.Contains(kanji, item.Value.ExpectedResultText);
        }

        TestSolutions(_resourcelessSolver, data.Values);
    }

    [TestMethod]
    public void RepeatedKanji()
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

            // Three kana per kanji
            ("州州", "しゅうしゅう", "[州|しゅう][州|しゅう]"),
            ("州々", "しゅうしゅう", "[州|しゅう][々|しゅう]"),

            // With a different reading for each kanji
            ("日日", "ひび", "[日|ひ][日|び]"),
            ("日々", "ひび", "[日|ひ][々|び]"),
            ("時時", "ときどき", "[時|とき][時|どき]"),
            ("時々", "ときどき", "[時|とき][々|どき]"),

            // "𩺊" is represented by a UTF-16 "Surrogate Pair"
            // with string Length == 2.
            ("𩺊𩺊", "あらあら", "[𩺊|あら][𩺊|あら]"),
            ("𩺊々", "あらあら", "[𩺊|あら][々|あら]"),

            // Non-kanji
            ("々々", "ときどき", "[々|とき][々|どき]"),
            ("〇〇", "まるまる", "[〇|まる][〇|まる]"),

            // With bordering kana
            ("捗捗しい", "はかばかしい", "[捗|はか][捗|ばか]しい"),
            ("捗々しい", "はかばかしい", "[捗|はか][々|ばか]しい"),
            ("かなしい時時", "かなしいときどき", "かなしい[時|とき][時|どき]"),
            ("かなしい時々", "かなしいときどき", "かなしい[時|とき][々|どき]"),
            ("捗捗しい時々", "はかばかしいときどき", "[捗|はか][捗|ばか]しい[時|とき][々|どき]"),
            ("捗々しい時時", "はかばかしいときどき", "[捗|はか][々|ばか]しい[時|とき][時|どき]"),
        };

        TestSolutions(_resourcelessSolver, data);
    }

    [TestMethod]
    public void EqualLengthTexts()
    {
        var data = new List<(string, string, string)>()
        {
            ("木の葉", "このは", "[木|こ]の[葉|は]"),
            ("こ之は", "このは", "こ[之|の]は"),
            ("余所見", "よそみ", "[余|よ][所|そ][見|み]"),
            ("御坊っちゃん", "おぼっちゃん", "[御|お][坊|ぼ]っちゃん"),

            // Don't capture the impossible start characters (っ, ん, etc.) if not followed by a kanji
            ("真っさお", "まっさお", "[真|ま]っさお"),
            ("を呼んで", "をよんで", "を[呼|よ]んで"),
            ("田ん圃", "たんぼ", "[田|た]ん[圃|ぼ]"),

            // With non-normalized kanji forms
            ("木ノ葉", "このは", "[木|こ]ノ[葉|は]"),
            ("木ノ葉", "きのは", "[木|き]ノ[葉|は]"),
            ("真ッさオ", "まっさお", "[真|ま]ッさオ"),
            ("田ン圃", "たんぼ", "[田|た]ン[圃|ぼ]"),

            // With non-normalized readings
            ("木の葉", "コのは", "[木|コ]の[葉|は]"),
            ("余所見", "ヨそミ", "[余|ヨ][所|そ][見|ミ]"),
            ("真っサお", "マっさお", "[真|マ]っサお"),
        };

        foreach (var (kanjiFormText, readingText, _) in data)
        {
            Assert.AreEqual(kanjiFormText.Length, readingText.Length);
        }

        TestSolutions(_resourcelessSolver, data);
    }

    /// <summary>
    /// Tests for unique and correct solutions when the length of the
    /// kanji form text is unequal to the length of the reading text.
    /// </summary>
    /// <remarks>
    /// Since readings of individual kanji cannot begin with 'っ', 'ょ', 'ゃ', 'ゅ', or 'ん',
    /// we are able to solve these problems without any prior knowledge of usual kanji readings.
    /// </remarks>
    [TestMethod]
    public void TestImpossibleKanjiReadingStarts()
    {
        var data = new List<(string, string, string)>()
        {
            // っ
            ("仏者", "ぶっしゃ", "[仏|ぶっ][者|しゃ]"),
            ("ご法度", "ごはっと", "ご[法|はっ][度|と]"),

            // ょ
            ("如意", "にょい", "[如|にょ][意|い]"),
            ("真如", "しんにょ", "[真|しん][如|にょ]"),

            // ゃ
            ("他社", "たしゃ", "[他|た][社|しゃ]"),
            ("三社", "さんしゃ", "[三|さん][社|しゃ]"),
            ("三輪車", "さんりんしゃ", "[三|さん][輪|りん][車|しゃ]"),
            ("不審者", "ふしんしゃ", "[不|ふ][審|しん][者|しゃ]"),

            // ゅ
            ("亜種", "あしゅ", "[亜|あ][種|しゅ]"),
            ("別種", "べっしゅ", "[別|べっ][種|しゅ]"),
            ("三鞭酒", "さんべんしゅ", "[三|さん][鞭|べん][酒|しゅ]"),

            // ん
            ("如何", "いかん", "[如|い][何|かん]"),
            ("阿呆陀羅", "あほんだら", "[阿|あ][呆|ほん][陀|だ][羅|ら]"),
            ("頑張る", "がんばる", "[頑|がん][張|ば]る"),
            ("真珠湾", "しんじゅわん", "[真|しん][珠|じゅ][湾|わん]"),

            // With two consecutive impossible-start characters
            ("勅勘", "ちょっかん", "[勅|ちょっ][勘|かん]"),

            // With kana following the kanji
            ("真さお", "まっさお", "[真|まっ]さお"),
            ("危機に瀕する", "ききにひんする", "[危|き][機|き]に[瀕|ひん]する"),

            // With non-normalized readings
            ("阿呆陀羅", "アほンだラ", "[阿|ア][呆|ほン][陀|だ][羅|ラ]"),
        };

        foreach (var (kanjiFormText, readingText, _) in data)
        {
            Assert.AreNotEqual(kanjiFormText.Length, readingText.Length);
        }

        TestSolutions(_resourcelessSolver, data);
    }

    [TestMethod]
    public void TestKanaBorderedKanji()
    {
        var data = new List<(string, string, string)>()
        {
            ("真っ青", "まっさお", "[真|ま]っ[青|さお]"),
            ("桜ん坊", "さくらんぼ", "[桜|さくら]ん[坊|ぼ]"),
            ("桜ん坊", "さくらんぼう", "[桜|さくら]ん[坊|ぼう]"),
            ("持ち運ぶ", "もちはこぶ", "[持|も]ち[運|はこ]ぶ"),
            ("難しい", "むずかしい", "[難|むずか]しい"),
            ("弄り回す", "いじりまわす", "[弄|いじ]り[回|まわ]す"),
            ("掻っ攫う", "かっさらう", "[掻|か]っ[攫|さら]う"),
            ("険しい路", "けわしいみち", "[険|けわ]しい[路|みち]"),
            ("好き運ぶ嫌い", "すきはこぶきらい", "[好|す]き[運|はこ]ぶ[嫌|きら]い"),
        };

        foreach (var (kanjiFormText, readingText, _) in data)
        {
            Assert.AreNotEqual(kanjiFormText.Length, readingText.Length);
        }

        TestSolutions(_resourcelessSolver, data);
    }

    [TestMethod]
    public void RequiresCharacterInfo()
    {
        var kanji = VocabKanji(new()
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
        });

        var data = new List<(string, string, string)>()
        {
            ("御姉さん", "おねえさん", "[御|お][姉|ねえ]さん"),
            ("御母さん", "おかあさん", "[御|お][母|かあ]さん"),
            ("御兄さん", "おにいさん", "[御|お][兄|にい]さん"),
            ("東京湾", "とうきょうわん", "[東|とう][京|きょう][湾|わん]"),
            ("日独協会", "にちどくきょうかい", "[日|にち][独|どく][協|きょう][会|かい]"),
        };
        var solver = new IterationSolver(kanji, []);

        TestSolutions(solver, data);

        var resourcelessData = data.Select(static x => (x.Item1, x.Item2)).ToList();
        TestNullSolutions(_resourcelessSolver, resourcelessData);
    }

    [TestMethod]
    public void TestWeirdKanaReadings()
    {
        var kanji = VocabKanji(new()
        {
            ["一"] = ["イチ", "イツ", "ひと-", "ひと.つ"],
            ["ヶ"] = ["か", "が"],
            ["ヵ"] = ["か", "が"],
            ["ケ"] = ["か", "が"],
            ["月"] = ["ゲツ", "ガツ", "つき"],
        });

        var solver = new IterationSolver(kanji, []);

        var data = new List<(string, string, string)>()
        {
            ("一ヶ月", "いっかげつ", "[一|いっ][ヶ|か][月|げつ]"),
            ("一ヵ月", "いっかげつ", "[一|いっ][ヵ|か][月|げつ]"),
            ("一ケ月", "いっかげつ", "[一|いっ][ケ|か][月|げつ]"),
            ("一ケ月", "いっけげつ", "[一|いっ]ケ[月|げつ]"),
            ("一ケ月", "いっケげつ", "[一|いっ]ケ[月|げつ]"),
        };

        TestSolutions(solver, data);
    }

    [TestMethod]
    public void TestNameKanji()
    {
        var vocabKanji = VocabKanji(new()
        {
            ["佐"] = ["あ"],
            ["藤"] = ["あ"],
        });
        var nameKanji = NameKanji(new()
        {
            ["佐"] = ["あ", "さ"],
            ["藤"] = ["あ", "とう"],
        });
        var solver = new IterationSolver(vocabKanji.Concat(nameKanji), []);

        // Cannot solve it as a Vocab Entry.
        var vocab = new VocabEntry("佐藤", "さとう");
        var vocabSolution = solver.Solve(vocab).FirstOrDefault();
        Assert.IsNull(vocabSolution);

        // Can solve it as a Name Entry.
        var name = new NameEntry("佐藤", "さとう");

        var nameSolutions = solver.Solve(name).ToList();
        Assert.HasCount(1, nameSolutions);

        var nameSolution = nameSolutions.FirstOrDefault();
        Assert.IsNotNull(nameSolution);

        var expectedSolution = TextSolution.Parse("[佐|さ][藤|とう]", name);
        CollectionAssert.AreEqual(expectedSolution.Parts, nameSolution.Parts);
    }

    [TestMethod]
    public void RequiresSpecialExpressions()
    {
        var specialExpressions = SpecialExpressions(new()
        {
            ["発条"] = ["ぜんまい", "ばね"],
            ["芝生"] = ["しばふ"],
            ["草履"] = ["ぞうり"],
            ["竹刀"] = ["しない"],
            ["大人"] = ["おとな"],
        });
        var solver = new IterationSolver([], specialExpressions);

        var data = new List<(string, string, string)>()
        {
            ("芝生", "しばふ", "[芝生|しばふ]"),
            ("草履", "ぞうり", "[草履|ぞうり]"),
            ("竹刀", "しない", "[竹刀|しない]"),
            ("大人の人", "おとなのひと", "[大人|おとな]の[人|ひと]"),

            // 発条 has two different special readings
            ("発条仕掛け", "ぜんまいじかけ", "[発条|ぜんまい][仕|じ][掛|か]け"),
            ("発条仕掛け", "ばねじかけ", "[発条|ばね][仕|じ][掛|か]け"),

            // This is bogus data but it will solve because it's the correct length.
            ("発条仕掛け", "ああああけ", "[発|あ][条|あ][仕|あ][掛|あ]け"),
        };

        TestSolutions(solver, data);

        // Unsolvable without kanji reading resource data.
        var unsolvableEntry = new VocabEntry("発条仕掛け", "はつじょうじかけ");
        TestSolutionsCount(0, solver, unsolvableEntry);
    }

    [TestMethod]
    public void RequiresKanjiReadingsAndSpecialExpressions()
    {
        var kanji = VocabKanji(new()
        {
            ["大"] = ["ダイ", "タイ", "おお-", "おお.きい", "-おお.いに"],
            ["和"] = ["ワ", "オ", "カ", "やわ.らぐ", "やわ.らげる", "なご.む", "なご.やか", "あ.える"],
            ["魂"] = ["コン", "たましい", "たま"],
            ["風"] = ["フウ", "フ", "かぜ", "かざ-"],
            ["邪"] = ["ジャ", "よこし.ま"],
            ["薬"] = ["ヤク", "くすり"],
            ["純"] = ["ジュン"],
            ["日"] = ["ニチ", "ジツ", "ひ", "-び", "-か"],
            ["本"] = ["ホン", "もと"],
            ["学"] = ["ガク", "まな.ぶ"],
            ["者"] = ["シャ", "もの"],
            ["製"] = ["セイ"],
            ["側"] = ["ソク", "かわ", "がわ", "そば"],
            ["刀"] = ["トウ", "かたな", "そり"],
            ["発"] = ["ハツ", "ホツ", "た.つ", "あば.く", "おこ.る", "つか.わす", "はな.つ"],
            ["条"] = ["ジョウ", "チョウ", "デキ", "えだ", "すじ"],
            ["仕"] = ["シ", "ジ", "つか.える"],
            ["掛"] = ["カイ", "ケイ", "か.ける", "-か.ける", "か.け", "-か.け", "-が.け", "か.かる", "-か.かる", "-が.かる", "か.かり", "-が.かり", "かかり", "-がかり"],
        });
        var specialExpressions = SpecialExpressions(new()
        {
            ["日本"] = ["にほん"],
            ["大和"] = ["やまと"],
            ["風邪"] = ["かぜ"],
            ["発条"] = ["ぜんまい", "ばね"],
        });
        var solver = new IterationSolver(kanji, specialExpressions);

        var data = new List<(string, string, string)>()
        {
            ("大和魂", "やまとだましい", "[大和|やまと][魂|だましい]"),
            ("風邪薬", "かぜぐすり", "[風邪|かぜ][薬|ぐすり]"),
            ("純日本風", "じゅんにほんふう", "[純|じゅん][日本|にほん][風|ふう]"),
            ("日本学者", "にほんがくしゃ", "[日本|にほん][学|がく][者|しゃ]"),
            ("日本製", "にほんせい", "[日本|にほん][製|せい]"),
            ("日本側", "にほんがわ", "[日本|にほん][側|がわ]"),
            ("日本刀", "にほんとう", "[日本|にほん][刀|とう]"),
            ("日本風", "にほんふう", "[日本|にほん][風|ふう]"),

            // 発条 has two different special readings
            ("発条仕掛け", "ぜんまいじかけ", "[発条|ぜんまい][仕|じ][掛|か]け"),
            ("発条仕掛け", "ばねじかけ", "[発条|ばね][仕|じ][掛|か]け"),

            // Here 発条 uses regular, kanji dictionary readings
            ("発条仕掛け", "はつじょうじかけ", "[発|はつ][条|じょう][仕|じ][掛|か]け"),

            // This is bogus data but it will solve because it's the correct length.
            ("発条仕掛け", "ああああけ", "[発|あ][条|あ][仕|あ][掛|あ]け"),

            // Repeat the above tests with non-normalized readings
            ("発条仕掛け", "ゼンマイじかけ", "[発条|ゼンマイ][仕|じ][掛|か]け"),
            ("発条仕掛け", "バねジカけ", "[発条|バね][仕|ジ][掛|カ]け"),
            ("発条仕掛け", "ハつじョうじカケ", "[発|ハつ][条|じョう][仕|じ][掛|カ]け"),
            ("発条仕掛け", "あアあアけ", "[発|あ][条|ア][仕|あ][掛|ア]け"),
        };

        TestSolutions(solver, data);
    }

    [TestMethod]
    public void UnsolvableWithoutReadingCache()
    {
        var data = new List<(string, string)>()
        {
            ("御姉さん", "おねえさん"),

            // Two characters, but no kanji repeats
            ("可能", "かのう"),
            ("津波", "つなみ"),
            ("問題", "もんだい"),
            ("質問", "しつもん"),

            // Despite being repeated kanji, the reading length is odd
            ("主主", "しゅしゅう"),
            ("主主", "ししう"),

            // Despite having impossible-start characters (ん, ゃ), there are
            // two possible solutions: [乱|らん][脈|みゃく] and [乱|らんみゃ][脈|く].
            ("乱脈", "らんみゃく"),

            // Despite having kanji delimited by kana characters, there are
            // two possible solutions: [好|す]き[嫌|きら]い and [好|すき]き[嫌|ら]い
            ("好き嫌い", "すききらい"),
        };

        TestNullSolutions(_resourcelessSolver, data);
    }

    /// <summary>
    /// Tests a situation in which there is no unique correct solution in principle.
    /// </summary>
    /// <remarks>
    /// [好|すき][嫌|きら] and [好|す][嫌|ききら] are both valid solutions according to the parameters of the problem.
    /// </remarks>
    [TestMethod]
    public void AmbiguousReadingCache()
    {
        var kanji = VocabKanji(new()
        {
            ["好"] = ["すき", "す"],
            ["嫌"] = ["きら", "ききら"],
        });
        var solver = new IterationSolver(kanji, []);

        var entry = new VocabEntry("好嫌", "すききら");
        var solutions = solver.Solve(entry).ToList();
        Assert.HasCount(2, solutions);
    }

    private static void TestSolution(IterationSolver solver, Entry entry, string expectedResultText)
    {
        var solutions = solver.Solve(entry).ToList();
        Assert.HasCount(1, solutions);

        var solution = solutions.First();
        var expectedSolution = TextSolution.Parse(expectedResultText, entry);
        CollectionAssert.AreEqual(expectedSolution.Parts, solution.Parts);
    }

    private static void TestSolutions(IterationSolver solver, IEnumerable<(string, string, string)> data)
    {
        foreach (var (kanjiFormText, readingText, expectedResultText) in data)
        {
            var entry = new VocabEntry(kanjiFormText, readingText);
            TestSolution(solver, entry, expectedResultText);
        }
    }

    private static void TestNullSolutions(IterationSolver solver, IEnumerable<(string, string)> data)
    {
        foreach (var (kanjiFormText, readingText) in data)
        {
            var entry = new VocabEntry(kanjiFormText, readingText);
            TestSolutionsCount(0, solver, entry);
        }
    }

    private static void TestSolutionsCount(int expectedSolutionCount, IterationSolver solver, Entry entry)
    {
        var solutions = solver.Solve(entry);
        Assert.AreEqual(expectedSolutionCount, solutions.Count());
    }

    private static IEnumerable<JapaneseCharacter> VocabKanji(Dictionary<string, List<string>> data) => data
        .Select(static item => new VocabKanji
        (
            item.Key.EnumerateRunes().First(),
            item.Value
        ));

    private static IEnumerable<JapaneseCharacter> NameKanji(Dictionary<string, List<string>> data) => data
        .Select(static item => new NameKanji
        (
            item.Key.EnumerateRunes().First(),
            item.Value
        ));

    private static IEnumerable<SpecialExpression> SpecialExpressions(Dictionary<string, List<string>> data) => data
        .Select(static item => new SpecialExpression
        (
            item.Key,
            item.Value
        ));
}
