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

namespace Jitendex.Tatoeba.Models;

[Table(nameof(Sequence))]
public sealed class Sequence
{
    [Key]
    public required int Id { get; init; }
    public required DateOnly CreatedDate { get; init; }

    [InverseProperty(nameof(JapaneseSentence.Sequence))]
    public JapaneseSentence? JapaneseSentence { get; set; }

    [InverseProperty(nameof(EnglishSentence.Sequence))]
    public EnglishSentence? EnglishSentence { get; set; }

    [InverseProperty(nameof(Revision.Sequence))]
    public List<Revision> Revisions { get; init; } = [];

    public override bool Equals(object? obj)
        => obj is Sequence sequence
        && JapaneseSentence == sequence.JapaneseSentence
        && EnglishSentence == sequence.EnglishSentence;

    public override int GetHashCode()
        => HashCode.Combine(JapaneseSentence, EnglishSentence);
}
