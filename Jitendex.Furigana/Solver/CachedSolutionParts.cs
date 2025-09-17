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

using System.Collections.Immutable;
using Jitendex.Furigana.Models;
using Jitendex.Furigana.TextExtensions;

namespace Jitendex.Furigana.Solver;

internal class CachedSolutionParts
{
    private readonly ImmutableArray<string> _cachedReadings;

    public CachedSolutionParts(ImmutableArray<string> cachedReadings)
    {
        _cachedReadings = cachedReadings;
    }

    public IEnumerable<List<Solution.Part>> Enumerate(KanjiFormSlice kanjiFormSlice, ReadingState readingState)
    {
        var baseText = kanjiFormSlice.RawText();
        foreach (var reading in _cachedReadings)
        {
            if (!readingState.RemainingTextNormalized.StartsWith(reading))
            {
                continue;
            }
            else if (baseText.IsKanaEquivalent(reading))
            {
                yield return [new(baseText, null)];
            }
            else
            {
                var furigana = readingState.RemainingText[..reading.Length];
                yield return [new(baseText, furigana)];
            }
        }
    }
}
