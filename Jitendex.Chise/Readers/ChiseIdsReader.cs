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

namespace Jitendex.Chise.Readers;

internal class ChiseIdsReader
{
    private readonly Logger _logger;

    public ChiseIdsReader()
    {
        _logger = new Logger();
    }

    public List<Codepoint> Read(DirectoryInfo chiseIdsDir)
    {
        var codepoints = new List<Codepoint>(215_000);
        foreach (var file in chiseIdsDir.EnumerateFiles("*.txt"))
        {
            ReadFile(file, codepoints);
        }
        _logger.WriteLogs();
        return codepoints;
    }

    private void ReadFile(FileInfo file, List<Codepoint> codepoints)
    {
        using StreamReader sr = file.OpenText();
        int lineNumber = 0;
        var filename = file.Name.AsSpan();

        while (sr.ReadLine() is string line)
        {
            lineNumber++;

            if (line.StartsWith(';'))
            {
                continue;
            }

            var lineElements = new LineElements(filename, lineNumber, line.AsSpan());

            _logger.LogLineErrors(lineElements);

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

        var sequence = MakeSequence(lineElements);
        var altSequence = MakeAltSequence(lineElements);

        if (sequence is null)
        {
            // There was an error making the sequence.
            return null;
        }

        return new Codepoint
        {
            Id = id,
            UnicodeScalarValue = unicodeCharacter?.ScalarValue,
            SequenceText = sequence.Text,
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

        var scalarValue = UnicodeConverter.ScalarValueOrDefault(lineElements.Character);

        if (scalarValue == default)
        {
            _logger.InvalidUnicodeCodepoint(lineElements);
            return null;
        }

        var longId = UnicodeConverter.GetLongCodepointId(scalarValue);
        var shortId = UnicodeConverter.GetShortCodepointId(scalarValue);

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

    private Sequence? MakeSequence(in LineElements lineElements)
    {
        Stack<Codepoint> sequenceArguments;

        try
        {
            sequenceArguments = SequenceTextParser.Parse(lineElements.Sequence);
        }
        catch (InvalidOperationException)
        {
            _logger.InsufficientIdsArgs(lineElements);
            return null;
        }

        if (sequenceArguments.Count != 1)
        {
            _logger.InsufficientIdsOps(lineElements);
            return null;
        }

        return sequenceArguments.Pop().Sequence;
    }

    private Sequence? MakeAltSequence(in LineElements lineElements)
    {
        if (lineElements.AltSequence is [])
        {
            return null;
        }

        Stack<Codepoint> altSequenceArguments;

        try
        {
            altSequenceArguments = SequenceTextParser.Parse(lineElements.AltSequence);
        }
        catch (InvalidOperationException)
        {
            _logger.InsufficientAltIdsArgs(lineElements);
            return null;
        }

        if (altSequenceArguments.Count != 1)
        {
            _logger.InsufficientAltIdsOps(lineElements);
            return null;
        }

        return altSequenceArguments.Pop().Sequence;
    }
}
