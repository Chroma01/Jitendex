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

namespace Jitendex.Furigana.Solver;

internal class SolutionParts
{
    private readonly CachedSolutionParts _cachedParts;
    private readonly DefaultSolutionParts _defaultParts;

    public SolutionParts(CachedSolutionParts cachedParts, DefaultSolutionParts defaultParts)
    {
        _cachedParts = cachedParts;
        _defaultParts = defaultParts;
    }

    public IEnumerable<List<Solution.Part>> Enumerate(Entry entry, KanjiFormSlice kanjiFormSlice, ReadingState readingState)
    {
        bool anyCachedParts = false;

        foreach(var nextParts in _cachedParts.Enumerate(entry, kanjiFormSlice, readingState))
        {
            anyCachedParts = true;
            yield return nextParts;
        }

        if (anyCachedParts) yield break;

        foreach(var nextParts in _defaultParts.Enumerate(kanjiFormSlice, readingState))
        {
            yield return nextParts;
        }
    }
}
