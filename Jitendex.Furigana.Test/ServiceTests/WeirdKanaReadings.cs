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

namespace Jitendex.Furigana.Test.ServiceTests;

[TestClass]
public class WeirdKanaReadings
{
    private static readonly IEnumerable<JapaneseCharacter> _kanji = ResourceMethods.VocabKanji(new()
    {
        ["一"] = ["イチ", "イツ", "ひと-", "ひと.つ"],
        ["ヶ"] = ["か", "が"],
        ["ヵ"] = ["か", "が"],
        ["ケ"] = ["か", "が"],
        ["月"] = ["ゲツ", "ガツ", "つき"],
    });

    private static readonly Service _service = new(_kanji, []);

    private static readonly SolvableData _data =
    [
        ("一ヶ月", "いっかげつ", "[一|いっ][ヶ|か][月|げつ]"),
        ("一ヵ月", "いっかげつ", "[一|いっ][ヵ|か][月|げつ]"),
        ("一ケ月", "いっかげつ", "[一|いっ][ケ|か][月|げつ]"),

        ("一ケ月", "いっけげつ", "[一|いっ]ケ[月|げつ]"),
        ("一ケ月", "いっケげつ", "[一|いっ]ケ[月|げつ]"),
    ];

    [TestMethod]
    public void TestSolvable()
    {
        SolverTestMethods.TestSolvable(_service, _data);
    }
}
