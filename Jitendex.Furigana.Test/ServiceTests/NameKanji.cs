/*
Copyright (c) 2025 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms of
the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

using Jitendex.Furigana.Models;
using Jitendex.Furigana.Models.TextUnits;

namespace Jitendex.Furigana.Test.ServiceTests;

[TestClass]
public class NameKanji : ServiceTest
{
    private static readonly IEnumerable<JapaneseCharacter> _kanji = ResourceMethods.NameKanji(new()
    {
        ["佐"] = (["あ"], ["さ"]),
        ["藤"] = (["あ"], ["とう"]),
    });

    private static readonly Service _service = new(_kanji, []);

    private const string _kanjiFormText = "佐藤";
    private const string _readingText = "さとう";
    private const string _expectedSolutionText = "[佐|さ][藤|とう]";

    [TestMethod]
    public void TestSolvable()
    {
        var nameEntry = new NameEntry(_kanjiFormText, _readingText);
        var nameSolution = _service.Solve(nameEntry);
        Assert.IsNotNull(nameSolution);

        var expectedSolution = TextSolution.Parse(_expectedSolutionText, nameEntry);
        Assert.AreEqual(expectedSolution, nameSolution);
    }

    [TestMethod]
    public void TestUnsolvable()
    {
        var vocabEntry = new VocabEntry(_kanjiFormText, _readingText);
        var vocabSolution = _service.Solve(vocabEntry);
        Assert.IsNull(vocabSolution);
    }
}
