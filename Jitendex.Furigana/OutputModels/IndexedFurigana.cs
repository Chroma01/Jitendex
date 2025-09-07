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

internal class IndexedFurigana : IComparable<IndexedFurigana>, ICloneable
{
    public string Value { get; }
    public int StartIndex { get; }
    public int EndIndex { get; }

    public IndexedFurigana(string value, int startIndex)
        : this(value, startIndex, startIndex) { }

    public IndexedFurigana(string value, int startIndex, int endIndex)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentOutOfRangeException(nameof(value),
                "Furigana character string cannot be empty or whitespace");
        if (startIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(startIndex),
                "Starting position must be non-negative");
        if (endIndex < startIndex)
            throw new ArgumentOutOfRangeException(nameof(endIndex),
                "End position must be greater than or equal to the start position");

        Value = value;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }

    public int CompareTo(IndexedFurigana? other) =>
        StartIndex.CompareTo(other?.StartIndex);

    public override bool Equals(object? obj) =>
        obj is IndexedFurigana other &&
        Value == other.Value &&
        StartIndex == other.StartIndex &&
        EndIndex == other.EndIndex;

    public override int GetHashCode() =>
        HashCode.Combine(Value, StartIndex, EndIndex);

    public object Clone() =>
        new IndexedFurigana(Value, StartIndex, EndIndex);
}
