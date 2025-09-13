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
internal class IterationSolverTestWithoutResources : SolverTest
{
    private static readonly IterationSolver _solver = new(new ReadingCache([], []));

    [TestMethod]
    public void EqualLengthSolutions()
    {
        var data = new List<(string, string, string)>()
        {
            ("木の葉", "このは", "[木|こ]の[葉|は]"),
            ("こ之は", "このは", "こ[之|の]は"),
            ("余所見", "よそみ", "[余|よ][所|そ][見|み]"),

            // Don't capture the impossible start characters (っ, ん, etc.) if not followed by a kanji
            ("真っさお", "まっさお", "[真|ま]っさお"),
            ("を呼んで", "をよんで", "を[呼|よ]んで"),
            ("田ん圃", "たんぼ", "[田|た]ん[圃|ぼ]"),

            // With non-normalized kanji forms
            ("木ノ葉", "このは", "[木|こ]ノ[葉|は]"),
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

        TestSolutions(_solver, data);
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
    public void UnequalLengthSolutions()
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
            ("嗹", "れん", "[嗹|れん]"),
            ("如何", "いかん", "[如|い][何|かん]"),
            ("阿呆陀羅", "あほんだら", "[阿|あ][呆|ほん][陀|だ][羅|ら]"),

            // With two consecutive impossible-start characters
            ("勅勘", "ちょっかん", "[勅|ちょっ][勘|かん]"),

            // With kana following the kanji
            ("真さお", "まっさお", "[真|まっ]さお"),
            ("危機に瀕する", "ききにひんする", "[危|き][機|き]に[瀕|ひん]する"),

            // With non-normalized readings
            ("阿呆陀羅", "アほンだラ", "[阿|ア][呆|ほン][陀|だ][羅|ラ]"),
            ("嗹", "レン", "[嗹|レン]"),
        };

        foreach (var (kanjiFormText, readingText, _) in data)
        {
            Assert.AreNotEqual(kanjiFormText.Length, readingText.Length);
        }

        TestSolutions(_solver, data);
    }

    [TestMethod]
    public void NullSolution()
    {
        var data = new List<(string, string)>()
        {
            // This one almost looks solvable, but note that there are two
            // potential solutions: [乱|らん][脈|みゃく] and [乱|らんみゃ][脈|く].
            ("乱脈", "らんみゃく"),

            // Solvable by RegexSolver, but not this solver.
            ("真っ青", "まっさお"),
            ("桜ん坊", "さくらんぼ"),

            // Solvable by RepeatedKanjiSolver, but not this solver.
            ("抑抑", "そもそも"),

            // Solvable by SingleKanjiSolver, but not this solver.
            ("難しい", "むずかしい"),
        };

        TestNullSolutions(_solver, data);
    }
}
