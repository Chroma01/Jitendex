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
using Jitendex.Furigana.OutputModels;
using Jitendex.Furigana.Solvers;

namespace Jitendex.Furigana;

public class Service
{
    private readonly IterationSolver _solver;

    public Service(ReadingCache readingCache)
    {
        _solver = new(readingCache);
    }

    public Solution? Solve(Entry entry)
    {
        var solutions = _solver.Solve(entry).ToList();

        if (solutions.Count == 1)
        {
            return solutions.First();
        }
        else
        {
            return null;
        }
    }
}
