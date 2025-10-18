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

namespace Jitendex.Chise.Models;

public abstract class Sequence
{
    [Key]
    public string Text { get; }
    public string FirstCodepointId { get; }
    public string? SecondCodepointId { get; }
    public string? ThirdCodepointId { get; }

    [ForeignKey(nameof(FirstCodepoint))]
    public Codepoint FirstCodepoint { get; }

    [ForeignKey(nameof(SecondCodepointId))]
    public Codepoint? SecondCodepoint { get; }

    [ForeignKey(nameof(ThirdCodepointId))]
    public Codepoint? ThirdCodepoint { get; }

    protected abstract string GetIndicator();
    protected abstract int ArgumentCount();

    public Sequence(Stack<Codepoint> arguments)
    {
        FirstCodepoint = arguments.Pop();
        FirstCodepointId = FirstCodepoint.Id;

        int argumentCount = ArgumentCount();

        if (argumentCount > 1)
        {
            SecondCodepoint = arguments.Pop();
            SecondCodepointId = SecondCodepoint.Id;
        }

        if (argumentCount > 2)
        {
            ThirdCodepoint = arguments.Pop();
            ThirdCodepointId = ThirdCodepoint.Id;
        }

        var firstCharacter = FirstCodepoint.UnicodeCharacter?.Character().ToString() ?? FirstCodepointId;
        var secondCharacter = SecondCodepoint?.UnicodeCharacter?.Character().ToString() ?? SecondCodepointId;
        var thirdCharacter = ThirdCodepoint?.UnicodeCharacter?.Character().ToString() ?? ThirdCodepointId;

        Text = $"{GetIndicator()}{firstCharacter}{secondCharacter}{thirdCharacter}";
    }
}
