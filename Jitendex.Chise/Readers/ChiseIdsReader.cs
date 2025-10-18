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

using Jitendex.Chise.Models;
using Jitendex.Chise.Models.Sequences;

namespace Jitendex.Chise.Readers;

public static class ChiseIdsReader
{
    public static async Task<List<Codepoint>> ReadAsync(DirectoryInfo chiseIdsDir)
    {
        var codepoints = new List<Codepoint>();
        foreach (var file in chiseIdsDir.EnumerateFiles("IDS-UCS-*.txt"))
        {
            Console.WriteLine();
            Console.WriteLine(file.FullName);
            await ReadFileAsync(file, codepoints);
        }
        return codepoints;
    }

    private static async Task ReadFileAsync(FileInfo file, List<Codepoint> codepoints)
    {
        using StreamReader sr = file.OpenText();
        while (await sr.ReadLineAsync() is string line)
        {
            if (line.StartsWith(';'))
            {
                continue;
            }
            var lineElements = new LineElements(line);
            var codepoint = MakeCodepoint(lineElements);
            codepoints.Add(codepoint);
        }
    }

    private static Codepoint MakeCodepoint(LineElements lineElements)
    {
        var unicodeCharacter = MakeUnicodeCharacter(lineElements);

        var id = unicodeCharacter is null
                 ? new string(lineElements.Codepoint)
                 : unicodeCharacter.CodepointId;

        var sequence = MakeSequence(lineElements.Sequence);

        var altSequence = lineElements.AltSequence is []
                          ? null
                          : MakeSequence(lineElements.AltSequence);

        return new Codepoint
        {
            Id = id,
            UnicodeScalarValue = unicodeCharacter?.ScalarValue,
            SequenceText = sequence?.Text,
            AltSequenceText = altSequence?.Text,
            UnicodeCharacter = unicodeCharacter,
            Sequence = sequence,
            AltSequence = altSequence,
        };
    }

    private static UnicodeCharacter? MakeUnicodeCharacter(LineElements lineElements)
    {
        var scalarValue = UnicodeScalarValue(lineElements.Character);
        if (scalarValue is null)
        {
            return null;
        }

        var shortId = GetShortCodepointId((int)scalarValue);
        var longId = GetLongCodepointId((int)scalarValue);

        if (!shortId.SequenceEqual(lineElements.Codepoint) && !longId.SequenceEqual(lineElements.Codepoint))
        {
            Console.WriteLine($"Inequality between codepoint '{lineElements.Codepoint}' and character '{lineElements.Character}'");
            Console.WriteLine($"Expected '{longId}' or '{shortId}'");
        }

        return new UnicodeCharacter
        {
            ScalarValue = (int)scalarValue,
            CodepointId = new string(longId),
        };
    }

    private static int? UnicodeScalarValue(ReadOnlySpan<char> character) => character switch
    {
        { Length: 1 } => character[0],
        { Length: 2 } when char.IsHighSurrogate(character[0])
                        && char.IsLowSurrogate(character[1])
                        => char.ConvertToUtf32(character[0], character[1]),
        _ => null,
    };
    private static ReadOnlySpan<char> GetLongCodepointId(int scalarValue) => $"U-{scalarValue:X8}".AsSpan();
    private static ReadOnlySpan<char> GetShortCodepointId(int scalarValue) => $"U+{scalarValue:X}";

    private static Sequence? MakeSequence(ReadOnlySpan<char> sequenceText)
    {
        Stack<Codepoint> arguments = [];

        while (PushNextArgument(sequenceText, arguments) is var nextText and not [])
        {
            sequenceText = nextText;
        }

        if (arguments.Count != 1)
        {
            throw new ArgumentException($"Invalid sequence text: `{sequenceText}`");
        }
        var codepoint = arguments.Pop();
        return codepoint.Sequence;
    }

