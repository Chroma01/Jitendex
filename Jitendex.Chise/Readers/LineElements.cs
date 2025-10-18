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

namespace Jitendex.Chise.Readers;

internal readonly ref struct LineElements
{
    public ReadOnlySpan<char> Codepoint { get; init; }
    public ReadOnlySpan<char> Character { get; init; }
    public ReadOnlySpan<char> Sequence { get; init; }
    public ReadOnlySpan<char> AltSequence { get; init; }
    private const string apparent = "@apparent=";

    public LineElements(in ReadOnlySpan<char> line)
    {
        int idx = 0;

        foreach (var range in line.Split('\t'))
        {
            var segment = line[range];

            if (idx == 0)
                Codepoint = segment;
            else if (idx == 1)
                Character = segment;
            else if (idx == 2)
                Sequence = segment[..^TagLength(segment)];
            else if (idx == 3 && segment.StartsWith(apparent))
                AltSequence = segment[apparent.Length..^TagLength(segment)];
            else if (idx == 3)
                Console.WriteLine($"Malformatted alt sequence element:\t`{line}`");

            idx++;
        }

        if (idx < 3)
        {
            throw new ArgumentException($"Not enough elements in line: `{line}`");
        }
        else if (idx > 4)
        {
            Console.WriteLine($"Too many elements in this line:   \t`{line}`");
        }
    }

    /// <summary>
    /// Length of a trailing "[U]" "[J]" "[K]" etc tag at the end of an IDS text
    /// </summary>
    private static int TagLength(in ReadOnlySpan<char> sequence) => sequence switch
    {
        [.., ' ', '[', _, ']'] => 4,
        [.., '[', _, ']'] => 3,
        _ => 0,
    };
}