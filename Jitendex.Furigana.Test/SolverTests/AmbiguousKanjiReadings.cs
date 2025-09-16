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
using Jitendex.Furigana.Solver;

namespace Jitendex.Furigana.Test.SolverTests;

/// <summary>
/// Tests a situation in which there is no unique correct solution in principle.
/// </summary>
/// <remarks>
/// [好|すき]き[嫌|ら]い and [好|す]き[嫌|きら]い are both valid solutions according to the parameters of the problem.
/// </remarks>
[TestClass]
public class AmbiguousKanjiReadings
{
    private static readonly IEnumerable<JapaneseCharacter> _kanji = ResourceMethods.VocabKanji(new()
    {
        ["好"] = ["コウ", "この.む", "す.く", "よ.い", "い.い"],
        ["嫌"] = ["ケン", "ゲン", "きら.う", "きら.い", "いや"],
    });

    private static readonly IterationSolver _solver = new(_kanji, []);

    private static readonly (string, string)[] _data = new[]
    {
        ("好き嫌い", "すききらい"),
    };

    [TestMethod]
    public void Test()
    {
        SolverTestMethods.TestUnsolvable(2, _solver, _data);
    }
}
