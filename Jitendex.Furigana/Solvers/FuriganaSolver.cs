/*
Copyright (c) 2015 Doublevil
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

namespace Jitendex.Furigana.Solvers;

internal interface IFuriganaSolver : IComparable<IFuriganaSolver>
{
    int Priority { get; set; }
    IEnumerable<Solution> Solve(Entry entry);
}

internal abstract class FuriganaSolver : IFuriganaSolver
{
    public int Priority { get; set; }

    public abstract IEnumerable<Solution> Solve(Entry entry);

    public int CompareTo(IFuriganaSolver? other)
    {
        return Priority.CompareTo(other?.Priority);
    }
}
