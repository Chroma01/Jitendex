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

    public LineElements(string line)
    {
        string[] lineSplit = line.Split('\t');

        if (lineSplit.Length < 3)
        {
            throw new ArgumentException($"Not enough elements in line: `{line}`");
        }
        else if (lineSplit.Length > 4)
        {
            Console.WriteLine($"Too many elements in this line:   \t`{line}`");
        }

        Codepoint = lineSplit[0];
        Character = lineSplit[1];
        Sequence = TrimTag(lineSplit[2]);

        if (lineSplit.Length == 3)
        {
            AltSequence = [];
        }
        else if (lineSplit[3].StartsWith(apparent, StringComparison.OrdinalIgnoreCase))
        {
            AltSequence = TrimTag(lineSplit[3].AsSpan()[apparent.Length..]);
        }
        else
        {
            Console.WriteLine($"Malformatted alt sequence element:\t`{line}`");
            AltSequence = null;
        }
    }

    /// <summary>
    /// Remove a trailing "[U]" "[J]" "[K]" etc tag from the end of an IDS text
    /// </summary>
    private static ReadOnlySpan<char> TrimTag(ReadOnlySpan<char> sequence) => sequence switch
    {
        [.., ' ', '[', _, ']'] => sequence[..^4],
        [.., '[', _, ']'] => sequence[..^3],
        _ => sequence,
    };
}