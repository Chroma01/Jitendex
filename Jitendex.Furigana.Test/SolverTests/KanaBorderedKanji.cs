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
public class KanaBorderedKanji
{
    private static readonly IterationSolver _solver = new([], []);

    private static readonly (string, string, string)[] _data = new[]
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

    [TestMethod]
    public void Test()
    {
        foreach (var (kanjiFormText, readingText, _) in _data)
        {
            Assert.AreNotEqual(kanjiFormText.Length, readingText.Length);
        }

        SolverTestMethods.TestSolvable(_solver, _data);
    }
}
