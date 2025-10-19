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

internal interface ISequence
{
    abstract static string GetIndicator();
    abstract static int ArgumentCount();
    abstract static string FirstPositionName();
    abstract static string SecondPositionName();
    abstract static string ThirdPositionName();
}

/// <summary>
/// Represents an Ideographic Description Sequence (IDS)
/// </summary>
public abstract class Sequence
{
    [Key]
    public string Text { get; init; } = null!;

    [InverseProperty(nameof(Component.Sequences))]
    public List<Component> Components { get; init; } = [];

    [InverseProperty(nameof(Codepoint.Sequence))]
    public List<Codepoint> Codepoints { get; init; } = [];

    [InverseProperty(nameof(Codepoint.AltSequence))]
    public List<Codepoint> AltCodepoints { get; init; } = [];
}
