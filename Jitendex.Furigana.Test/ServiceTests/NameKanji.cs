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
using Jitendex.Furigana.Models.TextUnits;

namespace Jitendex.Furigana.Test.ServiceTests;

[TestClass]
public class NameKanji : ServiceTest
{
    private static readonly IEnumerable<JapaneseCharacter> _vocabKanji = ResourceMethods.VocabKanji(new()
    {
        ["佐"] = ["あ"],
        ["藤"] = ["あ"],
    });

    private static readonly IEnumerable<JapaneseCharacter> _nameKanji = ResourceMethods.NameKanji(new()
    {
        ["佐"] = (["あ"], ["さ"]),
        ["藤"] = (["あ"], ["とう"]),
    });

    private static readonly Service _service = new(_vocabKanji.Concat(_nameKanji), []);

    private const string _kanjiFormText = "佐藤";
    private const string _readingText = "さとう";
    private const string _expectedSolutionText = "[佐|さ][藤|とう]";

    private static readonly VocabEntry _vocabEntry = new(_kanjiFormText, _readingText);
    private static readonly NameEntry _nameEntry = new(_kanjiFormText, _readingText);

    private static readonly Solution? _vocabSolution = _service.Solve(_vocabEntry);
    private static readonly Solution? _nameSolution = _service.Solve(_nameEntry);
    private static readonly Solution _expectedSolution = TextSolution.Parse(_expectedSolutionText, _nameEntry);

    [TestMethod]
    public void TestSolvable()
    {
        Assert.IsNotNull(_nameSolution);
        Assert.AreEqual(_expectedSolution, _nameSolution);
    }

    [TestMethod]
    public void TestUnsolvable()
    {
        Assert.IsNull(_vocabSolution);
    }
}
