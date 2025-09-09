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

using Jitendex.Furigana.InputModels;
using Jitendex.Furigana.OutputModels;

namespace Jitendex.Furigana.Test.Models;

[TestClass]
public class TextSolutionTest
{
    [TestMethod]
    public void TestRubySolutionAkagaeruka()
    {
        var vocab = new VocabEntry("アカガエル科", "アカガエルか");
        var indexedSolution = new IndexedSolution
        (
            vocab,
            new IndexedFurigana("か", 5)
        );

        var textSolution = indexedSolution.ToTextSolution();
        var parts = textSolution.Parts;

        Assert.HasCount(2, parts);
        Assert.AreEqual("アカガエル", parts[0].BaseText);
        Assert.IsNull(parts[0].Furigana);
        Assert.AreEqual("科", parts[1].BaseText);
        Assert.AreEqual("か", parts[1].Furigana);
    }

    [TestMethod]
    public void TestRubySolutionOtonagai()
    {
        var vocab = new VocabEntry("大人買い", "おとながい");
        var indexedSolution = new IndexedSolution
        (
            vocab,
            new IndexedFurigana("おとな", 0, 1),
            new IndexedFurigana("が", 2)
        );

        var textSolution = indexedSolution.ToTextSolution();
        var parts = textSolution.Parts;

        Assert.HasCount(3, parts);
        Assert.AreEqual("大人", parts[0].BaseText);
        Assert.AreEqual("おとな", parts[0].Furigana);
        Assert.AreEqual("買", parts[1].BaseText);
        Assert.AreEqual("が", parts[1].Furigana);
        Assert.AreEqual("い", parts[2].BaseText);
        Assert.IsNull(parts[2].Furigana);
    }

    [TestMethod]
    public void TestRubySolutionHakabakashii()
    {
        var vocab = new VocabEntry("捗々しい", "はかばかしい");
        var indexedSolution = new IndexedSolution
        (
            vocab,
            new IndexedFurigana("はか", 0),
            new IndexedFurigana("ばか", 1)
        );

        var textSolution = indexedSolution.ToTextSolution();
        var parts = textSolution.Parts;

        Assert.HasCount(3, parts);
        Assert.AreEqual("捗", parts[0].BaseText);
        Assert.AreEqual("はか", parts[0].Furigana);
        Assert.AreEqual("々", parts[1].BaseText);
        Assert.AreEqual("ばか", parts[1].Furigana);
        Assert.AreEqual("しい", parts[2].BaseText);
        Assert.IsNull(parts[2].Furigana);
    }
}
