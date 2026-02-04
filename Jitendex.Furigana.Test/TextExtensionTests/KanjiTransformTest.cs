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

using System.Text;
using Jitendex.Furigana.TextExtensions;

namespace Jitendex.Furigana.Test.TextExtensionTests;

/// <summary>
/// Tests the <see cref="KanjiTransform.IterationMarksToKanji"/> extension method.
/// </summary>
[TestClass]
public class KanjiTransformTest
{
    private static readonly (string, string)[] _data =
    [
        // Using 々
        ("時々", "時時"),
        ("古々々米", "古古古米"),
        ("事々物々", "事事物物"),
        ("一杯々々", "一杯一杯"),

        // Using 〻
        ("時〻", "時時"),
        ("古〻〻米", "古古古米"),
        ("事〻物〻", "事事物物"),
        ("一杯〻〻", "一杯一杯"),

        // Using a mix of both
        ("古々〻米", "古古古米"),
        ("古〻々米", "古古古米"),
        ("事々物〻", "事事物物"),
        ("事〻物々", "事事物物"),
        ("一杯々〻", "一杯一杯"),
        ("一杯〻々", "一杯一杯"),

        // Just for laughs
        ("一杯々々々々", "一杯一杯一杯"),
    ];

    /// <summary>
    /// Pathological case.
    /// </summary>
    /// <remarks>
    /// Could be handled by checking if the substitute character is
    /// truly a kanji with the <see cref="KanjiComparison.IsKanji"/>
    /// method. I don't think it's worth it.
    /// </remarks>
    private static readonly (string, string)[] _unsolvableData =
    [
        ("古々々米、古々々米", "古古古米、古古古米"),
    ];

    [TestMethod]
    public void TestSolvable()
    {
        foreach (var (expectedRunes, runes) in GetRunes(_data))
        {
            CollectionAssert.AreEqual
            (
                expectedRunes,
                runes
            );
        }
    }

    [TestMethod]
    public void TestUnsolvable()
    {
        foreach (var (expectedRunes, runes) in GetRunes(_unsolvableData))
        {
            CollectionAssert.AreNotEqual
            (
                expectedRunes,
                runes
            );
        }
    }

    private static IEnumerable<(Rune[] ExpectedRunes, Rune[] Runes)> GetRunes((string, string)[] data)
    {
        foreach (var (kanjiFormText, expectedText) in data)
        {
            var runes = kanjiFormText.EnumerateRunes().ToArray();
            var expectedRunes = expectedText.EnumerateRunes().ToArray();

            yield return (expectedRunes, runes.IterationMarksToKanji());
        }
    }
}
