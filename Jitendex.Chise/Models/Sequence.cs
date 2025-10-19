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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Jitendex.Chise.Models;

/// <summary>
/// Represents an Ideographic Description Sequence (IDS)
/// </summary>
public abstract class Sequence
{
    [Key]
    public string Text { get; }

    [InverseProperty(nameof(Component.Sequences))]
    public List<Component> Components { get; } = [];

    [InverseProperty(nameof(Codepoint.Sequence))]
    public List<Codepoint> Codepoints { get; } = [];

    [InverseProperty(nameof(Codepoint.AltSequence))]
    public List<Codepoint> AltCodepoints { get; } = [];

    protected abstract string GetIndicator();
    protected abstract int ArgumentCount();
    protected abstract string FirstPositionName();
    protected abstract string SecondPositionName();
    protected abstract string ThirdPositionName();

    public Sequence(Stack<Codepoint> arguments)
    {
        var textBuilder = new StringBuilder(GetIndicator());

        var firstCodepoint = arguments.Pop();
        Components.Add(new Component
        {
            CodepointId = firstCodepoint.Id,
            PositionName = FirstPositionName(),
            Codepoint = firstCodepoint,
        });
        textBuilder.Append(firstCodepoint.ToCharacter());

        int argumentCount = ArgumentCount();

        if (argumentCount > 1)
        {
            var secondCodepoint = arguments.Pop();
            Components.Add(new Component
            {
                CodepointId = secondCodepoint.Id,
                PositionName = SecondPositionName(),
                Codepoint = secondCodepoint,
            });
            textBuilder.Append(secondCodepoint.ToCharacter());
        }

        if (argumentCount > 2)
        {
            var thirdCodepoint = arguments.Pop();
            Components.Add(new Component
            {
                CodepointId = thirdCodepoint.Id,
                PositionName = ThirdPositionName(),
                Codepoint = thirdCodepoint,
            });
            textBuilder.Append(thirdCodepoint.ToCharacter());
        }

        Text = textBuilder.ToString();
    }
}
