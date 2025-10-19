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
using System.Text;
using Jitendex.Chise.Models;
using static Jitendex.Chise.Models.ComponentPositionId;

namespace Jitendex.Chise.Readers;

/// <summary>
/// Parse and evaluate an Ideographic Description Sequence (IDS)
/// </summary>
/// <remarks>
/// This algorithm tokenizes the IDS by starting from the end of the sequence
/// and stepping backwards. Each new token is pushed to a stack of codepoints.
/// When an Ideographic Description Character (IDC) token is found, it is
/// applied to codepoints at the top of the stack. The evaluated result is then
/// pushed to the stack. If the full IDS text is valid, only one codepoint will
/// remain on the stack at the end of the parsing.
/// <example>
/// <para>Example: The IDS for 丽, "⿱一⿰⿵冂丶⿵冂丶", contains 9 tokens.</para>
/// <list type="number">
/// <item><description> ["丶"]                  </description></item>
/// <item><description> ["丶", "冂"]             </description></item>
/// <item><description> ["⿵冂丶"]               </description></item>
/// <item><description> ["⿵冂丶", "丶"]          </description></item>
/// <item><description> ["⿵冂丶", "丶", "冂"]    </description></item>
/// <item><description> ["⿵冂丶", "⿵冂丶"]       </description></item>
/// <item><description> ["⿰⿵冂丶⿵冂丶"]         </description></item>
/// <item><description> ["⿰⿵冂丶⿵冂丶", "一"]    </description></item>
/// <item><description> ["⿱一⿰⿵冂丶⿵冂丶"]      </description></item>
/// </list>
/// </example>
/// </remarks>
/// <exception cref="InvalidOperationException">
/// Thrown when the IDS contains insufficient tokens for a given IDC.
/// </exception>
internal static class SequenceTextParser
{
    public static Stack<Codepoint> Parse(in ReadOnlySpan<char> sequenceText)
    {
        Stack<Codepoint> stack = [];
        int end = sequenceText.Length;
        while (end > 0)
        {
            int start = TokenStartIndex(sequenceText[..end]);
            var token = sequenceText[start..end];
            Evaluate(token, stack);
            end = start;
        }
        return stack;
    }

    private static int TokenStartIndex(in ReadOnlySpan<char> text)
    {
        if (text.Length == 0)
        {
            throw new ArgumentException("Text is empty", nameof(text));
        }

        if (text.Length == 1)
        {
            return 0;
        }

        // Check for a surrogate pair.
        // Example: 𠮟 (U+20B9F) is composed of two chars, 0xD842 and 0xDF9F.
        if (char.IsLowSurrogate(text[^1]) && char.IsHighSurrogate(text[^2]))
        {
            return text.Length - 2;
        }

        // Check for tokens that begin with '&' and end with ';'.
        // Examples: "&CDP-8BC4;", "&HZK03-ABFE;", "&U-i001+2FF1;", etc.
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

    private static void Evaluate(in ReadOnlySpan<char> token, Stack<Codepoint> arguments)
    {
        if (ApplyIdcToArguments(token, arguments) is Sequence sequence)
        {
            // Token was an Ideographic Description Character (IDC),
            // and its arguments were removed from the stack.
            // Now push the result of the evaluation onto the stack.

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
            // Token is a new codepoint argument.
            // Push it onto the stack.

            var scalarValue = UnicodeConverter.ScalarValueOrDefault(token);

            var id = scalarValue == default
                     ? token
                     : UnicodeConverter.GetLongCodepointId(scalarValue);

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

    private static Sequence? ApplyIdcToArguments(in ReadOnlySpan<char> idc, Stack<Codepoint> arguments)
        => IdcToPositionIds(idc) is var positionIds and not []
            ? NewSequence(idc, arguments, positionIds)
            : null;

    private static ImmutableArray<ComponentPositionId> IdcToPositionIds(in ReadOnlySpan<char> idc) => idc switch
    {
        ['⿰'] => [LeftHalf, RightHalf],
        ['⿱'] => [TopHalf, BottomHalf],
        ['⿲'] => [Left, VerticalCenter, Right],
        ['⿳'] => [Top, HorizontalCenter, Bottom],
        ['⿴'] => [FullSurrounding, FullSurrounded],
        ['⿵'] => [AboveSurrounding, BelowSurrounded],
        ['⿶'] => [BelowSurrounding, AboveSurrounded],
        ['⿷'] => [LeftSurrounding, RightSurrounded],
        ['⿼'] => [RightSurrounding, LeftSurrounded],
        ['⿸'] => [UpperLeftSurrounding, LowerRightSurrounded],
        ['⿹'] => [UpperRightSurrounding, LowerLeftSurrounded],
        ['⿺'] => [LowerLeftSurrounding, UpperRightSurrounded],
        ['⿽'] => [LowerRightSurrounding, UpperLeftSurrounded],
        ['⿻'] => [Overlaying, Overlaid],
        "&U-i001+2FF1;" => [UpperLeftAndRightSurrounding, LowerLeftAndRightSurrounded],
        "&U-i002+2FF1;" => [LowerLeftAndRightSurrounding, UpperLeftAndRightSurrounded],
        "&U-i001+2FFB;" => [UpperAndLowerSurrounding, LeftAndRightSurrounded],
        _ => [],
    };

    private static Sequence NewSequence(in ReadOnlySpan<char> idc, Stack<Codepoint> arguments, in ImmutableArray<ComponentPositionId> positionIds)
    {
        var textBuilder = new StringBuilder(new string(idc));
        var components = new List<Component>();

        foreach (var positionId in positionIds)
        {
            var codepoint = arguments.Pop();
            components.Add(new Component
            {
                CodepointId = codepoint.Id,
                PositionId = positionId,
                Codepoint = codepoint,
            });
            textBuilder.Append(codepoint.ToCharacter());
        }

        return new Sequence
        {
            Text = textBuilder.ToString(),
            Components = components,
        };
    }
}
