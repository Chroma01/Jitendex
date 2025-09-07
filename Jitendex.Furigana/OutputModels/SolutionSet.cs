/*
Copyright (c) 2025 Doublevil
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

namespace Jitendex.Furigana.OutputModels;

/// <summary>
/// Contains a set of furigana solutions that solves a vocab entry.
/// </summary>
internal class SolutionSet()
{
    private readonly List<IndexedSolution> _solutions = [];

    public int Count { get => _solutions.Count; }

    public void Add(IEnumerable<IndexedSolution> solutions)
    {
        foreach (var solution in solutions)
        {
            Add(solution);
        }
    }

    private void Add(IndexedSolution solution)
    {
        if (_solutions.Any(s => s.Equals(solution)))
            return;
        _solutions.Add(solution);
    }

    public Solution? GetSolution()
    {
        if (Count == 1)
            return _solutions.First().ToTextSolution();
        else
            return null;
    }
}
