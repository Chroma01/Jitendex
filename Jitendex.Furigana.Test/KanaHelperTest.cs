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

using Jitendex.Furigana.Helpers;

namespace Jitendex.Furigana.Test;

[TestClass]
public class KanaHelperTest
{
    private static readonly string hiraTest = "Abc5゠ぁゖずほヷヸヹヺ・ーゝゞヿ";
    private static readonly string kataTest = "Abc5゠ァヶズホヷヸヹヺ・ーヽヾヿ";

    [TestMethod]
    public void TestKataToHira()
    {
        Assert.AreEqual
        (
            hiraTest,
            KanaHelper.KatakanaToHiragana(kataTest)
        );
    }

    [TestMethod]
    public void TestHiraToKata()
    {
        Assert.AreEqual
        (
            KanaHelper.HiraganaToKatakana(hiraTest),
            kataTest
        );
    }

    [TestMethod]
    public void TestIsAllHiragana()
    {
        Assert.IsFalse(KanaHelper.IsAllHiragana("Abcあかさたアカサタ安加左太"));
        Assert.IsTrue(KanaHelper.IsAllHiragana("ぁあぃいぅうぇえぉんゝゞゟ"));

        Assert.IsTrue(KanaHelper.IsAllHiragana('\u3096'.ToString()));
        Assert.IsFalse(KanaHelper.IsAllHiragana('\u3097'.ToString()));
        Assert.IsFalse(KanaHelper.IsAllHiragana('\u3098'.ToString()));
        Assert.IsTrue(KanaHelper.IsAllHiragana('\u3099'.ToString()));
    }

    [TestMethod]
    public void TestIsAllKatakana()
    {
        Assert.IsFalse(KanaHelper.IsAllKatakana("Abcあかさたアカサタ安加左太"));
        Assert.IsTrue(KanaHelper.IsAllKatakana("゠アヲンヴヵヶヷヸヹヺ・ーヽヾヿ"));
    }

    [TestMethod]
    public void TestIsAllKana()
    {
        Assert.IsFalse(KanaHelper.IsAllKana("Abcあかさたアカサタ安加左太"));
        Assert.IsTrue(KanaHelper.IsAllKana("ぁあぃいぅうぇえぉんゝゞゟ"));
        Assert.IsTrue(KanaHelper.IsAllKana("゠アヲンヴヵヶヷヸヹヺ・ーヽヾヿ"));

        Assert.IsTrue(KanaHelper.IsAllKana('\u3096'.ToString()));
        Assert.IsFalse(KanaHelper.IsAllKana('\u3097'.ToString()));
        Assert.IsFalse(KanaHelper.IsAllKana('\u3098'.ToString()));
        Assert.IsTrue(KanaHelper.IsAllKana('\u3099'.ToString()));
    }
}
