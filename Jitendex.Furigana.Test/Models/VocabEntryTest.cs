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

namespace Jitendex.Furigana.Test.Models;

[TestClass]
public class VocabEntryTest
{
    [TestMethod]
    public void TestSingleRepeater()
    {
        var v = new VocabEntry("時々", "ときどき");
        var expectedRunes = "時時".EnumerateRunes().ToList();
        CollectionAssert.AreEqual(expectedRunes, v.KanjiFormRunes);
    }

    [TestMethod]
    public void TestSingleRepeaterRawRunes()
    {
        var v = new VocabEntry("時々", "ときどき");
        var expectedRunes = "時々".EnumerateRunes().ToList();
        CollectionAssert.AreEqual(expectedRunes, v.RawKanjiFormRunes);
    }

    [TestMethod]
    public void TestDoubleRepeater()
    {
        var v = new VocabEntry("一杯々々", "いっぱいいっぱい");
        var expectedRunes = "一杯一杯".EnumerateRunes().ToList();
        CollectionAssert.AreEqual(expectedRunes, v.KanjiFormRunes);
    }

    [TestMethod]
    public void TestDoubleRepeaterRawRunes()
    {
        var v = new VocabEntry("一杯々々", "いっぱいいっぱい");
        var expectedRunes = "一杯々々".EnumerateRunes().ToList();
        CollectionAssert.AreEqual(expectedRunes, v.RawKanjiFormRunes);
    }

    [TestMethod]
    public void TestDoubleRepeaterSameKanji()
    {
        var v = new VocabEntry("古々々米", "こここまい");
        var expectedRunes = "古古古米".EnumerateRunes().ToList();
        CollectionAssert.AreEqual(expectedRunes, v.KanjiFormRunes);
    }

    [TestMethod]
    public void TestTwoSingleRepeaters()
    {
        var v = new VocabEntry("事々物々", "じじぶつぶつ");
        var expectedRunes = "事事物物".EnumerateRunes().ToList();
        CollectionAssert.AreEqual(expectedRunes, v.KanjiFormRunes);
    }
}
