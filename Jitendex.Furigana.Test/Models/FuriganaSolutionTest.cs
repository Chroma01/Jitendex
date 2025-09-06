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

namespace Jitendex.Furigana.Test.Models;

[TestClass]
public class FuriganaSolutionTest
{
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

    [TestMethod]
    public void TestBreakIntoPartsHakabakashii()
    {
        var vocab = new VocabEntry("捗々しい", "はかばかしい");
        var solution = new FuriganaSolution(vocab, new FuriganaPart("はか", 0), new FuriganaPart("ばか", 1));

        var parts = solution.BreakIntoParts().ToList();

        Assert.HasCount(3, parts);
        Assert.AreEqual("捗", parts[0].Text);
        Assert.AreEqual("はか", parts[0].Furigana);
        Assert.AreEqual("々", parts[1].Text);
        Assert.AreEqual("ばか", parts[1].Furigana);
        Assert.AreEqual("しい", parts[2].Text);
        Assert.IsNull(parts[2].Furigana);
    }
}
