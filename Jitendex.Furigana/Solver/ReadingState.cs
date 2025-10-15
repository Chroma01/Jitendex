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

internal readonly record struct ReadingState
{
    private readonly Entry _entry;
    private readonly int _readingIndex;

    public string FullText { get => _entry.ReadingText; }
    public string PriorText { get => FullText[.._readingIndex]; }
    public string RemainingText { get => FullText[_readingIndex..]; }

    public string FullTextNormalized { get => _entry.NormalizedReadingText; }
    public string PriorTextNormalized { get => FullTextNormalized[.._readingIndex]; }
    public string RemainingTextNormalized { get => FullTextNormalized[_readingIndex..]; }

    public ReadingState(Entry entry, int readingIndex)
    {
        _entry = entry;
        _readingIndex = readingIndex;
    }
}
