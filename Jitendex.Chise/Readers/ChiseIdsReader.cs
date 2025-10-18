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

internal class ChiseIdsReader
{
    private readonly Logger _logger;

    public ChiseIdsReader()
    {
        _logger = new Logger();
    }

    public async Task<List<Codepoint>> ReadAsync(DirectoryInfo chiseIdsDir)
    {
        var codepoints = new List<Codepoint>(215_000);
        foreach (var file in chiseIdsDir.EnumerateFiles("*.txt"))
        {
            await ReadFileAsync(file, codepoints);
        }
        return codepoints;
    }

    private async Task ReadFileAsync(FileInfo file, List<Codepoint> codepoints)
    {
        using StreamReader sr = file.OpenText();
        int lineNumber = 0;

        while (await sr.ReadLineAsync() is string line)
        {
            lineNumber++;

            if (line.StartsWith(';'))
            {
                continue;
            }

            var lineElements = new LineElements(file.Name.AsSpan(), lineNumber, line.AsSpan());

            if (lineElements.AltSequenceFormatError) _logger.AltSequenceFormatError(lineElements);
            if (lineElements.ExcessiveElementsError) _logger.ExcessiveLineElements(lineElements);
            if (lineElements.InsufficientElementsError) _logger.InsufficientLineElements(lineElements);

            if (lineElements.InsufficientElementsError)
            {
                continue;
            }

            if (MakeCodepoint(lineElements) is Codepoint codepoint)
            {
                codepoints.Add(codepoint);
            }
        }
    }

    private Codepoint? MakeCodepoint(in LineElements lineElements)
    {
        var unicodeCharacter = MakeUnicodeCharacter(lineElements);

        var id = unicodeCharacter is null
                 ? new string(lineElements.Codepoint)
                 : unicodeCharacter.CodepointId;

        Stack<Codepoint> sequenceArguments;
        Stack<Codepoint>? altSequenceArguments;

        try
        {
            sequenceArguments = MakeArgumentStack(lineElements.Sequence);
        }
        catch (InvalidOperationException)
        {
            _logger.InsufficientIdsArgs(lineElements);
            return null;
        }

        try
        {
            altSequenceArguments = lineElements.AltSequence is []
                ? null : MakeArgumentStack(lineElements.AltSequence);
        }
        catch (InvalidOperationException)
        {
            _logger.InsufficientAltIdsArgs(lineElements);
            altSequenceArguments = null;
        }

        if (altSequenceArguments is not null && altSequenceArguments.Count != 1)
        {
            _logger.InsufficientAltIdsOps(lineElements);
            altSequenceArguments = null;
        }
        if (sequenceArguments.Count != 1)
        {
            _logger.InsufficientIdsOps(lineElements);
            return null;
        }

        var sequence = sequenceArguments.Pop().Sequence;
        var altSequence = altSequenceArguments?.Pop().Sequence;

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

    private UnicodeCharacter? MakeUnicodeCharacter(in LineElements lineElements)
    {
        if (!lineElements.Codepoint.StartsWith('U'))
        {
            return null;
        }

        var scalarValue = UnicodeScalarValueOrDefault(lineElements.Character);

        if (scalarValue == default)
        {
            _logger.InvalidUnicodeCodepoint(lineElements);
            return null;
        }

        var longId = GetLongCodepointId(scalarValue);
        var shortId = GetShortCodepointId(scalarValue);

        if (!shortId.SequenceEqual(lineElements.Codepoint) && !longId.SequenceEqual(lineElements.Codepoint))
        {
            _logger.UnicodeCharacterInequality(lineElements);
        }

        return new UnicodeCharacter
        {
            ScalarValue = (int)scalarValue,
            CodepointId = new string(longId),
        };
    }

    private static int UnicodeScalarValueOrDefault(in ReadOnlySpan<char> character) => character switch
    {
        { Length: 1 } => character[0],
        { Length: 2 } when char.IsHighSurrogate(character[0])
                        && char.IsLowSurrogate(character[1])
                        => char.ConvertToUtf32(character[0], character[1]),
        _ => default,
    };

    private static ReadOnlySpan<char> GetLongCodepointId(int scalarValue) => $"U-{scalarValue:X8}";
    private static ReadOnlySpan<char> GetShortCodepointId(int scalarValue) => $"U+{scalarValue:X}";

    private static Stack<Codepoint> MakeArgumentStack(in ReadOnlySpan<char> sequenceText)
    {
        Stack<Codepoint> arguments = [];
        int end = sequenceText.Length;
        while (end > 0)
        {
            int start = ArgumentIndex(sequenceText[..end]);
            var argumentText = sequenceText[start..end];
            ResolveArgument(argumentText, arguments);
            end = start;
        }
        return arguments;
    }

    private static Sequence? MakeSequence(in ReadOnlySpan<char> indicator, Stack<Codepoint> arguments) => indicator switch
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

    private static void ResolveArgument(in ReadOnlySpan<char> argumentText, Stack<Codepoint> arguments)
    {
        if (MakeSequence(argumentText, arguments) is Sequence sequence)
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
            var scalarValue = UnicodeScalarValueOrDefault(argumentText);

            var id = scalarValue == default
                     ? argumentText
                     : GetLongCodepointId(scalarValue);

            var character = scalarValue == default ? null : new UnicodeCharacter
            {
                ScalarValue = scalarValue,
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
    }

    private static int ArgumentIndex(in ReadOnlySpan<char> text)
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
