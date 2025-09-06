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

namespace Jitendex.Furigana.Models;

public class FuriganaPart : IComparable<FuriganaPart>, ICloneable
{
    public int StartIndex { get; }
    public int EndIndex { get; }
    public string Value { get; }

    public FuriganaPart(string value, int startIndex)
        : this(value, startIndex, startIndex) { }

    public FuriganaPart(string value, int startIndex, int endIndex)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentOutOfRangeException(nameof(value),
                "Furigana character string cannot be empty or whitespace");
        if (startIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(startIndex),
                "Starting position must be non-negative");
        if (endIndex < startIndex)
            throw new ArgumentOutOfRangeException(nameof(endIndex),
                "End position must be after the start position");

        Value = value;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }

    public override string ToString()
    {
        if (StartIndex == EndIndex)
        {
            return $"{StartIndex}:{Value}";
        }
        else
        {
            return $"{StartIndex}-{EndIndex}:{Value}";
        }
    }

    public int CompareTo(FuriganaPart? other)
    {
        return StartIndex.CompareTo(other?.StartIndex);
    }

    public override bool Equals(object? obj)
    {
        if (obj is FuriganaPart other)
        {
            return StartIndex == other.StartIndex
                && EndIndex == other.EndIndex
                && Value == other.Value;
        }
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public object Clone()
    {
        return new FuriganaPart(Value, StartIndex, EndIndex);
    }
}
