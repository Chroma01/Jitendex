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

using Jitendex.Furigana.Solver;

namespace Jitendex.Furigana.Test.SolverTests;

[TestClass]
public class EmptyOrWhitespace
{
    private static readonly IterationSolver _solver = new([], []);

    private const string Empty = "";
    private const string OneSpace = " ";
    private const string TwoSpaces = "  ";
    private const string ThreeSpaces = "   ";

    private static readonly SolvableData _solvableData =
    [
        (Empty, Empty, Empty),

        (OneSpace, OneSpace, OneSpace),
        (OneSpace, TwoSpaces, $"[{OneSpace}|{TwoSpaces}]"),
        (OneSpace, ThreeSpaces, $"[{OneSpace}|{ThreeSpaces}]"),

        (TwoSpaces, TwoSpaces, TwoSpaces),

        (ThreeSpaces, ThreeSpaces, ThreeSpaces),
    ];

    private static readonly UnsolvableData _unsolvableData =
    [
        (Empty, OneSpace, 0),
        (Empty, TwoSpaces, 0),
        (Empty, ThreeSpaces, 0),

        (OneSpace, Empty, 0),

        (TwoSpaces, Empty, 0),
        (TwoSpaces, OneSpace, 0),
        (TwoSpaces, ThreeSpaces, 0),

        (ThreeSpaces, Empty, 0),
        (ThreeSpaces, OneSpace, 0),
        (ThreeSpaces, TwoSpaces, 0),
    ];

    [TestMethod]
    public void TestSolvable()
    {
        SolverTestMethods.TestSolvable(_solver, _solvableData);
    }

    [TestMethod]
    public void TestUnsolvable()
    {
        SolverTestMethods.TestUnsolvable(_solver, _unsolvableData);
    }
}