    private static Sequence? MakeSequence(ReadOnlySpan<char> indicator, Stack<Codepoint> arguments) => indicator switch
    {
        [LeftToRightSequence.Indicator] => new LeftToRightSequence(arguments),
        [AboveToBelowSequence.Indicator] => new AboveToBelowSequence(arguments),
        [LeftToMiddleAndRightSequence.Indicator] => new LeftToMiddleAndRightSequence(arguments),
        [AboveToMiddleAndBelowSequence.Indicator] => new AboveToMiddleAndBelowSequence(arguments),
        [FullSurroundSequence.Indicator] => new FullSurroundSequence(arguments),
        [SurroundFromAboveSequence.Indicator] => new SurroundFromAboveSequence(arguments),
        [SurroundFromBelowSequence.Indicator] => new SurroundFromBelowSequence(arguments),
        [SurroundFromLeftSequence.Indicator] => new SurroundFromLeftSequence(arguments),
        [SurroundFromRightSequence.Indicator] => new SurroundFromRightSequence(arguments),
        [SurroundFromUpperLeftSequence.Indicator] => new SurroundFromUpperLeftSequence(arguments),
        [SurroundFromUpperRightSequence.Indicator] => new SurroundFromUpperRightSequence(arguments),
        [SurroundFromLowerLeftSequence.Indicator] => new SurroundFromLowerLeftSequence(arguments),
        [SurroundFromLowerRightSequence.Indicator] => new SurroundFromLowerRightSequence(arguments),
        [OverlaidSequence.Indicator] => new OverlaidSequence(arguments),
        SurroundFromUpperLeftAndRightSequence.Indicator => new SurroundFromUpperLeftAndRightSequence(arguments),
        SurroundFromLowerLeftAndRightSequence.Indicator => new SurroundFromLowerLeftAndRightSequence(arguments),
        SurroundFromLeftAndRightSequence.Indicator => new SurroundFromLeftAndRightSequence(arguments),
        "&A-compU+2FF6;" => new SurroundFromBelowSequence(arguments),
        _ => null,
    };

    private static ReadOnlySpan<char> PushNextArgument(ReadOnlySpan<char> text, Stack<Codepoint> arguments)
    {
        var argIndex = ArgumentIndex(text);
        var newArgument = text[argIndex..];

        if (MakeSequence(newArgument, arguments) is Sequence sequence)
        {
            arguments.Push(new Codepoint
            {
                Id = sequence.Text,
                UnicodeScalarValue = null,
                SequenceText = sequence.Text,
                AltSequenceText = null,
                UnicodeCharacter = null,
                Sequence = sequence,
                AltSequence = null,
            });
        }
        else
        {
            var scalarValue = UnicodeScalarValue(newArgument);
            var id = scalarValue is null
                     ? newArgument
                     : GetLongCodepointId((int)scalarValue);
            var character = scalarValue is null ? null : new UnicodeCharacter
            {
                ScalarValue = (int)scalarValue,
                CodepointId = new string(id),
            };
            arguments.Push(new Codepoint
            {
                Id = new string(id),
                UnicodeScalarValue = scalarValue,
                SequenceText = null,
                AltSequenceText = null,
                UnicodeCharacter = character,
                Sequence = null,
                AltSequence = null,
            });
        }

        return text[..argIndex];
    }

    private static int ArgumentIndex(ReadOnlySpan<char> text)
    {
        if (text.Length == 0)
        {
            throw new ArgumentException("Text is empty", nameof(text));
        }
        if (text.Length == 1)
        {
            return 0;
        }
        if (char.IsLowSurrogate(text[^1]) && char.IsHighSurrogate(text[^2]))
        {
            return text.Length - 2;
        }
        if (text[^1] == ';')
        {
            for (int i = 3; i <= text.Length; i++)
            {
                if (text[^i] == '&')
                {
                    return text.Length - i;
                }
            }
        }
        return text.Length - 1;
    }
}
