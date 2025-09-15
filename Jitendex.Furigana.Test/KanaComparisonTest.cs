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

using Jitendex.Furigana.TextExtensions;

namespace Jitendex.Furigana.Test;

[TestClass]
public class KanaComparisonTest
{
    [TestMethod]
    public void TestIsAllHiragana()
    {
        Assert.IsFalse("Abcあかさたアカサタ安加左太".IsAllHiragana());
        Assert.IsTrue("ぁあぃいぅうぇえぉんゝゞゟ".IsAllHiragana());

        Assert.IsTrue('\u3096'.IsHiragana());
        Assert.IsFalse('\u3097'.IsHiragana());
        Assert.IsFalse('\u3098'.IsHiragana());
        Assert.IsTrue('\u3099'.IsHiragana());
    }

    [TestMethod]
    public void TestIsAllKatakana()
    {
        Assert.IsFalse("Abcあかさたアカサタ安加左太".IsAllKatakana());
        Assert.IsTrue("゠アヲンヴヵヶヷヸヹヺ・ーヽヾヿ".IsAllKatakana());
    }

    [TestMethod]
    public void TestIsAllKana()
    {
        Assert.IsFalse("Abcあかさたアカサタ安加左太".IsAllKana());
        Assert.IsTrue("ぁあぃいぅうぇえぉんゝゞゟ".IsAllKana());
        Assert.IsTrue("゠アヲンヴヵヶヷヸヹヺ・ーヽヾヿ".IsAllKana());

        Assert.IsTrue('\u3096'.IsKana());
        Assert.IsFalse('\u3097'.IsKana());
        Assert.IsFalse('\u3098'.IsKana());
        Assert.IsTrue('\u3099'.IsKana());
    }
}
